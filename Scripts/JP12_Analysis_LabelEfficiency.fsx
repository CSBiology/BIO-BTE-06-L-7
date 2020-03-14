
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


//================================================================================
//====================== 1. LABEL EFFICIENCY =====================================
//================================================================================

let source = __SOURCE_DIRECTORY__

let labelEfficiencyResults : Frame<string*(string*(string*string)),(string*(string*int))> = 
    readQConcatResultFrame (source + @"\RERUN_Results\LabelEfficiency\QuantifiedPeptides.txt")
    |> Frame.mapColKeys 
        (fun (ck:string) -> 
            printfn "%s" ck
            let newCK = Map.find (ck.Split('_').[1 ..] |> String.concat "_") labelEfficiencyNameMapping
            ck.Split('_').[0] , newCK
        )
    |> Frame.sortColsByKey
    |> Frame.filterCols (fun ck _ -> (fst ck).Contains("Quant") || (fst ck).Contains("N15MZ") || (fst ck).Contains("N15Minus1MZ"))
    |> Frame.applyLevel (fun (sequence,(gmod,charge)) -> sequence,charge) Stats.mean
    |> Frame.transpose
    |> Frame.mapColKeys
        (fun ck ->
            match Map.tryFind (fst ck) peptideProtMapping with
            |Some prot  -> prot,ck
            |None       -> "NotFound",ck
        )

open Isotopes
open Elements

let initlabelN15Partial n15Prob =
    ///Diisotopic representation of nitrogen with abundancy of N14 and N15 swapped
    let n14Prob = 1. - n15Prob
    let N15 = Di (createDi "N15" (Isotopes.Table.N15,n15Prob) (Isotopes.Table.N14,n14Prob) )
    fun f -> Formula.replaceElement f Elements.Table.N N15

//let labelFullN15 =
//    let N15 = Elements.Table.Heavy.N15
//    fun f -> Formula.replaceElement f Elements.Table.N N15

let generateIsotopicDistributionOfFormulaBySum (charge:int) (f:Formula.Formula) =
    IsotopicDistribution.MIDA.ofFormula 
        IsotopicDistribution.MIDA.normalizeByProbSum
        0.01
        0.001
        charge
        f

let generateIsotopicDistributionOfFormulaByMax (charge:int) (f:Formula.Formula) =
    IsotopicDistribution.MIDA.ofFormula 
        IsotopicDistribution.MIDA.normalizeByMaxProb
        0.01
        0.001
        charge
        f

let labelEfficiency =
    labelEfficiencyResults
    |> Frame.filterRows 
        (fun (rk,_) _ -> not (rk.Contains("N14")))
    |> Frame.filterRows 
        (fun (_,(_,(rk,_))) _ -> rk = ("15N Q"))
    |> Frame.applyLevel 
        (fun (ex,(le,(rk,_))) -> (ex,(le,rk)))
        Stats.mean
    |> Frame.dropSparseCols
    |> fun x -> 
        x
        |> Frame.toArray2D
        |> Array2D.toJaggedArray
        |> JaggedArray.transpose
        |> Array.zip (x.ColumnKeys |> Array.ofSeq)
        //Calculate fully/uncompletely labeled peak ratio
        |> Array.map 
            (fun (key,values) -> 
                key,
                //N15-1 / N15
                (values.[1] ,values.[3]),
                //N15-1
                values.[0],
                //N15
                values.[2]
            )
        |> Array.map 
            (fun ((prot,(peptideSequence,charge)),(n15Minus1Quant,n15Quant),n15Minus1MZ,n15Mz) -> 

                let peakRatio = n15Minus1Quant / n15Quant

                printfn "[%s] : %s @ + %i" prot peptideSequence charge

                let peptide =
                    peptideSequence
                    |> BioArray.ofAminoAcidString
                    |> BioSeq.toFormula

                let theoreticalIsotopicDistributions =
                    [for i in 0.5 .. 0.001 .. 0.999 do
                        if ((int (i*1000.)) % 100) = 0 then
                            printfn "%.3f" i
                        yield
                            i,
                            peptide
                            |> initlabelN15Partial i
                            |> Formula.add Formula.Table.H2O
                            |> generateIsotopicDistributionOfFormulaByMax charge
                    ]

                let theoreticalRatios =
                    theoreticalIsotopicDistributions
                    |> List.map 
                        (fun (le,dist) ->
                            let n15Prob = 
                                dist
                                |> List.minBy 
                                    (fun (mz,prob) ->
                                        abs (n15Mz - mz)
                                    )
                                |> snd
                                
                            let n15Minus1Prob = 
                                dist
                                |> List.minBy 
                                    (fun (mz,prob) ->
                                        abs (n15Minus1MZ - mz)
                                    )
                                |> snd
                            le,(n15Minus1Prob / n15Prob), dist
                        )

                let bestLE = 
                    theoreticalRatios
                    |> List.minBy
                        (fun (le,ratio,dist) ->
                            abs (peakRatio - ratio)
                        )


                (prot,(peptideSequence,charge)),
                [(n15Minus1MZ,n15Minus1Quant);(n15Mz,n15Quant)],
                bestLE

                //let monoN15Mass =
                //    theoreticalIsotopicDistributions
            )
    //|> Array.zip (labelEfficiencyResults.ColumnKeys |> Array.ofSeq)
    //|> fun x -> frame ["Label Efficiency" => series x]
    //|> Frame.sortRowsByKey

let allPredictedLE =
    labelEfficiency
    |> Array.map 
        (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) ->
            (prot,peptideSequence),le
        )

let outlierBorders = FSharp.Stats.Testing.Outliers.tukey 3. (allPredictedLE |> Array.map snd)

//Boxplot with outlier borders
allPredictedLE
|> Array.map snd
|> fun x -> 
    [
    Chart.BoxPlot(
        x = (
            allPredictedLE
            |> Array.filter (fun (keys,eff) -> eff < outlierBorders.Upper && eff > outlierBorders.Lower)
            |> Array.map snd
        ),
        Jitter = 0.3,
        Boxpoints=StyleParam.Boxpoints.All
        )
    Chart.BoxPlot(x=x,Jitter = 0.3,Boxpoints=StyleParam.Boxpoints.All)
    |> Chart.withShapes 
        [
            (Shape.init(StyleParam.ShapeType.Line, X0 = outlierBorders.Upper, X1 = outlierBorders.Upper, Y0 = -0.4, Y1 = 1.4))
            (Shape.init(StyleParam.ShapeType.Line, X0 = outlierBorders.Lower, X1 = outlierBorders.Lower, Y0 = -0.4, Y1 = 1.4))
        ]
    ]
|> Chart.Combine
|> Chart.withX_Axis (yAxis false "N15 / N15 - 1 m/z Peak instensity ratio" 20 16 (0.925, 1.075))
|> Chart.withY_Axis (xAxis false "" 20 16)
|> Chart.withTitle "Label efficiency - Outlier detection"
|> Chart.withSize (1200.,600.)
|> Chart.withConfig config
|> Chart.ShowWithDescription
    {Heading = "Borders From tukey outlier detection with k = 3:"; Text = sprintf "Upper: %.3f <br></br>Lower: %.3f" outlierBorders.Upper outlierBorders.Lower}

/////Outlier Peptides in Label efficiency
//let outliersInLE =
//    allPredictedLE
//    |> Array.filter (fun (keys,eff) -> eff > outlierBorders.Upper || eff < outlierBorders.Lower)

///Label efficiency mean value without outliers
let filteredOverallPredictedLabelEfficiency =
    allPredictedLE
    |> Array.filter (fun (keys,eff) -> eff < outlierBorders.Upper && eff > outlierBorders.Lower)
    |> Array.map snd
    |> Seq.median 

labelEfficiency
|> Array.sortBy (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) -> prot)
|> Array.map 
    (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) ->
    
        let normExperimental =
            experimentalDist
            |> List.unzip
            |> fun (x,y) ->
                List.zip
                    x
                    (
                        y
                        |> fun vals -> 
                            let max = List.max vals
                            vals
                            |> List.map (fun v -> v / max)
                    )

        [
            dist
            |> List.map (fun (x,y) -> [(x,0.);(x,y);(x,0.)])
            |> List.concat
            |> Chart.Line
            |> Chart.withTraceName (sprintf "[%s] %s : PID @ %.3f " prot peptideSequence le)
            Chart.Point(normExperimental,Name= sprintf "[%s] %s : Experimental Masses"  prot peptideSequence)
        ]
        |> Chart.Combine
        |> Chart.withTitle (sprintf "[%s] : %s @ + %i" prot peptideSequence charge)
        |> Chart.withX_Axis (xAxis false "" 20 16)
        |> Chart.withY_Axis (yAxis false "" 20 16 (0.,1.1))
        |> Chart.withConfig config
    )
    |> Chart.Stack 4
    |> Chart.withSize (1500.,1500.)
    |> Chart.Show

//Overall
//1. Generate multiple patterns with label efficiencies
//2. See which fits best
//3. Take this LE for the peptide
//4. take median to get overall label efficiency

"NILLNEGIR"
|> BioArray.ofAminoAcidString
|> BioSeq.toFormula
|> Formula.count (Elements.Table.N)




[
    "AFPDAYVR"
    |> BioArray.ofAminoAcidString
    |> BioSeq.toFormula
    |> Formula.add Formula.Table.H2O
    |> initlabelN15Partial 0.999
    |> generateIsotopicDistributionOfFormulaByMax 2 
    |> Chart.Point
    |> Chart.withTraceName "0.999% 15N"

    "AFPDAYVR"
    |> BioArray.ofAminoAcidString
    |> BioSeq.toFormula
    |> Formula.add Formula.Table.H2O
    |> generateIsotopicDistributionOfFormulaByMax 2 
    |> Chart.Point
    |> Chart.withTraceName "Natural Abundance"

]
|> Chart.Combine
|> Chart.withX_Axis (xAxis false "" 20 16)
|> Chart.withY_Axis (xAxis false "" 20 16)
|> Chart.withConfig config
|> Chart.Show


//Correction Factor
//1. Generate Pattern with overall Label efficiency
//2. Generate fully labeled MIDAs pattern
//3. calculate ideal LabelEfficiency ~100%

type LabelEfficiencyPredictor =
    {
        Protein: string
        PeptideSequence: string
        Charge: int
        PredictedDistribution: (float*float) list
        PredictedLabelEfficiency: float
        ExperimentalDistribution: (float*float) list
    }

let createLabelEfficiencyPredictor p ps c pred eff exp =
    {
        Protein                     = p
        PeptideSequence             = ps
        Charge                      = c
        PredictedDistribution       = pred
        PredictedLabelEfficiency    = eff
        ExperimentalDistribution    = exp
    }


type LabelEfficiencyResult =
    {
        Protein                 : string
        PeptideSequence         : string
        Charge                  : int
        PredictedLabelEfficiency: float
        PredictedPattern        : (float*float) list
        ActualPattern           : (float*float) list
        MedianLabelEfficiency   : float
        MedianPattern           : (float*float) list
        FullLabeledPattern      : (float*float) list
        CorrectionFactor        : float
    }

let createLabelEfficiencyResult p ps c predLE predP aP mLE mP flP cf =
    {
        Protein                     = p
        PeptideSequence             = ps
        Charge                      = c
        PredictedLabelEfficiency    = predLE
        PredictedPattern            = predP
        ActualPattern               = aP
        MedianLabelEfficiency       = mLE
        MedianPattern               = mP
        FullLabeledPattern          = flP
        CorrectionFactor            = cf
    }




let getCorrectionFactors (medianPredictedLabelEfficiency:float) (predictors: LabelEfficiencyPredictor []) =
    predictors
    |> Array.map 
       (fun lePredictor ->

           let n15Minus1Mz, n15Minus1Quant =
               lePredictor.ExperimentalDistribution.[0]

           let n15Mz, n15Quant =
               lePredictor.ExperimentalDistribution.[1]

           let formulaWithH2O =
               lePredictor.PeptideSequence
               |> BioArray.ofAminoAcidString
               |> BioSeq.toFormula
               |> Formula.add Formula.Table.H2O

           let predictedWithMedianLE =
               formulaWithH2O
               |> initlabelN15Partial medianPredictedLabelEfficiency
               |> generateIsotopicDistributionOfFormulaBySum lePredictor.Charge

           let predictedWithMedianLENorm = 
               formulaWithH2O
               |> initlabelN15Partial medianPredictedLabelEfficiency
               |> generateIsotopicDistributionOfFormulaByMax lePredictor.Charge

           let predictedWithFullLE = 
               formulaWithH2O
               |> initlabelN15Partial 0.99999
               |> generateIsotopicDistributionOfFormulaBySum lePredictor.Charge

           let predictedWithFullLENorm =
               formulaWithH2O
               |> initlabelN15Partial 0.99999
               |> generateIsotopicDistributionOfFormulaByMax lePredictor.Charge

           let n15ProbWithMedianLE =
               predictedWithMedianLE
               |> List.minBy   
                   (fun (mz,prob) -> abs (mz - n15Mz))

           let n15ProbWithFullLE =
               predictedWithFullLE
               |> List.minBy   
                   (fun (mz,prob) -> abs (mz - n15Mz))

           let correctionFactor = 
               snd n15ProbWithFullLE / snd n15ProbWithMedianLE

           createLabelEfficiencyResult
               lePredictor.Protein
               lePredictor.PeptideSequence
               lePredictor.Charge
               lePredictor.PredictedLabelEfficiency
               lePredictor.PredictedDistribution
               (
                   lePredictor.ExperimentalDistribution
                   |> List.unzip
                   |> fun (x,y) ->
                       List.zip
                           x
                           (
                               y
                               |> fun vals -> 
                                   let max = List.max vals
                                   vals
                                   |> List.map (fun v -> v / max)
                           )
               )
               medianPredictedLabelEfficiency
               predictedWithMedianLENorm
               predictedWithFullLENorm
               correctionFactor

       )


let plotLabelEfficiencyResult (leRes: LabelEfficiencyResult) =
    [
        leRes.FullLabeledPattern
        |> List.map (fun (x,y) -> [(x,0.);(x,y);(x,0.)])
        |> List.concat
        |> Chart.Line
        |> Chart.withTraceName ("Fully Labeled Pattern")
        |> Chart.withLineStyle(Color="lightgray",Width = 20)

        leRes.MedianPattern
        |> List.map (fun (x,y) -> [(x,0.);(x,y);(x,0.)])
        |> List.concat
        |> Chart.Line
        |> Chart.withTraceName (sprintf "CorrectedPattern @ Median LE of %.3f" leRes.MedianLabelEfficiency)
        |> Chart.withLineStyle(Width = 10,Color="lightgreen")

        leRes.PredictedPattern
        |> List.map (fun (x,y) -> [(x,0.);(x,y);(x,0.)])
        |> List.concat
        |> Chart.Line
        |> Chart.withTraceName (sprintf "PredictedPattern @ %.3f LE" leRes.PredictedLabelEfficiency)
        |> Chart.withLineStyle(Color="orange",Width = 5)

        Chart.Point(leRes.ActualPattern,Name="Experimental Values")
        |> Chart.withMarkerStyle(Size = 15,Symbol = StyleParam.Symbol.X, Color = "lightred")



    ]
    |> Chart.Combine
    |> Chart.withTitle (sprintf "[%s] : %s @ z = %i" leRes.Protein leRes.PeptideSequence leRes.Charge)
    |> Chart.withX_Axis (xAxis false "m/z" 20 16)
    |> Chart.withY_Axis (yAxis false "normalized probability" 20 16 (0.,1.1))
    |> Chart.withConfig config


let labelEfficiencyResultsFinal =
    labelEfficiency
    |> Array.map 
        (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) ->
            createLabelEfficiencyPredictor prot peptideSequence charge dist le experimentalDist    
        )
    |> getCorrectionFactors filteredOverallPredictedLabelEfficiency

labelEfficiencyResultsFinal
|> Array.map plotLabelEfficiencyResult
|> Array.item 7
|> Chart.withSize (1000.,1000.)
|> Chart.Show


labelEfficiencyResultsFinal
|> Array.map plotLabelEfficiencyResult
|> Array.map (Chart.withSize (1000.,1000.))
|> Array.map Chart.Show


let labelEfficiencyFrame =
    labelEfficiencyResultsFinal
    |> Array.map 
        (fun leRes ->
            (leRes.Protein,(leRes.PeptideSequence,leRes.Charge)) => 
                series 
                    [
                        "PredictedLabelEfficiency"      => leRes.PredictedLabelEfficiency
                        "CorrectionFactor"              => leRes.CorrectionFactor
                    ]
        )
    |> frame
    |> Frame.transpose
    |> fun x ->
        let predDistCol =
            labelEfficiencyResultsFinal
            |> Array.map 
                (fun leRes -> (leRes.Protein,(leRes.PeptideSequence,leRes.Charge)) => leRes.PredictedPattern)
            |> series

        let actualDistCol = 
            labelEfficiencyResultsFinal
            |> Array.map 
                (fun leRes -> (leRes.Protein,(leRes.PeptideSequence,leRes.Charge)) => leRes.ActualPattern)
            |> series

        let medianLECol = 
            labelEfficiencyResultsFinal
            |> Array.map 
                (fun leRes -> (leRes.Protein,(leRes.PeptideSequence,leRes.Charge)) => leRes.MedianLabelEfficiency)
            |> series

        let medianDistCol =
            labelEfficiencyResultsFinal
            |> Array.map 
                (fun leRes -> (leRes.Protein,(leRes.PeptideSequence,leRes.Charge)) => leRes.MedianPattern)
            |> series

        let fullLabeDist = 
            labelEfficiencyResultsFinal
            |> Array.map 
                (fun leRes -> (leRes.Protein,(leRes.PeptideSequence,leRes.Charge)) => leRes.FullLabeledPattern)
            |> series

        x
        |> Frame.addCol "predictedIsotopicDistribution" predDistCol
        |> Frame.addCol "ExperimentallyObservedIsotopicPattern" actualDistCol
        |> Frame.addCol "MedianLabelEfficiency" medianLECol
        |> Frame.addCol "IsotopicDistributionAtMedianLabelEfficiency" medianDistCol
        |> Frame.addCol "IsotopicDistributionAt100%LabelEfficiency" fullLabeDist
        |> Frame.sortRowsByKey
        |> Frame.sortColsByKey

//let labelEfficiencyCorrectionFactorsOnly : Series<(string * string),float>  =
//    labelEfficiencyFrame
//    |> Frame.applyLevel (fun (a,(b,_)) -> a,b) Stats.mean
//    |> Frame.getCol "CorrectionFactor"


//let allLE =
//    let tmp =  
//        labelEfficiency
//        |> Frame.dropSparseRows
//    tmp
//    |> Frame.toArray2D
//    |> Array2D.toJaggedArray
//    |> Array.concat
//    |> Array.zip (tmp.RowKeys |> Array.ofSeq)
     
////Outlier detection to discard
//let outlierBorders = FSharp.Stats.Testing.Outliers.tukey 3. (allLE |> Array.map snd)

////Boxplot with outlier borders
//allLE
//|> Array.map snd
//|> fun x -> 
//    Chart.BoxPlot
//        (x=x,Jitter = 0.1,Boxpoints=StyleParam.Boxpoints.All)
//|> Chart.withShapes 
//    [
//        (Shape.init(StyleParam.ShapeType.Line, X0 = outlierBorders.Upper, X1 = outlierBorders.Upper, Y0 = -0.4, Y1 = 0.4))
//        (Shape.init(StyleParam.ShapeType.Line, X0 = outlierBorders.Lower, X1 = outlierBorders.Lower, Y0 = -0.4, Y1 = 0.4))
//    ]
//|> Chart.withX_Axis (yAxis false "N15 / N15 - 1 m/z Peak instensity ratio" 20 16 (0.25, 1.25))
//|> Chart.withY_Axis (xAxis false "" 20 16)
//|> Chart.withTitle "Label efficiency - Outlier detection"
//|> Chart.withSize (1000.,400.)
//|> Chart.withConfig config
//|> Chart.ShowWithDescription
//    {Heading = "Borders From tukey outlier detection with k = 3:"; Text = sprintf "Upper: %.3f <br></br>Lower: %.3f" outlierBorders.Upper outlierBorders.Lower}

/////Outlier Peptides in Label efficiency
//let outliersInLE =
//    allLE
//    |> Array.filter (fun (keys,eff) -> eff > outlierBorders.Upper || eff < outlierBorders.Lower)

/////Label efficiency mean value without outliers
//let filteredOverallLabelEfficiency =
//    allLE
//    |> Array.filter (fun (keys,eff) -> eff < outlierBorders.Upper && eff > outlierBorders.Lower)
//    |> Seq.meanBy snd

