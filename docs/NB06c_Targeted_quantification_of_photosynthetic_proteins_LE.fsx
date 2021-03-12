(**
# JP12 Targeted quantification of photosynthetic proteins (Label Efficiency)

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=JP12_Targeted_quantification_of_photosynthetic_proteins_LE.ipynb)

1. Label Efficiency of 15N QProteins
2. Read in .txt as Deedle Frame
3. Verify Limit of Detection
    1. Pearson correlation coefficient
4. Calculate Label Efficiency
    1. Median Label Efficiency
5. Midas Results
    1. Midas Results Var 1
    2. Midas Results Var 2
6. Label Efficiency Conclusion Frame
7. References

*)

(**
## Label Efficiency of 15N QProteins

The amount of measured 15N QProtein quantities can, even if you pipette perfectly, vary due to a faulty label efficiency.
The label efficiency is an indicator on how many atoms of the designated type (here 15N) are in fact their stable isotope.
A lower label efficiency will lead to lower quantities of the measured labeled peptides, as they will be detected on other m/z values than the predicted ones.
*)

#r "nuget: FSharp.Stats, 0.4.0"
#r "nuget: BioFSharp, 2.0.0-beta5"
#r "nuget: BioFSharp.IO, 2.0.0-beta5"
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: BIO-BTE-06-L-7_Aux, 0.0.1"
#r "nuget: Deedle, 2.3.0"

#if IPYNB
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: Plotly.NET.Interactive, 2.0.0-beta6"
#endif // IPYNB

open Deedle
open BioFSharp
open FSharpAux
open FSharp.Stats
open Plotly.NET
open FSharp.Stats.Fitting.LinearRegression.OrdinaryLeastSquares.Linear
open System.IO
open BIO_BTE_06_L_7_Aux.FS3_Aux

(**
We will use the same auxiliary functions as in [JP12_WC](JP12_Targeted_quantification_of_photosynthetic_proteins_WC.ipynb).
*)

// Code-Block 1

let colorArray = [|"#E2001A"; "#FB6D26"; "#00519E"; "#00e257";|]

let colorForMean = "#366F8E"

let xAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Zeroline=false,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside, Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))
let yAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside,Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))

let config = Config.init(ShowEditInChartStudio=true, ToImageButtonOptions = ToImageButtonOptions.init(Format = StyleParam.ImageFormat.SVG, Filename = "praktikumsplot.svg"), EditableAnnotations = [AnnotationEditOptions.LegendPosition])

(**
Next, we need a `map` of all proteins present in our QconCat proteins with their corresponding peptides.
*)

// Code block 2

let peptideProtMapping =
    [
    //PS
    "iRT"   =>  "LGGNEQVTR"
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
    "FNR1"  =>  "LYSIASSR"
    "FNR1"  =>  "LDYALSR"
    "D1"    =>  "VLNTWADIINR"
    "D1"    =>  "EWELSFR"
    "D1"    =>  "NTWADIINR"
    "D1"    =>  "LIFQYASFNNSR"
    "LCI5"  =>  "TALPADWR"
    "psbD"  =>  "LVFPEEVLPR"
    "psbD"  =>  "NILLNEGIR"
    "psbD"  =>  "TWFDDADDWLR"
    //CBC
    "PGK"   =>  "ADLNVPLDK"
    "PGK"   =>  "TFNDALADAK"
    "PGK"   =>  "LSELLGKPVTK"
    "Gap3"  =>  "AVSLVLPSLK"
    "Gap3"  =>  "VLITAPAK"
    "FBA3"  =>  "ALQNTVLK"
    "FBA3"  =>  "VMFEGILLK"
    "FBA3"  =>  "SVVSIPHGPSIIAAR"
    "FBP1"  =>  "VPLFIGSK"
    "FBP1"  =>  "TLLYGGIYGYPGDAK"
    "FBP1"  =>  "IYSFNEGNYGLWDDSVK"
    "SBP"   =>  "LTNITGR"
    "SBP"   =>  "LLFEALK"
    "TRK1"  =>  "FLAIDAINK"
    "TRK1"  =>  "VSTLIGYGSPNK"
    "TRK1"  =>  "NPDFFNR"
    "RPE1"  =>  "FIESQVAK"
    "RPE1"  =>  "GVNPWIEVDGGVTPENAYK"
    "RPE1"  =>  "SDIIVSPSILSADFSR"
    "PRK1"  =>  "IYLDISDDIK"
    "PRK1"  =>  "VAELLDFK"
    "PRK1"  =>  "GHSLESIK"
    "TPI1"  =>  "SLFGESNEVVAK"
    "TPI1"  =>  "LVDELNAGTIPR"
    "RPI1"  =>  "LANLPEVK"
    "RPI1"  =>  "LQNIVGVPTSIR"
    "RPI1"  =>  "TQLSQDELK"
    "DP12"  =>  "SGQPAVDLNK"
    "DP12"  =>  "ASGQPAVDLNK"
    "RMT1"  =>  "AEAALLVR"
    "RMT1"  =>  "SNSTPLGSR"
    "FBA1"  =>  "GILASDESNATTGK"
    "FBA1"  =>  "ALQSSTLK"
    "FBA2"  =>  "VSAADVAR"
    "FBA2"  =>  "ALQASVLK"
    "Cre07.g338451" =>  "VTEAAALASGR"
    "FBP1"  =>  "NLALELVR"
    "CalSciex"  =>  "SAEGLDASASLR"
    ]
    |> List.map (fun (x,y) -> y,x)
    |> Map.ofList

peptideProtMapping

(***include-it***)

(**
After we got our peptide &#8594; protein map, we need a `map` for the files we want to analyze. For that we need the filename and a description of what 
the file contains (experiment, spiked in peptide concentration).
This will be used as a schema for the .txt reader later on.
*)

// Code block 3

let labelEfficiencyNameMapping = 
    [
    // filename(from QuantifiedPeptides.txt) => ("LabelEfficiency", ("Descriptive Text", Dilution))
    "G1 q1 zu 0.00032"  =>  ("LabelEfficiency", ("15N Q + 14N Q",           0.00032 ))
    "G1 q1 zu 0.0016"   =>  ("LabelEfficiency", ("15N Q + 14N Q",           0.0016  ))
    "G1 q1 zu 0.008"    =>  ("LabelEfficiency", ("15N Q + 14N Q",           0.008   ))
    "G1 q1 zu 0.04"     =>  ("LabelEfficiency", ("15N Q + 14N Q",           0.04    ))
    "G1 q1 zu 0.2"      =>  ("LabelEfficiency", ("15N Q + 14N Q",           0.2     ))
    "G1 Q1 zu 1zu 1"    =>  ("LabelEfficiency", ("15N Q + 14N Q + 13C Q",   1.      ))
    ]
    |> Map.ofList

labelEfficiencyNameMapping

(***include-it***)

(**
## Read in .txt as Deedle Frame 
In the first step we will just read in the data with the correct data types and assign it with descriptive rowKeys
*)

// Code block 4

let readQConcatResultFrame p : Frame<string*(bool*int),string>=
    // read in .txt as deedle frame
    let schemaFrame =
        Frame.ReadCsv(path = p,separators="\t")
    // when creating deedle frames from files, the program can not always assume the correct type for all entries.
    // Therefore we give it a list of all type defitions for all columns.
    let schema =
        schemaFrame.ColumnKeys
        // only three tables are not float and these are the ones we filter out in the next step
        |> Seq.filter (fun x -> not (x = "StringSequence" || x = "GlobalMod" || x = "Charge"))
        // add float definition to all columns
        |> Seq.map (sprintf "%s=float")
        // specify the previously filtered out columns with the correct type defitions
        |> Seq.append ["StringSequence=string";"GlobalMod=bool";"Charge=int"]
        |> String.concat ","
    // read in the file again, this time with the correct types indicates as by schema
    Frame.ReadCsv(path = p,schema=schema,separators="\t")
    // schemaFrames rows are labeled with increasing numbers, we want to give them unique descriptive keys.
    // In this case we will use their peptideSequence (for example "LVFPEEVLPR"), the global modifier and charge
    |> Frame.indexRowsUsing (fun os -> (os.GetAs<string>("StringSequence"),((os.GetAs<bool>("GlobalMod"),(os.GetAs<int>("Charge"))))))
    // as we now have the information about seq, global mod and charge in the row keys we can drop the origin columns.
    |> Frame.dropCol "StringSequence"
    |> Frame.dropCol "GlobalMod"
    |> Frame.dropCol "Charge"
    |> Frame.sortRowsByKey

let directory = __SOURCE_DIRECTORY__
let path = Path.Combine[|directory;"downloads/Group1/G1_Q1_zu_1zu_1_QuantifiedPeptides.txt"|]
downloadFile path "G1_Q1_zu_1zu_1_QuantifiedPeptides.txt" "bio-bte-06-l-7/Group1"
    
/// DO NOT CHANGE THIS NAME! This value will be used in further code blocks.
/// Insert your path to your QuantifiedPeptides.txt file for your label efficiency (only QProteins) here.
let qConCatResults =
    readQConcatResultFrame path
    
// This part is only meant to show you the current state of your data  
qConCatResults
|> Frame.sliceCols (qConCatResults.ColumnKeys |> Array.ofSeq |> fun x -> x.[0..2])
|> fun x -> x.Print()

qConCatResults

(***include-it***)

(**
Next we apply all our label `map`s to the data and filter to reduce the information to only include necessary data for these functions.
Pay attention to the all caps comment in the code below!
*)

// Code block 5

let labelEfficiencyResults : Frame<string*(string*(string*float)),(string*(string*int))> = 
    qConCatResults
    // first use our "labelEfficiencyNameMapping" to correctly label all columns
    |> Frame.mapColKeys 
        (fun (ck:string) -> 
            let newCK = Map.find (ck.Split('_').[1 ..] |> String.concat "_") labelEfficiencyNameMapping
            ck.Split('_').[0] , newCK
        )
    |> Frame.sortColsByKey
    // filter out all extra information noted in the file, but not needed for now.
    |> Frame.filterCols (fun ck _ -> (fst ck).Contains("Quant") || (fst ck).Contains("N15MZ") || (fst ck).Contains("N15Minus1MZ"))
    // get mean values for all peptides, for which we found different charged versions.
    |> Frame.applyLevel (fun (sequence,(gmod,charge)) -> sequence,charge) Stats.mean
    // swap rows and columns
    |> Frame.transpose
    // map over "peptideProtMapping" to assign a related protein to all peptide sequences.
    |> Frame.mapColKeys
        (fun ck ->
            match Map.tryFind (fst ck) peptideProtMapping with
            |Some prot  -> prot,ck
            |None       -> "NotFound",ck
        )
    // THE FOLLOWING PART IS ONLY MEANT TO EASE ACCESS TO THESE FUNCTIONS AS FILTERING FOR ONLY 
    // RBCL GREATLY REDUCES THE COMPUTATION TIME. PLEASE REMOVE THIS FUNCTION AFTER YOU FAMILIARIZED 
    // YOURSELF WITH THE CODE AND OUTPUT TO ANALYZE ALL PROTEINS 
    |> Frame.filterCols (fun ck cs -> fst ck = "rbcL")
    
// This part is only meant to show you the current state of your data  
labelEfficiencyResults
|> Frame.sliceCols (labelEfficiencyResults.ColumnKeys |> Array.ofSeq |> fun x -> x.[0..1])
//|> Frame.filterCols (fun ck cs -> fst ck = "rbcL" && ((snd >> fst) ck = "FLFVAEAIYK" || (snd >> fst) ck = "EVTLGFVDLMR"))
|> fun x -> x.Print()

labelEfficiencyResults

(***include-it***)

(**
In the following we define helper functions and record types to increase readability/accessability for our code.
*)

// Code block 6

open Isotopes
open Elements

let initlabelN15Partial n15Prob =
    ///Diisotopic representation of nitrogen with abundancy of N14 and N15 swapped
    let n14Prob = 1. - n15Prob
    let N15 = Di (createDi "N15" (Isotopes.Table.N15,n15Prob) (Isotopes.Table.N14,n14Prob) )
    fun f -> Formula.replaceElement f Elements.Table.N N15

//let labelFullN15 =
//    let N15 = Elements.Table.Heavy.N15
//    fun f -> Formula.replaceElement f Elements.Table.N N15

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

type LabelEffCollector = 
    {
        Dilution: float
        N15Minus1Quant: float
        N15Quant:float
        N15Minus1MOverZ: float
        N15MOverZ: float
    }
    
    static member create n15Minus1quant n15quant n15Minus1MOverZ n15MOverZ dilution = {
        Dilution            = dilution
        N15Minus1Quant      = n15Minus1quant
        N15Quant            = n15quant
        N15Minus1MOverZ     = n15Minus1MOverZ
        N15MOverZ           = n15MOverZ
    }
    
type LabelEffCollectorLinearity = 
    {
        Dilution        : float []
        N14ToN15Quant   : float []
        Protein         : string
        Peptide         : string
        Charge          : int
        PCoEff          : Vector<float> option
        PFitVals        : float [] option
        PDetermination  : float option
    }
    
    static member create dil n14n15 prot pept charge coeff fitvals deter =
        {
            Dilution        = dil
            N14ToN15Quant   = n14n15
            Protein         = prot
            Peptide         = pept 
            Charge          = charge
            PCoEff          = coeff
            PFitVals        = fitvals
            PDetermination  = deter
        }

(**
## Verify Limit of Detection
`getLabelData` filters the data for the given dilutions and returns relative quantifications for each peptide. 
Additionally, it also tells us the corresponding protein and the charge of the peptide.
*)

// Code block 7

let dilutionArr = [|0.00032;0.0016;0.008;0.04;0.2;1.|]

let getLabelData dilutionArr= 
    labelEfficiencyResults
    // drop all columns with missing values
    |> Frame.dropSparseCols
    // filter for dilutions of interest
    |> Frame.filterRows (fun (prot,(_,(fileName,dilution))) rs -> Array.exists (fun x -> x = dilution) dilutionArr)
    // break frame format and start working on arrays
    |> fun x -> 
        x
        |> Frame.toArray2D
        |> Array2D.toJaggedArray
        |> JaggedArray.transpose
        |> Array.zip (x.ColumnKeys |> Array.ofSeq)
        //Calculate fully/uncompletely labeled peak ratio
        |> Array.map 
            (fun ((prot,(pepSeq,charge)),values) -> 
                // we use multiple dilutions of 15N and we can calculate a label efficiency for each of them and
                // take the mean in the end, as all originate from the same QProtein sample. This could also
                // show differences in measured label efficiency for high or low sample quantities.
                let quant, dil =
                    Array.init
                        dilutionArr.Length
                        (fun ind ->
                            [|for num in ind .. dilutionArr.Length .. values.Length-1 do
                                yield values.[num]|]
                        )
                    |> Array.mapi (fun i values -> 
                        // 14N/15N, dilution
                        values.[0]/values.[4], 1./dilutionArr.[i]
                    )
                    |> Array.sortBy snd
                    |> Array.unzip
                LabelEffCollectorLinearity.create dil quant prot pepSeq charge None None None
            )
    
/// DO NOT CHANGE THIS NAME! This value will be used in further code blocks.
/// Insert a dilution array with the dilutions you want to include
let labelData = getLabelData dilutionArr

// This part is only meant to show you the current state of your data  
labelData

(***include-it***)

(**
### Pearson correlation coefficient
Testing the limit of detection can be done with a dilution array and the [pearson coefficient](https://en.wikipedia.org/wiki/Pearson_correlation_coefficient).
 As you should assume a rather low pipetting error you can check the mass spectrometry detection for all 
 QProtein peptides with this experiment. If for any peptide the pearson coefficient is not high (so the relation would be non-linear) 
 the mass spectrometry was not able to detect the small quantities for some of the given dilutions and therefore underestimated the real amount of QProtein.
*)

// Code block 8

// Use the "labelData" to calculate the pearson coefficient for the measured data points for all dilutions for each peptide
let lEInfo =
    labelData
    |> Array.map (fun peptVal ->
        peptVal.N14ToN15Quant
        |> fun strainVals ->
            // RBCL Regression of relative quantification values
            let RBCLcoeff = Univariable.coefficient (vector peptVal.Dilution) (vector strainVals)
            let RBCLfitFunc = Univariable.fit RBCLcoeff
            let RBCLfitVals = peptVal.Dilution |> Array.map RBCLfitFunc
            let RBCLdetermination = FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue strainVals RBCLfitVals
            let RBCLpearson = FSharp.Stats.Correlation.Seq.pearson strainVals peptVal.Dilution
            printfn "Pearson WholeCell [%s]-%s @ z=%i: %f" peptVal.Protein peptVal.Peptide peptVal.Charge RBCLpearson
            {peptVal with 
                PCoEff = Some RBCLcoeff; 
                PFitVals = Some RBCLfitVals; 
                PDetermination = Some RBCLdetermination}
    )

lEInfo

(***include-it***)

(**
Those graphs shows the linearity of the peptides for our protein by displaying the single data points and their linear fit.
*)

// Code block 9

let showLinearity  =

    lEInfo
    |> Array.groupBy (fun x -> x.Protein)
    |> Array.map (fun (_,valArr) ->
        valArr
        |> Array.map (fun peptVal ->
            [
                Chart.Point (Array.zip peptVal.Dilution peptVal.N14ToN15Quant,Name = sprintf "[%s]-%s @z=%i Quantified Ratios" peptVal.Protein peptVal.Peptide peptVal.Charge)
                |> Chart.withMarkerStyle(Size=10,Symbol = StyleParam.Symbol.Cross)
                Chart.Line(Array.zip peptVal.Dilution peptVal.PFitVals.Value,Name = (sprintf "%s linear regression: %.2f x + (%2f);<br> R<sup>2</sup> = %.4f" peptVal.Peptide peptVal.PCoEff.Value.[1] peptVal.PCoEff.Value.[0] peptVal.PDetermination.Value))
                |> Chart.withLineStyle(Color="lightblue",Dash=StyleParam.DrawingStyle.DashDot)
            ] 
            |> Chart.Combine
        )
        |> Chart.Combine
        |> Chart.withTitle ((Array.head valArr).Protein + " N15 QProtein Dilution")
        |> Chart.withY_Axis (yAxis false "N14 / N15 Peak instensity ratio" 20 16)
        |> Chart.withX_Axis (xAxis false "Dilution of N15 Q Protein" 20 16)
        |> Chart.withConfig config
        |> Chart.withMargin (Margin.init(Right=400))
        |> Chart.withSize (900.,600.)
    )
    
showLinearity
|> Array.head

(***hide***)
showLinearity |> Array.head |> GenericChart.toChartHTML
(***include-it-raw***)

(**
## Calculate Label Efficiency
`prepareLabelEfficiencyResults` extracts all the information needed for label efficiency determination from our read-in mass 
spectrometry results for the given dilutions.
*)

// Code block 10

let prepareLabelEfficiencyResults (dilutionArr:float []) =
    labelEfficiencyResults
    |> Frame.filterRows 
        (fun (rk,_) _ -> not (rk.Contains("N14")))
    |> Frame.dropSparseCols
    |> fun x -> 
        x
        |> Frame.toArray2D
        |> Array2D.toJaggedArray
        |> JaggedArray.transpose
        |> Array.zip (x.ColumnKeys |> Array.ofSeq)
        //Calculate fully/uncompletely labeled peak ratio
        //|> Array.find (fun x -> (fst >> fst) x = "rbcL")
        |> Array.map 
        //|>
            (fun (key,values) -> 
                key,
                // we use multiple dilutions of N15 and we can calculate a label efficiency for each of them and
                // take the mean in the end, as all originate from the same QProtein sample. This could also
                // show differences in measured label efficiency for high or low sample quantities.
                Array.init
                    dilutionArr.Length
                    (fun ind ->
                        [|for num in ind .. dilutionArr.Length .. values.Length-1 do
                            yield values.[num]|]
                    )
                |> Array.mapi (fun i values -> 
                    LabelEffCollector.create values.[1] values.[3] values.[0] values.[2] dilutionArr.[i]
                )
            )
            
let preparedLabelEfficiencyResults =
    prepareLabelEfficiencyResults dilutionArr

preparedLabelEfficiencyResults
|> Array.head

(***include-it***)

(**
Here MIDAS calculates a label efficiency with corresponding isotopic distribution for our peptides.
*)

// Code block 11

// This function may take several minutes with up to an hour depending on the number of peptides and dilutions of interest
// Here, theoretical isotopic spectra are created and compared with the measured spectra.
let labelEfficiency =
    preparedLabelEfficiencyResults
    |> Array.collect 
        (fun ((prot,(peptideSequence,charge)),labelEffCollectorArr) ->

            let calculateLabelEffs (labelEffCollect:LabelEffCollector) =
            
                // ratio between the N15 peak and the N15-1 peak
                let peakRatio = labelEffCollect.N15Minus1Quant / labelEffCollect.N15Quant
                //printfn "peakRatio: %A" peakRatio
                
                // get chemical information about petide sequence
                let peptide =
                    peptideSequence
                    |> BioArray.ofAminoAcidString
                    |> BioSeq.toFormula
                //printfn "peptide: %A" peptide

                // create theoretical isotopic distributions for label efficiency 0.5 .. 0.001 .. 0.999
                let theoreticalIsotopicDistributions =
                    [for le in 0.5 .. 0.001 .. 0.999 do
                         yield
                             le,
                             peptide
                             |> initlabelN15Partial le
                             |> Formula.add Formula.Table.H2O
                             |> generateIsotopicDistributionOfFormulaByMax charge
                    ]

                
                let theoreticalRatios =
                    theoreticalIsotopicDistributions
                    // get all theoretical probabilities for the N15 and N15-1 peak
                    |> List.map 
                        (fun (le,dist) ->
                            let n15Prob = 
                                dist
                                // get the value from the list, for which the theoretical mz is closest to the
                                // measured m/z
                                |> List.minBy 
                                    (fun (mz,prob) ->
                                        abs (labelEffCollect.N15MOverZ - mz)
                                    )
                                // get probabilitie for the closest theoretical peak
                                |> snd
                            
                            let n15Minus1Prob = 
                                dist
                                // get the value from the list, for which the theoretical mz is closest to the
                                // measured m/z
                                |> List.minBy 
                                    (fun (mz,prob) ->
                                        //old: abs (n15Minus1MZ - mz)
                                        abs (labelEffCollect.N15Minus1MOverZ - mz)
                                    )
                                // get probabilitie for the closest theoretical peak
                                |> snd
                            // return le and dist and the theoretical probability (n15Minus1Prob / n15Prob)
                            le,(n15Minus1Prob / n15Prob), dist
                        )

                // get the theoretical le,ratio,dist that features the lowest difference in N15-1/N15 
                // peak ratio between the theoretical and the measured distributions
                let bestLE = 
                    theoreticalRatios
                    |> List.minBy
                        (fun (le,ratio,dist) ->
                            abs (peakRatio - ratio)
                        )
                // header information about protein, peptide, charge
                (prot,(peptideSequence,charge)),
                // LabelEffCollector Record type; saved information from measured data
                labelEffCollect,
                // found best fitting theoretical le, ratio and isotopic distribution
                bestLE
            labelEffCollectorArr
            |> Array.map calculateLabelEffs
        )

labelEfficiency |> Array.head

(***include-it***)

(**
### Median Label Efficiency
In the following, we will determine outlier borders for our calculated label efficiencies. 
We will do this via [tukey outlier calculation](https://en.wikipedia.org/wiki/Outlier).
*)

// Code block 12

// Drop all information except protein, peptide, dilution and le.
let allPredictedLE =
    labelEfficiency
    |> Array.map 
        (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) ->
            (prot,peptideSequence,experimentalDist.Dilution),le
        )
        
// calculate outlier borders with tukey (n=3)
let outlierBorders = FSharp.Stats.Testing.Outliers.tukey 3. (allPredictedLE |> Array.map snd)

allPredictedLE

(***include-it***)

outlierBorders

(***include-it***)

// Code block 13

//Boxplot with outlier borders
let showLEsBoxPlot =
    allPredictedLE
    |> Array.unzip
    |> fun (header,leValues) -> 
        [
            Chart.BoxPlot(
                x = (
                    allPredictedLE
                    |> Array.filter (fun (keys,eff) -> eff < outlierBorders.Upper && eff > outlierBorders.Lower)
                    |> Array.map snd
                ),
                Jitter = 0.3,
                Boxpoints=StyleParam.Boxpoints.All,
                Name="Measured Label <br> Efficiencies"
                )
            Chart.BoxPlot(x=leValues,Jitter = 0.3,Boxpoints=StyleParam.Boxpoints.All,Name="Measured Label <br> Efficiencies - <br> without outliers")
            |> Chart.withShapes 
                [
                    (Shape.init(StyleParam.ShapeType.Line, X0 = outlierBorders.Upper, X1 = outlierBorders.Upper, Y0 = -0.4, Y1 = 1.4))
                    (Shape.init(StyleParam.ShapeType.Line, X0 = outlierBorders.Lower, X1 = outlierBorders.Lower, Y0 = -0.4, Y1 = 1.4))
                ]
        ]
    |> Chart.Combine
    |> Chart.withX_Axis (yAxis false "N15 / N15 - 1 m/z Peak instensity ratio" 20 16)
    |> Chart.withY_Axis (xAxis false "" 20 16)
    |> Chart.withMargin (Margin.init(Left=200))
    |> Chart.withTitle "Label efficiency - Outlier detection"
    // adjust chart size
    |> Chart.withSize (1200.,600.)
    |> Chart.withConfig config

// You can use this function to find peptides of a specific LE of interest, which you found on the box blot.
let findPeptideOfLE le =
    let foundVals =
        allPredictedLE
        |> Array.filter (fun x -> snd x = le)
    if foundVals |> Array.isEmpty then failwithf "We could not find any label efficiency at the given value: %.3f" le
    else foundVals

findPeptideOfLE 0.972 

(***include-it***)

///// Use this function for an added description below the chart
//showLEs
//|> Chart.ShowWithDescription
//    {Heading = "Borders From tukey outlier detection with k = 3:"; Text = sprintf "Upper: %.3f <br></br>Lower: %.3f" outlierBorders.Upper outlierBorders.Lower}

showLEsBoxPlot

(***hide***)
showLEsBoxPlot |> GenericChart.toChartHTML
(***include-it-raw***)

// Code block 14

// Calculate the median label efficiency for all LE inside the borders given by the tukey outlier test
let filteredOverallPredictedLabelEfficiency =
    allPredictedLE
    |> Array.filter (fun (keys,eff) -> eff < outlierBorders.Upper && eff > outlierBorders.Lower)
    |> Array.map snd
    |> Seq.median 

filteredOverallPredictedLabelEfficiency

(***include-it***)

(**
## Midas Results
From here on you will find functions for charts displaying midas results on a single peptide basis. 
The two available options differ in complexity and it is not necessary to understand 100% of the code used. 
But it can be useful to change parameters for the chart creation and adjust them for your needs.
*)
(**
### Midas Results Var 1
This Variant shows basic information to the midas label efficiency calculation for all peptides of any protein.
*)

// Code block 15

let showLabelEfficiencyChartsSimple =
    labelEfficiency
    |> Array.groupBy (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) -> prot,peptideSequence,charge)
    |> Array.map (
        fun ((prot,peptideSequence,charge),arr) ->
            prot,
            peptideSequence,
            arr
            |> Array.sortByDescending (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) -> experimentalDist.Dilution)
            |> Array.map (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) ->
                let normExperimental =
                    experimentalDist
                    |> fun x ->
                        List.zip
                            [x.N15Minus1MOverZ;x.N15MOverZ]
                            (
                                [x.N15Minus1Quant; x.N15Quant]
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
                    Chart.Point(normExperimental,Name= sprintf "[%s] %s : Experimental Masses 1 to %s"  prot peptideSequence (string experimentalDist.Dilution))
                ]
                |> Chart.Combine
                |> Chart.withTitle (sprintf "[%s] : %s @ + %i" prot peptideSequence charge )
                |> Chart.withX_Axis (xAxis false "" 20 16)
                |> Chart.withY_Axis (yAxis false "" 20 16)
                |> Chart.withConfig config 
            )
        )
        |> Array.map ( fun (prot,pepSeq,charts) ->
            charts
            |> Chart.Stack charts.Length
            |> Chart.withSize (1500.,600.)
        )
        
showLabelEfficiencyChartsSimple
|> Array.head 

(***hide***)
showLabelEfficiencyChartsSimple |> Array.head |> GenericChart.toChartHTML
(***include-it-raw***)

(**
### Midas Results Var 2
The following code is used to generate in depth charts with most information about the midas calculation.
If variant 1 does not contain enough information, this variant can be used instead.
*)

// Code block 16

type LabelEfficiencyPredictor =
    {
        Protein: string
        PeptideSequence: string
        Charge: int
        PredictedDistribution: (float*float) list
        PredictedLabelEfficiency: float
        ExperimentalDistribution: (float*float) list
        EFCollector: LabelEffCollector
    }

let createLabelEfficiencyPredictor p ps c pred eff exp efCollector=
    {
        Protein                     = p
        PeptideSequence             = ps
        Charge                      = c
        PredictedDistribution       = pred
        PredictedLabelEfficiency    = eff
        ExperimentalDistribution    = exp
        EFCollector                 = efCollector
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
        EFCollector             : LabelEffCollector
    }

let createLabelEfficiencyResult p ps c predLE predP aP mLE mP flP cf efCollector=
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
        EFCollector                 = efCollector
    }

// Code block 17

let labelEfficiencyResultsFinalPre =
    labelEfficiency
    |> Array.map 
        (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) ->
            let prepExperimentalDist =
                [
                    experimentalDist.N15Minus1MOverZ, experimentalDist.N15Minus1Quant;
                    experimentalDist.N15MOverZ, experimentalDist.N15Quant
                ]
            createLabelEfficiencyPredictor prot peptideSequence charge dist le prepExperimentalDist experimentalDist
        )
labelEfficiencyResultsFinalPre
|> Array.head

(***include-it***)

// Code block 18

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
               lePredictor.EFCollector
       )
       
let labelEfficiencyResultsFinal =
    labelEfficiencyResultsFinalPre
    |> getCorrectionFactors filteredOverallPredictedLabelEfficiency
    
labelEfficiencyResultsFinal
|> Array.head

(***include-it***)

// Code block 19

let plotLabelEfficiencyResult (leRes: LabelEfficiencyResult) =
    [
        leRes.FullLabeledPattern
        |> List.map (fun (x,y) -> [(x,0.);(x,y);(x,0.)])
        |> List.concat
        |> Chart.Line
        |> Chart.withTraceName (sprintf "Dil=%s; Fully Labeled Pattern" (string leRes.EFCollector.Dilution))
        |> Chart.withLineStyle(Color="lightgray",Width = 20)

        leRes.MedianPattern
        |> List.map (fun (x,y) -> [(x,0.);(x,y);(x,0.)])
        |> List.concat
        |> Chart.Line
        |> Chart.withTraceName (sprintf "Dil=%s; CorrectedPattern @ Median LE of %.3f" (string leRes.EFCollector.Dilution) leRes.MedianLabelEfficiency)
        |> Chart.withLineStyle(Width = 10,Color="lightgreen")

        leRes.PredictedPattern
        |> List.map (fun (x,y) -> [(x,0.);(x,y);(x,0.)])
        |> List.concat
        |> Chart.Line
        |> Chart.withTraceName (sprintf "Dil=%s; PredictedPattern @ %.3f LE" (string leRes.EFCollector.Dilution) leRes.PredictedLabelEfficiency)
        |> Chart.withLineStyle(Color="orange",Width = 5)

        Chart.Point(leRes.ActualPattern,Name=sprintf "Dil=%s; Experimental Values" (string leRes.EFCollector.Dilution))
        |> Chart.withMarkerStyle(Size = 15,Symbol = StyleParam.Symbol.X, Color = "lightred")

    ]
    |> Chart.Combine
    |> Chart.withX_Axis 
        (xAxis false (sprintf "m/z for Dilution = %s" (string leRes.EFCollector.Dilution)) 20 16 )
    |> Chart.withY_Axis (yAxis false "normalized probability" 20 16)
    |> Chart.withConfig config

let showLabelEfficiencyResults =
    labelEfficiencyResultsFinal
    |> Array.groupBy (fun x -> x.Protein, x.PeptideSequence, x.Charge)
    |> Array.map (fun (header,pepSortedVals) ->
        let header = Array.head pepSortedVals
        pepSortedVals
        |> Array.sortByDescending (fun x -> x.EFCollector.Dilution)
        |> Array.map plotLabelEfficiencyResult
        |> Chart.Stack ((pepSortedVals.Length/2),Space=0.1)
        |> Chart.withTitle (sprintf "[%s] : %s @ z = %i" header.Protein header.PeptideSequence header.Charge)
        |> Chart.withSize (1400.,1000.)
    )
    
showLabelEfficiencyResults
|> Array.head

(***hide***)
showLabelEfficiencyResults |> Array.head |> GenericChart.toChartHTML
(***include-it-raw***)

// Code block 20

let labelEfficiencyFrame =
    labelEfficiencyResultsFinal
    |> Array.map 
        (fun leRes ->
            (leRes.Protein,(leRes.PeptideSequence, leRes.Charge, leRes.EFCollector.Dilution)) => 
                series 
                    [
                        "PredictedLabelEfficiency"      => leRes.PredictedLabelEfficiency
                        "MedianLabelEfficiency"         => leRes.MedianLabelEfficiency
                    ]
        )
    |> frame
    |> Frame.transpose
    
labelEfficiencyFrame

(***include-it***)
