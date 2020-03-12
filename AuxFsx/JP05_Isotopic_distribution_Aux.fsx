
#load @"../IfSharp/Paket.Generated.Refs.fsx"

open BioFSharp
open FSharp.Plotly
open BioFSharp.Elements


let createBarChart inputData name =
    let x,y = inputData |> List.unzip
    let column =
        let dyn = Trace("bar")
        dyn?x <- x
        dyn?y <- y
        dyn?name <- name
        dyn?textposition <- "auto"
        dyn?textposition <- "outside"
        dyn?constraintext <- "inside"
        dyn?fontsize <- 20.
        dyn?width <- 0.5
        dyn?opacity <- 0.8
        dyn
    GenericChart.ofTraceObject column
    |> Chart.withX_AxisStyle("m/z")

let getMinOfIsotopeX (isoList:(float*_) list) =
    isoList |> (List.unzip >> fst >> List.min >> float) |> fun x -> x - 10.

let getMaxOfIsotopeX (isoList:(float*_) list) =
    isoList |> (List.unzip >> fst >> List.max >> float) |> fun x -> x + 10.