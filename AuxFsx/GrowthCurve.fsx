open System

// Include CsbScaffold
#load @"..\IfSharp\Paket.Generated.Refs.fsx"
// If you want to use the wrappers for unmanaged LAPACK functions from of FSharp.Stats 
// include the path to the .lib folder manually to your PATH environment variable and make sure you set FSI to 64 bit

// use the following lines of code to ensure that LAPACK functionalities are enabled if you want to use them
// fails with "MKL service either not available, or not started" if lib folder is not included in PATH.
//open FSharp.Stats
//FSharp.Stats.Algebra.LinearAlgebra.Service()
 
open BioFSharp

open FSharp.Plotly

open FSharp.Stats
open FSharp.Stats.Fitting
open FSharp.Stats.Fitting.LinearRegression
open FSharp.Plotly

type Color = {
    H : int
    S : int
    L : int
    }

let createHSL (color:Color) =
    (sprintf "hsl(%i," color.H) + string color.S + @"%," + string color.L + @"%)"

let mainColorPeachFF6F61 = {
    H = 5
    S = 100
    L = 69
}

let mainColorBlue92a8d1 = {
    H = 219
    S = 41
    L = 70
}

let mainColorGreen88b04b = {
    H = 84
    S = 40
    L = 49
}

let mainColorYellowEFC050 = {
    H = 42
    S = 83
    L = 63
}

let createRelatedColors (color:Color) nOfColors =
    let wheelchange = -20
    let changeTempColor wheelChange tempColor =
        if tempColor.H + wheelChange < 0 
        then 
            360 + (color.H + wheelChange)
        elif tempColor.H + wheelChange > 360
        then 
            (tempColor.H + wheelChange) - 360
        else tempColor.H + wheelChange
    let rec loopVariant results iteration =
        if iteration = 1 
        then 
            loopVariant ({color with H = changeTempColor wheelchange color}::results) (iteration + 1)
        elif iteration < nOfColors 
        then 
            let mostRecentVariant =
                results.Head
            loopVariant ({mostRecentVariant with H = changeTempColor wheelchange mostRecentVariant}::results) (iteration + 1)
        else results
    loopVariant [color] 1
    |> (List.rev >> Array.ofList)

//BASE COLORS: #FF6F61 and #577284

let timeHGr3 = [|0.; 19.5; 25.5; 43.; 48.5; 51.25; 67.75|]

let timeHGr4 = [|0.; 19.5; 25.; 42.25; 48.; 49.; 66.25 |]

// corrected by blank substraction
let oneTo1Gr3 = [| 1659000.; 4169000.; 6585400.; 16608400.; 17257800.; nan; 18041000.|]

let oneTo25Gr3 = [|
    (Array.head >> fun x -> x/25.) oneTo1Gr3 
    165200.
    356600.
    1081400.
    2037800.
    2632600.
    nan
|]
let oneTo50Gr3 = [|
    (Array.head >> fun x -> x/50.) oneTo1Gr3 
    67400.
    182600.
    850200.
    1156800.
    nan
    4053000.
|]

let oneTo1Gr4 = [|
    946600.
    3104000.
    6535400.
    15018400.
    15977800.
    nan
    17636400.
|]
let oneTo25Gr4 = [|
    (Array.head >> fun x -> x/25.) oneTo1Gr4
    104400.
    220200.
    563200.
    nan //970600. <- this number is assumed to be a mistaken measurement of 1:50 around that time point
    1645800.
    2788400.
|]
let oneTo50Gr4 = [|
    (Array.head >> fun x -> x/50.) oneTo1Gr4
    80600.
    108800.
    352000.
    nan
    752400.
    3193400.
|]

// style axis
let xAxis title = Axis.LinearAxis.init(Title=title,Showgrid=false,Showline=true,Mirror=StyleParam.Mirror.All,Zeroline=false,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside, Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=26.),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=20.))       
let yAxis title = Axis.LinearAxis.init(Title=title,Showgrid=false,Showline=true,Mirror=StyleParam.Mirror.All,Zeroline=false,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside,Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=26.),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=20.)(*,AxisType = StyleParam.AxisType.Log*))
let chartConfig =
    Config.init(StaticPlot = false, Responsive = true, Editable = true,Autosizable=true,ShowEditInChartStudio=true,ToImageButtonOptions = ToImageButtonOptions.init(Format = StyleParam.ImageFormat.SVG))


let gr3_1To1 =
    Array.zip timeHGr3 oneTo1Gr3
    |> Array.filter (fun (x,y) -> isNan y = false && isNan x = false )
    |> fun x -> Chart.Line(x,Name = "1:1 - Gr3")
    |> Chart.withMarkerStyle(Color = "#374955")

let gr3_1To25 =
    Array.zip timeHGr3 oneTo25Gr3
    |> Array.filter (fun (x,y) -> isNan y = false && isNan x = false )
    |> fun x -> Chart.Line(x,Name = "1:25 - Gr3")
    |> Chart.withMarkerStyle(Color = "#577284")

let gr3_1To50 =
    Array.zip timeHGr3 oneTo50Gr3
    |> Array.filter (fun (x,y) -> isNan y = false && isNan x = false )
    |> fun x -> Chart.Line(x,Name = "1:50 - Gr3")
    |> Chart.withMarkerStyle(Color = "#9BAEBC")

let gr4_1To1 =
    Array.zip timeHGr4 oneTo1Gr4
    |> Array.filter (fun (x,y) -> isNan y = false && isNan x = false )
    |> fun x -> Chart.Line(x,Name = "1:1 - Gr4")
    |> Chart.withMarkerStyle(Color = "#FF6F61")

let gr4_1To25 =
    Array.zip timeHGr4 oneTo25Gr4
    |> Array.filter (fun (x,y) -> isNan y = false && isNan x = false )
    |> fun x -> Chart.Line(x,Name = "1:25 - Gr4")
    |> Chart.withMarkerStyle(Color = "#F15B7C")

let gr4_1To50 =
    Array.zip timeHGr4 oneTo50Gr4
    |> Array.filter (fun (x,y) -> isNan y = false && isNan x = false )
    |> fun x -> Chart.Line(x,Name = "1:50 - Gr4")
    |> Chart.withMarkerStyle(Color = "#D65394")

[
    gr3_1To1
    gr3_1To25
    gr3_1To50
    gr4_1To1
    gr4_1To25
    gr4_1To50
]
|> Chart.Combine
|> Chart.withTitle "Growth curve of <i>Clamydomonas reinhardtii</i> cell cultures"
|> Chart.withY_Axis (yAxis "Number of cells")
|> Chart.withX_Axis (xAxis "Time [hours]")
//|> Chart.Show

open FSharp.Stats.Fitting.NonLinearRegression

let LogisticFunction = {
    ParameterNames= [|"L - curve maximum";"k - Steepness"; "x0 xValue of midpoint"; "N - curve minimum"|]
    GetFunctionValue = (fun (parameterVector:Vector<float>) xValue -> 
                        //printfn "Equation: (%f / (1. + exp(-%f * (%f - %f)))) + %f" parameterVector.[0] parameterVector.[1] xValue parameterVector.[2] parameterVector.[3]
                        parameterVector.[0] / (1. + exp(-parameterVector.[1]*(xValue-parameterVector.[2]))) + parameterVector.[3])
    GetGradientValue = (fun (parameterVector:Vector<float>) (gradientVector: Vector<float>) xValue ->
                        gradientVector.[0] <- 1. / (1. + exp(parameterVector.[1]*(xValue-parameterVector.[2])))
                        gradientVector.[1] <- (parameterVector.[0] * (xValue-parameterVector.[2]) * exp(parameterVector.[1]*(xValue-parameterVector.[2])) ) / (exp(parameterVector.[1]*(xValue-parameterVector.[2])) + 1.)**2.
                        gradientVector.[2] <- (parameterVector.[0] * parameterVector.[1] * exp(parameterVector.[1]*(xValue-parameterVector.[2])) ) / (exp(parameterVector.[1]*(xValue-parameterVector.[2])) + 1.)**2.
                        gradientVector.[3] <- 1.
                        gradientVector)
    }

let expModel =
    let parameterNames = [|"a";"b"|]
    let getFunctionValues = (fun (parameters:Vector<float>) x ->
        parameters.[0] * Math.Exp(parameters.[1] * x))
    let getGradientValues =
        (fun (parameterVector:Vector<float>) (gradientVector: Vector<float>) xValue ->
            gradientVector.[0] <- Math.Exp(parameterVector.[1] * xValue)
            gradientVector.[1] <- parameterVector.[0] * xValue * Math.Exp(parameterVector.[1] * xValue)
            gradientVector)
    createModel parameterNames getFunctionValues getGradientValues

let estimatedParamsWithRSS (model: Model) (solverOptions: SolverOptions) lambdaInitial lambdaFactor (lowerBound: vector) (upperBound: vector) (xData: float[]) (yData: float []) =
    let estParams = LevenbergMarquardtConstrained.estimatedParamsVerbose model solverOptions lambdaInitial lambdaFactor lowerBound upperBound xData yData
    estParams
    |> fun estParams ->
        let paramGuess = Vector.ofArray solverOptions.InitialParamGuess
        let rss = getRSS model xData yData paramGuess
        estParams.[estParams.Count-1], rss

///Takes the result of the linearization as initialGuessParams
let expSolverOptions (x_data:float []) (y_data:float [])=
    //gets the linear representation of the problem and solves it by simple linear regression
    let initialParamGuess =
        let y_ln = y_data |> Array.map (fun x -> Math.Log(x)) |> vector
        let linearReg = LinearRegression.OrdinaryLeastSquares.Linear.Univariable.coefficient (vector x_data) y_ln
        let a = exp linearReg.[0]
        let b = linearReg.[1]
        [|a;b|]
    {
    MinimumDeltaValue       = 0.0001
    MinimumDeltaParameters  = 0.0001
    MaximumIterations       = 10000
    InitialParamGuess       = initialParamGuess
    }

let exponentialGrowth timePoints intensityPoints =
    let initialGuess =
        expSolverOptions timePoints intensityPoints
    let lowerBound =
        initialGuess.InitialParamGuess
        |> Array.map (fun param -> param - (abs param) * 0.2)
        |> vector
    let upperBound =
        initialGuess.InitialParamGuess
        |> Array.map (fun param -> param + (abs param) * 0.2)
        |> vector
    let estimate =
        estimatedParamsWithRSS
            expModel
            initialGuess
            0.001 
            10.
            lowerBound
            upperBound
            timePoints
            intensityPoints
        |> fun x ->
            printfn "Chosen Estimate exponential fit: %A" x
            fst x
    expModel.GetFunctionValue estimate


let logisticFunction timePoints intensityPoints steepnessRange =
    let lineSolverOptions initialParamGuess = {
        MinimumDeltaValue       = 0.00001
        MinimumDeltaParameters  = 0.00001
        MaximumIterations       = 10000
        InitialParamGuess       = initialParamGuess
        }
    let initialGuess = 
        steepnessRange
        |> Array.map (fun x -> lineSolverOptions [|intensityPoints |> Array.max; x ; (timePoints |> Array.max) / 2.; intensityPoints |> Array.min |])
    let estimate =
        initialGuess
        |> Array.map (
            fun initial ->
                let lowerBound =
                    initial.InitialParamGuess
                    |> Array.map (fun param -> param - (abs param) * 0.2)
                    |> vector
                let upperBound =
                    initial.InitialParamGuess
                    |> Array.map (fun param -> param + (abs param) * 0.2)
                    |> vector
                estimatedParamsWithRSS 
                    LogisticFunction 
                    initial 
                    0.001 
                    10.
                    lowerBound
                    upperBound
                    timePoints
                    intensityPoints
        )
        |> Array.minBy snd
        |> fun x ->
            printfn "Chosen Estimate logistic fit: %A" x
            fst x
    LogisticFunction.GetFunctionValue estimate

    /// used to predict growth curve with not enough data points. Growth curve still in log phase.
let logisticFunctionTheoretical timePoints intensityPoints steepnessRange maxIntensity curveMidTimePoint minIntensity =
    let lineSolverOptions initialParamGuess = {
        MinimumDeltaValue       = 0.00001
        MinimumDeltaParameters  = 0.00001
        MaximumIterations       = 10000
        InitialParamGuess       = initialParamGuess
        }
    let initialGuess = 
        steepnessRange
        |> Array.map (fun x -> lineSolverOptions [|maxIntensity; x ; curveMidTimePoint; minIntensity |])
    let estimate =
        initialGuess
        |> Array.map (
            fun initial ->
                let lowerBound =
                    initial.InitialParamGuess
                    |> Array.map (fun param -> param - (abs param) * 0.2)
                    |> vector
                let upperBound =
                    initial.InitialParamGuess
                    |> Array.map (fun param -> param + (abs param) * 0.2)
                    |> vector
                estimatedParamsWithRSS 
                    LogisticFunction 
                    initial 
                    0.001 
                    10.
                    lowerBound
                    upperBound
                    timePoints
                    intensityPoints
        )
        |> Array.minBy snd
        |> fun x ->
            printfn "Chosen Estimate theoretical logistic fit: %A" x
            fst x
    LogisticFunction.GetFunctionValue estimate

let timePointsGr3,intensityPoints1To1Gr3 =
    Array.zip timeHGr3 oneTo1Gr3
    |> Array.filter (fun (x,y) -> isNan y = false && isNan x = false )
    |> Array.unzip
let _,intensityPoints1To25Gr3 =
    Array.zip timeHGr3 oneTo25Gr3
    |> Array.filter (fun (x,y) -> isNan y = false && isNan x = false )
    |> Array.unzip
let _,intensityPoints1To50Gr3 =
    Array.zip timeHGr3 oneTo50Gr3
    |> Array.filter (fun (x,y) -> isNan y = false && isNan x = false )
    |> Array.unzip


let timePointsGr4,intensityPoints1To1Gr4 =
    Array.zip timeHGr4 oneTo1Gr4
    |> Array.filter (fun (x,y) -> isNan y = false && isNan x = false )
    |> Array.unzip
let _,intensityPoints1To25Gr4 =
    Array.zip timeHGr4 oneTo25Gr4
    |> Array.filter (fun (x,y) -> isNan y = false && isNan x = false )
    |> Array.unzip
let _,intensityPoints1To50Gr4 =
    Array.zip timeHGr4 oneTo50Gr4
    |> Array.filter (fun (x,y) -> isNan y = false && isNan x = false )
    |> Array.unzip

//let timePointsGr4,intensityPoints1To1Gr4 =
//    Array.zip timeHGr4 oneTo1Gr4
//    |> Array.filter (fun (x,y) -> isNan y = false && isNan x = false )
//    |> Array.unzip

let steepnessRange =
    [|0.01 .. 0.01 .. 1.|]//[|0.05 .. 0.01 .. 2.|]

//let showExpFunctionFit timepoints intensitypoints =
//    let tpArr =
//        [|0. .. 70.|]
//    let fit =
//        tpArr
//        |> Array.map (exponentialModel timepoints intensitypoints)
//        |> fun x -> Chart.Line(tpArr,x)
//    let originalData =
//        Chart.Point(timepoints,intensitypoints)
//    [
//        fit; originalData
//    ] |> Chart.Combine (*|> Chart.Show*)

let showLogisticFunctionFit timepoints intensitypoints steepnessRange name color =
    let colorArr =
        createRelatedColors color 2
    let tpArr =
        [|0. .. 70.|]
    let fitLog =
        tpArr
        |> Array.map (logisticFunction timepoints intensitypoints steepnessRange)
        |> fun x -> Chart.Line(tpArr,x, Name = "Fit with logistic function - " + name)
        |> Chart.withMarkerStyle(Color = createHSL colorArr.[1])
    let originalData =
        Chart.Point(timepoints,intensitypoints,Name = "Meassured data points - " + name)
        |> Chart.withMarkerStyle(Color = createHSL colorArr.[0] )
    [
        fitLog;originalData
    ] |> Chart.Combine (*|> Chart.Show*)

let showTheoreticalLogisticFunctionFit timepoints intensitypoints steepnessRange maxIntensinty curveMidTimePoint minIntensity name color =
    let colorArr =
        createRelatedColors color 3
    let maxTimePoint =
        timepoints |> Array.max
    let tpArr max=
        [|0. .. max|]
    let fitExp =
        tpArr maxTimePoint
        |> Array.map (exponentialGrowth timepoints intensitypoints)
        |> fun x -> Chart.Line((tpArr maxTimePoint),x, Name = "Exponential fit - " + name)
        |> Chart.withMarkerStyle(Color = createHSL colorArr.[1])
    let fitTheoLog =
        tpArr (maxTimePoint + 50.)
        |> Array.map (logisticFunctionTheoretical timepoints intensitypoints steepnessRange maxIntensinty curveMidTimePoint minIntensity)
        |> fun x -> Chart.Line((tpArr (maxTimePoint + 50.)),x, Name = "Theoretical fit with Logistic Function - " + name)
        |> Chart.withMarkerStyle(Color = createHSL colorArr.[2])
    let originalData =
        Chart.Point(timepoints,intensitypoints, Name = "Meassured data points - " + name)
        |> Chart.withMarkerStyle(Color = createHSL colorArr.[0])
    [
        fitExp;fitTheoLog;originalData
    ] |> Chart.Combine (*|> Chart.Show*)

let calculateDoublingTime fittingFunction rootsXX =

    //https://en.wikipedia.org/wiki/Doubling_time
    // -> Cell culture doubling time
    let growthRate nCells0 nCellsT t =
        log2(nCellsT/nCells0)
        |> fun x -> x/t

    let doublingTime growthRate =
        (log2(2.))/growthRate

    let rootsYY = rootsXX |> Array.map fittingFunction
    
    let diff = rootsXX.[1] - rootsXX.[0]
    let min,max = rootsYY.[0], rootsYY.[1]

    let doublingTime =
        growthRate min max diff
        |> doublingTime

    doublingTime

let doublingTimeGr31To1 =

    ///use function (from print in LogistiFunction) to determine 3rd derivative. Use wolfram alpha to calculate roots
    let functionGr31To1 x =
        (18041000.000002 / (1. + exp(-0.130000 * (x - 33.875000)))) + 1659000.000000
    let rootsXX = [| 23.745 ; 44.005 |]

    calculateDoublingTime functionGr31To1 rootsXX

let doublingTimeGr31To25 =
    
    ///use function to determine 3rd derivative. Use wolfram alpha to calculate roots
    let functionGr31To25 x =
        (18121613.566379 / (1. + exp(-0.068163 * (x - 90.084689)))) + 77108.046495

    /// use rootsXX to calculate Y at roots.
    let rootsXX = [| 70.764 ; 109.405 |]
    
    calculateDoublingTime functionGr31To25 rootsXX

let doublingTimeGr31To50 =
    
    ///use function to determine 3rd derivative. Use wolfram alpha/or https://www.desmos.com/calculator/qszsndc8px to calculate roots
    let functionGr31To50 x =
        (18041000.000004 / (1. + exp(-0.090000 * (x - 80.875000)))) + 33180.000003

    /// use rootsXX to calculate Y at roots.
    let rootsXX = [| 66.242 ; 95.508 |]

    calculateDoublingTime functionGr31To50 rootsXX

let doublingTimeGr41To1 =
    
    ///use function to determine 3rd derivative. Use wolfram alpha to calculate roots
    let functionGr41To1 x =
        (18041000.000004 / (1. + exp(-0.090000 * (x - 80.875000)))) + 33180.000003

    /// use rootsXX to calculate Y at roots.
    let rootsXX = [| 22.995 ; 43.255 |]
    calculateDoublingTime functionGr41To1 rootsXX

let doublingTimeGr41To25 =
    
    ///use function to determine 3rd derivative. Use wolfram alpha to calculate roots
    let functionGr41To25 x =
        (18041000.000004 / (1. + exp(-0.090000 * (x - 80.875000)))) + 33180.000003

    /// use rootsXX to calculate Y at roots.
    let rootsXX = [| 68.964 ; 102.445 |]
    calculateDoublingTime functionGr41To25 rootsXX

let doublingTimeGr41To50 =
    
    ///use function to determine 3rd derivative. Use wolfram alpha to calculate roots
    let functionGr41To50 x =
        (17637849.446985 / (1. + exp(-0.089840 * (x - 84.165639)))) + 20536.724019

    /// use rootsXX to calculate Y at roots.
    let rootsXX = [| 68.507 ; 98.825 |]
    calculateDoublingTime functionGr41To50 rootsXX

let doublingTimeChart = 
    [
        Chart.Column(seq["1:1";"1:25";"1:50"],seq[doublingTimeGr31To1;doublingTimeGr31To25;doublingTimeGr31To50],Name = "Gr3")
            |> Chart.withMarkerStyle(Color = createHSL mainColorPeachFF6F61)
        Chart.Column(seq["1:1";"1:25";"1:50"],seq[doublingTimeGr41To1;doublingTimeGr41To25;doublingTimeGr41To50], Name = "Gr4")
            |> Chart.withMarkerStyle(Color = createHSL mainColorBlue92a8d1)
    ] 
    |> Chart.Combine

//let groupStr = "
//Gr3 : <span style=\"color: #FF6F61\">red</span><br>
//Gr4 : <span style=\"color: #92a8d1\">blue</span>
//    "

    //Gr 3
let chartGr3Combined = [
    showLogisticFunctionFit timePointsGr3 intensityPoints1To1Gr3 steepnessRange "1:1" mainColorPeachFF6F61
    showTheoreticalLogisticFunctionFit 
        timePointsGr3 
        intensityPoints1To25Gr3 
        steepnessRange
        (intensityPoints1To1Gr3 |> Array.max)
        ((timePointsGr3 |> Array.max) / 2. |> fun x -> x + 57. )
        (intensityPoints1To25Gr3 |> Array.min)
        "1:25"
        mainColorBlue92a8d1
    showTheoreticalLogisticFunctionFit
        timePointsGr3
        intensityPoints1To50Gr3
        steepnessRange
        (intensityPoints1To1Gr3 |> Array.max)
        ((timePointsGr3 |> Array.max) / 2. |> fun x -> x + 47. )
        (intensityPoints1To50Gr3 |> Array.min)
        "1:50"
        mainColorGreen88b04b
]

/// Gr 4
let chartGr4Combined = [
    showLogisticFunctionFit timePointsGr4 intensityPoints1To1Gr4 steepnessRange "1:1" mainColorPeachFF6F61
    showTheoreticalLogisticFunctionFit 
        timePointsGr4 
        intensityPoints1To25Gr4 
        steepnessRange
        (intensityPoints1To1Gr4 |> Array.max)
        ((timePointsGr4 |> Array.max) / 2. |> fun x -> x + 53. )
        (intensityPoints1To25Gr4 |> Array.min)
        "1:25"
        mainColorBlue92a8d1
    showTheoreticalLogisticFunctionFit
        timePointsGr4
        intensityPoints1To50Gr4
        steepnessRange
        (intensityPoints1To1Gr4 |> Array.max)
        ((timePointsGr4 |> Array.max) / 2. |> fun x -> x + 51. )
        (intensityPoints1To50Gr4 |> Array.min)
        "1:50"
        mainColorGreen88b04b
]

doublingTimeChart
|> Chart.withTitle("Doubling times for all cell cultures",Titlefont = (Font.init(Size = 30)))
|> Chart.withX_Axis (xAxis "Relative Dilution to origin culture")
|> Chart.withY_Axis (yAxis "doubling time [h]")
|> Chart.withConfig chartConfig
//|> Chart.Show

chartGr3Combined
|> Chart.Combine
|> Chart.withX_Axis (xAxis "time [hours]")
|> Chart.withY_Axis (yAxis "number of cells")
|> Chart.withTitle("Growth curve - Gr 3",Titlefont = (Font.init(Size = 30)))
|> Chart.withSize (1200.,600.)
|> Chart.withConfig chartConfig
//|> Chart.Show

chartGr4Combined
|> Chart.Combine
|> Chart.withX_Axis (xAxis "time [hours]")
|> Chart.withY_Axis (yAxis "number of cells")
|> Chart.withTitle("Growth curve - Gr 4",Titlefont = (Font.init(Size = 30)))
|> Chart.withSize (1200.,600.)
|> Chart.withConfig chartConfig
//|> Chart.Show

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//let exmp_x_Hours = [|0.; 19.5; 25.5; 43.; 48.5; 51.25; 67.75|]
//let exmp_y_Count = [| 1659000.; 4169000.; 6585400.; 16608400.; 17257800.; nan; 18041000.|]

////let model = Table.LogisticFunctionAscending

//let model = Table.LogisticFunctionVarYAscending

///// filter out any nans
//let exmp_x_Hours_Filtered,exmp_y_Count_Filtered =
//    Array.zip timeHGr3 oneTo1Gr3
//    |> Array.filter (fun (x,y) -> isNan y = false && isNan x = false )
//    |> Array.unzip


//Chart.Line(exmp_x_Hours_Filtered,exmp_y_Count_Filtered)
//|> Chart.Show

//let lineSolverOptions initialParamGuess = {
//    MinimumDeltaValue       = 0.00001
//    MinimumDeltaParameters  = 0.00001
//    MaximumIterations       = 10000
//    InitialParamGuess       = initialParamGuess
//    }

//let steepnessRange2 =
//    [|0.01 .. 0.01 .. 1.|]

//let initialGuess = 
//    steepnessRange2
//    |> Array.map (fun x -> 
//        lineSolverOptions [|
//            exmp_y_Count_Filtered |> Array.max
//            x 
//            34. //(exmp_x_Hours_Filtered |> Array.max) / 2.
//            exmp_y_Count_Filtered |> Array.min 
//        |]
//    )

//let estParamsRSS =
//    initialGuess
//    |> Array.map (
//        fun solvO ->
//            let lowerBound =
//                solvO.InitialParamGuess
//                |> Array.map (fun param -> param - (abs param) * 0.2)
//                |> vector
//            let upperBound =
//                solvO.InitialParamGuess
//                |> Array.map (fun param -> param + (abs param) * 0.2)
//                |> vector
//            LevenbergMarquardtConstrained.estimatedParamsWithRSS 
//                model solvO 0.001 10. lowerBound upperBound exmp_x_Hours_Filtered exmp_y_Count_Filtered
//    )
//    |> Array.filter (fun (param,rss) -> not(param |> Vector.exists System.Double.IsNaN))
//    |> Array.minBy snd
//    |> fun x ->
//        let next = fst x
//        printfn "Chosen Estimate: %A" x
//        printfn "Equation: (%f / (1. + exp(-%f * (%s - %f)))) + %f" next.[0] next.[1] "x" next.[2] next.[3]
//        next


///// Insert equation in https://www.ableitungsrechner.net/ and calculate third derivation. Get XXRoot at third derivation neighboring 
///// exponential phase

//let exmp_roots_XX = [| 23.745 ; 44.005 |]

//let fittingFunction = model.GetFunctionValue estParamsRSS

//let calculateDoublingTime2 fittingFunction rootsXX =

//    //https://en.wikipedia.org/wiki/Doubling_time
//    // -> Cell culture doubling time
//    let growthRate nCells0 nCellsT t =
//        log2(nCellsT/nCells0)
//        |> fun x -> x/t

//    let doublingTime growthRate =
//        (log2(2.))/growthRate

//    let rootsYY = rootsXX |> Array.map fittingFunction
    
//    let diff = rootsXX.[1] - rootsXX.[0]
//    let min,max = rootsYY.[0], rootsYY.[1]

//    let doublingTime =
//        growthRate min max diff
//        |> doublingTime

//    doublingTime

//calculateDoublingTime2 fittingFunction exmp_roots_XX

//let fittedY = Array.zip [|0. .. 68.|] ([|0. .. 68.|] |> Array.map fittingFunction)

//let fittedLogisticFunc =
//    [
//    Chart.Point (exmp_x_Hours, exmp_y_Count)
//    |> Chart.withTraceName"Data Points"
//    Chart.Line fittedY
//    |> Chart.withTraceName "Fit"
//    ]
//    |> Chart.Combine
//    |> Chart.withY_AxisStyle "Cellcount"
//    |> Chart.withX_AxisStyle "Time"
//    |> Chart.Show
