(** 
# JP04 Digestion and mass calculation

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=JP04_Digestion_and_mass_calculation.ipynb)

1. [Digestion and mass calculation](#Digestion-and-mass-calculation)
    2. [Accessing the protein sequences of <i>Chlamydomonas reinhardtii</i>](#Accessing-the-protein-sequences-of-Chlamydomonas-reinhardtii)
    3. [Amino acid distribution for <i>C. reinhardtii</i>](#Amino-acid-distribution-for-C.-reinhardtii)
4. [Calculating the molecular weight for peptides](#Calculating-the-molecular-weight-for-peptides)
    5. [<i>In silico</i> digestion of FASTA proteins with trypsin](#In-silico-digestion-of-FASTA-proteins-with-trypsin)
    6. [Calculating peptide masses](#Calculating-peptide-masses)
    7. [Calculating peptide masses for charge 2](#Calculating-peptide-masses-for-charge-2)
6. [References](#References)
*)

(** 
## Digestion and mass calculation
<a href="#Digestion-and-mass-calculation" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
The most widely applied method for protein digestion involves the use of enzymes. Many proteases are available for this purpose, 
each having their own characteristics in terms of specificity, efficiency and optimum digestion conditions. Trypsin is most widely 
applied in bottom-up proteomics and and has a very high degree of specificity, cleaving the peptide bonds C-terminal to the basic residues 
Lys and Arg, except when followed by Pro<sup><a href="#23">23</a></sup>. In general, Lys and Arg are relatively abundant amino acids and are 
usually well distributed throughout a protein<sup><a href="#24">24</a></sup>. This leads to tryptic peptides with an average length of ∼14 amino 
acids that carry at least two positive charges, which is ideally suited for CID-MS analysis<sup><a href="#23">23</a></sup>.

Using <i>in silico</i> analysis, we want to confirm that the general properties of trypsin digestion also apply for the 
proteome of <i>Chlamydomonas reinhardtii</i> . First, we load the proteome of <i>Chlamydomonas</i> in standard fastA format. 
Amino acid composition of the proteome is simply counting each amino acid occurrence and can be visualized by a histogram:
</div>
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
## Accessing the protein sequences of <i>Chlamydomonas reinhardtii</i>
<a href="#Digestion-and-mass-calculation" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
FASTA is a standardized text format, containing gene or protein sequence information. Such FASTAs can be donwloaded 
from <a href="https://www.uniprot.org/proteomes/UP000006906">UniProt</a> for example.

To gain informations about the amino acid composition of <i>C. reinhardtii</i>, we need information about the proteome 
of <i>Chlamydomonas</i>, which is saved in the .fasta file we are accessing below.
</div>
*)

// __SOURCE_DIRECTORY__ returns the directory in which the current notebook is located
let source = __SOURCE_DIRECTORY__
// with /../ we navigate a directory 
let filePath = source + "/../AuxFiles/Chlamy_JGI5_5(Cp_Mp).fasta"
filePath

(*** include-it ***)

(**
Functions to read information from FASTA files exist in the <a href="https://csbiology.github.io/BioFSharp/">BioFSharp</a> library.
*)

let sequences = 
    filePath
    |> IO.FastA.fromFile BioArray.ofAminoAcidString
    |> Seq.toArray
    
// Display the first element in the array collection
sequences |> Array.head

(*** include-it ***)

(**

## Amino acid distribution for <i>C. reinhardtii</i>
<a href="#Digestion-and-mass-calculation" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
To count the amino acid composition, we take the sequence of every protein and count the occurences of each amino acid
</div>

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
    |> Array.map (fun (name,count) -> count, string name)
    // sort by number of occurences
    |> Array.sortBy fst
    // create chart
    |> Chart.Bar
    // style chart
    |> Chart.withX_AxisStyle "Count"
    |> Chart.withTitle "Amino Acid composition of the <i>Chlamydomonas reinhardtii</i> proteome"

(***hide***)
aaDistributionHis |> GenericChart.toChartHTML
(***include-it-raw***)
    

(**

## Calculating the molecular weight for peptides
<a href="#Digestion-and-mass-calculation" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
The molecular weight M of a peptide may be estimated by calculating the equation for the molecular weight of a peptide: 

<img src="https://latex.codecogs.com/gif.latex?M&space;=&space;M_{N}&plus;M_{C}\sum_{i=0}^{n}N_{i}M_{i}" title="M = M_{N}+M_{C}\sum_{i=0}^{n}N_{i}M_{i}" Style="margin: 1rem auto 0; display: block" />

where N<sub>i</sub> are the number, and M<sub>i</sub> the average residue molecular weights, of the amino acids. M<sub>N</sub> + M<sub>C</sub> 
are added to the total in order to account for the termini: H at the N-terminus and OH at the C-terminus. (Remark: if the termini are modified, 
these additions are replaced by those of the modifiers.) 

<div Style="text-align: justify ; font-size: 1.5rem ; margin-top: 2rem ; margin-bottom: 2rem ; line-height: 1.3 ; width: 85% ; margin-left: auto ; margin-right: auto ; padding: 10px ; border: 2px dotted #708090 ; color: #708090">
The distribution of all molecular weights for the peptides resulting from the previous proteome digest can be calculated and visualized using a histogram chart: 
</div>

</div>
*)

(**
## <i>In silico</i> digestion of FASTA proteins with trypsin
<a href="#Digestion-and-mass-calculation" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
To gain information about the peptide sequences of each protein, we have to compute the digested sequence, A digest function with 
variable protease exists in BioFSharp.
</div>
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
<a href="#Digestion-and-mass-calculation" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
We calculate the mass of each peptide by calculating the monoisotopic mass of each amino acid and adding the weight 
of an H<sub>2</sub>O to each peptide weight.
</div>
*)

let chartDigestedProteins =
    digestedProteins
    |> Array.map (fun peptide ->
        // calculate mass for each peptide
        BioSeq.toMonoisotopicMassWith (BioItem.monoisoMass ModificationInfo.Table.H2O) peptide.PepSequence
        )
    // visualize distribution of all peptide masses < 5000 Da
    |> Chart.Histogram
    |> Chart.withX_AxisStyle (title = "Mass [Da]",MinMax=(0.,5000.))
    |> Chart.withY_AxisStyle "Count"

(***hide***)
chartDigestedProteins |> GenericChart.toChartHTML
(***include-it-raw***)
    
(**
## Calculating peptide masses for charge 2
<a href="#Digestion-and-mass-calculation" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
However, in mass spectrometry we are only able to detect ions. Therefore, the measurements report the mass-to-charge ratio. 
The abbreviation m/z (m = mass; z = charge) is used to denote the dimensionless quantity formed by dividing the molecular weight 
of an ion (M+nH<sup>+</sup>) by its charge number (n).

<img src="https://latex.codecogs.com/gif.latex?M_{z}=\frac{(M&plus;nH^{&plus;})}{n}" title="M_{z}=\frac{(M+nH^{+})}{n}" Style="margin: 1rem auto 0; display: block" />

In the following, we will convert the uncharged peptide masses to the m/z ratio with charge two by applaying the Mass.toMZ 
function from the BioFSharp library and displax its distribution again. Note that m/z ratio with a charge of two represents 
the predominant charge species.

</div>
*)

let digestedPeptideMasses =
    digestedProteins
    |> Array.map (fun peptide ->
            BioSeq.toMonoisotopicMassWith (BioItem.monoisoMass ModificationInfo.Table.H2O) peptide.PepSequence
        )

let chartDigestedPeptideMasses =
    digestedPeptideMasses
    |> Array.map (fun ucMass -> Mass.toMZ ucMass 2.) 
    |> Chart.Histogram
    |> Chart.withX_AxisStyle (title = "m/z",MinMax=(0.,5000.))
    |> Chart.withY_AxisStyle "Count"

(***hide***)
chartDigestedPeptideMasses |> GenericChart.toChartHTML
(***include-it-raw***)

(**
<hr>

<nav class="level is-mobile">
    <div class="level-left">
        <div class="level-item">
            <button class="button is-primary is-outlined" onclick="location.href='/JP03_Mass_spectrometry_based_proteomics.html';">&#171; JP03</button>
        </div>
    </div>
    <div class="level-right">
        <div class="level-item">
            <button class="button is-primary is-outlined" onclick="location.href='/JP05_Isotopic_distribution.html';">JP05 &#187;</button>
        </div>
    </div>
</nav>
*)
(**
## References
<a href="#Digestion-and-mass-calculation" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<ol>
<li Value="23" Id="23"> Burkhart, J. M., Schumbrutzki, C., Wortelkamp, S., Sickmann, A. & Zahedi, R. P. Systematic and quantitative comparison of digest efficiency and specificity reveals the impact of trypsin quality on MS-based proteomics. Journal of proteomics 75, 1454–1462; 10.1016/j.jprot.2011.11.016 (2012).</li>
<li Id="24"> Switzar, L., Giera, M. & Niessen, W. M. A. Protein digestion: an overview of the available techniques and recent developments. J. Proteome Res. 12, 1067–1077; 10.1021/pr301201x (2013).</li>
</ol>
*)

