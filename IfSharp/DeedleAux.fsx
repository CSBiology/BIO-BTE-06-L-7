#r "IfSharp.Kernel.dll"

#load @".paket/load/main.group.fsx"

namespace Deedle
open FSharp.Plotly
open IfSharp.Kernel
open IfSharp.Kernel.Globals
open Deedle


[<AutoOpen>]
module Frame =

    do Printers.addDisplayPrinter(fun (printer:Deedle.Internal.IFsiFormattable) -> { ContentType = "text/html"; Data = "\n" + (printer.Format())})

    /// Prints a frame
    let show (frame : Frame<'R,'C>) =
        frame.Print()

    /// Prints the first n columns of a frame
    let showTop n (frame : Frame<'R,'C>) =
        frame
        |> Frame.sliceCols (frame.ColumnKeys |> Seq.take n)
        |> fun x -> x.Print()

    /// Filters column keys for keys contained in columns
    let filterColsBySeq (columns : seq<'C>) (frame : Frame<'R,'C>) =
        frame
        |> Frame.filterCols (fun ck cs -> columns |> Seq.contains ck)