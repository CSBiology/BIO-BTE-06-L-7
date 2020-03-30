
#load @"../IfSharp/References.fsx"
#load @"../IfSharp/Paket.Generated.Refs.fsx"
#load @"../AuxFsx/DeedleAux.fsx"

open BioFSharp
open Deedle
open FSharpAux
open FSharp.Stats
open FSharp.Plotly

let colorArray = [|"#E2001A"; "#FB6D26"; "#00519E"; "#00e257";|]

let colorForMean = "#366F8E"

let xAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Zeroline=false,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside, Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))
let yAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside,Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))

let config = Config.init(ToImageButtonOptions = ToImageButtonOptions.init(Format = StyleParam.ImageFormat.SVG, Filename = "praktikumsplot.svg"), EditableAnnotations = [AnnotationEditOptions.LegendPosition])


let isBad a =
    nan.Equals(a) || infinity.Equals(a) || (-infinity).Equals(a)

let source = __SOURCE_DIRECTORY__
//qConCatSeq.Length


let qConcatResult =
    Frame.ReadCsv(path = (source + @"\..\AuxFiles\GroupsData\G1_1690WC1zu1_QuantifiedPeptides.txt"),separators="\t")
    |> Frame.groupRowsBy "StringSequence"
    |> Frame.filterRows (fun (sequence,idx) _ -> sequence <> "EVTLGFVDLMR" && sequence <> "AFPDAYVR"  )
    |> Frame.indexRowsUsing (fun os -> 
            os.GetAs<string>("StringSequence"),
            (os.GetAs<bool>("GlobalMod"), os.GetAs<int>("Charge"))
    )


//FileName Experiment Content ProteinAmount[ug] Replicate
let sampleDesc :Frame<string,string>= 
    Frame.ReadCsv(path = (source + @"\..\AuxFiles\WC_SampleDesc.txt"),separators="\t",schema="Strain=string")
    |> Frame.indexRows "RawFileName"

let ionRatios = 
    sampleDesc
    |> Frame.mapRows (fun rawFileName _ -> 
        let n14 = qConcatResult.GetColumn<float>("N14Quant_" + rawFileName) |> Series.filterValues (fun x ->  x < 2000000. && x > 1. )
        let n15 = qConcatResult.GetColumn<float>("N15Quant_" + rawFileName) |> Series.filterValues (fun x ->  x < 2000000. && x > 1. )
        n14 / n15 
        )
    |> Frame.ofColumns

let peptideProtMapping : Frame<string,string>=
    Frame.ReadCsv(source + @"\..\AuxFiles\PeptideProtMap.tsv",hasHeaders=true,separators="\t")
    |> Frame.indexRows "Peptide"

let peptideRatios = 
    ionRatios
    |> Frame.applyLevel (fun (sequence,(globalMod,charge)) -> sequence) Stats.mean
    |> Frame.join JoinKind.Inner peptideProtMapping 
    |> Frame.groupRowsByString "Protein"
    |> Frame.getNumericCols
    |> Frame.ofColumns

let proteinRatios =
    peptideRatios
    |> Frame.applyLevel fst Stats.mean

peptideRatios.Print() 
proteinRatios.Print() 

sampleDesc.Print()

let s1690 = sampleDesc |> Frame.filterRows (fun k s -> s.GetAs<string>("Strain") = "1690")

proteinRatios.Transpose().Print()
sampleDesc.["Strain"]

let tmp :Series<string,string> = sampleDesc.GetColumn "Strain"
let tmp :Series<string,string> = sampleDesc.GetRow "G1 1690WC1zu1"


let x:Frame<string*string,string> = 
    proteinRatios
    |> Frame.transpose
    |> Frame.groupRowsUsing (fun k s -> 
        sampleDesc.GetColumn("Strain").[k]
        )

x.Print()

//================================================================================
//====================== 2. WholeCell ISD ========================================
//================================================================================
    


/////////////////////////////////// Chart Step 1 //////////////////////////////////////////

// create charts to compare rel quant per dilution in strains
let createChartForPeptideComparison =
    pepResults
    |> Array.groupBy (fun x -> x.StrainName, x.Protein)
    |> Array.map (fun (header,peptInfoArr) -> 
        header,
        peptInfoArr
        |> Array.mapi (fun i peptInfo ->
            Chart.Scatter(peptInfo.StrainValues,mode=StyleParam.Mode.Markers, MarkerSymbol = StyleParam.Symbol.Cross, Color=colorArray.[i])
            |> Chart.withTraceName (sprintf "%s -  %s - %s" peptInfo.StrainName peptInfo.Protein peptInfo.Peptide)
        )
        |> Chart.Combine
        |> Chart.withX_Axis (xAxis false (fst header + "-N14 Sample/N15 QProtein ratio") 20 16)
        |> Chart.withY_Axis (yAxis false "N14/N15 Quantification ratio" 20 16)
    )

// create charts to compare rel quant per dilution in strains as mean
let calculateMEANSPerStrain =
    pepResults
    |> Array.groupBy (fun x -> x.StrainName, x.Protein)
    |> Array.map (fun (header,peptInfoArr) -> 
        header,
        peptInfoArr
        |> Array.collect (fun peptide ->
            peptide.StrainValues
        )
    )
    |> Array.map (fun (header,values) -> header,values|> (Array.groupBy fst))
    |> Array.map (fun ((strain,prot),values) -> 
        let means =
            values 
            |> Seq.map (fun (xAxis,values) -> 
                xAxis,values 
                |> Seq.meanBy snd
            )
        createPepResult1 prot "mean" (Seq.toArray means) strain
    )

let createChartsForMEANSPerStrain =
    calculateMEANSPerStrain
    |> Array.map (fun meanPepResults ->
        (meanPepResults.StrainName,meanPepResults.Protein),
        Chart.Scatter(meanPepResults.StrainValues,mode=StyleParam.Mode.Lines_Markers, MarkerSymbol = StyleParam.Symbol.Circle, Color=colorForMean, Opacity=0.8)
        |> Chart.withTraceName (sprintf "mean %s - %s" meanPepResults.StrainName meanPepResults.Protein)
    )

// create charts to compare rel quant per dilution between strains
let createChartsForMEANSComparison =
    calculateMEANSPerStrain
    |> Array.groupBy (fun pepRes -> pepRes.Protein)
    |> Array.map (fun (prot,strains) -> 
        ("CompareStrains", prot),
        strains
        |> Array.mapi (fun i pepRes -> 
            Chart.Scatter(pepRes.StrainValues,mode=StyleParam.Mode.Lines_Markers, MarkerSymbol = StyleParam.Symbol.Cross, Color=colorArray.[i])
            |> Chart.withTraceName (sprintf "mean %s -  %s" pepRes.StrainName pepRes.Protein)
        )
        |> Chart.Combine
        |> Chart.withX_Axis (xAxis false ("Strain Means-" + "N14 Sample/N15 QProtein ratio") 20 16)
        |> Chart.withY_Axis (yAxis false "N14/N15 Quantification ratio" 20 16)
    )
snd createChartsForMEANSComparison.[0] |> Chart.Show

let alignPeptideAndPeptideMeans =
    Array.zip createChartForPeptideComparison createChartsForMEANSPerStrain
    |> Array.map (fun ((header,chart),(header,chartMean)) -> header, Chart.Combine [|chart; chartMean|])

Array.append createChartsForMEANSComparison alignPeptideAndPeptideMeans
|> Array.groupBy (fun (header,chart) -> snd header) 
|> fun x -> x
|> Array.map (fun (prot,chartsWithMeta) -> 
    let chart =
        chartsWithMeta
        |> Array.map (fun x -> 
            snd x 
        )
        |> Chart.Stack(2,Space=0.15)
        |> Chart.withTitle prot
        |> Chart.withSize (1200.,900.)
        |> Chart.withConfig config
        //|> Chart.Show
    if prot = "rbcL" || prot = "RBCS2" then Chart.Show chart else ()
)


/////////////////////////////////// Chart Step 2 //////////////////////////////////////////

open FSharp.Stats.Fitting.LinearRegression.OrdinaryLeastSquares.Linear

let protein1 = "rbcL"
let protein2 = "RBCS2"


let prot1 =
    calculateMEANSPerStrain
    |> Array.filter (fun x -> x.Protein = protein1)

let prot1Coeff,prot1FitVals,prot1Determination =
    prot1
    |> Array.map (
        fun pepInfos ->
            let dilutionsSorted,strainVals =
                pepInfos.StrainValues 
                |> Array.unzip
            // RBCL Regression of relative quantification values
            let RBCLcoeff = Univariable.coefficient (vector dilutionsSorted) (vector strainVals)
            let RBCLfitFunc = Univariable.fit RBCLcoeff
            let RBCLfitVals = dilutionsSorted |> Array.map RBCLfitFunc
            let RBCLdetermination = FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue strainVals RBCLfitVals
            let RBCLpearson = FSharp.Stats.Correlation.Seq.pearson strainVals dilutionsSorted
            printfn "%s - Pearson WholeCell RBCL: %f" pepInfos.StrainName RBCLpearson
            RBCLcoeff, RBCLfitVals, RBCLdetermination
    )
    |> Array.unzip3
                
let prot2 =
    calculateMEANSPerStrain
    |> Array.filter (fun x -> x.Protein = protein2)

let prot2Coeff,prot2FitVals,prot2Determination =
    prot2
    |> Array.map (
        fun pepInfos ->
            let dilutionsSorted,strainVals =
                pepInfos.StrainValues 
                |> Array.unzip
            let RBCScoeff = Univariable.coefficient (vector dilutionsSorted ) (vector strainVals) 
            let RBCSfitFunc = Univariable.fit RBCScoeff
            let RBCSfitVals = dilutionsSorted  |> Array.map RBCSfitFunc
            let RBCSdetermination = FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue strainVals RBCSfitVals
            let RBCSpearson = FSharp.Stats.Correlation.Seq.pearson (strainVals) dilutionsSorted
            printfn "%s - Pearson WholeCell RBCS: %f" pepInfos.StrainName RBCSpearson
            RBCScoeff, RBCSfitVals, RBCSdetermination
    )
    |> Array.unzip3

let chartRatios (prot1:PepResult1) (prot1Coeff:Vector<float>) prot1FitVals prot1Determination (prot2:PepResult1) (prot2Coeff:Vector<float>) prot2FitVals prot2Ddetermination =
    let dilutionsSorted,strainVals =
        prot1.StrainValues
        |> Array.unzip
    let strain = prot1.StrainName
    [
        Chart.Point (prot1.StrainValues,Name = sprintf "%s Quantified Ratios" protein1)
        |> Chart.withMarkerStyle(Size=10,Symbol = StyleParam.Symbol.Cross)
        Chart.Line(Array.zip dilutionsSorted prot1FitVals,Name = (sprintf "%s linear regression: %.2f x + (%2f) ; R = %.4f" protein1 prot1Coeff.[1] prot1Coeff.[0] prot1Determination))
        |> Chart.withLineStyle(Color="lightblue",Dash=StyleParam.DrawingStyle.DashDot)

        Chart.Point (prot2.StrainValues,Name = sprintf "%s Quantified Ratios" protein2,MarkerSymbol = StyleParam.Symbol.Cross)
        |> Chart.withMarkerStyle(Size=10,Symbol = StyleParam.Symbol.Cross)
        Chart.Line(Array.zip dilutionsSorted prot2FitVals,Name = (sprintf "%s linear regression: %.2f x + (%2f) ; R = %.4f" protein2 prot2Coeff.[1] prot2Coeff.[0] prot2Ddetermination))
        |> Chart.withLineStyle(Color="LightGreen",Dash=StyleParam.DrawingStyle.DashDot)
    ]
    |> Chart.Combine
    |> Chart.withTitle (sprintf "%s - Whole cell extracts: Stability of %s/%s ratios between samples" strain protein1 protein2)
    |> Chart.withX_Axis (yAxis false "N14 Sample / N15 QProtein ratio" 20 16)
    |> Chart.withY_Axis (xAxis false "relative quantification" 20 16 )
    |> Chart.withConfig config
    |> Chart.withSize (900.,500.)
    |> Chart.Show


prot1
|> Array.mapi (fun i x ->
    chartRatios 
        x prot1Coeff.[i] prot1FitVals.[i] prot1Determination.[i]
        prot2.[i] prot2Coeff.[i] prot2FitVals.[i] prot2Determination.[i]
)
