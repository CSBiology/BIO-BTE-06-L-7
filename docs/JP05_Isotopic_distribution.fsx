(**
# JP05 Isotopic Distribution

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=JP05_Isotopic_distribution.ipynb)


1. Isotopic Distribution
    1. Simulating Isotopic Clusters for peptides
    2. Simulating Isotopic Clusters for peptides with stable isotope labeled variant
2. References
*)
(**
## Isotopic Distribution

Peptide signals exhibit a characteristic shape in the mass spectrum that depend on their isotopic profile, which is defined by 
the number of naturally occurring isotopes in the peptide. The occurrence probabilities of natural isotopes are reflected in the mass 
spectrum by the relative heights of the peak series belonging to the respective peptide. The frequency at which natural isotopes occur 
is known and can be used to compute the isotope distribution of a molecule. The isotopic distribution for a given peptide molecule 
C(v)H(w)N(x)O(y)S(z) is described by the following product of polynomials:

![](https://latex.codecogs.com/gif.latex?\large&space;\newline(&space;{}^{12}\textrm{C}&space;&plus;&space;{}^{13}\textrm{C})^{v}&space;\times&space;({}^{1}\textrm{H}&plus;{}^{2}\textrm{H})^{w}&space;\times&space;({}^{14}\textrm{N}&plus;{}^{15}\textrm{N})^{x}\times({}^{16}\textrm{O}&plus;{}^{17}\textrm{O}&space;&plus;&space;{}^{18}\textrm{O})^{y}\newline\times({}^{32}\textrm{S}&plus;{}^{33}\textrm{S}&plus;{}^{34}\textrm{S}&plus;{}^{36}\textrm{S})^{z})

Symbolic expansion of the polynomials results in many product terms, which correspond to different isotopic variants of a molecule. 
Even for molecules of a medium size, the straightforward expansion of the polynomials leads to an explosion regarding the number of product terms. 
Due to this complexity, there was a need to develop algorithms for efficient computation. The different strategies comprise pruning the 
polynomials to discard terms with coefficients below a threshold (Yergey 1983) combined with a recursive 
computation (Claesen et al. 2012), and Fourier Transformation for a more efficient convolution of the isotope distributions of 
individual elements (Rockwood et al. 1995), or rely on dynamic programming (Snider 2007).

> MIDAs (Alves and Yu 2005) is one of the more elaborate algorithms to predict an isotope cluster based on a given peptide sequence. 
> Simulate the isotopic cluster of the peptide sequence ‘PEPTIDES’ and ‘PEPTIDEPEPTIDEPEPTIDEPEPTIDES’ with natural occurring isotope abundances.

*)

#r "nuget: BioFSharp, 2.0.0-beta5"
#r "nuget: BioFSharp.IO, 2.0.0-beta5"
#r "nuget: Plotly.NET, 2.0.0-beta6"

#if IPYNB
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: Plotly.NET.Interactive, 2.0.0-beta6"
#endif // IPYNB

open Plotly.NET
open BioFSharp
(**

## Simulating Isotopic Clusters for peptides

We will use two artificial peptide sequences and translate them into their elemental composition to simulate their isotopic clusters. 
Therefore, we first define a function that maps from a peptide sequence to its formula:
*)

// Code-Block 1

// create chemical formula for amino acid and add water to reflect hydrolysed state in mass spectrometer
let toFormula bioseq =  
    bioseq
    |> BioSeq.toFormula
    // peptides are hydrolysed in the mass spectrometer, so we add H2O
    |> Formula.add Formula.Table.H2O

(**
Next, we will apply our function to receive the elemental composition or chemical formula of the peptides.
*)

// Code-Block 2

// translate single letter code into amino acids and create chemical formula of it.
let peptide_short = 
    "PEPTIDES" 
    |> BioSeq.ofAminoAcidString
    |> toFormula
    
let peptide_long  = 
    "PEPTIDEPEPTIDEPEPTIDEPEPTIDES" 
    |> BioSeq.ofAminoAcidString
    |> toFormula
    
let peptide_shortString =
    peptide_short 
    |> Formula.toString


let peptide_longString =
    peptide_long 
    |> Formula.toString

(*** include-value:peptide_shortString ***)

(*** include-value:peptide_longString ***)

(**
Additionally, we need a function that maps from Formula (and charge) to the isotopic distribution. Here, we 
can use `IsotopicDistribution.MIDA.ofFormula` from the BioFSharp library. However, for convenience 
(to use the same parameter twice), we define our function `generateIsotopicDistribution`:
*)

// Code-Block 3

// Predicts an isotopic distribution of the given formula at the given charge, 
// normalized by the sum of probabilities, using the MIDAs algorithm
let generateIsotopicDistribution (charge:int) (f:Formula.Formula) =
    IsotopicDistribution.MIDA.ofFormula 
        IsotopicDistribution.MIDA.normalizeByMaxProb
        0.01
        0.005
        charge
        f
        
// create pattern for peptide_short
let isoPattern_peptide_short = 
    generateIsotopicDistribution 1 peptide_short

// create pattern for peptide_long
let isoPattern_peptide_long = 
    generateIsotopicDistribution 1 peptide_long
    
isoPattern_peptide_long

(*** include-it ***)


// Code-Block 4

// create one chart for both, short and long peptide isotopic patterns.     
let isoPatternChart = 
    [
        Chart.Column(isoPattern_peptide_short,Name= "peptide_short" )
        |> Chart.withX_AxisStyle ("m/z",MinMax=(885.,895.))
        Chart.Column(isoPattern_peptide_long,Name= "peptide_long" )
        |> Chart.withX_AxisStyle ("m/z",MinMax=(3230., 3240.))
    ]
    |> Chart.Stack 2
    |> Chart.withSize (900.,600.)
    |> Chart.withTitle "Isotopeclusters"
    |> Chart.withY_AxisStyle "intensity"
isoPatternChart

(***hide***)
isoPatternChart |> GenericChart.toChartHTML
(***include-it-raw***)

(**
## Simulating Isotopic Clusters for peptides with stable isotope labeled variant

In addition to the natural occurring isotopic distribution, the field of proteomics has benefited greatly from the ability to 
introduce stable isotopes into peptide sequences. So called isotopic labeling refers to the introduction of a naturally low-abundance 
isotope of carbon, nitrogen, hydrogen and, in some cases, oxygen, into a peptide sequence. The isotopes commonly used are 13C, 
15N, 2H (deuterium) and 18O with natural abundances of 1.10%, 0.366%, 0.015% and 0.200%, 
respectively (Becker 2008). Therefore, the introduction of these isotopes into a peptide sequence can be detected by 
most modern mass spectrometers leading to a respective mass shift and the ability to separate the same peptide species within the same run.

> MIDAs (Alves and Yu 2005) is also able to predict isotope clusters with altered isotope abundances. Simulate the isotopic cluster 
> of the peptide sequence ‘PEPTIDES’ and ‘PEPTIDEPEPTIDEPEPTIDEPEPTIDES’ with stable isotopes 15N labeling. 

Therefore, we define a function called `label`. The function maps from a formula to a formula with exchangen nitrogen isotopes. 
(Attention: Don't get confused a formula is just a FSharpMap.) 
*)

// Code-Block 5

/// returns a function that replaces the nitrogen atoms in a formula
/// with the 15N isotope
let label formula =
    Formula.replaceElement formula Elements.Table.N Elements.Table.Heavy.N15

(** *)

// Code-Block 6

let N15_peptide_short = 
    "PEPTIDES" 
    |> BioSeq.ofAminoAcidString
    |> toFormula
    |> label

let N15_peptide_long  = 
    "PEPTIDEPEPTIDEPEPTIDEPEPTIDES" 
    |> BioSeq.ofAminoAcidString
    |> toFormula
    |> label

//result: N15_peptide_short
N15_peptide_short
(*** include-value:N15_peptide_short ***)

//result: N15_peptide_long
N15_peptide_long
(*** include-value:N15_peptide_long ***)

(** *)

// Code-Block 7

// create pattern for N15_peptide_short
let N15_isoPattern_peptide_short = 
    generateIsotopicDistribution 1 N15_peptide_short

// create pattern for N15_peptide_long
let N15_isoPattern_peptid_long = 
    generateIsotopicDistribution 1 N15_peptide_long

(***include-value:N15_isoPattern_peptide_short***)
(***include-value:N15_isoPattern_peptid_long***)

(** *)

// Code-Block 8

// Create two charts. Each with the related N14 and N15 isotopic clusters. Then stack them two one unit.
let isoPatternChart2 = 
    [
        [
            Chart.Column(isoPattern_peptide_short,Name= "peptide_short" )
            Chart.Column(N15_isoPattern_peptide_short,Name= "N15_peptide_short" )
        ] 
        |> Chart.Combine 
        |> Chart.withX_AxisStyle ("m/z",MinMax=(885., 905.0))

        [
            Chart.Column(isoPattern_peptide_long,Name= "peptide_long" )
            Chart.Column(N15_isoPattern_peptid_long,Name= "N15_peptide_long" )            
        ] 
        |> Chart.Combine 
        |> Chart.withX_AxisStyle ("m/z",MinMax=(3230.0, 3270.0))
    ]
    |> Chart.Stack 2
    |> Chart.withTitle "Isotopeclusters"
    |> Chart.withY_AxisStyle "intensity"
isoPatternChart2
(***hide***)
isoPatternChart2 |> GenericChart.toChartHTML
(***include-it-raw***)

(**
## References

25. Yergey, J. A. A General-Approach to Calculating Isotopic Distributions for Mass-Spectrometry. Int J Mass Spectrom 52, 337–349; 10.1016/0020-7381(83)85053-0 (1983).
26. Claesen, J., Dittwald, P., Burzykowski, T. & Valkenborg, D. An efficient method to calculate the aggregated isotopic distribution and exact center-masses. Journal of the American Society for Mass Spectrometry 23, 753–763; 10.1007/s13361-011-0326-2 (2012).
27. Rockwood, A. L., Vanorden, S. L. & Smith, R. D. Rapid Calculation of Isotope Distributions. Anal Chem 67, 2699–2704; 10.1021/Ac00111a031 (1995).
28. Snider, R. K. Efficient calculation of exact mass isotopic distributions. Journal of the American Society for Mass Spectrometry 18, 1511–1515; 10.1016/j.jasms.2007.05.016 (2007).
29. Alves, G. & Yu, Y. K. Robust accurate identification of peptides (RAId). deciphering MS2 data using a structured library search with de novo based statistics. Bioinformatics 21, 3726–3732; 10.1093/bioinformatics/bti620 (2005).
30. Becker, G. W. Stable isotopic labeling of proteins for quantitative proteomic applications. Brief Funct Genomic Proteomic 7, 371–382; 10.1093/bfgp/eln047 (2008).
*)

