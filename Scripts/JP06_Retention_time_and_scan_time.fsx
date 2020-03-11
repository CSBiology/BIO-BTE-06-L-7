
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

let random (pmf:Map<float,(float*seq<Peptide>)>) = 
    if pmf.Count <= 0 then raise (System.Exception("Pmf contains no values") )  
    let target = FSharp.Stats.Random.rndgen.NextFloat()
    let x,y =
        pmf
        |> Seq.scan (fun state kv -> (kv.Key, fst kv.Value + snd state)) (0.,0.)
        |> Seq.find (fun (x,y) -> y >= target)
    x

/// Creates probability mass function    
let create bandwidth (peptideArr:Peptide []) =            
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

let sampleList hisMap =
    let rnd = System.Random()
    [0 .. 10000]
    |> List.map (fun _ -> random hisMap)
    |> List.map (fun x -> Map.find x hisMap)
    |> List.map (
        fun (mass,peps) -> 
            let rndInd = rnd.Next(0,Seq.length peps)
            Seq.item rndInd peps
    )

let digestedPeptideMasses =
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
        if mass < 3000. then Some (newPep()) else None
    )

let peptidesWithHydroIndAround hydroInd =
    digestedPeptideMasses
    |> Array.filter (fun x -> x.HydrophobicityInd > hydroInd-0.05 && x.HydrophobicityInd < hydroInd+0.05)

let massVisualization07 =
    Chart.Histogram (peptidesWithHydroIndAround 0.7 |> Array.map (fun x -> x.Mass),Name="Peptides with Hydrophobicity Index around 0.7")
    //|> Chart.withX_AxisStyle (title = "Hydro Ind")
    //|> Chart.withY_AxisStyle "Count"
    //|> Chart.Show

let massVisualization2 =
    Chart.Histogram (peptidesWithHydroIndAround 2. |> Array.map (fun x -> x.Mass),Name="Peptides with Hydrophobicity Index around 2.")
    //|> Chart.withX_AxisStyle (title = "Hydro Ind")
    //|> Chart.withY_AxisStyle "Count"
    //|> Chart.Show

let massVisualization4 =
    Chart.Histogram (peptidesWithHydroIndAround 4. |> Array.map (fun x -> x.Mass),Name="Peptides with Hydrophobicity Index around 4.")
    //|> Chart.withX_AxisStyle (title = "Hydro Ind")
    //|> Chart.withY_AxisStyle "Count"
    //|> Chart.Show

let massVisualization6 =
    Chart.Histogram (peptidesWithHydroIndAround 6. |> Array.map (fun x -> x.Mass),Name="Peptides with Hydrophobicity Index around 6.")
    //|> Chart.withX_AxisStyle (title = "Hydro Ind")
    //|> Chart.withY_AxisStyle "Count"
    //|> Chart.Show

let massVisualization =
    Chart.Histogram (digestedPeptideMasses |> Array.map (fun x -> x.Mass), Name="All Peptides", Opacity=0.2)

[
    massVisualization
    massVisualization07
    massVisualization2
    massVisualization4
    massVisualization6
]
|> Chart.Combine
//|> Chart.Stack(2)
|> Chart.withSize (900.,600.)
|> Chart.withX_AxisStyle ("Hydro Ind")
|> Chart.withY_AxisStyle ("Count")
|> Chart.Show


let empHis = create 0.5 (peptidesWithHydroIndAround 6.)

let my60List =
    sampleList empHis

let testing =
    my60List
    |> List.groupBy (fun x -> x.PeptideSequence)

testing.Length

my60List
|> List.map (fun x -> x.Mass)
|> Chart.Histogram 
|> Chart.withX_AxisStyle (title = "Mass [Da]")
|> Chart.withY_AxisStyle "Count"
|> Chart.Show



/// Teil 1

///Predicts an isotopic distribution of the given formula at the given charge, normalized by the sum of probabilities, using the MIDAs algorithm
let generateIsotopicDistribution (charge:int) (f:Formula.Formula) =
    IsotopicDistribution.MIDA.ofFormula 
        IsotopicDistribution.MIDA.normalizeByMaxProb
        0.01
        0.005
        charge
        f

///returns a function that replaces the nitrogen atoms in a formula with a nitrogen with the given probability of being the 15N isotope
let initlabelN15Partial n15Prob =
    ///Diisotopic representation of nitrogen with abundancy of N14 and N15 swapped
    let n14Prob = 1. - n15Prob
    let N15 = Di (createDi "N15" (Isotopes.Table.N15,n15Prob) (Isotopes.Table.N14,n14Prob) )
    fun f -> Formula.lableElement f Elements.Table.N N15

let toFormula bioseq =  
    bioseq
    |> BioSeq.toFormula
    |> Formula.add Formula.Table.H2O

let peptide_short = 
    "PEPTIDES" 
    |> BioSeq.ofAminoAcidString
    |> toFormula

let peptide_long  = 
    "PEPTIDEPEPTIDEPEPTIDEPEPTIDES" 
    |> BioSeq.ofAminoAcidString
    |> toFormula

let isoPattern_peptide_short = 
    generateIsotopicDistribution 1 peptide_short

let isoPattern_peptide_long = 
    generateIsotopicDistribution 1 peptide_long

[
Chart.Point isoPattern_peptide_short 
|> Chart.withTraceName "shortPep"
Chart.Point isoPattern_peptide_long 
|> Chart.withTraceName "longPep"
]
|> Chart.Combine
|> Chart.Show

/// Teil 2

let N15_peptide_short = 
    "PEPTIDES" 
    |> BioSeq.ofAminoAcidString
    |> toFormula
    |> initlabelN15Partial 0.999999

let N15_peptide_long  = 
    "PEPTIDEPEPTIDEPEPTIDEPEPTIDES" 
    |> BioSeq.ofAminoAcidString
    |> toFormula
    |> initlabelN15Partial 0.999999

let N15_isoPattern_peptide_short = 
    generateIsotopicDistribution 1 N15_peptide_short

let N15_isoPattern_peptid_long = 
    generateIsotopicDistribution 1 N15_peptide_long

[
Chart.Point isoPattern_peptide_short 
|> Chart.withTraceName "shortPep"
Chart.Point isoPattern_peptide_long 
|> Chart.withTraceName "longPep"
Chart.Point N15_isoPattern_peptide_short 
|> Chart.withTraceName "N15shortPep"
Chart.Point N15_isoPattern_peptid_long 
|> Chart.withTraceName "N15longPep"

]
|> Chart.Combine
|> Chart.Show


