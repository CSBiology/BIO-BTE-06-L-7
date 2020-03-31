
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
    Frame.ReadCsv(source + @"\..\AuxFiles\PeptideProtMap.tsv",hasHeaders=true,separators="\t")
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

let calcultateRelativeQuantForCutOuts frame =
    let nestedFrame: Series<string,Frame<'a,'b>> =
        frame
        |> Frame.transpose
        |> Frame.nest
    nestedFrame.["RBCL"] // or "RBCL" we only want col keys and ignore actucal series.
    |> Frame.mapCols (fun ck _ ->
        let rbcsSeries =
            nestedFrame.["RBCS"].GetColumn<float>ck
        let rbclSeries =
            nestedFrame.["RBCL"].GetColumn<float>ck
        rbclSeries / rbcsSeries
    )
    |> Frame.transpose

let peptideRatiosWithDesc15N15N = 
    peptideRatiosWithDesc
    |> calcultateRelativeQuantForCutOuts

let proteinRatiosWithDesc14N15N =
    proteinRatiosWithDesc
    |> calcultateRelativeQuantForCutOuts
    
open FSharp.Stats.Fitting.LinearRegression.OrdinaryLeastSquares.Linear

let meanValuesFor protName strainName=
    let meanSeries : Series<(string * float),float> = proteinRatiosWithDesc14N15N.GetRow protName
    meanSeries
    |> Series.filter (fun k t -> fst k = strainName)
    |> fun x -> x.Observations
    |> Seq.map (fun x -> snd x.Key, x.Value)
    |> Array.ofSeq

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

meanValuesFor "rbcL" "4A"
|> calculatePearson "rbcL" "4A"

//let coeffsVS =
//    cotrolledAmountRatioStrain
//    |> Array.map (fun (amount,ratio,strain) ->
//        Univariable.coefficient (vector amount) (vector ratio)
//    )

//let fitFuncsVS = 
//    coeffsVS
//    |> Array.map (fun (coeff) ->
//        Univariable.fit coeff
//    )

//let fitValsVS = 
//    cotrolledAmountRatioStrain
//    |> Array.map2 (fun fitFunc (amount,ratio,strain) ->
//        amount |> Array.map fitFunc
//    ) fitFuncsVS

//let determinationsVS = 
//    cotrolledAmountRatioStrain
//    |> Array.map2 (fun fitVal (amount,ratio,strain) ->
//        FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue ratio fitVal
//    ) fitValsVS

//let pearsonsVS = 
//    cotrolledAmountRatioStrain
//    |> Array.map (fun (amount,ratio,strain) ->
//        FSharp.Stats.Correlation.Seq.pearson ratio amount
//    )

//let resultCollectionVS =
//    [|for i = 0 to groupedStrainInfoForProt1vsProt2.Length-1 do
//        yield
//            {|
//                ProteinAmountsYg = cotrolledAmountRatioStrain.[i] |> fun (amount,_,_) -> amount
//                ProteinRatios    = cotrolledAmountRatioStrain.[i] |> fun (_,ratios,_) -> ratios
//                StrainName       = cotrolledAmountRatioStrain.[i] |> fun (_,_,strain) -> strain
//                Coefficients     = coeffsVS.[i]
//                FitValues        = fitValsVS.[i]
//                Determination    = determinationsVS.[i]
//                Pearson          = pearsonsVS.[i]
//            |}
//    |]

//resultCollectionVS
//|> Array.iter (fun (result) ->
//    printfn "Strain %s Pearson IGD: %f" result.StrainName result.Pearson
//)

//let ratioQuantChartsVS =
//    resultCollectionVS
//    |> Array.map (fun result ->
//        Chart.Point (result.ProteinAmountsYg,result.ProteinRatios,MarkerSymbol = StyleParam.Symbol.Cross)
//        |> Chart.withTraceName (sprintf "Quantified Ratios (%s/%s) for strain %s" protein1 protein2 result.StrainName)
//    )
//    |> Chart.Combine

//let fitChartsVS =
//    resultCollectionVS
//    |> Array.map (fun result ->
//        Chart.Line(Array.zip result.ProteinAmountsYg result.FitValues)
//        |> Chart.withTraceName (sprintf "linear regression: %.2f x + (%2f) ; R² = %.4f for strain %s" result.Coefficients.[1] result.Coefficients.[0] result.Determination result.StrainName)
//        |> Chart.withLineStyle(Color="#D3D3D3",Dash=StyleParam.DrawingStyle.DashDot)
//    )
//    |> Chart.Combine

//[
//    ratioQuantChartsVS
//    fitChartsVS
//]
//|> Chart.Combine
//|> Chart.withTitle (sprintf "SDS IGD: Stability of %s/%s ratios between strains" protein1 protein2)
//|> Chart.withX_Axis (axisStyle false "absolute protein amount in sample [µg]" 20 16 )
//|> Chart.withY_Axis (axisStyle false (sprintf "relative quantification %s/%s" protein1 protein2) 20 16 )
//|> Chart.withConfig config
//|> Chart.withSize (1200.,700.)
//|> Chart.Show


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
    |> Chart.withX_Axis (xAxis false "N14 Sample / N15 QProtein ratio" 20 16)
    |> Chart.withY_Axis (xAxis false "relative quantification" 20 16 )
    |> Chart.withConfig config
    |> Chart.withSize (900.,500.)
    |> Chart.Show

chartRatios "rbcL" "RBCS2" "4A"


let groupStrainInfoForQuant (protAmounts: int []) (strains: string []) content1 protein1 content2 protein2=
    
    let strainNamesSorted = strains |> Array.sort

    let protAmountAndStrains =
        [|for amount in (protAmounts |> Array.sort) do 
            yield! Array.zip strainNamesSorted (Array.init strains.Length (fun _ -> amount))|]

    let prot1_RatiosSdsIgd = proteinMean_RatiosSdsIgd content1 protein1
    
    let prot2_RatiosSdsIgd = proteinMean_RatiosSdsIgd content2 protein2

    let prot1 =
        prot1_RatiosSdsIgd
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D
        |> Array.concat

    let prot2 =
        prot2_RatiosSdsIgd
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D
        |> Array.concat

    let strainInfo = 
        Array.map3 
            (fun (strain,protAmnt) prot1 prot2 ->
                {|
                    Strain     = strain
                    ProtYg     = protAmnt
                    Prot1Quant = prot1
                    Prot2Quant = prot2
                |}
            ) protAmountAndStrains prot1 prot2

    let groupedStrainInfo =
        strainInfo
        |> Array.groupBy (fun x -> x.Strain)
        |> Array.map snd
        |> Array.map (fun x ->
            x
            |> Array.sortBy (fun x -> x.ProtYg)
        )
    groupedStrainInfo

let groupedStrainInfoForProt1and2Quant =
    groupStrainInfoForQuant [|5;10;20|] [|"1690"; "1883"; "4A"|] "RBCL" protein1 "RBCS" protein2

let relQuantProt1Prot2 =
    groupedStrainInfoForProt1and2Quant
    |> Array.map (fun strainInfo ->
        strainInfo
        |> Array.map (fun strain ->
            strain.Prot1Quant
        ),
        strainInfo
        |> Array.map (fun strain ->
            strain.Prot2Quant
        ),
        strainInfo
        |> Array.map (fun strain ->
            float strain.ProtYg
        ),
        strainInfo.[0].Strain
    )

let coeffsQ = 
    relQuantProt1Prot2
    |> Array.map (fun (prot1Q,prot2Q,amount,strain) ->
        Univariable.coefficient (vector amount) (vector prot1Q),
        Univariable.coefficient (vector amount) (vector prot2Q)
    )

let fitFuncsQ =
    coeffsQ
    |> Array.map (fun (prot1C,prot2C) ->
        Univariable.fit prot1C,
        Univariable.fit prot2C
    )

let fitValsQ =
    relQuantProt1Prot2
    |> Array.map2 (fun (fitFuncP1,fitFuncP2) (prot1Q,prot2Q,amount,strain) ->
        amount |> Array.map fitFuncP1,
        amount |> Array.map fitFuncP2
    )fitFuncsQ

let determinationsQ = 
    relQuantProt1Prot2
    |> Array.map2 (fun (fitValP1,fitValP2) (prot1Q,prot2Q,amount,strain) ->
        FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue prot1Q fitValP1,
        FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue prot2Q fitValP2
    ) fitValsQ

let resultCollectionQ =
    [|for i = 0 to groupedStrainInfoForProt1and2Quant.Length-1 do
        yield
            {|
                ProteinAmountsYg = relQuantProt1Prot2.[i] |> (fun (_,_,amount,_) -> amount)
                Protein1Quant    = relQuantProt1Prot2.[i] |> (fun (prot1Q,_,_,_) -> prot1Q)
                Protein2Quant    = relQuantProt1Prot2.[i] |> (fun (_,prot2Q,_,_) -> prot2Q)
                StrainName       = relQuantProt1Prot2.[i] |> (fun (_,_,_,strain) -> strain)
                Coefficients     = coeffsQ.[i]
                FitValues        = fitValsQ.[i]
                Determination    = determinationsQ.[i]
            |}
    |]

let n14n15RatioCharts =
    resultCollectionQ
    |> Array.map (fun result ->
        [
            Chart.Point (result.ProteinAmountsYg,result.Protein1Quant,MarkerSymbol = StyleParam.Symbol.Cross)
            |> Chart.withTraceName (sprintf "relative quantification of %s for %s" protein1 result.StrainName)
            Chart.Point (result.ProteinAmountsYg,result.Protein2Quant,MarkerSymbol = StyleParam.Symbol.Cross)
            |> Chart.withTraceName (sprintf "relative quantification of %s for %s" protein2 result.StrainName)
        ]
        |> Chart.Combine
    )
    |> Chart.Combine

let fitChartsQ =
    resultCollectionQ
    |> Array.map (fun result ->
        [
            Chart.Line(Array.zip result.ProteinAmountsYg (fst result.FitValues))
            |> Chart.withTraceName (sprintf "linear regression: %.2f x + (%2f) ; R² = %.4f for strain %s and %s"
                (fst result.Coefficients).[1] (fst result.Coefficients).[0] (fst result.Determination) result.StrainName protein1)
            |> Chart.withLineStyle(Color="#D3D3D3",Dash=StyleParam.DrawingStyle.DashDot)
            Chart.Line(Array.zip result.ProteinAmountsYg (snd result.FitValues))
            |> Chart.withTraceName (sprintf "linear regression: %.2f x + (%2f) ; R² = %.4f for strain %s and %s"
                (snd result.Coefficients).[1] (snd result.Coefficients).[0] (snd result.Determination) result.StrainName protein2)
            |> Chart.withLineStyle(Color="#D3D3D3",Dash=StyleParam.DrawingStyle.DashDot)
        ]
        |> Chart.Combine
    )
    |> Chart.Combine

[
    n14n15RatioCharts
    fitChartsQ
]
|> Chart.Combine
|> Chart.withTitle (sprintf "SDS IGD: relative Quantifivation of %s and %s" protein1 protein2)
|> Chart.withX_Axis (axisStyle false "absolute protein amount in sample [µg]" 20 16 )
|> Chart.withY_Axis (axisStyle false (sprintf "14N/15N of %s and %s" protein1 protein2) 20 16 )
|> Chart.withConfig config
|> Chart.withSize (1200.,700.)
|> Chart.Show
