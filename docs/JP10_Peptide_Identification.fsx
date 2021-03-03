
(**
# JP10 Peptide Identification

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=JP10_Peptide_Identification.ipynb)


1. [Understanding peptide identification from MS<sup>2</sup> spectra](#Understanding-peptide-identification-from-MS2-spectra)
2. [Matching and scoring of Tandem MS peptide identification](#Matching-and-scoring-of-Tandem-MS-peptide-identification)
    3. [Step 1: Data acquisition and preprocessing](#Step-1:-Data-acquisition-and-preprocessing)
    4. [Step 2: Preselecting the peptides of interest](#Step-2:-Preselecting-the-peptides-of-interest)
    5. [Step 3+4: Matching and Scoring](#Step-3+4:-Matching-and-Scoring)
*)

(**
## Understanding peptide identification from MS<sup>2</sup> spectra
<a href="#Peptide-Identification" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
Under low-energy fragmentation conditions, peptide fragment patterns are reproducible and, in general, predictable, which allows for an 
amino acid sequence identification according to a fragmentation expectation. Algorithms for peptide identification perform in principle 
three basic tasks:

<b>(i)</b> a raw data preprocessing step is applied to the MS/MS spectra to obtain clean peak information. The same signal filtering 
and background subtraction methods are used as discussed in the section of low-level processing. Peak detection, however, may be performed 
differently. Preprocessing of MS/MS spectra focuses on the extraction of the precise m/z of the peak rather than the accurate peak areas. 
The conversion of a peak profile into the corresponding m/z and intensity values reduces the complexity, its representation is termed centroiding. 
To extract the masses for identification in a simple and fast way, peak fitting approaches are used. These approaches take either the most intense 
point of the peak profile, fit a Lorentzian distribution to the profile, or use a quadratic fit<sup><a href="#37">37</a></sup>. 

<b>(ii)</b> Spectrum information and possible amino acid sequences assignments are evaluated. 

<b>(iii)</b> The quality of the match between spectrum and possible sequences is scored.

Even though the three steps roughly describe the basic principle of algorithms used for peptide sequence identification, most implementations 
show differences in individual steps which can lead to major changes in the outcome. Therefore, it has been proven useful to utilize more than 
one algorithm for a robust and thorough identification. Due to their major difference in identification strategies and prerequisites, 
identification algorithms are normally classified into three categories: (i) <i>de novo</i> peptide sequencing, (ii) peptide sequence-tag (PST) 
searching, and (iii) uninterpreted sequence searching. However, in this notebook we focus on the explanation of (iii) uninterpreted sequence 
searching, the most frequently used methods.
</div>
*)

(**
## Matching and scoring of Tandem MS peptide identification
<a href="#Peptide-Identification" style="display: inline-block"><sup>&#8593;back</sup></a><br>


<div class="container">

<div Id="figure4" Style="float: right ; display: inline-block ; color: #44546a ; width: 60% ; padding: 15px">
    <img src="img/ComputationalProteinIdentification.png" Style="width: 100%">
    <div Style="padding-left: 1rem ; padding-right: 1rem ; text-align: justify ; font-size: 0.8rem"><b>
        Figure 4: Process of computational identification of peptides from their fragment spectra</b>
    </div>
</div>    

Previously we learned, that peptides fragment to create patterns characteristic of a specific amino acid sequence. These patterns are reproducible and, in general, predictable taking the applied fragmentation method into account. This can be used for computational identification of peptides from their fragment spectra. This process can be subdivided into 5 main steps: spectrum preprocessing, selection of possible sequences, generating theoretical spectra, matching and scoring (<a href="#figure5" >Figure 4</a>). The first step is a preprocessing of the experimental spectra and is done to reduce noise. Secondly, all possible amino acid sequences are selected which match the particular precursor peptide mass. The possible peptides can but do not need to be restricted to a particular organism. A theoretical spectrum is predicted for each of these amino acid sequences. Matching and scoring is performed by comparing experimental spectra to their predicted corresponding theoretical spectra. The score function measures the closeness of fit between the experimental acquired and theoretical spectrum. There are many different score functions, which can be considered as the main reason of different identifications considering different identification algorithm. The most natural score function is the cross correlation score (xcorr) used by the commercially available search tool SEQUEST. One of the reasons the xcorr is so sensitive is because it involves a correction factor that assesses the background correlation for each acquired spectrum and the theoretically predicted spectrum from sequences within a database. To compute this correction factor, a measure of similarity is calculated at different offsets between a preprocessed mass spectrum and a theoretical spectrum. The final xcorr is then the correlation at zero offset minus the mean correlation from all the individual offsets.

Thus, the correlation function measures the coherence of two signals by, in effect, translating one signal across the other. This can be mathematically achieved using the formula for cross-correlation in the form for discrete input signals:

<i><b>Equation 5</b></i>

<img src="https://latex.codecogs.com/gif.latex?\large&space;R_{\tau}&space;=&space;\sum_{i=0}^{n-1}x_{i}\cdot y_{i&plus;\tau}" title="\large R_{𝜏} = \sum_{i=0}^{n-1}x_{i}\cdot y_{i+𝜏}" style="margin: 1rem auto 1rem; display: block" />

where x<sub>i</sub> is a peak of the reconstructed spectrum at position i and y<sub>i</sub> is a peak of the experimental spectrum. The displacement value 𝜏 is the amount by which the signal is offset during the translation and is varied over a range of values. If two signals are the same, the correlation function should have its maxima at 𝜏=0, where there is no offset. The average of the offset-correlation is seen as the average background correlation and needs to be subtracted from the correlation at 𝜏=0. It follows: 

<i><b>Equation 6</b></i>

<img src="https://latex.codecogs.com/gif.latex?xcorr&space;=&space;R_{0}&space;-&space;\frac{(\sum&space;\begin{matrix}&space;\tau=&plus;offeset\\&space;\tau=-offeset\end{matrix}R_{\tau})}{2*offset&plus;1}" title="xcorr = R_{0} - \frac{(\sum \begin{matrix} \tau=+offeset\\ \tau=-offeset\end{matrix}R_{\tau})}{2*offset+1}" style="margin: 1rem auto 1rem; display: block" />

In practice many theoretical spectra have to be matched again a single experimental spectrum. Therefore, the calculation can be speed up by reformulating Equation 5 and Equation 6 and introduce a preprocessing step, which is independent of the predicted spectra.

<i><b>Equation 7</b></i>

<img src="https://latex.codecogs.com/gif.latex?xcorr&space;=&space;x_{0}\cdot&space;y_{0}&space;-&space;\frac{(\sum&space;\begin{matrix}&space;\tau=&plus;offeset\\&space;\tau=-offeset\end{matrix}x_{0}\cdot&space;y_{\tau})}{2*offset&plus;1}" title="xcorr = x_{0}\cdot y_{0} - \frac{(\sum \begin{matrix} \tau=+offeset\\ \tau=-offeset\end{matrix}x_{0}\cdot y_{\tau})}{2*offset+1}" style="margin: 1rem auto 1rem; display: block" />

For the preprocessed experimental spectrum y' it follows:

<i><b>Equation 8</b></i>

<img src="https://latex.codecogs.com/gif.latex?xcorr&space;=&space;x_{0}\cdot&space;y'" title="xcorr = x_{0}\cdot y'" style="margin: 1rem auto 1rem; display: block" />

where:

<i><b>Equation 9</b></i>

<img src="https://latex.codecogs.com/gif.latex?y'&space;=&space;y_{0}&space;-&space;\frac{(\sum&space;\begin{matrix}&space;\tau=&plus;offeset\\&space;\tau=-offeset\end{matrix}x_{0}\cdot&space;y_{\tau})}{2*offset&plus;1}" title="y' = y_{0} - \frac{(\sum \begin{matrix} \tau=+offeset\\ \tau=-offeset\end{matrix}x_{0}\cdot y_{\tau})}{2*offset+1}" style="margin: 1rem auto 1rem; display: block" />

<div Style="text-align: justify ; margin-top: 2rem ; margin-bottom: 2rem ; line-height: 1.3 ; width: 85% ; margin-left: auto ; margin-right: auto ; padding: 10px ; border: 2px dotted #708090 ; color: #708090">
Matching a measured spectrum against chlamy database
</div>
</div>
*)

#r "nuget: BioFSharp, 2.0.0-beta5"
#r "nuget: BioFSharp.IO, 2.0.0-beta5"
#r "nuget: BioFSharp.Mz, 0.1.5-beta"
#r "nuget: Plotly.NET, 2.0.0-beta6"

#if IPYNB
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: Plotly.NET.Interactive, 2.0.0-beta6"
#endif // IPYNB

open Plotly.NET
open BioFSharp

(**
### Step 1: Data acquisition and preprocessing
<a href="#Peptide-Identification" style="display: inline-block"><sup>&#8593;back</sup></a><br>

We load a single MS<sup>2</sup> spectrum that is saved in a mgf file.
*)

// Code-Block 1

let ms2 = 
    BioFSharp.IO.Mgf.readMgf (__SOURCE_DIRECTORY__ + @"/../AuxFiles/ms2sample.mgf")
    |> List.head
    
ms2

(***include-it***)

(**
Here, the spectrum is already centroidized as shown in <a href="JP08_Centroidisation.ipynb">JP08_Centroidisation</a> using the function 
<code>msPeakPicking</code>. So we just visualize mass and intensity:
*)

// Code-Block 2

let ms2Chart = Chart.Column(ms2.Mass, ms2.Intensity)

(***hide***)
ms2Chart |> GenericChart.toChartHTML
(***include-it-raw***)

(**
Now, we will preprocess the experimental spectrum from our example. This is on the one hand to reduce noise, but also to make 
the measured spectrum more like the once we are able to simulate. 
*)

// Code-Block 3

let lowerScanLimit = 150.
let upperScanLimit = 1000.

let preprocessedIntesities =
    Mz.PeakArray.zip ms2.Mass ms2.Intensity
    |> (fun pa -> Mz.PeakArray.peaksToNearestUnitDaltonBinVector pa lowerScanLimit upperScanLimit)
    |> (fun pa -> Mz.SequestLike.windowNormalizeIntensities pa 10)
    
let intensityChart = Chart.Column([lowerScanLimit .. upperScanLimit], preprocessedIntesities)

(***hide***)
intensityChart |> GenericChart.toChartHTML
(***include-it-raw***)

(**
### Step 2: Preselecting the peptides of interest
<a href="#Peptide-Identification" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
Every MS<sup>2</sup> spectrum is accompanied by a m/z ratio reported by the instrument. Additionally, we can estimate the charge looking 
at the isotopic cluster. We take the peptide "DTDILAAFR" from our previous notebook again. Our example has a measured m/z = 511.2691141 and 
a charge of z=2.
</div>
*)

let peptideMass = 
    Mass.ofMZ 511.2691141 2.

peptideMass

(***include-it***)

(**
From our previos notebook <a href="JP04_Digestion_and_mass_calculation.ipynb">JP04 Digestion and mass calculation</a>, we know how to 
calculate all peptide masses that we can expect to be present in <i>Chlamydomonas reinhardtii</i>.
*)

// Code-Block 4

let source = __SOURCE_DIRECTORY__
let filePath = source + @"/../AuxFiles/Chlamy_JGI5_5(Cp_Mp).fasta"

let peptideAndMasses = 
    filePath
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
However, we are only interest in possible amino acid sequences, that match the particular precursor peptide mass of our example spectrum 
with 1020.523675 Da. Additionaly, we should also consider a small measurement error.
*)

// Code-Block 5

peptideAndMasses
|> Array.filter (fun (sequence,mass) -> mass > 1020.52  && mass < 1020.53)

(***include-it***)

(**
In the previous notebook <a href="JP09_Fragmentation_for_peptide_identification.ipynb">Fragmentation for peptide identification</a>, 
we used functions that generate the theoretical series of b- and y-ions from the given peptide. Combined with the function 
<code>Mz.SequestLike.predictOf</code> that generates theoretical spectra that fit the Sequest scoring algorithm.
*)

// Code-Block 6

let predictFromSequence peptide =
    [
        peptide
        |> Mz.Fragmentation.Series. yOfBioList BioItem.initMonoisoMassWithMemP
        peptide
        |> Mz.Fragmentation.Series.bOfBioList BioItem.initMonoisoMassWithMemP
    ]
    |> List.concat
    |> Mz.SequestLike.predictOf (lowerScanLimit,upperScanLimit) 2.

predictFromSequence

(**
### Step 3+4: Matching and Scoring
<a href="#Peptide-Identification" style="display: inline-block"><sup>&#8593;back</sup></a><br>

In the matching step, we compare theoretical spectra of peptides within our precursor peptide mass range with our measured spectra. 
We get a score which tells us how well the theoretical spectrum fits the given experimental spectrum.
*)

// Code-Block 7

peptideAndMasses
|> Array.filter (fun (sequence,mass) ->
    mass > 1020.52  && mass < 1020.53
)
|> Array.map (fun (sequence,mass)    ->
    sequence,predictFromSequence sequence
)
|> Array.map (fun (sequence,theoSpectrum) -> 
    sequence,Mz.SequestLike.scoreSingle theoSpectrum preprocessedIntesities
)
|> Array.sortByDescending (fun (sequence,score) -> score)

(***include-it***)

(**
Finaly, we pick the sequence with the best score and are done for now. Notice however, that in a real world we would need to 
relate our score to the complete data set to get an idea of the overall quality and which numerical value we could trust. 
*)

(**
<hr>
<nav class="level is-mobile">
    <div class="level-left">
        <div class="level-item">
            <button class="button is-primary is-outlined" onclick="location.href='/JP09_Fragmentation_for_peptide_identification.html';">&#171; JP09</button>
        </div>
    </div>
    <div class="level-right">
        <div class="level-item">
            <button class="button is-primary is-outlined" onclick="location.href='/JP11_Quantification.html';">JP11 &#187;</button>
        </div>
    </div>
</nav>
*)