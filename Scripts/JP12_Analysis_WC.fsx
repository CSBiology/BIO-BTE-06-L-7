

#load @"../IfSharp/References.fsx"
#load @"../IfSharp/Paket.Generated.Refs.fsx"
#load @"../AuxFsx/ProtAux.fsx"
#load @"../AuxFsx/DeedleAux.fsx"

open BioFSharp
open Deedle
open FSharpAux
open FSharp.Stats
open FSharp.Plotly

type Color = {
    H : int
    S : int
    L : int
    }

let createHSL (color:Color) =
    (sprintf "hsl(%i," color.H) + string color.S + @"%," + string color.L + @"%)"

let mainColorPeachFF6F61 = {
    H = 5
    S = 100
    L = 69
}

let mainColorBlue92a8d1 = {
    H = 219
    S = 41
    L = 70
}

let mainColorGreen88b04b = {
    H = 84
    S = 40
    L = 49
}

let mainColorYellowEFC050 = {
    H = 42
    S = 83
    L = 63
}

let soos =  {
    H = 166
    S = 100
    L = 30
}

let mainColors = [mainColorPeachFF6F61;mainColorBlue92a8d1;mainColorGreen88b04b;mainColorYellowEFC050;soos] |> List.map createHSL |> List.rev

let createRelatedColors (color:Color) nOfColors =
    let wheelchange = -20
    let changeTempColor wheelChange tempColor =
        if tempColor.H + wheelChange < 0 
        then 
            360 + (color.H + wheelChange)
        elif tempColor.H + wheelChange > 360
        then 
            (tempColor.H + wheelChange) - 360
        else tempColor.H + wheelChange
    let rec loopVariant results iteration =
        if iteration = 1 
        then 
            loopVariant ({color with H = changeTempColor wheelchange color}::results) (iteration + 1)
        elif iteration < nOfColors 
        then 
            let mostRecentVariant =
                results.Head
            loopVariant ({mostRecentVariant with H = changeTempColor wheelchange mostRecentVariant}::results) (iteration + 1)
        else results
    loopVariant [color] 1
    |> (List.rev >> Array.ofList)

createRelatedColors mainColorGreen88b04b 3
|> Array.map createHSL

let xAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Zeroline=false,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside, Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))
let yAxis showGrid title titleSize tickSize (range:float*float)= Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside,Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize),Range=StyleParam.Range.MinMax range)

let config = Config.init(ToImageButtonOptions = ToImageButtonOptions.init(Format = StyleParam.ImageFormat.SVG, Filename = "praktikumsplot.svg"), EditableAnnotations = [AnnotationEditOptions.LegendPosition])


let isBad a =
    nan.Equals(a) || infinity.Equals(a) || (-infinity).Equals(a)


//let qConCatSeq =
//    "MASMTGGQQMGRDPAGAKLGGNEQVTRADLNVPLDKTFNDALADAKLSELLGKPVTKAVSLVLPSLKVLITAPAKALQNTVLKVMFEGILLKSVVSIPHGPSIIAARVPLFIGSKTLLYGGIYGYPGDAKIYSFNEGNYGLWDDSVKLTNITGRLLFEALKFLAIDAINKVSTLIGYGSPNKNPDFFNRFIESQVAKGVNPWIEVDGGVTPENAYKSDIIVSPSILSADFSRIYLDISDDIKVAELLDFKGHSLESIKSLFGESNEVVAKLVDELNAGTIPRLANLPEVKLQNIVGVPTSIRTQLSQDELKSGQPAVDLNKASGQPAVDLNKAEAALLVRSNSTPLGSRGILASDESNATTGKALQSSTLKVSAADVARALQASVLKVTEAAALASGRNLALELVRSAEGLDASASLRAAWSHHHHHHHKAWASWASKLAAALEHHHHHH"
//    //"MASMTGGQQMGRDPSRSALPSNWKSVLPANWRDTDILAAFREVTLGFVDLMRFLFVAEAIYKLTYYTPDYVVRAYVSNESAIRLVAFDNQKYWTMWKAFPDAYVRVPLILGIWGGKIGQQLVNARSLVDEQENVKLGADSGALEFVPKDDYLNAPGETYSVKTPLANLVYWKALYGFDFLLSSKTNFGIGHRLSIFETGIKTAPAFVDLDTRIPAGPDLIVKNILVVGPVPGKIVAITALSEKYPIYFGGNRVLNTWADIINREWELSFRNTWADIINRLIFQYASFNNSRTALPADWRLVFPEEVLPRNILLNEGIRTWFDDADDWLRAAHHHHHHHKLAAALEHHHHHH"
//    |> BioArray.ofAminoAcidString
//    |> Digestion.BioSeq.digest Digestion.Table.Trypsin
//    |> Array.ofSeq
//    |> Array.map (fun x -> BioList.toString x)

//qConCatSeq.Length

let peptideProtMapping =
    [
    "LCI5"  =>  "SALPSNWK"
    "LCI5"  =>  "SVLPANWR"
    "rbcL"  =>  "DTDILAAFR"
    "rbcL"  =>  "EVTLGFVDLMR"
    "rbcL"  =>  "FLFVAEAIYK"
    "rbcL"  =>  "LTYYTPDYVVR"
    "RBCS2" =>  "AYVSNESAIR"
    "RBCS2" =>  "LVAFDNQK"
    "RBCS2" =>  "YWTMWK"
    "RBCS2" =>  "AFPDAYVR"
    "RCA1"  =>  "VPLILGIWGGK"
    "RCA1"  =>  "IGQQLVNAR"
    "RCA1"  =>  "SLVDEQENVK"
    "PCY1"  =>  "LGADSGALEFVPK"
    "PCY1"  =>  "DDYLNAPGETYSVK"
    "psaB"  =>  "TPLANLVYWK"
    "psaB"  =>  "ALYGFDFLLSSK"
    "psaB"  =>  "TNFGIGHR"
    "atpB"  =>  "LSIFETGIK"
    "atpB"  =>  "TAPAFVDLDTR"
    "petA"  =>  "IPAGPDLIVK"
    "petA"  =>  "NILVVGPVPGK"
    "petA"  =>  "IVAITALSEK"
    "petA"  =>  "YPIYFGGNR"
    "FNR1"  =>  "LYSIASSR"
    "FNR1"  =>  "LDYALSR"
    "D1"    =>  "VLNTWADIINR"
    "D1"    =>  "EWELSFR"
    "D1"    =>  "NTWADIINR"
    "D1"    =>  "LIFQYASFNNSR"
    "LCI5"  =>  "TALPADWR"
    "psbD"  =>  "LVFPEEVLPR"
    "psbD"  =>  "NILLNEGIR"
    "psbD"  =>  "TWFDDADDWLR"
    //CBC
    "PGK"   =>  "ADLNVPLDK"
    "PGK"   =>  "TFNDALADAK"
    "PGK"   =>  "LSELLGKPVTK"
    "Gap3"  =>  "AVSLVLPSLK"
    "Gap3"  =>  "VLITAPAK"
    "FBA3"  =>  "ALQNTVLK"
    "FBA3"  =>  "VMFEGILLK"
    "FBA3"  =>  "SVVSIPHGPSIIAAR"
    "FBP1"  =>  "VPLFIGSK"
    "FBP1"  =>  "TLLYGGIYGYPGDAK"
    "FBP1"  =>  "IYSFNEGNYGLWDDSVK"
    "SBP"   =>  "LTNITGR"
    "SBP"   =>  "LLFEALK"
    "TRK1"  =>  "FLAIDAINK"
    "TRK1"  =>  "VSTLIGYGSPNK"
    "TRK1"  =>  "NPDFFNR"
    "RPE1"  =>  "FIESQVAK"
    "RPE1"  =>  "GVNPWIEVDGGVTPENAYK"
    "RPE1"  =>  "SDIIVSPSILSADFSR"
    "PRK1"  =>  "IYLDISDDIK"
    "PRK1"  =>  "VAELLDFK"
    "PRK1"  =>  "GHSLESIK"
    "TPI1"  =>  "SLFGESNEVVAK"
    "TPI1"  =>  "LVDELNAGTIPR"
    "RPI1"  =>  "LANLPEVK"
    "RPI1"  =>  "LQNIVGVPTSIR"
    "RPI1"  =>  "TQLSQDELK"
    "DP12"  =>  "SGQPAVDLNK"
    "DP12"  =>  "ASGQPAVDLNK"
    "RMT1"  =>  "AEAALLVR"
    "RMT1"  =>  "SNSTPLGSR"
    "FBA1"  =>  "GILASDESNATTGK"
    "FBA1"  =>  "ALQSSTLK"
    "FBA2"  =>  "VSAADVAR"
    "FBA2"  =>  "ALQASVLK"
    "Cre07.g338451" =>  "VTEAAALASGR"
    "FBP1"  =>  "NLALELVR"
    "CalSciex"  =>  "SAEGLDASASLR"
    ]
    |> List.map (fun (x,y) -> y,x)
    |> Map.ofList

peptideProtMapping.Count

//let peptideMapping =
//    [
//        "DTDILAAFR"      => "rbcL"
//        "EVTLGFVDLMR"    => "rbcL"
//        "FLFVAEAIYK"     => "rbcL"
//        "LTYYTPDYVVR"    => "rbcL"
//        "AYVSNESAIR"     => "RBCS2"
//        "LVAFDNQK"       => "RBCS2"
//        "YWTMWK"         => "RBCS2"
//        "AFPDAYVR"       => "RBCS2"
//        "VPLILGIWGGK"    => "RCA1"
//        "IGQQLVNAR"      => "RCA1"
//        "SLVDEQENVK"     => "RCA1"
//    ] |> Map.ofList

//FileName	Experiment	Content	ProteinAmount[ug]	Replicate
let sdsSampleNameMapping = 
    [
    //80,40,20 ug geladenes protein
    "Data02018VP_G3_4_SDS_IGD1"  => ("SDS_IGD",("RBCL", ("80","3")))
    "Data02018VP_G3_4_SDS_IGD2"  => ("SDS_IGD",("RBCL", ("40","3")))
    "Data02018VP_G3_4_SDS_IGD3"  => ("SDS_IGD",("RBCL", ("20","3")))
    "Data02018VP_G3_4_SDS_IGD4"  => ("SDS_IGD",("RBCS", ("80","3")))
    "Data02018VP_G3_4_SDS_IGD5"  => ("SDS_IGD",("RBCS", ("40","3")))
    "Data02018VP_G3_4_SDS_IGD6"  => ("SDS_IGD",("RBCS", ("20","3")))
    "Data02018VP_G3_4_SDS_IGD7"  => ("SDS_IGD",("37kDa",("80","3")))
    "Data02018VP_G3_4_SDS_IGD8"  => ("SDS_IGD",("RBCL", ("20","4")))
    "Data02018VP_G3_4_SDS_IGD9"  => ("SDS_IGD",("RBCL", ("10","4")))
    "Data02018VP_G3_4_SDS_IGD10" => ("SDS_IGD",("RBCL", ("5","4")))
    "Data02018VP_G3_4_SDS_IGD11" => ("SDS_IGD",("RBCS", ("20","4")))
    "Data02018VP_G3_4_SDS_IGD12" => ("SDS_IGD",("RBCS", ("10","4")))
    "Data02018VP_G3_4_SDS_IGD13" => ("SDS_IGD",("RBCS", ("5","4")))
    "Data02018VP_G3_4_SDS_IGD14" => ("SDS_IGD",("37kDa",("20","4")))
    
    ]
    |> Map.ofList


    
let bnNameMapping = 
    [
    //Random_Band_# nummer entspricht der nummerierung auf dem BN Bild
    "Data02018VP_G3-G4_BN_IGD1" =>  ("BN_IGD",("NF",("Random_Band_1","1")))
    "Data02018VP_G3-G4_BN_IGD2" =>  ("BN_IGD",("NF",("Random_Band_2","1")))
    "Data02018VP_G3-G4_BN_IGD3" =>  ("BN_IGD",("NF",("Random_Band_3","1")))
    "Data02018VP_G3-G4_BN_IGD4" =>  ("BN_IGD",("NF",("Random_Band_4","1")))
    "Data02018VP_G3-G4_BN_IGD5" =>  ("BN_IGD",("NF",("RBC_Band","1"))     )
    "Data02018VP_G3-G4_BN_IGD6" =>  ("BN_IGD",("NF",("RBC_Band","2"))     )
    
    ]
    |> Map.ofList

//let wholeCellNameMapping = 
//    [
//    "Data20180126_InSolutionDigestWholeCell_Gr3+41" =>  ("WholeCell_ISD",("2:1","3") )
//    "Data20180126_InSolutionDigestWholeCell_Gr3+42" =>  ("WholeCell_ISD",("1:1","3") )
//    "Data20180126_InSolutionDigestWholeCell_Gr3+43" =>  ("WholeCell_ISD",("1:5","3") )
//    "Data20180126_InSolutionDigestWholeCell_Gr3+44" =>  ("WholeCell_ISD",("1:10","3"))
//    "Data20180126_InSolutionDigestWholeCell_Gr3+45" =>  ("WholeCell_ISD",("2:1","4") )
//    "Data20180126_InSolutionDigestWholeCell_Gr3+46" =>  ("WholeCell_ISD",("1:1","4") )
//    "Data20180126_InSolutionDigestWholeCell_Gr3+47" =>  ("WholeCell_ISD",("1:5","4") )
//    "Data20180126_InSolutionDigestWholeCell_Gr3+48" =>  ("WholeCell_ISD",("1:10","4"))
//    ]
//    |> Map.ofList

let labelEfficiencyNameMapping = 
    [
    "DataLabelingEfficiency1"                           => ("LabelEfficiency",("15N IRT + 14N IRT","3"))
    "DataLabelingEfficiency2"                           => ("LabelEfficiency",("15N Q + 15N IRT"  ,"3"))
    "DataLabelingEfficiency3"                           => ("LabelEfficiency",("14N Q + 15N IRT"  ,"3"))
    "Data00VP2018_Gr4_LabelEfficiency4"                 => ("LabelEfficiency",("15N Q + 14N Q + 13C Q","4"))
    "Data00VP2018_Gr4_LabelEfficiency5"                 => ("LabelEfficiency",("15N Q","4"))
    "Data00VP2018_Gr4_LabelEfficiency6"                 => ("LabelEfficiency",("15N Q + 14N Q","4"))
    ]
    |> Map.ofList
    
let readQConcatResultFrame p : Frame<string*(bool*int),string>=
    let schemaFrame =
        Frame.ReadCsv(path = p,separators="\t")
    let schema =
        schemaFrame.ColumnKeys
        |> Seq.filter (fun x -> not (x = "StringSequence" || x = "GlobalMod" || x = "Charge"))
        |> Seq.map (sprintf "%s=float")
        |> Seq.append ["StringSequence=string";"GlobalMod=bool";"Charge=int"]
        |> String.concat ","
    Frame.ReadCsv(path = p,schema=schema,separators="\t")
    |> Frame.indexRowsUsing (fun os -> (os.GetAs<string>("StringSequence"),((os.GetAs<bool>("GlobalMod"),(os.GetAs<int>("Charge"))))))
    |> Frame.dropCol "StringSequence"
    |> Frame.dropCol "GlobalMod"
    |> Frame.dropCol "Charge"
    |> Frame.sortRowsByKey

let source = __SOURCE_DIRECTORY__

//JP12_WC_02
let wholeCellNameMapping = 
    [
    // filename(found in metadata file) => ("WholeCell_ISD", (spiked in peptide concentration, C. reinhardtii strain) ) 
    "20200206MS169msFSSTqp001"      =>  ("WholeCell_ISD",("1:1","CW15") )
    "20200206MS169msFSSTqp002"      =>  ("WholeCell_ISD",("1:5","CW15") )
    "20200206MS169msFSSTqp003"      =>  ("WholeCell_ISD",("1:25","CW15") )
    "20200206MS169msFSSTqp004"      =>  ("WholeCell_ISD",("1:125","CW15"))
    //"20200206MS169msFSSTqp005"      =>  ("WholeCell_ISD",("1:625","CW15") )
    //"20200206MS169msFSSTqp006"      =>  ("WholeCell_ISD",("1:3125","CW15") )
    "20200206MS169msFSSTqp007"      =>  ("WholeCell_ISD",("1:1","UVM4") )
    "20200206MS169msFSSTqp008"      =>  ("WholeCell_ISD",("1:5","UVM4") )
    "20200206MS169msFSSTqp009"      =>  ("WholeCell_ISD",("1:25","UVM4") )
    "20200206MS169msFSSTqp010"      =>  ("WholeCell_ISD",("1:125","UVM4"))
    //"20200206MS169msFSSTqp011"      =>  ("WholeCell_ISD",("1:625","UVM4") )
    //"20200206MS169msFSSTqp012"      =>  ("WholeCell_ISD",("1:3125","UVM4") )
    "20200206MS169msFSSTqp013"      =>  ("WholeCell_ISD",("1:1","4A") )
    "20200206MS169msFSSTqp014"      =>  ("WholeCell_ISD",("1:5","4A") )
    "20200206MS169msFSSTqp015"      =>  ("WholeCell_ISD",("1:25","4A") )
    "20200206MS169msFSSTqp016"      =>  ("WholeCell_ISD",("1:125","4A"))
    //"20200206MS169msFSSTqp017"      =>  ("WholeCell_ISD",("1:625","4A") )
    //"20200206MS169msFSSTqp018"      =>  ("WholeCell_ISD",("1:3125","4A") )
    ]
    |> Map.ofList

//================================================================================
//====================== 2. WholeCell ISD ========================================
//================================================================================
    
    //JP12_WC_01
let wholeCellResults : Frame<string*(string*(string*string)),string*string> = 
    readQConcatResultFrame @"C:\Users\Kevin\Desktop\QuantifiedPeptides.txt"
    |> Frame.filterCols 
        (fun ck cs ->
            let newCK = Map.tryFindKey (fun key t -> key = (ck.Split('_').[1 ..] |> String.concat "_")) wholeCellNameMapping
            newCK.IsSome
        )
    |> Frame.mapColKeys 
        (fun (ck:string) -> 
            //printfn "%s" ck
            let newCK = Map.find (ck.Split('_').[1 ..] |> String.concat "_") wholeCellNameMapping
            ck.Split('_').[0] , newCK
        )
    |> Frame.sortColsByKey
    |> Frame.filterCols (fun ck _ -> (fst ck).Contains("Quant"))
    |> Frame.applyLevel (fun (sequence,(gmod,charge)) -> sequence) Stats.mean
    |> Frame.transpose
    // JP12_WC_04
    |> Frame.mapColKeys
        (fun ck ->
            match Map.tryFind ck peptideProtMapping with
            |Some prot  -> prot,ck
            |None       -> "NotFound",ck
        )  
    //|> Frame.transpose
    //|> fun f ->
    //    let keysToCorrect =
    //        f
    //        |> Frame.filterCols (fun (ck,_) _ -> ck = "N15Quant")
    //        |> fun x -> x.ColumnKeys
    //        |> Array.ofSeq
    //    let correctedN15 = 
    //        keysToCorrect
    //        |> Array.map 
    //            (fun (k,(exp,(ratio,rep))) ->
    //                ("N15QuantCorrected",(exp,(ratio,rep))) => (f |> Frame.getCol (k,(exp,(ratio,rep)))) * labelEfficiencyCorrectionFactorsOnly
    //            )
    //        |> frame
    //    f |> Frame.merge correctedN15
    //|> Frame.transpose

let forLinearity = 
    wholeCellResults
    |> Frame.mapCols (
        fun _ os -> 
            os.As<float>() 
            |> Series.mapValues (fun (x:float) -> if x > 2000000. || x < 1. then nan else x)
    )
    //|> Frame.filterCols (fun ck _ -> (fst ck).ToLower().Contains("rbc"))
    // JP12_WC_05
    |> Frame.mapRowKeys 
        (fun (q,(n,(ratio,strain))) ->  
            let ratioSplit = ratio.Split(':')
            printfn "%A" ratioSplit
            let ratio' = (float ratioSplit.[0]) / (float ratioSplit.[1])
            (q,(n,(ratio',strain)))
    
        )
    // JP12_WC_06
    // Bad Peptides according to Hammel et al
    |> Frame.filterCols (fun ck _ -> not ((snd ck) = "EVTLGFVDLMR") && not ((snd ck) = "AFPDAYVR"))
    |> Frame.filterRows (fun (q,(n,ratio)) _ -> not (q.Contains("Minus")))
    |> Frame.sortRowsByKey
    |> Frame.sortColsByKey
    |> Frame.filterCols (fun ck cs -> fst ck = "rbcL" (*|| fst ck = "RBCS2"*))


let n14Lin =
    forLinearity
    |> Frame.filterRows (fun rk _ -> not ((fst rk).Contains("N15Quant")))
    |> Frame.transpose
    |> Frame.toArray2D
    |> Array2D.toJaggedArray
    // Group 3 had 4x less than in protocol
    //|> JaggedArray.mapi (fun i x -> if List.contains i [ 0 .. 2 ..7 ] then x/4. else x)


let n15Lin =
    forLinearity
    |> Frame.filterRows (fun rk _ -> not ((fst rk).Contains("N14")) && not ((fst rk).Contains("Corrected")))
    |> Frame.transpose
    |> Frame.toArray2D
    |> Array2D.toJaggedArray

// Rows = 37, outer array
// Cols = 12, inner array


/// Return full protein and mapping peptide plot for all concentrations for whole cell ISD
let plotPeptideISD (*(proteinNames : string [])*) =

    Array.map2 (Array.zip) n14Lin n15Lin
    |> fun x -> x
    // JP12_WC_07
    |> JaggedArray.map (fun (n14,n15) -> if (isBad n14 || isBad n15) then 0. else n14/n15)
    // dilutions times the number of strains 3 x 125, 3 x 25, 3 x 5 ...
    // zip the following values to the columns
    |> Array.map (fun x -> Array.zip [|(*3125.;3125.;3125.;625.;625.;625.;*)125.;125.;125.;25.;25.;25.;5.;5.;5.;1.;1.;1.|] x)
    |> Array.map 
        (fun (values) -> 
            //// this is meant to average found values through all strains. 
            //// Leftover code from previous practical course iteration, where 
            //// we did not have multiple strains, but combined groups as technical replicates
            values 
            // chunk by the number of strains
            |> Array.chunkBySize 3//2 
            |> Array.map (fun reps -> 
                fst reps.[0], 
                reps |> Array.map snd |> Seq.mean
            ),
            // 4A
            [|for i in 0 .. 3 ..values.Length-1 do yield values.[i]|],
            // CW15
            [|for i in 1 .. 3 ..values.Length-1 do yield values.[i]|],
            // UVM4
            [|for i in 2 .. 3 ..values.Length-1 do yield values.[i]|]
        )
    |> Seq.zip (forLinearity.ColumnKeys)
    // group by protein, NOT BY PEPTIDE
    |> Seq.groupBy (fst >> fst)
    |> Seq.map 
        (fun ((protein,peptideMeans)) -> 
            let peptidePlots =
                peptideMeans
                |> Seq.map 
                    (fun ((prot,pep),(means,r1,r2,r3)) ->
                    [
                        Chart.Scatter(r1,mode=StyleParam.Mode.Markers, MarkerSymbol = StyleParam.Symbol.Cross, Color="#92a8d1")
                        |> Chart.withTraceName "4A"
                        Chart.Scatter(r2,mode=StyleParam.Mode.Markers, MarkerSymbol = StyleParam.Symbol.Cross, Color="#FF6F61")
                        |> Chart.withTraceName "CW15"
                        Chart.Scatter(r3,mode=StyleParam.Mode.Markers, MarkerSymbol = StyleParam.Symbol.Cross, Color="#00e257")
                        |> Chart.withTraceName "UVM4"
                        //|> GenericChart.mapTrace
                        //    (fun t ->
                        //        t?fill <- "tonexty"
                        //        t?fillcolor <- "lightgrey"
                        //        t
                        //    )
                        Chart.Scatter(means,mode=StyleParam.Mode.Lines_Markers, MarkerSymbol = StyleParam.Symbol.Circle, Color = "#C78D9B") (*|> Chart.withYError (Error.init(Symmetric=true,Array=se))*)
                        |> Chart.withTraceName (sprintf "mean [%s] %s" prot pep)
                    ]
                    |> Chart.Combine
                    |> Chart.withX_Axis (xAxis false "N14 Sample / N15 QProtein ratio" 20 16)
                    |> Chart.withY_Axis (yAxis false "N14/N15 Quantification ratio" 20 16 (0.,7.5))
                )

            let meanPoints =
                peptideMeans
                |> Seq.map 
                    (fun ((_,pep),(means,r1,r2,r3)) ->
                        Chart.Scatter(means,mode=StyleParam.Mode.Markers, MarkerSymbol = StyleParam.Symbol.Cross)
                        |> Chart.withTraceName pep
                    )
                |> Chart.Combine
                
            let proteinLine =
                peptideMeans
                |> Seq.map 
                    (fun ((_,_),(means,r1,r2,r3)) -> means)
                |> fun x -> x
                |> JaggedArray.ofJaggedSeq
                |> JaggedArray.transpose
                |> Array.map 
                    (fun x -> 
                        x
                        |> Array.unzip
                        |> fun (x,y) -> Seq.mean x, Seq.mean y)
                |> fun x -> Chart.Scatter(x,mode=StyleParam.Mode.Lines_Markers, MarkerSymbol = StyleParam.Symbol.Circle) (*|> Chart.withYError (Error.init(Symmetric=true,Array=se))*)
                |> Chart.withTraceName protein

            protein, 
            [ 
                [meanPoints;proteinLine] 
                |> Chart.Combine
                |> Chart.withX_Axis (xAxis false "N14 Sample / N15 QProtein ratio" 20 16)
                |> Chart.withY_Axis (yAxis false "N14/N15 Quantification ratio" 20 16 (0.,7.5))
                yield! peptidePlots
            ]
            )
           
    |> Seq.filter (fun (x,_) -> 
        [|"rbcl";"rbcs"|]//proteinNames
        |> Array.exists (fun p -> x.ToLower().Contains(p) )
        )
    |> Seq.map (fun (prot,x) -> x |> Chart.Stack 2 |> Chart.withTitle prot)
    |> Seq.map (Chart.withConfig config)
    |> Seq.map (Chart.withSize (1600.,750.))
    |> Array.ofSeq
    |> Array.iter Chart.Show

//plotPeptideISD [|"rbcl";"rbcs"|]


let wholeCell_PeptideRatios =

    Array.map2 (Array.zip) n14Lin n15Lin
    |> JaggedArray.map (fun (n14,n15) -> if (isBad n14 || isBad n15) then 0. else n14/n15)
    |> Array.map (fun x -> Array.zip[|10.;10.;5.;5.;1.;1.;0.5;0.5|] x)
    |> Array.map 
        (fun (values) -> 
            values 
            |> Array.chunkBySize 2 
            |> Array.map (fun reps -> fst reps.[0] , reps |> Array.map snd |> Seq.mean),
            [|for i in 0 .. 2 ..7 do yield values.[i]|],
            [|for i in 1 .. 2 ..7 do yield values.[i]|]
            )
    |> Seq.zip (forLinearity.ColumnKeys)
    |> Seq.groupBy (fst >> fst)
    |> Seq.map (snd)
    |> Seq.map 
        (
            fun x ->
                let (rk1,rk2),(mean,r1,r2) = Seq.item 0 x
                [
                    (rk1,(rk2,"mean")) => series mean
                    (rk1,(rk2,"r1")) => series r1
                    (rk1,(rk2,"r2")) => series r2
                ]
                |> frame
    
        )
    |> Seq.reduce (Frame.join JoinKind.Outer)
    |> Frame.transpose

let rbcl_RatiosS_wholeCell =
    wholeCell_PeptideRatios
    |> Frame.filterRows (fun (_,(_,rk)) _ -> rk = "mean")
    |> Frame.transpose
    |> Frame.filterCols (fun (prot,(pep,_)) _ -> prot = "rbcL" && (not (pep = "AFPDAYVR" || pep = "EVTLGFVDLMR")))
    |> Frame.transpose
    |> Frame.applyLevel fst Stats.mean

let rbcs_RatiosSdsIgd_wholeCell =
    wholeCell_PeptideRatios
    |> Frame.filterRows (fun (_,(_,rk)) _ -> rk = "mean")
    |> Frame.transpose
    |> Frame.filterCols (fun (prot,(pep,_)) _ -> prot = "RBCS2" && (not (pep = "AFPDAYVR" || pep = "EVTLGFVDLMR")))
    |> Frame.transpose
    |> Frame.applyLevel fst Stats.mean

open FSharp.Stats.Fitting.LinearRegression.OrdinaryLeastSquares.Linear



let rbc_L_vs_S_rbcl_RatiosS_wholeCell =

    let rbcs =
        rbcs_RatiosSdsIgd_wholeCell
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D
        |> Array.concat

    //let ratios =
    //    Array.zip3 [|10.;5.;1.;0.5|] rbcl rbcs
    //    |> Array.map (fun (x,l,s) -> x, (l/s))

    let RBCScoeff = Univariable.coefficient (vector [|10.;5.;1.;0.5|]) (vector rbcs) 
    let RBCSfitFunc = Univariable.fit RBCScoeff
    let RBCSfitVals = [|10.;5.;1.;0.5|] |> Array.map RBCSfitFunc
    let RBCSdetermination = FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue rbcs RBCSfitVals
    let RBCSpearson = FSharp.Stats.Correlation.Seq.pearson (rbcs) [|10.;5.;1.;0.5|]
    printfn "Pearson WholeCell RBCS: %f" RBCSpearson

    let rbcl =
        rbcl_RatiosS_wholeCell
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D
        |> Array.concat

    // RBCL Regression of relative quantification values
    let RBCLcoeff = Univariable.coefficient (vector [|10.;5.;1.;0.5|]) (vector rbcl)
    let RBCLfitFunc = Univariable.fit RBCLcoeff
    let RBCLfitVals = [|10.;5.;1.;0.5|] |> Array.map RBCLfitFunc
    let RBCLdetermination = FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue rbcl RBCLfitVals
    let RBCLpearson = FSharp.Stats.Correlation.Seq.pearson (rbcl) [|10.;5.;1.;0.5|]
    printfn "Pearson WholeCell RBCL: %f" RBCLpearson



    [
        Chart.Point ((Array.zip [|10.;5.;1.;0.5|] rbcl),Name = "RbcL Quantified Ratios")
        |> Chart.withMarkerStyle(Size=10,Symbol = StyleParam.Symbol.Cross)
        Chart.Line(Array.zip [|10.;5.;1.;0.5|] RBCLfitVals,Name = (sprintf "RbcL linear regression: %.2f x + (%2f) ; R² = %.4f" RBCLcoeff.[1] RBCLcoeff.[0] RBCLdetermination))
        |> Chart.withLineStyle(Color="lightblue",Dash=StyleParam.DrawingStyle.DashDot)

        Chart.Point ((Array.zip [|10.;5.;1.;0.5|] rbcs),Name = "RbcS Quantified Ratios",MarkerSymbol = StyleParam.Symbol.Cross)
        |> Chart.withMarkerStyle(Size=10,Symbol = StyleParam.Symbol.Cross)
        Chart.Line(Array.zip [|10.;5.;1.;0.5|] RBCSfitVals,Name = (sprintf "RbcS linear regression: %.2f x + (%2f) ; R² = %.4f" RBCScoeff.[1] RBCScoeff.[0] RBCSdetermination))
        |> Chart.withLineStyle(Color="LightGreen",Dash=StyleParam.DrawingStyle.DashDot)
    ]
    |> Chart.Combine
    |> Chart.withTitle "Whole cell extracts: Stability of rbcl/rbcs ratios between samples"
    |> Chart.withX_Axis (yAxis false "N14 Sample / N15 QProtein ratio" 20 16 (0. ,10.5))
    |> Chart.withY_Axis (xAxis false "relative quantification" 20 16 (*(1.3,1.6)*))
    |> Chart.withConfig config
    |> Chart.withSize (700.,700.)
    |> Chart.Show
