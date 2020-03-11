
#load @"../IfSharp/References.fsx"
#load @"../IfSharp/Paket.Generated.Refs.fsx"
#load @"../AuxFsx/ProtAux.fsx"

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
open MzIO.Processing
open MzIO.Processing.MzIOLinq
open MzIO.IO
open MzIO
open MzIO.Model
open MzIO.MzSQL
open ProtAux

// Access the ms database
let source = __SOURCE_DIRECTORY__

// mzlite is a file-based local SQLite database. (Similiar to a collection of optimized excel tables.)  
let dbPath = source + @"/../AuxFiles/sample.mzlite"

// The id of the data we want to access
let runID = "sample=0"


// Creates an object capable of reading an mzlite database.
let reader =
    new MzSQL.MzSQL(dbPath)
    :> MzIO.IO.IMzIODataReader
    
// Uses the 'reader' to start the transaction with the database.
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

// calculate the M/Z value for the 'peptideOfInterest'
let theoMzCharge2 = 
    Mass.toMZ peptideOfInterest.Mass 2.

let ms2Headers = 
    Processing.MassSpectrum.getMassSpectraBy reader runID
        // filter for only MS2 spectrum
    |> Seq.filter (fun ms -> Processing.MassSpectrum.getMsLevel ms = 2)
    |> Seq.filter (fun ms2 -> 
        // Seq.filter iterates the whole sequenz applying a certain filter function to each element.
        // The function picks the correlated precursor - M/Z for each found MS2 
        let precMz = Processing.MassSpectrum.getPrecursorMZ ms2 
        // Then it filters out all MS2, where the precursor - M/Z is not within a certain range to the 'theoMzCharge2'
        precMz > theoMzCharge2-0.05 && precMz < theoMzCharge2+0.05
    )

///
let ms2WithSpectra = 
    ms2Headers
    |> Seq.map (fun ms2Header -> 
        // Get the MS2 IDs of all found MS2 headers 
        let ms2ID = Processing.MassSpectrum.getID ms2Header
        // Get all peaks related to 'ms2ID'
        let peaks = 
            // A peak is a sequence of x- and y-data points
            reader.ReadSpectrumPeaks(ms2ID).Peaks
            |> Seq.map (fun p-> Peak(p.Mz,p.Intensity))
            |> Array.ofSeq
        // return a tuple of the MS2 header (as identifier for later), and the peak array
        ms2Header, peaks
    )

// Create peak charts from all found peaks within the M/Z range of the theoretical M/Z value for charge 2.
ms2WithSpectra
|> Seq.map (
    // model is here the MS2 header
    fun (model, peakArray) ->
        // create a Chart.Point from peak.Mz and peak.Intensity
        Chart.Point (
            // access peak M/Z
            peakArray
            |> Array.map (fun peak -> peak.Mz )
            ,
            // access peak intensity
            peakArray
            |> Array.map (fun peak -> peak.Intensity )
        )
        |> Chart.withTraceName (model.ID)
)
|> Chart.Combine
|> Chart.withX_AxisStyle "m/z"
|> Chart.withY_AxisStyle "Intensity"
|> Chart.withSize (900.,900.)
|> Chart.Show

// this is used to create simulated MS2 spectra for a peptide of interest
let calcIonSeries aal = 
    Fragmentation.Series.fragmentMasses 
        Fragmentation.Series.bOfBioList 
        Fragmentation.Series.yOfBioList 
        BioItem.initMonoisoMassWithMemP 
        aal

let peptideSpectrumMatches chargeState ms2PrecursorMZ lookUpResult centroidedMs2 (ms2: Model.MassSpectrum) =
    let theoSpec =
        lookUpResult
        |> List.map (fun lookUpResult -> 
            // lookUpResult will be the 'peptideOfInterest'
            let ionSeries = calcIonSeries lookUpResult.BioSequence
            lookUpResult,ionSeries
        )
        // (100.,1500.) is the M/Z range in which we are interested
        |> SequestLike.getTheoSpecs (100.,1500.) chargeState
    SequestLike.calcSequestScore
        (100.,1500.)
        // the MS2 spectra from 'ms2WithSpectra'
        centroidedMs2
        // ms2 is the header from 'ms2WithSpectra'
        (MassSpectrum.getScanTime ms2)
        2
        // the theoretical M/Z from 'theoMzCharge2'
        ms2PrecursorMZ
        theoSpec
        ms2.ID
        
let score = 
    ms2WithSpectra
    |> Seq.map (fun (ms2Header,centroidedPeaks) -> 
        let score = 
            // get all peptide spectrum matches
            let results = peptideSpectrumMatches 2 theoMzCharge2 [peptideOfInterest] centroidedPeaks ms2Header
            match results with 
            // Should no match be found, return None
            | []  -> None 
            // Should there be at least one match, get the first value (the one with the highest score)
            | h::tail -> Some h 
        ms2Header, score 
    )
    |> List.ofSeq

let retTimeAndScore =                                                    
    score 
    // filte so that any match was found and score.IsSome
    |> List.filter (fun (ms2Header, score) -> score.IsSome)
    // map to return the value of score.
    |> List.map (fun (ms2Header, score) -> (ms2Header, score.Value)) 
    // sort by descending (from high to low)
    |> List.sortByDescending (fun (ms2Header, score) -> score.Score) 
    // get the related retention time for the found score, via the ms2Header as identifier
    |> List.map (fun (ms2Header, score) -> 
        let retTime = Processing.MassSpectrum.getScanTime ms2Header
        retTime,score.Score 
    )

// normalize score to the maximal found score
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
let ret = 27.71

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
    |> Array.map (fun (ret,intensity) -> ret, (intensity / maxIntensity))

let xicVis = 
    Chart.Point(normXic)
    |> Chart.withTraceName "Normalized XIC"

let psmVis = 
    Chart.Point(normRetTimeAndScore)
    |> Chart.withTraceName "Retention time and normalized Score"

[
    xicVis
    psmVis
]
|> Chart.Combine
|> Chart.Show


///
let quantifiedXIC =
    let peaks = 
        xic
        |> Array.unzip
        |> (fun (ret, intensity) ->
            FSharp.Stats.Signal.PeakDetection.SecondDerivative.getPeaks 0.1 2 13 ret intensity
        )
    let peakToQuantify = BioFSharp.Mz.Quantification.HULQ.getPeakBy peaks ret
    BioFSharp.Mz.Quantification.HULQ.quantifyPeak peakToQuantify

    
[
    Chart.Point(xic)
    |> Chart.withTraceName "XIC"
    Chart.SplineArea(showQuantifiedXic xic quantifiedXIC)
    |> Chart.withTraceName "quantified XIC"
]
|> Chart.Combine
|> Chart.Show