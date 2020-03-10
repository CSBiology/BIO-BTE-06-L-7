#load "references.fsx"

open BioFSharp.Mz
open FSharp.Plotly


open BioFSharp
open BioFSharp.IO
open BioFSharp.Mz.SignalDetection.Padding
open BioFSharp.Mz.SearchDB
open SequestLike

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
    BioFSharp.IO.Mgf.readMgf (__SOURCE_DIRECTORY__ + @"/data/ms2MGF.mgf")
    |> List.item 0 

///
let centroidedMs2 = 
    ms2PeakPicking ms2.Mass ms2.Intensity

///
[
    Chart.Point(ms2.Mass,ms2.Intensity)
    Chart.Point(fst centroidedMs2,snd centroidedMs2)
]
|> Chart.Combine
|> Chart.Show

/// PrecursorMZ that was picked in the MS1 full scan to generate a MS2
let ms2PrecursorMZ = 
    match BioFSharp.IO.Mgf.tryGetPrecursorMZ ms2 with 
    | Some mz -> mz 
    | None    -> 0.

/// This mass can now be used to obtain a peptide search in a database created by in silico digestion of the known proteom of a organism of choice.
/// This search was done beforehand and you can retrieve the result stored in the following file: 
let lookUpResult = 
     [
        BioFSharp.Mz.SearchDB.createLookUpResult 3439282 302200012 1190.654689 1190654689L "VYVVGEDGILK" (BioList.ofAminoAcidString "VYVVGEDGILK") 0
        BioFSharp.Mz.SearchDB.createLookUpResult 5411223 444660385 1190.670631 1190670631L "AIRGFCQRLK" (BioList.ofAminoAcidString "AIRGFCQRLK") 0
        BioFSharp.Mz.SearchDB.createLookUpResult 2343340 203080049 1190.670631 1190670631L "IGRGLFRNMK" (BioList.ofAminoAcidString "IGRGLFRNMK") 0
     ]

//Understanding ms2
let calcIonSeries massF aal = 
    let ys = Fragmentation.Series.yOfBioList massF aal
    let bs = Fragmentation.Series.bOfBioList massF aal
    ys@bs

let peptideSpectrumMatches = 
    SequestLike.calcSequestLikeScoresRevDecoy 
        calcIonSeries (BioFSharp.Formula.monoisoMass) (100.,1500.) 
            (centroidedMs2 |> fun (x,y) -> PeakArray.zipMzInt x y ) 0. 2 ms2PrecursorMZ lookUpResult "ExemplaryMS2"           
  

  