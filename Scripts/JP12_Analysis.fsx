// Include CsbScaffold
#load "../../.env/CsbScaffold.fsx"
// If you want to use the wrappers for unmanaged LAPACK functions from of FSharp.Stats 
// include the path to the .lib folder manually to your PATH environment variable and make sure you set FSI to 64 bit

// use the following lines of code to ensure that LAPACK functionalities are enabled if you want to use them
// fails with "MKL service either not available, or not started" if lib folder is not included in PATH.
//open FSharp.Stats
//FSharp.Stats.Algebra.LinearAlgebra.Service()

open BioFSharp
open Deedle
open FSharpAux
open FSharp.Stats
open FSharp.Plotly

type Color = {
    H : int
    S : int
    L : int
    }

let createHSL (color:Color) =
    (sprintf "hsl(%i," color.H) + string color.S + @"%," + string color.L + @"%)"

let mainColorPeachFF6F61 = {
    H = 5
    S = 100
    L = 69
}

let mainColorBlue92a8d1 = {
    H = 219
    S = 41
    L = 70
}

let mainColorGreen88b04b = {
    H = 84
    S = 40
    L = 49
}

let mainColorYellowEFC050 = {
    H = 42
    S = 83
    L = 63
}

let soos =  {
    H = 166
    S = 100
    L = 30
}

let mainColors = [mainColorPeachFF6F61;mainColorBlue92a8d1;mainColorGreen88b04b;mainColorYellowEFC050;soos] |> List.map createHSL |> List.rev

let createRelatedColors (color:Color) nOfColors =
    let wheelchange = -20
    let changeTempColor wheelChange tempColor =
        if tempColor.H + wheelChange < 0 
        then 
            360 + (color.H + wheelChange)
        elif tempColor.H + wheelChange > 360
        then 
            (tempColor.H + wheelChange) - 360
        else tempColor.H + wheelChange
    let rec loopVariant results iteration =
        if iteration = 1 
        then 
            loopVariant ({color with H = changeTempColor wheelchange color}::results) (iteration + 1)
        elif iteration < nOfColors 
        then 
            let mostRecentVariant =
                results.Head
            loopVariant ({mostRecentVariant with H = changeTempColor wheelchange mostRecentVariant}::results) (iteration + 1)
        else results
    loopVariant [color] 1
    |> (List.rev >> Array.ofList)

createRelatedColors mainColorGreen88b04b 3
|> Array.map createHSL

let xAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Zeroline=false,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside, Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))
let yAxis showGrid title titleSize tickSize (range:float*float)= Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside,Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize),Range=StyleParam.Range.MinMax range)

let config = Config.init(ToImageButtonOptions = ToImageButtonOptions.init(Format = StyleParam.ImageFormat.SVG, Filename = "praktikumsplot.svg"), EditableAnnotations = [AnnotationEditOptions.LegendPosition])


let isBad a =
    nan.Equals(a) || infinity.Equals(a) || (-infinity).Equals(a)


let qConCatSeq =
    "MASMTGGQQMGRDPSRSALPSNWKSVLPANWRDTDILAAFREVTLGFVDLMRFLFVAEAIYKLTYYTPDYVVRAYVSNESAIRLVAFDNQKYWTMWKAFPDAYVRVPLILGIWGGKIGQQLVNARSLVDEQENVKLGADSGALEFVPKDDYLNAPGETYSVKTPLANLVYWKALYGFDFLLSSKTNFGIGHRLSIFETGIKTAPAFVDLDTRIPAGPDLIVKNILVVGPVPGKIVAITALSEKYPIYFGGNRVLNTWADIINREWELSFRNTWADIINRLIFQYASFNNSRTALPADWRLVFPEEVLPRNILLNEGIRTWFDDADDWLRAAHHHHHHHKLAAALEHHHHHH"
    |> BioArray.ofAminoAcidString
    |> Digestion.BioSeq.digest Digestion.Table.Trypsin
    |> Array.ofSeq
    |> Array.map (fun x -> BioList.toString x)

qConCatSeq.Length
let peptideProtMapping =
    [
    "LCI5"  =>  "SALPSNWK"
    "LCI5"  =>  "SVLPANWR"
    "rbcL"  =>  "DTDILAAFR"
    "rbcL"  =>  "EVTLGFVDLMR"
    "rbcL"  =>  "FLFVAEAIYK"
    "rbcL"  =>  "LTYYTPDYVVR"
    "RBCS2" =>  "AYVSNESAIR"
    "RBCS2" =>  "LVAFDNQK"
    "RBCS2" =>  "YWTMWK"
    "RBCS2" =>  "AFPDAYVR"
    "RCA1"  =>  "VPLILGIWGGK"
    "RCA1"  =>  "IGQQLVNAR"
    "RCA1"  =>  "SLVDEQENVK"
    "PCY1"  =>  "LGADSGALEFVPK"
    "PCY1"  =>  "DDYLNAPGETYSVK"
    "psaB"  =>  "TPLANLVYWK"
    "psaB"  =>  "ALYGFDFLLSSK"
    "psaB"  =>  "TNFGIGHR"
    "atpB"  =>  "LSIFETGIK"
    "atpB"  =>  "TAPAFVDLDTR"
    "petA"  =>  "IPAGPDLIVK"
    "petA"  =>  "NILVVGPVPGK"
    "petA"  =>  "IVAITALSEK"
    "petA"  =>  "YPIYFGGNR"
    "D1"    =>  "VLNTWADIINR"
    "D1"    =>  "EWELSFR"
    "D1"    =>  "NTWADIINR"
    "D1"    =>  "LIFQYASFNNSR"
    "LCI5"  =>  "TALPADWR"
    "psbD"  =>  "LVFPEEVLPR"
    "psbD"  =>  "NILLNEGIR"
    "psbD"  =>  "TWFDDADDWLR"
    ]
    |> List.map (fun (x,y) -> y,x)
    |> Map.ofList

peptideProtMapping.Count

let peptideMapping =
    [
        "DTDILAAFR"      => "rbcL"
        "EVTLGFVDLMR"    => "rbcL"
        "FLFVAEAIYK"     => "rbcL"
        "LTYYTPDYVVR"    => "rbcL"
        "AYVSNESAIR"     => "RBCS2"
        "LVAFDNQK"       => "RBCS2"
        "YWTMWK"         => "RBCS2"
        "AFPDAYVR"       => "RBCS2"
        "VPLILGIWGGK"    => "RCA1"
        "IGQQLVNAR"      => "RCA1"
        "SLVDEQENVK"     => "RCA1"
    ] |> Map.ofList

//FileName	Experiment	Content	ProteinAmount[ug]	Replicate
let sdsSampleNameMapping = 
    [
    //80,40,20 ug geladenes protein
    "Data02018VP_G3_4_SDS_IGD1"  => ("SDS_IGD",("RBCL", ("80","3")))
    "Data02018VP_G3_4_SDS_IGD2"  => ("SDS_IGD",("RBCL", ("40","3")))
    "Data02018VP_G3_4_SDS_IGD3"  => ("SDS_IGD",("RBCL", ("20","3")))
    "Data02018VP_G3_4_SDS_IGD4"  => ("SDS_IGD",("RBCS", ("80","3")))
    "Data02018VP_G3_4_SDS_IGD5"  => ("SDS_IGD",("RBCS", ("40","3")))
    "Data02018VP_G3_4_SDS_IGD6"  => ("SDS_IGD",("RBCS", ("20","3")))
    "Data02018VP_G3_4_SDS_IGD7"  => ("SDS_IGD",("37kDa",("80","3")))
    "Data02018VP_G3_4_SDS_IGD8"  => ("SDS_IGD",("RBCL", ("20","4")))
    "Data02018VP_G3_4_SDS_IGD9"  => ("SDS_IGD",("RBCL", ("10","4")))
    "Data02018VP_G3_4_SDS_IGD10" => ("SDS_IGD",("RBCL", ("5","4")))
    "Data02018VP_G3_4_SDS_IGD11" => ("SDS_IGD",("RBCS", ("20","4")))
    "Data02018VP_G3_4_SDS_IGD12" => ("SDS_IGD",("RBCS", ("10","4")))
    "Data02018VP_G3_4_SDS_IGD13" => ("SDS_IGD",("RBCS", ("5","4")))
    "Data02018VP_G3_4_SDS_IGD14" => ("SDS_IGD",("37kDa",("20","4")))
    
    ]

    |> Map.ofList


    
let bnNameMapping = 
    [
    //Random_Band_# nummer entspricht der nummerierung auf dem BN Bild
    "Data02018VP_G3-G4_BN_IGD1" =>  ("BN_IGD",("NF",("Random_Band_1","1")))
    "Data02018VP_G3-G4_BN_IGD2" =>  ("BN_IGD",("NF",("Random_Band_2","1")))
    "Data02018VP_G3-G4_BN_IGD3" =>  ("BN_IGD",("NF",("Random_Band_3","1")))
    "Data02018VP_G3-G4_BN_IGD4" =>  ("BN_IGD",("NF",("Random_Band_4","1")))
    "Data02018VP_G3-G4_BN_IGD5" =>  ("BN_IGD",("NF",("RBC_Band","1"))     )
    "Data02018VP_G3-G4_BN_IGD6" =>  ("BN_IGD",("NF",("RBC_Band","2"))     )
    
    ]
    |> Map.ofList

let wholeCellNameMapping = 
    [
    "Data20180126_InSolutionDigestWholeCell_Gr3+41" =>  ("WholeCell_ISD",("2:1","3") )
    "Data20180126_InSolutionDigestWholeCell_Gr3+42" =>  ("WholeCell_ISD",("1:1","3") )
    "Data20180126_InSolutionDigestWholeCell_Gr3+43" =>  ("WholeCell_ISD",("1:5","3") )
    "Data20180126_InSolutionDigestWholeCell_Gr3+44" =>  ("WholeCell_ISD",("1:10","3"))
    "Data20180126_InSolutionDigestWholeCell_Gr3+45" =>  ("WholeCell_ISD",("2:1","4") )
    "Data20180126_InSolutionDigestWholeCell_Gr3+46" =>  ("WholeCell_ISD",("1:1","4") )
    "Data20180126_InSolutionDigestWholeCell_Gr3+47" =>  ("WholeCell_ISD",("1:5","4") )
    "Data20180126_InSolutionDigestWholeCell_Gr3+48" =>  ("WholeCell_ISD",("1:10","4"))
    ]
    |> Map.ofList

let labelEfficiencyNameMapping = 
    [
    "DataLabelingEfficiency1"                           => ("LabelEfficiency",("15N IRT + 14N IRT","3"))
    "DataLabelingEfficiency2"                           => ("LabelEfficiency",("15N Q + 15N IRT"  ,"3"))
    "DataLabelingEfficiency3"                           => ("LabelEfficiency",("14N Q + 15N IRT"  ,"3"))
    "Data00VP2018_Gr4_LabelEfficiency4"                 => ("LabelEfficiency",("15N Q + 14N Q + 13C Q","4"))
    "Data00VP2018_Gr4_LabelEfficiency5"                 => ("LabelEfficiency",("15N Q","4"))
    "Data00VP2018_Gr4_LabelEfficiency6"                 => ("LabelEfficiency",("15N Q + 14N Q","4"))
    ]
    |> Map.ofList

let readQConcatResultFrame p : Frame<string*(bool*int),string>=
    let schemaFrame =
        Frame.ReadCsv(path = p,separators="\t")
    let schema =
        schemaFrame.ColumnKeys
        |> Seq.filter (fun x -> not (x = "StringSequence" || x = "GlobalMod" || x = "Charge"))
        |> Seq.map (sprintf "%s=float")
        |> Seq.append ["StringSequence=string";"GlobalMod=bool";"Charge=int"]
        |> String.concat ","
    Frame.ReadCsv(path = p,schema=schema,separators="\t")
    |> Frame.indexRowsUsing (fun os -> (os.GetAs<string>("StringSequence"),((os.GetAs<bool>("GlobalMod"),(os.GetAs<int>("Charge"))))))
    |> Frame.dropCol "StringSequence"
    |> Frame.dropCol "GlobalMod"
    |> Frame.dropCol "Charge"
    |> Frame.sortRowsByKey


//================================================================================
//====================== 1. LABEL EFFICIENCY =====================================
//================================================================================



let labelEfficiencyResults : Frame<string*(string*(string*string)),(string*(string*int))> = 
    readQConcatResultFrame @"C:\Users\kevin\Downloads\CsbScaffold-master\BioTechVP\RERUN\RERUN_Results\LabelEfficiency\QuantifiedPeptides.txt"
    |> Frame.mapColKeys 
        (fun (ck:string) -> 
            printfn "%s" ck
            let newCK = Map.find (ck.Split('_').[1 ..] |> String.concat "_") labelEfficiencyNameMapping
            ck.Split('_').[0] , newCK
        )
    |> Frame.sortColsByKey
    |> Frame.filterCols (fun ck _ -> (fst ck).Contains("Quant") || (fst ck).Contains("N15MZ") || (fst ck).Contains("N15Minus1MZ"))
    |> Frame.applyLevel (fun (sequence,(gmod,charge)) -> sequence,charge) Stats.mean
    |> Frame.transpose
    |> Frame.mapColKeys
        (fun ck ->
            match Map.tryFind (fst ck) peptideProtMapping with
            |Some prot  -> prot,ck
            |None       -> "NotFound",ck
        )

open Isotopes
open Elements

let initlabelN15Partial n15Prob =
    ///Diisotopic representation of nitrogen with abundancy of N14 and N15 swapped
    let n14Prob = 1. - n15Prob
    let N15 = Di (createDi "N15" (Isotopes.Table.N15,n15Prob) (Isotopes.Table.N14,n14Prob) )
    fun f -> Formula.replaceElement f Elements.Table.N N15

let labelFullN15 =
    let N15 = Elements.Table.Heavy.N15
    fun f -> Formula.replaceElement f Elements.Table.N N15

let generateIsotopicDistributionOfFormulaBySum (charge:int) (f:Formula.Formula) =
    IsotopicDistribution.MIDA.ofFormula 
        IsotopicDistribution.MIDA.normalizeByProbSum
        0.01
        0.001
        charge
        f

let generateIsotopicDistributionOfFormulaByMax (charge:int) (f:Formula.Formula) =
    IsotopicDistribution.MIDA.ofFormula 
        IsotopicDistribution.MIDA.normalizeByMaxProb
        0.01
        0.001
        charge
        f

let labelEfficiency =
    labelEfficiencyResults
    |> Frame.filterRows 
        (fun (rk,_) _ -> not (rk.Contains("N14")))
    |> Frame.filterRows 
        (fun (_,(_,(rk,_))) _ -> rk = ("15N Q"))
    |> Frame.applyLevel 
        (fun (ex,(le,(rk,_))) -> (ex,(le,rk)))
        Stats.mean
    |> Frame.dropSparseCols
    |> fun x -> 
        x
        |> Frame.toArray2D
        |> Array2D.toJaggedArray
        |> JaggedArray.transpose
        |> Array.zip (x.ColumnKeys |> Array.ofSeq)
        //Calculate fully/uncompletely labeled peak ratio
        |> Array.map 
            (fun (key,values) -> 
                key,
                //N15-1 / N15
                (values.[1] ,values.[3]),
                //N15-1
                values.[0],
                //N15
                values.[2]
            )
        |> Array.map 
            (fun ((prot,(peptideSequence,charge)),(n15Minus1Quant,n15Quant),n15Minus1MZ,n15Mz) -> 

                let peakRatio = n15Minus1Quant / n15Quant

                printfn "[%s] : %s @ + %i" prot peptideSequence charge

                let peptide =
                    peptideSequence
                    |> BioArray.ofAminoAcidString
                    |> BioSeq.toFormula

                let theoreticalIsotopicDistributions =
                    [for i in 0.5 .. 0.001 .. 0.999 do
                        if ((int (i*1000.)) % 100) = 0 then
                            printfn "%.3f" i
                        yield
                            i,
                            peptide
                            |> initlabelN15Partial i
                            |> Formula.add Formula.Table.H2O
                            |> generateIsotopicDistributionOfFormulaByMax charge
                    ]

                let theoreticalRatios =
                    theoreticalIsotopicDistributions
                    |> List.map 
                        (fun (le,dist) ->
                            let n15Prob = 
                                dist
                                |> List.minBy 
                                    (fun (mz,prob) ->
                                        abs (n15Mz - mz)
                                    )
                                |> snd
                                
                            let n15Minus1Prob = 
                                dist
                                |> List.minBy 
                                    (fun (mz,prob) ->
                                        abs (n15Minus1MZ - mz)
                                    )
                                |> snd
                            le,(n15Minus1Prob / n15Prob), dist
                        )

                let bestLE = 
                    theoreticalRatios
                    |> List.minBy
                        (fun (le,ratio,dist) ->
                            abs (peakRatio - ratio)
                        )


                (prot,(peptideSequence,charge)),
                [(n15Minus1MZ,n15Minus1Quant);(n15Mz,n15Quant)],
                bestLE

                //let monoN15Mass =
                //    theoreticalIsotopicDistributions
            )
    //|> Array.zip (labelEfficiencyResults.ColumnKeys |> Array.ofSeq)
    //|> fun x -> frame ["Label Efficiency" => series x]
    //|> Frame.sortRowsByKey

let allPredictedLE =
    labelEfficiency
    |> Array.map 
        (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) ->
            (prot,peptideSequence),le
        )

let outlierBorders = FSharp.Stats.Testing.Outliers.tukey 3. (allPredictedLE |> Array.map snd)

//Boxplot with outlier borders
allPredictedLE
|> Array.map snd
|> fun x -> 
    [
    Chart.BoxPlot(
        x = (
            allPredictedLE
            |> Array.filter (fun (keys,eff) -> eff < outlierBorders.Upper && eff > outlierBorders.Lower)
            |> Array.map snd
        ),
        Jitter = 0.3,
        Boxpoints=StyleParam.Boxpoints.All
        )
    Chart.BoxPlot(x=x,Jitter = 0.3,Boxpoints=StyleParam.Boxpoints.All)
    |> Chart.withShapes 
        [
            (Shape.init(StyleParam.ShapeType.Line, X0 = outlierBorders.Upper, X1 = outlierBorders.Upper, Y0 = -0.4, Y1 = 1.4))
            (Shape.init(StyleParam.ShapeType.Line, X0 = outlierBorders.Lower, X1 = outlierBorders.Lower, Y0 = -0.4, Y1 = 1.4))
        ]
    ]
|> Chart.Combine
|> Chart.withX_Axis (yAxis false "N15 / N15 - 1 m/z Peak instensity ratio" 20 16 (0.925, 1.075))
|> Chart.withY_Axis (xAxis false "" 20 16)
|> Chart.withTitle "Label efficiency - Outlier detection"
|> Chart.withSize (1200.,600.)
|> Chart.withConfig config
|> Chart.ShowWithDescription
    {Heading = "Borders From tukey outlier detection with k = 3:"; Text = sprintf "Upper: %.3f <br></br>Lower: %.3f" outlierBorders.Upper outlierBorders.Lower}

///Outlier Peptides in Label efficiency
let outliersInLE =
    allPredictedLE
    |> Array.filter (fun (keys,eff) -> eff > outlierBorders.Upper || eff < outlierBorders.Lower)

///Label efficiency mean value without outliers
let filteredOverallPredictedLabelEfficiency =
    allPredictedLE
    |> Array.filter (fun (keys,eff) -> eff < outlierBorders.Upper && eff > outlierBorders.Lower)
    |> Array.map snd
    |> Seq.median 

labelEfficiency
|> Array.sortBy (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) -> prot)
|> Array.map 
    (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) ->
    
        let normExperimental =
            experimentalDist
            |> List.unzip
            |> fun (x,y) ->
                List.zip
                    x
                    (
                        y
                        |> fun vals -> 
                            let max = List.max vals
                            vals
                            |> List.map (fun v -> v / max)
                    )

        [
            dist
            |> List.map (fun (x,y) -> [(x,0.);(x,y);(x,0.)])
            |> List.concat
            |> Chart.Line
            |> Chart.withTraceName (sprintf "[%s] %s : PID @ %.3f " prot peptideSequence le)
            Chart.Point(normExperimental,Name= sprintf "[%s] %s : Experimental Masses"  prot peptideSequence)
        ]
        |> Chart.Combine
        |> Chart.withTitle (sprintf "[%s] : %s @ + %i" prot peptideSequence charge)
        |> Chart.withX_Axis (xAxis false "" 20 16)
        |> Chart.withY_Axis (yAxis false "" 20 16 (0.,1.1))
        |> Chart.withConfig config
    )
    |> Chart.Stack 4
    |> Chart.withSize (1500.,1500.)
    |> Chart.Show

//Overall
//1. Generate multiple patterns with label efficiencies
//2. See which fits best
//3. Take this LE for the peptide
//4. take median to get overall label efficiency

"NILLNEGIR"
|> BioArray.ofAminoAcidString
|> BioSeq.toFormula
|> Formula.count (Elements.Table.N)




[
    "AFPDAYVR"
    |> BioArray.ofAminoAcidString
    |> BioSeq.toFormula
    |> Formula.add Formula.Table.H2O
    |> initlabelN15Partial 0.999
    |> generateIsotopicDistributionOfFormulaByMax 2 
    |> Chart.Point
    |> Chart.withTraceName "0.999% 15N"

    "AFPDAYVR"
    |> BioArray.ofAminoAcidString
    |> BioSeq.toFormula
    |> Formula.add Formula.Table.H2O
    |> generateIsotopicDistributionOfFormulaByMax 2 
    |> Chart.Point
    |> Chart.withTraceName "Natural Abundance"

]
|> Chart.Combine
|> Chart.withX_Axis (xAxis false "" 20 16)
|> Chart.withY_Axis (xAxis false "" 20 16)
|> Chart.withConfig config
|> Chart.Show


//Correction Factor
//1. Generate Pattern with overall Label efficiency
//2. Generate fully labeled MIDAs pattern
//3. calculate ideal LabelEfficiency ~100%

type LabelEfficiencyPredictor =
    {
        Protein: string
        PeptideSequence: string
        Charge: int
        PredictedDistribution: (float*float) list
        PredictedLabelEfficiency: float
        ExperimentalDistribution: (float*float) list
    }

let createLabelEfficiencyPredictor p ps c pred eff exp =
    {
        Protein                     = p
        PeptideSequence             = ps
        Charge                      = c
        PredictedDistribution       = pred
        PredictedLabelEfficiency    = eff
        ExperimentalDistribution    = exp
    }


type LabelEfficiencyResult =
    {
        Protein                 : string
        PeptideSequence         : string
        Charge                  : int
        PredictedLabelEfficiency: float
        PredictedPattern        : (float*float) list
        ActualPattern           : (float*float) list
        MedianLabelEfficiency   : float
        MedianPattern           : (float*float) list
        FullLabeledPattern      : (float*float) list
        CorrectionFactor        : float
    }

let createLabelEfficiencyResult p ps c predLE predP aP mLE mP flP cf =
    {
        Protein                     = p
        PeptideSequence             = ps
        Charge                      = c
        PredictedLabelEfficiency    = predLE
        PredictedPattern            = predP
        ActualPattern               = aP
        MedianLabelEfficiency       = mLE
        MedianPattern               = mP
        FullLabeledPattern          = flP
        CorrectionFactor            = cf
    }




let getCorrectionFactors (medianPredictedLabelEfficiency:float) (predictors: LabelEfficiencyPredictor []) =
    predictors
    |> Array.map 
       (fun lePredictor ->

           let n15Minus1Mz, n15Minus1Quant =
               lePredictor.ExperimentalDistribution.[0]

           let n15Mz, n15Quant =
               lePredictor.ExperimentalDistribution.[1]

           let formulaWithH2O =
               lePredictor.PeptideSequence
               |> BioArray.ofAminoAcidString
               |> BioSeq.toFormula
               |> Formula.add Formula.Table.H2O

           let predictedWithMedianLE =
               formulaWithH2O
               |> initlabelN15Partial medianPredictedLabelEfficiency
               |> generateIsotopicDistributionOfFormulaBySum lePredictor.Charge

           let predictedWithMedianLENorm = 
               formulaWithH2O
               |> initlabelN15Partial medianPredictedLabelEfficiency
               |> generateIsotopicDistributionOfFormulaByMax lePredictor.Charge

           let predictedWithFullLE = 
               formulaWithH2O
               |> initlabelN15Partial 0.99999
               |> generateIsotopicDistributionOfFormulaBySum lePredictor.Charge

           let predictedWithFullLENorm =
               formulaWithH2O
               |> initlabelN15Partial 0.99999
               |> generateIsotopicDistributionOfFormulaByMax lePredictor.Charge

           let n15ProbWithMedianLE =
               predictedWithMedianLE
               |> List.minBy   
                   (fun (mz,prob) -> abs (mz - n15Mz))

           let n15ProbWithFullLE =
               predictedWithFullLE
               |> List.minBy   
                   (fun (mz,prob) -> abs (mz - n15Mz))

           let correctionFactor = 
               snd n15ProbWithFullLE / snd n15ProbWithMedianLE

           createLabelEfficiencyResult
               lePredictor.Protein
               lePredictor.PeptideSequence
               lePredictor.Charge
               lePredictor.PredictedLabelEfficiency
               lePredictor.PredictedDistribution
               (
                   lePredictor.ExperimentalDistribution
                   |> List.unzip
                   |> fun (x,y) ->
                       List.zip
                           x
                           (
                               y
                               |> fun vals -> 
                                   let max = List.max vals
                                   vals
                                   |> List.map (fun v -> v / max)
                           )
               )
               medianPredictedLabelEfficiency
               predictedWithMedianLENorm
               predictedWithFullLENorm
               correctionFactor

       )


let plotLabelEfficiencyResult (leRes: LabelEfficiencyResult) =
    [
        leRes.FullLabeledPattern
        |> List.map (fun (x,y) -> [(x,0.);(x,y);(x,0.)])
        |> List.concat
        |> Chart.Line
        |> Chart.withTraceName ("Fully Labeled Pattern")
        |> Chart.withLineStyle(Color="lightgray",Width = 20)

        leRes.MedianPattern
        |> List.map (fun (x,y) -> [(x,0.);(x,y);(x,0.)])
        |> List.concat
        |> Chart.Line
        |> Chart.withTraceName (sprintf "CorrectedPattern @ Median LE of %.3f" leRes.MedianLabelEfficiency)
        |> Chart.withLineStyle(Width = 10,Color="lightgreen")

        leRes.PredictedPattern
        |> List.map (fun (x,y) -> [(x,0.);(x,y);(x,0.)])
        |> List.concat
        |> Chart.Line
        |> Chart.withTraceName (sprintf "PredictedPattern @ %.3f LE" leRes.PredictedLabelEfficiency)
        |> Chart.withLineStyle(Color="orange",Width = 5)

        Chart.Point(leRes.ActualPattern,Name="Experimental Values")
        |> Chart.withMarkerStyle(Size = 15,Symbol = StyleParam.Symbol.X, Color = "lightred")



    ]
    |> Chart.Combine
    |> Chart.withTitle (sprintf "[%s] : %s @ z = %i" leRes.Protein leRes.PeptideSequence leRes.Charge)
    |> Chart.withX_Axis (xAxis false "m/z" 20 16)
    |> Chart.withY_Axis (yAxis false "normalized probability" 20 16 (0.,1.1))
    |> Chart.withConfig config


let labelEfficiencyResultsFinal =
    labelEfficiency
    |> Array.map 
        (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) ->
            createLabelEfficiencyPredictor prot peptideSequence charge dist le experimentalDist    
        )
    |> getCorrectionFactors filteredOverallPredictedLabelEfficiency

labelEfficiencyResultsFinal
|> Array.map plotLabelEfficiencyResult
|> Array.item 7
|> Chart.withSize (1000.,1000.)
|> Chart.Show


labelEfficiencyResultsFinal
|> Array.map plotLabelEfficiencyResult
|> Array.map (Chart.withSize (1000.,1000.))
|> Array.map Chart.Show


let labelEfficiencyFrame =
    labelEfficiencyResultsFinal
    |> Array.map 
        (fun leRes ->
            (leRes.Protein,(leRes.PeptideSequence,leRes.Charge)) => 
                series 
                    [
                        "PredictedLabelEfficiency"      => leRes.PredictedLabelEfficiency
                        "CorrectionFactor"              => leRes.CorrectionFactor
                    ]
        )
    |> frame
    |> Frame.transpose
    |> fun x ->
        let predDistCol =
            labelEfficiencyResultsFinal
            |> Array.map 
                (fun leRes -> (leRes.Protein,(leRes.PeptideSequence,leRes.Charge)) => leRes.PredictedPattern)
            |> series

        let actualDistCol = 
            labelEfficiencyResultsFinal
            |> Array.map 
                (fun leRes -> (leRes.Protein,(leRes.PeptideSequence,leRes.Charge)) => leRes.ActualPattern)
            |> series

        let medianLECol = 
            labelEfficiencyResultsFinal
            |> Array.map 
                (fun leRes -> (leRes.Protein,(leRes.PeptideSequence,leRes.Charge)) => leRes.MedianLabelEfficiency)
            |> series

        let medianDistCol =
            labelEfficiencyResultsFinal
            |> Array.map 
                (fun leRes -> (leRes.Protein,(leRes.PeptideSequence,leRes.Charge)) => leRes.MedianPattern)
            |> series

        let fullLabeDist = 
            labelEfficiencyResultsFinal
            |> Array.map 
                (fun leRes -> (leRes.Protein,(leRes.PeptideSequence,leRes.Charge)) => leRes.FullLabeledPattern)
            |> series

        x
        |> Frame.addCol "predictedIsotopicDistribution" predDistCol
        |> Frame.addCol "ExperimentallyObservedIsotopicPattern" actualDistCol
        |> Frame.addCol "MedianLabelEfficiency" medianLECol
        |> Frame.addCol "IsotopicDistributionAtMedianLabelEfficiency" medianDistCol
        |> Frame.addCol "IsotopicDistributionAt100%LabelEfficiency" fullLabeDist
        |> Frame.sortRowsByKey
        |> Frame.sortColsByKey

let labelEfficiencyCorrectionFactorsOnly : Series<(string * string),float>  =
    labelEfficiencyFrame
    |> Frame.applyLevel (fun (a,(b,_)) -> a,b) Stats.mean
    |> Frame.getCol "CorrectionFactor"


//let allLE =
//    let tmp =  
//        labelEfficiency
//        |> Frame.dropSparseRows
//    tmp
//    |> Frame.toArray2D
//    |> Array2D.toJaggedArray
//    |> Array.concat
//    |> Array.zip (tmp.RowKeys |> Array.ofSeq)
     
////Outlier detection to discard
//let outlierBorders = FSharp.Stats.Testing.Outliers.tukey 3. (allLE |> Array.map snd)

////Boxplot with outlier borders
//allLE
//|> Array.map snd
//|> fun x -> 
//    Chart.BoxPlot
//        (x=x,Jitter = 0.1,Boxpoints=StyleParam.Boxpoints.All)
//|> Chart.withShapes 
//    [
//        (Shape.init(StyleParam.ShapeType.Line, X0 = outlierBorders.Upper, X1 = outlierBorders.Upper, Y0 = -0.4, Y1 = 0.4))
//        (Shape.init(StyleParam.ShapeType.Line, X0 = outlierBorders.Lower, X1 = outlierBorders.Lower, Y0 = -0.4, Y1 = 0.4))
//    ]
//|> Chart.withX_Axis (yAxis false "N15 / N15 - 1 m/z Peak instensity ratio" 20 16 (0.25, 1.25))
//|> Chart.withY_Axis (xAxis false "" 20 16)
//|> Chart.withTitle "Label efficiency - Outlier detection"
//|> Chart.withSize (1000.,400.)
//|> Chart.withConfig config
//|> Chart.ShowWithDescription
//    {Heading = "Borders From tukey outlier detection with k = 3:"; Text = sprintf "Upper: %.3f <br></br>Lower: %.3f" outlierBorders.Upper outlierBorders.Lower}

/////Outlier Peptides in Label efficiency
//let outliersInLE =
//    allLE
//    |> Array.filter (fun (keys,eff) -> eff > outlierBorders.Upper || eff < outlierBorders.Lower)

/////Label efficiency mean value without outliers
//let filteredOverallLabelEfficiency =
//    allLE
//    |> Array.filter (fun (keys,eff) -> eff < outlierBorders.Upper && eff > outlierBorders.Lower)
//    |> Seq.meanBy snd


//================================================================================
//====================== 2. WholeCell ISD ========================================
//================================================================================


let wholeCellResults : Frame<string*(string*(string*string)),string*string> = 
    readQConcatResultFrame @"C:\Users\kevin\Downloads\CsbScaffold-master\BioTechVP\RERUN\RERUN_Results\WholeCell\QuantifiedPeptides.txt"
    |> Frame.mapColKeys 
        (fun (ck:string) -> 
            //printfn "%s" ck
            let newCK = Map.find (ck.Split('_').[1 ..] |> String.concat "_") wholeCellNameMapping
            ck.Split('_').[0] , newCK
        )
    |> Frame.sortColsByKey
    |> Frame.filterCols (fun ck _ -> (fst ck).Contains("Quant"))
    |> Frame.applyLevel (fun (sequence,(gmod,charge)) -> sequence) Stats.mean
    |> Frame.transpose
    |> Frame.mapColKeys
        (fun ck ->
            match Map.tryFind ck peptideProtMapping with
            |Some prot  -> prot,ck
            |None       -> "NotFound",ck
        )  
    //|> Frame.transpose
    //|> fun f ->
    //    let keysToCorrect =
    //        f
    //        |> Frame.filterCols (fun (ck,_) _ -> ck = "N15Quant")
    //        |> fun x -> x.ColumnKeys
    //        |> Array.ofSeq
    //    let correctedN15 = 
    //        keysToCorrect
    //        |> Array.map 
    //            (fun (k,(exp,(ratio,rep))) ->
    //                ("N15QuantCorrected",(exp,(ratio,rep))) => (f |> Frame.getCol (k,(exp,(ratio,rep)))) * labelEfficiencyCorrectionFactorsOnly
    //            )
    //        |> frame
    //    f |> Frame.merge correctedN15
    //|> Frame.transpose




let forLinearity = 
    wholeCellResults
    |> Frame.mapCols(fun _ os -> os.As<float>() |> Series.mapValues (fun (x:float) -> if x > 2000000. || x < 1. then nan else x))
    //|> Frame.filterCols (fun ck _ -> (fst ck).ToLower().Contains("rbc"))
    |> Frame.mapRowKeys 
        (fun (q,(n,(ratio,rep))) ->  
            let ratioSplit = ratio.Split(':')
            printfn "%A" ratioSplit
            let ratio' = (float ratioSplit.[0]) / (float ratioSplit.[1])
            (q,(n,(ratio',rep)))
    
        )
    // Bad Peptides according to Hammel et al
    |> Frame.filterCols (fun ck _ -> not ((snd ck) = "EVTLGFVDLMR") && not ((snd ck) = "AFPDAYVR"))
    |> Frame.filterRows (fun (q,(n,ratio)) _ -> not (q.Contains("Minus")))
    |> Frame.sortRowsByKey
    |> Frame.sortColsByKey


let n14Lin =
    forLinearity
    |> Frame.filterRows (fun rk _ -> not ((fst rk).Contains("N15Quant")))
    |> Frame.transpose
    |> Frame.toArray2D
    |> Array2D.toJaggedArray
    // Group 3 had 4x less than in protocol
    |> JaggedArray.mapi (fun i x -> if List.contains i [ 0 .. 2 ..7 ] then x/4. else x)

let n15Lin =
    forLinearity
    |> Frame.filterRows (fun rk _ -> not ((fst rk).Contains("N14")) && not ((fst rk).Contains("Corrected")))
    |> Frame.transpose
    |> Frame.toArray2D
    |> Array2D.toJaggedArray


/// Return full protein and mapping peptide plot for all concentrations for whole cell ISD
let plotPeptideISD (proteinNames : string []) =

    Array.map2 (Array.zip) n14Lin n15Lin
    |> JaggedArray.map (fun (n14,n15) -> if (isBad n14 || isBad n15) then 0. else n14/n15)
    |> Array.map (fun x -> Array.zip[|10.;10.;5.;5.;1.;1.;0.5;0.5|] x)
    |> Array.map 
        (fun (values) -> 
            values 
            |> Array.chunkBySize 2 
            |> Array.map (fun reps -> fst reps.[0] , reps |> Array.map snd |> Seq.mean),
            [|for i in 0 .. 2 ..7 do yield values.[i]|],
            [|for i in 1 .. 2 ..7 do yield values.[i]|]
            )
    |> Seq.zip (forLinearity.ColumnKeys)
    |> Seq.groupBy (fst >> fst)
    |> Seq.map 
        (fun ((protein,peptideMeans)) -> 
            let peptidePlots =
                peptideMeans
                |> Seq.map 
                    (fun ((prot,pep),(means,r1,r2)) ->
                    [
                        Chart.Scatter(r1,mode=StyleParam.Mode.Markers, MarkerSymbol = StyleParam.Symbol.Cross, Color="#92a8d1")
                        |> Chart.withTraceName "G3"
                        Chart.Scatter(r2,mode=StyleParam.Mode.Markers, MarkerSymbol = StyleParam.Symbol.Cross, Color="#FF6F61")
                        |> Chart.withTraceName "G4"
                        //|> GenericChart.mapTrace
                        //    (fun t ->
                        //        t?fill <- "tonexty"
                        //        t?fillcolor <- "lightgrey"
                        //        t
                        //    )
                        Chart.Scatter(means,mode=StyleParam.Mode.Lines_Markers, MarkerSymbol = StyleParam.Symbol.Circle, Color = "#C78D9B") (*|> Chart.withYError (Error.init(Symmetric=true,Array=se))*)
                        |> Chart.withTraceName (sprintf "[%s] %s" prot pep)
                    ]
                    |> Chart.Combine
                    |> Chart.withX_Axis (xAxis false "N14 Sample / N15 QProtein ratio" 20 16)
                    |> Chart.withY_Axis (yAxis false "N14/N15 Quantification ratio" 20 16 (0.,7.5))
                    
                )

            let meanPoints =
                peptideMeans
                |> fun x -> Seq.zip x (mainColors.[0 .. ((Seq.length x )-1)])
                |> Seq.map 
                    (fun (((_,pep),(means,r1,r2)),color) ->
                        Chart.Scatter(means,mode=StyleParam.Mode.Markers, MarkerSymbol = StyleParam.Symbol.Cross, Color=color)
                        |> Chart.withTraceName pep
                    )
                |> Chart.Combine
                
            let proteinLine =
                peptideMeans
                |> Seq.map 
                    (fun ((_,_),(means,r1,r2)) -> means)
                |> JaggedArray.ofJaggedSeq
                |> JaggedArray.transpose
                |> Array.map 
                    (fun x -> 
                        x
                        |> Array.unzip
                        |> fun (x,y) -> Seq.mean x, Seq.mean y)
                |> fun x -> Chart.Scatter(x,mode=StyleParam.Mode.Lines_Markers, MarkerSymbol = StyleParam.Symbol.Circle) (*|> Chart.withYError (Error.init(Symmetric=true,Array=se))*)
                |> Chart.withTraceName protein

            protein, 
            [ 
                [meanPoints;proteinLine] 
                |> Chart.Combine
                |> Chart.withX_Axis (xAxis false "N14 Sample / N15 QProtein ratio" 20 16)
                |> Chart.withY_Axis (yAxis false "N14/N15 Quantification ratio" 20 16 (0.,7.5))
                yield! peptidePlots
            ]
            )
           
    |> Seq.filter (fun (x,_) -> 
        proteinNames
        |> Array.exists (fun p -> x.ToLower().Contains(p) )
        )
    |> Seq.map (fun (prot,x) -> x |> Chart.Stack 2 |> Chart.withTitle prot)
    |> Seq.map (Chart.withConfig config)
    |> Seq.map (Chart.withSize (750.,750.))
    |> Array.ofSeq
    |> Array.iter Chart.Show


plotPeptideISD [|"rbcl";"rbcs"|]


let wholeCell_PeptideRatios =

    Array.map2 (Array.zip) n14Lin n15Lin
    |> JaggedArray.map (fun (n14,n15) -> if (isBad n14 || isBad n15) then 0. else n14/n15)
    |> Array.map (fun x -> Array.zip[|10.;10.;5.;5.;1.;1.;0.5;0.5|] x)
    |> Array.map 
        (fun (values) -> 
            values 
            |> Array.chunkBySize 2 
            |> Array.map (fun reps -> fst reps.[0] , reps |> Array.map snd |> Seq.mean),
            [|for i in 0 .. 2 ..7 do yield values.[i]|],
            [|for i in 1 .. 2 ..7 do yield values.[i]|]
            )
    |> Seq.zip (forLinearity.ColumnKeys)
    |> Seq.groupBy (fst >> fst)
    |> Seq.map (snd)
    |> Seq.map 
        (
            fun x ->
                let (rk1,rk2),(mean,r1,r2) = Seq.item 0 x
                [
                    (rk1,(rk2,"mean")) => series mean
                    (rk1,(rk2,"r1")) => series r1
                    (rk1,(rk2,"r2")) => series r2
                ]
                |> frame
    
        )
    |> Seq.reduce (Frame.join JoinKind.Outer)
    |> Frame.transpose

let rbcl_RatiosS_wholeCell =
    wholeCell_PeptideRatios
    |> Frame.filterRows (fun (_,(_,rk)) _ -> rk = "mean")
    |> Frame.transpose
    |> Frame.filterCols (fun (prot,(pep,_)) _ -> prot = "rbcL" && (not (pep = "AFPDAYVR" || pep = "EVTLGFVDLMR")))
    |> Frame.transpose
    |> Frame.applyLevel fst Stats.mean

let rbcs_RatiosSdsIgd_wholeCell =
    wholeCell_PeptideRatios
    |> Frame.filterRows (fun (_,(_,rk)) _ -> rk = "mean")
    |> Frame.transpose
    |> Frame.filterCols (fun (prot,(pep,_)) _ -> prot = "RBCS2" && (not (pep = "AFPDAYVR" || pep = "EVTLGFVDLMR")))
    |> Frame.transpose
    |> Frame.applyLevel fst Stats.mean

open FSharp.Stats.Fitting.LinearRegression.OrdinaryLeastSquares.Linear



let rbc_L_vs_S_rbcl_RatiosS_wholeCell =

    let rbcs =
        rbcs_RatiosSdsIgd_wholeCell
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D
        |> Array.concat

    //let ratios =
    //    Array.zip3 [|10.;5.;1.;0.5|] rbcl rbcs
    //    |> Array.map (fun (x,l,s) -> x, (l/s))

    let RBCScoeff = Univariable.coefficient (vector [|10.;5.;1.;0.5|]) (vector rbcs) 
    let RBCSfitFunc = Univariable.fit RBCScoeff
    let RBCSfitVals = [|10.;5.;1.;0.5|] |> Array.map RBCSfitFunc
    let RBCSdetermination = FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue rbcs RBCSfitVals
    let RBCSpearson = FSharp.Stats.Correlation.Seq.pearson (rbcs) [|10.;5.;1.;0.5|]
    printfn "Pearson WholeCell RBCS: %f" RBCSpearson

    let rbcl =
        rbcl_RatiosS_wholeCell
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D
        |> Array.concat

    // RBCL Regression of relative quantification values
    let RBCLcoeff = Univariable.coefficient (vector [|10.;5.;1.;0.5|]) (vector rbcl)
    let RBCLfitFunc = Univariable.fit RBCLcoeff
    let RBCLfitVals = [|10.;5.;1.;0.5|] |> Array.map RBCLfitFunc
    let RBCLdetermination = FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue rbcl RBCLfitVals
    let RBCLpearson = FSharp.Stats.Correlation.Seq.pearson (rbcl) [|10.;5.;1.;0.5|]
    printfn "Pearson WholeCell RBCL: %f" RBCLpearson



    [
        Chart.Point ((Array.zip [|10.;5.;1.;0.5|] rbcl),Name = "RbcL Quantified Ratios")
        |> Chart.withMarkerStyle(Size=10,Symbol = StyleParam.Symbol.Cross)
        Chart.Line(Array.zip [|10.;5.;1.;0.5|] RBCLfitVals,Name = (sprintf "RbcL linear regression: %.2f x + (%2f) ; R² = %.4f" RBCLcoeff.[1] RBCLcoeff.[0] RBCLdetermination))
        |> Chart.withLineStyle(Color="lightblue",Dash=StyleParam.DrawingStyle.DashDot)

        Chart.Point ((Array.zip [|10.;5.;1.;0.5|] rbcs),Name = "RbcS Quantified Ratios",MarkerSymbol = StyleParam.Symbol.Cross)
        |> Chart.withMarkerStyle(Size=10,Symbol = StyleParam.Symbol.Cross)
        Chart.Line(Array.zip [|10.;5.;1.;0.5|] RBCSfitVals,Name = (sprintf "RbcS linear regression: %.2f x + (%2f) ; R² = %.4f" RBCScoeff.[1] RBCScoeff.[0] RBCSdetermination))
        |> Chart.withLineStyle(Color="LightGreen",Dash=StyleParam.DrawingStyle.DashDot)
    ]
    |> Chart.Combine
    |> Chart.withTitle "Whole cell extracts: Stability of rbcl/rbcs ratios between samples"
    |> Chart.withX_Axis (yAxis false "N14 Sample / N15 QProtein ratio" 20 16 (0. ,10.5))
    |> Chart.withY_Axis (xAxis false "relative quantification" 20 16 (*(1.3,1.6)*))
    |> Chart.withConfig config
    |> Chart.withSize (700.,700.)
    |> Chart.Show

//================================================================================
//====================== 3. SDS IGD ==============================================
//================================================================================

let sdsIgdResults : Frame<(string*(string*(string*(string*string)))),string*string> = 
    readQConcatResultFrame @"C:\Users\kevin\Downloads\CsbScaffold-master\BioTechVP\RERUN\RERUN_Results\SDS_IGD\QuantifiedPeptides.txt"
    |> Frame.mapColKeys 
        (fun (ck:string) -> 
            let newCK = Map.find (ck.Split('_').[1 ..] |> String.concat "_") sdsSampleNameMapping
            ck.Split('_').[0] , newCK
        )
    |> Frame.sortColsByKey
    |> Frame.filterCols (fun ck _ -> (fst ck).Contains("Quant"))
    |> Frame.applyLevel (fun (sequence,(gmod,charge)) -> sequence) Stats.mean
    |> Frame.transpose
    |> Frame.mapColKeys
        (fun ck ->
            match Map.tryFind ck peptideProtMapping with
            |Some prot  -> prot,ck
            |None       -> "NotFound",ck
        )
    |> Frame.sortColsByKey

    |> Frame.sortRowsByKey


let sdsIgdPeptideRatios =
    sdsIgdResults
    |> Frame.mapCols(fun _ os -> os.As<float>() |> Series.mapValues (fun (x:float) -> if x > 2000000. || x < 1. then nan else x))
    |> Frame.filterRows (fun (q,n) _ -> not (q.Contains("Minus")))
    |> Frame.sortRowsByKey
    |> Frame.sortColsByKey
    |> fun f ->
        
        let finalRK =
            f
            |> Frame.filterRows (fun (q,n) _ -> (q.Contains("N14")))
            |> Frame.mapRowKeys snd
            |> fun x -> x.RowKeys

        let N14,N15 = 
            f
            |> Frame.filterRows (fun (q,n) _ -> (q.Contains("N14")))
            |> Frame.toArray2D
            |> JaggedArray.ofArray2D
            ,
            f
            |> Frame.filterRows (fun (q,n) _ -> (q.Contains("N15")))
            |> Frame.toArray2D
            |> JaggedArray.ofArray2D

        Array.map2 (Array.zip) N14 N15
        |> JaggedArray.map (fun (n14,n15) -> if (isBad n14 || isBad n15) then 0. else n14/n15)
        |> Array.map (fun x -> series (Seq.zip f.ColumnKeys x))
        |> Seq.zip finalRK 
        |> frame
        |> Frame.transpose
        // take means AFTER calculating ratios!
        |> Frame.applyLevel (fun (experiment,(band,(proteinAmnt,rep))) -> (experiment,(band,proteinAmnt))) Stats.mean

// Hier ist geladenes Protein bekannt, dadurch kann rückschluss auf anteil rbcl an gesamtprotein gezogen werden
let kda37_RatiosSdsIgd =
    sdsIgdPeptideRatios
    |> Frame.filterRows (fun (_,(band,_)) _ -> band = "37kDa")
    |> Frame.filterCols (fun (prot,pep) _ -> (prot = "rbcL" && (not (pep = "AFPDAYVR" || pep = "EVTLGFVDLMR"))) || (prot = "RBCS2" && (not (pep = "AFPDAYVR" || pep = "EVTLGFVDLMR"))))
    |> Frame.transpose
    //|> Frame.applyLevel fst Stats.mean

kda37_RatiosSdsIgd
|> Frame.filterRows (fun (prot,pep) _ -> (prot = "rbcL"))
|> fun f ->
    let normVals = 
        f
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D
        |> JaggedArray.transpose
        |> Array.map 
            (fun x ->
                let max = Array.max x
                x |> Array.map (fun v -> v / max)
            )
    let keys = 
        f.RowKeys
        |> Seq.map snd
        |> Array.ofSeq

    let names = 
        f.ColumnKeys
        |> Seq.map (fun (_,(_,r)) -> sprintf "%s [ug] total protein" r)
        |> Array.ofSeq

    normVals
    |> Array.map (fun inner -> Array.zip keys inner)
    |> Array.zip names
    |> Array.map (
        fun (k,c) ->
            Chart.Column(
                keysvalues=c,
                Name=k
                )
            |> Chart.withX_Axis (xAxis false "" 20. 18.)
            |> Chart.withY_Axis (yAxis false "" 20. 18. (0.,1.))
        )

    |> Chart.Stack 2
    |> Chart.withSize (1200.,800.)
    |> Chart.withConfig config
    |> Chart.Show

let rbcl_RatiosSdsIgd =
    sdsIgdPeptideRatios
    |> Frame.filterRows (fun (_,(band,_)) _ -> band = "RBCL")
    |> Frame.filterCols (fun (prot,pep) _ -> prot = "rbcL" && (not (pep = "AFPDAYVR" || pep = "EVTLGFVDLMR" || pep = "FLFVAEAIYK")))
    |> Frame.transpose
    |> Frame.applyLevel fst Stats.mean

let rbcs_RatiosSdsIgd =
    sdsIgdPeptideRatios
    |> Frame.filterRows (fun (_,(band,_)) _ -> band = "RBCS")
    |> Frame.filterCols (fun (prot,pep) _ -> prot = "RBCS2" && (not (pep = "AFPDAYVR" || pep = "EVTLGFVDLMR")))
    |> Frame.transpose
    |> Frame.applyLevel fst Stats.mean

//let with37kDa =
//    let corrRbcL = 
//        rbcl_RatiosSdsIgd
//        |> Frame.filterCols(fun (_,(_,r)) _ -> (r = "20") || (r = "80"))
//        |> fun x -> 
//            let lol = kda37_RatiosSdsIgd |> Frame.mapColKeys (fun (a,(b,c)) -> (a,("RBCL",c)))
//            x + lol |> Frame.dropSparseRows

    //let corrRbcS = 
    //    rbcs_RatiosSdsIgd
    //    |> Frame.filterCols(fun (_,(_,r)) _ -> (r = "20") || (r = "80"))
    //    |> fun x -> 
    //        let lol = kda37_RatiosSdsIgd |> Frame.mapColKeys (fun (a,(b,c)) -> (a,("RBCS",c)))
    //        x + lol |> Frame.dropSparseRows 

    //Frame.merge corrRbcL corrRbcS



let rbc_L_vs_S_SDS_Igd =

    let rbcl =
        rbcl_RatiosSdsIgd
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D
        |> Array.concat

    let rbcs =
        rbcs_RatiosSdsIgd
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D
        |> Array.concat

    let xVals,ratios = 
        Array.zip3 [|10.;20.;40.;5.;80.|] rbcl rbcs 
        |> Array.map 
            (fun (protAmnt,rbcl,rbcs) ->
                protAmnt, (rbcl/rbcs)
            )
        |> Array.sortBy fst
        //|> Array.skip 1
        |> Array.unzip


    let coeff = Univariable.coefficient (vector xVals) (vector ratios)
    let fitFunc = Univariable.fit coeff
    let fitVals = xVals |> Array.map fitFunc
    let determination = FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue ratios fitVals
    let pearson = FSharp.Stats.Correlation.Seq.pearson ratios xVals
    printfn "Pearson IGD: %f" pearson

    [
        Chart.Point (xVals,ratios,Name = "Quantified Ratios",MarkerSymbol = StyleParam.Symbol.Cross)
        Chart.Line(Array.zip xVals fitVals,Name = (sprintf "linear regression: %.2f x + (%2f) ; R² = %.4f" coeff.[1] coeff.[0] determination))
        |> Chart.withLineStyle(Color="#D3D3D3",Dash=StyleParam.DrawingStyle.DashDot)
    ]
    |> Chart.Combine
    |> Chart.withTitle "SDS IGD: Stability of rbcl/rbcs ratios between samples"
    |> Chart.withX_Axis (xAxis false "absolute protein amount in sample [unit]" 20 16 )
    |> Chart.withY_Axis (xAxis false "relative quantification (rbcL/rbcS)" 20 16 )
    |> Chart.withConfig config
    |> Chart.withSize (700.,700.)
    |> Chart.Show


//================================================================================
//====================== 4. BlueNative IGD =======================================
//================================================================================



let blueNativeIgdResults : Frame<(string*(string*(string*(string*string)))),string*string> = 
    readQConcatResultFrame @"C:\Users\kevin\Downloads\CsbScaffold-master\BioTechVP\RERUN\RERUN_Results\BN_IGD\QuantifiedPeptides.txt"
    |> Frame.mapColKeys 
        (fun (ck:string) -> 
            printfn "%s" ck
            let newCK = Map.find (ck.Split('_').[1 ..] |> String.concat "_") bnNameMapping
            ck.Split('_').[0] , newCK
        )
    |> Frame.sortColsByKey
    |> Frame.filterCols (fun ck _ -> (fst ck).Contains("Quant"))
    |> Frame.applyLevel (fun (sequence,(gmod,charge)) -> sequence) Stats.mean
    |> Frame.transpose
    |> Frame.mapColKeys
        (fun ck ->
            match Map.tryFind ck peptideProtMapping with
            |Some prot  -> prot,ck
            |None       -> "NotFound",ck
        )
    |> Frame.sortColsByKey
    //|> Frame.map 
    //    (fun (n,_) ck v ->
    //        if n = "N15Quant" then
    //            v * filteredOverallLabelEfficiency
    //        else v
    //    )


let BNPeptideRatios =
    blueNativeIgdResults
    |> Frame.mapCols(fun _ os -> os.As<float>() |> Series.mapValues (fun (x:float) -> if x > 1000000. || x < 1. then nan else x))
    |> Frame.filterRows (fun (q,n) _ -> not (q.Contains("Minus")))
    |> Frame.sortRowsByKey
    |> Frame.sortColsByKey
    |> fun f ->
        
        let finalRK =
            f
            |> Frame.filterRows (fun (q,n) _ -> (q.Contains("N14")))
            |> Frame.mapRowKeys snd
            |> fun x -> x.RowKeys

        let N14,N15 = 
            f
            |> Frame.filterRows (fun (q,n) _ -> (q.Contains("N14")))
            |> Frame.toArray2D
            |> JaggedArray.ofArray2D
            ,
            f
            |> Frame.filterRows (fun (q,n) _ -> (q.Contains("N15")))
            |> Frame.toArray2D
            |> JaggedArray.ofArray2D

        Array.map2 (Array.zip) N14 N15
        |> JaggedArray.map (fun (n14,n15) -> if (isBad n14 || isBad n15) then 0. else n14/n15)
        |> Array.map (fun x -> series (Seq.zip f.ColumnKeys  x))
        |> Seq.zip finalRK
        |> frame
        |> Frame.transpose
        // take means AFTER calculating ratios!
        |> Frame.applyLevel (fun (experiment,(freeze,(band,rep))) -> (experiment,(freeze,band))) Stats.mean


BNPeptideRatios
|> Frame.transpose
|> Frame.applyLevel fst Stats.mean
|> Frame.transpose
|> fun f ->
    let rk = f.RowKeys |> Seq.map (fun (_,(_,band)) -> band )
    let ck = f.ColumnKeys 
    let data = f |> Frame.toArray2D |> JaggedArray.ofArray2D
    Chart.Heatmap(data,ColNames = ck,RowNames=rk,Colorscale=StyleParam.Colorscale.Custom [(0.,"#dbecff");(1.,"#000685")])
    |> Chart.withSize(1200.,400.)
    |> Chart.withConfig config
    |> Chart.Show

let rbcl_RatiosBN =
    BNPeptideRatios
    |> Frame.filterRows (fun (_,(_,band)) _ -> band = "RBC_Band")
    |> Frame.filterCols (fun (prot,pep) _ -> prot = "rbcL" && (not (pep = "AFPDAYVR" || pep = "EVTLGFVDLMR")))
    |> Frame.transpose
    |> Frame.applyLevel fst Stats.mean

let rbcs_RatiosBN =
    BNPeptideRatios
    |> Frame.filterRows (fun (_,(_,band)) _ -> band = "RBC_Band")
    |> Frame.filterCols (fun (prot,pep) _ -> prot = "RBCS2" && (not (pep = "AFPDAYVR" || pep = "EVTLGFVDLMR")))
    |> Frame.transpose
    |> Frame.applyLevel fst Stats.mean

let rbc_L_vs_S_BN =

    let rbcl =
        rbcl_RatiosBN
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D
        |> Array.concat

    let rbcs =
        rbcs_RatiosBN
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D
        |> Array.concat

    rbcl.[0] / rbcs.[0]


//================================================================================
//============================= 5. Overall =======================================
//================================================================================



let overallRatioPlot =
    
    
    let wholeCell =
    
        let rbcl =
            rbcl_RatiosS_wholeCell
            |> Frame.toArray2D
            |> JaggedArray.ofArray2D
            |> Array.concat
    
        let rbcs =
            rbcs_RatiosSdsIgd_wholeCell
            |> Frame.toArray2D
            |> JaggedArray.ofArray2D
            |> Array.concat

        Array.zip rbcl rbcs
        |> Array.map (fun (l,s) -> (l/s))
    
    let SDS_Igd =
    
        let rbcl =
            rbcl_RatiosSdsIgd
            |> Frame.toArray2D
            |> JaggedArray.ofArray2D
            |> Array.concat
    
        let rbcs =
            rbcs_RatiosSdsIgd
            |> Frame.toArray2D
            |> JaggedArray.ofArray2D
            |> Array.concat
    

        Array.zip rbcl rbcs 
        |> Array.map 
            (fun (rbcl,rbcs) ->
                (rbcl/rbcs)
            )
    
    let BN =
    
        

        let rbcl =
            rbcl_RatiosBN
            |> Frame.toArray2D
            |> JaggedArray.ofArray2D
            |> Array.concat
    
        let rbcs =
            rbcs_RatiosBN
            |> Frame.toArray2D
            |> JaggedArray.ofArray2D
            |> Array.concat
    
        [|rbcl.[0] / rbcs.[0]|]

    [
        Chart.BoxPlot
            (
                x = BN,
                Name = "Native Complexes by BlueNative",
                Jitter = 0.3,
                Boxpoints = StyleParam.Boxpoints.All

            )
        Chart.BoxPlot
            (
                x = SDS_Igd,
                Name = "Separated Subunits by SDS Page",
                Jitter = 0.3,
                Boxpoints = StyleParam.Boxpoints.All
            )
        Chart.BoxPlot
            (
                x = wholeCell,
                Name = "Whole Cell Extracts",
                Jitter = 0.3,
                Boxpoints = StyleParam.Boxpoints.All
            )
    ]

overallRatioPlot
    |> Chart.Combine
    |> Chart.withLegend false
    |> Chart.withTitle "relative Quantification ratios (rbcL/RBCS2) - Comparison between all experiments"
    |> Chart.withX_Axis (yAxis false "relative quantification (rbcL/rbcS)" 20 16 (0.75 ,1.75))
    |> Chart.withY_Axis (xAxis false "" 20 16)
    |> Chart.withConfig config
    |> Chart.withSize (1000.,700.)
    |> Chart.withMarginSize 300.
    |> Chart.Show

//==================================================================
//================= Absolute Quantification ========================
//==================================================================

let qConcatSequence =
    "MASMTGGQQMGRDPSRSALPSNWKSVLPANWRDTDILAAFREVTLGFVDLMRFLFVAEAIYKLTYYTPDYVVRAYVSNESAIRLVAFDNQKYWTMWKAFPDAYVRVPLILGIWGGKIGQQLVNARSLVDEQENVKLGADSGALEFVPKDDYLNAPGETYSVKTPLANLVYWKALYGFDFLLSSKTNFGIGHRLSIFETGIKTAPAFVDLDTRIPAGPDLIVKNILVVGPVPGKIVAITALSEKYPIYFGGNRVLNTWADIINREWELSFRNTWADIINRLIFQYASFNNSRTALPADWRLVFPEEVLPRNILLNEGIRTWFDDADDWLRAAHHHHHHHKLAAALEHHHHHH"
    |> BioArray.ofAminoAcidString

let n15Amount =
    qConcatSequence
    |> BioSeq.toFormula
    |> initlabelN15Partial 0.998
    |> Formula.averageMass

let qConCatMolecularMass =
    qConcatSequence
    |> BioArray.toMonoisotopicMass
    |> fun x -> x + n15Amount

[<AutoOpen>]
module IsotopicDistribution =
    let ofElement (e: Element) =
            match e with
            | Mono  { X = x; Xcomp = xcomp} -> 
                [(1.,x)]
            | Di    
                {   
                    X       = x; 
                    Xcomp   = xcomp; 
                    X1      = x1; 
                    X1comp  = x1comp
                } -> 
                    [
                        (xcomp,x)
                        (x1comp,x1)
                    ] 

            | Tri   
                {   
                    X       = x; 
                    Xcomp   = xcomp; 
                    X1      = x1; 
                    X1comp  = x1comp
                    X2      = x2; 
                    X2comp  = x2comp
                } -> 
                    [
                        (xcomp,x)
                        (x1comp,x1)
                        (x2comp,x2)
                    ] 
            | Multi 
                {   
                    X       = x; 
                    Xcomp   = xcomp; 
                    X1      = x1; 
                    X1comp  = x1comp
                    X2      = x2; 
                    X2comp  = x2comp
                    XL      = rest
                } -> 
                    [
                        (xcomp,x)
                        (x1comp,x1)
                        (x2comp,x2)
                    ] @ (rest |> Array.map (fun x -> x.NatAbundance, x) |> Array.toList)

//Atomic Mass: 

//corresponds to standard atomic weight if natural abundances are used
//weighted average of 
let getAtomicWeight (e: Element) =
    e
    |> ofElement
    |> List.map (fun (ab,iso) -> ab,iso.Mass)
    |> List.sumBy 
        (fun (abundance,atomicMass) ->
            abundance * atomicMass
        ) 


//corresponds to standard atomic weight if natural abundances are used
//weighted average of 
let getAverageAtomicWeight (f: Formula.Formula) =
    f 
    |> Seq.sumBy 
        (fun elem -> getAtomicWeight elem.Key * elem.Value)

Elements.Table.Se
|> getAtomicWeight

Formula.parseFormulaString "N"
|> Formula.averageMass

Formula.parseFormulaString "N"
|> getAverageAtomicWeight

Formula.parseFormulaString "N"
|> initlabelN15Partial 0.998
|> Formula.monoisoMass

Formula.parseFormulaString "N"
|> initlabelN15Partial 0.998
|> getAverageAtomicWeight


//================================================================================
//============================= 6. Absolute Quantification =======================================
//================================================================================

[<Measure>] type g 
[<Measure>] type mg 
[<Measure>] type ug 
[<Measure>] type ng 
[<Measure>] type pg 
[<Measure>] type fg 

[<Measure>] type mol
[<Measure>] type mmol
[<Measure>] type umol
[<Measure>] type nmol
[<Measure>] type pmol
[<Measure>] type fmol

[<Measure>] type l
[<Measure>] type ml
[<Measure>] type ul
[<Measure>] type nl
[<Measure>] type pl
[<Measure>] type fl

[<Measure>] type cell


let qConCatMW =
    qConcatSequence
    |> BioSeq.toFormula
    |> initlabelN15Partial 0.998
    |> getAverageAtomicWeight 
    |> fun x -> x * 1.<g/mol>


"FLFVAEAIYK"
|> BioArray.ofAminoAcidString
|> BioSeq.toFormula
|> getAverageAtomicWeight


let qConCatGramsToMol (qcg:float<g>) =
    qcg / qConCatMW

let isdCellConcentration =
    7280000.<cell/ml>

let isdCellCount =
    isdCellConcentration * (15.1<ul> / 1000.<ul/ml> )

let isdQMass =
   
        "AddedQProtein[uq]" =>
            series
                [
                    10. => 0.25<ug>
                    5.  => 0.5<ug>
                    1.  => 2.5<ug>
                    0.5 => 5.<ug>
                ]


let isdQMole =
        "AddedQProtein[pmol]" =>
            series
                [
                    10. => ((0.25<ug> * (0.000001<g/ug>)) |> qConCatGramsToMol |> fun x -> (x * 1000000000000.<pmol/mol>))
                    5.  => ((0.5<ug>  * (0.000001<g/ug>)) |> qConCatGramsToMol |> fun x -> (x * 1000000000000.<pmol/mol>))
                    1.  => ((2.5<ug>  * (0.000001<g/ug>)) |> qConCatGramsToMol |> fun x -> (x * 1000000000000.<pmol/mol>))
                    0.5 => ((5.<ug>   * (0.000001<g/ug>)) |> qConCatGramsToMol |> fun x -> (x * 1000000000000.<pmol/mol>))
                ]
    
0.25<ug> * (0.000001<g/ug>) 
|> qConCatGramsToMol 
|> fun x -> (x * 1000000000000.<pmol/mol>) 
|> fun x -> x / isdCellCount

let isdCells =
        "Cell count"=>
            series
                [
                    10. => ( isdCellCount)
                    5.  => ( isdCellCount)
                    1.  => ( isdCellCount)
                    0.5 => ( isdCellCount)
                ]




let wholeCell = 
    rbcl_RatiosS_wholeCell
    |> Frame.merge rbcs_RatiosSdsIgd_wholeCell
    |> Frame.transpose
    |> Frame.addCol 
        "Ratio (rbcL/rbcs)" 
            (
                let rbcl : Series<float,float> = (rbcl_RatiosS_wholeCell |> Frame.transpose |> Frame.getCol "rbcL") 
                let rbcs : Series<float,float> = (rbcs_RatiosSdsIgd_wholeCell |> Frame.transpose |> Frame.getCol "RBCS2")
                rbcl/rbcs
            )

    |> Frame.transpose
    |> Frame.map (fun _ _ v -> Math.round 3 v)



let isd = 
    rbcl_RatiosS_wholeCell
    |> Frame.merge rbcs_RatiosSdsIgd_wholeCell
    |> Frame.transpose
    |> Frame.addCol 
        "Ratio (rbcL/rbcs)" 
            (
                let rbcl : Series<float,float> = (rbcl_RatiosS_wholeCell |> Frame.transpose |> Frame.getCol "rbcL") 
                let rbcs : Series<float,float> = (rbcs_RatiosSdsIgd_wholeCell |> Frame.transpose |> Frame.getCol "RBCS2")
                rbcl/rbcs
            )

    |> Frame.transpose
    |> Frame.map (fun _ _ v -> Math.round 3 v)
    |> Frame.transpose
    |> Frame.addCol (fst isdQMass) (snd isdQMass)
    |> Frame.addCol (fst isdQMole) (snd isdQMole)
    |> fun f ->
        let rbclMol =
            f.["rbcL"] * f.["AddedQProtein[pmol]"]

        let rbclPerCell =
            rbclMol
            |> Series.mapValues (fun v -> (v * 1.<pmol> * 1000.<fmol/pmol>) / isdCellCount)

        let rbcsMol =
            f.["RBCS2"] * f.["AddedQProtein[pmol]"]

        let rbcsPerCell =
            rbcsMol
            |> Series.mapValues (fun v -> (v * 1.<pmol> * 1000.<fmol/pmol>) / isdCellCount)


        f
        |> Frame.addCol (fst isdCells) (snd isdCells)
        |> Frame.addCol "Quantified rbcL [pmol]" rbclMol
        |> Frame.addCol "rbcL per Cell [fmol/cell]" rbclPerCell
        |> Frame.addCol "Quantified RBCS [pmol]" rbcsMol
        |> Frame.addCol "RBCS per Cell [fmol/cell]" rbcsPerCell



isd.SaveCsv(
    path = @"C:\Users\kevin\Downloads\CsbScaffold-master\BioTechVP\results\isdAlResults.txt",
    includeRowKeys = true,
    keyNames = ["Ratio(14N Sample / 15N QProtein)"],
    separator = '\t'
    )
