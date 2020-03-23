

#load @"../IfSharp/References.fsx"
#load @"../IfSharp/Paket.Generated.Refs.fsx"
#load @"../AuxFsx/DeedleAux.fsx"

open BioFSharp
open Deedle
open FSharpAux
open FSharp.Stats
open FSharp.Plotly

let colorArray = [|"#E2001A"; "#FB6D26"; "#00519E"; "#00e257";|]

let colorForMean = "#366F8E"

let xAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Zeroline=false,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside, Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))
let yAxis showGrid title titleSize tickSize = Axis.LinearAxis.init(Title=title,Showgrid=showGrid,Showline=true,Mirror=StyleParam.Mirror.All,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside,Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=tickSize),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=titleSize))

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
    "iRT"   =>  "LGGNEQVTR"
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
    
let root = __SOURCE_DIRECTORY__

    //JP12_WC_01
let getWholeCellResults path : Frame<string*(string*(string*string)),string*string> = 
    readQConcatResultFrame (root + path)//@"\..\AuxFiles\FredQuantifiedPeptides.txt"
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
            |None       -> (sprintf "%s not found in 'peptideProtMapping'(QConCat)" ck),ck
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

let getForLinearity (proteinsToShow:string [] option) (peptidesToIgnore:string [] option) (wcResults:Frame<(string*(string*(string*string))),(string*string)>)= 
    
    let prepToShowProteins =
        if proteinsToShow.IsSome then
            proteinsToShow.Value 
            |> Array.map (fun x -> x.ToLower())
        else 
            [||]

    let prepToIgnorePeptides =
        if peptidesToIgnore.IsSome then
            peptidesToIgnore.Value
            |> Array.map (fun x -> x.ToLower())
        else 
            [||]

    wcResults
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
    |> Frame.filterCols (fun ck cs -> 
        let prepProtInFrame = (fst ck).ToLower()
        let prepPeptInFrame = (snd ck).ToLower()
        match proteinsToShow, peptidesToIgnore with
        | Some _, Some _ -> 
            Array.exists (fun x -> x = prepProtInFrame) prepToShowProteins
            && Array.exists (fun x -> x = prepPeptInFrame) prepToIgnorePeptides |> not
        | Some _, None ->
            Array.exists (fun x -> x = prepProtInFrame) prepToShowProteins
        | None, Some _ ->
            Array.exists (fun x -> x = prepPeptInFrame) prepToIgnorePeptides |> not
        | None, None -> true
    )

let n14Lin (linearityData:Frame<(string*(string*(float*string))),(string*string)>) =
    linearityData
    |> Frame.filterRows (fun rk _ -> not ((fst rk).Contains("N15Quant")))
    |> Frame.transpose
    |> Frame.toArray2D
    |> Array2D.toJaggedArray
    // Group 3 had 4x less than in protocol
    //|> JaggedArray.mapi (fun i x -> if List.contains i [ 0 .. 2 ..7 ] then x/4. else x)

let n15Lin (linearityData:Frame<(string*(string*(float*string))),(string*string)>)=
    linearityData
    |> Frame.filterRows (fun rk _ -> not ((fst rk).Contains("N14")) && not ((fst rk).Contains("Corrected")))
    |> Frame.transpose
    |> Frame.toArray2D
    |> Array2D.toJaggedArray

// Rows = 37, outer array
// Cols = 12, inner array

type PepResult1 = 
    {
        Protein     :   string
        Peptide     :   string
        StrainValues:   (float*float) []
        StrainName  :   string
    }

    static member create prot pep vals strainName = {
        Protein      =  prot
        Peptide      =  pep
        StrainValues =  vals
        StrainName   =  strainName
    }

let plotPeptideISD wcResults (proteinsToShow:string[] option) (peptidesToIgnore:string[] option) strainNameArr dilutionArr =

    let forLinearity = getForLinearity proteinsToShow peptidesToIgnore wcResults

    let strainNamesSorted = strainNameArr |> Array.sort //[|"4A"; "UVM";"CW15"|] 

    let dilutionsSorted = dilutionArr |> Array.sortDescending //[|1.;5.;25.;125.|] 

    let dilutionArrTimesStrains =
        [|for dil in dilutionsSorted do 
            yield! Array.init strainNamesSorted.Length (fun _ -> dil) |]

    let sortedStrainValues =
        Array.map2 (Array.zip) (n14Lin forLinearity) (n15Lin forLinearity)
        // JP12_WC_07
        |> JaggedArray.map (fun (n14,n15) -> if (isBad n14 || isBad n15) then 0. else n14/n15)
        // dilutions times the number of strains 3 x 125, 3 x 25, 3 x 5 ...
        // zip the following values to the columns
        |> Array.map (fun x -> Array.zip dilutionArrTimesStrains x)
        |> Array.map 
            (fun (values) -> 
                Array.init 
                    strainNamesSorted.Length 
                    (fun ind -> 
                        [|for x in ind .. strainNamesSorted.Length .. values.Length-1 do yield values.[x]|]
                    )
                |> Array.zip strainNamesSorted
            )
        |> Array.zip (Seq.toArray forLinearity.ColumnKeys)
        |> Array.collect (fun ((prot,pept),strainResults) -> 
            strainResults 
            |> Array.map (fun (strainName,values) ->
                PepResult1.create prot pept values strainName
            )  
        )

    let comparePeptidesInStrainCharts =
        sortedStrainValues
        |> Array.groupBy (fun x -> x.StrainName, x.Protein)
        |> Array.map (fun (header,peptInfoArr) -> 
            header,
            peptInfoArr
            |> Array.mapi (fun i peptInfo ->
                Chart.Scatter(peptInfo.StrainValues,mode=StyleParam.Mode.Markers, MarkerSymbol = StyleParam.Symbol.Cross, Color=colorArray.[i])
                |> Chart.withTraceName (sprintf "%s -  %s - %s" peptInfo.StrainName peptInfo.Protein peptInfo.Peptide)
            )
            |> Chart.Combine
            |> Chart.withX_Axis (xAxis false (fst header + "-N14 Sample/N15 QProtein ratio") 20 16)
            |> Chart.withY_Axis (yAxis false "N14/N15 Quantification ratio" 20 16)
        )

    let comparePeptidesInStrainMEANS =
        sortedStrainValues
        |> Array.groupBy (fun x -> x.StrainName, x.Protein)
        |> Array.map (fun (header,peptInfoArr) -> 
            header,
            [|for pept in peptInfoArr do
                yield! pept.StrainValues|]
        )
        |> Array.map (fun (header,values) -> header,values|> (Array.groupBy fst))
        |> Array.map (fun ((strain,prot),values) -> 
            let means =
                values 
                |> Seq.map (fun (xAxis,values) -> 
                    xAxis,values 
                    |> Seq.meanBy snd
                )
            (strain,prot),means
        )

    let compareBetweenStrainsChart =
        comparePeptidesInStrainMEANS
        |> Array.groupBy (fun (header,x) -> snd header)
        |> Array.map (fun (prot,strains) -> 
            ("CompareStrains", prot),
            strains
            |> Array.mapi (fun i ((strain,prot),values) -> 
                Chart.Scatter(values,mode=StyleParam.Mode.Lines_Markers, MarkerSymbol = StyleParam.Symbol.Cross, Color=colorArray.[i])
                |> Chart.withTraceName (sprintf "mean %s -  %s" strain prot)
            )
            |> Chart.Combine
            |> Chart.withX_Axis (xAxis false ("Strain Means-" + "N14 Sample/N15 QProtein ratio") 20 16)
            |> Chart.withY_Axis (yAxis false "N14/N15 Quantification ratio" 20 16)
        )

    let comparePeptidesInStrainMEANSCharts =
        comparePeptidesInStrainMEANS
        |> Array.map (fun ((strain,prot),means) ->
            (strain,prot),
            Chart.Scatter(means,mode=StyleParam.Mode.Lines_Markers, MarkerSymbol = StyleParam.Symbol.Circle, Color=colorForMean, Opacity=0.8)
            |> Chart.withTraceName (sprintf "mean %s - %s" strain prot)
        )

    let alignPeptideAndPeptideMeans =
        Array.zip comparePeptidesInStrainCharts comparePeptidesInStrainMEANSCharts
        |> Array.map (fun ((header,chart),(header,chartMean)) -> header, Chart.Combine [|chart; chartMean|])

    [|yield! compareBetweenStrainsChart; yield! alignPeptideAndPeptideMeans|]
    |> Array.groupBy (fun (header,chart) -> snd header) 
    |> fun x -> x
    |> Array.map (fun (prot,chartsWithMeta) -> 
        chartsWithMeta
        |> Array.map (fun x -> 
            snd x 
        )
        |> Chart.Stack(2,Space=0.15)
        |> Chart.withTitle prot
        |> Chart.withSize (1200.,900.)
        |> Chart.withConfig config
        |> Chart.Show
    )

let wcResults = getWholeCellResults "\..\AuxFiles\FredQuantifiedPeptides.txt"

let sth = plotPeptideISD wcResults (Some [|"RBCL"; "Rbcs2"|]) None [|"4A"; "UVM";"CW15"|] [|1.;5.;25.;125.|] 

let wholeCell_PeptideRatios wcResults proteinsToShow peptidesToIgnore (strainNameArr:string []) (dilutionArr: float[]) =

    let forLinearity = getForLinearity proteinsToShow peptidesToIgnore wcResults

    let strainNamesSorted = strainNameArr |> Array.sort //[|"4A"; "UVM";"CW15"|] 

    let dilutionsSorted = dilutionArr |> Array.sortDescending //[|1.;5.;25.;125.|] 

    let dilutionArrTimesStrains =
        [|for dil in dilutionsSorted do 
            yield! Array.init strainNamesSorted.Length (fun _ -> dil) |]

    let sortedStrainValues =
        Array.map2 (Array.zip) (n14Lin forLinearity) (n15Lin forLinearity)
        // JP12_WC_07
        |> JaggedArray.map (fun (n14,n15) -> if (isBad n14 || isBad n15) then 0. else n14/n15)
        // dilutions times the number of strains 3 x 125, 3 x 25, 3 x 5 ...
        // zip the following values to the columns
        |> Array.map (fun x -> Array.zip dilutionArrTimesStrains x)
        |> Array.map 
            (fun (values) -> 
                Array.init 
                    strainNamesSorted.Length 
                    (fun ind -> 
                        [|for x in ind .. strainNamesSorted.Length .. values.Length-1 do yield values.[x]|]
                    )
                |> Array.zip strainNamesSorted
            )
        |> Array.zip (Seq.toArray forLinearity.ColumnKeys)
        |> Array.collect (fun ((prot,pept),strainResults) -> 
            strainResults 
            |> Array.map (fun (strainName,values) ->
                PepResult1.create prot pept values strainName
            )  
        )

    let comparePeptidesInStrainMEANS =
        sortedStrainValues
        |> Array.groupBy (fun x -> x.StrainName, x.Protein)
        |> Array.map (fun (header,peptInfoArr) -> 
            header,
            [|for pept in peptInfoArr do
                yield! pept.StrainValues|]
        )
        |> Array.map (fun (header,values) -> header,values|> (Array.groupBy fst))
        |> Array.map (fun ((strain,prot),values) -> 
            let means =
                values 
                |> Seq.map (fun (xAxis,values) -> 
                    xAxis,values 
                    |> Seq.meanBy snd
                )
            PepResult1.create prot "mean" (Seq.toArray means) strain
        )

    [|yield! comparePeptidesInStrainMEANS; yield! sortedStrainValues|]
    |> Array.groupBy (fun x -> x.StrainName, x.Protein)
    |> Array.map (fun x -> snd x)
    |> Array.map (
        fun x ->
            [for pepRes in x do
                yield
                    ((pepRes.Protein,pepRes.StrainName),pepRes.Peptide) => series pepRes.StrainValues
            ]
            |> frame
        )
    |> Seq.reduce (Frame.join JoinKind.Outer)
    |> Frame.transpose
    |> Frame.sortRowsByKey
    |> Frame.sortColsByKey

let sth2 = wholeCell_PeptideRatios wcResults (Some [|"RBCL"; "Rbcs2"|]) (Some [|"DTDILAAFR"|]) [|"4A"; "UVM";"CW15"|] [|1.;5.;25.;125.|] 



open FSharp.Stats.Fitting.LinearRegression.OrdinaryLeastSquares.Linear

let rbc_L_vs_S_rbcl_RatiosS_wholeCell prot1Name prot2Name (wcPeptideRatios:Frame<((string*string)*string),float>) =
    
    let prot1_RatiosS_wholeCell =
        wcPeptideRatios
        |> Frame.filterRows (fun ((prot,strain),pep) _ -> pep = "mean")
        |> Frame.transpose
        |> Frame.filterCols (fun ((prot,strain),pep) _ -> prot = prot1Name)
        |> Frame.transpose
        |> Frame.applyLevel fst Stats.mean
    
    let prot2_RatiosSdsIgd_wholeCell =
        wcPeptideRatios
        |> Frame.filterRows (fun ((prot,strain),pep) _ -> pep = "mean")
        |> Frame.transpose
        |> Frame.filterCols (fun ((prot,strain),pep) _ -> prot = prot2Name) //(fun (prot,(pep,_)) _ -> prot = "RBCS2" && (not (pep = "AFPDAYVR" || pep = "EVTLGFVDLMR")))
        |> Frame.transpose
        |> Frame.applyLevel fst Stats.mean

    let dilutionsSorted = 
        wcPeptideRatios.ColumnKeys
        |> Array.ofSeq

    let strainNames = 
        wcPeptideRatios.RowKeys
        |> Seq.map (fun ((x,y),z) -> y)
        |> (Seq.distinct >> Array.ofSeq)

    let prot1 =
        prot1_RatiosS_wholeCell
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D

    let prot1Coeff,prot1FitVals,prot1Determination =
        prot1
        |> Array.mapi (
            fun i strainVals ->
                // RBCL Regression of relative quantification values
                let RBCLcoeff = Univariable.coefficient (vector dilutionsSorted) (vector strainVals)
                let RBCLfitFunc = Univariable.fit RBCLcoeff
                let RBCLfitVals = dilutionsSorted |> Array.map RBCLfitFunc
                let RBCLdetermination = FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue strainVals RBCLfitVals
                let RBCLpearson = FSharp.Stats.Correlation.Seq.pearson (strainVals) dilutionsSorted
                printfn "%s - Pearson WholeCell RBCL: %f" strainNames.[i] RBCLpearson
                RBCLcoeff, RBCLfitVals, RBCLdetermination
        )
        |> Array.unzip3
                
    let prot2 =
        prot2_RatiosSdsIgd_wholeCell
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D

    let prot2Coeff,prot2FitVals,prot2Determination =
        prot2
        |> Array.mapi (
            fun i strainVals ->
                let RBCScoeff = Univariable.coefficient (vector dilutionsSorted ) (vector strainVals) 
                let RBCSfitFunc = Univariable.fit RBCScoeff
                let RBCSfitVals = dilutionsSorted  |> Array.map RBCSfitFunc
                let RBCSdetermination = FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue strainVals RBCSfitVals
                let RBCSpearson = FSharp.Stats.Correlation.Seq.pearson (strainVals) dilutionsSorted
                printfn "%s - Pearson WholeCell RBCS: %f" strainNames.[i] RBCSpearson
                RBCScoeff, RBCSfitVals, RBCSdetermination
        )
        |> Array.unzip3

    let chartPearsons prot1 (prot1Coeff:Vector<float>) prot1FitVals prot1Determination prot2 (prot2Coeff:Vector<float>) prot2FitVals prot2Ddetermination strain =
        [
            Chart.Point ((Array.zip dilutionsSorted prot1),Name = sprintf "%s Quantified Ratios" prot1Name)
            |> Chart.withMarkerStyle(Size=10,Symbol = StyleParam.Symbol.Cross)
            Chart.Line(Array.zip dilutionsSorted prot1FitVals,Name = (sprintf "%s linear regression: %.2f x + (%2f) ; R² = %.4f" prot1Name prot1Coeff.[1] prot1Coeff.[0] prot1Determination))
            |> Chart.withLineStyle(Color="lightblue",Dash=StyleParam.DrawingStyle.DashDot)

            Chart.Point ((Array.zip dilutionsSorted prot2),Name = sprintf "%s Quantified Ratios" prot2Name,MarkerSymbol = StyleParam.Symbol.Cross)
            |> Chart.withMarkerStyle(Size=10,Symbol = StyleParam.Symbol.Cross)
            Chart.Line(Array.zip dilutionsSorted prot2FitVals,Name = (sprintf "%s linear regression: %.2f x + (%2f) ; R² = %.4f" prot2Name prot2Coeff.[1] prot2Coeff.[0] prot2Ddetermination))
            |> Chart.withLineStyle(Color="LightGreen",Dash=StyleParam.DrawingStyle.DashDot)
        ]
        |> Chart.Combine
        |> Chart.withTitle (sprintf "%s - Whole cell extracts: Stability of %s/%s ratios between samples" strain prot1Name prot2Name)
        |> Chart.withX_Axis (yAxis false "N14 Sample / N15 QProtein ratio" 20 16)
        |> Chart.withY_Axis (xAxis false "relative quantification" 20 16 )
        |> Chart.withConfig config
        |> Chart.withSize (900.,500.)
        |> Chart.Show

    for i in 0 .. 2 do 
        chartPearsons 
            prot1.[i] prot1Coeff.[i] prot1FitVals.[i] prot1Determination.[i]
            prot2.[i] prot2Coeff.[i] prot2FitVals.[i] prot2Determination.[i]
            strainNames.[i]

rbc_L_vs_S_rbcl_RatiosS_wholeCell "rbcL" "RBCS2" sth2
