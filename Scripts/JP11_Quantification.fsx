
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

let ms2 = 
    BioFSharp.IO.Mgf.readMgf (__SOURCE_DIRECTORY__ + @"/../AuxFiles/DavesTaskData/ms2MGF.mgf")
    |> List.head

let centroidedMs2 =
    ms2PeakPicking ms2.Mass ms2.Intensity

///
let peptideOfInterest = 
    BioFSharp.Mz.SearchDB.createLookUpResult 3439282 302200012 1190.654689 1190654689L "VYVVGEDGILK" (BioList.ofAminoAcidString "VYVVGEDGILK") 0

// calculate the M/Z value for the 'peptideOfInterest'
let theoMzCharge2 = 
    Mass.toMZ peptideOfInterest.Mass 2.

// this is used to create simulated MS2 spectra for a peptide of interest
let calcIonSeries aal = 
    Fragmentation.Series.fragmentMasses 
        Fragmentation.Series.bOfBioList 
        Fragmentation.Series.yOfBioList 
        BioItem.initMonoisoMassWithMemP 
        aal

let ionSeries =
    peptideOfInterest
    |> fun x -> calcIonSeries x.BioSequence

let theoSpec = BioFSharp.Mz.SequestLike.getTheoSpecs (100.,1500.) 2 [(peptideOfInterest, ionSeries)]
        
let score = 
    // get all peptide spectrum matches
    let results = peptideSpectrumMatches 2 theoMzCharge2 [peptideOfInterest] centroidedMs2 theoSpec
    match results with 
    // Should no match be found, return None
    | []  -> None 
    // Should there be at least one match, get the first value (the one with the highest score)
    | h::tail -> Some h 

let inDb = matchInDB 2. peptideOfInterest

Chart.Point inDb
|> Chart.withX_AxisStyle "Retention Time"
|> Chart.withY_AxisStyle "Score"
|> Chart.Show


///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

let reader = getReader dbPath
let tr = beginTransaction reader

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

// normalize score to the maximal found score
let normRetTimeAndScore =
    let retTimeAndScore = matchInDB 2. peptideOfInterest
    let maxScore = 
        retTimeAndScore
        |> List.maxBy snd 
        |> snd 
    retTimeAndScore
    |> List.map (fun (ret,score) -> ret, (score / maxScore))

let xicVis = 
    Chart.Point(normXic)
    |> Chart.withTraceName "Normalized XIC"

let psmVis = 
    Chart.Point(normRetTimeAndScore)
    |> Chart.withTraceName "Normalized Score"

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