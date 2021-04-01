#r "nuget: FSharp.Stats, 0.4.0"
#r "nuget: BioFSharp, 2.0.0-beta5"
#r "nuget: BioFSharp.IO, 2.0.0-beta5"
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: BIO-BTE-06-L-7_Aux, 0.0.8"
#r "nuget: Deedle, 2.3.0"
#r "nuget: ISADotNet, 0.2.4"
#r "nuget: ISADotNet.XLSX, 0.2.4"

#if IPYNB
#r "nuget: Plotly.NET, 2.0.0-beta8"
#r "nuget: Plotly.NET.Interactive, 2.0.0-beta8"
#endif // IPYNB

open System.IO
open ISADotNet
open ISADotNet.API
open Deedle
open BioFSharp
open FSharpAux
open FSharp.Stats
open Plotly.NET
open System.IO
open BIO_BTE_06_L_7_Aux.FS3_Aux
open BIO_BTE_06_L_7_Aux.Deedle_Aux

(**
# NB06c' Label efficiency for SDS

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=NB06c_Label_efficiency_BN.ipynb)

[Download Notebook](https://github.com/CSBiology/BIO-BTE-06-L-7/releases/download/NB06b_NB06b_NB06c_NB06c_NB06d_NB06d/NB06c_Label_efficiency_BN.ipynb)

Stable isotopic peptide labeling is the foundation of QconCAT experiments. While an excellent tool when carried out with correctly, it also exposes 
challenges and pitfalls that have to be checked and possibly accounted for. One of these pitfalls is the efficiency with which we labeled 
our QconCAT protein (Why?). In this notebook we will have a look at some high quality peptides selected in the previous notebook and 
illustrate how the label efficiency can be calculated using simulations.  

## I. Reading the data
As promised, we start this notebook with the output of the previous analysis, this notebook assumes that the data from *NB06b Data Access and Quality Control* is stored in a .txt
*)

type PeptideIon = 
    {|
        ProteinGroup    : string  
        Synonyms        : string
        StringSequence  : string
        PepSequenceID   : int
        Charge          : int
    |}

//This is the filepath you chose in *NB06b Data Access and Quality Control*
// let filePath = @"C:\YourPath\testOut.txt"
let filePath = System.IO.Path.Combine [|__SOURCE_DIRECTORY__; "downloads"; "qualityControlResult_BN.txt"|]

// What is different about this function from the one known from the last notebook?
let qConcatDataFiltered =
    Frame.ReadCsv(path = filePath, separators = "\t")
    // StringSequence is the peptide sequence
    |> Frame.indexRowsUsing (fun os -> 
            let proteinGroup = os.GetAs<string>("ProteinGroup")
            {|
                ProteinGroup    = os.GetAs<string>("ProteinGroup"); 
                Synonyms        = os.GetAs<string>("Synonyms")
                StringSequence  = os.GetAs<string>("StringSequence");
                PepSequenceID   = os.GetAs<int>("PepSequenceID");
                Charge          = os.GetAs<int>("Charge");
            |}
        )
    |> Frame.filterRows (fun k s -> k.ProteinGroup |> String.contains "QProt_newPS")

qConcatDataFiltered.ColumnKeys
|> Array.ofSeq

(***include-it***)

(**
First we reuse a proved pattern and define a function to manipulate our frame
*)

let sliceQuantColumns quantColID frame = 
    frame
    |> Frame.filterCols (fun ck os -> ck |> String.contains ("." + quantColID))
    |> Frame.mapColKeys (fun ck -> ck.Split('.') |> Array.item 0)

(**
Besides already familiar slices...
*)

let heavy = sliceQuantColumns "Heavy" qConcatDataFiltered

(**
... we can also use this function for information needed to reconstruct isotopic patterns.

## II. Extraction and visualization of measured isotopic envelopes.
*)

let heavyPatternMz = sliceQuantColumns "heavyPatternMz" qConcatDataFiltered
let heavyPatternI  = sliceQuantColumns "heavyPatternI" qConcatDataFiltered


(**
Now, there's a challenge: The info to reconstruct an isotopic pattern is
separated into two columns, the x component (heavyPatternMz) and the y component (heavyPatternI).
As always, this challenged can be solved using a function! 
Hint: Note how we define a function 'floatArrayOf' that specifies how the string is parsed. 
*)

let getHeavyPatternsInFile fileName = 
    let floatArrayOf s = 
        if String.isNullOrEmpty s then 
            [||]
        else
            s
            |> String.split (';') 
            |> Array.map float
    let mz, intensities =
        heavyPatternMz
        |> Frame.getCol fileName 
        |> Series.mapValues floatArrayOf,
        heavyPatternI
        |> Frame.getCol fileName 
        |> Series.mapValues floatArrayOf
    let zipped = Series.zipInner mz intensities
    zipped

let extractedPatterns = getHeavyPatternsInFile "20210312BN2_U1"

(***include-it***)

(**
Additionally, we can write two functions to plot the patterns of a peptide. When it comes
to the build the chart (plotIsotopicPattern), things get a little bit trickier, but this is not necessarily your concern. Please inspect the Chart 
created by 'plotIsotopicPatternOf' and write correct descriptions for the x and the y axis. (Fill: |> Chart.withX_AxisStyle "" and |> Chart.withY_AxisStyle "")
*)

let plotIsotopicPattern color mzsAndintensities =
    let min,max =
        mzsAndintensities |> Seq.minBy fst |> fst,
        mzsAndintensities |> Seq.maxBy fst |> fst
    Seq.map (fun (x,y) -> 
        Chart.Line([x;x],[0.;y], Showlegend = false)
        |> Chart.withLineStyle (Width = 7)
    ) mzsAndintensities
    |> Chart.Combine
    |> Chart.withMarkerStyle(Size=0,Color = FSharpAux.Colors.toWebColor color)
    |> Chart.withX_AxisStyle ("", MinMax = (min - 1., max + 1.))
    |> Chart.withY_AxisStyle ""

type ExtractedIsoPattern = 
    {|
        PeptideSequence : PeptideIon
        Charge          : int
        Pattern         : seq<(float*float)>
    |}

let getIsotopicPattern peptideSequence charge =
    let (k,(mzs,intensities)) = 
        extractedPatterns
        |> Series.observations
        |> Seq.find (fun (k,(mzs,intensities)) -> 
                k.StringSequence = peptideSequence && k.Charge = charge
            )
    {|
        PeptideSequence=k
        Charge  = charge
        Pattern = Seq.zip mzs intensities
    |}
(**
*)
let examplePep1 = getIsotopicPattern "DTDILAAFR" 2

(***condition:ipynb***)
#if IPYNB
plotIsotopicPattern FSharpAux.Colors.Table.Office.blue examplePep1.Pattern
#endif // IPYNB
(***hide***)
plotIsotopicPattern FSharpAux.Colors.Table.Office.blue examplePep1.Pattern |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
let examplePep2 = getIsotopicPattern "LTYYTPDYVVR" 2

(***condition:ipynb***)
#if IPYNB
plotIsotopicPattern FSharpAux.Colors.Table.Office.blue examplePep2.Pattern
#endif // IPYNB
(***hide***)
plotIsotopicPattern FSharpAux.Colors.Table.Office.blue examplePep2.Pattern |> GenericChart.toChartHTML
(***include-it-raw***)

(**
## III. Simulation of isotopic patterns: revisited.

Now that we visualized the patterns of two sample peptides, we will simulate theoretical patterns
and compare them to the ones we measured! You will recognize a lot of the used code from *NB02c Isotopic distribution*
Note: we copy the code so you can make yourself familiar with it, of course we could also reference functions defined beforehand.
*)

// create chemical formula for amino acid and add water to reflect hydrolysed state in mass spectrometer
let toFormula bioseq =  
    bioseq
    |> BioSeq.toFormula
    // peptides are hydrolysed in the mass spectrometer, so we add H2O
    |> Formula.add Formula.Table.H2O

let label n15LableEfficiency formula =
    let heavyN15 = Elements.Di  (Elements.createDi "N15" (Isotopes.Table.N15,n15LableEfficiency) (Isotopes.Table.N14,1.-n15LableEfficiency) )
    Formula.replaceElement formula Elements.Table.N heavyN15

// Predicts an isotopic distribution of the given formula at the given charge, 
// normalized by the sum of probabilities, using the MIDAs algorithm
let generateIsotopicDistribution (charge:int) (f:Formula.Formula) =
    IsotopicDistribution.MIDA.ofFormula 
        IsotopicDistribution.MIDA.normalizeByProbSum
        0.01
        0.000000001
        charge
        f

type SimulatedIsoPattern = 
    {|
        PeptideSequence : string
        Charge          : int
        LableEfficiency : float
        SimPattern      : list<(float*float)>
    |}

let simulateFrom peptideSequence charge lableEfficiency =
    let simPattern =
        peptideSequence
        |> BioSeq.ofAminoAcidString
        |> toFormula 
        |> label lableEfficiency
        |> generateIsotopicDistribution charge     
    {|
        PeptideSequence = peptideSequence
        Charge          = charge
        LableEfficiency = lableEfficiency
        SimPattern      = simPattern
    |}
(**
*)
let examplePep2_Sim1 = simulateFrom "LTYYTPDYVVR" 2 0.95

plotIsotopicPattern FSharpAux.Colors.Table.Office.orange examplePep2_Sim1.SimPattern
(***hide***)
plotIsotopicPattern FSharpAux.Colors.Table.Office.orange examplePep2_Sim1.SimPattern |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
let examplePep2_Sim2 = simulateFrom "LTYYTPDYVVR" 2 0.99

plotIsotopicPattern FSharpAux.Colors.Table.Office.orange examplePep2_Sim2.SimPattern
(***hide***)
plotIsotopicPattern FSharpAux.Colors.Table.Office.orange examplePep2_Sim2.SimPattern |> GenericChart.toChartHTML
(***include-it-raw***)
(**
## IV. Comparing measured and theoretical isotopic patterns.

As we see, there is a discrepancy between real and simulated patterns, both in peak height and in peak count. 
But before we compare both patterns, we have to take some things into consideration.
While both patterns are normalized in a way that their intensities
sum to 1., they were normalized independently from each other. Since it is often not possible to 
extract all peaks of an isotopic pattern from a MS run (e.g. due to measurement inaccuracies), we have to 
write a function which filters the simulated patterns for those peaks present in the experimentally 
measured one. Then we normalize it again and have two spectra that can be compared.
// How are distributions called that sum up to 1?
*)

let normBySum (a:seq<float*float>) =
    let s = Seq.sumBy snd a 
    Seq.map (fun (x,y) -> x,y / s) a

let compareIsotopicDistributions (measured:ExtractedIsoPattern) (simulated:SimulatedIsoPattern)= 
    let patternSim' = 
        measured.Pattern 
        |> Seq.map (fun (mz,intensities) -> 
                mz,
                simulated.SimPattern
                |> Seq.filter (fun (mzSim,intensitiesSim) -> abs(mzSim-mz) < 0.05 )
                |> Seq.sumBy snd
            )
        |> normBySum
    {|
        Plot = 
            [
            plotIsotopicPattern FSharpAux.Colors.Table.Office.blue measured.Pattern
            plotIsotopicPattern FSharpAux.Colors.Table.Office.orange patternSim'
            ]
            |> Chart.Combine
    |}
(**
*)
let comp1 = compareIsotopicDistributions examplePep2 examplePep2_Sim1
comp1.Plot
(***hide***)
comp1.Plot |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
let comp2 = compareIsotopicDistributions examplePep2 examplePep2_Sim2
comp2.Plot
(***hide***)
comp2.Plot |> GenericChart.toChartHTML
(***include-it-raw***)
(**
Comparing both simulations, we see that the simulation with a label efficiency of 0.99 fits the measured spectra better than the simulation with 0.95.
But since we do not want to find a better fit, but the best fit to our measured pattern, this is no goal that is achievable in a feasable way 
using visual inspections. As a solution we utilize the fact that isotopic patterns can be abstracted as ___ ___ (See: How are distributions called that sum up to 1?) distributions.
A measure to compare measured and theoretical distributions is the kullback leibler divergence. The following code block extends the function 
'compareIsotopicDistributions' to compute the KL divergence between the precisely measured distribution p and our approximation 
of p (q) using the mida algorithm. 
*)

/// Calculates the Kullback-Leibler divergence Dkl(p||q) from q (theory, model, description, or approximation of p) 
/// to p (the "true" distribution of data, observations, or a ___ ___ precisely measured).
let klDiv (p:seq<float>) (q:seq<float>) = 
    Seq.fold2 (fun acc p q -> (System.Math.Log(p/q)*p) + acc ) 0. p q

let compareIsotopicDistributions' (measured:ExtractedIsoPattern) (simulated:SimulatedIsoPattern)= 
    let patternSim' = 
        measured.Pattern 
        |> Seq.map (fun (mz,intensities) -> 
                mz,
                simulated.SimPattern
                |> Seq.filter (fun (mzSim,intensitiesSim) -> abs(mzSim-mz) < 0.05 )
                |> Seq.sumBy snd
            )
        |> normBySum
    let klDiv = klDiv (patternSim' |> Seq.map snd)  (measured.Pattern |> Seq.map snd)
    {|
        KLDiv = klDiv
        Plot  = 
            [
            plotIsotopicPattern FSharpAux.Colors.Table.Office.blue measured.Pattern
            plotIsotopicPattern FSharpAux.Colors.Table.Office.orange patternSim'
            ]
            |> Chart.Combine
    |}

(**
## V. Determining the lable efficiency: an optimiziation problem.

Using this function we can now visualize the kullback leibler divergence between
different models and the two peptides we measured. Since the lower the divergence. We will
also visualize the pattern with the best fit. Please inspect the Chart created by 'Chart.Point(lableEfficiency,comparison |> Seq.map (fun x -> x.KLDiv))' 
and write correct descriptions for the x and the y axis. (Fill: |> Chart.withX_AxisStyle "" and |> Chart.withY_AxisStyle "")
*)

let lableEfficiency, comparison = 
    [|0.95 .. 0.001 .. 0.999|]
    |> Array.map (fun lableEfficiency -> 
            let sim = simulateFrom "DTDILAAFR" 2 lableEfficiency
            let comp = compareIsotopicDistributions' examplePep1 sim
            lableEfficiency,
            comp
        )
    |> Seq.unzip
let bestFit = comparison |> Seq.minBy (fun x -> x.KLDiv) 

Chart.Point(lableEfficiency,comparison |> Seq.map (fun x -> x.KLDiv))
|> Chart.withX_AxisStyle ""
|> Chart.withY_AxisStyle ""


(***hide***)
Chart.Point(lableEfficiency,comparison |> Seq.map (fun x -> x.KLDiv)) |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)

bestFit.Plot
(***hide***)
bestFit.Plot |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
let lableEfficiency2, comparison2 = 
    [|0.95 .. 0.001 .. 0.999|]
    |> Array.map (fun lableEfficiency -> 
            let sim = simulateFrom "LTYYTPDYVVR" 2 lableEfficiency
            let comp = compareIsotopicDistributions' examplePep2 sim
            lableEfficiency,
            comp
        )
    |> Seq.unzip

let bestFit2 = comparison2 |> Seq.minBy (fun x -> x.KLDiv) 

Chart.Point(lableEfficiency2,comparison2 |> Seq.map (fun x -> x.KLDiv))
|> Chart.withX_AxisStyle ""
|> Chart.withY_AxisStyle ""


(***hide***)
Chart.Point(lableEfficiency2,comparison2 |> Seq.map (fun x -> x.KLDiv)) |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
bestFit2.Plot
(***hide***)
bestFit2.Plot |> GenericChart.toChartHTML
(***include-it-raw***)

(**
Observing the output, we can make two observations: the function x(lablefficiency) = KL(measured,sim(lableeffciency)) has in both cases a local minimum
that is similar, yet slightly different for peptides "LTYYTPDYVVR" and "DTDILAAFR", and that the best fit resembles the measured distribution closely, but not
perfectly, what is the reason for this?

Finding this local minimum will give us the best estimator for the lable efficiency. This can be done using brute force approaches (as we just did) 
or more elaborate optimization techniques. For this we will use an algorithm called 'Brent's method'. This method is more precise and speeds up the calculation time (Why?). 
How close are the estimates?
*)

let calcKL peptideSequence charge lableEfficiency = 
    let measured = 
        getIsotopicPattern peptideSequence charge
    let sim = simulateFrom peptideSequence charge lableEfficiency
    let comp = 
        compareIsotopicDistributions' measured sim
    comp.KLDiv

let est1 = Optimization.Brent.minimize (calcKL "DTDILAAFR" 2) 0.98 0.999
let est2 = Optimization.Brent.minimize (calcKL "LTYYTPDYVVR" 2) 0.98 0.999

(**
Since the estimates have a certain level of uncertainty we will repeat the estimation for 
some high intensity peptides and visualize the results. Please fill the x axis description (|> Chart.withX_AxisStyle "")
*)

let highIntensityPeptides = 
    heavy
    |> Frame.getCol "20210312BN2_U1" 
    |> Series.sortBy (fun (x:float) -> - x)
    |> Series.filter (fun k v -> k.StringSequence |> String.exists (fun x -> x='[') |> not)

let estimates = 
    highIntensityPeptides
    |> Series.take 20 
    |> Series.map (fun k v -> 
        FSharp.Stats.Optimization.Brent.minimize (calcKL k.StringSequence k.Charge) 0.98 0.999
        )
    |> Series.values
    |> Seq.choose id

(***condition:ipynb***)
#if IPYNB
Chart.BoxPlot estimates
|> Chart.withX_AxisStyle ""
#endif // IPYNB
(***hide***)
Chart.BoxPlot estimates |> GenericChart.toChartHTML
(***include-it-raw***)

(**
Now that we know more than an educated guess of an lable efficiency estimate we can start with our main goal:
the absolute quantification of chlamydomonas proteins!
*)

