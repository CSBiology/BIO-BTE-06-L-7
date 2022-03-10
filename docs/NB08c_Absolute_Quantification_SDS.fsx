#r "nuget: FSharp.Stats, 0.4.3"
#r "nuget: BioFSharp, 2.0.0-beta5"
#r "nuget: BioFSharp.IO, 2.0.0-beta5"
#r "nuget: Plotly.NET, 2.0.0-preview.16"
#r "nuget: BIO-BTE-06-L-7_Aux, 0.0.9"
#r "nuget: Deedle, 2.5.0"
#r "nuget: ISADotNet, 0.4.0-preview.4"
#r "nuget: ISADotNet.XLSX, 0.4.0-preview.4"
#r "nuget: ISADotNet.IO, 0.0.2"

#if IPYNB
#r "nuget: Plotly.NET.Interactive, 2.0.0-preview.16"
#endif // IPYNB

open System.IO
open ISADotNet
open Deedle
open BioFSharp
open FSharpAux
open FSharp.Stats
open Plotly.NET
open arcIO.NET

(**
# NB08c Absolute Quantification (for SDS-PAGE results)

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=NB06d_Absolute_Quantification_SDS.ipynb)

[Download Notebook](https://github.com/CSBiology/BIO-BTE-06-L-7/releases/download/NB08c/NB08c_Absolute_Quantification_SDS.ipynb)

Finally, after careful peptide ion selection, quality control and assuring that our label efficiency allows accurate for quantifications, we can start to
calculate protein abundancies. Since we start again by getting access to our data and its description, this notebook will start off familiar!


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

//This is the filepath you chose in *NB08a Data Access and Quality Control (results for SDS-PAGE)*
let filePath = @"C:\yourPath\testOut.txt"

let qConcatDataFiltered =
    Frame.ReadCsv(path = filePath, separators = "\t")
    // StringSequence is the peptide sequence
    |> Frame.indexRowsUsing (fun os -> 
        {|
            ProteinGroup    = os.GetAs<string>("ProteinGroup"); 
            Synonyms        = os.GetAs<string>("Synonyms")
            StringSequence  = os.GetAs<string>("StringSequence");
            PepSequenceID   = os.GetAs<int>("PepSequenceID");
            Charge          = os.GetAs<int>("Charge");
        |}
    )
    |> Frame.filterRows (fun k s -> String.contains "QProt_newPS" k.ProteinGroup)

(**
## III. From Ratios to mol proteins per cell.

Now we can use the extensive information stored in the sample sheet and map each quantified peptide ion passing
the quality checks to an estimator for protein abundance! First we start off by defining a function to extract ratios:
*)

let sliceQuantColumns quantColID frame = 
    frame
    |> Frame.filterCols (fun ck os -> ck |> String.contains ("." + quantColID))
    |> Frame.mapColKeys (fun ck -> ck.Split('.') |> Array.item 0)

let ratios = sliceQuantColumns "Ratio" qConcatDataFiltered

(** 
Finally, we can iterate the ratios and map each to a protein abundance using our well annotated experiment.
*)

//
let absoluteAbundances = 
    ratios
    |> Frame.map (fun peptide fileName ratio -> 
        try 
            let μgChlorophPerMlCult     = getμgChlorophPerMlCult fileName
            let cellCountPerMlCult      = getCellCountPerMlCult fileName
            let μgChlorophPerμlSample   = getμgChlorophPerμlSample fileName
            let μgProtPerμlSample       = getμgProtPerμlSample fileName
            let μgQProtSpike            = get15N_PS_Amount fileName
            let μgloadedProtein         = getLoadAmount fileName
            let molWeightQProt          = PSMass
            let molWeightTargetProt     = peptide.AverageProtGroupMass
            let result = 
                calcAbsoluteAbundance
                    μgChlorophPerMlCult  
                    cellCountPerMlCult   
                    μgChlorophPerμlSample
                    μgProtPerμlSample    
                    μgQProtSpike         
                    μgloadedProtein
                    molWeightQProt       
                    molWeightTargetProt
                    ratio
            result.AttoMolProteinPerCell
        with :? System.FormatException -> nan
    )

(***condition:ipynb***)
#if IPYNB
formatAsTable 1500. absoluteAbundances |> Chart.Show
#endif // IPYNB

// Why don't we see results for the SDS experiments with CBB-QProt?
// Why don't we see any results in the BN experiment?
// Discuss (and maybe look into your assay file if you don't progress).

(**
To see if our calculations are not off, we look at the calculated abundance for the well studied abundances of rbcL and RBCS
and compare this to the published knowledge about these proteins.
For this, we write a function that, given a protein synonym and a list of peptide sequences, returns a list of quantifications (via mean)
and the estimated uncertainty (via standard deviation). The results can then be visualized using e.g. column charts.
*)

let extractAbsoluteAbundancesOf filterCutoutProtein groupID prot peptidelist = 
    absoluteAbundances
    |> Frame.filterRows (fun k s -> k.Synonyms |> String.contains prot)
    |> Frame.filterRows (fun k s -> 
        peptidelist |> List.exists (fun (sequence,charge) -> sequence = k.StringSequence && charge = k.Charge)
    )
    |> Frame.getNumericCols 
    |> Series.observationsAll
    |> Seq.sortBy (fun (k, x) -> getLoadAmount k)
    |> Seq.filter (fun (k, x) -> getGroupID k = groupID)
    // why do we filter out the protein which is not cut out here?
    |> fun res ->
        if filterCutoutProtein then
            res
            |> Seq.filter (fun (k, x) -> 
                match getCutoutBand k with
                | RbcL -> String.contains (prot.ToLower()) (k.ToLower())
                | RbcS -> String.contains (prot.ToLower()) (k.ToLower())
            )
        else res
    |> Seq.choose (fun (k,x) -> 
        match x with
        | Some x -> Some (k, x)
        | _ -> None
    )
    |> Seq.map (fun (k, v) -> 
        {|
            Filename   = k 
            MeanQuant  = Stats.mean v
            StdevQuant = Stats.stdDev v
        |}
    )

// with filtering
let rbclQuantification = 
    extractAbsoluteAbundancesOf true 2 "rbcL" ["DTDILAAFR", 2;"FLFVAEAIYK", 2] 
    |> Array.ofSeq
let rbcsQuantification = 
    extractAbsoluteAbundancesOf true 2 "RBCS" ["AFPDAYVR", 2;"LVAFDNQK", 2] 
    |> Array.ofSeq

let protAbundanceChart =
    [
        Chart.Column(rbclQuantification |> Seq.map (fun x -> x.Filename + "_rbcL"), rbclQuantification |> Seq.map (fun x -> x.MeanQuant))
        |> Chart.withYErrorStyle (rbclQuantification |> Seq.map (fun x -> x.StdevQuant))
        |> Chart.withTraceName "rbcL"
        Chart.Column(rbcsQuantification |> Seq.map (fun x -> x.Filename + "_rbcS"), rbcsQuantification |> Seq.map (fun x -> x.MeanQuant))
        |> Chart.withYErrorStyle (rbcsQuantification |> Seq.map (fun x -> x.StdevQuant))
        |> Chart.withTraceName "RBCS"
    ]
    |> Chart.combine
    |> Chart.withYAxisStyle "protein abundance [amol/cell]"

(***condition:ipynb***)
#if IPYNB
protAbundanceChart
#endif // IPYNB

// without filtering. Compare both Charts.
let rbclQuantification' = 
    extractAbsoluteAbundancesOf false 2 "rbcL" ["DTDILAAFR", 2;"FLFVAEAIYK", 2] 
    |> Array.ofSeq
let rbcsQuantification' = 
    extractAbsoluteAbundancesOf false 2 "RBCS" ["AFPDAYVR", 2;"LVAFDNQK", 2] 
    |> Array.ofSeq

let protAbundanceChart' =
    [
        Chart.Column(rbclQuantification' |> Seq.map (fun x -> x.Filename + "_rbcL"), rbclQuantification' |> Seq.map (fun x -> x.MeanQuant))
        |> Chart.withYErrorStyle (rbclQuantification' |> Seq.map (fun x -> x.StdevQuant))
        |> Chart.withTraceName "rbcL"
        Chart.Column(rbcsQuantification' |> Seq.map (fun x -> x.Filename + "_rbcS"), rbcsQuantification' |> Seq.map (fun x -> x.MeanQuant))
        |> Chart.withYErrorStyle (rbcsQuantification' |> Seq.map (fun x -> x.StdevQuant))
        |> Chart.withTraceName "RBCS"
    ]
    |> Chart.combine
    |> Chart.withYAxisStyle "protein abundance [amol/cell]"

(***condition:ipynb***)
#if IPYNB
protAbundanceChart'
#endif // IPYNB

(**
Since we didn't change the amount of QProt given to the sample but the amount of protein loaded into our SDS-PAGE, we check the reliability of our experiments via comparing 
the chart above with a chart of the protein quantification per band. We remember that the bands were loaded with different amounts of the proteins seperated by the SDS-PAGE.

Uncomment everything below.
This is the formula from above. Now that we want to get the protein per band in mol, just add a respective calculation to get the protein band in mol and define it in the 
anonymous record type.
*)

// let calcAbsoluteAbundance μgChlorophPerMlCult cellCountPerMlCult μgChlorophPerμlSample μgProtPerμlSample μgQProtSpike μgloadedProtein molWeightQProt molWeightTargetProt ratio1415N =
//     let chlorophPerCell : float = μgChlorophPerMlCult / cellCountPerMlCult
//     let cellsPerμlSample = μgChlorophPerμlSample / chlorophPerCell
//     let μgProteinPerCell = μgProtPerμlSample / cellsPerμlSample
//     let molQProtSpike = μgQProtSpike * 10. ** -6. / molWeightQProt
//     let molProtPerBand = ratio1415N * molQProtSpike
//     let molProtIn1μgLoadedProt = molProtPerBand / μgloadedProtein
//     let gTargetProtIn1μgLoadedProt = molWeightTargetProt * molProtIn1μgLoadedProt
//     let molProteinPerCell = molProtIn1μgLoadedProt * μgProteinPerCell
//     let proteinsPerCell = molProteinPerCell * 6.022 * 10. ** 23.
//     let attoMolProteinPerCell = molProteinPerCell * (10. ** 18.)
//     let attoMolProteinPerBand = ??? // <--- write your calculation here
//     {|
//         MassTargetProteinInLoadedProtein    = gTargetProtIn1μgLoadedProt
//         ProteinsPerCell                     = proteinsPerCell
//         AttoMolProteinPerCell               = attoMolProteinPerCell
//         AttoMolProteinPerBand               = attoMolProteinPerBand
//     |}

// let absoluteBandAbundances = 
//     ratios
//     |> Frame.map (fun peptide fileName ratio -> 
//         try 
//             let μgChlorophPerMlCult     = getμgChlorophPerMlCult fileName
//             let cellCountPerMlCult      = getCellCountPerMlCult fileName
//             let μgChlorophPerμlSample   = getμgChlorophPerμlSample fileName
//             let μgProtPerμlSample       = getμgProtPerμlSample fileName
//             let μgQProtSpike            = get15N_PS_Amount fileName
//             let μgloadedProtein         = getLoadAmount fileName
//             let molWeightQProt          = PSMass
//             let molWeightTargetProt     = peptide.AverageProtGroupMass
//             let result = 
//                 calcAbsoluteAbundance
//                     μgChlorophPerMlCult  
//                     cellCountPerMlCult   
//                     μgChlorophPerμlSample
//                     μgProtPerμlSample    
//                     μgQProtSpike         
//                     μgloadedProtein
//                     molWeightQProt       
//                     molWeightTargetProt
//                     ratio
//             result.AttoMolProteinPerBand
//         with :? System.FormatException -> nan
//     )

// let extractAbsoluteBandAbundancesOf prot peptidelist = 
//     absoluteBandAbundances
//     |> Frame.filterRows (fun k s -> k.Synonyms |> String.contains prot)
//     |> Frame.filterRows (fun k s -> 
//         peptidelist |> List.exists (fun (sequence,charge) -> sequence = k.StringSequence && charge = k.Charge)
//     )
//     |> Frame.getNumericCols 
//     // |> Series.filter (fun k s -> getExpressionLevel k = "")
//     |> Series.map (fun k v -> 
//         {|
//             Synonym    = k 
//             MeanQuant  = Stats.mean v
//             StdevQuant = Stats.stdDev v
//         |}
//     )
//     |> Series.values

// let rbclBandQuantification = 
//     extractAbsoluteBandAbundancesOf "rbcL" ["DTDILAAFR", 2;"FLFVAEAIYK", 2] 
//     |> Array.ofSeq
// let rbcsBandQuantification = 
//     extractAbsoluteBandAbundancesOf "RBCS" ["AFPDAYVR", 2;"LVAFDNQK", 2] 
//     |> Array.ofSeq

// let protAbundanceBandChart =
//     [
//         Chart.Column(rbclBandQuantification |> Seq.map (fun x -> x.Synonym),rbclBandQuantification |> Seq.map (fun x -> x.MeanQuant))
//         |> Chart.withYErrorStyle (rbclBandQuantification |> Seq.map (fun x -> x.StdevQuant))
//         |> Chart.withTraceName "rbcL"
//         Chart.Column(rbcsBandQuantification |> Seq.map (fun x -> x.Synonym),rbcsBandQuantification |> Seq.map (fun x -> x.MeanQuant))
//         |> Chart.withYErrorStyle (rbcsBandQuantification |> Seq.map (fun x -> x.StdevQuant))
//         |> Chart.withTraceName "RBCS"
//     ]
//     |> Chart.Combine
//     |> Chart.withY_AxisStyle "protein abundance per band [amol/band]"

// protAbundanceBandChart |> Chart.Show

(**
Comparing this to the published results (see: https://www.frontiersin.org/articles/10.3389/fpls.2020.00868/full) we see that our preliminary results are
not only in the same order of magnitude as the published values, but in many cases really close! Of course it could be that you see systematic differences between your results
and published results. As data analysts it is now your task to estimate if the differences are the product of biology (e.g. different growth conditions or genetic background)
or caused by technical artifacts (e.g. different amounts of spiked proteins, mistakes estimating a parameter like the cell count) which could be accounted for by developing
normalization strategies. We look forward to read your explanations!
*)