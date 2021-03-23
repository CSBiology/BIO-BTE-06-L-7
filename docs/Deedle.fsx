
(**
## Deedle Basics

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
#r "nuget: BIO-BTE-06-L-7_Aux, 0.0.1"
#r "nuget: FSharp.Stats"

#if IPYNB
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: Plotly.NET.Interactive, 2.0.0-beta6"
#endif // IPYNB

open Plotly.NET
open BioFSharp
open BioFSharp.Mz
open BIO_BTE_06_L_7_Aux.FS3_Aux
open System.IO
open Deedle
open FSharp.Stats

(**
As our dataset we take the FASTA with Chlamy proteins, select 10 random proteins, and digest them.
The digested peptides are represented using a record type. Deedle frames can be directly constructed from
record types with `Frame.ofRecords`. Alternatively, a character separated file could be used as source for a Frame as well.
*)

let path = Path.Combine[|__SOURCE_DIRECTORY__;"downloads/Chlamy_JGI5_5(Cp_Mp).fasta"|]
downloadFile path "Chlamy_JGI5_5(Cp_Mp).fasta" "bio-bte-06-l-7"

let samplePeptides () = 
    path
    |> IO.FastA.fromFile BioArray.ofAminoAcidString
    |> Seq.toArray
    |> shuffleFisherYates
    |> Array.take 1
    |> Array.mapi (fun i fastAItem ->
        Digestion.BioArray.digest Digestion.Table.Trypsin i fastAItem.Sequence
        |> Digestion.BioArray.concernMissCleavages 0 0
        )
    |> Array.concat
    |> Array.distinctBy (fun x -> x.PepSequence)

let peptides = samplePeptides()

let peptidesFrame =
    peptides
    |> Frame.ofRecords

peptidesFrame.Print()

(**
As you can see, our columns are named the same as the field of the record type, while our rows are indexed by numbers only. It is often helpful to use a more descriptive
row key. In this case, we can use the peptide sequence for that. **Note** Row keys must be unique. In `peptides` we made sure every peptide occurs onyl once
in our array, so we can take it as row key.
*)

let pfIndexedSequenceList =
    peptidesFrame
    |> Frame.indexRowsUsing (fun os -> 
        os.GetAs<list<AminoAcids.AminoAcid>>("PepSequence")
    )
    // Since our Frame is now indexed by "PepSequence", we don't need the column anymore and can drop it
    |> Frame.dropCol "PepSequence"

pfIndexedSequenceList.Print()

(**
Our rows are now indexed with the peptide sequences. The peptide sequence is still an aarray of amino acids. For better visibility we can transform it to its string representation. 
For that we can map over our row keys similar to an array and call the function `BioList.toString` on each row key.
*)

let pfIndexedStringSequence =
    pfIndexedSequenceList
    |> Frame.mapRowKeys (fun rc -> rc |> BioList.toString)

(**
We now have a frame containing information about our peptides. To add additional information we can go back to the peptide array we started with and calculate 
the monoisotopic mass, for example. The monoisotopic mass is tupled with the peptide sequence as string, the same as in our peptide frame. The resulting array
can then be transformed into a `series`
*)

let peptidesAndMasses =
    peptides
    |> Array.map (fun peptide ->
        // calculate mass for each peptide
        peptide.PepSequence |> BioList.toString, BioSeq.toMonoisotopicMassWith (BioItem.monoisoMass ModificationInfo.Table.H2O) peptide.PepSequence
        )

let peptidesAndMassesSeries =
    peptidesAndMasses
    |> series

(**
The columns in frames are called series. Since we now have a series containing our monoisotopic masses, together with the peptide sequence, we can simply add 
it to our frame and give the column a name.
*)

let pfAddedMass =
    pfIndexedStringSequence
    |> Frame.addCol "Mass" peptidesAndMassesSeries

pfAddedMass.Print()

(**
Similarily, we can take a column from our frame, apply a function to it, and add it under a new name.
*)

let pfChargedMass =
    pfAddedMass
    |> Frame.getCol "Mass"
    |> Series.mapValues (fun mass -> Mass.toMZ mass 2.)
    |> fun chargedMass ->
        pfAddedMass
        |> Frame.addCol "Mass Charge 2" chargedMass

pfChargedMass.Print()


