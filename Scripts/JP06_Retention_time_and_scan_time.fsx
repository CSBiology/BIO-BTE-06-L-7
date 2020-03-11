
#load @"../IfSharp/Paket.Generated.Refs.fsx"

open BioFSharp
open BioFSharp.IO
open FSharp.Stats
open FSharp.Plotly

//[JP4] Digestion and mass calculation
//The most widely applied method for protein digestion involves the use of enzymes. Many proteases are available for this purpose,
//each having their own characteristics in terms of specificity, efficiency and optimum digestion conditions. Trypsin is most widely applied
//in bottom-up proteomics and and has a very high degree of specificity, cleaving the peptide bonds C-terminal to the basic residues Lys and Arg,
//except when followed by Pro 23. In general, Lys and Arg are relatively abundant amino acids and are usually well distributed throughout a
//protein 24. This leads to tryptic peptides with an average length of ?14 amino acids that carry at least two positive charges, which is ideally
//suited for CID-MS analysis23.
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
        let hydro = 
        if mass < 3000. then Some mass else None
    )

let massVisualization =
    Chart.Histogram digestedPeptideMasses
    |> Chart.withX_AxisStyle (title = "Mass [Da]")
    |> Chart.withY_AxisStyle "Count"
    //|> Chart.Show

/// Creates probability mass function    
let create bandwidth data =            
    let halfBw = bandwidth / 2.0       
    let tmp = 
        data
        |> Seq.groupBy (fun x -> floor (x / bandwidth)) 
        |> Seq.map (fun (k,values) -> 
            let count = (Seq.length(values)) |> float                                        
            if k < 0. then
                ((k  * bandwidth) + halfBw, count)   
            else
                ((k + 1.) * bandwidth) - halfBw, count)
        |> Seq.sortBy fst
    printfn "%A" tmp
    let area =
        tmp
        |> Seq.fold (fun acc (x,y) ->                         
                        acc + y) 0.
    printfn "%A" area
    tmp    
    |> Seq.map (fun (a,b) -> (a,b / area))
    |> Map.ofSeq

let empHis = create 0.5 digestedPeptideMasses

let sampleList =
    [0 .. 10000]
    |> List.map (fun _ -> FSharp.Stats.Distributions.Empirical.random empHis)

sampleList
|> Chart.Histogram 
|> Chart.withX_AxisStyle (title = "Mass [Da]")
|> Chart.withY_AxisStyle "Count"
|> Chart.Show

