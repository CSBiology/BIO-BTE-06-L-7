
(**
# Deedle Basics

[Deedle](http://bluemountaincapital.github.io/Deedle/index.html)  is an easy to use library for data and time series manipulation and for scientific 
programming. It supports working with structured data frames, ordered and unordered data, as well as time series.

The analysis of your data in the following notebooks will be mostly done in Deedle, so here are some explanations and examples to help you better understand 
the analysis notebooks.

We start by loading our usual nuget packages and the Deedle package.
*)

#r "nuget: Deedle, 2.3.0"
#r "nuget: BioFSharp, 2.0.0-beta4"
#r "nuget: BioFSharp.IO, 2.0.0-beta4"
#r "nuget: BioFSharp.Mz, 0.1.5-beta"
#r "nuget: BIO-BTE-06-L-7_Aux, 0.0.5"
#r "nuget: FSharp.Stats"

#if IPYNB
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: Plotly.NET.Interactive, 2.0.0-beta6"
#endif // IPYNB

open Plotly.NET
open BioFSharp
open BioFSharp.Mz
open BIO_BTE_06_L_7_Aux.FS3_Aux
open BIO_BTE_06_L_7_Aux.Deedle_Aux
open System.IO
open Deedle
open FSharp.Stats

(**
## Deedle Basics
Familiarize yourself with Deedle! Below you will find the source code that was used in the lecture. Create a series yourself that you add to the frame 
'persons' frame.
*)
let firstNames      = Series.ofValues ["Kevin";"Lukas";"Benedikt";"Michael"] 
let coffeesPerWeek  = Series.ofValues [15;12;10;11] 
let lastNames       = Series.ofValues ["Schneider";"Weil";"Venn";"Schroda"]  
let group           = Series.ofValues ["CSB";"CSB";"CSB";"MBS"] 
let persons = 
    Frame.ofColumns(List.zip ["fN";"lN";"g"] [firstNames;lastNames;group])
    |> Frame.addCol "cpw" coffeesPerWeek

#if IPYNB
persons
|> formatAsTable 
#endif // IPYNB

(**
Follow the above scheme and create another frame that is exactly the same, but represents different persons (the frame can be small, e.g. two persons).
Use the function Frame.merge to combine your frame and 'persons'.
Back to the frame 'persons'! In the following you see a series of frame/series manipulations. Write 
comments explaining each step
*)
let coffeePerWeek' :Series<int,int> = Frame.getCol ("cpw") persons 
let groupedByG :Frame<string*int,_> = persons |> Frame.groupRowsBy "g"
let withOutG :Frame<string*int,_> = groupedByG |> Frame.sliceCols ["fN";"lN";"cpw"]
let coffeePerWeek'' :Series<string*int,int>= groupedByG |> Frame.getCol ("cpw")
let coffeePerWeekPerGroup = Series.applyLevel Pair.get1Of2 (Series.values >> Seq.sum) coffeePerWeek''

(**
As our dataset we take the FASTA with Chlamy proteins, select 50 random proteins, and digest them.
The digested peptides are represented using a record type. Deedle frames can be directly constructed from
record types with `Frame.ofRecords`. Alternatively, a character separated file could be used as source for a Frame as well.
*)

let path = Path.Combine[|__SOURCE_DIRECTORY__;"downloads/Chlamy_JGI5_5(Cp_Mp).fasta"|]
downloadFile path "Chlamy_JGI5_5(Cp_Mp).fasta" "bio-bte-06-l-7"

let examplePeptides = 
    path
    |> IO.FastA.fromFile BioArray.ofAminoAcidString
    |> Seq.toArray
    |> Array.take 50
    |> Array.mapi (fun i fastAItem ->
        Digestion.BioArray.digest Digestion.Table.Trypsin i fastAItem.Sequence
        |> Digestion.BioArray.concernMissCleavages 0 0 
        |> Array.map (fun dp ->
            {|
                PeptideSequence = dp.PepSequence
                Protein = fastAItem.Header.Split ' ' |> Array.head
            |}
        )
    )
    |> Array.concat
    |> Array.filter (fun x -> x.PeptideSequence.Length > 5)

let peptidesFrame =
    examplePeptides
    |> Frame.ofRecords

(***condition:ipynb***)
#if IPYNB
peptidesFrame
|> Frame.take 10
|> formatAsTable 
#endif // IPYNB
(***hide***)
peptidesFrame |> Frame.take 10 |> fun x -> x.Print()
(***include-fsi-merged-output***)

(**
As you can see, our columns are named the same as the field of the record type, while our rows are indexed by numbers only. It is often helpful to use a more descriptive
row key. In this case, we can use the peptide sequence for that. **Note** Row keys must be unique. By grouping with "PeptidesSequence", we get the sequence tupled with the index as key. 
The function `Frame.reduceLevel` aggregates the rows now based on the first part of the tuple, the peptide sequence, ignoring the second part of the tuple, the index. 
The aggregator function given to `Frame.reduceLevel` aggregates each column separately.
*)

let pfIndexedSequenceList : Frame<list<AminoAcids.AminoAcid>,string> =
    peptidesFrame
    |> Frame.groupRowsBy "PeptideSequence"
    |> Frame.dropCol "PeptideSequence"
    |> Frame.reduceLevel fst (fun a b -> a + "," + b)

(***condition:ipynb***)
#if IPYNB
pfIndexedSequenceList
|> Frame.take 10
|> formatAsTable 
#endif // IPYNB
(***hide***)
pfIndexedSequenceList |> Frame.take 10 |> fun x -> x.Print()
(***include-fsi-merged-output***)
(**
Our rows are now indexed with the peptide sequences. The peptide sequence is still an aarray of amino acids. For better visibility we can transform it to its string representation. 
For that we can map over our row keys similar to an array and call the function `BioList.toString` on each row key.
*)

let pfIndexedStringSequence =
    pfIndexedSequenceList
    |> Frame.mapRowKeys (fun rc -> rc |> BioList.toString)

(***condition:ipynb***)
#if IPYNB
pfIndexedStringSequence
|> Frame.take 10
|> formatAsTable 
#endif // IPYNB
(***hide***)
pfIndexedStringSequence |> Frame.take 10 |> fun x -> x.Print()
(***include-fsi-merged-output***)
(**
We now have a frame containing information about our peptides. To add additional information we can go back to the peptide array we started with and calculate 
the monoisotopic mass, for example. The monoisotopic mass is tupled with the peptide sequence as string, the same as in our peptide frame. The resulting array
can then be transformed into a `series`
*)

let peptidesAndMasses =
    examplePeptides
    |> Array.distinctBy (fun x -> x.PeptideSequence)
    |> Array.map (fun peptide ->
        // calculate mass for each peptide
        peptide.PeptideSequence |> BioList.toString, BioSeq.toMonoisotopicMassWith (BioItem.monoisoMass ModificationInfo.Table.H2O) peptide.PeptideSequence
        )

let peptidesAndMassesSeries =
    peptidesAndMasses
    |> series

(**
The columns in frames consist of series. Since we now have a series containing our monoisotopic masses, together with the peptide sequence, we can simply add 
it to our frame and give the column a name.
*)

let pfAddedMass =
    pfIndexedStringSequence
    |> Frame.addCol "Mass" peptidesAndMassesSeries

(***condition:ipynb***)
#if IPYNB
pfAddedMass
|> Frame.take 10
|> formatAsTable 
#endif // IPYNB
(***hide***)
pfAddedMass |> Frame.take 10 |> fun x -> x.Print()
(***include-fsi-merged-output***)
(**
Alternatively, we can take a column from our frame, apply a function to it, and create a new frame from the series.
*)

let pfChargedMass =
    pfAddedMass
    |> Frame.getCol "Mass"
    |> Series.mapValues (fun mass -> Mass.toMZ mass 2.)
    |> fun s -> ["Mass Charge 2", s]
    |> Frame.ofColumns

(***condition:ipynb***)
#if IPYNB
pfChargedMass
|> Frame.take 10
|> formatAsTable 
#endif // IPYNB
(***hide***)
pfChargedMass |> Frame.take 10 |> fun x -> x.Print()
(***include-fsi-merged-output***)
(**
The new frame has the same row keys as our previous frame. The information from our new frame can be joined with our old frame by using `Frame.join`.
`Frame.join` is similar to `Frame.addCol`, but can join whole frames at once instead of single columns.
*)

let joinedFrame =
    pfAddedMass
    |> Frame.join JoinKind.Left pfChargedMass

(***condition:ipynb***)
#if IPYNB
joinedFrame
|> Frame.take 10
|> formatAsTable 
#endif // IPYNB
(***hide***)
joinedFrame |> Frame.take 10 |> fun x -> x.Print()
(***include-fsi-merged-output***)