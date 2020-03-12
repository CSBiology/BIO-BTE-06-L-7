
#load @"../IfSharp/Paket.Generated.Refs.fsx"

open BioFSharp
open BioFSharp.IO
open FSharp.Stats
open FSharp.Plotly
open AminoProperties
open BioFSharp
open BioFSharp.Elements
open BioFSharp.Formula

let createBarChart inputData name =
    let x,y = inputData |> List.unzip
    let column =
        let dyn = Trace("bar")
        dyn?x <- x
        dyn?y <- y
        dyn?name <- name
        dyn?textposition <- "auto"
        dyn?textposition <- "outside"
        dyn?constraintext <- "inside"
        dyn?fontsize <- 20.
        dyn?width <- 0.2
        dyn?opacity <- 0.8
        dyn
    GenericChart.ofTraceObject column

let getHydrophobicityIndex  = initGetAminoProperty AminoProperty.HydrophobicityIndex

let toFormula bioseq =  
    bioseq
    |> BioSeq.toFormula
    |> Formula.add Formula.Table.H2O

///Predicts an isotopic distribution of the given formula at the given charge, normalized by the sum of probabilities, using the MIDAs algorithm
let generateIsotopicDistribution (charge:int) (f:Formula.Formula) =
    IsotopicDistribution.MIDA.ofFormula 
        IsotopicDistribution.MIDA.normalizeByMaxProb
        0.01
        0.005
        charge
        f

let source = __SOURCE_DIRECTORY__
let filePath = source + @"/../AuxFiles/Chlamy_JGI5_5(Cp_Mp).fasta"

let sequences = 
    filePath
    |> FastA.fromFile BioArray.ofAminoAcidString
    |> Seq.toArray

let digestedProteins =
    sequences
    |> Array.mapi (fun i fastAItem ->
        Digestion.BioArray.digest Digestion.Table.Trypsin i fastAItem.Sequence
        |> Digestion.BioArray.concernMissCleavages 0 0
    )
    |> Array.concat

let digestedPeptideMasses =
    digestedProteins
    |> Array.choose (fun peptide ->
        let mass = BioSeq.toMonoisotopicMassWith (BioItem.monoisoMass ModificationInfo.Table.H2O) peptide.PepSequence
        if mass < 3000. then Some (mass,peptide) else None
    )

let chargePeptidesBy charge (peptides:(float*_) []) =
    peptides 
    |> Array.map (fun (x,y) -> Mass.toMZ x charge, y, charge)


let sampleRndPeptides n (peptides: (float*Digestion.DigestedPeptide<int>*float)[])=
    peptides
    |> Array.shuffleFisherYates
    |> Array.take n
    |> Array.map (fun (mass,pep,charge) -> 
        sprintf "%s, z=%i" (BioList.toString pep.PepSequence) (int charge),
        mass,
        pep,
        charge
    )

let showIsotopeClusters name (rndPeptides: (string*float*Digestion.DigestedPeptide<int>*float) []) =

    let getMin = rndPeptides |> (Array.map (fun (w,x,y,z) -> x) >> Array.min) |> fun x -> x - 15.
    let getMax= rndPeptides |> (Array.map (fun (w,x,y,z) -> x) >> Array.max) |> fun x -> x + 15.

    rndPeptides
    |> Array.map (
        fun (name,mass,pep,charge) ->
            let isotopicCluster = generateIsotopicDistribution (int charge) (toFormula pep.PepSequence)
            createBarChart 
                isotopicCluster 
                name
    )
    |> Chart.Combine
    |> Chart.withX_AxisStyle("m/z - " + name, MinMax=(-10.,3010.))
    |> Chart.withSize (900.,400.)
    |> Chart.withY_AxisStyle ("Intensity", MinMax=(0.,1.3))

let peptidesChargedWithHydro peptides =
    let getHydro ((pepMass,pep,pepCharge): (float*Digestion.DigestedPeptide<int>*float)) = 
        let aalist =
            pep.PepSequence
            |> BioList.toString
            |> BioArray.ofAminoAcidString
            |> Array.map AminoAcidSymbols.aminoAcidSymbol
        if aalist.Length > 5 then
            aalist
            |> (AminoProperties.ofWindowedBioArray 3 getHydrophobicityIndex 
            >> Array.sum)
        else 
            aalist
            |> (Array.map getHydrophobicityIndex 
            >> Array.sum)
        |> fun x -> pepMass,x,pep,pepCharge
    peptides
    |> Array.map getHydro


let sampleRndPeptidesAtHydro n hydroInd (hydroPeps:(float*float*Digestion.DigestedPeptide<int>*float)[])=
    hydroPeps
    |> Array.filter (fun (x,y,z,w) -> y > hydroInd-0.05 && y < hydroInd+0.05)
    |> fun x -> 
        printfn "%i" x.Length
        x |> Array.shuffleFisherYates
    |> Array.take n
    |> Array.map (fun (mass,hydroInd,pep,charge) -> 
        sprintf "%s, z=%i, hi=%A" (BioList.toString pep.PepSequence) (int charge) hydroInd,
        mass,
        pep,
        charge
    )
