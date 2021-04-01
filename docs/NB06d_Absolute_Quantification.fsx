#r "nuget: FSharp.Stats, 0.4.0"
#r "nuget: BioFSharp, 2.0.0-beta5"
#r "nuget: BioFSharp.IO, 2.0.0-beta5"
#r "nuget: Plotly.NET, 2.0.0-beta8"
#r "nuget: BIO-BTE-06-L-7_Aux, 0.0.8"
#r "nuget: Deedle, 2.3.0"
#r "nuget: ISADotNet, 0.2.4"
#r "nuget: ISADotNet.XLSX, 0.2.4"

#if IPYNB
#r "nuget: Plotly.NET.Interactive, 2.0.0-beta8"
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

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=NB06d_Absolute_Quantification.ipynb)

[Download Notebook](https://github.com/CSBiology/BIO-BTE-06-L-7/releases/download/NB06d/NB06d_Absolute_Quantification.ipynb)

Finally, after careful peptide ion selection, quality control and assuring that our label efficiency allows accurate for quantifications, we can start to
calculate protein abundancies. Since we start again by getting access to our data and its description, this notebook will start off familiar!

## I. Reading the sample description

As always: before we analyze our data, we will download and read the sample description provided by the experimentalist.
*)
let directory = __SOURCE_DIRECTORY__
let path2 = Path.Combine[|directory;"downloads/alle_Gruppen_V7_SWATE.xlsx"|]
downloadFile path2 "alle_Gruppen_V7_SWATE.xlsx" "bio-bte-06-l-7"

let _,_,_,myAssayFile = XLSX.AssayFile.AssayFile.fromFile path2
let inOutMap = BIO_BTE_06_L_7_Aux.ISA_Aux.createInOutMap myAssayFile

(**
Next, we will prepare functions to look up parameters which might be needed for further calculations.
If you compare this list to the one of note book *NB06b Data Access and Quality Control* you will find additional functions. We will need these functions
in order to calculate the absolute abundances. 
*)

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
let get15N_CBC_Amount (fileName:string) =
    let fN = fileName |> normalizeFileName
    BIO_BTE_06_L_7_Aux.ISA_Aux.tryGetCharacteristic inOutMap "Extraction" "gram" fN myAssayFile |> Option.defaultValue ""
    |> String.split ' '
    |> Array.head
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

(**
A quick execution to test the retrieval of data from the isa sample table:
*)

getStrain "WCGr2_U1.wiff"
getExpressionLevel "WCGr2_U1.wiff"
getμgChlorophPerMlCult "WCGr2_U1.wiff"
getCellCountPerMlCult "WCGr2_U1.wiff"
getμgChlorophPerμlSample "WCGr2_U1.wiff"
getμgProtPerμlSample "WCGr2_U1.wiff"
get15N_CBC_Amount "WCGr2_U1.wiff"
get15N_PS_Amount "WCGr2_U1.wiff"
getGroupID "WCGr2_U1.wiff"

(**
## II. Reading the data
As promised, we start this notebook with the output of the previous analysis, this notebook assumes that the data from *NB06b Data Access and Quality Control* is stored in a .txt
*)

// Similarly to the previous notebook, we start by defining a type, modelling our qProteins. 
type Qprot = 
    | CBB
    | PS 

// Finally we want to define a function that given a distinct Qprot,
// returns the correct ISA lookup. (See: 'Reading the sample description')
let initGetQProtAmount qProt =
    match qProt with 
    | CBB -> get15N_CBC_Amount
    | PS  -> get15N_PS_Amount

type PeptideIon = 
    {|
        ProteinGroup    : string  
        Synonyms        : string
        StringSequence  : string
        PepSequenceID   : int
        Charge          : int
        QProt           : Qprot
    |}

//This is the filepath you chose in *NB06b Data Access and Quality Control*
let filePath = @"C:\YourPath\testOut.txt"

let qConcatDataFiltered =
    Frame.ReadCsv(path = filePath,separators="\t")
    // StringSequence is the peptide sequence
    |> Frame.indexRowsUsing (fun os -> 
            let proteinGroup = os.GetAs<string>("ProteinGroup")
            let qprot = 
                match proteinGroup |> String.contains "QProt_newCBB", proteinGroup |> String.contains "QProt_newPS" with 
                | true, false  -> Some CBB
                | false, true  -> Some PS 
                | _ -> None  
            {|
                ProteinGroup    = os.GetAs<string>("ProteinGroup"); 
                Synonyms        = os.GetAs<string>("Synonyms")
                StringSequence  = os.GetAs<string>("StringSequence");
                PepSequenceID   = os.GetAs<int>("PepSequenceID");
                Charge          = os.GetAs<int>("Charge");
                QProt           = qprot;
            |}
        )
    |> Frame.filterRows (fun k s -> k.QProt.IsSome)
    |> Frame.mapRowKeys (fun k -> {|k with QProt = k.QProt.Value|})

(***condition:ipynb***)
#if IPYNB
qConcatDataFiltered
|> Frame.take 10
|> formatAsTable 1500.
|> Chart.Show
#endif // IPYNB

(**
## III. From Ratios to mol proteins per cell.

Now we can use the extensive information stored in the sample sheet and map each quantified peptide ion passing
the quality checks to an estimator for protein abundance! First we start off by defining a function to extract ratios:
*)

let sliceQuantColumns quantColID frame = 
    frame
    |> Frame.filterCols (fun ck os -> ck |> String.contains ("."+quantColID))
    |> Frame.mapColKeys (fun ck -> ck.Split('.') |> Array.item 0)


(**
Next up, we have to define a function, which maps the measured ratio and measured parameters to an quantification value.
*)

/// 
let calcAbsoluteAbundance μgChlorophPerMlCult cellCountPerMlCult μgChlorophPerμlSample μgProtPerμlSample μgQProtSpike molWeightQProt molWeightTargetProt ratio1415N =
    let chlorophPerCell :float = μgChlorophPerMlCult / cellCountPerMlCult
    let cellsPerμlSample = μgChlorophPerμlSample / chlorophPerCell
    let μgProteinPerCell = μgProtPerμlSample / cellsPerμlSample
    let molQProtSpike = μgQProtSpike * 10. ** -6. / molWeightQProt
    let molProtIn50μgWCProt = ratio1415N * molQProtSpike
    let molProtIn1μgWCProt = molProtIn50μgWCProt / 50.
    let gTargetProtIn1μgWCProt = molWeightTargetProt * molProtIn1μgWCProt
    let molProteinPerCell = molProtIn1μgWCProt * μgProteinPerCell
    let proteinsPerCell = molProteinPerCell * 6.022 * 10. ** 23.
    let attoMolProteinPerCell = molProteinPerCell * (10.**18.)
    {|
        MassTargetProteinInWCProtein    = gTargetProtIn1μgWCProt
        ProteinsPerCell                 = proteinsPerCell
        AttoMolProteinPerCell           = attoMolProteinPerCell
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

let CBB = 
    examplePeptides 
    |> Seq.find (fun prot -> prot.Header |> String.contains "QProt_newCBB2")

let CBBMass = 
    BioFSharp.BioSeq.toMonoisotopicMassWith (Formula.monoisoMass Formula.Table.H2O) CBB.Sequence

let PS = 
    examplePeptides 
    |> Seq.find (fun prot -> prot.Header |> String.contains "QProt_newPS")

let PSMass = 
    BioFSharp.BioSeq.toMonoisotopicMassWith (Formula.monoisoMass Formula.Table.H2O) PS.Sequence

let getQProtMass qProt =
    match qProt with 
    | CBB -> CBBMass
    | PS  -> PSMass

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

(***condition:ipynb***)
#if IPYNB
ratios
|> Frame.take 10
|> formatAsTable 1500.
|> Chart.Show
#endif // IPYNB

(** 
Finally, we can iterate the ratios and map each to a protein abundance using our well annotated experiment.
*)

//
let absoluteAbundances  = 
    ratios
    |> Frame.map (fun peptide filenName ratio -> 
        let μgChlorophPerMlCult     = getμgChlorophPerMlCult filenName
        let cellCountPerMlCult      = getCellCountPerMlCult filenName
        let μgChlorophPerμlSample   = getμgChlorophPerμlSample filenName
        let μgProtPerμlSample       = getμgProtPerμlSample filenName
        let μgQProtSpike            = initGetQProtAmount peptide.QProt filenName
        let molWeightQProt          = getQProtMass peptide.QProt
        let molWeightTargetProt     = peptide.AverageProtGroupMass
        let result = 
            calcAbsoluteAbundance
                μgChlorophPerMlCult  
                cellCountPerMlCult   
                μgChlorophPerμlSample
                μgProtPerμlSample    
                μgQProtSpike         
                molWeightQProt       
                molWeightTargetProt
                ratio
        result.AttoMolProteinPerCell 
        )

(**
To see if our calculations are not off, we look at the calculated abundance for the well studied abundances of rbcL and RBCS
and compare this to the published knowledge about these proteins.
For this, we write a function that, given a protein synonym and a list of peptide sequences, returns a list of quantifications (via mean)
and the estimated uncertainty (via standard deviation). The results can then be visualized using e.g. column charts.
*)

let extractAbsolutAbundancesOf prot peptidelist = 
    absoluteAbundances
    |> Frame.filterRows (fun k s -> k.Synonyms |> String.contains prot)
    |> Frame.filterRows (fun k s -> 
        peptidelist |> List.exists (fun (sequence,charge) -> sequence = k.StringSequence && charge = k.Charge)
        )
    |> Frame.getNumericCols 
    |> Series.filter (fun k s -> getExpressionLevel k = "")
    |> Series.map (fun k v -> 
        {|
            Filename   = k 
            MeanQuant  = Stats.mean v
            StdevQuant = Stats.stdDev v
        |}
        )
    |> Series.values

let rbclQuantification = extractAbsolutAbundancesOf "rbcL" ["DTDILAAFR", 2;"FLFVAEAIYK",2]
let rbcsQuantification = extractAbsolutAbundancesOf "RBCS" ["AFPDAYVR", 2;"LVAFDNQK",2]

let protAbundanceChart =
    [
    Chart.Column(rbclQuantification |> Seq.map (fun x -> x.Filename),rbclQuantification |> Seq.map (fun x -> x.MeanQuant))
    |> Chart.withYErrorStyle (rbclQuantification |> Seq.map (fun x -> x.StdevQuant))
    |> Chart.withTraceName "rbcL"
    Chart.Column(rbcsQuantification |> Seq.map (fun x -> x.Filename),rbcsQuantification |> Seq.map (fun x -> x.MeanQuant))
    |> Chart.withYErrorStyle (rbcsQuantification |> Seq.map (fun x -> x.StdevQuant))
    |> Chart.withTraceName "RBCS"
    ]
    |> Chart.Combine
    |> Chart.withY_AxisStyle "protein abundance [amol/cell]"

protAbundanceChart
(***hide***)
protAbundanceChart |> GenericChart.toChartHTML
(***include-it-raw***)

(**
Comparing this to the published results (see: https://www.frontiersin.org/articles/10.3389/fpls.2020.00868/full) we see that our preliminary results are
not only in the same order of magnitude as the published values, but in many cases really close! Of course it could be that you see systematic differences between your results
and published results. As data analysts it is now your task to estimate if the differences are the product of biology (e.g. different growth conditions or genetic background)
or caused by technical artifacts (e.g. different amounts of spiked proteins, mistakes estimating a parameter like the cell count) which could be accounted for by developing
normalization strategies. We look forward to read your explanations!
*)