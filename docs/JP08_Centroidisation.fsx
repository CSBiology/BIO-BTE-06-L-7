(**
# JP08 Centroidisation

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=JP08_Centroidisation.ipynb)


1. [Centroidisation](#Centroidisation)
2. [Peak fitting and picking functions](#Peak-fitting-and-picking-functions)
3. [Application of the peak picking function](#Application-of-the-peak-picking-function)
*)

(**
## Centroidisation
<a href="#Centroidisation" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
In reality, a peak is represented by a collection of signals from a peptide or fragment ion species that are measured by the 
specific detector. Due to imperfections of the measurement, there is a scatter around the accurate mass. This distribution 
along the m/z axis of signals from ion species is termed profile peak. The conversion of a peak profile into the corresponding m/z and 
intensity values reduces the complexity, its representation is termed centroiding. To extract the masses for identification in a simple 
and fast way, peak fitting approaches are used. Further, peak fitting algorithms are also needed to extract ion abundancies and therefore 
explained under quantification in the following section.
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
open BioFSharp.Mz

(**
## Peak fitting and picking functions
<a href="#Centroidisation" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
We declare a function which centroids the given m/z and intensity data. In the scope of the function the m/z and intensity data 
are padded for the wavelet (You will read more about wavelet functions later in <a href="JP11_Quantification.ipynb#Quantification-Theorie">JP11</a>) 
and the centroided. For the centroidisation, we use a Ricker 2D wavelet.
</div>
*)

// Code-Block 1

let ms1PeakPicking (mzData:float []) (intensityData: float []) = 
    if mzData.Length < 3 then 
        [||],[||]
    else
        let paddYValue = Array.min intensityData
        // we need to define some padding and wavelet parameters
        let paddingParams = 
            SignalDetection.Padding.createPaddingParameters paddYValue (Some 7) 0.05 150 95.
        let waveletParameters = 
            SignalDetection.Wavelet.createWaveletParameters 10 paddYValue 0.1 90. 1.
        
        let paddedMz,paddedIntensity = 
            SignalDetection.Padding.paddDataBy paddingParams mzData intensityData
        
        BioFSharp.Mz.SignalDetection.Wavelet.toCentroidWithRicker2D waveletParameters paddedMz paddedIntensity


(**
We load a sample MS<sup>1</sup> from a mgf file.
*)

// Code-Block 2

let ms1 = 
    BioFSharp.IO.Mgf.readMgf (__SOURCE_DIRECTORY__ + @"/../AuxFiles/DavesTaskData/ms1MGF.mgf")
    |> List.head

ms1

(***include-it***)

(**
## Application of the peak picking function
<a href="#Centroidisation" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
We centroid the MS2 data using the function declared beforehand:
</div>
*)

// Code-Block 3

let centroidedMs1 = 
    ms1PeakPicking ms1.Mass ms1.Intensity

(**
*)

// Code-Block 4

//removes low intensity data points for charting
let filteredMs1Mass, filteredMs1Intensity =
    Array.zip ms1.Mass ms1.Intensity
    |> Array.filter (fun (mass, intensity) ->
        intensity > 400.
    )
    |> Array.unzip

let filteredChart =
    [
        Chart.Point(filteredMs1Mass,filteredMs1Intensity)
        |> Chart.withTraceName "Uncentroided MS1"
        Chart.Point(fst centroidedMs1,snd centroidedMs1)
        |> Chart.withTraceName "Centroided MS1"
    ]
    |> Chart.Combine
    |> Chart.withY_AxisStyle "Intensity"
    |> Chart.withX_AxisStyle (title = "m/z", MinMax = (400., 800.))
    |> Chart.withSize (900.,900.)

(***hide***)
filteredChart |> GenericChart.toChartHTML
(***include-it-raw***)

(**
<hr>
<nav class="level is-mobile">
    <div class="level-left">
        <div class="level-item">
            <button class="button is-primary is-outlined" onclick="location.href='/JP07_Signal_detection_and_quantification.html';">&#171; JP07</button>
        </div>
    </div>
    <div class="level-right">
        <div class="level-item">
            <button class="button is-primary is-outlined" onclick="location.href='/JP09_Fragmentation_for_peptide_identification.html';">JP09 &#187;</button>
        </div>
    </div>
</nav>
*)

