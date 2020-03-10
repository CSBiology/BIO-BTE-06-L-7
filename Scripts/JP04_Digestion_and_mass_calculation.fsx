
#load @"../IfSharp/Paket.Generated.Refs.fsx"

open BioFSharp
open BioFSharp.IO
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

let aminoAcidDistribution =
    sequences
    |> Array.collect (fun fastAItem -> fastAItem.Sequence)
    |> Array.countBy id

let aaDistributionHis =
    aminoAcidDistribution
    |> Array.map (fun (name,count) -> name.ToString(), count)
    |> Array.sortByDescending snd
    |> Chart.Column
    |> Chart.withY_AxisStyle "Count"
    |> Chart.withTitle "Amino Acid composition of the <i>Chlamydomonas reinhardtii</i> proteome"
    |> Chart.Show

let digestedProteins =
    sequences
    |> Array.mapi (fun i fastAItem ->
        fastAItem.Header,
        Digestion.BioArray.digest Digestion.Table.Trypsin i fastAItem.Sequence
        |> Digestion.BioArray.concernMissCleavages 0 0
    )

let digestedPeptideMasses =
    let massFunction:IBioItem -> float = BioItem.initMonoisoMassWithMemP
    digestedProteins
    |> Array.collect (fun (header, digestedProtein) ->
        digestedProtein
        |> Array.map (fun peptide ->
            peptide.PepSequence
            |> List.fold (fun acc aa -> 
                match aa with
                | AminoAcids.AminoAcid.Mod(a,_) ->
                    acc + massFunction a
                | a -> 
                    acc + massFunction a
             ) (massFunction ModificationInfo.Table.H2O) 
        )
    )

let massVisualization =
    Chart.Histogram digestedPeptideMasses
    |> Chart.withX_AxisStyle "Mass"
    |> Chart.withY_AxisStyle "Count"
    |> Chart.Show