#r "nuget: Deedle"
#r "nuget: FSharpAux"

open Deedle
open FSharpAux

let myPathSDS = @"C:\Repos\BIO-BTE-06-L-7\docs\downloads\Quantifications_sds_annotated.txt"
let myPathBN = @"C:\Repos\BIO-BTE-06-L-7\docs\downloads\Quantifications_bn_annotated.txt"

let framSDS = Frame.ReadCsv (myPathSDS, separators = "\t")
let framBN = Frame.ReadCsv (myPathBN, separators = "\t")

framSDS.ColumnKeys |> Array.ofSeq
framBN.ColumnKeys |> Array.ofSeq

let framSDS' = framSDS |> Frame.mapColKeys (fun cK -> cK |> String.replace "2.5" "2_5")
let framBN' = framBN |> Frame.mapColKeys (fun cK -> cK |> String.replace "2.5" "2_5")

framSDS'.ColumnKeys |> Array.ofSeq
framBN'.ColumnKeys |> Array.ofSeq

framSDS'.SaveCsv (myPathSDS |> String.replace ".txt" "_replaced.txt", separator = '\t', includeRowKeys = false)
framBN'.SaveCsv (myPathBN |> String.replace ".txt" "_replaced.txt", separator = '\t', includeRowKeys = false)