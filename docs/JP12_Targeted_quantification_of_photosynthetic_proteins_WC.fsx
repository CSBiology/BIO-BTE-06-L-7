(**
# JP12 Targeted quantification of photosynthetic proteins (Whole Cell)

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=JP12_Targeted_quantification_of_photosynthetic_proteins_WC.ipynb)

1. [General steps for targeted quantification](#General-steps-for-targeted-quantification)
2. [Peptide Ratio Visualization](#Peptide-Ratio-Visualization)
3. [Sample stability](#Sample-stability)
*)

(**
## General steps for targeted quantification


1. Read the output file of the QconQuantifier containing the raw peptide ion quantification.
2. Performing a first data cleaning step
3. Calculating the <sup>14</sup>N/<sup>15</sup>N ratio per peptide ion per sample
4. Aggregating the peptide ions to their corresponding peptide
5. Calculating the average of the peptide quantification value for protein quantification
6. Visually inspect the peptide/protein quantification

For the explorative data analysis, we are using 
<a href="http://bluemountaincapital.github.io/Deedle/tutorial.html">Deedle</a>.
Deedle is an easy to use library for data and time series manipulation and for scientific programming. 
It supports working with structured data frames, ordered and unordered data, as well as time series. Deedle is designed to work well for exploratory programming using F#.
*)


#r "nuget: FSharp.Stats, 0.4.0"
#r "nuget: BioFSharp, 2.0.0-beta5"
#r "nuget: BioFSharp.IO, 2.0.0-beta5"
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: BIO-BTE-06-L-7_Aux, 0.0.1"
#r "nuget: Deedle, 2.3.0"

#if IPYNB
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: Plotly.NET.Interactive, 2.0.0-beta6"
#endif // IPYNB

open Deedle
open BioFSharp
open FSharpAux
open FSharp.Stats
open Plotly.NET
open FSharp.Stats.Fitting.LinearRegression.OrdinaryLeastSquares.Linear
open System.IO
open BIO_BTE_06_L_7_Aux.FS3_Aux

(**
At the start we have the output file of the QconQuantifier. We want to read the file, bind it to 
<code>qConcatRawData</code> and group the rows by peptide sequence, modifcation (<sup>14</sup>N or <sup>15</sup>N) and the charge state of the ion.
*)

// Code block 1

let directory = __SOURCE_DIRECTORY__
let path = Path.Combine[|directory;"downloads/Group1/G1_1690WC1zu1_QuantifiedPeptides.txt"|]
downloadFile path "G1_1690WC1zu1_QuantifiedPeptides.txt" "bio-bte-06-l-7/Group1"

let qConcatRawData =
    Frame.ReadCsv(path,separators="\t")
    // StringSequence is the peptide sequence
    |> Frame.indexRowsUsing (fun os -> 
            os.GetAs<string>("StringSequence"),
            os.GetAs<bool>("GlobalMod"), 
            os.GetAs<int>("Charge")
        )
        
qConcatRawData

(***include-it***)

(**
From literature we know that there are peptides with a very bad flyability 
(Hammel et al.). Additionally, there are extreme values only due to technical artifacts. Both should be avoided in further analysis:
*)

// Code block 2

let qConcatData =
    qConcatRawData
    |> Frame.filterRows ( fun (sequence, gmod, charge) _ -> 
        sequence <> "EVTLGFVDLMR" && sequence <> "AFPDAYVR" 
        )
    |> Frame.mapValues (fun x ->  if x < 2000000. && x > 1. then x else nan)

(**
Reading the sample description file provides us with a list of all measured files and additional information about the experiment (mixing ratio, strain, etc.) 
*)

// Code block 3

//FileName Experiment Content ProteinAmount[ug] Replicate

let path2 = Path.Combine[|directory;"downloads/Group1/WC_SampleDesc.txt"|]
downloadFile path2 "WC_SampleDesc.txt" "bio-bte-06-l-7/Group1"

let sampleDesc = 
    Frame.ReadCsv(path2 ,separators="\t",schema="Strain=string")
    |> Frame.indexRowsString "RawFileName"
    
sampleDesc

(***include-it***)

(**
We map the list of filenames and get the corresponding <sup>14</sup>N and <sup>15</sup>N column series. 
This allows us to calculate the <sup>14</sup>N/<sup>15</sup>N ratio per peptide ion per sample.
*)

// Code block 4

let ionRatios = 
    sampleDesc
    |> Frame.mapRows (fun rawFileName _ -> 
        let n14 = qConcatData.GetColumn<float>("N14Quant_" + rawFileName) 
        let n15 = qConcatData.GetColumn<float>("N15Quant_" + rawFileName)
        n14 / n15 
        )
    |> Frame.ofColumns
    
ionRatios

(***include-it***)

(**
From our in silico protein digest during the design of the qConCat protein, 
we know the peptide(s) &rarr; protein relationship. We read this information from the "PeptideProtMap.txt" file.
*)

// Code block 5

let path3 = Path.Combine[|directory;"downloads/PeptideProtMap.txt"|]
downloadFile path3 "PeptideProtMap.txt" "bio-bte-06-l-7"

let peptideProtMapping =
    Frame.ReadCsv(path3,hasHeaders=true,separators="\t")
    |> Frame.indexRowsString "Peptide"
    
peptideProtMapping

(***include-it***)

(**
Next, we will aggregate the peptide ion ratios to obtain one ratio per peptide sequence despite the ion charge. For convenience, we join the protein names.
*)

// Code block 6

let peptideRatios = 
    ionRatios
    |> Frame.applyLevel (fun (sequence,globalMod,charge) -> sequence) Stats.mean
    |> Frame.join JoinKind.Inner peptideProtMapping 
    |> Frame.groupRowsByString "Protein"
    |> Frame.getNumericCols
    |> Frame.ofColumns
    
peptideRatios

(***include-it***)

(**
Now, we join the sample description with the data.
*)

// Code block 7

let peptideRatiosWithDesc = 
    peptideRatios
    |> Frame.nest
    |> Series.map (fun k v -> 
        v
        |> Frame.transpose
        |> Frame.join JoinKind.Right sampleDesc
        |> Frame.indexRowsUsing (fun os -> 
                os.GetAs<string>("Strain"),
                os.GetAs<float>("Dilution")
            )
        |> Frame.filterCols (fun ck cs -> v.RowKeys |> Seq.contains ck)
        |> Frame.transpose
        )
    |> Frame.unnest
    
peptideRatiosWithDesc

(***include-it***)

(**
By calculating the mean value per protein, we have two final tables with peptide and protein ratios:
*)

// Code block 8

let proteinRatiosWithDesc =
    //peptideRatiosWithDesc
    peptideRatiosWithDesc
    |> Frame.applyLevel fst Stats.mean
    
proteinRatiosWithDesc

(***include-it***)

(**
Here are functions and parameters which are used for the styling of the graphs.
*)

// Code block 9

let xAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Zeroline=false,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside, Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))
let yAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside,Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))

let config = Config.init(ToImageButtonOptions = ToImageButtonOptions.init(Format = StyleParam.ImageFormat.SVG, Filename = "praktikumsplot.svg"), EditableAnnotations = [AnnotationEditOptions.LegendPosition])

(**
## Peptide Ratio Visualization

createChartForPeptideComparison</code> creates a chart for the given protein comparing the ratios for each given strain. 
It generates a chart for each strain showing the individual peptide ratios and their mean (protein ratio). It also compares the protein ratios for each strain.
*)

// Code block 10

// create charts to compare rel quant per dilution in strains
let createChartForPeptideComparison (protString:string) (strainStrings:string []) =
    
    let protFrame =
        peptideRatiosWithDesc.Nest()
        |> fun x -> x.Get(protString)

    let peptideStrings =
        protFrame.RowKeys
        |> Array.ofSeq

    let peptideRows : Series<string,Series<(string * float),float>> =
        protFrame
        |> Frame.getRows

    let protMeanFrame : Series<(string * float),float> =
        proteinRatiosWithDesc.GetRow protString

    let xyMean =
        strainStrings
        |> Array.map (fun strain ->
            strain,
            protMeanFrame
            |> Series.filter (fun k t -> fst k = strain)
            |> fun x -> x.Observations
            |> Seq.map (fun x -> 1./snd x.Key, x.Value)
            |> Array.ofSeq
        )

    let xyMeanChart =
        xyMean
        |> Array.map (fun (strain,xyMean) ->
            Chart.Scatter(xyMean,mode=StyleParam.Mode.Lines_Markers, MarkerSymbol = StyleParam.Symbol.Circle, Opacity=0.8)
            |> Chart.withTraceName (sprintf "%s - %s" protString strain )
        )
        |> Chart.Combine
        |> Chart.withX_Axis (xAxis false "Means: <sup>14</sup>N Sample/<sup>15</sup>N QProtein ratio" 20 16)
        |> Chart.withY_Axis (yAxis false "<sup>14</sup>N/<sup>15</sup>N Quantification ratio" 20 16)

    strainStrings
    |> Array.map (fun strain ->
        let peptideCharts =
            peptideStrings
            |> Array.map (fun peptide ->
                let strainValueSeries = 
                    peptideRows.[peptide] 
                    |> Series.filter (fun k t -> fst k = strain)
                let xy =
                    strainValueSeries.Observations
                    |> Seq.map (fun x -> 1./snd x.Key, x.Value)
                    |> Array.ofSeq
                Chart.Scatter(xy,mode=StyleParam.Mode.Markers, MarkerSymbol = StyleParam.Symbol.Cross)
                |> Chart.withTraceName (sprintf "%s -  %s - %s" strain protString peptide)
            )
        let relMeanChart = 
            let xyMeanVals = Array.find (fun (x,y) -> x = strain) xyMean |> snd
            Chart.Scatter(xyMeanVals,mode=StyleParam.Mode.Lines_Markers, MarkerSymbol = StyleParam.Symbol.Circle, Color="#366F8E", Opacity=0.8)
            |> Chart.withTraceName (sprintf "Mean %s - %s" protString strain)
        Array.append peptideCharts [|relMeanChart|]
        |> Chart.Combine
        |> Chart.withX_Axis (xAxis false (strain + ": <sup>14</sup>N Sample/<sup>15</sup>N QProtein ratio") 20 16)
        |> Chart.withY_Axis (yAxis false "<sup>14</sup>N/<sup>15</sup>N Quantification ratio" 20 16)
    )
    |> Array.append [|xyMeanChart|]
    |> Chart.Stack(2,Space=0.2)
    |> Chart.withSize (1200.,900.)
    |> Chart.withTitle (sprintf "Relative Quantification of %s" protString)
    |> Chart.withConfig config

(**
<code>rbclChart</code> executes <code>createChartForPeptideComparison</code> for rbcL and the strains 4A, 1690 and 1883. 
With <code>allCharts</code> you can generate charts for all proteins and strains (<span Style="color: red">Warning! This displays a lot of charts</span>).
*)

// Code block 11

let rbclChart = createChartForPeptideComparison "rbcL" [|"4A";"1690";"1883"|]

rbclChart

(***hide***)
rbclChart |> GenericChart.toChartHTML
(***include-it-raw***)


//let allCharts =
//    let strains = 
//        peptideRatiosWithDesc.ColumnKeys
//        |> Seq.map fst
//        |> Seq.distinct
//        |> Array.ofSeq
//        
//    peptideRatiosWithDesc.RowKeys
//    |> Seq.map (fun (prot,pep) ->
//        createChartForPeptideComparison prot strains
//    )
//allCharts

(**
## Sample stability

Next, we do a quality assessment for the whole-cell sample preparation. 
For that we will do a linear regression of the RuBisCO subunits relative quantification (<sup>14</sup>N/<sup>15</sup>N) protein ratio in whole-cell samples.

Next we need two peptides (in this case rbcl and rbcs) for the assessment. You can exchange them for other proteins if you want to.
*)

(**
Here, we fit a linear function to our mean peptide ratios and dilution series for the first protein. The  x-values are our different dilutions and the y-values our ratios. We calculate the <a href="https://en.wikipedia.org/wiki/Goodness_of_fit">goodness
 of the fit</a> (discrepancy between predicted and observed values) for each fit and and also the <a href="https://en.wikipedia.org/wiki/Pearson_correlation_coefficient">pearson correlation coefficient</a> (measure of the linear correlation between our ratios and dilution) for our values.
*)

// Code block 12

let calculateFit protName strainName (meanValueArray:(float*float) [])  =
    let dilutionsSorted,strainVals =
        meanValueArray
        |> Array.unzip
    // RBCL Regression of relative quantification values
    let RBCLcoeff = Univariable.coefficient (vector dilutionsSorted) (vector strainVals)
    let RBCLfitFunc = Univariable.fit RBCLcoeff
    let RBCLfitVals = dilutionsSorted |> Array.map RBCLfitFunc
    let RBCLdetermination = FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue strainVals RBCLfitVals
    let RBCLpearson = FSharp.Stats.Correlation.Seq.pearson strainVals dilutionsSorted
    printfn "%s - Pearson WholeCell %s: %f" strainName protName RBCLpearson
    RBCLcoeff, RBCLfitVals, RBCLdetermination

let meanValuesFor protName strainName=
    let meanSeries : Series<(string * float),float> = proteinRatiosWithDesc.GetRow protName
    meanSeries
    |> Series.filter (fun k t -> fst k = strainName)
    |> fun x -> x.Observations
    |> Seq.map (fun x -> 1./snd x.Key, x.Value)
    |> Array.ofSeq

meanValuesFor "rbcL" "4A"
//|> calculateFit "rbcL" "4A"

(**
<code>chartRatios</code> generates charts for each given strain and our chosen proteins. 
Each chart contains a comparison of the two proteins, showing their mean data points, the linear fit and the goodness of the fit.
*)

// Code block 13

let chartRatios prot1 prot2 strain =
    let prot1Vals = meanValuesFor prot1 strain
    let prot2Vals = meanValuesFor prot2 strain

    let (prot1Coeff:Vector<float>),prot1FitVals,prot1Determination =
        calculateFit prot1 strain prot1Vals

    let (prot2Coeff:Vector<float>),prot2FitVals,prot2Determination =
        calculateFit prot2 strain prot2Vals

    let dilutionsSorted,_ =
        prot1Vals // or prot2Vals, does not matter, as we only want x-axis
        |> Array.unzip
    [
        Chart.Point (prot1Vals,Name = sprintf "%s Quantified Ratios" prot1)
        |> Chart.withMarkerStyle(Size=10,Symbol = StyleParam.Symbol.Cross)
        Chart.Line(Array.zip dilutionsSorted prot1FitVals,Name = (sprintf "%s linear regression: %.2f x + (%2f) ; R = %.4f" prot1 prot1Coeff.[1] prot1Coeff.[0] prot1Determination))
        |> Chart.withLineStyle(Color="lightblue",Dash=StyleParam.DrawingStyle.DashDot)

        Chart.Point (prot2Vals,Name = sprintf "%s Quantified Ratios" prot2,MarkerSymbol = StyleParam.Symbol.Cross)
        |> Chart.withMarkerStyle(Size=10,Symbol = StyleParam.Symbol.Cross)
        Chart.Line(Array.zip dilutionsSorted prot2FitVals,Name = (sprintf "%s linear regression: %.2f x + (%2f) ; R = %.4f" prot2 prot2Coeff.[1] prot2Coeff.[0] prot2Determination))
        |> Chart.withLineStyle(Color="LightGreen",Dash=StyleParam.DrawingStyle.DashDot)
    ]
    |> Chart.Combine
    |> Chart.withTitle (sprintf "%s - Whole cell extracts: Stability of %s/%s ratios between samples" strain prot1 prot2)
    |> Chart.withX_Axis (yAxis false "<sup>14</sup>N Sample/<sup>15</sup>N QProtein ratio" 20 16)
    |> Chart.withY_Axis (xAxis false "relative quantification" 20 16 )
    |> Chart.withConfig config
    |> Chart.withSize (1200.,500.)

(**
Here we display the chart of rbcl and rbcs for the strain 4A.
*)

// Code block 14

chartRatios "rbcL" "RBCS2" "4A"

(***hide***)
rbclChart |> GenericChart.toChartHTML
(***include-it-raw***)

