
#load @"../IfSharp/Paket.Generated.Refs.fsx"

open BioFSharp
open BioFSharp.IO
open FSharp.Stats
open FSharp.Plotly
open AminoProperties
open BioFSharp
open BioFSharp.Elements
open BioFSharp.Formula


//[JP4] Digestion and mass calculation
//The most widely applied method for protein digestion involves the use of enzymes. Many proteases are available for this purpose,
//each having their own characteristics in terms of specificity, efficiency and optimum digestion conditions. Trypsin is most widely applied
//in bottom-up proteomics and and has a very high degree of specificity, cleaving the peptide bonds C-terminal to the basic residues Lys and Arg,
//except when followed by Pro 23. In general, Lys and Arg are relatively abundant amino acids and are usually well distributed throughout a
//protein 24. This leads to tryptic peptides with an average length of ?14 amino acids that carry at least two positive charges, which is ideally
//suited for CID-MS analysis23.
let source = __SOURCE_DIRECTORY__
let filePath = source + @"/../AuxFiles/Chlamy_JGI5_5(Cp_Mp).fasta"

/// Tuple
type Peptide = 
    {
        PeptideSequence: string
        HydrophobicityInd: float
        Mass: float
    }
    /// sequence, hydroInd, mass
    static member create seq hydroInd mass = {
        PeptideSequence = seq
        HydrophobicityInd = hydroInd
        Mass = mass
    }

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

let getHydrophobicityIndex  = initGetAminoProperty AminoProperty.HydrophobicityIndex


/// ersetzen durch smapleWithoutReplacement?????
let random (pmf:Map<float,(float*seq<Peptide>)>) = 
    if pmf.Count <= 0 then raise (System.Exception("Pmf contains no values") )  
    let target = FSharp.Stats.Random.rndgen.NextFloat()
    let x,y =
        pmf
        |> Seq.scan (fun state kv -> (kv.Key, fst kv.Value + snd state)) (0.,0.)
        |> Seq.find (fun (x,y) -> y >= target)
    x

/// Creates probability mass function for Peptide array and keeps peptide information for each bin   
let createPeptideHis bandwidth (peptideArr:Peptide []) =            
    let halfBw = bandwidth / 2.0       
    let tmp = 
        peptideArr
        |> Seq.groupBy (fun x -> floor (x.Mass / bandwidth)) 
        |> Seq.map (fun (k,values) -> 
            let count = (Seq.length(values)) |> float                                        
            if k < 0. then
                ((k  * bandwidth) + halfBw, count, values)   
            else
                ((k + 1.) * bandwidth) - halfBw, count, values)
        |> Seq.sortBy (fun (x,y,z) -> x)
    printfn "%A" tmp
    let area =
        tmp
        |> Seq.fold (fun acc (x,y,z) -> acc + y) 0.
    printfn "%A" area
    tmp    
    |> Seq.map (fun (a,b,peps) -> (a,(b/ area, peps)))
    |> Map.ofSeq

/// Samples "n" from empirical map "hisMap"
/// "hisMap" should be create via "createPeptideHis"
let sampleList n hisMap =
    let rnd = System.Random()
    [0 .. n]
    |> List.map (fun _ -> random hisMap)
    |> List.map (fun x -> Map.find x hisMap)
    |> List.map (
        fun (mass,peps) -> 
            let rndInd = rnd.Next(0,Seq.length peps)
            Seq.item rndInd peps
    )

let digestPeptideMasses lowerMassBound upperMassBound =
    digestedProteins
    |> Array.choose (fun peptide ->
        let mass = BioSeq.toMonoisotopicMassWith (BioItem.monoisoMass ModificationInfo.Table.H2O) peptide.PepSequence
        let pepSeq = peptide.PepSequence |> BioList.toString
        let hydro = 
            let aalist =
                pepSeq
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
        let newPep() = Peptide.create pepSeq hydro mass
        if mass < upperMassBound && mass > lowerMassBound then Some (newPep()) else None
    )

let chargePeptidesBy charge peptides =
    peptides 
    |> Array.map (fun x -> {x with Mass = Mass.toMZ x.Mass charge})

let chargedPeptideMasses lowerMassBound upperMassBound charges =
    let digestedPeptides = digestPeptideMasses lowerMassBound upperMassBound
    let chargedPeptides = charges |> Array.collect (fun x -> digestedPeptides |> chargePeptidesBy x)
    chargedPeptides

let peptidesWithHydroIndAround hydroInd inp =
    inp
    |> Array.filter (fun x -> x.HydrophobicityInd > hydroInd-0.05 && x.HydrophobicityInd < hydroInd+0.05)

let massVisualization07 inp =
    Chart.Histogram (inp |> Array.map (fun x -> x.Mass),Name="Peptides with Hydrophobicity Index around 0.7", Opacity=0.8)
    //|> Chart.withX_AxisStyle (title = "Hydro Ind")
    //|> Chart.withY_AxisStyle "Count"
    //|> Chart.Show

let massVisualization2 inp =
    Chart.Histogram (inp |> Array.map (fun x -> x.Mass),Name="Peptides with Hydrophobicity Index around 2.", Opacity=0.8)
    //|> Chart.withX_AxisStyle (title = "Hydro Ind")
    //|> Chart.withY_AxisStyle "Count"
    //|> Chart.Show

let massVisualization4 inp =
    Chart.Histogram (inp |> Array.map (fun x -> x.Mass),Name="Peptides with Hydrophobicity Index around 4.", Opacity=0.8)
    //|> Chart.withX_AxisStyle (title = "Hydro Ind")
    //|> Chart.withY_AxisStyle "Count"
    //|> Chart.Show

let massVisualization6 inp =
    Chart.Histogram (inp |> Array.map (fun x -> x.Mass),Name="Peptides with Hydrophobicity Index around 6.", Opacity=0.8)
    //|> Chart.withX_AxisStyle (title = "Hydro Ind")
    //|> Chart.withY_AxisStyle "Count"
    //|> Chart.Show

let massVisualization inp =
    Chart.Histogram (inp |> Array.map (fun x -> x.Mass), Name="All Peptides", Opacity=0.2)


let createHydrophobicityChart lowerMassBound upperMassBound charges =
    let masses = chargedPeptideMasses lowerMassBound upperMassBound charges
    [
        massVisualization masses
        massVisualization07 (peptidesWithHydroIndAround 0.7 masses)
        massVisualization2 (peptidesWithHydroIndAround 2. masses)
        massVisualization4 (peptidesWithHydroIndAround 4. masses)
        massVisualization6 (peptidesWithHydroIndAround 6. masses)
    ]
    |> Chart.Combine
    //|> Chart.Stack(2)
    |> Chart.withSize (900.,600.)
    |> Chart.withTitle (sprintf "Chlamydomonas reinhardtii peptides between %A m/z and %A m/z for charge %A" lowerMassBound upperMassBound charges)
    |> Chart.withX_AxisStyle ("Hydrophobicity Index")
    |> Chart.withY_AxisStyle ("Count")

createHydrophobicityChart 100. 3000. [|1.|] |> Chart.Show

let masses = chargedPeptideMasses 100. 3000. [|1.|]

let empHis = createPeptideHis 0.5 (peptidesWithHydroIndAround 6. masses)

let my6List =
    sampleList 10 empHis

//let testing =
//    my6List
//    |> List.groupBy (fun x -> x.PeptideSequence)

//testing.Length

my6List
|> List.map (fun x -> x.Mass)
|> Chart.Histogram 
|> Chart.withX_AxisStyle (title = "Mass [Da]")
|> Chart.withY_AxisStyle "Count"
|> Chart.Show

