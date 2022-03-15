#r "nuget: FSharp.Stats, 0.4.0"
#r "nuget: BioFSharp, 2.0.0-beta5"
#r "nuget: BioFSharp.IO, 2.0.0-beta5"
#r "nuget: Plotly.NET, 2.0.0-preview.16"
#r "nuget: BIO-BTE-06-L-7_Aux, 0.0.9"
#r "nuget: Deedle, 2.5.0"
#r "nuget: ISADotNet, 0.4.0-preview.4"
#r "nuget: ISADotNet.XLSX, 0.4.0-preview.4"
#r "nuget: ISADotNet.IO, 0.0.2"

#if IPYNB
#r "nuget: Plotly.NET.Interactive, 2.0.0-preview.16"
#endif // IPYNB

open System.IO
open ISADotNet
open Deedle
open FSharpAux
open FSharp.Stats
open Plotly.NET
open arcIO.NET
open BIO_BTE_06_L_7_Aux.Deedle_Aux

(**
# NB08a Data Access and Quality Control (for SDS-PAGE results)

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=NB08a_Data_Access_And_Quality_Control_SDS.ipynb)

[Download Notebook](https://github.com/CSBiology/BIO-BTE-06-L-7/releases/download/NB08a/NB08a_Data_Access_And_Quality_Control_SDS.ipynb)

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

let path2 = @"..\assays\VP21_SDS\isa.assay.xlsx"

let _,_,_,myAssayFile = XLSX.AssayFile.Assay.fromFile path2
let inOutMap = ISADotNet.createInOutMap myAssayFile

(**
Next, we will prepare functions to look up parameters, which might be needed for further calculations. 
*)

let normalizeFileName (f : string) = if Path.HasExtension f then f else Path.ChangeExtension(f, "wiff")

type CutoutBand =
    | RbcL
    | RbcS

//        
let getStrain (fileName : string) =
    let fN = fileName |> normalizeFileName
    ISADotNet.tryGetCharacteristic inOutMap "Cultivation" "strain" fN myAssayFile
    |> Option.defaultValue "Wt"

//
let getExpressionLevel (fileName : string) =
    let fN = fileName |> normalizeFileName 
    ISADotNet.tryGetCharacteristic inOutMap "Cultivation" "gene expression" fN myAssayFile 
    |> Option.defaultValue "Wt-Like"

//
let get15N_PS_Amount (fileName : string) =
    let fN = fileName |> normalizeFileName
    ISADotNet.tryGetParameter inOutMap "Protein extraction" "15N Photosynthesis QconCAT mass #4" fN myAssayFile |> Option.defaultValue "0"
    |> String.split ' '
    |> Array.head
    |> float 
//
let getGroupID (fileName : string) =
    let fN = fileName |> normalizeFileName
    ISADotNet.tryGetParameter inOutMap "Protein extraction" "Group name" fN myAssayFile |> Option.defaultValue ""
    |> int

let getLoadAmount (fileName : string) =
    let fN = fileName |> normalizeFileName
    ISADotNet.tryGetCharacteristic inOutMap "Sample preparation (PAGE)" "soluble protein content" fN myAssayFile |> Option.defaultValue ""
    |> String.split ' '
    |> Array.head
    |> float

let getCutoutBand (fileName : string) =
    let fN = fileName |> normalizeFileName
    match fN.Contains("rbcL"), fN.Contains("rbcS") with
    | true,false -> RbcL
    | false,true -> RbcS
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

let path = @"..runs\VP21_SDS\notebookInput\SDSAnnotated.txt"

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
|> formatAsTable 1500.
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
            Synonyms        = os.GetAs<string>("Synonym")
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
|> formatAsTable 1500.
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

(***condition:ipynb***)
#if IPYNB
finalRaw
|> Frame.take 10
|> formatAsTable 1500.
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
let light  = sliceQuantColumns "Quant_Light" finalRaw
let heavy  = sliceQuantColumns "Quant_Heavy" finalRaw

(***condition:ipynb***)
#if IPYNB
ratios
|> Frame.take 10
|> formatAsTable 1500.
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
    |> Chart.combine
    |> Chart.withYAxisStyle "Ion intensity"

(**
The function applied to the N14 values: 
*)
// How is the data distributed?
light
|> createBoxPlot

(***hide***)
light |> createBoxPlot |> GenericChart.toChartHTML
(***include-it-raw***)
(**
The function applied to the N15 values:
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
let plotPeptidesOf (ratios : Frame<PeptideIon,string>) (prot : string) (groupID : int) = 
    try 
        ratios
        |> Frame.filterRows (fun k s -> k.Synonyms.Contains prot || k.ProteinGroup.Contains prot)
        |> Frame.filterCols (fun k s -> getGroupID k = groupID)   
        |> Frame.transpose
        |> Frame.getNumericCols
        |> Series.map (fun pep (values) -> 
            let fileNames,ratios,fileLabel =
                values
                |> Series.map (fun fileName (ratio) -> 
                        let expressionLevel = getExpressionLevel fileName
                        fileName, ratio, (sprintf "%s %s" fileName expressionLevel)         
                    )
                |> Series.values
                |> Seq.unzip3
            Chart.Point(fileNames, ratios, MultiText = fileLabel)
            |> Chart.withTraceName (sprintf "S:%s_C:%i" pep.StringSequence pep.Charge)
            |> Chart.withXAxisStyle("File name")
            |> Chart.withYAxisStyle("Ratio")
        )
        |> Series.values
        |> Chart.combine
        |> Chart.withTitle "SDS-PAGE extraction"
        |> Chart.withSize (600.,700.)
        |> Chart.withMarginSize (Bottom = 175.)
    with e when e :? System.ArgumentException = true -> failwith $"ERROR: Input protein was not found.\n\t{e.ToString()}"

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
plotPeptidesOf ratios "rbcL" 1
#endif // IPYNB
(***hide***)
plotPeptidesOf ratios "rbcL" 1 |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
(***condition:ipynb***)
#if IPYNB
plotPeptidesOf ratios "RBCS1;RBCS2" 2
#endif // IPYNB
(***hide***)
plotPeptidesOf ratios "RBCS1;RBCS2" 2 |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
(***condition:ipynb***)
#if IPYNB
plotPeptidesOf ratios "SEBP1" 4
#endif // IPYNB
(***hide***)
plotPeptidesOf ratios "SEBP1" 1 |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
(***condition:ipynb***)
#if IPYNB
plotPeptidesOf ratios "TRK1" 1 
#endif // IPYNB
(***hide***)
plotPeptidesOf ratios "TRK1" 1 |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)

(***condition:ipynb***)
#if IPYNB
plotPeptidesOf ratios "PRK1" 1
#endif // IPYNB
(***hide***)
plotPeptidesOf ratios "PRK1" 1 |> GenericChart.toChartHTML
(***include-it-raw***)

// Describe what happened with the last 3 plots.


(**
Since we want to save our result and use it for the next notebook, where we will have a look at the isotopic labeling efficiency and finally calculate absolute protein amounts, we 
need to save the filtered frame. Additionally, we want to keep information which was dropped along the way: isotopic patterns. In order to do so, we perform a join operation, which keeps only those rows 
present in both files:

*)
//  Are there redundant columns in the result frame? Why?
let frameComplete = 
    Frame.join JoinKind.Inner finalRaw ratios

(**
With the plots at hand, we can use the following functions to manipulate the data frame and discard peptides and/or whole files which we do not want to use for 
an absolute protein quantification e.g.:
*)

let discardPeptideIonInFile stringsequence charge filename (ratios : Frame<PeptideIon,string>) = 
    ratios
    |> Frame.map (fun r c value -> 
        let cFileName = String.split '.' c |> Array.head
        if r.StringSequence = stringsequence && r.Charge = charge && cFileName = filename then nan else value
    )

let discardPeptideIon stringsequence charge (ratios : Frame<PeptideIon,string>) = 
    ratios
    |> Frame.filterRows (fun r s -> (r.StringSequence = stringsequence && r.Charge = charge) |> not)
(**
These functions can then be used to create an updated version of the frame, containing only the values we want to use for quantification e.g.:
*)

let filtered = 
    frameComplete
    |> discardPeptideIonInFile "IYSFNEGNYGLWDDSVK" 3 "Gr2rbcL2_5" 
    |> discardPeptideIon "IYSFNEGNYGLWDDSVK" 2

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
        | _ -> true
    )

(**
This frame can then be saved locally using the following pattern:    
*)    

let frameToSave = 
    ratiosFiltered
    |> Frame.indexRowsOrdinally

frameToSave.SaveCsv(@"C:\YourPath\testOut.txt", separator = '\t', includeRowKeys = false)