(**
# JP06 Retention time and scan time

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=JP06_Retention_time_and_scan_time.ipynb)

1. Retention time and scan time
    1. m/z calculation of the digested peptides
    2. Determination of peptide hydrophobicity
*)

(**



## Retention time and scan time

In general, peptides are separated by one or more steps of liquid chromatography (LC). The retention time (RT) is the time when the measured 
peptides were eluting from the column and is therefore influenced by the physicochemical interaction of the particular peptide with the 
column material. Scan time is basically synonym to retention time, but more from the point of view of the device.

The aim of this notebook is to understand that even though peptides are roughly separated by the LC, multiple peptides elute at the same 
retention time and are recorded within one MS1 spectrum. Here, we will simulate a MS1 spectrum by random sampling from 
our previously generated peptide-mass distribution. Further, we will try to improve our simulation by incorporating information about the peptide 
hydrophobicity. It is a only a crude model, but considers the fact that less hydrophobic peptides elute faster from the 13C LC column.

As always, we start by loading our famous libraries.
*)

#r "nuget: FSharp.Stats, 0.4.0"
#r "nuget: BioFSharp, 2.0.0-beta5"
#r "nuget: BioFSharp.IO, 2.0.0-beta5"
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: BIO-BTE-06-L-7_Aux, 0.0.1"

#if IPYNB
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: Plotly.NET.Interactive, 2.0.0-beta6"
#endif // IPYNB

open BioFSharp
open Plotly.NET
open BioFSharp.Elements
open BIO_BTE_06_L_7_Aux
open FS3_Aux
open Retention_time_and_scan_time_Aux
open System.IO

open FSharp.Stats

(**
## m/z calculation of the digested peptides

I think you remember the protein digestion process from the privious notebook (see: *JP04_Digestion_and_mass_calculation.ipynb* ). This time we also remember the peptide sequence, because we need it later for hydrophobicity calculation. 
*)

// Code-Block 1

let directory = __SOURCE_DIRECTORY__
let path = Path.Combine[|directory;"downloads/Chlamy_JGI5_5(Cp_Mp).fasta"|]
downloadFile path "Chlamy_JGI5_5(Cp_Mp).fasta" "bio-bte-06-l-7"
// with /../ we navigate a directory 
path

let peptideAndMasses = 
    path
    |> IO.FastA.fromFile BioArray.ofAminoAcidString
    |> Seq.toArray
    |> Array.mapi (fun i fastAItem ->
        Digestion.BioArray.digest Digestion.Table.Trypsin i fastAItem.Sequence
        |> Digestion.BioArray.concernMissCleavages 0 0
        )
    |> Array.concat
    |> Array.map (fun peptide ->
        // calculate mass for each peptide
        peptide.PepSequence, BioSeq.toMonoisotopicMassWith (BioItem.monoisoMass ModificationInfo.Table.H2O) peptide.PepSequence
        )

peptideAndMasses |> Array.head

(***include-it***)

(**
Calculate the single and double charged m/z for all peptides and combine both in a single collection.
*)

// Code-Block 2

// calculate m/z for each peptide z=1
let singleChargedPeptides =
    peptideAndMasses
    // we only consider peptides longer than 6 amino acids 
    |> Array.filter (fun (peptide,ucMass) -> peptide.Length >=7)
    |> Array.map (fun (peptide,ucMass) -> peptide, Mass.toMZ ucMass 1.) 

// calculate m/z for each peptide z=2
let doubleChargedPeptides =
    peptideAndMasses
    // we only consider peptides longer than 6 amino acids 
    |> Array.filter (fun (peptide,ucMass) -> peptide.Length >=7)
    |> Array.map (fun (peptide,ucMass) -> peptide, Mass.toMZ ucMass 2.) 

// combine this two    
let chargedPeptides =
    Array.concat [singleChargedPeptides;doubleChargedPeptides]


chargedPeptides.[1]

(***include-it***)

(**
Now, we can sample our random "MS1" spectrum from this collection of m/z.
*)

// Code-Block 3

// initialze a random generator 
let rnd = new System.Random()

// sample n random peptides from all Chlamydomonas reinhardtii peptides
let chargedPeptideChar =
    Array.sampleWithOutReplacement rnd chargedPeptides 100
    // we only want the m/z
    |> Array.map (fun (peptide,mz) -> mz,1.) 
    |> Chart.Column
    |> Chart.withX_AxisStyle("m/z", MinMax=(0.,3000.))
    |> Chart.withY_AxisStyle ("Intensity", MinMax=(0.,1.3))
    |> Chart.withSize (900.,400.)
chargedPeptideChar
(***hide***)
chargedPeptideChar |> GenericChart.toChartHTML
(***include-it-raw***)

(**

This looks quite strange. I think you immediately see that we forgot about our isotopic cluster. A peptide doesn’t produce a single peak, 
but a full isotopic cluster. Therefore, we use our convenience function from the previous notebook 
(see: *JP05_Isotopic_distribution.ipynb* ).

*)

// Code-Block 4

// Predicts an isotopic distribution of the given formula at the given charge, 
// normalized by the sum of probabilities, using the MIDAs algorithm
let generateIsotopicDistribution (charge:int) (f:Formula.Formula) =
    IsotopicDistribution.MIDA.ofFormula 
        IsotopicDistribution.MIDA.normalizeByMaxProb
        0.01
        0.005
        charge
        f
    |> List.toArray
        
generateIsotopicDistribution

// Code-Block 5

let peptidesAndMassesChart =
    // sample n random peptides from all Chlamydomonas reinhardtii peptides
    Array.sampleWithOutReplacement rnd peptideAndMasses 500
    |> Array.map (fun (peptide,mz) -> 
            peptide
            |> BioSeq.toFormula
            // peptides are hydrolysed in the mass spectrometer, so we add H2O
            |> Formula.add Formula.Table.H2O
            )
    |> Array.collect (fun formula -> 
        [
            // generate single charged iones 
            generateIsotopicDistribution 1 formula
            // generate double charged iones 
            generateIsotopicDistribution 2 formula
        ] |> Array.concat
        )
    |> Chart.Column
    |> Chart.withX_AxisStyle("m/z", MinMax=(0.,3000.))
    |> Chart.withY_AxisStyle ("Intensity", MinMax=(0.,1.3))
    |> Chart.withSize (900.,400.)
peptidesAndMassesChart
// HINT: zoom in on peptides

(***hide***)
peptidesAndMassesChart |> GenericChart.toChartHTML
(***include-it-raw***)

(**
## Determination of peptide hydrophobicity

In a MS1 scan, peptides don't appear randomly. They elute according to their hydrophobicity and other physicochemical properties 
from the LC.

To more accurately represent a MSU+00B9 spectrum, we determine the hydrophobicity of each peptide. Therefore, we first need a function 
that maps from sequence to hydrophobicity.
*)

// Code-Block 6

open BioFSharp.AminoProperties

// first, define a function that maps from amino acid to hydophobicity
let getHydrophobicityIndex =
    BioFSharp.AminoProperties.initGetAminoProperty AminoProperty.HydrophobicityIndex
    
// second, use that function to map from peptide sequence to hydophobicity
let toHydrophobicity (peptide:AminoAcids.AminoAcid[]) =
    peptide
    |> Array.map AminoAcidSymbols.aminoAcidSymbol
    |> AminoProperties.ofWindowedBioArray 3 getHydrophobicityIndex
    |> Array.average

toHydrophobicity

// Code-Block 7

let peptidesFirst200 = 
    chargedPeptides 
    // now we sort according to hydrophobicity
    |> Array.sortBy (fun (peptide,mass) ->   
        peptide
        |> Array.ofList
        |> toHydrophobicity
        )
    |> Array.take 200

peptidesFirst200 |> Array.head

(***include-it***)

(**
Now, we need to generate the isotopic cluster again and visualize afterwards.
*)

// Code-Block 8

let peptidesFirst200Chart =
    peptidesFirst200
    |> Array.map (fun (peptide,mz) -> 
            peptide
            |> BioSeq.toFormula
            // peptides are hydrolysed in the mass spectrometer, so we add H2O
            |> Formula.add Formula.Table.H2O
            )
    |> Array.collect (fun formula -> 
        [
            // generate single charged iones 
            generateIsotopicDistribution 1 formula
            // generate double charged iones 
            generateIsotopicDistribution 2 formula
        ] |> Array.concat
        )
    // Display
    |> Chart.Column
    |> Chart.withX_AxisStyle("m/z", MinMax=(0.,3000.))
    |> Chart.withY_AxisStyle ("Intensity", MinMax=(0.,1.3))
    |> Chart.withSize (900.,400.)
peptidesFirst200Chart
// HINT: zoom in on peptides

(***hide***)
peptidesFirst200Chart |> GenericChart.toChartHTML
(***include-it-raw***)