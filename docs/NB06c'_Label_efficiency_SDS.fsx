#r "nuget: FSharp.Stats, 0.4.0"
#r "nuget: BioFSharp, 2.0.0-beta5"
#r "nuget: BioFSharp.IO, 2.0.0-beta5"
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: BIO-BTE-06-L-7_Aux, 0.0.8"
#r "nuget: Deedle, 2.3.0"
#r "nuget: ISADotNet, 0.2.4"
#r "nuget: ISADotNet.XLSX, 0.2.4"

#if IPYNB
#r "nuget: Plotly.NET, 2.0.0-beta6"
#r "nuget: Plotly.NET.Interactive, 2.0.0-beta6"
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

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=NB06b_Label_efficiency.ipynb)

[Download Notebook](https://github.com/CSBiology/BIO-BTE-06-L-7/releases/download/NB06c/NB06c_Label_efficiency.ipynb)

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
let filePath = @

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
    |> Frame.filterRows (fun k s -> k.ProteinGroup |> String.contains "QProt_newCBB")

qConcatDataFiltered.ColumnKeys
|> Array.ofSeq

qConcatDataFilteredBN.ColumnKeys
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

let heavySDS = sliceQuantColumns "Heavy" qConcatDataFilteredSDS
let heavyBN = sliceQuantColumns "Heavy" qConcatDataFilteredBN

(**
... we can also use this function for information needed to reconstruct isotopic patterns.

## II. Extraction and visualization of measured isotopic envelopes.
*)

let heavyPatternMzSDS = sliceQuantColumns "heavyPatternMz" qConcatDataFilteredSDS
let heavyPatternISDS  = sliceQuantColumns "heavyPatternI" qConcatDataFilteredSDS

let heavyPatternMzBN = sliceQuantColumns "heavyPatternMz" qConcatDataFilteredBN
let heavyPatternIBN  = sliceQuantColumns "heavyPatternI" qConcatDataFilteredBN

(**
Now, there's a challenge: The info to reconstruct an isotopic pattern is
separated into two columns, the x component (heavyPatternMz) and the y component (heavyPatternI).
As always, this challenged can be solved using a function! 
Hint: Note how we define a function 'floatArrayOf' that specifies how the string is parsed. 
*)

let getHeavyPatternsInFile extractionType fileName = 
    let floatArrayOf s = 
        if String.isNullOrEmpty s then 
            [||]
        else
            s
            |> String.split (';') 
            |> Array.map float
    let mz, intensities =
        match extractionType with
        | "SDS" ->
            heavyPatternMzSDS 
            |> Frame.getCol fileName 
            |> Series.mapValues floatArrayOf,
            heavyPatternISDS 
            |> Frame.getCol fileName 
            |> Series.mapValues floatArrayOf
        | "BN" -> 
            heavyPatternMzBN
            |> Frame.getCol fileName 
            |> Series.mapValues floatArrayOf,
            heavyPatternIBN 
            |> Frame.getCol fileName 
            |> Series.mapValues floatArrayOf
        | _ -> failwith "ERROR: Accepted extractionType input is either \"SDS\" or \"BN\""
    let zipped = Series.zipInner mz intensities
    zipped

let extractedPatternsSDS = getHeavyPatternsInFile "SDS" "Gr2rbcL2_5"
let extractedPatternsBN = getHeavyPatternsInFile "BN" "20210312BN2_U1"

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

let getIsotopicPattern extractionType peptideSequence charge =
    let (k,(mzs,intensities)) = 
        match extractionType with
        | "SDS" ->
            extractedPatternsSDS
            |> Series.observations
            |> Seq.find (fun (k,(mzs,intensities)) -> 
                    k.StringSequence = peptideSequence && k.Charge = charge
                )
        | "BN" ->
        extractedPatternsSDS
            |> Series.observations
            |> Seq.find (fun (k,(mzs,intensities)) -> 
                    k.StringSequence = peptideSequence && k.Charge = charge
                ) 
        | _ -> failwith "ERROR: Accepted extractionType input is either \"SDS\" or \"BN\""
    {|
        PeptideSequence=k
        Charge  = charge
        Pattern = Seq.zip mzs intensities
    |}
(**
*)
let examplePep1SDS = getIsotopicPattern "SDS" "DTDILAAFR" 2
let examplePep1BN = getIsotopicPattern "BN" "DTDILAAFR" 2

plotIsotopicPattern FSharpAux.Colors.Table.Office.blue examplePep1SDS.Pattern
plotIsotopicPattern FSharpAux.Colors.Table.Office.blue examplePep1BN.Pattern
(***hide***)
plotIsotopicPattern FSharpAux.Colors.Table.Office.blue examplePep1SDS.Pattern |> GenericChart.toChartHTML
plotIsotopicPattern FSharpAux.Colors.Table.Office.blue examplePep1BN.Pattern |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
let examplePep2SDS = getIsotopicPattern "SDS" "LTYYTPDYVVR" 2
let examplePep2BN = getIsotopicPattern "BN" "LTYYTPDYVVR" 2

plotIsotopicPattern FSharpAux.Colors.Table.Office.blue examplePep2SDS.Pattern
plotIsotopicPattern FSharpAux.Colors.Table.Office.blue examplePep2BN.Pattern
(***hide***)
plotIsotopicPattern FSharpAux.Colors.Table.Office.blue examplePep2SDS.Pattern |> GenericChart.toChartHTML
plotIsotopicPattern FSharpAux.Colors.Table.Office.blue examplePep2BN.Pattern |> GenericChart.toChartHTML
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
let comp1SDS = compareIsotopicDistributions examplePep2SDS examplePep2_Sim1
let comp1BN = compareIsotopicDistributions examplePep2BN examplePep2_Sim1
comp1SDS.Plot
comp1BN.Plot
(***hide***)
comp1SDS.Plot |> GenericChart.toChartHTML
comp1BN.Plot |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
let comp2SDS = compareIsotopicDistributions examplePep2SDS examplePep2_Sim2
let comp2BN = compareIsotopicDistributions examplePep2SDS examplePep2_Sim2
comp2SDS.Plot
comp2BN.Plot
(***hide***)
comp2SDS.Plot |> GenericChart.toChartHTML
comp2BN.Plot |> GenericChart.toChartHTML
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

let lableEfficiencySDS,comparisonSDS,lableEfficiencyBN,comparisonBN = 
    (
        [|0.95 .. 0.001 .. 0.999|]
        |> Array.map (fun lableEfficiency -> 
                let sim = simulateFrom "DTDILAAFR" 2 lableEfficiency
                let comp = compareIsotopicDistributions' examplePep1SDS sim
                lableEfficiency,
                comp
            )
        |> Seq.unzip,
        [|0.95 .. 0.001 .. 0.999|]
        |> Array.map (fun lableEfficiency -> 
                let sim = simulateFrom "DTDILAAFR" 2 lableEfficiency
                let comp = compareIsotopicDistributions' examplePep1BN sim
                lableEfficiency,
                comp
            )
        |> Seq.unzip
    )
    |> fun (x,y) -> fst x, snd x, fst y, snd y
let bestFitSDS = comparisonSDS |> Seq.minBy (fun x -> x.KLDiv) 
let bestFitBN = comparisonBN |> Seq.minBy (fun x -> x.KLDiv) 

Chart.Point(lableEfficiencySDS,comparisonSDS |> Seq.map (fun x -> x.KLDiv))
|> Chart.withX_AxisStyle ""
|> Chart.withY_AxisStyle ""

Chart.Point(lableEfficiencyBN,comparisonBN |> Seq.map (fun x -> x.KLDiv))
|> Chart.withX_AxisStyle ""
|> Chart.withY_AxisStyle ""

(***hide***)
Chart.Point(lableEfficiencySDS,comparisonSDS |> Seq.map (fun x -> x.KLDiv)) |> GenericChart.toChartHTML
Chart.Point(lableEfficiencyBN,comparisonBN |> Seq.map (fun x -> x.KLDiv)) |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)

bestFitSDS.Plot
bestFitBN.Plot
(***hide***)
bestFitSDS.Plot |> GenericChart.toChartHTML
bestFitBN.Plot |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
let lableEfficiency2SDS,comparison2SDS,lableEfficiency2BN,comparison2BN = 
    (
        [|0.95 .. 0.001 .. 0.999|]
        |> Array.map (fun lableEfficiency -> 
                let sim = simulateFrom "LTYYTPDYVVR" 2 lableEfficiency
                let comp = compareIsotopicDistributions' examplePep2SDS sim
                lableEfficiency,
                comp
            )
        |> Seq.unzip,
        [|0.95 .. 0.001 .. 0.999|]
        |> Array.map (fun lableEfficiency -> 
                let sim = simulateFrom "LTYYTPDYVVR" 2 lableEfficiency
                let comp = compareIsotopicDistributions' examplePep2BN sim
                lableEfficiency,
                comp
            )
        |> Seq.unzip
    )
    |> fun ((a,b),(c,d)) -> a,b,c,d

let bestFit2SDS = comparison2SDS |> Seq.minBy (fun x -> x.KLDiv) 
let bestFit2BN = comparison2BN |> Seq.minBy (fun x -> x.KLDiv) 

Chart.Point(lableEfficiency2SDS,comparison2SDS |> Seq.map (fun x -> x.KLDiv))
|> Chart.withX_AxisStyle ""
|> Chart.withY_AxisStyle ""

Chart.Point(lableEfficiency2BN,comparison2BN |> Seq.map (fun x -> x.KLDiv))
|> Chart.withX_AxisStyle ""
|> Chart.withY_AxisStyle ""
(***hide***)
Chart.Point(lableEfficiency2SDS,comparison2SDS |> Seq.map (fun x -> x.KLDiv)) |> GenericChart.toChartHTML
Chart.Point(lableEfficiency2BN,comparison2BN |> Seq.map (fun x -> x.KLDiv)) |> GenericChart.toChartHTML
(***include-it-raw***)
(**
*)
bestFit2SDS.Plot
bestFit2BN.Plot
(***hide***)
bestFit2SDS.Plot |> GenericChart.toChartHTML
bestFit2BN.Plot |> GenericChart.toChartHTML
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
    let measuredSDS, measuredBN = 
        getIsotopicPattern "SDS" peptideSequence charge, 
        getIsotopicPattern "BN" peptideSequence charge
    let sim = simulateFrom peptideSequence charge lableEfficiency
    let compSDS, compBN = 
        compareIsotopicDistributions' measuredSDS sim,
        compareIsotopicDistributions' measuredBN sim
    compSDS.KLDiv, compBN.KLDiv

let est1SDS = Optimization.Brent.minimize (calcKL "DTDILAAFR" 2 >> fst) 0.98 0.999
let est1BN = Optimization.Brent.minimize (calcKL "DTDILAAFR" 2 >> snd) 0.98 0.999
let est2SDS = Optimization.Brent.minimize (calcKL "LTYYTPDYVVR" 2 >> fst) 0.98 0.999
let est2BN = Optimization.Brent.minimize (calcKL "LTYYTPDYVVR" 2 >> snd) 0.98 0.999

(**
Since the estimates have a certain level of uncertainty we will repeat the estimation for 
some high intensity peptides and visualize the results. Please fill the x axis description (|> Chart.withX_AxisStyle "")
*)

let highIntensityPeptidesSDS = 
    heavySDS
    |> Frame.getCol "WCGr1_U1" 
    |> Series.sortBy (fun (x:float) -> - x)
    |> Series.filter (fun k v -> k.StringSequence |> String.exists (fun x -> x='[') |> not)

let highIntensityPeptidesBN = 
    heavyBN
    |> Frame.getCol "WCGr1_U1" 
    |> Series.sortBy (fun (x:float) -> - x)
    |> Series.filter (fun k v -> k.StringSequence |> String.exists (fun x -> x='[') |> not)

let estimatesSDS = 
    highIntensityPeptidesSDS
    |> Series.take 20 
    |> Series.map (fun k v -> 
        FSharp.Stats.Optimization.Brent.minimize (calcKL k.StringSequence k.Charge >> fst) 0.98 0.999
        )
    |> Series.values
    |> Seq.choose id

let estimatesBN = 
    highIntensityPeptidesBN
    |> Series.take 20 
    |> Series.map (fun k v -> 
        FSharp.Stats.Optimization.Brent.minimize (calcKL k.StringSequence k.Charge >> snd) 0.98 0.999
        )
    |> Series.values
    |> Seq.choose id

Chart.BoxPlot estimatesSDS
|> Chart.withX_AxisStyle ""
(***hide***)
Chart.BoxPlot estimatesBN |> GenericChart.toChartHTML
(***include-it-raw***)

(**
Now that we know more than an educated guess of an lable efficiency estimate we can start with our main goal:
the absolute quantification of chlamydomonas proteins!
*)

