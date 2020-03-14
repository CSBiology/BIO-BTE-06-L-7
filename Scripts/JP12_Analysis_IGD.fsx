
#load @"../IfSharp/References.fsx"
#load @"../IfSharp/Paket.Generated.Refs.fsx"
#load @"../AuxFsx/ProtAux.fsx"


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

let wholeCellNameMapping = 
    [
    "Data20180126_InSolutionDigestWholeCell_Gr3+41" =>  ("WholeCell_ISD",("2:1","3") )
    "Data20180126_InSolutionDigestWholeCell_Gr3+42" =>  ("WholeCell_ISD",("1:1","3") )
    "Data20180126_InSolutionDigestWholeCell_Gr3+43" =>  ("WholeCell_ISD",("1:5","3") )
    "Data20180126_InSolutionDigestWholeCell_Gr3+44" =>  ("WholeCell_ISD",("1:10","3"))
    "Data20180126_InSolutionDigestWholeCell_Gr3+45" =>  ("WholeCell_ISD",("2:1","4") )
    "Data20180126_InSolutionDigestWholeCell_Gr3+46" =>  ("WholeCell_ISD",("1:1","4") )
    "Data20180126_InSolutionDigestWholeCell_Gr3+47" =>  ("WholeCell_ISD",("1:5","4") )
    "Data20180126_InSolutionDigestWholeCell_Gr3+48" =>  ("WholeCell_ISD",("1:10","4"))
    ]
    |> Map.ofList

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

//================================================================================
//====================== 3. SDS IGD ==============================================
//================================================================================

let sdsIgdResults : Frame<(string*(string*(string*(string*string)))),string*string> = 
    readQConcatResultFrame (source + @"\RERUN_Results\SDS_IGD\QuantifiedPeptides.txt")
    |> Frame.mapColKeys 
        (fun (ck:string) -> 
            let newCK = Map.find (ck.Split('_').[1 ..] |> String.concat "_") sdsSampleNameMapping
            ck.Split('_').[0] , newCK
        )
    |> Frame.sortColsByKey
    |> Frame.filterCols (fun ck _ -> (fst ck).Contains("Quant"))
    |> Frame.applyLevel (fun (sequence,(gmod,charge)) -> sequence) Stats.mean
    |> Frame.transpose
    |> Frame.mapColKeys
        (fun ck ->
            match Map.tryFind ck peptideProtMapping with
            |Some prot  -> prot,ck
            |None       -> "NotFound",ck
        )
    |> Frame.sortColsByKey

    |> Frame.sortRowsByKey


let sdsIgdPeptideRatios =
    sdsIgdResults
    |> Frame.mapCols(fun _ os -> os.As<float>() |> Series.mapValues (fun (x:float) -> if x > 2000000. || x < 1. then nan else x))
    |> Frame.filterRows (fun (q,n) _ -> not (q.Contains("Minus")))
    |> Frame.sortRowsByKey
    |> Frame.sortColsByKey
    |> fun f ->
        
        let finalRK =
            f
            |> Frame.filterRows (fun (q,n) _ -> (q.Contains("N14")))
            |> Frame.mapRowKeys snd
            |> fun x -> x.RowKeys

        let N14,N15 = 
            f
            |> Frame.filterRows (fun (q,n) _ -> (q.Contains("N14")))
            |> Frame.toArray2D
            |> JaggedArray.ofArray2D
            ,
            f
            |> Frame.filterRows (fun (q,n) _ -> (q.Contains("N15")))
            |> Frame.toArray2D
            |> JaggedArray.ofArray2D

        Array.map2 (Array.zip) N14 N15
        |> JaggedArray.map (fun (n14,n15) -> if (isBad n14 || isBad n15) then 0. else n14/n15)
        |> Array.map (fun x -> series (Seq.zip f.ColumnKeys x))
        |> Seq.zip finalRK 
        |> frame
        |> Frame.transpose
        // take means AFTER calculating ratios!
        |> Frame.applyLevel (fun (experiment,(band,(proteinAmnt,rep))) -> (experiment,(band,proteinAmnt))) Stats.mean

// Hier ist geladenes Protein bekannt, dadurch kann rückschluss auf anteil rbcl an gesamtprotein gezogen werden
let kda37_RatiosSdsIgd =
    sdsIgdPeptideRatios
    |> Frame.filterRows (fun (_,(band,_)) _ -> band = "37kDa")
    |> Frame.filterCols (fun (prot,pep) _ -> (prot = "rbcL" && (not (pep = "AFPDAYVR" || pep = "EVTLGFVDLMR"))) || (prot = "RBCS2" && (not (pep = "AFPDAYVR" || pep = "EVTLGFVDLMR"))))
    |> Frame.transpose
    //|> Frame.applyLevel fst Stats.mean

kda37_RatiosSdsIgd
|> Frame.filterRows (fun (prot,pep) _ -> (prot = "rbcL"))
|> fun f ->
    let normVals = 
        f
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D
        |> JaggedArray.transpose
        |> Array.map 
            (fun x ->
                let max = Array.max x
                x |> Array.map (fun v -> v / max)
            )
    let keys = 
        f.RowKeys
        |> Seq.map snd
        |> Array.ofSeq

    let names = 
        f.ColumnKeys
        |> Seq.map (fun (_,(_,r)) -> sprintf "%s [ug] total protein" r)
        |> Array.ofSeq

    normVals
    |> Array.map (fun inner -> Array.zip keys inner)
    |> Array.zip names
    |> Array.map (
        fun (k,c) ->
            Chart.Column(
                keysvalues=c,
                Name=k
                )
            |> Chart.withX_Axis (xAxis false "" 20. 18.)
            |> Chart.withY_Axis (yAxis false "" 20. 18. (0.,1.))
        )

    |> Chart.Stack 2
    |> Chart.withSize (1200.,800.)
    |> Chart.withConfig config
    |> Chart.Show

let rbcl_RatiosSdsIgd =
    sdsIgdPeptideRatios
    |> Frame.filterRows (fun (_,(band,_)) _ -> band = "RBCL")
    |> Frame.filterCols (fun (prot,pep) _ -> prot = "rbcL" && (not (pep = "AFPDAYVR" || pep = "EVTLGFVDLMR" || pep = "FLFVAEAIYK")))
    |> Frame.transpose
    |> Frame.applyLevel fst Stats.mean

let rbcs_RatiosSdsIgd =
    sdsIgdPeptideRatios
    |> Frame.filterRows (fun (_,(band,_)) _ -> band = "RBCS")
    |> Frame.filterCols (fun (prot,pep) _ -> prot = "RBCS2" && (not (pep = "AFPDAYVR" || pep = "EVTLGFVDLMR")))
    |> Frame.transpose
    |> Frame.applyLevel fst Stats.mean

//let with37kDa =
//    let corrRbcL = 
//        rbcl_RatiosSdsIgd
//        |> Frame.filterCols(fun (_,(_,r)) _ -> (r = "20") || (r = "80"))
//        |> fun x -> 
//            let lol = kda37_RatiosSdsIgd |> Frame.mapColKeys (fun (a,(b,c)) -> (a,("RBCL",c)))
//            x + lol |> Frame.dropSparseRows

    //let corrRbcS = 
    //    rbcs_RatiosSdsIgd
    //    |> Frame.filterCols(fun (_,(_,r)) _ -> (r = "20") || (r = "80"))
    //    |> fun x -> 
    //        let lol = kda37_RatiosSdsIgd |> Frame.mapColKeys (fun (a,(b,c)) -> (a,("RBCS",c)))
    //        x + lol |> Frame.dropSparseRows 

    //Frame.merge corrRbcL corrRbcS

open FSharp.Stats.Fitting.LinearRegression.OrdinaryLeastSquares.Linear

let rbc_L_vs_S_SDS_Igd =

    let rbcl =
        rbcl_RatiosSdsIgd
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D
        |> Array.concat

    let rbcs =
        rbcs_RatiosSdsIgd
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D
        |> Array.concat

    let xVals,ratios = 
        Array.zip3 [|10.;20.;40.;5.;80.|] rbcl rbcs 
        |> Array.map 
            (fun (protAmnt,rbcl,rbcs) ->
                protAmnt, (rbcl/rbcs)
            )
        |> Array.sortBy fst
        //|> Array.skip 1
        |> Array.unzip


    let coeff = Univariable.coefficient (vector xVals) (vector ratios)
    let fitFunc = Univariable.fit coeff
    let fitVals = xVals |> Array.map fitFunc
    let determination = FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue ratios fitVals
    let pearson = FSharp.Stats.Correlation.Seq.pearson ratios xVals
    printfn "Pearson IGD: %f" pearson

    [
        Chart.Point (xVals,ratios,Name = "Quantified Ratios",MarkerSymbol = StyleParam.Symbol.Cross)
        Chart.Line(Array.zip xVals fitVals,Name = (sprintf "linear regression: %.2f x + (%2f) ; R² = %.4f" coeff.[1] coeff.[0] determination))
        |> Chart.withLineStyle(Color="#D3D3D3",Dash=StyleParam.DrawingStyle.DashDot)
    ]
    |> Chart.Combine
    |> Chart.withTitle "SDS IGD: Stability of rbcl/rbcs ratios between samples"
    |> Chart.withX_Axis (xAxis false "absolute protein amount in sample [unit]" 20 16 )
    |> Chart.withY_Axis (xAxis false "relative quantification (rbcL/rbcS)" 20 16 )
    |> Chart.withConfig config
    |> Chart.withSize (700.,700.)
    |> Chart.Show

