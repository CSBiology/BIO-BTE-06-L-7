
#load @"../IfSharp/References.fsx"
#load @"../IfSharp/Paket.Generated.Refs.fsx"
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

type StrainInfo =
    {
        Strain     : string
        ProtYg     : int
        Prot1Quant : float
        Prot2Quant : float
        Ratio      : float option
    }
    static member create strain protYg prot1Quant prot2Quant ratio =
        {
             Strain      = strain
             ProtYg      = protYg
             Prot1Quant  = prot1Quant
             Prot2Quant  = prot2Quant
             Ratio       = ratio
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

//FileName	Experiment	Content	ProteinAmount[ug]	Strain
let sdsSampleNameMapping = 
    [
    //80,40,20 ug geladenes protein
    "G2 L 4A 5myg"     => ("SDS_IGD",("RBCL",(5,"4A")))
    "G2 L 4A 10myg"    => ("SDS_IGD",("RBCL",(10,"4A")))
    "G2 L 4A 20myg"    => ("SDS_IGD",("RBCL",(20,"4A")))
    "G2 L 1690 5myg"   => ("SDS_IGD",("RBCL",(5,"1690")))
    "G2 L 1690 10myg"  => ("SDS_IGD",("RBCL",(10,"1690")))
    "G2 L 1690 20myg"  => ("SDS_IGD",("RBCL",(20,"1690")))
    "G2 L 1883 5myg"   => ("SDS_IGD",("RBCL",(5,"1883")))
    "G2 L 1883 10myg"  => ("SDS_IGD",("RBCL",(10,"1883")))
    "G2 L 1883 20myg"  => ("SDS_IGD",("RBCL",(20,"1883")))
    "G2 S 4A 5myg"     => ("SDS_IGD",("RBCS",(5,"4A")))
    "G2 S 4A 10myg"    => ("SDS_IGD",("RBCS",(10,"4A")))
    "G2 S 4A 20myg"    => ("SDS_IGD",("RBCS",(20,"4A")))
    "G2 S 1690 5myg"   => ("SDS_IGD",("RBCS",(5,"1690")))
    "G2 S 1690 10myg"  => ("SDS_IGD",("RBCS",(10,"1690")))
    "G2 S 1690 20myg"  => ("SDS_IGD",("RBCS",(20,"1690")))
    "G2 S 1883 5myg"   => ("SDS_IGD",("RBCS",(5,"1883")))
    "G2 S 1883 10myg"  => ("SDS_IGD",("RBCS",(10,"1883")))
    "G2 S 1883 20myg"  => ("SDS_IGD",("RBCS",(20,"1883")))
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

let sdsIgdResults : Frame<(string*(string*(string*(int*string)))),string*string> = 
    readQConcatResultFrame (source + @"\..\AuxFiles\GroupsData\G2_L_4A_20myg_QuantifiedPeptides.txt")
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
        //|> Frame.applyLevel (fun (experiment,(band,(proteinAmnt,rep))) -> (experiment,(band,proteinAmnt))) Stats.mean

let proteinMean_RatiosSdsIgd content protein =
    sdsIgdPeptideRatios
    |> Frame.filterRows (fun (_,(band,_)) _ -> band = content)
    |> Frame.filterCols (fun (prot,pep) _ -> prot = protein && (not (pep = "AFPDAYVR" || pep = "EVTLGFVDLMR" || pep = "FLFVAEAIYK")))
    |> Frame.transpose
    |> Frame.applyLevel fst Stats.mean

let rbcs_RatiosSdsIgd = proteinMean_RatiosSdsIgd "RBCS" "RBCS2"

let rbcl_RatiosSdsIgd = proteinMean_RatiosSdsIgd "RBCL" "rbcL"

open FSharp.Stats.Fitting.LinearRegression.OrdinaryLeastSquares.Linear

let rbc_L_vs_S_SDS_Igd (protAmounts: int []) (strains: string []) content1 protein1 content2 protein2=
    
    let strainNamesSorted = strains |> Array.sort

    let protAmountAndStrains =
        [|for amount in (protAmounts |> Array.sort) do 
            yield! Array.zip strainNamesSorted (Array.init strains.Length (fun _ -> amount))|]

    let prot1_RatiosSdsIgd = proteinMean_RatiosSdsIgd content1 protein1
    
    let prot2_RatiosSdsIgd = proteinMean_RatiosSdsIgd content2 protein2

    let prot1 =
        prot1_RatiosSdsIgd
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D
        |> Array.concat

    let prot2 =
        prot2_RatiosSdsIgd
        |> Frame.toArray2D
        |> JaggedArray.ofArray2D
        |> Array.concat

    let strainInfo = 
        Array.map3 
            (fun (strain,protAmnt) prot1 prot2 ->
                let ratio = prot1/prot2
                let ratioChecked =
                    if isBad ratio then
                        None
                    else
                        Some ratio
                StrainInfo.create
                    strain
                    protAmnt
                    prot1
                    prot2
                    ratioChecked
            ) protAmountAndStrains prot1 prot2

    let groupedStrainInfo =
        strainInfo
        |> Array.groupBy (fun x -> x.Strain)
        |> Array.map snd
        |> Array.map (fun x ->
            x
            |> Array.sortBy (fun x -> x.ProtYg)
        )

    printfn "%A" groupedStrainInfo

    let cotrolledAmountRatioStrain =
        groupedStrainInfo
        |> Array.map (fun strainInfos ->
            strainInfos
            |> Array.choose (fun strainInfo ->
                if strainInfo.Ratio.IsSome then
                    Some (float strainInfo.ProtYg, strainInfo.Ratio.Value)
                else
                    None
            ),strainInfos.[0].Strain
        )
        |> Array.map (fun ((amountRatioArr), strain) ->
            let amount,ratio = Array.unzip amountRatioArr
            amount, ratio, strain
        )

    let coeffs =
        cotrolledAmountRatioStrain
        |> Array.map (fun (amount,ratio,strain) ->
            Univariable.coefficient (vector amount) (vector ratio)
        )

    let fitFuncs = 
        coeffs
        |> Array.map (fun (coeff) ->
            Univariable.fit coeff
        )

    let fitVals = 
        cotrolledAmountRatioStrain
        |> Array.map2 (fun fitFunc (amount,ratio,strain) ->
            amount |> Array.map fitFunc
        ) fitFuncs

    let determinations = 
        cotrolledAmountRatioStrain
        |> Array.map2 (fun fitVal (amount,ratio,strain) ->
            FSharp.Stats.Fitting.GoodnessOfFit.calculateDeterminationFromValue ratio fitVal
        ) fitVals

    let pearsons = 
        cotrolledAmountRatioStrain
        |> Array.map (fun (amount,ratio,strain) ->
            FSharp.Stats.Correlation.Seq.pearson ratio amount
        )

    let resultCollection =
        [|for i = 0 to strains.Length-1 do
            yield
                {|
                    ProteinAmountsYg = cotrolledAmountRatioStrain.[i] |> fun (amount,_,_) -> amount
                    ProteinRatios    = cotrolledAmountRatioStrain.[i] |> fun (_,ratios,_) -> ratios
                    StrainName       = cotrolledAmountRatioStrain.[i] |> fun (_,_,strain) -> strain
                    Coefficients     = coeffs.[i]
                    FitValues        = fitVals.[i]
                    Determination    = determinations.[i]
                    Pearson          = pearsons.[i]
                |}
        |]

    resultCollection
    |> Array.iter (fun (result) ->
        printfn "Strain %s Pearson IGD: %f" result.StrainName result.Pearson
    )

    let ratioQuantCharts =
        resultCollection
        |> Array.map (fun result ->
            Chart.Point (result.ProteinAmountsYg,result.ProteinRatios,MarkerSymbol = StyleParam.Symbol.Cross)
            |> Chart.withTraceName (sprintf "Quantified Ratios (%s/%s) for strain %s" protein1 protein2 result.StrainName)
        )
        |> Chart.Combine

    let relQuantProt1Prot2 =
        groupedStrainInfo
        |> Array.map (fun strainInfo ->
            strainInfo
            |> Array.map (fun strain ->
                strain.Prot1Quant, strain.Prot2Quant
            ), (strainInfo |> Array.map (fun x -> float x.ProtYg)), strainInfo.[0].Strain
        )
        |> Array.map (fun ((quants),amount, strain) ->
            Array.unzip quants, amount, strain
        )
        |> Array.map (fun ((prot1Q,prot2Q),amount,strain) ->
            [
                Chart.Line(amount,prot1Q)
                |> Chart.withTraceName (sprintf "14N/15N %s for strain %s" protein1 strain)
                Chart.Line(amount,prot2Q)
                |> Chart.withTraceName (sprintf "14N/15N %s for strain %s" protein2 strain)
            ]
            |> Chart.Combine
        )
        |> Chart.Combine

    let fitCharts =
        resultCollection
        |> Array.map (fun result ->
            Chart.Line(Array.zip result.ProteinAmountsYg result.FitValues)
            |> Chart.withTraceName (sprintf "linear regression: %.2f x + (%2f) ; R² = %.4f for strain %s" result.Coefficients.[1] result.Coefficients.[0] result.Determination result.StrainName)
            |> Chart.withLineStyle(Color="#D3D3D3",Dash=StyleParam.DrawingStyle.DashDot)
        )
        |> Chart.Combine
    [
        ratioQuantCharts
        fitCharts
        relQuantProt1Prot2
    ]
    |> Chart.Combine
    |> Chart.withTitle (sprintf "SDS IGD: Stability of %s/%s ratios between strains" protein1 protein2)
    |> Chart.withX_Axis (xAxis false "absolute protein amount in sample [µg]" 20 16 )
    |> Chart.withY_Axis (xAxis false (sprintf "relative quantification") 20 16 )
    |> Chart.withConfig config
    |> Chart.withSize (1200.,700.)
    |> Chart.Show

rbc_L_vs_S_SDS_Igd [|5;10;20|] [|"1690"; "1883"; "4A"|] "RBCL" "rbcL" "RBCS" "RBCS2"