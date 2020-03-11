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