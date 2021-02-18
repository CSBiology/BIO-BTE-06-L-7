(**
# JP07 Signal detection and quantification

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=JP07_Signal_detection_and_quantification.ipynb)

1. [Signal detection and quantification](#Signal-detection-and-quantification)
*)

(**
## Signal detection and quantification
<a href="#Signal-detection-and-quantification" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
Signals detected during a LC-MS measurement are pairs of m/z value and ion intensity in time. All signals recorded at a particular retention 
time in a given mass range compose a spectrum. Thus, in its most raw form, a generic spectrum contains the following information: (i) scan 
(spectra) number; (ii) retention time; (iii) vector of m/z values; (iv) vector of ion intensities; (v) scan mode. The scan number is a simple 
enumeration over the measurements. The retention time is the time when the measured peptides were eluting from the column. The m/z values 
represent the mass over charge values of the ions and ion intensity values are the corresponding ion abundances. The scan mode denotes the 
operational mode used to record the spectrum. It is either a full scan or the number of the fragmentation scan.

<div Id="figure3" Style="margin-left: auto ; margin-right: auto ; color: #44546a ; width: 80% ; padding: 15px">
    <img src="img/MSDerivedDataSpaces.png" Style="width: 100%">
    <div Style="padding-left: 1rem ; padding-right: 1rem ; text-align: justify ; margin-top: 1rem; font-size: 0.8rem ">
        <b>Figure 3: A conceptual view of different spaces of MS derived data sets.</b>
        (i) The metadata-space serves as a descriptive layer to order, assign and integrate spectra information. (ii) 
        The MS1-space and the (iii) MS2-space represent two independent entities that differ in signal shape, resolution and their 
        information content (‘features’).
    </div>
</div>    
</div>
*)

(**
<hr>
<nav class="level is-mobile">
    <div class="level-left">
        <div class="level-item">
            <button class="button is-primary is-outlined" onclick="location.href='/JP06_Retention_time_and_scan_time.html';">&#171; JP06</button>
        </div>
    </div>
    <div class="level-right">
        <div class="level-item">
            <button class="button is-primary is-outlined" onclick="location.href='/JP08_Centroidisation.html';">JP08 &#187;</button>
        </div>
    </div>
</nav>
*)