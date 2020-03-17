#load @"../IfSharp/References.fsx"
#load @"../IfSharp/Paket.Generated.Refs.fsx"


open System.IO
open BioFSharp.IO
open BioFSharp.Mz
open BioFSharp.Mz.Ions
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

let calcIonSeries aal = 
    Fragmentation.Series.fragmentMasses 
        Fragmentation.Series.bOfBioList 
        Fragmentation.Series.yOfBioList 
        BioItem.initMonoisoMassWithMemP 
        aal

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

let peptideSpectrumMatches ms2PrecursorMZ (centroidedMs2: (float[]*float[])) theoSpec =
    //let theoSpec =
    //    lookUpResult
    //    |> List.map (fun lookUpResult -> 
    //        // lookUpResult will be the 'peptideOfInterest'
    //        let ionSeries = calcIonSeries lookUpResult.BioSequence
    //        lookUpResult,ionSeries
    //    )
    //    // (100.,1500.) is the M/Z range in which we are interested
    //    |> SequestLike.getTheoSpecs (100.,1500.) chargeState
    let peakArray =
        centroidedMs2
        |> fun (x,y) -> Array.zip x y
        |> PeakArray.zipMzInt

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

let matchInDB charge (lookUpResult: LookUpResult<AminoAcids.AminoAcid>)  =
    let source = __SOURCE_DIRECTORY__
    let dbPath = source + @"/../AuxFiles/sample.mzlite"
    let runID = "sample=0"
    let reader =
        new MzSQL.MzSQL(dbPath)
        :> MzIO.IO.IMzIODataReader
    let tr = reader.BeginTransaction()
    let theoMzCharge = 
        Mass.toMZ lookUpResult.Mass charge
    let ms2Headers = 
        Processing.MassSpectrum.getMassSpectraBy reader runID
        |> Seq.filter (fun ms -> Processing.MassSpectrum.getMsLevel ms = 2)
        |> Seq.filter (fun ms2 -> 
            let precMz = Processing.MassSpectrum.getPrecursorMZ ms2 
            precMz > theoMzCharge-0.05 && precMz < theoMzCharge+0.05
        )
    let ms2WithSpectra = 
        ms2Headers
        |> Seq.map (fun ms2Header -> 
            let ms2ID = Processing.MassSpectrum.getID ms2Header
            let peaks = 
                reader.ReadSpectrumPeaks(ms2ID).Peaks
                |> Seq.map (fun p-> Peak(p.Mz,p.Intensity))
                |> Array.ofSeq
            ms2Header, peaks
        )
    tr.Dispose() |> ignore
    let peptideSpectrumMatches chargeState ms2PrecursorMZ lookUpResult centroidedMs2 (ms2: Model.MassSpectrum) =
        let theoSpec =
            lookUpResult
            |> List.map (fun lookUpResult -> 
                let ionSeries = calcIonSeries lookUpResult.BioSequence
                lookUpResult,ionSeries
            )
            |> SequestLike.getTheoSpecs (100.,1500.) chargeState
        SequestLike.calcSequestScore
            (100.,1500.)
            centroidedMs2
            (MassSpectrum.getScanTime ms2)
            chargeState
            ms2PrecursorMZ
            theoSpec
            ms2.ID
    let score = 
        ms2WithSpectra
        |> Seq.map (fun (ms2Header,centroidedPeaks) -> 
            let score = 
                let results = peptideSpectrumMatches (int charge) theoMzCharge [lookUpResult] centroidedPeaks ms2Header
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
        |> List.sortByDescending (fun (ms2Header, score) -> score.Score) 
        |> List.map (fun (ms2Header, score) -> 
            let retTime = Processing.MassSpectrum.getScanTime ms2Header
            retTime,score.Score 
        )
    retTimeAndScore


let inp1 = "JP01_Systems_Biology_FSharp_Introduction.ipynb"
let inp2 = "JP03_Mass_spectrometry_based_proteomics.ipynb"

open System

let showbuttons (str1:string option) (str2:string option) =
    let button1 =
        if str1.IsNone then ""
        else 
            let name = str1.Value.Split([|"_"|],StringSplitOptions.RemoveEmptyEntries).[0]
            sprintf 
                "<a target=\"_blank\" href=\"%s\" title=\"%s\" class=\"mySelectButton-left cancela\">
                    <span>
                        %s
                    </span>
                </a>"
                str1.Value
                str1.Value
                name

    let button2 =
        if str2.IsNone then ""
        else 
            let name = str2.Value.Split([|"_"|],StringSplitOptions.RemoveEmptyEntries).[0]
            sprintf
                "<a target=\"_blank\" href=\"%s\" title=\"%s\" class=\"mySelectButton-right cancela\">
                    <span>
                        %s
                    </span>
                </a>"
                str2.Value
                str2.Value
                name

    sprintf 
        "
        <head>
        <style>
        .mySelectButton-right {
                display: inline-block;
                border-radius: 4px;
                background-color: blue;
                border: none;
                color: white;
                text-align: center;
                font-size: 18px;
                width: 120px;
                padding: 20px;
                transition: all 0.5s;
                cursor: pointer;
                margin: 5px;
                margin-left: auto;
            }
            
            /*style when clicked*/
            .mySelectButton-right:active {
              box-shadow: 0 5px #666;
              transform: translateY(4px);
            }
            
            .mySelectButton-right::-moz-focus-inner {
                border: 0;
            }
            
            .mySelectButton-right span {
                cursor: pointer;
                display: inline-block;
                color: white;
                position: relative;
                transition: 0.5s;
            }
            
            .mySelectButton-right span:after {
                content: '\00bb';
                position: absolute;
                color: white;
                opacity: 0;
                top: 0;
                right: -20px;
                transition: 0.5s;
            }
            
            .mySelectButton-right:hover span {
                padding-right: 25px;
            }
            
            .mySelectButton-right:hover span:after {
                opacity: 1;
                right: 0;
            }
            
        
        .mySelectButton-left {
            display: inline-block;
            border-radius: 4px;
            background-color: blue;
            border: none;
            color: white;
            text-align: center;
            font-size: 18px;
            width: 120px;
            padding: 20px;
            transition: all 0.5s;
            cursor: pointer;
            margin: 5px;
            margin-right: auto;
        }
        
        /*style when clicked*/
        .mySelectButton-left:active {
          box-shadow: 0 5px #666;
          transform: translateY(4px);
        }
        
        .mySelectButton-left::-moz-focus-inner {
            border: 0;
        }
        
        .mySelectButton-left span {
            cursor: pointer;
            display: inline-block;
            position: relative;
            transition: 0.5s;
            color: white;
        }
        
        .mySelectButton-left span::before {
            content: '\00AB';
            color: white;
            position: absolute;
            opacity: 0;
            top: 0;
            left: -20px;
            transition: 0.5s;
        }
        
        .mySelectButton-left:hover span {
            padding-left: 25px;
        }
        
        .mySelectButton-left:hover span::before {
            opacity: 1;
            left: 0;
        }
        
        .cancela,.cancela:link,.cancela:visited,.cancela:hover,.cancela:focus,.cancela:active{
            color: inherit;
            text-decoration: none;
        }
        
        </style>
        </head>
        <body>
        
        <div Style=\"display: flex ; justify-content: space-between ; width: 100%% \">
            %s
            %s
        </div>
        
        </body>
        "
        button1
        button2