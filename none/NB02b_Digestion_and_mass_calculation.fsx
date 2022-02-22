(** 
# NB02b Digestion and mass calculation

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=NB02b_Digestion_and_mass_calculation.ipynb)

[Download Notebook](https://github.com/CSBiology/BIO-BTE-06-L-7/releases/download/NB02a_NB02b_NB02c/NB02b_Digestion_and_mass_calculation.ipynb)

1. Digestion and mass calculation
    2. Accessing the protein sequences of *Chlamydomonas reinhardtii*
    3. Amino acid distribution for *C. reinhardtii*
4. Calculating the molecular weight for peptides
    5. *In silico* digestion of FASTA proteins with trypsin
    6. Calculating peptide masses
    7. Calculating peptide masses for charge 2
6. References
7. Questions

*)

(** 
## Digestion and mass calculation

The most widely applied method for protein digestion involves the use of enzymes. Many proteases are available for this purpose, 
each having their own characteristics in terms of specificity, efficiency and optimum digestion conditions. Trypsin is most widely 
applied in bottom-up proteomics and and has a very high degree of specificity, cleaving the peptide bonds C-terminal to the basic residues 
Lys and Arg, except when followed by Pro (Burkhart et al. 2012). In general, Lys and Arg are relatively abundant amino acids and are 
usually well distributed throughout a protein (Switzar et al. 2013). This leads to tryptic peptides with an average length of ∼14 amino 
acids that carry at least two positive charges, which is ideally suited for CID-MS analysis (Burkhart et al. 2012).

Using *in silico* analysis, we want to confirm that the general properties of trypsin digestion also apply for the 
proteome of *Chlamydomonas reinhardtii* . First, we load the proteome of *Chlamydomonas* in standard fastA format. 
Amino acid composition of the proteome is simply counting each amino acid occurrence and can be visualized by a histogram:
*)

#r "nuget: BioFSharp, 2.0.0-beta5"
#r "nuget: BioFSharp.IO, 2.0.0-beta5"
#r "nuget: Plotly.NET, 2.0.0-preview.16"
#r "nuget: BIO-BTE-06-L-7_Aux, 0.0.1"

#if IPYNB
#r "nuget: Plotly.NET.Interactive, 2.0.0-preview.16"
#endif // IPYNB

open Plotly.NET
open BioFSharp
open BIO_BTE_06_L_7_Aux.FS3_Aux
open System.IO

(**
## Accessing the protein sequences of *Chlamydomonas reinhardtii*

FASTA is a standardized text format, containing gene or protein sequence information. Such FASTAs can be donwloaded 
from [UniProt](https://www.uniprot.org/proteomes/UP000006906) for example.

To gain informations about the amino acid composition of *C. reinhardtii*, we need information about the proteome 
of *Chlamydomonas*, which is saved in the .fasta file we are accessing below.
*)

// __SOURCE_DIRECTORY__ returns the directory in which the current notebook is located
let directory = __SOURCE_DIRECTORY__
let path = Path.Combine [|directory; "downloads/Chlamy_JGI5_5(Cp_Mp).fasta"|]
downloadFile path "Chlamy_JGI5_5(Cp_Mp).fasta" "bio-bte-06-l-7"
// with /../ we navigate a directory 
path

(*** include-it ***)

(**
Functions to read information from FASTA files exist in the [BioFSharp](https://csbiology.github.io/BioFSharp/) library.
*)

let sequences = 
    path
    |> IO.FastA.fromFile BioArray.ofAminoAcidString
    |> Seq.toArray
    
// Display the first element in the array collection
sequences |> Array.head

(*** include-it ***)

(**

## Amino acid distribution for *C. reinhardtii*

To count the amino acid composition, we take the sequence of every protein and count the occurences of each amino acid
*)

let aminoAcidDistribution =
    sequences
    // only access Sequence from each array element.
    |> Array.collect (fun fastAItem -> fastAItem.Sequence)
    // count each occurence of all amino acids. 
    |> Array.countBy id
    
aminoAcidDistribution

let aaDistributionHis =
    aminoAcidDistribution
    |> Array.map (fun (name,count) -> string name, count)
    // sort by number of occurences
    |> Array.sortByDescending snd
    // create chart
    |> Chart.Column
    // style chart
    |> Chart.withYAxisStyle "Count"
    |> Chart.withSize (650.,600.)
    |> Chart.withTitle "Amino Acid composition of the <i>Chlamydomonas reinhardtii</i> proteome"

aaDistributionHis

(***hide***)
aaDistributionHis |> GenericChart.toChartHTML
(***include-it-raw***)    

(**

## Calculating the molecular weight for peptides

The molecular weight M of a peptide may be estimated by calculating the equation for the molecular weight of a peptide: 

![](https://latex.codecogs.com/png.latex?M&space;=&space;M_{N}&plus;M_{C}\sum_{i=0}^{n}N_{i}M_{i})

where N(i) are the number, and M(i) the average residue molecular weights, of the amino acids. M(N) + M(C) 
are added to the total in order to account for the termini: H at the N-terminus and OH at the C-terminus. (Remark: if the termini are modified, 
these additions are replaced by those of the modifiers.)

The distribution of all molecular weights for the peptides resulting from the previous proteome digest can be calculated and visualized using a histogram chart:
*)

(**
## *In silico* digestion of FASTA proteins with trypsin

To gain information about the peptide sequences of each protein, we have to compute the digested sequence, A digest function with 
variable protease exists in BioFSharp.
*)

let digestedProteins =
    // sequences is the fasta data
    sequences
    |> Array.mapi (fun i fastAItem ->
        // in silico digestion
        Digestion.BioArray.digest Digestion.Table.Trypsin i fastAItem.Sequence
        |> Digestion.BioArray.concernMissCleavages 0 1
    )
    |> Array.concat
    
digestedProteins |> Array.head

(*** include-it ***)

(**
## Calculating peptide masses

We calculate the mass of each peptide by calculating the monoisotopic mass of each amino acid and adding the weight 
of an H(2)O to each peptide weight.
*)

let chartDigestedProteins =
    digestedProteins
    |> Array.map (fun peptide ->
        // calculate mass for each peptide
        BioSeq.toMonoisotopicMassWith (BioItem.monoisoMass ModificationInfo.Table.H2O) peptide.PepSequence
        )
    |> Array.filter (fun x -> x < 3000.)
    // visualize distribution of all peptide masses < 3000 Da
    |> fun masses -> Chart.Histogram(data = masses, orientation = StyleParam.Orientation.Vertical, NBinsX = 100)
    |> Chart.withXAxisStyle (title = "Mass [Da]", MinMax = (0., 3000.))
    |> Chart.withYAxisStyle "Count"

chartDigestedProteins
(***hide***)
chartDigestedProteins |> GenericChart.toChartHTML
(***include-it-raw***) 
(**
## Calculating peptide masses for charge 2

However, in mass spectrometry we are only able to detect ions. Therefore, the measurements report the mass-to-charge ratio. 
The abbreviation m/z (m = mass; z = charge) is used to denote the dimensionless quantity formed by dividing the molecular weight 
of an ion (M+nH(+)) by its charge number (n).

![](https://latex.codecogs.com/png.latex?M_{z}=\frac{(M&plus;nH^{&plus;})}{n})

In the following, we will convert the uncharged peptide masses to the m/z ratio with charge two by applaying the Mass.toMZ 
function from the BioFSharp library and displax its distribution again. Note that m/z ratio with a charge of two represents 
the predominant charge species.
*)

let digestedPeptideMasses =
    digestedProteins
    |> Array.map (fun peptide ->
        BioSeq.toMonoisotopicMassWith (BioItem.monoisoMass ModificationInfo.Table.H2O) peptide.PepSequence
    )

let chartDigestedPeptideMasses =
    digestedPeptideMasses
    |> Array.map (fun ucMass -> Mass.toMZ ucMass 2.)
    |> Array.filter (fun x -> x < 3000.)
    |> fun masses -> Chart.Histogram(data = masses, orientation = StyleParam.Orientation.Vertical, NBinsX=100)
    |> Chart.withXAxisStyle (title = "m/z", MinMax = (0., 3000.))
    |> Chart.withYAxisStyle "Count"
    
chartDigestedPeptideMasses

(***hide***)
chartDigestedPeptideMasses |> GenericChart.toChartHTML
(***include-it-raw***)

(**
## Questions

1. When trypsin is used for digestion in a MS experiment, it is often combined with another protease (e.g. Lys-C). Why can it be beneficial to combine trypsin?
2. A peptide with a charge of 2 has a m/z of 414. What is the m/z of the same peptide with a charge of 3? Visualize the m/z of the peptides from the fastA with a charge of 3
like done above.
3. Peptides can occur at different charge states during a MS run. Do the different charge states of an peptide usually possess similar intensities?
*)


(**
## References

23. Burkhart, J. M., Schumbrutzki, C., Wortelkamp, S., Sickmann, A. & Zahedi, R. P. Systematic and quantitative comparison of digest efficiency and specificity reveals the impact of trypsin quality on MS-based proteomics. Journal of proteomics 75, 1454–1462; 10.1016/j.jprot.2011.11.016 (2012).
24. Switzar, L., Giera, M. & Niessen, W. M. A. Protein digestion: an overview of the available techniques and recent developments. J. Proteome Res. 12, 1067–1077; 10.1021/pr301201x (2013).
*)

