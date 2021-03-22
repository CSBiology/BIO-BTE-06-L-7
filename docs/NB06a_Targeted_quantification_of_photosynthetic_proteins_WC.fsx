(**
# NB06a Targeted quantification of photosynthetic proteins (Whole Cell)

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=NB06a_Targeted_quantification_of_photosynthetic_proteins_WC.ipynb)

1. General steps for targeted quantification
2. Peptide Ratio Visualization
3. Sample stability

*)

(**
## General steps for targeted quantification


1. Read the output file of the QconQuantifier containing the raw peptide ion quantification.
2. Performing a first data cleaning step
3. Calculating the 14N/15N ratio per peptide ion per sample
4. Aggregating the peptide ions to their corresponding peptide
5. Calculating the average of the peptide quantification value for protein quantification
6. Visually inspect the peptide/protein quantification

For the explorative data analysis, we are using 
[Deedle](http://bluemountaincapital.github.io/Deedle/tutorial.html).
Deedle is an easy to use library for data and time series manipulation and for scientific programming. 
It supports working with structured data frames, ordered and unordered data, as well as time series. Deedle is designed to work well for exploratory programming using F#.
*)


#r "nuget: FSharp.Stats, 0.4.0"
#r "nuget: BioFSharp, 2.0.0-beta5"
#r "nuget: BioFSharp.IO, 2.0.0-beta5"
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: BIO-BTE-06-L-7_Aux, 0.0.3"
#r "nuget: Deedle, 2.3.0"
#r "nuget: ISADotNet, 0.2.2"
#r "nuget: ISADotNet.XLSX, 0.2.2"

#if IPYNB
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: Plotly.NET.Interactive, 2.0.0-beta6"
#endif // IPYNB

open ISADotNet
open ISADotNet.API
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
`qConcatRawData` and group the rows by peptide sequence, modifcation (14N or 15N) and the charge state of the ion.
*)

// Code block 1

let directory = __SOURCE_DIRECTORY__
let path = Path.Combine[|directory;"downloads/Sample.tab"|]
downloadFile path "Sample.tab" "bio-bte-06-l-7"

let qConcatRawData =
    Frame.ReadCsv(path,separators="\t")
    // StringSequence is the peptide sequence
    |> Frame.indexRowsUsing (fun os -> 
            os.GetAs<int>("ModSequenceID"),
            os.GetAs<int>("PepSequenceID"),
            os.GetAs<string>("StringSequence"),
            os.GetAs<string>("Proteingroup"), 
            os.GetAs<int>("Charge")
        )
        
qConcatRawData.Print()

(***include-fsi-merged-output***)

(**
From literature we know that there are peptides with a very bad flyability 
(Hammel et al.). Additionally, there are extreme values only due to technical artifacts. Both should be avoided in further analysis.
We also only want to keep the peptides that also appear in a QProtein:
*)

// Code block 2

let qConcatData =
    qConcatRawData
    |> Frame.filterRows ( fun (modID, pepID, sequence, protGroup, charge) _ -> 
        sequence <> "EVTLGFVDLMR" && sequence <> "AFPDAYVR" && (protGroup |> String.contains "Qprot_")
        )
    |> Frame.mapValues (fun x ->  if x < 2000000. && x > 0. then x else nan)

qConcatData.Print()

(**
Reading the sample description file provides us with a list of all measured files and additional information about the experiment (mixing ratio, strain, etc.) 
*)

// Code block 3

//FileName Experiment Content ProteinAmount[ug] Replicate

let path2 = Path.Combine[|directory;"downloads/alle_Gruppen_V2_SWATE.xlsx"|]
downloadFile path2 "alle_Gruppen_V2_SWATE.xlsx" "bio-bte-06-l-7"

let _,_,_,myAssayFile = XLSX.AssayFile.AssayFile.fromFile path2

let inOutMap = BIO_BTE_06_L_7_Aux.ISA_Aux.createInOutMap myAssayFile

let fileNames =
    myAssayFile.ProcessSequence.Value
    |> ProcessSequence.filterByProtocolName "Measurement"
    |> List.collect (fun p -> 
        p.Outputs.Value
        |> List.map (fun po ->
            ProcessOutput.getName po
        )
    )
    |> List.choose id
    |> List.filter (fun x -> x |> String.contains "WC")

let characteristicsOfInterest = 
    [
        "Cultivation -Sample preparation","strain"
        "Extraction","gram #2"
    ]

let sampleDesc =
    characteristicsOfInterest
    |> List.map (fun (prot,char) ->
        char,
        fileNames
        |> List.map (fun fn ->
            fn,       
            BIO_BTE_06_L_7_Aux.ISA_Aux.tryGetCharacteristic inOutMap prot char fn myAssayFile    
            |> Option.defaultValue ""
        )

        |> series
    )
    |> frame
    |> Frame.mapRowKeys (fun r -> r |> String.replace ".wiff" "")
    
sampleDesc.Print()

(***include-fsi-merged-output***)

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
    
peptideProtMapping.Print()

(***include-fsi-merged-output***)

(**
Next, we will aggregate the peptide value of choice (e.g. Ratio, Quant light, Quant heavy) to obtain one value per peptide sequence despite the ion charge. For convenience, we join the protein names.
*)

// Code block 6

type QuantificationValue =
    | Ratio
    | N14Quant
    | N15Quant

let quantificationValueToString (quantVal: QuantificationValue) =
    match quantVal with
    | Ratio    -> "Ratio"
    | N14Quant -> "MeasuredApex_Light"
    | N15Quant -> "MeasuredApex_Heavy"


let aggregatePepValue frame (quantificationValue: QuantificationValue) = 
    frame
    |> Frame.applyLevel (fun (modID, pepID, sequence, protGroup, charge) -> sequence) Stats.mean
    |> Frame.filterCols (fun ck os -> ck |> String.contains (quantificationValueToString quantificationValue))
    |> Frame.mapColKeys (fun ck -> ck.Replace("." + (quantificationValueToString quantificationValue), ""))
    |> Frame.join JoinKind.Inner peptideProtMapping 
    |> Frame.groupRowsByString "Protein"
    |> Frame.getNumericCols
    |> Frame.ofColumns
    
(aggregatePepValue qConcatData QuantificationValue.Ratio).Print()

(***include-fsi-merged-output***)

(**
Now, we join the sample description with the data.
*)

// Code block 7

let peptideValuesWithDesc frame (quantificationValue: QuantificationValue) = 
    aggregatePepValue frame quantificationValue
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
    
(peptideValuesWithDesc qConcatData QuantificationValue.Ratio).Print()

(***include-fsi-merged-output***)

(**
By calculating the mean value per protein, we have two final tables with peptide and protein values:
*)

// Code block 8

let proteinValuesWithDesc frame (quantificationValue: QuantificationValue) =
    //peptideRatiosWithDesc
    peptideValuesWithDesc frame quantificationValue
    |> Frame.applyLevel fst Stats.mean
    
(proteinValuesWithDesc qConcatData QuantificationValue.Ratio).Print()

(***include-fsi-merged-output***)

(**
Here are functions and parameters which are used for the styling of the graphs.
*)

// Code block 9

let xAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Zeroline=false,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside, Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))
let yAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside,Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))

let config = Config.init(ToImageButtonOptions = ToImageButtonOptions.init(Format = StyleParam.ImageFormat.SVG, Filename = "praktikumsplot.svg"), EditableAnnotations = [AnnotationEditOptions.LegendPosition])

(**
## Peptide Ratio Visualization

`createChartForPeptideComparison` creates a chart for the given protein comparing the ratios for each given strain. 
It generates a chart for each strain showing the individual peptide ratios and their mean (protein ratio). It also compares the protein ratios for each strain.
*)

// Code block 10
let peptideRatiosWithDesc = peptideValuesWithDesc qConcatData QuantificationValue.Ratio
let proteinRatiosWithDesc = proteinValuesWithDesc qConcatData QuantificationValue.Ratio

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
`rbclChart` executes `createChartForPeptideComparison` for rbcL and the strains 4A, 1690 and 1883. 
With `allCharts` you can generate charts for all proteins and strains ( ***Warning! This displays a lot of charts*** ).
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
For that we will do a linear regression of the RuBisCO subunits relative quantification (14N/15N) protein ratio in whole-cell samples.

Next we need two peptides (in this case rbcl and rbcs) for the assessment. You can exchange them for other proteins if you want to.
*)

(**
Here, we fit a linear function to our mean peptide ratios and dilution series for the first protein. The  x-values are our different dilutions and the y-values our ratios. We calculate the [goodness
 of the fit](https://en.wikipedia.org/wiki/Goodness_of_fit) (discrepancy between predicted and observed values) for each fit and and also the [pearson correlation coefficient](https://en.wikipedia.org/wiki/Pearson_correlation_coefficient) (measure of the linear correlation between our ratios and dilution) for our values.
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
`chartRatios` generates charts for each given strain and our chosen proteins. 
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

chartRatios "rbcL" "RCA1" "Test"

(***hide***)
chartRatios "rbcL" "RCA1" "Test" |> GenericChart.toChartHTML
(***include-it-raw***)

(**
## Abundance of 14N and 15N samples

Here we will take a look at the 14N and 15N quantifications without calculating their ratios to see wether they are stable along the dilutions.
We will do this once on the peptide and once on the protein level.
*)


let chartDilutionBoxplot (frame : Frame<'T,string*float>) (strains: string[]) (labeling: int) (proteins: bool) =
    let protOrPep =
        if proteins then "protein"
        else "peptide"
    frame.ColumnKeys
    |> Seq.toArray
    |> Array.groupBy fst
    |> Array.filter (fun (strain,_) -> strains |> Array.contains strain)
    |> Array.map (fun (strain,sd) -> 
        sd
        |> Array.sortBy snd
        |> Array.map (fun (str,dil) ->         
            frame.GetColumn<float>((str,dil))
            |> Series.values
            |> fun values -> Chart.BoxPlot(y = values, Name = string dil)
        )
        |> Chart.Combine
        |> Chart.withX_Axis (yAxis false "Dilution Series" 20 16)
        |> Chart.withY_Axis (xAxis false "Intensity" 20 16 )
        |> Chart.withTitle (sprintf "%s - Whole cell extracts: <sup>%i</sup>N %s intensities over the dilution series" strain labeling protOrPep)
        |> Chart.withConfig config
        |> Chart.withSize (1200.,700.)
    )


let peptideN14QuantsWithDesc = peptideValuesWithDesc qConcatData QuantificationValue.N14Quant
let proteinN14QuantsWithDesc = proteinValuesWithDesc qConcatData QuantificationValue.N14Quant

chartDilutionBoxplot peptideN14QuantsWithDesc [|"Test"|] 14 false

(***hide***)
chartDilutionBoxplot peptideN14QuantsWithDesc [|"Test"|] 14 false |> Array.map GenericChart.toChartHTML
(***include-it-raw***)

chartDilutionBoxplot proteinN14QuantsWithDesc [|"Test"|] 14 true

(***hide***)
chartDilutionBoxplot proteinN14QuantsWithDesc [|"Test"|] 14 true |> Array.map GenericChart.toChartHTML
(***include-it-raw***)

let peptideN15QuantsWithDesc = peptideValuesWithDesc qConcatData QuantificationValue.N15Quant
let proteinN15QuantsWithDesc = proteinValuesWithDesc qConcatData QuantificationValue.N15Quant

chartDilutionBoxplot peptideN15QuantsWithDesc [|"Test"|] 15 false

(***hide***)
chartDilutionBoxplot peptideN15QuantsWithDesc [|"Test"|] 15 false |> Array.map GenericChart.toChartHTML
(***include-it-raw***)

chartDilutionBoxplot proteinN15QuantsWithDesc [|"Test"|] 15 true

(***hide***)
chartDilutionBoxplot proteinN15QuantsWithDesc [|"Test"|] 15 true |> Array.map GenericChart.toChartHTML
(***include-it-raw***)