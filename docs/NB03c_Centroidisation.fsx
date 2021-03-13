(**
# NB03c Centroidisation

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=NB03c_Centroidisation.ipynb)


1. Centroidisation
2. Peak fitting and picking functions
3. Application of the peak picking function
4. Questions

*)

(**
## Centroidisation

In reality, a peak is represented by a collection of signals from a peptide or fragment ion species that are measured by the 
specific detector. Due to imperfections of the measurement, there is a scatter around the accurate mass. This distribution 
along the m/z axis of signals from ion species is termed profile peak. The conversion of a peak profile into the corresponding m/z and 
intensity values reduces the complexity, its representation is termed centroiding. To extract the masses for identification in a simple 
and fast way, peak fitting approaches are used. Further, peak fitting algorithms are also needed to extract ion abundancies and therefore 
explained under quantification in the following section.
*)

#r "nuget: BioFSharp, 2.0.0-beta5"
#r "nuget: BioFSharp.IO, 2.0.0-beta5"
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: BioFSharp.Mz, 0.1.5-beta"
#r "nuget: BIO-BTE-06-L-7_Aux, 0.0.1"

#if IPYNB
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: Plotly.NET.Interactive, 2.0.0-beta6"
#endif // IPYNB

open Plotly.NET
open BioFSharp.Mz
open BIO_BTE_06_L_7_Aux.FS3_Aux
open System.IO

(**
## Peak fitting and picking functions

We declare a function which centroids the given m/z and intensity data. In the scope of the function the m/z and intensity data 
are padded for the wavelet (You will read more about wavelet functions later in *NB05a\_Quantification.ipynb* ) 
and the centroided. For the centroidisation, we use a Ricker 2D wavelet.
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
            SignalDetection.Wavelet.createWaveletParameters 10 paddYValue 0.1 90. 1. false false
        
        let paddedMz,paddedIntensity = 
            SignalDetection.Padding.paddDataBy paddingParams mzData intensityData
        
        BioFSharp.Mz.SignalDetection.Wavelet.toCentroidWithRicker2D waveletParameters paddedMz paddedIntensity 


(**
We load a sample MS1 from a mgf file.
*)

// Code-Block 2
let directory = __SOURCE_DIRECTORY__
let path = Path.Combine[|directory;"downloads/ms1MGF.mgf"|]
downloadFile path "ms1MGF.mgf" "bio-bte-06-l-7"

let ms1 = 
    BioFSharp.IO.Mgf.readMgf (path)
    |> List.head

ms1

(***include-it***)

(**
## Application of the peak picking function

We centroid the MS2 data using the function declared beforehand:
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
filteredChart
(***hide***)
filteredChart |> GenericChart.toChartHTML
(***include-it-raw***)

(**
## Questions:

1. The aim of centroidization is finding the m/z for each profile peak. How can this improve the performance and quality of the following steps?

2. In the result plot, a single ms1 spectrum is shown. Naively describe the differences between the uncentroided and the centroided spectrums.

3. Taking into consideration your answer for question 1, do your findings of question 2 meet your expectations? If yes, why? If no, why?

*)
