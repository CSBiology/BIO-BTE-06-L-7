
#load @"../IfSharp/References.fsx"
#load @"../IfSharp/Paket.Generated.Refs.fsx"
#load @"../AuxFsx/DeedleAux.fsx"

open Deedle
open FSharpAux
open FSharp.Stats
open FSharp.Plotly

let xAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Zeroline=false,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside, Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))
let yAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside,Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))

let config = Config.init(ToImageButtonOptions = ToImageButtonOptions.init(Format = StyleParam.ImageFormat.SVG, Filename = "praktikumsplot.svg"), EditableAnnotations = [AnnotationEditOptions.LegendPosition])

let source = __SOURCE_DIRECTORY__

let qConcatRawData =
    Frame.ReadCsv(path = (source + @"\..\AuxFiles\GroupsData\G1_1690WC1zu1_QuantifiedPeptides.txt"),separators="\t")
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
    Frame.ReadCsv(path = (source + @"\..\AuxFiles\WC_SampleDesc.txt"),separators="\t",schema="Strain=string")
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
    Frame.ReadCsv(source + @"\..\AuxFiles\PeptideProtMap.tsv",hasHeaders=true,separators="\t")
    |> Frame.indexRowsString "Peptide"

let peptideRatios = 
    ionRatios
    |> Frame.applyLevel (fun (sequence,globalMod,charge) -> sequence) Stats.mean
    |> Frame.join JoinKind.Inner peptideProtMapping 
    |> Frame.groupRowsByString "Protein"
    |> Frame.getNumericCols
    |> Frame.ofColumns

// This does not work if we give dilutions as float, because of 
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
        // get numerical cols would return dilution column, 
        // after changing it to a float in sampleDesc file
        |> Frame.filterCols (fun ck cs -> v.RowKeys |> Seq.contains ck)
        |> Frame.getCols
        |> Frame.ofColumns
        |> Frame.transpose
        )
    |> Frame.unnest 

let peptideRatiosWithDesc' : Frame<(string * string),(string * float)>=
    peptideRatios
    |> Frame.mapColKeys (fun rk ->
        sampleDesc.GetColumn("Strain").[rk],
        sampleDesc.GetColumn("Dilution").[rk]
    )

peptideRatiosWithDesc = peptideRatiosWithDesc' // val it : bool = true 

let proteinRatiosWithDesc =
    //peptideRatiosWithDesc
    peptideRatiosWithDesc'
    |> Frame.applyLevel fst Stats.mean


//================================================================================
//====================== 2. WholeCell ISD ========================================
//================================================================================
    

/////////////////////////////////// Chart Step 1 - RELATIVE QUANTIFICATION //////////////////////////////////////////

// create charts to compare rel quant per dilution in strains
let createChartForPeptideComparison (protString:string) (strainStrings:string []) =
    
    let protFrame =
        peptideRatiosWithDesc'.Nest()
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
        |> Chart.withX_Axis (xAxis false "Means-N14 Sample/N15 QProtein ratio" 20 16)
        |> Chart.withY_Axis (yAxis false "N14/N15 Quantification ratio" 20 16)

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
        |> Chart.withX_Axis (xAxis false (strain + "-N14 Sample/N15 QProtein ratio") 20 16)
        |> Chart.withY_Axis (yAxis false "N14/N15 Quantification ratio" 20 16)
    )
    |> Array.append [|xyMeanChart|]
    |> Chart.Stack(2,Space=0.2)
    |> Chart.withSize (1200.,900.)

let testing2 = createChartForPeptideComparison "rbcL" [|"4A";"1690";"1883"|]

testing2 |> Chart.Show


/////////////////////////////////// Chart Step 2 - PEARSON //////////////////////////////////////////

open FSharp.Stats.Fitting.LinearRegression.OrdinaryLeastSquares.Linear

//let prot1Coeff,prot1FitVals,prot1Determination =
let calculatePearson protName strainName (meanValueArray:(float*float) [])  =
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

//meanValuesFor "rbcL" "4A"
//|> calculatePearson "rbcL" "4A"

let chartRatios prot1 prot2 strain =
    let prot1Vals = meanValuesFor prot1 strain
    let prot2Vals = meanValuesFor prot2 strain

    let (prot1Coeff:Vector<float>),prot1FitVals,prot1Determination =
        calculatePearson prot1 strain prot1Vals

    let (prot2Coeff:Vector<float>),prot2FitVals,prot2Determination =
        calculatePearson prot2 strain prot2Vals

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
    |> Chart.withX_Axis (yAxis false "N14 Sample / N15 QProtein ratio" 20 16)
    |> Chart.withY_Axis (xAxis false "relative quantification" 20 16 )
    |> Chart.withConfig config
    |> Chart.withSize (900.,500.)
    |> Chart.Show

chartRatios "rbcL" "RBCS2" "4A"
