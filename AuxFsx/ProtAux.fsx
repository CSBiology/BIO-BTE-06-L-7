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
open MzIO.Processing
open MzIO.Processing.MzIOLinq
open MzIO.IO
open MzIO
open MzIO.Model
open MzIO.MzSQL

let getReader (instrumentOutput:string) = 
    match System.IO.Path.GetExtension instrumentOutput with 
    | ".mzlite" ->
        let mzLiteReader = new MzSQL.MzSQL(instrumentOutput)
        mzLiteReader :> IMzIODataReader
    | _ ->
        failwith "Reader could not be opened. Only the format .mzlite (CSBiology) is supported."

let getDefaultRunID (mzReader:IMzIODataReader) = 
    match mzReader with
    | :? MzSQL -> "sample=0"
    | _ -> failwith "Invalid reader format"

let beginTransaction (mzReader:IMzIODataReader) =
    mzReader.BeginTransaction()

let endTransaction (transaction: ITransactionScope) =
    transaction.Dispose()

let showQuantifiedXic (xic: (float*float)[]) (quantifiedXic: QuantifiedPeak)= 
    xic 
    |> Array.map (fun (rt,i) -> 
                    match quantifiedXic.Model.Value with 
                    | Gaussian f -> rt, f.GetFunctionValue (quantifiedXic.EstimatedParams |>vector) rt
                    | _ -> failwith "not a gaussian function"
    )

let source = __SOURCE_DIRECTORY__

let dbPath = source + @"/../AuxFiles/sample.mzlite"

let runID = "sample=0"

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

let peptideSpectrumMatches calcIonSeries chargeState ms2PrecursorMZ lookUpResult (centroidedMs2: (float[]*float[])) =
    let theoSpec =
        lookUpResult
        |> List.map (fun lookUpResult -> 
            // lookUpResult will be the 'peptideOfInterest'
            let ionSeries = calcIonSeries lookUpResult.BioSequence
            lookUpResult,ionSeries
        )
        // (100.,1500.) is the M/Z range in which we are interested
        |> SequestLike.getTheoSpecs (100.,1500.) chargeState
    let peakArray =
        centroidedMs2
        |> fun (mass, intensity) ->
            PeakArray.zipMzInt mass intensity
    SequestLike.calcSequestScore
        (100.,1500.)
        // the MS2 spectra from 'ms2WithSpectra'
        peakArray
        0.
        2
        // the theoretical M/Z from 'theoMzCharge2'
        ms2PrecursorMZ
        theoSpec
        "sample=0 experiment=12 scan=1033"