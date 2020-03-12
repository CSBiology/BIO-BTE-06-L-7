
#load @"../IfSharp/Paket.Generated.Refs.fsx"

open BioFSharp
open FSharp.Plotly
open BioFSharp.Elements


/// Teil 1

let createBarChart inputData name =
    let x,y = inputData |> List.unzip
    let column =
        let dyn = Trace("bar")
        dyn?x <- x
        dyn?y <- y
        dyn?name <- name
        dyn?textposition <- "auto"
        dyn?textposition <- "outside"
        dyn?constraintext <- "inside"
        dyn?fontsize <- 20.
        dyn?width <- 0.5
        dyn?opacity <- 0.8
        dyn
    GenericChart.ofTraceObject column
    |> Chart.withX_AxisStyle("m/z")

let getMinOfIsotopeX (isoList:(float*_) list) =
    isoList |> (List.unzip >> fst >> List.min >> float) |> fun x -> x - 10.

let getMaxOfIsotopeX (isoList:(float*_) list) =
    isoList |> (List.unzip >> fst >> List.max >> float) |> fun x -> x + 10.

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


let isotopicClusterChart = 
    let min_short,max_short =
        getMinOfIsotopeX isoPattern_peptide_short, getMaxOfIsotopeX isoPattern_peptide_short
    let min_long,max_long =
        getMinOfIsotopeX isoPattern_peptide_long, getMaxOfIsotopeX isoPattern_peptide_long
    [
        createBarChart isoPattern_peptide_short "peptide_short"
        |> Chart.withX_AxisStyle ("m/z",MinMax=(min_short,max_short))
        createBarChart isoPattern_peptide_long "peptide_long"
        |> Chart.withX_AxisStyle ("m/z",MinMax=(min_long, max_long))
    ]
    |> Chart.Stack 2
    |> Chart.withTitle "Isotopeclusters"
    |> Chart.withY_AxisStyle "intensity"
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


let isotopicClusterChartWith15N =
    let min_short,max_short =
        getMinOfIsotopeX isoPattern_peptide_short, getMaxOfIsotopeX N15_isoPattern_peptide_short
    let min_long,max_long =
        getMinOfIsotopeX isoPattern_peptide_long, getMaxOfIsotopeX N15_isoPattern_peptid_long
    [
        [
            createBarChart isoPattern_peptide_short "peptide_short"
            createBarChart N15_isoPattern_peptide_short "N15_peptide_short"
        ] 
        |> Chart.Combine 
        |> Chart.withX_AxisStyle ("m/z",MinMax=(min_short, max_short))
        [
            createBarChart isoPattern_peptide_long "peptide_long"
            createBarChart N15_isoPattern_peptid_long "N15_peptide_long"
        ] 
        |> Chart.Combine 
        |> Chart.withX_AxisStyle ("m/z",MinMax=(min_long, max_long))
    ]
    |> Chart.Stack 2
    |> Chart.withTitle "Isotopeclusters"
    |> Chart.withY_AxisStyle "intensity"
    |> Chart.Show

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////      JP Isotopic_distribution        //////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////