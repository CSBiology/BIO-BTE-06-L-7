
#load @"../IfSharp/References.fsx"
#load @"../IfSharp/Paket.Generated.Refs.fsx"

open BioFSharp.Mz
open FSharp.Plotly

///
let initPaddingParams paddYValue = 
    SignalDetection.Padding.createPaddingParameters paddYValue (Some 7) 0.05 150 95.

///    
let initWaveletParametersMS2 paddYValue = 
    SignalDetection.Wavelet.createWaveletParameters 10 paddYValue 0.1 90. 1.
        
///
let ms2PeakPicking (mzData:float []) (intensityData: float []) = 
    if mzData.Length < 3 then 
        [||],[||]
    else
        let yThreshold = Array.min intensityData
        let paddingParams = initPaddingParams yThreshold
        let paddedMz,paddedIntensity = 
            SignalDetection.Padding.paddDataBy paddingParams mzData intensityData
        let waveletParameters = initWaveletParametersMS2 yThreshold 
        BioFSharp.Mz.SignalDetection.Wavelet.toCentroidWithRicker2D waveletParameters paddedMz paddedIntensity
        
///
let ms2 = 
    BioFSharp.IO.Mgf.readMgf (__SOURCE_DIRECTORY__ + @"/../AuxFiles/DavesTaskData/ms2MGF.mgf")
    |> List.item 0 

///
let centroidedMs2 = 
    ms2PeakPicking ms2.Mass ms2.Intensity

[
    Chart.Point(ms2.Mass,ms2.Intensity)
    |> Chart.withTraceName "Uncentroided MS2"
    Chart.Point(fst centroidedMs2,snd centroidedMs2)
    |> Chart.withTraceName "Centroided MS2"
]
|> Chart.Combine
|> Chart.withY_AxisStyle "Intensity"
|> Chart.withX_AxisStyle "m/z"
|> Chart.withSize (900.,900.)
|> Chart.Show