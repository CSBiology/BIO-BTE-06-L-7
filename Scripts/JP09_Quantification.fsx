
#load @"../IfSharp/References.fsx"
#load @"../IfSharp/Paket.Generated.Refs.fsx"

open System.IO
open BioFSharp.IO
open BioFSharp.Mz
open SearchDB
open QConQuantifier
open Parameters.Domain
open Parameters.DTO
open System.Data.SQLite
open MzIO.Processing
open MzIO
open BioFSharp
open FSharp.Stats
open BioFSharp.Mz.Quantification.HULQ
open PeptideLookUp
open SearchEngineResult
open QConQuantifier.Quantification
open FSharp.Plotly
open FSharp.Stats
open BioFSharp
open SearchDB
open Fragmentation
open TheoreticalSpectra
open FSharpAux
open SearchEngineResult

// Access the ms database
let source = __SOURCE_DIRECTORY__

let dbPath = source + @"/../AuxFiles/sample.mzlite"

let runID = "sample=0"

let reader =
    new MzSQL.MzSQL(dbPath)
    :> MzIO.IO.IMzIODataReader

let tr = reader.BeginTransaction()

/////
//let initPaddingParams paddYValue = 
//    SignalDetection.Padding.createPaddingParameters paddYValue (Some 7) 0.05 150 95.

/////    
//let initWaveletParametersMS2 paddYValue = 
//    SignalDetection.Wavelet.createWaveletParameters 10 paddYValue 0.1 90. 1.

/////
//let ms2PeakPicking (mzData:float []) (intensityData: float []) = 
//    if mzData.Length < 3 then 
//        [||],[||]
//    else
//        let yThreshold = Array.min intensityData
//        let paddingParams = initPaddingParams yThreshold
//        let paddedMz,paddedIntensity = 
//            SignalDetection.Padding.paddDataBy paddingParams mzData intensityData
//        let waveletParameters = initWaveletParametersMS2 yThreshold 
//        BioFSharp.Mz.SignalDetection.Wavelet.toCentroidWithRicker2D waveletParameters paddedMz paddedIntensity

///
let peptideOfInterest = 
    BioFSharp.Mz.SearchDB.createLookUpResult 3439282 302200012 1190.654689 1190654689L "VYVVGEDGILK" (BioList.ofAminoAcidString "VYVVGEDGILK") 0

///
let theoMzCharge2 = 
    Mass.toMZ peptideOfInterest.Mass 2.

///
let ms2Headers = 
    Processing.MassSpectrum.getMassSpectraBy reader runID
    |> Seq.filter (fun ms -> Processing.MassSpectrum.getMsLevel ms = 2)
    |> Seq.filter (fun ms2 -> 
        let precMz = Processing.MassSpectrum.getPrecursorMZ ms2 
        precMz > theoMzCharge2-0.05 && precMz < theoMzCharge2+0.05
    )

///
let ms2WithSpectra = 
    ms2Headers
    |> Seq.map (fun ms2Header -> 
        let ms2ID = Processing.MassSpectrum.getID ms2Header
        let (mzData, intensityData) = 
            Processing.PeakArray.getPeak1DArray reader ms2ID  
            |> Processing.PeakArray.mzIntensityArrayOf 
        let peaks = 
            (mzData, intensityData)
            |> fun (mzData, intensityData) -> PeakArray.zipMzInt mzData intensityData
        ms2Header, peaks
    ) 

ms2WithSpectra
|> Seq.map (fun (model, peakArray) ->
    Chart.Point (
        peakArray
        |> Array.map (fun peak ->
            printfn "%f" peak.Mz
            peak.Mz
        ),
        peakArray
        |> Array.map (fun peak ->
            peak.Intensity
        )
    )
    |> Chart.withTraceName (model.ID)
)
|> Chart.Combine
|> Chart.withX_AxisStyle "m/z"
|> Chart.withY_AxisStyle "Intensity"
|> Chart.Show

///
let calcIonSeries massF aal = 
    let ys = Fragmentation.Series.yOfBioList massF aal
    let bs = Fragmentation.Series.bOfBioList massF aal
    ys@bs
///
let peptideSpectrumMatches chargeState ms2PrecursorMZ lookUpResult centroidedMs2 modelID = 
    SequestLike.calcSequestLikeScoresRevDecoy 
        calcIonSeries (BioItem.monoisoMass) (100.,1500.) 
            centroidedMs2 1. chargeState ms2PrecursorMZ lookUpResult modelID

let score = 
    ms2WithSpectra
    |> Seq.map (fun (ms2Header,centroidedPeaks) -> 
        let score = 
            let results = peptideSpectrumMatches 2 theoMzCharge2 [peptideOfInterest] centroidedPeaks ms2Header.ID
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
    |> List.sortByDescending (fun (ms2Header, score) -> score) 
    |> List.map (fun (ms2Header, score) -> 
        let retTime = Processing.MassSpectrum.getScanTime ms2Header
        retTime,score.Score 
    )

let normRetTimeAndScore = 
    let maxScore = 
        retTimeAndScore
        |> List.maxBy snd 
        |> snd 
    retTimeAndScore
    |> List.map (fun (ret,score) -> ret, (score / maxScore))

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

///
let retTimeIdx = 
    let idx = Processing.Query.getMS1RTIdx reader runID
    idx

///   
let ret = 27.68108333

///
let rtQuery = Processing.Query.createRangeQuery ret 50.

///
let mzQuery = Processing.Query.createRangeQuery theoMzCharge2 0.04

///
let xic = 
    Processing.Query.getXIC reader retTimeIdx rtQuery mzQuery  
    |> Array.map (fun p -> p.Rt , p.Intensity)

let normXic = 
    let maxIntensity = 
        xic
        |> Array.maxBy snd 
        |> snd 
    xic    
    |> Array.map (fun (ret,score) -> ret, (score / maxIntensity))

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
//let quantifiedXIC = 
//     Quantification.HULQ.quantifyPeak FSharp.Stats.Signal.PeakDetection.idxOfClosestLabeledPeak 11 1. 20. ret rtData intensityData 