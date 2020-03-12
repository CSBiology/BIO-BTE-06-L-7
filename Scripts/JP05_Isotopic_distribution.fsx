
#load @"../IfSharp/Paket.Generated.Refs.fsx"

open BioFSharp
open BioFSharp.IO
open FSharp.Stats
open FSharp.Plotly
open AminoProperties
open BioFSharp
open BioFSharp.Elements
open BioFSharp.Formula


/// Teil 1

///Predicts an isotopic distribution of the given formula at the given charge, normalized by the sum of probabilities, using the MIDAs algorithm
let generateIsotopicDistribution (charge:int) (f:Formula.Formula) =
    IsotopicDistribution.MIDA.ofFormula 
        IsotopicDistribution.MIDA.normalizeByMaxProb
        0.01
        0.005
        charge
        f

///returns a function that replaces the nitrogen atoms in a formula with a nitrogen with the given probability of being the 15N isotope
let initlabelN15Partial n15Prob =
    ///Diisotopic representation of nitrogen with abundancy of N14 and N15 swapped
    let n14Prob = 1. - n15Prob
    let N15 = Di (createDi "N15" (Isotopes.Table.N15,n15Prob) (Isotopes.Table.N14,n14Prob) )
    fun f -> Formula.replaceElement f Elements.Table.N N15

let toFormula bioseq =  
    bioseq
    |> BioSeq.toFormula
    |> Formula.add Formula.Table.H2O

let peptide_short = 
    "PEPTIDES" 
    |> BioSeq.ofAminoAcidString
    |> toFormula

let peptide_long  = 
    "PEPTIDEPEPTIDEPEPTIDEPEPTIDES" 
    |> BioSeq.ofAminoAcidString
    |> toFormula

let isoPattern_peptide_short = 
    generateIsotopicDistribution 1 peptide_short

let isoPattern_peptide_long = 
    generateIsotopicDistribution 1 peptide_long

[
    Chart.Point isoPattern_peptide_short 
    |> Chart.withTraceName "shortPep"
    Chart.Point isoPattern_peptide_long 
    |> Chart.withTraceName "longPep"
]
|> Chart.Combine
|> Chart.Show

/// Teil 2

let N15_peptide_short = 
    "PEPTIDES" 
    |> BioSeq.ofAminoAcidString
    |> toFormula
    |> initlabelN15Partial 0.999999

let N15_peptide_long  = 
    "PEPTIDEPEPTIDEPEPTIDEPEPTIDES" 
    |> BioSeq.ofAminoAcidString
    |> toFormula
    |> initlabelN15Partial 0.999999

let N15_isoPattern_peptide_short = 
    generateIsotopicDistribution 1 N15_peptide_short

let N15_isoPattern_peptid_long = 
    generateIsotopicDistribution 1 N15_peptide_long

[
    Chart.Point isoPattern_peptide_short 
    |> Chart.withTraceName "shortPep"
    Chart.Point isoPattern_peptide_long 
    |> Chart.withTraceName "longPep"
    Chart.Point N15_isoPattern_peptide_short 
    |> Chart.withTraceName "N15shortPep"
    Chart.Point N15_isoPattern_peptid_long 
    |> Chart.withTraceName "N15longPep"
]
|> Chart.Combine
|> Chart.Show

