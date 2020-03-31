
#load @"../IfSharp/References.fsx"
#load @"../IfSharp/Paket.Generated.Refs.fsx"
#load @"../AuxFsx/DeedleScriptPrint.fsx"

open Deedle
open FSharpAux
open FSharp.Stats
open FSharp.Plotly

let xAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Zeroline=false,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside, Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))

let config = Config.init(ToImageButtonOptions = ToImageButtonOptions.init(Format = StyleParam.ImageFormat.SVG, Filename = "praktikumsplot.svg"), EditableAnnotations = [AnnotationEditOptions.LegendPosition])

let isBad a =
    nan.Equals(a) || infinity.Equals(a) || (-infinity).Equals(a)

//================================================================================
//====================== 3. SDS IGD ==============================================
//================================================================================

let source = __SOURCE_DIRECTORY__

let qConcatRawData =
    Frame.ReadCsv(path = (source + @"\..\AuxFiles\GroupsData\G2_L_4A_20myg_QuantifiedPeptides.txt"),separators="\t")
    |> Frame.indexRowsUsing (fun os -> 
            os.GetAs<string>("StringSequence"),
            os.GetAs<bool>("GlobalMod"), 
            os.GetAs<int>("Charge")
    )

let qConcatData =
    qConcatRawData
    |> Frame.filterRows (fun (sequence, gmod, charge) _ -> sequence <> "EVTLGFVDLMR" && sequence <> "AFPDAYVR"  )

//FileName Experiment Content ProteinAmount[ug] Replicate
let sampleDesc :Frame<string,string>= 
    Frame.ReadCsv(path = (source + @"\..\AuxFiles\IGD_SampleDesc.txt"),separators="\t",schema="Strain=string")
    |> Frame.indexRows "RawFileName"

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

let peptideProtMapping =
    Frame.ReadCsv(source + @"\..\AuxFiles\PeptideProtMap.txt",hasHeaders=true,separators="\t")
    |> Frame.indexRowsString "Peptide"

let peptideRatios = 
    ionRatios
    |> Frame.applyLevel (fun (sequence,globalMod,charge) -> sequence) Stats.mean
    |> Frame.join JoinKind.Inner peptideProtMapping 
    |> Frame.groupRowsByString "Protein"
    |> Frame.getNumericCols
    |> Frame.ofColumns

let peptideRatiosWithDesc : Frame<(string * string),(string * (string * float))>=
    peptideRatios
    |> Frame.mapColKeys (fun rk ->
        sampleDesc.GetColumn("CutOutBand").[rk],
        (sampleDesc.GetColumn("Strain").[rk],
         sampleDesc.GetColumn("Dilution").[rk])
    )

let proteinRatiosWithDesc =
    //peptideRatiosWithDesc
    peptideRatiosWithDesc
    |> Frame.applyLevel fst Stats.mean

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

/////////////////////////////////// Chart Step 1 - Fitting //////////////////////////////////////////

open FSharp.Stats.Fitting.LinearRegression.OrdinaryLeastSquares.Linear

let meanValuesFor prot1Name prot2Name strainName=
    let meanSeries : Series<(string * float),float> = rbclRBCS2.GetRow (prot1Name+"/"+prot2Name)
    meanSeries
    |> Series.filter (fun k t -> fst k = strainName)
    |> fun x -> x.Observations
    |> Seq.map (fun x -> snd x.Key, x.Value)
    |> Array.ofSeq

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

meanValuesFor "rbcL" "RBCS2" "4A"
|> calculatePearson "rbcL" "RBCS2" "4A"

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
    |> Chart.withSize (900.,500.)
    |> Chart.Show

chartRatios "rbcL" "RBCS2" "1883"

/////////////////////////////////// Chart Step 1 - Compare CUTOUT1 rbcL with CUTOUT2 RBCS2 //////////////////////////////////////////

let calculateLinearFit ((amount,quant): float[]*float[]) =
    let coeffs =
        Univariable.coefficient (vector amount) (vector quant)
    let linearFitFunc =
        Univariable.fit coeffs
    let linearFitVals =
        amount
        |> Array.map linearFitFunc
    let determination =
        FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue quant linearFitVals
    {|Coefficients = coeffs; LinearFitValues = linearFitVals; Determination = determination; LoadedProtein = amount|}


let createChartForRbcsRbclComparison strain =
    
    let prot1 = "rbcL"
    let prot2 = "RBCS2"

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
    |> Chart.withSize (1200.,600.)

[|"4A"; "1883";"1690"|]
|> Array.map (createChartForRbcsRbclComparison >> Chart.Show)