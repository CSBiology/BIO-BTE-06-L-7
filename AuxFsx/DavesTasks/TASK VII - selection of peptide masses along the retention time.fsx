#load "references.fsx"

open System
open BioFSharp.Mz
open FsMzLite
open FSharp.Plotly

// Accesses ms spectra
let fileName = @"task7.wiff"

let dbPath = (__SOURCE_DIRECTORY__ + "/data/" + fileName)

let runID = "sample=0";

let reader = new MzLite.Wiff.WiffFileReader(dbPath)

///
let retTimeIdx = 
    let trans = reader.BeginTransaction() 
    let idx = FsMzLite.Query.getMS1RTIdx reader runID;
    trans.Dispose()
    idx   
   
///   
let ret = 27.68108333

///
let precMZ = 596.3373454

///
let rtQuery = FsMzLite.Query.createRangeQuery ret 2.

///
let mzQuery = FsMzLite.Query.createRangeQuery precMZ 0.04

///
let xic = 
    FsMzLite.Query.getXIC reader retTimeIdx rtQuery mzQuery  
    |> Array.map (fun p -> p.Rt , p.Intensity)
    
Chart.Point(xic)
|> Chart.Show

///
let quantifiedXIC = 
    let rtData,intensityData = xic |> Array.unzip
    Quantification.MyQuant.quantify Quantification.MyQuant.idxOfClosestLabeledPeak 5 1. 20. ret rtData intensityData 
    
let quantFit = 
    let quantProps = quantifiedXIC.Value
    let xData,yData = xic |> Array.unzip
    let fittedValues = 
        xData 
        |> Array.map (quantProps.SelectedModel.GetFunctionValue quantProps.EstimatedParameters) 
    [
    Chart.Point(xic)
    Chart.Spline(xData,fittedValues)
    ]
    |> Chart.Combine    
    |> Chart.Show
    
       