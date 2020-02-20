#r "IfSharp.Kernel.dll"

#load @".paket/load/main.group.fsx"

open IfSharp.Kernel
open IfSharp.Kernel.App
open IfSharp.Kernel.Globals

do
    System.Net.ServicePointManager.SecurityProtocol <- System.Net.SecurityProtocolType.Tls12
    use wc = new System.Net.WebClient()
    sprintf
        """
<script type="text/javascript">
if (loadedgooglecharts == undefined) {
%s
    var loadedgooglecharts = true;
}
</script>
"""
        (wc.DownloadString("https://www.gstatic.com/charts/loader.js"))
        |> Util.Html
        |> Display

type XPlot.GoogleCharts.GoogleChart with
  member __.GetContentHtml() =
    let html = __.GetInlineHtml()
    html
      .Replace ("google.charts.setOnLoadCallback(drawChart);", "google.charts.load('current',{ packages: ['corechart', 'annotationchart', 'calendar', 'gauge', 'geochart', 'map', 'sankey', 'table', 'timeline', 'treemap'], callback: drawChart });")

type XPlot.GoogleCharts.Chart with
  static member Content (chart : XPlot.GoogleCharts.GoogleChart) =
    { ContentType = "text/html"; Data = chart.GetContentHtml() }

AddDisplayPrinter (fun (plot: XPlot.GoogleCharts.GoogleChart) -> { ContentType = "text/html"; Data = plot.GetContentHtml() })
