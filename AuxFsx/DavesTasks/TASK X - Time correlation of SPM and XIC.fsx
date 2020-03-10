#load "references.fsx"

open System
open BioFSharp
open BioFSharp.Mz
open FsMzLite
open FSharp.Plotly

// Accesses ms spectra
let fileName = @"task7.wiff"

let dbPath = (__SOURCE_DIRECTORY__ + "/data/" + fileName)

let runID = "sample=0";

let reader = new MzLite.Wiff.WiffFileReader(dbPath)


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
let peptideOfInterest = 
    BioFSharp.Mz.SearchDB.createLookUpResult 3439282 302200012 1190.654689 1190654689L "VYVVGEDGILK" (BioList.ofAminoAcidString "VYVVGEDGILK") 0

///
let theoMzCharge2 = 
    Mass.toMZ peptideOfInterest.Mass 2.  

///
let ms2Headers = 
    FsMzLite.AccessMassSpectrum.getMassSpectraBy reader runID
    |> Seq.filter (fun ms -> FsMzLite.AccessMassSpectrum.getMsLevel ms = 2)
    |> Seq.filter (fun ms2 -> 
                        let precMz = FsMzLite.AccessMassSpectrum.getPrecursorMZ ms2 
                        precMz > theoMzCharge2-0.5 && precMz < theoMzCharge2+0.05
                  )
                  

///
let ms2WithSpectra = 
    ms2Headers
    |> Seq.map (fun ms2Header -> 
                    let ms2ID = FsMzLite.AccessMassSpectrum.getID ms2Header
                    let (mzData,intensityData) = 
                        FsMzLite.AccessPeakArray.getPeak1DArray reader ms2ID  
                        |> FsMzLite.AccessPeakArray.mzIntensityArrayOf 
                    let (centroidedPeaks) = 
                        ms2PeakPicking mzData intensityData
                        |> fun (centrMzData,centrIntensityData) -> PeakArray.zipMzInt centrMzData centrIntensityData
                    ms2Header, centroidedPeaks
               ) 
                    
///
let calcIonSeries massF aal = 
    let ys = Fragmentation.Series.yOfBioList massF aal
    let bs = Fragmentation.Series.bOfBioList massF aal
    ys@bs

///
let peptideSpectrumMatches chargeState ms2PrecursorMZ lookUpResult centroidedMs2 = 
    SequestLike.calcSequestLikeScoresRevDecoy 
        calcIonSeries (BioFSharp.Formula.monoisoMass) (100.,1500.) 
            centroidedMs2 1. chargeState ms2PrecursorMZ lookUpResult "ExemplaryMS2"           
  
let score = 
    ms2WithSpectra
    |> Seq.map (fun (ms2Header,centroidedPeaks) -> 
                    let score = 
                        let results = peptideSpectrumMatches 2 theoMzCharge2 [peptideOfInterest] centroidedPeaks
                        match results with 
                        | []  -> None 
                        | h::tail -> Some h 
                    ms2Header, score 
                    )
    |> List.ofSeq
    
let retTimeAndScore =                                                    
    score 
    |> List.filter (fun (ms2Header, score) -> score.IsSome)
    |> List.map (fun (ms2Header, score) -> (ms2Header, score.Value)) 
    |> List.sortByDescending (fun (ms2Header, score) -> score ) 
    |> List.map (fun (ms2Header, score) -> 
                    let retTime = FsMzLite.AccessMassSpectrum.getScanTime ms2Header
                    retTime,score.Score 
                )

let normRetTimeAndScore = 
    let maxScore = 
        retTimeAndScore
        |> List.maxBy snd 
        |> snd 
    retTimeAndScore
    |> List.map (fun (ret,score) -> ret, (score / maxScore) )   


///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

///
let retTimeIdx = 
    let trans = reader.BeginTransaction() 
    let idx = FsMzLite.Query.getMS1RTIdx reader runID;
    trans.Dispose()
    idx   
   
///   
let ret = 27.68108333

///
let rtQuery = FsMzLite.Query.createRangeQuery ret 50.

///
let mzQuery = FsMzLite.Query.createRangeQuery theoMzCharge2 0.04

///
let xic = 
    FsMzLite.Query.getXIC reader retTimeIdx rtQuery mzQuery  
    |> Array.map (fun p -> p.Rt , p.Intensity)
    
let normXic = 
    let maxIntensity = 
        xic
        |> Array.maxBy snd 
        |> snd 
    xic    
    |> Array.map (fun (ret,score) -> ret, (score / maxIntensity) )   

let xicVis = 
    Chart.Point(normXic)

let psmVis = 
    Chart.Point(normRetTimeAndScore)

[
    xicVis
    psmVis
]
|> Chart.Combine
|> Chart.Show


///
let quantifiedXIC = 
    let trans = reader.BeginTransaction()
    Quantification.MyQuant.quantify Quantification.MyQuant.idxOfClosestLabeledPeak 11 1. 20. ret rtData intensityData 




    
    
       