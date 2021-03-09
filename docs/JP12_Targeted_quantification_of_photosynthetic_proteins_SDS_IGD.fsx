(**
# JP12 Targeted quantification of photosynthetic proteins (SDS in gel digest)

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=JP12_Targeted_quantification_of_photosynthetic_proteins_SDS_IGD.ipynb)

1. [Relative Quantification between rbcL and rbcS](#Relative-Quantification-between-rbcL-and-rbcS)<br>
2. [Compare 14N/15N for rbcL and rbcS](#Compare-14N/15N-for-rbcL-and-rbcS)
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
At start we have the output file of the QconQuantifier. We want to read the file, bind it to `qConcatRawData` 
and group the rows by peptide sequence, modifcation (14N or 15N) and the charge state of the ion.
*)

// Code block 1

let directory = __SOURCE_DIRECTORY__
let path = Path.Combine[|directory;"downloads/Group2/G2_L_4A_20myg_QuantifiedPeptides.txt"|]
downloadFile path "G2_L_4A_20myg_QuantifiedPeptides.txt" "bio-bte-06-l-7/Group2"

let qConcatRawData =
    Frame.ReadCsv(path = path,separators="\t")
    // StringSequence is the peptide sequence
    |> Frame.indexRowsUsing (fun os -> 
            os.GetAs<string>("StringSequence"),
            os.GetAs<bool>("GlobalMod"), 
            os.GetAs<int>("Charge")
        )
        
qConcatRawData

(***include-it***)

(**
From literature we know that there are peptides with a very bad flyability (Hammel et al.). Additionally, 
there are extreme values only due to technical artifacts. Both should be avoided in further analysis:
*)

// Code block 2

let qConcatData =
    qConcatRawData
    |> Frame.filterRows ( fun (sequence, gmod, charge) _ -> 
        sequence <> "EVTLGFVDLMR" && sequence <> "AFPDAYVR" 
        )
    |> Frame.mapValues (fun x ->  if x < 2000000. && x > 1. then x else nan)
    
qConcatData

(***include-it***)

(**
Reading the sample description file provides us with a list of all measured files and additional information about 
the experiment (mixing ratio, strain, etc.). Here you need to write ***YOUR*** filenames into the .txt file!
*)

// Code block 3

let path2 = Path.Combine[|directory;"downloads/Group2/IGD_SampleDesc.txt"|]
downloadFile path2 "IGD_SampleDesc.txt" "bio-bte-06-l-7/Group2"

//FileName CutOutBand Dilution Strain
let sampleDesc :Frame<string,string>= 
    Frame.ReadCsv(path = path2,separators="\t",schema="Strain=string")
    |> Frame.indexRows "RawFileName"
    
sampleDesc

(***include-it***)

(**
We map the list of filenames and get the corresponding 14N and 15N column series. 
This allows us to calculate the 14N/15N ratio per peptide ion per sample.
*)

// Code block 4

let ionRatios = 
    sampleDesc
    |> Frame.mapRows (fun rawFileName _ -> 
        let n14 = 
            qConcatData.GetColumn<float>("N14Quant_" + rawFileName) 
            |> Series.filterValues (fun x ->  x < 2000000. && x > 1. )
        let n15 = 
            qConcatData.GetColumn<float>("N15Quant_" + rawFileName) 
            |> Series.filterValues (fun x ->  x < 2000000. && x > 1. )
        n14 / n15 
        )
    |> Frame.ofColumns

ionRatios

(***include-it***)

(**
From our in silico protein digest during the design of the qConCat protein, we know the peptide(s) &rarr; 
protein relationship. We read this information from the "PeptideProtMap.txt" file.
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

let peptideRatiosWithDesc : Frame<(string * string),(string * (string * float))>=
    peptideRatios
    |> Frame.mapColKeys (fun rk ->
        sampleDesc.GetColumn("CutOutBand").[rk],
        (sampleDesc.GetColumn("Strain").[rk],
         sampleDesc.GetColumn("Dilution").[rk])
    )
    
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

(**

Next we want to compare the quantities between the cut-out band for rbcL and the cut-out band for rbcS. 
Therefore we divide the rbcL quantities from the rbcL cut-out by the rbcS quantities from the rbcS cut.out.
*)

// Code block 9

let calcultateRelativeQuantForCutOutsProteins (prot1) (prot2) frame =
    let nestedFrame: Series<string,Frame<'a,string>> =
        frame
        |> Frame.transpose
        |> Frame.nest
    nestedFrame.["RBCL"] 
    |> Frame.filterCols (fun ck cs -> ck = prot1)
    |> Frame.mapCols (fun ck _ ->
        let rbclSeries =
            nestedFrame.["RBCL"].GetColumn<float>prot1
        let rbcsSeries =
            nestedFrame.["RBCS"].GetColumn<float>prot2
        rbclSeries / rbcsSeries
    )
    |> Frame.mapColKeys (fun ck -> prot1 + "/" + prot2)
    |> Frame.transpose

let proteinRatiosWithDescCutOuts prot1 prot2=
    proteinRatiosWithDesc
    |> calcultateRelativeQuantForCutOutsProteins prot1 prot2
    
let rbclRBCS2 = proteinRatiosWithDescCutOuts "rbcL" "RBCS2"

rbclRBCS2
|> Frame.transpose

(***include-it***)

(**
Here are functions and parameters which are used for the styling of the graphs.
*)

// Code block 10

let xAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Zeroline=false,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside, Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))
let yAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside,Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))

let config = Config.init(ToImageButtonOptions = ToImageButtonOptions.init(Format = StyleParam.ImageFormat.SVG, Filename = "praktikumsplot.svg"), EditableAnnotations = [AnnotationEditOptions.LegendPosition])

(**
## Relative Quantification between rbcL and rbcS
At first we calculated the 14N/15N relative quantities for rbcL and rbcS. Then, as we normalized against the same 
QProtein we can now calculate the relation of subunits from rbcL and rbcS, both from their respective cut-out band.

First we will access the data for rbcL and rbcS for a given strain from our Deedle frame.
*)

// Code block 11

/////////////////////////////////// Chart Step 1 //////////////////////////////////////////

open FSharp.Stats.Fitting.LinearRegression.OrdinaryLeastSquares.Linear

// access the data for rbcL and rbcS for a given strain
let meanValuesFor prot1Name prot2Name strainName=
    let meanSeries : Series<(string * float),float> = rbclRBCS2.GetRow (prot1Name+"/"+prot2Name)
    meanSeries
    |> Series.filter (fun k t -> fst k = strainName)
    |> fun x -> x.Observations
    |> Seq.map (fun x -> snd x.Key, x.Value)
    |> Array.ofSeq
    
let testMeanValues =
    meanValuesFor "rbcL" "RBCS2" "4A"
    
testMeanValues

(***include-it***)

(**
In addition we will display the pearson coefficient for all different given dilutions.
*)

// Code block 12

//let prot1Coeff,prot1FitVals,prot1Determination =
let calculatePearson prot1Name prot2Name strainName (meanValueArray:(float*float) [])  =
    let dilutionsSorted,strainVals =
        meanValueArray
        |> Array.unzip
    // RBCL Regression of relative quantification values
    let RBCLcoeff = Univariable.coefficient (vector dilutionsSorted) (vector strainVals)
    let RBCLfitFunc = Univariable.fit RBCLcoeff
    let RBCLfitVals = dilutionsSorted |> Array.map RBCLfitFunc
    let RBCLdetermination = FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue strainVals RBCLfitVals
    let RBCLpearson = FSharp.Stats.Correlation.Seq.pearson strainVals dilutionsSorted
    printfn "%s - Pearson WholeCell %s: %f" strainName (prot1Name+"/"+prot2Name) RBCLpearson
    RBCLcoeff, RBCLfitVals, RBCLdetermination
    
testMeanValues
|> calculatePearson "rbcL" "RBCS2" "4A"

(***include-it***)

// Code block 13

let chartRatios prot1 prot2 strain =
    let prot1Vals = meanValuesFor prot1 prot2 strain

    let (prot1Coeff:Vector<float>),prot1FitVals,prot1Determination =
        calculatePearson prot1 prot2 strain prot1Vals

    let dilutionsSorted,_ =
        prot1Vals
        |> Array.unzip

    [
        Chart.Point (prot1Vals,Name = sprintf "%s Quantified Ratios" (prot1+"/"+prot2))
        |> Chart.withMarkerStyle(Size=10,Symbol = StyleParam.Symbol.Cross)
        Chart.Line(Array.zip dilutionsSorted prot1FitVals,Name = (sprintf "%s linear regression: %.2f x + (%2f) ; R = %.4f" (prot1+"/"+prot2) prot1Coeff.[1] prot1Coeff.[0] prot1Determination))
        |> Chart.withLineStyle(Color="lightblue",Dash=StyleParam.DrawingStyle.DashDot)
    ]
    |> Chart.Combine
    |> Chart.withTitle (sprintf "%s - In-Gel-Digest: Stability of %s/%s ratios between samples" strain prot1 prot2)
    |> Chart.withX_Axis (xAxis false (sprintf "Cut Out 1 - %s / Cut Out 2 - %s" prot1 prot2) 20 16)
    |> Chart.withY_Axis (xAxis false "relative quantification" 20 16 )
    |> Chart.withConfig config
    |> Chart.withSize (1000.,400.)

chartRatios "rbcL" "RBCS2" "1690"

(***hide***)
chartRatios "rbcL" "RBCS2" "1690" |> GenericChart.toChartHTML
(***include-it-raw***)

(**
## Compare 14N/15N for rbcL and rbcS
To calculate absolute quantities we need to know the relation of 14N protein to the 15N QProtein. 
We will show these relation with the following charts.
We add a linear fit to the charts to allow for more precise evaluation of sample linearity.
*)

// Code block 14

/////////////////////////////////// Chart Step 2 //////////////////////////////////////////

let calculateLinearFit ((amount,quant): float[]*float[]) =
    // Calculation of the coefficients for a linear fit.
    let coeffs =
        Univariable.coefficient (vector amount) (vector quant)
    // Calculation of the linear fit with the coefficients.
    let linearFitFunc =
        Univariable.fit coeffs
    // Here, we apply the fitting function to our x-values (amount of loaded protein) 
    // to get the corresponding fitted values.
    let linearFitVals =
        amount
        |> Array.map linearFitFunc
    // Calculation of the goodness of the fit by comparing our calculated 
    // Protein1/Protein2 ratios to the values of the fit.
    let determination =
        FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue quant linearFitVals
    {|Coefficients = coeffs; LinearFitValues = linearFitVals; Determination = determination; LoadedProtein = amount|}

(***include-it***)

// Code block 15

let createChartForRbcsRbclComparison prot1 prot2 strain =

    let nestedFrame =
        proteinRatiosWithDesc
        |> Frame.filterRows (fun rk rs -> rk = prot1 || rk = prot2)
        |> Frame.transpose
        |> Frame.nest     

    let getProtValuesFromSeries series =
        series
        |> Series.filter (fun k t -> fst k = strain)
        |> fun x -> x.Observations 
        |> Array.ofSeq
        |> Array.map (fun x -> snd x.Key, x.Value)

    let rbcLRBCLValues =
        nestedFrame.["RBCL"].GetColumn<float>prot1
        |> getProtValuesFromSeries

    let rbcs2RBCSValues =
        nestedFrame.["RBCS"].GetColumn<float>prot2
        |> getProtValuesFromSeries

    let rbclFit =
        calculateLinearFit (Array.unzip rbcLRBCLValues)

    let rbcsFit =
        calculateLinearFit (Array.unzip rbcs2RBCSValues)

    let fitChart =
        [
            Chart.Line(Array.zip rbclFit.LoadedProtein rbclFit.LinearFitValues)
            |> Chart.withTraceName (sprintf "linear regression: %.2f x + (%2f) ; R² = %.4f for strain %s and %s"
                rbclFit.Coefficients.[1] rbclFit.Coefficients.[0] rbclFit.Determination strain prot1)
            |> Chart.withLineStyle(Color="#D3D3D3",Dash=StyleParam.DrawingStyle.DashDot)
            Chart.Line(Array.zip rbcsFit.LoadedProtein rbcsFit.LinearFitValues)
            |> Chart.withTraceName (sprintf "linear regression: %.2f x + (%2f) ; R² = %.4f for strain %s and %s"
                rbcsFit.Coefficients.[1] rbcsFit.Coefficients.[0] rbcsFit.Determination strain prot2)
            |> Chart.withLineStyle(Color="#D3D3D3",Dash=StyleParam.DrawingStyle.DashDot)
        ]
        |> Chart.Combine

    let dataChart =
        [
            Chart.Scatter(rbcLRBCLValues,mode=StyleParam.Mode.Lines_Markers, MarkerSymbol = StyleParam.Symbol.Circle, Opacity=0.8)
            |> Chart.withTraceName (sprintf "Mean %s - %s" prot1 strain)
            Chart.Scatter(rbcs2RBCSValues,mode=StyleParam.Mode.Lines_Markers, MarkerSymbol = StyleParam.Symbol.Circle, Opacity=0.8)
            |> Chart.withTraceName (sprintf "Mean %s - %s" prot2 strain)
        ]
        |> Chart.Combine
        |> Chart.withX_Axis (xAxis false "Loaded protein [µg]" 20 16)
        |> Chart.withY_Axis (xAxis false "<sup>14</sup>N/<sup>15</sup>N Quantification ratio" 20 16)

    [fitChart;dataChart]
    |> Chart.Combine
    |> Chart.withTitle (sprintf "%s/%s relative protein quantification" prot1 prot2)
    |> Chart.withSize (1200.,400.)

let prot1 = "rbcL"
let prot2 = "RBCS2"

[|"4A"; "1883";"1690"|]
|> Array.map (createChartForRbcsRbclComparison prot1 prot2)

(***hide***)
[|"4A"; "1883";"1690"|]
|> Array.map (createChartForRbcsRbclComparison prot1 prot2 >> GenericChart.toChartHTML)
(***include-it-raw***)