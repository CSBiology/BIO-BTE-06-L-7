#r "nuget: FSharp.Stats, 0.4.0"
#r "nuget: BioFSharp, 2.0.0-beta5"
#r "nuget: BioFSharp.IO, 2.0.0-beta5"
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: BIO-BTE-06-L-7_Aux, 0.0.6"
#r "nuget: Deedle, 2.3.0"
#r "nuget: ISADotNet, 0.2.4"
#r "nuget: ISADotNet.XLSX, 0.2.4"

#if IPYNB
#r "nuget: Plotly.NET, 2.0.0-beta8"
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
# NB06b Data Access and Quality Control

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=NB06b_Data_Access_And_Quality_Control_SDS.ipynb)

[Download Notebook](https://github.com/CSBiology/BIO-BTE-06-L-7/releases/download/NB06b_NB06b_NB06c_NB06c_NB06d_NB06d/NB06b_Data_Access_And_Quality_Control_SDS.ipynb)

With this notebook, we want to converge the threads of computational and experimental proteomics by analyzing the data measured by you during the practical course.
Behind the scenes there was already a lot going on! While you were going through the hands-on tutorials addressing single steps of the computation proteomics pipeline, we executed
a pipeline combining all the steps. In this tutorial, we will provide you with the output of the pipeline, your sample description and a lot of code, which should help you explore the 
data set generated by you! Since the experiment is of a non neglectable depth - so is the code needed to analyze it. Do not hesitate if you do not understand every step right away, this is no
trivial task and nontrivial are the means to achieve it! We included some questions along the way. The questions aim to check your understanding, but they can also serve as food for thought.

For the explorative data analysis, we are using [Deedle](http://bluemountaincapital.github.io/Deedle/tutorial.html).
Deedle is an easy to use library for data and time series manipulation and for scientific programming. 
It supports working with structured data frames, ordered and unordered data, as well as time series. Deedle is designed to work well for exploratory programming using F#.

## I. Reading the sample description

Before we analyze our data, we will download and read the sample description provided by the experimentalist.
*)
let directory = __SOURCE_DIRECTORY__
let path2 = Path.Combine[|directory;"downloads/alle_Gruppen_V11_SWATE.xlsx"|]
downloadFile path2 "alle_Gruppen_V11_SWATE.xlsx" "bio-bte-06-l-7"

let _,_,_,myAssayFile = XLSX.AssayFile.AssayFile.fromFile path2
let inOutMap = BIO_BTE_06_L_7_Aux.ISA_Aux.createInOutMap myAssayFile

(**
Next, we will prepare functions to look up parameters, which might be needed for further calculations. 
*)

let normalizeFileName (f:string) = if Path.HasExtension f then f else Path.ChangeExtension(f, "wiff")

type CutoutBand =
    | RbcL
    | RbcS

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
get15N_PS_Amount "Gr2rbcL2_5.wiff"
getGroupID "Gr2rbcL2_5.wiff"
getLoadAmount "Gr2rbcL2_5.wiff"
getCutoutBand "Gr2rbcL2_5.wiff"

(**
Now that we have the sample sheet, all that is missing is the data to be analyzed:
*)

let path = Path.Combine[|directory; "downloads/Quantifications_sds_annotated_replaced.txt"|]
downloadFile path "Quantifications_sds_annotated.txt" "bio-bte-06-l-7"

(**
## II. Raw data access using Deedle:
As teasered in the primer, we want to work with our tabular data using Deedle. Luckily, Deedle does not only deliver data frame and series
manipulation, but also allows us to quickly read the recently downloaded data into the memory:
*)

let rawData = Frame.ReadCsv(path, separators = "\t")

(**
To visualize the data, we can call the "formatAsTable" function. The preview of visual studio code does not allow
for the charts to be scrollable, so we pipe the output into "Chart.Show", to visualize the data in your browser.
*)

(***condition:ipynb***)
#if IPYNB
rawData
|> Frame.take 10
|> formatAsTable 
|> Chart.Show

#endif // IPYNB
(***hide***)
rawData |> Frame.take 10 |> fun x -> x.Print()

(***include-fsi-merged-output***)
(**
Looking at the raw data, we can see that each row contains a different quantification of a peptide ion, with the columns containing 
a single ion feature each, such as peptide ion charge, sequence or a quantification value reported for a file (e.g. light, heavy or ratio).
Since the columns ProteinGroup, StringSequence, PepSequenceID and Charge uniquely identify a row, we can use these to index the rows.
For this, we use a language feature called ["anonymous record type"](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/anonymous-records). 
Here we create a tuple like structure, with the additional feature that each element of the tuple is named (e.g.: Proteingroup).
*)
let indexedData =
    rawData
    // StringSequence is the peptide sequence
    |> Frame.indexRowsUsing (fun os -> 
        {|
            ProteinGroup    = os.GetAs<string>("ProteinGroup"); 
            Synonyms        = os.GetAs<string>("Synonyms")
            StringSequence  = os.GetAs<string>("StringSequence");
            PepSequenceID   = os.GetAs<int>("PepSequenceID");
            Charge          = os.GetAs<int>("Charge")
        |}
    )
    

(***condition:ipynb***)
#if IPYNB
// The effect of our frame manipulation can be observed:
indexedData
|> Frame.take 10
|> formatAsTable 
|> Chart.Show

#endif // IPYNB
(***hide***)
indexedData |> Frame.take 10 |> fun x -> x.Print()

(***include-fsi-merged-output***)
(**
## III. Augmenting and filtering the data frame 
The data frame already contains all information needed to perform the analysis, but it could still benefit from 
some quality-of-life upgrades. Say, we want to encode the specific qConcat protein as a separate feature:
*)
// Why does it make sense to model Qprots using this type, why do we not simply use a string?

let finalRaw = 
    indexedData
    // What does this line filter for? Why does this make sense for our analysis?
    // How many peptide ions did the filter remove? 
    |> Frame.filterRows (fun k s -> k.Synonyms <> "" && k.ProteinGroup |> String.contains "QProt_newPS")

finalRaw
|> Frame.filterRows (fun x s -> x.StringSequence = "DTDILAAFR")
|> Frame.getCol "Gr3rbcL2_5.Ratio"
|> Series.mapValues string

(***condition:ipynb***)
#if IPYNB
finalRaw
|> Frame.take 10
|> formatAsTable 
|> Chart.Show

#endif // IPYNB
(***hide***)
finalRaw |> Frame.take 10 |> fun x -> x.Print()

(***include-fsi-merged-output***)

(**
## IV. Global quality control.

With our data frame prepared, we want to see on a global scale if our experiment worked.
We plot the overall mean of the 14N and 15N quantifications and observe if we can recover our dilution series (15N),
while keeping the analyte to be quantified at a constant level (14N).

Since it comes in handy to simplify the data frame, we will only keep columns that contain a specific identifier, 
such as, "Ratio", "Light" or "Heavy". 
*)
let sliceQuantColumns quantColID frame = 
    frame
    |> Frame.filterCols (fun ck os -> ck |> String.contains ("." + quantColID))
    |> Frame.mapColKeys (fun ck -> ck.Split('.') |> Array.item 0)

// How did the data frame change, how did the column headers change?
let ratios = sliceQuantColumns "Ratio" finalRaw
let light = sliceQuantColumns "Light" finalRaw
let heavy = sliceQuantColumns "Heavy" finalRaw

ratios
|> Frame.filterRows (fun x s -> x.StringSequence = "DTDILAAFR")
|> Frame.getCol "Gr3rbcL2_5"
|> Series.mapValues string

(***condition:ipynb***)
#if IPYNB
ratios
|> Frame.take 10
|> formatAsTable 
|> Chart.Show

#endif // IPYNB
(***hide***)
ratios |> Frame.take 10 |> fun x -> x.Print()

(***include-fsi-merged-output***)

(**
A nice tool to explore and compare distributions of different populations is the representation as a boxplot!
To use this tool, we will define a function which creates a boxplot for every column (file) of our data set:
*)
let createBoxPlot f = 
    f
    |> Frame.getNumericCols
    |> Series.map (fun k s -> 
         let x,y =
            s
            |> Series.values 
            |> Seq.map (fun values -> k,values)
            |> Seq.unzip
         Chart.BoxPlot(x, y, Orientation = StyleParam.Orientation.Vertical)         
         )
    |> Series.values
    |> Chart.Combine
    |> Chart.withY_AxisStyle "Ion intensity"

(**
The function applied to the n14 values: 
*)
// How is the data distributed?
light
|> createBoxPlot

(***hide***)
light |> createBoxPlot |> GenericChart.toChartHTML
(***include-it-raw***)
(**
The function applied to the n15 values:
*)

// Can you recover the dilution series?
heavy
|> createBoxPlot

(***hide***)
heavy |> createBoxPlot |> GenericChart.toChartHTML
(***include-it-raw***)
(**
The following function performs a normalization which accounts for a specific effect. Can you 
determine what the function accounts for?
*)
let normalizePeptides f =
    f
    |> Frame.transpose
    |> Frame.getNumericCols
    |> Series.mapValues (fun s -> 
        let m = Stats.median s
        s / m 
        )
    |> Frame.ofColumns
    |> Frame.transpose
(**
*)
// How does the distribution of the date change, when the normalization is applied? 
light
|> normalizePeptides
|> createBoxPlot

(***hide***)
light |> normalizePeptides |> createBoxPlot |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
heavy
|> normalizePeptides
|> createBoxPlot 

(***hide***)
heavy |> normalizePeptides |> createBoxPlot |> GenericChart.toChartHTML
(***include-it-raw***)
(**
Finally we have a look at the ratios. 
*)

// Does it make sense to normalize the ratios the same way?
ratios
|> createBoxPlot 

(**
## Local quality control

Now that we know on a global scale how our experiment worked, it might be time to have a look at the details.
First, we want to write a function that allows us to plot all peptides of a protein vs. the dilution used. This way we can identify peptides that
we want to use and those that seem to be prone to error and should thus be discarded. 
To keep things simple, we apply a filter step at the beginning, which only keeps peptides belonging to one protein and samples measured by one group
in the data frame. What are the sources of error? Which peptides do you think should be discarded and why? Which proteins need to be analyzed with extra care?
Hint: you can hover over the data points to get insight into the file name and gene expression pattern of the corresponding strain.
*)

// With this type we create an alias to our row key, this allows us to write functions, which operate on data frames such as 'plotPeptidesOf','discardPeptideIonInFile' and 'discardPeptideIon'
type PeptideIon = 
    {|
        ProteinGroup    : string  
        Synonyms        : string
        StringSequence  : string
        PepSequenceID   : int
        Charge          : int
    |}



// Given a frame, a prot-ID and a group-ID this function creates an xy plot for every peptide ion belonging to the protein/proteingroup.
// The parameter 'prot' can either be given a valid Cre-ID or a synonym.
// What is the unit of the x-Axis? How is the ratio calculated? 
let plotPeptidesOf (ratios : Frame<PeptideIon,string>) (prot : string) (cutoutBand : CutoutBand) (groupID : int) = 
    try 
        ratios
        |> Frame.filterRows (fun k s -> k.Synonyms.Contains prot || k.ProteinGroup.Contains prot)
        |> Frame.filterCols (fun k s -> getCutoutBand k = cutoutBand && getGroupID k = groupID)   
        |> Frame.transpose
        |> Frame.getNumericCols
        |> Series.map (fun pep (values) -> 
            let loadAmounts,ratios,fileLabel =
                values
                |> Series.map (fun fileName (ratio) -> 
                        let loadAmount = getLoadAmount fileName
                        let expressionLevel = getExpressionLevel fileName
                        loadAmount, ratio, (sprintf "%s %s" fileName expressionLevel)         
                    )
                |> Series.values
                |> Seq.unzip3
            Chart.Point(loadAmounts, ratios, Labels = fileLabel)
            |> Chart.withTraceName (sprintf "S:%s_C:%i" pep.StringSequence pep.Charge)
            |> Chart.withX_AxisStyle("Loaded protein amount")
            |> Chart.withY_AxisStyle("Ratio")
            )
        |> Series.values
        |> Chart.Combine
        |> Chart.withTitle "SDS-PAGE extraction"
    with :? System.ArgumentException ->  failwith (sprintf "Input protein %s was not found" prot)

(**
First we get an overview of available protein ids.
*)

ratios.RowKeys
|> Array.ofSeq 
|> Array.map (fun k -> k.Synonyms)
|> Array.distinct

(**
Then we can start to visualizes our results:
*)
(***condition:ipynb***)
#if IPYNB
plotPeptidesOf ratios "rbcL" RbcL 1
#endif // IPYNB
(***hide***)
plotPeptidesOf ratios "rbcL" RbcL 1 |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
(***condition:ipynb***)
#if IPYNB
plotPeptidesOf ratios "RBCS2;RBCS1" RbcS 2
#endif // IPYNB
(***hide***)
plotPeptidesOf ratios "RBCS2;RBCS1" RbcS 2 |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
(***condition:ipynb***)
#if IPYNB
plotPeptidesOf ratios "SEBP1" RbcL 4
#endif // IPYNB
(***hide***)
plotPeptidesOf ratios "SEBP1" RbcL 4 |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
(***condition:ipynb***)
#if IPYNB
plotPeptidesOf ratios "TRK1" RbcL 1 
#endif // IPYNB
(***hide***)
plotPeptidesOf ratios "TRK1" RbcL 1 |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)

(***condition:ipynb***)
#if IPYNB
plotPeptidesOf ratios "PRK1" RbcL 1
#endif // IPYNB
(***hide***)
plotPeptidesOf ratios "PRK1" RbcL 1 |> GenericChart.toChartHTML
(***include-it-raw***)

// Describe what happened with the last 3 plots.


(**
With the plots at hand, we can use the following functions to manipulate the data frame and discard peptides and/or whole files which we do not want to use for 
an absolute protein quantification e.g.:
*)
let discardPeptideIonInFile stringsequence charge filename (ratios:Frame<PeptideIon,string>) = 
    ratios
    |> Frame.map (fun r c value -> 
        let cFileName = String.split '.' c |> Array.head
        if r.StringSequence = stringsequence && r.Charge = charge && cFileName = filename then nan else value
    )

let discardPeptideIon stringsequence charge (ratios:Frame<PeptideIon,string>) = 
    ratios
    |> Frame.filterRows (fun r s -> (r.StringSequence = stringsequence && r.Charge = charge) |> not)
(**
These functions can then be used to create an updated version of the frame, containing only the values we want to use for quantification e.g.:
*)
let filtered = 
    ratios
    |> discardPeptideIonInFile "IYSFNEGNYGLWDDSVK" 3 "Gr2rbcL2_5" 
    |> discardPeptideIon "IYSFNEGNYGLWDDSVK" 2


// Plotting the updated frame again, we see that the exemplary filtering worked just fine.
(***condition:ipynb***)
#if IPYNB
plotPeptidesOf filtered "rbcL" RbcL 1
#endif // IPYNB
(***hide***)
plotPeptidesOf filtered "rbcL" RbcL 1 |> GenericChart.toChartHTML
(***include-it-raw***)

(**
Of course, it is possible to apply very strict additional filters onto the previously filtered frame:
*)
let ratiosFiltered = 
    filtered
    |> Frame.filterCols (fun k s -> 
        let kFileName = String.split '.' k |> Array.head
        try
            get15N_PS_Amount kFileName > 0.1 
        with
        | _ -> false
    )


(**
Since we want to save our result and use it for the next notebook, where we will have a look at the isotopic labeling efficiency and finally calculate absolute protein amounts, we 
need to save the filtered frame. Additionally, we want to keep information which was dropped along the way: isotopic patterns. In order to do so, we perform a join operation, which keeps only those rows 
present in both files:

*)
//  Are there redundant columns in the result frame? Why?
let frameToSave = 
    Frame.join JoinKind.Inner finalRaw ratiosFiltered
    |> Frame.indexRowsOrdinally


(**
This frame can then be saved locally using the following pattern:    
*)    

// frameToSave.SaveCsv(@"C:\YourPath\testOut.txt", separator = '\t', includeRowKeys = false)
frameToSave.SaveCsv(System.IO.Path.Combine [|__SOURCE_DIRECTORY__; "downloads"; "qualityControlResult_SDS.txt"|], separator = '\t', includeRowKeys = false)