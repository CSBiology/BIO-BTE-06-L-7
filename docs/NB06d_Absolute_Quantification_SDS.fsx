#r "nuget: FSharp.Stats, 0.4.0"
#r "nuget: BioFSharp, 2.0.0-beta5"
#r "nuget: BioFSharp.IO, 2.0.0-beta5"
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: BIO-BTE-06-L-7_Aux, 0.0.6"
#r "nuget: Deedle, 2.3.0"
#r "nuget: ISADotNet, 0.2.4"
#r "nuget: ISADotNet.XLSX, 0.2.4"

#if IPYNB
#r "nuget: Plotly.NET, 2.0.0-beta7"
#r "nuget: Plotly.NET.Interactive, 2.0.0-beta7"
#endif // IPYNB

open System.IO
open ISADotNet
open ISADotNet.API
open Deedle
open BioFSharp
open FSharpAux
open FSharp.Stats
open Plotly.NET
open FSharp.Stats.Fitting.LinearRegression.OrdinaryLeastSquares.Linear
open System.IO
open BIO_BTE_06_L_7_Aux.FS3_Aux
open BIO_BTE_06_L_7_Aux.Deedle_Aux

(**
# NB06d Absolute Quantification

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=NB06d_Absolute_Quantification_SDS.ipynb)

[Download Notebook](https://github.com/CSBiology/BIO-BTE-06-L-7/releases/download/NB06b_NB06b_NB06c_NB06c_NB06d_NB06d/NB06d_Absolute_Quantification_SDS.ipynb)

Finally, after careful peptide ion selection, quality control and assuring that our label efficiency allows accurate for quantifications, we can start to
calculate protein abundancies. Since we start again by getting access to our data and its description, this notebook will start off familiar!

## I. Reading the sample description

As always: before we analyze our data, we will download and read the sample description provided by the experimentalist.
*)
let directory = __SOURCE_DIRECTORY__
let path2 = Path.Combine[|directory;"downloads/alle_Gruppen_V11_SWATE.xlsx"|]
downloadFile path2 "alle_Gruppen_V11_SWATE.xlsx" "bio-bte-06-l-7"

let _,_,_,myAssayFile = XLSX.AssayFile.AssayFile.fromFile path2
let inOutMap = BIO_BTE_06_L_7_Aux.ISA_Aux.createInOutMap myAssayFile

(**
Next, we will prepare functions to look up parameters which might be needed for further calculations.
If you compare this list to the one of note book NB06b you will find additional functions. We will need these functions
in order to calculate the absolute abundances. 
*)

type CutoutBand =
    | RbcL
    | RbcS

let normalizeFileName (f:string) = if Path.HasExtension f then f else Path.ChangeExtension(f, "wiff")

//
let getStrain (fileName:string) =
    let fN = fileName |> normalizeFileName
    BIO_BTE_06_L_7_Aux.ISA_Aux.tryGetCharacteristic inOutMap "Cultivation -Sample preparation" "strain" fN myAssayFile
    |> Option.defaultValue ""

//
let getExpressionLevel (fileName:string) =
    let fN = fileName |> normalizeFileName 
    BIO_BTE_06_L_7_Aux.ISA_Aux.tryGetCharacteristic inOutMap "Cultivation -Sample preparation" "gene expression" fN myAssayFile 
    |> Option.defaultValue "Wt-Like"

// 
let getμgChlorophPerMlCult (fileName:string) =
    let fN = fileName |> normalizeFileName
    BIO_BTE_06_L_7_Aux.ISA_Aux.tryGetCharacteristic inOutMap "Cultivation -Sample preparation" "concentration #5" fN myAssayFile |> Option.defaultValue ""
    |> float 
    |> (*) 1000.

// 
let getCellCountPerMlCult (fileName:string) =
    let fN = fileName |> normalizeFileName
    BIO_BTE_06_L_7_Aux.ISA_Aux.tryGetParameter inOutMap "Cultivation -Sample preparation" "concentration" fN myAssayFile |> Option.defaultValue ""
    |> float 

// 
let getμgChlorophPerμlSample (fileName:string) =
    let fN = fileName |> normalizeFileName
    BIO_BTE_06_L_7_Aux.ISA_Aux.tryGetCharacteristic inOutMap "Cultivation -Sample preparation" "concentration #2" fN myAssayFile |> Option.defaultValue ""
    |> float 

// 
let getμgProtPerμlSample (fileName:string) =
    let fN = fileName |> normalizeFileName
    BIO_BTE_06_L_7_Aux.ISA_Aux.tryGetCharacteristic inOutMap "Cultivation -Sample preparation" "concentration #3" fN myAssayFile |> Option.defaultValue ""
    |> float 

//
let get15N_PS_Amount (fileName:string) =
    let fN = fileName |> normalizeFileName
    BIO_BTE_06_L_7_Aux.ISA_Aux.tryGetCharacteristic inOutMap "Extraction" "gram #2" fN myAssayFile |> Option.defaultValue ""
    |> String.split ' '
    |> Array.head
    |> float 
//
let getGroupID (fileName:string) =
    let fN = fileName |> normalizeFileName
    BIO_BTE_06_L_7_Aux.ISA_Aux.tryGetParameter inOutMap "Extraction" "Group name" fN myAssayFile |> Option.defaultValue ""
    |> int    

let getLoadAmount (fileName : string) =
    let fN = fileName |> normalizeFileName
    BIO_BTE_06_L_7_Aux.ISA_Aux.tryGetParameter inOutMap "PAGE - Sample preparation" "soluble protein content" fN myAssayFile |> Option.defaultValue ""
    |> float


let getCutoutBand (fileName : string) =
    let fN = fileName |> normalizeFileName
    BIO_BTE_06_L_7_Aux.ISA_Aux.tryGetParameter inOutMap "PAGE - Sample preparation" "Cutout band" fN myAssayFile |> Option.defaultValue ""
    |> fun str ->
        match str with
        | "rbcL" -> RbcL
        | "rbcS" -> RbcS
        | _ -> failwith (sprintf "rbcL or rbcS not cut out in file %s" fN)

(**
A quick execution to test the retrieval of data from the isa sample table:
*)

getStrain "Gr2rbcL2_5.wiff"
getExpressionLevel "Gr2rbcL2_5.wiff"
getμgChlorophPerMlCult "Gr2rbcL2_5.wiff"
getCellCountPerMlCult "Gr2rbcL2_5.wiff"
getμgChlorophPerμlSample "Gr2rbcL2_5.wiff"
getμgProtPerμlSample "Gr2rbcL2_5.wiff"
get15N_PS_Amount "Gr2rbcL2_5.wiff"
getGroupID "Gr2rbcL2_5.wiff"
getLoadAmount "Gr2rbcL2_5.wiff"

(**
## II. Reading the data
As promised, we start this notebook with the output of the previous analysis, this notebook assumes that the data from *NB06b Data Access and Quality Control* is stored in a .txt
*)


type PeptideIon = 
    {|
        ProteinGroup    : string  
        Synonyms        : string
        StringSequence  : string
        PepSequenceID   : int
        Charge          : int
    |}

//This is the filepath you chose in *NB06b Data Access and Quality Control*
// let filePath = @"C:\yourPath\testOut.txt"
let filePath = System.IO.Path.Combine [|__SOURCE_DIRECTORY__ + "/downloads/qualityControlResult_SDS.txt"|]

let qConcatDataFiltered =
    Frame.ReadCsv(path = filePath, separators = "\t")
    // StringSequence is the peptide sequence
    |> Frame.indexRowsUsing (fun os -> 
        {|
            ProteinGroup    = os.GetAs<string>("ProteinGroup"); 
            Synonyms        = os.GetAs<string>("Synonyms")
            StringSequence  = os.GetAs<string>("StringSequence");
            PepSequenceID   = os.GetAs<int>("PepSequenceID");
            Charge          = os.GetAs<int>("Charge");
        |}
    )
    |> Frame.filterRows (fun k s -> String.contains "QProt_newPS" k.ProteinGroup)

(**
## III. From Ratios to mol proteins per cell.

Now we can use the extensive information stored in the sample sheet and map each quantified peptide ion passing
the quality checks to an estimator for protein abundance! First we start off by defining a function to extract ratios:
*)

let sliceQuantColumns quantColID frame = 
    frame
    |> Frame.filterCols (fun ck os -> ck |> String.contains ("." + quantColID))
    |> Frame.mapColKeys (fun ck -> ck.Split('.') |> Array.item 0)


(**
Next up, we have to define a function, which maps the measured ratio and measured parameters to an quantification value.
*)

/// 
let calcAbsoluteAbundance μgChlorophPerMlCult cellCountPerMlCult μgChlorophPerμlSample μgProtPerμlSample μgQProtSpike μgloadedProtein molWeightQProt molWeightTargetProt ratio1415N =
    let chlorophPerCell : float = μgChlorophPerMlCult / cellCountPerMlCult
    let cellsPerμlSample = μgChlorophPerμlSample / chlorophPerCell
    let μgProteinPerCell = μgProtPerμlSample / cellsPerμlSample
    let molQProtSpike = μgQProtSpike * 10. ** -6. / molWeightQProt
    let molProtPerBand = ratio1415N * molQProtSpike
    let molProtIn1μgLoadedProt = molProtPerBand / μgloadedProtein
    let gTargetProtIn1μgLoadedProt = molWeightTargetProt * molProtIn1μgLoadedProt
    let molProteinPerCell = molProtIn1μgLoadedProt * μgProteinPerCell
    let proteinsPerCell = molProteinPerCell * 6.022 * 10. ** 23.
    let attoMolProteinPerCell = molProteinPerCell * (10. ** 18.)
    {|
        MassTargetProteinInLoadedProtein    = gTargetProtIn1μgLoadedProt
        ProteinsPerCell                     = proteinsPerCell
        AttoMolProteinPerCell               = attoMolProteinPerCell
    |}

(** 
Inspecting the input parameters of 'calcAbsoluteAbundance' we can see that we need both, the molcular weight of the qProtein and of the 
native Protein. Since we have none at hand we will use our newly aquired skills to compute both and add them to the row key of our Frame. 
*)

let path = Path.Combine[|__SOURCE_DIRECTORY__;"downloads/Chlamy_JGI5_5(Cp_Mp)_QProt.fasta"|]
downloadFile path "Chlamy_JGI5_5(Cp_Mp)_QProt.fasta" "bio-bte-06-l-7"

let examplePeptides = 
    path
    |> IO.FastA.fromFile BioArray.ofAminoAcidString
    |> Array.ofSeq

(** 
First we find the sequences of the qProteins, calculate their masses and define a function to retrieve the calculated mass.
*)

let PS = 
    examplePeptides 
    |> Seq.find (fun prot -> prot.Header |> String.contains "QProt_newPS")

let PSMass = 
    BioFSharp.BioSeq.toMonoisotopicMassWith (Formula.monoisoMass Formula.Table.H2O) PS.Sequence


(** 
Then we repeat the process and assign the calculated masses to each protein.
*)

let withProteinWeights = 
    qConcatDataFiltered
    /// For each row (peptide) in the frame...
    |> Frame.mapRowKeys (fun k -> 
        let proteinsOfInterest = 
            k.ProteinGroup 
            |> String.split ';' 
            |> Array.filter (fun x -> x.Contains "Cre")
        let masses = 
            proteinsOfInterest
            /// ...we look up the matching protein sequence
            |> Seq.choose (fun creID -> 
                examplePeptides 
                |> Seq.tryFind (fun prot -> prot.Header |> String.contains creID)
                )
            /// ... and calculate the protein masses        
            |> Seq.map (fun prot -> 
                BioFSharp.BioSeq.toMonoisotopicMassWith (Formula.monoisoMass Formula.Table.H2O) prot.Sequence 
                )
        let avgMass = if Seq.isEmpty masses then 0. else masses |> Seq.average
        /// ... and add the average to the peptide.   
        {|k with AverageProtGroupMass = avgMass|}
    )

(** 
With our newest update to our meta data (adding the masses to the rowkey), we can slice out the columns
needed to calculate absolute abundances: the ratio columns.
*)

let ratios = sliceQuantColumns "Ratio" withProteinWeights

(** 
Finally, we can iterate the ratios and map each to a protein abundance using our well annotated experiment.
*)

//
let absoluteAbundances = 
    ratios
    |> Frame.map (fun peptide fileName ratio -> 
        try 
            let μgChlorophPerMlCult     = getμgChlorophPerMlCult fileName
            let cellCountPerMlCult      = getCellCountPerMlCult fileName
            let μgChlorophPerμlSample   = getμgChlorophPerμlSample fileName
            let μgProtPerμlSample       = getμgProtPerμlSample fileName
            let μgQProtSpike            = get15N_PS_Amount fileName
            let μgloadedProtein         = getLoadAmount fileName
            let molWeightQProt          = PSMass
            let molWeightTargetProt     = peptide.AverageProtGroupMass
            let result = 
                calcAbsoluteAbundance
                    μgChlorophPerMlCult  
                    cellCountPerMlCult   
                    μgChlorophPerμlSample
                    μgProtPerμlSample    
                    μgQProtSpike         
                    μgloadedProtein
                    molWeightQProt       
                    molWeightTargetProt
                    ratio
            result.AttoMolProteinPerCell
        with :? System.FormatException -> nan
    )

(***condition:ipynb***)
#if IPYNB
formatAsTable absoluteAbundances |> Chart.Show
#endif // IPYNB

// Why don't we see results for the SDS experiments with CBB-QProt?
// Why don't we see any results in the BN experiment?
// Discuss (and maybe look into your assay file if you don't progress).

(**
To see if our calculations are not off, we look at the calculated abundance for the well studied abundances of rbcL and RBCS
and compare this to the published knowledge about these proteins.
For this, we write a function that, given a protein synonym and a list of peptide sequences, returns a list of quantifications (via mean)
and the estimated uncertainty (via standard deviation). The results can then be visualized using e.g. column charts.
*)

let extractAbsoluteAbundancesOf filterCutoutProtein groupID prot peptidelist = 
    absoluteAbundances
    |> Frame.filterRows (fun k s -> k.Synonyms |> String.contains prot)
    |> Frame.filterRows (fun k s -> 
        peptidelist |> List.exists (fun (sequence,charge) -> sequence = k.StringSequence && charge = k.Charge)
    )
    |> Frame.getNumericCols 
    |> Series.observationsAll
    |> Seq.sortBy (fun (k, x) -> getLoadAmount k)
    |> Seq.filter (fun (k, x) -> getGroupID k = groupID)
    // why do we filter out the protein which is not cut out here?
    |> fun res ->
        if filterCutoutProtein then
            res
            |> Seq.filter (fun (k, x) -> 
                match getCutoutBand k with
                | RbcL -> String.contains (prot.ToLower()) (k.ToLower())
                | RbcS -> String.contains (prot.ToLower()) (k.ToLower())
            )
        else res
    |> Seq.choose (fun (k,x) -> 
        match x with
        | Some x -> Some (k, x)
        | _ -> None
    )
    |> Seq.map (fun (k, v) -> 
        {|
            Filename   = k 
            MeanQuant  = Stats.mean v
            StdevQuant = Stats.stdDev v
        |}
    )

// with filtering
let rbclQuantification = 
    extractAbsoluteAbundancesOf true 2 "rbcL" ["DTDILAAFR", 2;"FLFVAEAIYK", 2] 
    |> Array.ofSeq
let rbcsQuantification = 
    extractAbsoluteAbundancesOf true 2 "RBCS" ["AFPDAYVR", 2;"LVAFDNQK", 2] 
    |> Array.ofSeq

let protAbundanceChart =
    [
        Chart.Column(rbclQuantification |> Seq.map (fun x -> x.Filename + "_rbcL"),rbclQuantification |> Seq.map (fun x -> x.MeanQuant))
        |> Chart.withYErrorStyle (rbclQuantification |> Seq.map (fun x -> x.StdevQuant))
        |> Chart.withTraceName "rbcL"
        Chart.Column(rbcsQuantification |> Seq.map (fun x -> x.Filename + "_rbcS"),rbcsQuantification |> Seq.map (fun x -> x.MeanQuant))
        |> Chart.withYErrorStyle (rbcsQuantification |> Seq.map (fun x -> x.StdevQuant))
        |> Chart.withTraceName "RBCS"
    ]
    |> Chart.Combine
    |> Chart.withY_AxisStyle "protein abundance [amol/cell]"

(***condition:ipynb***)
#if IPYNB
protAbundanceChart
#endif // IPYNB

// without filtering. Compare both Charts.
let rbclQuantification' = 
    extractAbsoluteAbundancesOf false 2 "rbcL" ["DTDILAAFR", 2;"FLFVAEAIYK", 2] 
    |> Array.ofSeq
let rbcsQuantification' = 
    extractAbsoluteAbundancesOf false 2 "RBCS" ["AFPDAYVR", 2;"LVAFDNQK", 2] 
    |> Array.ofSeq

let protAbundanceChart' =
    [
        Chart.Column(rbclQuantification' |> Seq.map (fun x -> x.Filename + "_rbcL"),rbclQuantification' |> Seq.map (fun x -> x.MeanQuant))
        |> Chart.withYErrorStyle (rbclQuantification' |> Seq.map (fun x -> x.StdevQuant))
        |> Chart.withTraceName "rbcL"
        Chart.Column(rbcsQuantification' |> Seq.map (fun x -> x.Filename + "_rbcS"),rbcsQuantification' |> Seq.map (fun x -> x.MeanQuant))
        |> Chart.withYErrorStyle (rbcsQuantification' |> Seq.map (fun x -> x.StdevQuant))
        |> Chart.withTraceName "RBCS"
    ]
    |> Chart.Combine
    |> Chart.withY_AxisStyle "protein abundance [amol/cell]"

(***condition:ipynb***)
#if IPYNB
protAbundanceChart'
#endif // IPYNB

(**
Since we didn't change the amount of QProt given to the sample but the amount of protein loaded into our SDS-PAGE, we check the reliability of our experiments via comparing 
the chart above with a chart of the protein quantification per band. We remember that the bands were loaded with different amounts of the proteins seperated by the SDS-PAGE.

Uncomment everything below.
This is the formula from above. Now that we want to get the protein per band in mol, just add a respective calculation to get the protein band in mol and define it in the 
anonymous record type.
*)

// let calcAbsoluteAbundance μgChlorophPerMlCult cellCountPerMlCult μgChlorophPerμlSample μgProtPerμlSample μgQProtSpike μgloadedProtein molWeightQProt molWeightTargetProt ratio1415N =
//     let chlorophPerCell : float = μgChlorophPerMlCult / cellCountPerMlCult
//     let cellsPerμlSample = μgChlorophPerμlSample / chlorophPerCell
//     let μgProteinPerCell = μgProtPerμlSample / cellsPerμlSample
//     let molQProtSpike = μgQProtSpike * 10. ** -6. / molWeightQProt
//     let molProtPerBand = ratio1415N * molQProtSpike
//     let molProtIn1μgLoadedProt = molProtPerBand / μgloadedProtein
//     let gTargetProtIn1μgLoadedProt = molWeightTargetProt * molProtIn1μgLoadedProt
//     let molProteinPerCell = molProtIn1μgLoadedProt * μgProteinPerCell
//     let proteinsPerCell = molProteinPerCell * 6.022 * 10. ** 23.
//     let attoMolProteinPerCell = molProteinPerCell * (10. ** 18.)
//     let attoMolProteinPerBand = ??? // <--- write your calculation here
//     {|
//         MassTargetProteinInLoadedProtein    = gTargetProtIn1μgLoadedProt
//         ProteinsPerCell                     = proteinsPerCell
//         AttoMolProteinPerCell               = attoMolProteinPerCell
//         AttoMolProteinPerBand               = attoMolProteinPerBand
//     |}

// let absoluteBandAbundances = 
//     ratios
//     |> Frame.map (fun peptide fileName ratio -> 
//         try 
//             let μgChlorophPerMlCult     = getμgChlorophPerMlCult fileName
//             let cellCountPerMlCult      = getCellCountPerMlCult fileName
//             let μgChlorophPerμlSample   = getμgChlorophPerμlSample fileName
//             let μgProtPerμlSample       = getμgProtPerμlSample fileName
//             let μgQProtSpike            = get15N_PS_Amount fileName
//             let μgloadedProtein         = getLoadAmount fileName
//             let molWeightQProt          = PSMass
//             let molWeightTargetProt     = peptide.AverageProtGroupMass
//             let result = 
//                 calcAbsoluteAbundance
//                     μgChlorophPerMlCult  
//                     cellCountPerMlCult   
//                     μgChlorophPerμlSample
//                     μgProtPerμlSample    
//                     μgQProtSpike         
//                     μgloadedProtein
//                     molWeightQProt       
//                     molWeightTargetProt
//                     ratio
//             result.AttoMolProteinPerBand
//         with :? System.FormatException -> nan
//     )

// let extractAbsoluteBandAbundancesOf prot peptidelist = 
//     absoluteBandAbundances
//     |> Frame.filterRows (fun k s -> k.Synonyms |> String.contains prot)
//     |> Frame.filterRows (fun k s -> 
//         peptidelist |> List.exists (fun (sequence,charge) -> sequence = k.StringSequence && charge = k.Charge)
//     )
//     |> Frame.getNumericCols 
//     // |> Series.filter (fun k s -> getExpressionLevel k = "")
//     |> Series.map (fun k v -> 
//         {|
//             Synonym    = k 
//             MeanQuant  = Stats.mean v
//             StdevQuant = Stats.stdDev v
//         |}
//     )
//     |> Series.values

// let rbclBandQuantification = 
//     extractAbsoluteBandAbundancesOf "rbcL" ["DTDILAAFR", 2;"FLFVAEAIYK", 2] 
//     |> Array.ofSeq
// let rbcsBandQuantification = 
//     extractAbsoluteBandAbundancesOf "RBCS" ["AFPDAYVR", 2;"LVAFDNQK", 2] 
//     |> Array.ofSeq

// let protAbundanceBandChart =
//     [
//         Chart.Column(rbclBandQuantification |> Seq.map (fun x -> x.Synonym),rbclBandQuantification |> Seq.map (fun x -> x.MeanQuant))
//         |> Chart.withYErrorStyle (rbclBandQuantification |> Seq.map (fun x -> x.StdevQuant))
//         |> Chart.withTraceName "rbcL"
//         Chart.Column(rbcsBandQuantification |> Seq.map (fun x -> x.Synonym),rbcsBandQuantification |> Seq.map (fun x -> x.MeanQuant))
//         |> Chart.withYErrorStyle (rbcsBandQuantification |> Seq.map (fun x -> x.StdevQuant))
//         |> Chart.withTraceName "RBCS"
//     ]
//     |> Chart.Combine
//     |> Chart.withY_AxisStyle "protein abundance per band [amol/band]"

// protAbundanceBandChart |> Chart.Show

(**
Comparing this to the published results (see: https://www.frontiersin.org/articles/10.3389/fpls.2020.00868/full) we see that our preliminary results are
not only in the same order of magnitude as the published values, but in many cases really close! Of course it could be that you see systematic differences between your results
and published results. As data analysts it is now your task to estimate if the differences are the product of biology (e.g. different growth conditions or genetic background)
or caused by technical artifacts (e.g. different amounts of spiked proteins, mistakes estimating a parameter like the cell count) which could be accounted for by developing
normalization strategies. We look forward to read your explanations!
*)