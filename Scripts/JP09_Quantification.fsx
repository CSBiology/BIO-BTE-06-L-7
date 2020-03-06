
#load @"../IfSharp/References.fsx"
#load @"../IfSharp/Paket.Generated.Refs.fsx"

open System.IO
open BioFSharp.IO
open BioFSharp.Mz
open SearchDB
open QConQuantifier
open Parameters.Domain
open Parameters.DTO
open System.Data.SQLite
open MzIO.Processing
open BioFSharp
open FSharp.Stats
open BioFSharp.Mz.Quantification.HULQ
open PeptideLookUp
open SearchEngineResult
open QConQuantifier.Quantification
open FSharp.Plotly

let source = __SOURCE_DIRECTORY__


let axis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Zeroline=false,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside, Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))

let showPSMChart sequence globalMod ch avgScanTime ms2s 
    (xXic:float[]) (yXic:float[])  (xToQuantify:float[]) (ypToQuantify:float[]) (fitY:float[]) 
        (xXicInferred:float[]) (yXicinferred:float[]) (xInferred:float[]) (inferredFit:float[]) =
    [
    Chart.Point(xXic, yXic)                     |> Chart.withTraceName "Target XIC"
    Chart.Point(ms2s)                           |> Chart.withTraceName "MS2s with scores"
    Chart.Point([avgScanTime],[1.])             |> Chart.withTraceName "Weighted Mean of Ms2 scan times"
    Chart.Point((xToQuantify), (ypToQuantify))  |> Chart.withTraceName "Identified Target Peak"
    Chart.Line(xToQuantify,fitY)                |> Chart.withTraceName "Fit of target Peak"
    Chart.Point(xXicInferred, yXicinferred)     |> Chart.withTraceName "Inferred XIC"
    Chart.Line(xInferred,inferredFit)           |> Chart.withTraceName "Fit of inferred Peak"

    ]
    |> Chart.Combine
    |> Chart.withTitle(sprintf "Sequence= %s, GlobalMod = %i, Charge State = %i" sequence globalMod ch)
    |> Chart.withX_Axis (axis false "Intensity" 20 16)
    |> Chart.withY_Axis (axis false "Retention Time" 20 16)
    |> Chart.withSize(1500.,800.)
    |> Chart.Show

let qConQuantParams :Parameters.Domain.QConQuantifierParams = 
    {
    Name                            = "ChlamyTruncDB"
    DbFolder                        = source + @"\..\AuxFiles"
    QConCatFastaPaths               = [source + @"\..\AuxFiles\CBB QconCAT.fasta"; source + @"\..\AuxFiles\PS QconCAT.fasta"]
    OrganismFastaPath               = source + @"\..\AuxFiles\Chlamy_JGI5_5(Cp_Mp).fasta"
    ParseProteinIDRegexPattern      = "id"
    Protease                        = Protease.Trypsin
    MinMissedCleavages              = 0
    MaxMissedCleavages              = 1
    MaxMass                         = 15000.
    MinPepLength                    = 5
    MaxPepLength                    = 50
    IsotopicMod                     = [IsotopicMod.N15]
    MassMode                        = MassMode.Monoisotopic
    FixedMods                       = []            
    VariableMods                    = []
    VarModThreshold                 = 3
    ExpectedMinCharge               = 2
    ExpectedMaxCharge               = 3
    LookUpPPM                       = 30.
    ScanRange                       = 100.0,1600.0
    PSMThreshold                    = PSMThreshold.PepValue 0.05
    ScanTimeWindow                  = 2.5
    MzWindow_Da                     = 0.07
    NTerminalSeries                 = NTerminalSeries.B
    CTerminalSeries                 = CTerminalSeries.Y
    EstimateLabelEfficiency         = true
    }
    |> QConQuantifierParams.toDomain


//////////////////
//IO preparation
//////////////////

let peptideDB = PeptideLookUp.dbLookUpCn qConQuantParams

///
let inReader = IO.Reader.createReader @"C:\Users\jonat\source\repos\2020Practical\20200206MS169msFSSTqp009.mzlite"

///
let tr = inReader.BeginTransaction()  

//////////////////
//Identification
//////////////////        
///copy peptideDB to memory to facilitate a fast look up
let memoryDB = PeptideLookUp.copyDBIntoMemory peptideDB

/// Gets all modified peptide between first and second mass.
let selectModPeptideByMassRange = PeptideLookUp.initSelectModPeptideByMassRange qConQuantParams memoryDB
    
/// Gets all modified peptides by protein accession.
let selectQConcatPeptideByProtAccession = PeptideLookUp.initSelectQConcatPeptideByProtAccession memoryDB

/// Given a AminoAcid list this function will compute N- and C-terminal ion ladders.
let calcIonSeries = Identification.initCalcIonSeries qConQuantParams
           
/// All peptides of qConcat heritage.
let qConCatPeps = 
    let protHeader = 
        Seq.collect (FastA.fromFile id) qConQuantParams.QConCatFastaPaths
        |> Seq.map (fun prot -> prot.Header)
    Seq.collect selectQConcatPeptideByProtAccession protHeader
    |> Seq.filter (fun x -> x.MissCleavages = 0)

    
/// Returns true if mz is theoretically mapping to an ion originating from a QConcat peptide.
let isValidMz = PeptideLookUp.initIsValidMz qConQuantParams qConCatPeps

/// Filter all measured ms2s for those with a precursor mz theoretically mapping to an ion originating from a QConcat peptide.
let possibleMs2s =
    inReader
    |> IO.Reader.getMassSpectra
    |> Seq.filter (fun ms -> MassSpectrum.getMsLevel ms = 2  && MassSpectrum.getPrecursorMZ ms |> isValidMz )
    |> Array.ofSeq
        
/// Maps all fragment spectra (ms2s) and matches their spectra against in silico spectra. The insilico spectra are retrieved based on the precursor mzs of the 
/// ms2s, user supplied minimal and maximum charge states and user supplied search tolerance in ppm.  
/// The algorithm used to compare theoretical and recorded spectra is the SEQUEST algorithm implemented in the BioFSharp.Mz library.
let psms = Identification.calcPeptideSpectrumMatches inReader selectModPeptideByMassRange calcIonSeries qConQuantParams possibleMs2s

/// Get all peptide spectrum matches above a use defined threshold.
let thresholdedPsms = Identification.thresholdPSMs qConQuantParams psms

//////////////////
//Quantification
//////////////////

/// Indexes each scan with it's corresponding retention time
let rtIndex = IO.XIC.getRetentionTimeIdx inReader

/// Given an isotopic variant of a qConcat peptide this function returns the respective labled/unlabeled version. 
let getIsotopicVariant = Quantification.initGetIsotopicVariant qConCatPeps

/// Quantifies every Ion identified by at least one psm. Given a collection of PSMs this function first performs an aggregation, grouping all psms mapping
/// to the same ion species. Afterwards the average PSM is computed. Based uppon this, XIC extraction and quantification is performed. Additionally, a paired
/// quantification is performed. This means that if a light variant (e.g a N14 labeled peptide) was identified, the XIC corresponding to the
/// heavy variant is extracted and quantified. This does not only enlarge the fraction of quantified peptides it also alows to control for the quality of 
/// the quantification if an ion is quantified from both perspectives (in case of both, Heavy-To-Light and Light-To-Heavy inference).
/// Besides the monoisotopic peak this function also quantifies the N15Minus1 peak this allows to calculate the labeling efficiency. 
let quantifyPSMs reader rtIndex qConQuantifierParams getIsotopicVariant (psms:SearchEngineResult<float> list) = 
    psms
    |> List.groupBy (fun x -> x.StringSequence, x.GlobalMod,x.PrecursorCharge)
    |> List.choose (fun ((sequence,globMod,ch),psms) ->
                    try
                    let ms2s = psms |> Seq.map (fun x -> x.ScanTime,x.Score)
                    let averagePSM = average reader rtIndex qConQuantifierParams psms
                    let avgMass = Mass.ofMZ (averagePSM.MeanPrecMz) (ch |> float)
                    let peaks          = Signal.PeakDetection.SecondDerivative.getPeaks 0.1 2 13 averagePSM.X_Xic averagePSM.Y_Xic
                    let peakToQuantify = getPeakBy peaks averagePSM.WeightedAvgScanTime
                    let quantP = quantifyPeak peakToQuantify 
                    let searchScanTime = 
                        if quantP.EstimatedParams |> Array.isEmpty then
                            averagePSM.WeightedAvgScanTime
                        else 
                            quantP.EstimatedParams.[1] 
                    // The target PSM was not global modified -> a light peptide
                    if globMod = 0 then 
                        let labeled        = getIsotopicVariant sequence globMod
                        let n15mz          = Mass.toMZ (labeled.ModMass) (ch|> float)
                        let n15Quant,rt,itz,rtP = quantifyBy reader rtIndex qConQuantifierParams n15mz searchScanTime
                        let n15Minus1Mz    = n15mz - (Mass.Table.NMassInU / (ch|> float))
                        let n15Minus1Quant,_,_,_ = quantifyBy reader rtIndex qConQuantifierParams n15Minus1Mz searchScanTime
                        let chart = showPSMChart sequence globMod ch averagePSM.WeightedAvgScanTime ms2s averagePSM.X_Xic averagePSM.Y_Xic   
                                        peakToQuantify.XData peakToQuantify.YData quantP.YPredicted rt itz rtP n15Quant.YPredicted
  
                        createQuantifiedPeptide sequence globMod averagePSM.WeightedAvgScanTime averagePSM.MeanScore
                                ch avgMass averagePSM.MeanPrecMz quantP.Area n15mz n15Quant.Area n15Minus1Mz n15Minus1Quant.Area
                        |> Some
                    // The target PSM was global modified -> a heavy peptide
                    else
                        let labeled        = getIsotopicVariant sequence globMod
                        let n14mz          = Mass.toMZ (labeled.ModMass) (ch|> float)
                        let n14Quant,rt,itz,rtP      = quantifyBy reader rtIndex qConQuantifierParams n14mz searchScanTime
                        let n15Minus1Mz    = averagePSM.MeanPrecMz - (Mass.Table.NMassInU / (ch|> float))
                        let n15Minus1Quant,_,_,_ = quantifyBy reader rtIndex qConQuantifierParams n15Minus1Mz searchScanTime
                        let chart = showPSMChart sequence globMod ch averagePSM.WeightedAvgScanTime ms2s averagePSM.X_Xic averagePSM.Y_Xic  
                                        peakToQuantify.XData peakToQuantify.YData quantP.YPredicted rt itz rtP n14Quant.YPredicted
                            
                        createQuantifiedPeptide sequence globMod averagePSM.WeightedAvgScanTime averagePSM.MeanScore
                                ch avgMass n14mz n14Quant.Area averagePSM.MeanPrecMz quantP.Area n15Minus1Mz n15Minus1Quant.Area
                        |> Some
                    with
                    | e as exn -> 
                        //printfn "%s" e.Message
                        Option.None
                    )

quantifyPSMs inReader rtIndex qConQuantParams getIsotopicVariant thresholdedPsms

let groupedPSMs =
    thresholdedPsms
    |> List.groupBy (fun x -> x.StringSequence, x.GlobalMod,x.PrecursorCharge)

let averagePSM =
    groupedPSMs
    |> List.map (fun ((sequence,globMod,ch),psms) ->
        let averagePSM = average inReader rtIndex qConQuantParams psms
        sequence, averagePSM
    )

averagePSM
|> List.map (fun (sequence, apsm) ->
    Chart.Point (apsm.X_Xic, apsm.Y_Xic)
    |> Chart.withTraceName sequence
)
|> Chart.Combine
|> Chart.Show