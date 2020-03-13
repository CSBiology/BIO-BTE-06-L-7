
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

let peptidesCharged =
    [|1.;2.|]
    |> Array.collect (fun x -> chargePeptidesBy x digestedPeptideMasses)


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

let pepsWithHydro = peptidesChargedWithHydro peptidesCharged

let sampleRndPeptidesAtHydro n hydroInd =
    pepsWithHydro
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

let rndPeps = sampleRndPeptides 100 peptidesCharged
let rndPeptidesAtHydro2 = sampleRndPeptidesAtHydro 100 2.
let rndPeptidesAtHydro6 = sampleRndPeptidesAtHydro 100 6.


[
    showIsotopeClusters "from all peptides" rndPeps
    showIsotopeClusters "from peptides around hi=2" rndPeptidesAtHydro2
    showIsotopeClusters "from peptides around hi=6" rndPeptidesAtHydro6
]
|> Chart.Stack 1
|> Chart.withSize (1200.,900.)
|> Chart.withTitle "Ionspectra for different hydrophobicity indice"
|> Chart.Show

1

//type Peptide = 
//    {
//        PeptideSequence: string
//        HydrophobicityInd: float
//        Mass: float
//    }
//    /// sequence, hydroInd, mass
//    static member create seq hydroInd mass = {
//        PeptideSequence = seq
//        HydrophobicityInd = hydroInd
//        Mass = mass
//    }

///// ersetzen durch smapleWithoutReplacement?????
//let random (pmf:Map<float,(float*seq<Peptide>)>) = 
//    if pmf.Count <= 0 then raise (System.Exception("Pmf contains no values") )  
//    let target = FSharp.Stats.Random.rndgen.NextFloat()
//    let x,y =
//        pmf
//        |> Seq.scan (fun state kv -> (kv.Key, fst kv.Value + snd state)) (0.,0.)
//        |> Seq.find (fun (x,y) -> y >= target)
//    x

///// Creates probability mass function for Peptide array and keeps peptide information for each bin   
//let createPeptideHis bandwidth (peptideArr:Peptide []) =            
//    let halfBw = bandwidth / 2.0       
//    let tmp = 
//        peptideArr
//        |> Seq.groupBy (fun x -> floor (x.Mass / bandwidth)) 
//        |> Seq.map (fun (k,values) -> 
//            let count = (Seq.length(values)) |> float                                        
//            if k < 0. then
//                ((k  * bandwidth) + halfBw, count, values)   
//            else
//                ((k + 1.) * bandwidth) - halfBw, count, values)
//        |> Seq.sortBy (fun (x,y,z) -> x)
//    printfn "%A" tmp
//    let area =
//        tmp
//        |> Seq.fold (fun acc (x,y,z) -> acc + y) 0.
//    printfn "%A" area
//    tmp    
//    |> Seq.map (fun (a,b,peps) -> (a,(b/ area, peps)))
//    |> Map.ofSeq

///// Samples "n" from empirical map "hisMap"
///// "hisMap" should be create via "createPeptideHis"
//let sampleList n hisMap =
//    let rnd = System.Random()
//    [0 .. n]
//    |> List.map (fun _ -> random hisMap)
//    |> List.map (fun x -> Map.find x hisMap)
//    |> List.map (
//        fun (mass,peps) -> 
//            let rndInd = rnd.Next(0,Seq.length peps)
//            Seq.item rndInd peps
//    )

//let digestPeptideMasses lowerMassBound upperMassBound =
//    digestedProteins
//    |> Array.choose (fun peptide ->
//        let mass = BioSeq.toMonoisotopicMassWith (BioItem.monoisoMass ModificationInfo.Table.H2O) peptide.PepSequence
//        let pepSeq = peptide.PepSequence |> BioList.toString
//        let hydro = 
//            let aalist =
//                pepSeq
//                |> BioArray.ofAminoAcidString
//                |> Array.map AminoAcidSymbols.aminoAcidSymbol
//            if aalist.Length > 5 then
//                aalist
//                |> (AminoProperties.ofWindowedBioArray 3 getHydrophobicityIndex 
//                >> Array.sum)
//            else 
//                aalist
//                |> (Array.map getHydrophobicityIndex 
//                >> Array.sum)
//        let newPep() = Peptide.create pepSeq hydro mass
//        if mass < upperMassBound && mass > lowerMassBound then Some (newPep()) else None
//    )



//let chargePeptidesBy charge peptides =
//    peptides 
//    |> Array.map (fun x -> {x with Mass = Mass.toMZ x.Mass charge})

//let chargedPeptideMasses lowerMassBound upperMassBound charges =
//    let digestedPeptides = digestPeptideMasses lowerMassBound upperMassBound
//    let chargedPeptides = charges |> Array.collect (fun x -> digestedPeptides |> chargePeptidesBy x)
//    chargedPeptides

//let peptidesWithHydroIndAround hydroInd inp =
//    inp
//    |> Array.filter (fun x -> x.HydrophobicityInd > hydroInd-0.05 && x.HydrophobicityInd < hydroInd+0.05)

//let massVisualization07 inp =
//    Chart.Histogram (inp |> Array.map (fun x -> x.Mass),Name="Peptides with Hydrophobicity Index around 0.7", Opacity=0.8)
//    //|> Chart.withX_AxisStyle (title = "Hydro Ind")
//    //|> Chart.withY_AxisStyle "Count"
//    //|> Chart.Show

//let massVisualization2 inp =
//    Chart.Histogram (inp |> Array.map (fun x -> x.Mass),Name="Peptides with Hydrophobicity Index around 2.", Opacity=0.8)
//    //|> Chart.withX_AxisStyle (title = "Hydro Ind")
//    //|> Chart.withY_AxisStyle "Count"
//    //|> Chart.Show

//let massVisualization4 inp =
//    Chart.Histogram (inp |> Array.map (fun x -> x.Mass),Name="Peptides with Hydrophobicity Index around 4.", Opacity=0.8)
//    //|> Chart.withX_AxisStyle (title = "Hydro Ind")
//    //|> Chart.withY_AxisStyle "Count"
//    //|> Chart.Show

//let massVisualization6 inp =
//    Chart.Histogram (inp |> Array.map (fun x -> x.Mass),Name="Peptides with Hydrophobicity Index around 6.", Opacity=0.8)
//    //|> Chart.withX_AxisStyle (title = "Hydro Ind")
//    //|> Chart.withY_AxisStyle "Count"
//    //|> Chart.Show

//let massVisualization inp =
//    Chart.Histogram (inp |> Array.map (fun x -> x.Mass), Name="All Peptides", Opacity=0.2)


//let createHydrophobicityChart lowerMassBound upperMassBound charges =
//    let masses = chargedPeptideMasses lowerMassBound upperMassBound charges
//    [
//        massVisualization masses
//        massVisualization07 (peptidesWithHydroIndAround 0.7 masses)
//        massVisualization2 (peptidesWithHydroIndAround 2. masses)
//        massVisualization4 (peptidesWithHydroIndAround 4. masses)
//        massVisualization6 (peptidesWithHydroIndAround 6. masses)
//    ]
//    |> Chart.Combine
//    //|> Chart.Stack(2)
//    |> Chart.withSize (900.,600.)
//    |> Chart.withTitle (sprintf "Chlamydomonas reinhardtii peptides between %A m/z and %A m/z for charge %A" lowerMassBound upperMassBound charges)
//    |> Chart.withX_AxisStyle ("Hydrophobicity Index")
//    |> Chart.withY_AxisStyle ("Count")

//createHydrophobicityChart 100. 3000. [|1.|] |> Chart.Show

//let masses = chargedPeptideMasses 100. 3000. [|1.|]

//let empHis = createPeptideHis 0.5 (peptidesWithHydroIndAround 6. masses)

//let my6List =
//    sampleList 10 empHis

////let testing =
////    my6List
////    |> List.groupBy (fun x -> x.PeptideSequence)

////testing.Length

//my6List
//|> List.map (fun x -> x.Mass)
//|> Chart.Histogram 
//|> Chart.withX_AxisStyle (title = "Mass [Da]")
//|> Chart.withY_AxisStyle "Count"
//|> Chart.Show

