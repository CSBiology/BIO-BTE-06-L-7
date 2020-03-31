
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

    let rbsLRBCLValues =
        nestedFrame.["RBCL"].GetColumn<float>prot1
        |> getProtValuesFromSeries

    let RBCS2RBCSValues =
        nestedFrame.["RBCS"].GetColumn<float>prot2
        |> getProtValuesFromSeries

    [
        Chart.Scatter(rbsLRBCLValues,mode=StyleParam.Mode.Lines_Markers, MarkerSymbol = StyleParam.Symbol.Circle, Opacity=0.8)
        |> Chart.withTraceName (sprintf "Mean %s - %s" prot1 strain)
        Chart.Scatter(RBCS2RBCSValues,mode=StyleParam.Mode.Lines_Markers, MarkerSymbol = StyleParam.Symbol.Circle, Opacity=0.8)
        |> Chart.withTraceName (sprintf "Mean %s - %s" prot2 strain)
    ]
    |> Chart.Combine
    |> Chart.withX_Axis (xAxis false (sprintf "%s-Cut Out 1 %s/<br>Cut Out 2 %s" strain prot1 prot2) 20 16)
    |> Chart.withY_Axis (xAxis false "N14/N15 Quantification ratio" 20 16)

[|"4A"; "1883";"1690"|]
|> Array.map createChartForRbcsRbclComparison
|> Chart.Stack(3,Space=0.15)
|> Chart.withSize (1200.,400.)
|> Chart.Show



//let groupStrainInfoForQuant (protAmounts: int []) (strains: string []) content1 protein1 content2 protein2=
    
//    let strainNamesSorted = strains |> Array.sort

//    let protAmountAndStrains =
//        [|for amount in (protAmounts |> Array.sort) do 
//            yield! Array.zip strainNamesSorted (Array.init strains.Length (fun _ -> amount))|]

//    let prot1_RatiosSdsIgd = proteinMean_RatiosSdsIgd content1 protein1
    
//    let prot2_RatiosSdsIgd = proteinMean_RatiosSdsIgd content2 protein2

//    let prot1 =
//        prot1_RatiosSdsIgd
//        |> Frame.toArray2D
//        |> JaggedArray.ofArray2D
//        |> Array.concat

//    let prot2 =
//        prot2_RatiosSdsIgd
//        |> Frame.toArray2D
//        |> JaggedArray.ofArray2D
//        |> Array.concat

//    let strainInfo = 
//        Array.map3 
//            (fun (strain,protAmnt) prot1 prot2 ->
//                {|
//                    Strain     = strain
//                    ProtYg     = protAmnt
//                    Prot1Quant = prot1
//                    Prot2Quant = prot2
//                |}
//            ) protAmountAndStrains prot1 prot2

//    let groupedStrainInfo =
//        strainInfo
//        |> Array.groupBy (fun x -> x.Strain)
//        |> Array.map snd
//        |> Array.map (fun x ->
//            x
//            |> Array.sortBy (fun x -> x.ProtYg)
//        )
//    groupedStrainInfo

//let groupedStrainInfoForProt1and2Quant =
//    groupStrainInfoForQuant [|5;10;20|] [|"1690"; "1883"; "4A"|] "RBCL" protein1 "RBCS" protein2

//let relQuantProt1Prot2 =
//    groupedStrainInfoForProt1and2Quant
//    |> Array.map (fun strainInfo ->
//        strainInfo
//        |> Array.map (fun strain ->
//            strain.Prot1Quant
//        ),
//        strainInfo
//        |> Array.map (fun strain ->
//            strain.Prot2Quant
//        ),
//        strainInfo
//        |> Array.map (fun strain ->
//            float strain.ProtYg
//        ),
//        strainInfo.[0].Strain
//    )

//let coeffsQ = 
//    relQuantProt1Prot2
//    |> Array.map (fun (prot1Q,prot2Q,amount,strain) ->
//        Univariable.coefficient (vector amount) (vector prot1Q),
//        Univariable.coefficient (vector amount) (vector prot2Q)
//    )

//let fitFuncsQ =
//    coeffsQ
//    |> Array.map (fun (prot1C,prot2C) ->
//        Univariable.fit prot1C,
//        Univariable.fit prot2C
//    )

//let fitValsQ =
//    relQuantProt1Prot2
//    |> Array.map2 (fun (fitFuncP1,fitFuncP2) (prot1Q,prot2Q,amount,strain) ->
//        amount |> Array.map fitFuncP1,
//        amount |> Array.map fitFuncP2
//    )fitFuncsQ

//let determinationsQ = 
//    relQuantProt1Prot2
//    |> Array.map2 (fun (fitValP1,fitValP2) (prot1Q,prot2Q,amount,strain) ->
//        FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue prot1Q fitValP1,
//        FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue prot2Q fitValP2
//    ) fitValsQ

//let resultCollectionQ =
//    [|for i = 0 to groupedStrainInfoForProt1and2Quant.Length-1 do
//        yield
//            {|
//                ProteinAmountsYg = relQuantProt1Prot2.[i] |> (fun (_,_,amount,_) -> amount)
//                Protein1Quant    = relQuantProt1Prot2.[i] |> (fun (prot1Q,_,_,_) -> prot1Q)
//                Protein2Quant    = relQuantProt1Prot2.[i] |> (fun (_,prot2Q,_,_) -> prot2Q)
//                StrainName       = relQuantProt1Prot2.[i] |> (fun (_,_,_,strain) -> strain)
//                Coefficients     = coeffsQ.[i]
//                FitValues        = fitValsQ.[i]
//                Determination    = determinationsQ.[i]
//            |}
//    |]

//let n14n15RatioCharts =
//    resultCollectionQ
//    |> Array.map (fun result ->
//        [
//            Chart.Point (result.ProteinAmountsYg,result.Protein1Quant,MarkerSymbol = StyleParam.Symbol.Cross)
//            |> Chart.withTraceName (sprintf "relative quantification of %s for %s" protein1 result.StrainName)
//            Chart.Point (result.ProteinAmountsYg,result.Protein2Quant,MarkerSymbol = StyleParam.Symbol.Cross)
//            |> Chart.withTraceName (sprintf "relative quantification of %s for %s" protein2 result.StrainName)
//        ]
//        |> Chart.Combine
//    )
//    |> Chart.Combine

//let fitChartsQ =
//    resultCollectionQ
//    |> Array.map (fun result ->
//        [
//            Chart.Line(Array.zip result.ProteinAmountsYg (fst result.FitValues))
//            |> Chart.withTraceName (sprintf "linear regression: %.2f x + (%2f) ; R² = %.4f for strain %s and %s"
//                (fst result.Coefficients).[1] (fst result.Coefficients).[0] (fst result.Determination) result.StrainName protein1)
//            |> Chart.withLineStyle(Color="#D3D3D3",Dash=StyleParam.DrawingStyle.DashDot)
//            Chart.Line(Array.zip result.ProteinAmountsYg (snd result.FitValues))
//            |> Chart.withTraceName (sprintf "linear regression: %.2f x + (%2f) ; R² = %.4f for strain %s and %s"
//                (snd result.Coefficients).[1] (snd result.Coefficients).[0] (snd result.Determination) result.StrainName protein2)
//            |> Chart.withLineStyle(Color="#D3D3D3",Dash=StyleParam.DrawingStyle.DashDot)
//        ]
//        |> Chart.Combine
//    )
//    |> Chart.Combine

//[
//    n14n15RatioCharts
//    fitChartsQ
//]
//|> Chart.Combine
//|> Chart.withTitle (sprintf "SDS IGD: relative Quantifivation of %s and %s" protein1 protein2)
//|> Chart.withX_Axis (axisStyle false "absolute protein amount in sample [µg]" 20 16 )
//|> Chart.withY_Axis (axisStyle false (sprintf "14N/15N of %s and %s" protein1 protein2) 20 16 )
//|> Chart.withConfig config
//|> Chart.withSize (1200.,700.)
//|> Chart.Show
