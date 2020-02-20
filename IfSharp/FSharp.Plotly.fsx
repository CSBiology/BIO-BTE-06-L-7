#r "IfSharp.Kernel.dll"

#load @".paket/load/main.group.fsx"

open FSharp.Plotly
open IfSharp.Kernel
open IfSharp.Kernel.Globals

do
    Printers.addDisplayPrinter(fun (plot: GenericChart.GenericChart) ->
        { ContentType = "text/html"; Data = GenericChart.toChartHTML plot })

    //System.Net.ServicePointManager.SecurityProtocol <- System.Net.SecurityProtocolType.Tls12
    //use wc = new System.Net.WebClient()
    
    sprintf
        """
<script type="text/javascript">
var require_save = require;
var requirejs_save = requirejs;
var define_save = define;
var MathJax_save = MathJax;
MathJax = require = requirejs = define = undefined;
%s
require = require_save;
requirejs = requirejs_save;
define = define_save;
MathJax = MathJax_save;
function ifsharpMakeImage(gd, fmt) {
    return Plotly.toImage(gd, {format: fmt})
        .then(function(url) {
            var img = document.createElement('img');
            img.setAttribute('src', url);
            var div = document.createElement('div');
            div.appendChild(img);
            gd.parentNode.replaceChild(div, gd);
        });
}
function ifsharpMakePng(gd) {
    var fmt =
        (document.documentMode || /Edge/.test(navigator.userAgent)) ?
            'svg' : 'png';
    return ifsharpMakeImage(gd, fmt);
}
function ifsharpMakeSvg(gd) {
    return ifsharpMakeImage(gd, 'svg');
}
</script>
"""
        //(wc.DownloadString("https://cdn.plot.ly/plotly-latest.min.js"))
        (System.IO.File.ReadAllText(__SOURCE_DIRECTORY__ + "/plotly-latest.min.js"))
        |> Util.Html
        |> Display

type GenericChart.GenericChart with

    member __.GetPngHtml() =
        let html =  GenericChart.toChartHTML __
        html
            .Replace("Plotly.newPlot(", "Plotly.plot(")
            .Replace(
                "data, layout);",
                "data, layout).then(ifsharpMakePng);")

    member __.GetSvgHtml() =
        let html =  GenericChart.toChartHTML __
        html
            .Replace("Plotly.newPlot(", "Plotly.plot(")
            .Replace(
                "data, layout);",
                "data, layout).then(ifsharpMakeSvg);")

type GenericChart.GenericChart with

    static member Png (chart: GenericChart.GenericChart) =
        { ContentType = "text/html"
          Data = chart.GetPngHtml()
        }

    static member Svg (chart: GenericChart.GenericChart) =
        { ContentType = "text/html"
          Data = chart.GetSvgHtml()
        }
