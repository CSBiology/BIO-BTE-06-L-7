#r "nuget: FSharp.Stats, 0.4.0"
#r "nuget: BioFSharp, 2.0.0-beta5"
#r "nuget: BioFSharp.IO, 2.0.0-beta5"
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: BIO-BTE-06-L-7_Aux, 0.0.5"
#r "nuget: Deedle, 2.3.0"
#r "nuget: ISADotNet, 0.2.3"
#r "nuget: ISADotNet.XLSX, 0.2.3"

#if IPYNB
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: Plotly.NET.Interactive, 2.0.0-beta6"
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

let getRecordFieldAndTypes t =     
    Microsoft.FSharp.Reflection.FSharpType.GetRecordFields t
    |> Array.map (fun x -> 
            {|Name=x.Name;Type=x.PropertyType;GetValue=x.GetValue|}
            )


let formatAsTable (frame:Frame<'a,'b>) =
    let kToString k = 
        let t = k.GetType()
        if FSharp.Reflection.FSharpType.IsRecord t then 
            let sb = new System.Text.StringBuilder()
            let t = getRecordFieldAndTypes t
            t
            |> Array.fold (fun (sb:System.Text.StringBuilder) field -> 
                // sb.AppendLine("asd")
                sb.Append(sprintf "%s: %A;<br>" field.Name (k |> box |> field.GetValue) )
                ) sb
            |> fun x -> x.ToString()
        else
            k.ToString()
    
    let f =
        frame
        |> Frame.mapValues string
        |> Frame.mapColKeys kToString
        |> Frame.mapRowKeys kToString
    let maxColsIdx = f.ColumnCount 
    let maxRowIdx = f.RowCount
    let header = 
        f.ColumnKeys 
        |> Seq.take maxColsIdx
        |> Seq.append ["RowKey"]
    let f' =
        f
        |> Frame.sliceCols header
        |> fun f' -> 
            if f'.ColumnCount < f.ColumnCount then 
                f'
                |> Frame.addCol "..." (f'.RowKeys |> Seq.map (fun ck -> ck,"...") |> Series.ofObservations)
            else 
                f'
    let columnWidth =
        let headerLength = 
            header
            |> Seq.map (fun (x:string) -> (x.Length*10) + 10)
        let colLenght    =
            f'
            |> Frame.getCols 
            |> Series.values
            |> Seq.map (fun s ->
                s
                |> Series.fillMissingWith "null"
                |> Series.values
                |> Seq.map (string >> String.length >> float) 
                |> Seq.average 
                |> int
                )
        let rowKeyLength =
            f'
            |> Frame.getRows
            |> Series.observations
            |> Seq.map (fun (k,v) -> k)
            |> Seq.map (string >> String.length >> float) 
            |> Seq.average 
            |> int
        Seq.map2 (fun (x:int) (y:int) -> System.Math.Min(250,(System.Math.Max(x,y)))) headerLength (Seq.append [rowKeyLength] colLenght)
    let rows = 
        f'    
        |> Frame.mapRows (fun k s -> 
            s.As<string>() 
            |> Series.values 
            |> Seq.append [k])
        |> Series.values
        |> Seq.take maxRowIdx
    Chart.Table(
        header,
        rows,
        AlignHeader = [StyleParam.HorizontalAlign.Left],
        AlignCells  = [StyleParam.HorizontalAlign.Left],
        ColorHeader = "#45546a",    
        ColorCells  = (header |> Seq.mapi (fun i x -> if i%2 = 0 then  "#deebf7" else "lightgrey")),
        FontHeader  = Font.init(StyleParam.FontFamily.Courier_New, Size=12, Color="white"),      
        HeightHeader= 30.,
        LineHeader  = Line.init(2.,"black"),                 
        ColumnWidth = columnWidth , 
        HeightCells= 125.    
        )
    |> Chart.withSize((columnWidth |> Seq.sum |> float |> (*) 2.),500.)

//Todo:
// 1. formatAsTable in repo
// 2. sample sheet umbenennen.

(**
# NB06a Data Access and Quality Control

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=NB06a_Data_Access_And_Quality_Control.ipynb)


For the explorative data analysis, we are using 
[Deedle](http://bluemountaincapital.github.io/Deedle/tutorial.html).
Deedle is an easy to use library for data and time series manipulation and for scientific programming. 
It supports working with structured data frames, ordered and unordered data, as well as time series. Deedle is designed to work well for exploratory programming using F#.

## Reading the sample description

Before we analyze our data, we will download and read the sample description provided by the experimentalist.
*)
let directory = __SOURCE_DIRECTORY__
let path2 = Path.Combine[|directory;"downloads/alle_Gruppen_V7_SWATE.xlsx"|]
downloadFile path2 "alle_Gruppen_V7_SWATE.xlsx" "bio-bte-06-l-7"

let _,_,_,myAssayFile = XLSX.AssayFile.AssayFile.fromFile path2
let inOutMap = BIO_BTE_06_L_7_Aux.ISA_Aux.createInOutMap myAssayFile

(**
Next, we will prepare functions that given a file name look up parameters which might be needed for further calculations. 
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
A quick execution to assure that all values can be retrieved from the isa sample table:
*)
getStrain "WCGr2_U1.wiff"
getExpressionLevel "WCGr2_U1.wiff"
get15N_CBC_Amount "WCGr2_U1.wiff"
get15N_PS_Amount "WCGr2_U1.wiff"
getGroupID "WCGr2_U1.wiff"

(**
Now that we have the sample sheet, all that is missing is the data to be analyzed:
*)

let path = Path.Combine[|directory;"downloads/Quantifications_wc.txt"|]
downloadFile path "Quantifications_wc.txt" "bio-bte-06-l-7"

(**
## Raw data access using Deedle:
As teasered in the primer, we want to work with our tabular data using Deedle. Luckily Deedle does nonly deliver data frame and series
manipulation, but also allows us to quickly read the recently downloaded data into the memory:
*)

let rawData = Frame.ReadCsv(path,separators="\t")

(**
To visualize the data we can call the "formatAsTable" function. The preview of visual studio code does not allow
for the charts to be scrollable so feel free to pipe the output into "Chart.Show", to visualize the data in your browser.
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
Looking at the raw data, we can see that each row contains a different quantifiction of a peptide ion, with the columns containing 
a single ion feature each, such as peptide ion charge, sequence or a quantification value reported for a file (e.g. light, heavy or ratio).
Since the columns ProteinGroup, StringSequence, PepSequenceID and Charge uniquely identify a row, we can use these to index the rows.
For this we use a language feature called ["anonymous record type"](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/anonymous-records). Here we create a tuple like structure, with the additional feature
that each element of the tuple is named (e.g.: Proteingroup).
*)
let indexedData =
    rawData
    // StringSequence is the peptide sequence
    |> Frame.indexRowsUsing (fun os -> 
            {|
                ProteinGroup=os.GetAs<string>("ProteinGroup"); 
                StringSequence=os.GetAs<string>("StringSequence");
                PepSequenceID=os.GetAs<int>("PepSequenceID");
                Charge=os.GetAs<int>("Charge")
            |}
        )
    |> Frame.dropCol "ProteinGroup"
    |> Frame.dropCol "StringSequence"
    |> Frame.dropCol "PepSequenceID"
    |> Frame.dropCol "Charge"

(***condition:ipynb***)
#if IPYNB
// The effect of our frame manipulation can be observed:
indexedData
// |> Frame.take 10
|> formatAsTable 
|> Chart.Show
#endif // IPYNB
(***hide***)
indexedData |> Frame.take 10 |> fun x -> x.Print()
(***include-fsi-merged-output***)
(**
## Augmenting and filtering the data frame 
The data frame already contains all information needed to perform the analysis, but it could still benefit from 
some quality-of-life upgrades. For this we want to map the Cre-numbers to human readable synonyms.
*)

let pathSyn = Path.Combine[|directory;"downloads/CreToSynonym.txt"|]
downloadFile pathSyn "CreToSynonym.txt" "bio-bte-06-l-7"

let creToSynonym = 
    Frame.ReadCsv(pathSyn,separators="\t")
    |> Frame.indexRowsString "Identifier"
    |> Frame.getCol "Synonym"
    |> Series.mapValues string

let withSynonyms = 
    indexedData
    |> Frame.mapRowKeys (fun k ->
        let synonyms =
            k.ProteinGroup
            |> String.split ';'
            |> Array.map (fun cre -> 
                let cre' = cre.Split('.').[0..1] |> String.concat "."
                match creToSynonym |> Series.tryGet cre' with 
                | Some syn -> syn
                | None -> ""
                )
            |> Array.filter (fun x ->String.isNullOrEmpty x |> not)
            |> String.concat ";"
        {|k with Synonyms=synonyms|}
        )

(***condition:ipynb***)
#if IPYNB
withSynonyms
|> Frame.take 10
|> formatAsTable 
|> Chart.Show
#endif // IPYNB
(***hide***)
withSynonyms |> Frame.take 10 |> fun x -> x.Print()
(***include-fsi-merged-output***)
type Qprot = 
    | CBB
    | PS

(**
Subsequently and finally, we will also encode the specific qConcat protein as a separate feature:
*)

let final = 
    withSynonyms
    |> Frame.mapRowKeys (fun k ->
        let qprot = 
            match k.ProteinGroup |> String.contains "QProt_newCBB", k.ProteinGroup |> String.contains "QProt_newPS" with 
            | true, false  -> Some CBB
            | false, true  -> Some PS 
            | _ -> None  
        {|k with QProt = qprot|}
        )
    // What does this line filter for? Why does this make sense for our analysis?
    |> Frame.filterRows (fun k s -> k.QProt.IsSome)
    |> Frame.mapRowKeys (fun k -> {|k with QProt = k.QProt.Value|})

(***condition:ipynb***)
#if IPYNB
final
|> Frame.take 10
|> formatAsTable 
|> Chart.Show
#endif // IPYNB
(***hide***)
final |> Frame.take 10 |> fun x -> x.Print()
(***include-fsi-merged-output***)
// How many peptide ions did the filter remove? 
(**
## Global quality control.

With our data frame prepared, we want to see on a global scale if our experiment worked.
For this we plot the overall mean of the 14N and 15N quantifications and observe if we can recover our dilution series (15N),
while keeping the analyte to be quantified at a constant level (14N).

Since it comes in handy to simplify the data frame, in this code we will only keep columns that contain a specific identifier, 
such as, "Ratio", "Light", or "Heavy".
*)
let sliceQuantColumns quantColID frame = 
    frame
    |> Frame.filterCols (fun ck os -> ck |> String.contains ("."+quantColID))
    |> Frame.mapColKeys (fun ck -> ck.Split('.') |> Array.item 0)

let ratios = sliceQuantColumns "Ratio" final
let light  = sliceQuantColumns "Light" final
let heavy  = sliceQuantColumns "Heavy" final

/// How did the data frame change, how did the column headers change?
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

/// This function will plot the distribution of column valuesusing boxplots. 
let createBoxPlot f = 
    f
    |> Frame.getNumericCols
    |> Series.map (fun k s -> 
         let x,y =
            s
            |> Series.values 
            |> Seq.map (fun values -> k,values)
            |> Seq.unzip
         Chart.BoxPlot(x,y,Orientation=StyleParam.Orientation.Vertical)         
         )
    |> Series.values
    |> Chart.Combine
    |> Chart.withY_AxisStyle "Ion intensity"
(**
*)
// The function applied to the n14 values, what do you see?

light
|> createBoxPlot

(***hide***)
light |> createBoxPlot |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
// The function applied to the n15 values, what do you see?

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
Finally we have a look at the ratios. Does it make sense to normalize the ratios the same way?
*)

ratios
|> createBoxPlot 
(**
*)
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
(***condition:ipynb***)
#if IPYNB
ratios
|> Frame.transpose
|> Frame.take 10
|> formatAsTable 
|> Chart.Show
#endif // IPYNB
(***hide***)
ratios |> Frame.transpose |> Frame.take 10 |> fun x -> x.Print()
(***include-fsi-merged-output***)
(**
## V. Local quality control. 

Now that we know on a global scale how our experiment worked it might be time to have a look at the details.
First, we want to write a function that allows us to plot all peptides of a protein vs. the dilution used. This way we can identify peptides that
we want to use and those, that seem to be prone to error and should thus be discarded. 
To keep things simple, we apply a filter step at the beginning, which only keeps peptides belonging to one protein and samples measured by one group
in the data frame. What are sources of error? Which peptides do you think should be discarded and why? Which proteins need to be analyzed with extra care?
*)

let initGetQProtAmount qProt =
    match qProt with 
    | CBB -> get15N_CBC_Amount
    | PS  -> get15N_PS_Amount

let plotPeptidesOf (prot:string) (groupID:int) = 
    ratios
    |> Frame.filterRows (fun k s -> k.Synonyms.Contains prot)
    |> Frame.filterCols (fun k s -> getGroupID k = groupID)    
    |> Frame.transpose
    |> Frame.getNumericCols
    |> Series.map (fun pep (values) -> 
        let getQProtAmount = initGetQProtAmount pep.QProt
        let qprotAmounts,ratios,expressionLevel =
            values
            |> Series.map (fun fileName (ratio) -> 
                    let qProtAmount =  getQProtAmount fileName
                    let expressionLevel = getExpressionLevel fileName
                    qProtAmount, ratio, expressionLevel         
                )
            |> Series.values
            |> Seq.unzip3
        Chart.Point(qprotAmounts,ratios,Labels=expressionLevel)
        |> Chart.withTraceName (sprintf "S:%s_C:%i" pep.StringSequence pep.Charge)
        )
    |> Series.values
    |> Chart.Combine

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
plotPeptidesOf "rbcL" 1
(***hide***)
plotPeptidesOf "rbcL" 1 |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
plotPeptidesOf "RBCS2;RBCS1" 2
(***hide***)
plotPeptidesOf "RBCS2;RBCS1" 2 |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
plotPeptidesOf "FBP1" 2
(***hide***)
plotPeptidesOf "FBP1" 2 |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
plotPeptidesOf "FBP2" 2
(***hide***)
plotPeptidesOf "FBP2" 2 |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
plotPeptidesOf "SEBP1" 2
(***hide***)
plotPeptidesOf "SEBP1" 2 |> GenericChart.toChartHTML
(***include-it-raw***)

(**
With the plots at hand we can manipulate the data frame and discard peptides and/or whole files which we do not want to use for 
a absolute protein quantification e.g.:
*)

let ratiosFiltered = 
    ratios
    |> Frame.filterCols (fun k s -> get15N_CBC_Amount k > 0.1 )
(***condition:ipynb***)
#if IPYNB
ratiosFiltered
|> Frame.take 10
|> formatAsTable 
|> Chart.Show
#endif // IPYNB
(***hide***)
ratiosFiltered |> Frame.take 10 |> fun x -> x.Print()
(***include-fsi-merged-output***)
(**
This file can then be saved and used for the next notebook, where we will have a look on the isotopic labeling efficiency and finally calculate absolute protein amounts.
*)