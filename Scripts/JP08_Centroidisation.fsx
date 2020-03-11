
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
let ms1PeakPicking (mzData:float []) (intensityData: float []) = 
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
let ms1 = 
    BioFSharp.IO.Mgf.readMgf (__SOURCE_DIRECTORY__ + @"/../AuxFiles/DavesTaskData/ms1MGF.mgf")
    |> List.head

///
let centroidedMs1 = 
    ms1PeakPicking ms1.Mass ms1.Intensity



[
    Chart.Point(ms1.Mass,ms1.Intensity)
    |> Chart.withTraceName "Uncentroided MS1"
    Chart.Point(fst centroidedMs1,snd centroidedMs1)
    |> Chart.withTraceName "Centroided MS1"
]
|> Chart.Combine
|> Chart.withY_AxisStyle "Intensity"
|> Chart.withX_AxisStyle (title = "m/z", MinMax = (400., 800.))
|> Chart.withSize (900.,900.)
|> Chart.Show