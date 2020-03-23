#r "IfSharp.Kernel.dll"

#load @".paket/load/main.group.fsx"

open FSharp.Plotly
open IfSharp.Kernel
open IfSharp.Kernel.Globals


do Printers.addDisplayPrinter(fun (printer:Deedle.Internal.IFsiFormattable) -> { ContentType = "text/html"; Data = "\n" + (printer.Format())})