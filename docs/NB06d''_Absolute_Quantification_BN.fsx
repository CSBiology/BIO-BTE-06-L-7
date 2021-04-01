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
# NB06d Absolute Quantification

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=NB06d''_Absolute_Quantification_SDS.ipynb_BN.ipynb)

[Download Notebook](https://github.com/CSBiology/BIO-BTE-06-L-7/releases/download/NB06b'_NB06b''_NB06c'_NB06c''_NB06d'_NB06d''/NB06d''_Absolute_Quantification_SDS.ipynb_BN.ipynb)

Finally, after careful peptide ion selection, quality control and assuring that our label efficiency allows accurate for quantifications, we can start to
calculate protein abundancies. Since we start again by getting access to our data and its description, this notebook will start off familiar!


## I. Reading the data
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
let filePath = System.IO.Path.Combine [|__SOURCE_DIRECTORY__ + "/downloads/qualityControlResult_BN.txt"|]

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
## II. Extracting ratios and calculating comparisons.

Next, we want to look at the stoichiometry between rbcL and rbcS. First, we extract the ratios from our curated results file and plot them.
*)

let sliceQuantColumns quantColID frame = 
    frame
    |> Frame.filterCols (fun ck os -> ck |> String.contains ("." + quantColID))
    |> Frame.mapColKeys (fun ck -> ck.Split('.') |> Array.item 0)


let ratios = sliceQuantColumns "Ratio" qConcatDataFiltered

ratios
|> Frame.getRow {|Charge = 2; PepSequenceID = 1457965; ProteinGroup = "QProt_newPS;Cre02.g120150.t1.2;Cre02.g120100.t1.2"; StringSequence = "AFPDAYVR"; Synonyms = "RBCS2;RBCS1"|}

let ratiosRbcL =
    ratios
    |> Frame.filterRows (fun r s -> r.Synonyms |> String.contains "rbcL")

let ratiosRbcS =
    ratios
    |> Frame.filterRows (fun r s -> r.Synonyms |> String.contains "RBCS")

let medianRbcL =
    ratiosRbcL
    |> Frame.getNumericCols
    |> Series.mapValues Stats.median
    |> Series.observations

let medianRbcS =
    ratiosRbcS
    |> Frame.getNumericCols
    |> Series.mapValues Stats.median
    |> Series.observations

(***condition:ipynb***)
#if IPYNB
[
    Chart.Column (medianRbcL)
    |> Chart.withTraceName "rbcL"
    Chart.Column (medianRbcS)
    |> Chart.withTraceName "rbcS"
]
|> Chart.Combine
#endif // IPYNB


(**
Since we are interested in the ratio between rbcL and rbcS, we just divide the rbcL ratio through the ratio of rbcS and compare the result with values from the literature.
*)

(***condition:ipynb***)
#if IPYNB
(medianRbcL, medianRbcS)
||> Seq.map2 (fun (s1,v1) (s2,v2) -> s1, v1 / v2)
|> Chart.Column
#endif // IPYNB


(**
Do your results differ from the literature? Why? Discuss.
*)