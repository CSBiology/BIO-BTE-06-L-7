(** 
# JP02 Plant Systems Biology

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=JP02_Plant_Systems_Biology.ipynb)


1. [Plant Systems Biology](#Plant-Systems-Biology)
2. [Modeling growth for a defined cell number](#Modeling-growth-for-a-defined-cell-number)
1. [Insert Growth Data and Display as Chart](#Insert-Growth-Data-and-Display-as-Chart)
2. [Calculation of growth rate and doubling time for cell cultures](#Calculation-of-growth-rate-and-doubling-time-for-cell-cultures)
3. [Fitting biological growth curves](#Fitting-biological-growth-curves)<br>
    1. [Theory](#Theory)<br>
    2. [Model selection](#Model-selection)<br>
    3. [Exponential Fit](#Exponential-Fit)<br>
4. [Calculate Doubling Time](#Calculate-Doubling-Time)
5. [References](#References)
*)

(** 
## Plant Systems Biology
<a href="#Plant-Systems-Biology" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
The general paradigm of Systems Biology clearly applies to plants, as they represent complex biological systems. 
The functioning of a plant as a biological system is the result of a combination of multiple intertwined and dynamic interactions between its components. 
In addition, most plants are sessile systems that have to face fluctuating environmental conditions, including biotic and abiotic stresses<sup><a href="#8">8</a></sup>.
The process of a biological system responding to changes in environmental conditions is termed acclimation. These molecular physiological responses represent a complex 
dynamic adjustment of the interplay between genes, proteins and metabolites that allows the organism to acclimate to the changing environment. 
The ability to acclimate ensures the survival of all living organisms and is therefore fundamental for the understanding of biological systems. 
Detailed knowledge about how plants acclimate to a changing environment is crucial especially in times of global climate changes, 
as plants are of great importance for our quality of life as a key source of food, shelter, fiber, medicine, and fuel<sup><a href="#9">9</a></sup>.

The prominent model plant <i>Arabidopsis thaliana</i> is well suited for plant Systems Biology studies because sophisticated experimental tools and extensive data 
collections are readily available<sup><a href="#10">10</a></sup>. However, the importance of a model organism is not only coined by the availability of molecular 
tools to manipulate the organism, but also by its agricultural and economic impact like in the cases of tobacco, rice, maize or 
barley<sup><a href="#11">11</a></sup>. Also microalgae are of special economic interest due to their potential as biofuel producers<sup><a href="#12">12</a></sup>. 
Additionally, the use of organisms with lower biological complexity facilitates the feasibility of System Biology studies and is an important factor to consider 
for the choice of a suitable model organism in Systems Biology.

The eukaryotic green alga <i>Chlamydomonas reinhardtii</i> is particularly well suited for plant Systems Biology approaches. 
This unicellular freshwater and soil-dwelling alga has a single, cup-shaped chloroplast with a photosynthetic apparatus that is similar to 
that of higher plants<sup><a href="#13">13</a>,<a href="#14">14</a></sup>. Hence, results gained on photosynthesis processes in <i>Chlamydomonas</i> 
are likely to be transferable to higher plants. The nuclear, mitochondrial, and chloroplast genomes have been sequenced and tools for manipulating them 
are available<sup><a href="#14">14</a></sup>. <i>Chlamydomonas</i> cells have a size of ~10 µm and grow under photo-, mixo-, and heterotrophic conditions 
with a generation time of ~5-8 h<sup><a href="#15">15</a></sup>. <i>Chlamydomonas</i> can be maintained under controlled conditions and environmental 
changes can be applied homogeneously and rapidly to all cells in a liquid culture. In contrast to multicellular organisms there are no influences by 
tissue heterogeneity. Even the influence of different cell cycle stages may be ruled out by performing experiments with asynchronous cell cultures 
<sup><a href="#16">16</a>,<a href="#17">17</a></sup>. Finally, gene families in Chlamydomonas have fewer members than those in higher plants thus facilitating the 
interpretation of results involving many genes/proteins<sup><a href="#14">14</a></sup>.
</div>
*)

(** 
## Modeling growth for a defined cell number
<a href="#Plant-Systems-Biology" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
In order to solves real world task more convenient, F# provides a huge collection of additional programming libraries. 
Anything that extends beyond the basics must be written by a user. If the chunk of code is useful to multiple different users, 
it's often put into a library to make it easily reusable. A library is a collection of related pieces of code that have been compiled 
and stored together in a single file and can than be used an included. The most important libraries in F# for bioinformatics are:

<ul>
    <li><a href="https://csbiology.github.io/BioFSharp/">BioFSharp</a>: Open source bioinformatics and computational biology toolbox written in F#</li>
    <li><a href="https://csbiology.github.io/FSharp.Stats/">FSharp.Stats</a>: F# project for statistical computing </li>
    <li><a href="https://github.com/plotly/Plotly.NET">Plotly.NET</a>: .NET interface for plotly.js written in F# 📈 </li>
</ul>

The first real world use case of F# in Systems Biology is to model growth for a defined cell number to see possible overexpression effects. 
Biologists often utilize growth experiments to analyze basic properties of a given organism or cellular model. For a solid comparison of data 
obtain from different experiment and to investigate the speciﬁc eﬀect of a given experimental set up, modeling the growth is needed after recording the data. 

This notebook introduces two basic ways to model growth of <i>Chlamydomonas reinhardtii</i> using F#.

Now, let's get started by loading our libraries first.
</div>
*)

#r "nuget: FSharp.Stats, 0.4.0"
#r "nuget: Plotly.NET, 2.0.0-beta6"
//#load "..\IfSharp\Paket.Generated.Refs.fsx"
//#load "..\IfSharp\FSharp.Plotly.fsx"
open Plotly.NET
open FSharp.Stats
open FSharp.Stats.Fitting.NonLinearRegression

(** 
## Insert Growth Data and Display as Chart
<a href="#Plant-Systems-Biology" style="display: inline-block"><sup>&#8593;back</sup></a><br>

A normal cell culture experiment with measurements for the growth curve will return data like the following.
Multiple cell counts (y_Count) each related to a specific timepoint (x_Hours).
*)
// Code-Block 1

let exmp_x_Hours = [|0.; 19.5; 25.5; 43.; 48.5; 51.25; 67.75|]
let exmp_y_Count = [|1659000.; 4169000.; 6585400.; 16608400.; 17257800.; nan; 18041000.|]

/// filter out any nans. These could be introduced through missing measurements.
let exmp_x_Hours_Filtered,exmp_y_Count_Filtered =
    Array.zip exmp_x_Hours exmp_y_Count
    |> Array.filter (fun (x,y) -> isNan y = false && isNan x = false )
    |> Array.unzip

// Such data can easily be display with the following code block.
// Chart.Point takes a sequence of x-axis-points and a series of y-axis-points as input
let example_Chart_1 = 
    Chart.Point(exmp_x_Hours_Filtered,exmp_y_Count_Filtered)
    // some minor styling with title and axis-titles.
    |> Chart.withTitle "Growth curve of <i>Clamydomonas reinhardtii</i> cell cultures"
    |> Chart.withX_AxisStyle ("Number of cells")
    |> Chart.withY_AxisStyle ("Time [hours]")

(***hide***)
example_Chart_1 |> GenericChart.toChartHTML
(***include-it-raw***)

(**
## Calculation of growth rate and doubling time for cell cultures
<a href="#Plant-Systems-Biology" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
The normal growth of an in vitro cell culture is defined through three phases. The lag phase in which the cells still acclimate to the growth conditions, the exponential growth, also called log phase, during which cell growth is exponential due to the proliferation of cells into two daughter cells, and the stationary phase in which the growth rate and the death rate are equal. The stationary phase is typically initiated due to limitations in growth conditions, e.g. depletion of essential nutrients or accumulation of toxic/inhibitory excretions/products. The doubling time defines a time interval in which the quantity of cells doubles and is calculated as seen in Equation 1.

<i>Equation 1: Calculation of the doubling time. Growth rate is calculates as shown in Equation 2.</i>

<div class="container">
<img src="https://latex.codecogs.com/gif.latex?doubling&space;Time&space;=&space;\frac{ln(2)}{growthRate}" title="doubling Time = \frac{ln(2)}{growthRate}" style="margin: 1rem auto 0; display: block"/>
</div>

Growth rate can then be calculated as shown in Equation 2.

<i>Equation 2: Calculation of the growth rate. With N(t) = the number of cells at time t, N(0) = number of cells at time 0, gr = growth rate, and t = time.</i>

<div class="container">
<img src="https://latex.codecogs.com/gif.latex?gr=\frac{ln(\frac{N(t)}{N(0)})}{t}" title="gr=\frac{ln(\frac{N(t)}{N(0)})}{t}" style="margin: 1rem auto 0; display: block"/>
</div>
</div>
<hr>
*)

(** 
## Fitting biological growth curves
<a href="#Plant-Systems-Biology" style="display: inline-block"><sup>&#8593;back</sup></a><br>


### Theory
<a href="#Plant-Systems-Biology" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
To derive parameters required for e.g. the doubling time calculation, the measured growth data points have to be modelled. 
In order to obtain a continuous function with known coefficients, a suitable model function is fitted onto the existing data. 
Many models exist, each one of them optimized for a specific task<sup><a href="#18">18</a></sup>.

Linear model function example: <img src="https://latex.codecogs.com/gif.latex?f(x)&space;=&space;mx&space;&plus;&space;b" title="f(x) = mx + b" />
 
When a model is fitted onto the data, there are endless possibilities to choose coefficients of the model function. 
In the case above there are two coefficients to be identified: The slope m and the y-intercept b. But how can the best fitting coefficients be determined?

Therefore a quality measure called <b>Residual Sum of Squares (RSS)</b> is used. It describes the discrepancy of the measured points 
and the corresponding estimation model. If the discrepancy is small, the RSS is small too.

In regression analysis the optimal set of coefficients (m and b) that <a href= "https://mathworld.wolfram.com/LeastSquaresFitting.html"> minimizes the RSS is searched</a>.

If there is no straightforward way to identify the RSS-minimizing coefficient set, then the problem is part of nonlinear regression. 
Here, initial coefficients are guessed and the RSS is calculated. Thereafter, the coefficients are modified in tiny steps. 
If the RSS decreases, the direction of the coefficient change seems to be correct. 
By <a href= "https://books.google.de/books?id=rs51DwAAQBAJ&pg=PA422&lpg=PA422&dq=rss+minimizing+solver&source=bl&ots=qZ0Y4cYtM-&sig=ACfU3U0rHGWCmTo_kv5wqYMmSo8ZKyj5Pg&hl=de&sa=X&ved=2ahUKEwjKtdf-oaHoAhUUwsQBHX07DTwQ6AEwBHoECAkQAQ#v=onepage&q=rss%20minimizing%20solver&f=false"> 
iteratively changing coefficients</a>, the optimal coefficient set is determined when no further change leads to an decrease in RSS. 
Algorithms, that perform such a 'gradient descent' methods to solve nonlinear regression tasks are called <b>solver</b> 
(e.g. Gauss-Newton algorithm or Levenberg–Marquardt algorithm). <a href= "https://www.youtube.com/watch?v=sDv4f4s2SB8"> Introduction to RSS and optimization problems. </a>
</div>

### Model selection
<a href="#Plant-Systems-Biology" style="display: inline-block"><sup>&#8593;back</sup></a><br>
     
<div class="container">
Under certain circumstances, more than one solution may arise out of a optimization process. 
If the solutions are based on the same data and the same fitting model, the function minimizing the RSS can be selected as best estimator. 
</div>

### Exponential Fit
<a href="#Plant-Systems-Biology" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
Since cellular growth behaves in an exponential manner, it seems to make sense to use an exponential fitting function. 

<img src="https://latex.codecogs.com/gif.latex?f(x)&space;=&space;ae^{bx}" title="f(x) = ae^{bx}" />

As seen below, the resulting <a href="https://da.khanacademy.org/science/biology/ecology/population-growth-and-regulation/a/exponential-logistic-growth"> exponential fit</a> does not represent the data sufficiently, even though it is the best fit, that a exponential model can provide. This is caused by the lag- and stationary phase, both not following an exponential increase. 
In order to use an exponential function as model, it would be necessary to discard data points from lag- and stationary phases and model the remaining data points. 

 
There are two main problems regarding this workflow: 
<ol>
    <li value="(1)"> The assignment of points to lag-, log-, and stationary phases is a nontrivial task.</li>
    <li value="(2)"> The exponential phase only lasts a short period of time and therefore the number of points that can be assigned to the log phase is (very) low.</li> 
</ol>
Consequential the fitted function is not robust against variance introduced during cell count measurements.

</div>

*)

// Code-Block 2

// An template exponential function has the form f(x) = a * exp(b * x) with the two unknowns a and b. 

// The model we need already exists in FSharp.Stats and can be taken from the "Table" module.
let expModel = Table.expModel

// the solver needs additional information like the initial coefficient guesses or the coefficient accuracy
// FSharp.Stats assists by estimating the required parameters based on the original input data
let solverOptions = Table.expSolverOptions exmp_x_Hours_Filtered exmp_y_Count_Filtered

// The Gauss-Newton solver is used to find the optimal coefficients for an exponential function (expModel)
// The result is a vector, containing parameter a and b as floats.
let coefficientsExp = GaussNewton.estimatedParams expModel solverOptions exmp_x_Hours_Filtered exmp_y_Count_Filtered

// The determined coefficients can be inserted into the exponential template function
let fittingExpFunction x = coefficientsExp.[0] * System.Math.Exp(coefficientsExp.[1] * x)

// create a chart with 
let exp_Chart_1 = 
    [|0. .. (Array.last exmp_x_Hours_Filtered)|]
    |> Array.map (fun xValue -> xValue,fittingExpFunction xValue) // gives tuples of (xValue,yValue)
    |> Chart.Line
    |> Chart.withTraceName "exponential fit"

// styling of the chart axis
let templateAxis title = Axis.LinearAxis.init(Title=title,Showgrid=false,Showline=true,Mirror=StyleParam.Mirror.All)       

let exponentialFitChart =
    [
        example_Chart_1
        |> Chart.withTraceName "data points"
        exp_Chart_1
    ]
    |> Chart.Combine
    |> Chart.withX_Axis (templateAxis "time [hours]")
    |> Chart.withY_Axis (templateAxis "number of cells")
    |> Chart.withSize (900.,600.)

(***hide***)
exponentialFitChart |> GenericChart.toChartHTML
(***include-it-raw***)
    
(**
## Logistic regression fit
<a href="#Plant-Systems-Biology" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
As seen above, the model selection is a crucial step for obtaining reasonable functions and to derive function properties with 
which further studies are examined. The selected model should match the theoretical (time) course of the studied signal, but under 
consideration of Occams razor principle. It states, that a approriate model with a low number of coefficients should be preferred over a 
model with many coefficients, since the excessive use of coefficients leads to overfitting.

A better model that can be used in growth curve fitting, is a <a href= "https://en.wikipedia.org/wiki/Logistic_function">logistic function</a>. 
It is defined by a minimum, a maximum, and a sigmoidal transition between those two. Thereby, the lag, log, and stationary phase are covered.

The function has the form: <img src="https://latex.codecogs.com/gif.latex?f(x)=\frac{L}{1&plus;e^{-k(x-x_{0})}}&plus;N" title="f(x)=\frac{L}{1+e^{-k(x-x_{0})}}+N" />

where:
        
__L__ = curve maximum

__k__ = steepness

__x0__ = xValue of sigmoid's midpoint

__N__ = curve minimum

In the following, we will go through the necessary steps to calculate the doubling time with the help of a logistic fit. 
This is more complex than the exponential fit, but the given problem requires a more sophisticated method.
</div>
*)

// Code-Block 3

// The model we need already exists in FSharp.Stats and can be taken from the "Table" module.
let modelLogistic = Table.LogisticFunctionVarYAscending

// To fit the logistic function, the solver requires more parameters. Some of them are stored in the solverOption type
let lineSolverOptions initialParamGuess = {
    // defines the stepwidth of the x_value change
    MinimumDeltaValue       = 0.00001
    // defines the stepwidth of the parameter change
    MinimumDeltaParameters  = 0.00001
    // defines the number of iterations until the solver converges to a solution
    MaximumIterations       = 10000
    // initial parameters to start the solving algorithms
    // vector containing all coefficients of the function: vector [L;k;x0;N]
    InitialParamGuess       = initialParamGuess
    }

// Generation of solverOptions with varying steepnesses
let initialGuess = 
    // maximum measured intensity/cell count
    let l  = exmp_y_Count_Filtered |> Array.max
    // estimate of the xValue of sigmoid's midpoint
    let x0 = (exmp_x_Hours_Filtered |> Array.max) / 2.
    // minimum measured intensity/cell count.
    let n  = exmp_y_Count_Filtered |> Array.min 
    
    //since steepness in unknown, a variety of steepnesses is provided 
    let steepnessRange = [|0.01 .. 0.01 .. 1.|]
    
    steepnessRange
    |> Array.map (fun steepness -> lineSolverOptions [|l; steepness; x0; n|])

// Estimate coefficients for a possible solution based on residual sum 
// of squares. Besides the solverOptions, an upper and lower bound for 
// the parameters are required. It is recommended to define them depending 
// on the initial param guess.
// It reports coefficients tupled with a corresponding RSS value.
let estimateCoefficientsRSS =
    initialGuess
    |> Array.map (fun solvOpt ->
        let lowerBound =
            solvOpt.InitialParamGuess
            |> Array.map (fun param -> param - (abs param) * 0.2)
            |> vector
        let upperBound =
            solvOpt.InitialParamGuess
            |> Array.map (fun param -> param + (abs param) * 0.2)
            |> vector
        // all parameters are given to the solver and the set of coefficients that minimize the RSS are reported
        LevenbergMarquardtConstrained.estimatedParamsWithRSS 
            modelLogistic         // logistic function model
            solvOpt               // solver options with optimization parameters and initial guess
            0.001                 //
            10.                   //
            lowerBound            // lower bound of coefficient space
            upperBound            // upper bound of coefficient space
            exmp_x_Hours_Filtered // x data
            exmp_y_Count_Filtered // y data
    )
    |> Array.filter (fun (coeffs,rss) -> not(coeffs |> Vector.exists System.Double.IsNaN)) // discard solutions with nan as coefficients
    |> Array.minBy snd // minimize all possible solutions based on RSS quality measure
    |> fun (solCoeffs,rss) ->
        printfn "Chosen Estimate: %A" solCoeffs
        printfn "Equation: (%.1f / (1. + exp(-%.3f * (x - %.3f)))) + %.1f" solCoeffs.[0] solCoeffs.[1] solCoeffs.[2] solCoeffs.[3]
        solCoeffs
        
// Create fitting function from optimal coefficients
let fittingLogisticFunction = modelLogistic.GetFunctionValue estimateCoefficientsRSS

// Code-Block 4

// fit of the optimized logistic function over all x Values
let fittedY = 
     [|0. .. exmp_x_Hours |> Array.max|]
     |> Array.map (fun x -> x, fittingLogisticFunction x) //tupled (xValue,yValue)

let fittedLogisticFunc =
    [
        // raw chart
        Chart.Point (exmp_x_Hours, exmp_y_Count) |> Chart.withTraceName"data points"
        // logistic fit
        Chart.Line fittedY                       |> Chart.withTraceName "logistic fit"
    ]
    |> Chart.Combine
    |> Chart.withY_Axis (templateAxis "cell count")
    |> Chart.withX_Axis (templateAxis "time [Hours]")

(***hide***)
fittedLogisticFunc |> GenericChart.toChartHTML
(***include-it-raw***)

(**
    
## Calculate Doubling Time
<a href="#Plant-Systems-Biology" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<div class="container">
To calculate the doubling time it is necessary to determine the growth rate (gr) for <i>equation 1</i>.
To get gr we make use of the first and second derivative of the logistic function. They can be calculated by hand or with help 
of <a href="https://www.ableitungsrechner.net/">derivative calculator</a>.

The first derivative of the logistic function is: 

<div class="container">
<img src="https://latex.codecogs.com/gif.latex?\dfrac{kl\mathrm{e}^{k\left(x-m\right)}}{\left(\mathrm{e}^{k\left(x-m\right)}&plus;1\right)^2}" title="\dfrac{kl\mathrm{e}^{k\left(x-m\right)}}{\left(\mathrm{e}^{k\left(x-m\right)}+1\right)^2}" style="margin: 1rem auto 0; display: block" />
</div>

The second derivative of the logistic function is: 

<div class="container">
<img src="https://latex.codecogs.com/gif.latex?-\dfrac{k^2l\left(\mathrm{e}^{k\left(x-m\right)}-1\right)\mathrm{e}^{k\left(x-m\right)}}{\left(\mathrm{e}^{k\left(x-m\right)}&plus;1\right)^3}" title="-\dfrac{k^2l\left(\mathrm{e}^{k\left(x-m\right)}-1\right)\mathrm{e}^{k\left(x-m\right)}}{\left(\mathrm{e}^{k\left(x-m\right)}+1\right)^3}" style="margin: 1rem auto 0; display: block" />
</div>

</div>
*)

// Code-Block 5

// calculate fst derivative of logistic function
let fstDerivative x = 
    let l  = estimateCoefficientsRSS.[0]
    let k  = estimateCoefficientsRSS.[1]
    let x0 = estimateCoefficientsRSS.[2]
    let n  = estimateCoefficientsRSS.[3]
    let exp = System.Math.Exp(k*(x-x0))
    - (k**2.*l*(exp - 1.)* exp ) / (exp + 1.)**3.

// calculate snd derivative of logistic function 
let sndDerivative x = 
    let l  = estimateCoefficientsRSS.[0]
    let k  = estimateCoefficientsRSS.[1]
    let x0 = estimateCoefficientsRSS.[2]
    let n  = estimateCoefficientsRSS.[3]
    let exp = System.Math.Exp(k*(x-x0))
    (k*l*exp) / (exp + 1.)**2.

// calculate derivatives to corresponding x values
let yValuesOfDerivative fkt = 
    [|0. .. 0.5 .. exmp_x_Hours |> Array.max|]
    |> Array.map (fun x -> x,fkt x)
    
let fitAllLogisticFunc =
    [
        Chart.Line (yValuesOfDerivative fstDerivative)|> Chart.withTraceName "fst derivative"
        Chart.Line (yValuesOfDerivative sndDerivative)|> Chart.withTraceName "snd derivative"
    ]
    |> Chart.Combine
    |> Chart.withY_Axis (templateAxis "slope or curvature")
    |> Chart.withX_Axis (templateAxis "time [hours]")

(***hide***)
fitAllLogisticFunc |> GenericChart.toChartHTML
(***include-it-raw***)

(**
<div class="container">
We define the region between the maximal and minimal curvature (second derivative) as the time period to derive the growth rate from. 
An alternative is to just use the slope at the midpoint because this is the point of maximal slope (minimal generation time), but since 
this calculation would be only dependent from this particular point we go for the more conservative approach.
    
When the xValues of the maximal curvatures are identified (either by calculus or by plotting the derivatives) the generation time calculation
 is straight forward (<a href="https://en.wikipedia.org/wiki/Doubling_time">Wikipedia - Doubling time</a>).
</div>
*)

// Code-Block 6

// The exponential phase is considered to be between the maximal positive curvature
// and the minimal negative curvature of the fitting functions (other interpretations possible).
let xValuesOfMaximalCurvature = [| 23.5 ; 44.0 |]

let calculateDoublingTimeLogistic fittingFunction =

    //https://en.wikipedia.org/wiki/Doubling_time
    // -> Cell culture doubling time
    let growthRate nCells0 nCellsT t =
        log(nCellsT/nCells0)
        |> fun x -> x/t

    let doublingTime growthRate =
        (log(2.))/growthRate

    /// get the corresponding cell counts around the midpoint of the logistic function
    let rootsYY = xValuesOfMaximalCurvature |> Array.map fittingFunction
    
    /// calculate the time difference between both roots
    let diff = xValuesOfMaximalCurvature.[1] - xValuesOfMaximalCurvature.[0]
    
    /// get the minimum measured cell count and the maximum measured cell count
    let min,max = rootsYY.[0], rootsYY.[1]

    let doublingTime =
        growthRate min max diff
        |> doublingTime

    doublingTime
    
let doublingTime = 
    calculateDoublingTimeLogistic fittingLogisticFunction
    
sprintf "The doubling time is %.2f hours." doublingTime

(*** include-it ***)

(**
<nav class="level is-mobile">
    <div class="level-left">
        <div class="level-item">
            <button class="button is-primary is-outlined" onclick="location.href='/JP01_FSharpExcercises.html';">&#171; JP01</button>
        </div>
    </div>
    <div class="level-right">
        <div class="level-item">
            <button class="button is-primary is-outlined" onclick="location.href='/JP03_Mass_spectrometry_based_proteomics.html';">JP03 &#187;</button>
        </div>
    </div>
</nav>
*)

(**
## References
<a href="#Plant-Systems-Biology" style="display: inline-block"><sup>&#8593;back</sup></a><br>

<br>

<ol>
<li Value="8" Id="8">Ruffel, S., Krouk, G. & Coruzzi, G. M. A systems view of responses to nutritional cues in Arabidopsis: toward a paradigm shift for predictive network modeling. Plant physiology 152, 445–452; 10.1104/pp.109.148502 (2010).</li>
<li Id="9"> Minorsky, P. V. Achieving the in Silico Plant. Systems Biology and the Future of Plant Biological Research. Plant physiology 132, 404–409; 10.1104/pp.900076 (2003). <div>
<li Id="10">Van Norman, Jaimie M & Benfey, P. N. Arabidopsis thaliana as a model organism in systems biology. Wiley interdisciplinary reviews. Systems biology and medicine 1, 372–379; 10.1002/wsbm.25 (2009).</li>
<li Id="11">Pãcurar, D. I. Model organisms - a journey from the dawn of biological research to the post-genomic era. Romanian Society of Biological Sciences, 4087–4094 (2009).</li>
<li Id="12">Cagnon, C. et al. Development of a forward genetic screen to isolate oil mutants in the green microalga Chlamydomonas reinhardtii. Biotechnology for biofuels 6, 178; 10.1186/1754-6834-6-178 (2013).</li>
<li Id="13">Eberhard, S., Finazzi, G. & Wollman, F.-A. The dynamics of photosynthesis. Annual review of genetics 42, 463–515; 10.1146/annurev.genet.42.110807.091452 (2008).</li>
<li Id="14">Merchant, S. S. et al. The Chlamydomonas genome reveals the evolution of key animal and plant functions. Science (New York, N.Y.) 318, 245–250; 10.1126/science.1143609 (2007).</li>
<li Id="15">Harris, E. H. The chlamydomonas sourcebook. 2nd ed. (Academic, London, 2008).</li>
<li Id="16">Bruggeman, F. J. & Westerhoff, H. V. The nature of systems biology. Trends in microbiology 15, 45–50; 10.1016/j.tim.2006.11.003 (2007).</li>
<li Id="17">Harris, E. H. CHLAMYDOMONAS AS A MODEL ORGANISM. Annual review of plant physiology and plant molecular biology 52, 363–406; 10.1146/annurev.arplant.52.1.363 (2001).</li>
<li Id="18">Kaplan, S. et al. Comparison of growth curves using non-linear regression function in Japanese squail. Journal of Applied Animal Research 46, 112-117; 10.1080/09712119.2016.1268965 (2018).</li>.
</ol>
*)