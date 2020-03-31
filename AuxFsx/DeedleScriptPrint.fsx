#nowarn "211"

//#load @"../IfSharp/Paket.Generated.Refs.fsx"

//open FSharp.Compiler.Interactive.Shell.Settings

do fsi.AddPrinter(fun (printer:Deedle.Internal.IFsiFormattable) -> "\n" + (printer.Format()))
