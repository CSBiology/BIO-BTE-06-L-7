#nowarn "211"

do fsi.AddPrinter(fun (printer:Deedle.Internal.IFsiFormattable) -> "\n" + (printer.Format()))