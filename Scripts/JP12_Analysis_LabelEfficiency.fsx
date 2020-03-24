
#load @"../IfSharp/References.fsx"
#load @"../IfSharp/Paket.Generated.Refs.fsx"
#load @"../AuxFsx/ProtAux.fsx"
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

let labelEfficiencyNameMapping = 
    [
    "G1 q1 zu 0.00032"  =>  ("LabelEfficiency", ("15N Q + 14N Q",           0.00032 ))
    "G1 q1 zu 0.0016"   =>  ("LabelEfficiency", ("15N Q + 14N Q",           0.0016  ))
    "G1 q1 zu 0.008"    =>  ("LabelEfficiency", ("15N Q + 14N Q",           0.008   ))
    "G1 q1 zu 0.04"     =>  ("LabelEfficiency", ("15N Q + 14N Q",           0.04    ))
    "G1 q1 zu 0.2"      =>  ("LabelEfficiency", ("15N Q + 14N Q",           0.2     ))
    "G1 Q1 zu 1zu 1"    =>  ("LabelEfficiency", ("15N Q + 14N Q + 13C Q",   1.      ))
    //"DataLabelingEfficiency1"                           => ("LabelEfficiency",("15N IRT + 14N IRT","3"))
    //"DataLabelingEfficiency2"                           => ("LabelEfficiency",("15N Q + 15N IRT"  ,"3"))
    //"DataLabelingEfficiency3"                           => ("LabelEfficiency",("14N Q + 15N IRT"  ,"3"))
    //"Data00VP2018_Gr4_LabelEfficiency4"                 => ("LabelEfficiency",("15N Q + 14N Q + 13C Q","4"))
    //"Data00VP2018_Gr4_LabelEfficiency5"                 => ("LabelEfficiency",("15N Q","4"))
    //"Data00VP2018_Gr4_LabelEfficiency6"                 => ("LabelEfficiency",("15N Q + 14N Q","4"))
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

let labelEfficiencyResults : Frame<string*(string*(string*float)),(string*(string*int))> = 
    readQConcatResultFrame (source + @"\..\AuxFiles\GroupsData\G1_Q1_zu_1zu_1_QuantifiedPeptides.txt")
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

type LabelEffCollector = 
    {
        Dilution: float
        N15Minus1Quant: float
        N15Quant:float
        N15Minus1MOverZ: float
        N15MOverZ: float
    }
    
    static member create n15Minus1quant n15quant n15Minus1MOverZ n15MOverZ dilution = {
        Dilution            = dilution
        N15Minus1Quant      = n15Minus1quant
        N15Quant            = n15quant
        N15Minus1MOverZ     = n15Minus1MOverZ
        N15MOverZ           = n15MOverZ
    }

let dilutionArr = 
    [|0.00032;0.0016;0.008;0.04;0.2;1.|]

let linearityTest =
    labelEfficiencyResults
    |> Frame.dropSparseCols
    |> fun x -> 
        x
        |> Frame.toArray2D
        |> Array2D.toJaggedArray
        |> JaggedArray.transpose
        |> Array.zip (x.ColumnKeys |> Array.ofSeq)
        //Calculate fully/uncompletely labeled peak ratio
        |> Array.map 
            (fun ((prot,(pepSeq,charge)),values) -> 
                prot,
                // we use multiple dilutions of N15 and we can calculate a label efficiency for each of them and
                // take the mean in the end, as all originate from the same QProtein sample. This could also
                // show differences in measured label efficiency for high or low sample quantities.
                Array.init
                    dilutionArr.Length
                    (fun ind ->
                        [|for num in ind .. dilutionArr.Length .. values.Length-1 do
                            yield values.[num]|]
                    )
                |> Array.mapi (fun i values -> 
                    // N14/N15, dilution
                    values.[0]/values.[4], 1./dilutionArr.[i]
                )
                |> fun x ->
                    let (n15n14,dilution) = Array.unzip x
                    let name = sprintf "[%s] : %s @ z = %i" prot pepSeq charge
                    Chart.Line(dilution,n15n14,Name=name)
            )
        |> Array.groupBy (fun x -> fst x)
        |> Array.map (fun (header,chartArr) ->
            let charts = chartArr |> Array.map snd
            charts
            |> Chart.Combine
            |> Chart.withY_Axis (yAxis false "N14 / N15 Peak instensity ratio" 20 16)
            |> Chart.withX_Axis (xAxis false "1:x Dilution of N15 Q Protein" 20 16)
        )
    |> Array.map Chart.Show


let labelEfficiency =
    labelEfficiencyResults
    |> Frame.filterRows 
        (fun (rk,_) _ -> not (rk.Contains("N14")))
    |> Frame.dropSparseCols
    |> fun x -> 
        x
        |> Frame.toArray2D
        |> Array2D.toJaggedArray
        |> JaggedArray.transpose
        |> Array.zip (x.ColumnKeys |> Array.ofSeq)
        //Calculate fully/uncompletely labeled peak ratio
        //|> Array.find (fun x -> (fst >> fst) x = "rbcL")
        |> Array.map 
        //|>
            (fun (key,values) -> 
                key,
                // we use multiple dilutions of N15 and we can calculate a label efficiency for each of them and
                // take the mean in the end, as all originate from the same QProtein sample. This could also
                // show differences in measured label efficiency for high or low sample quantities.
                Array.init
                    dilutionArr.Length
                    (fun ind ->
                        [|for num in ind .. dilutionArr.Length .. values.Length-1 do
                            yield values.[num]|]
                    )
                |> Array.mapi (fun i values -> 
                    LabelEffCollector.create values.[1] values.[3] values.[0] values.[2] dilutionArr.[i]
                )
            )
        |> Array.collect 
        //|>
            (fun ((prot,(peptideSequence,charge)),labelEffCollectorArr) -> //(n15Minus1Quant,n15Quant),n15Minus1MZ,n15Mz

                let calculateLabelEffs (labelEffCollect:LabelEffCollector) =
                
                    let peakRatio = labelEffCollect.N15Minus1Quant / labelEffCollect.N15Quant //n15Minus1Quant / n15Quant

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
                                            // old: abs (n15Mz - mz)
                                            abs (labelEffCollect.N15MOverZ - mz)
                                        )
                                    |> snd
                                
                                let n15Minus1Prob = 
                                    dist
                                    |> List.minBy 
                                        (fun (mz,prob) ->
                                            //old: abs (n15Minus1MZ - mz)
                                            abs (labelEffCollect.N15Minus1MOverZ - mz)
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
                    labelEffCollect,
                    // old
                    //(prot,(peptideSequence,charge)),
                    //[(n15Minus1MZ,n15Minus1Quant);(n15Mz,n15Quant)],
                    bestLE

                labelEffCollectorArr
                |> Array.map calculateLabelEffs
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
        Boxpoints=StyleParam.Boxpoints.All,
        Name="Measured Label <br> Efficiencies"
        )
    Chart.BoxPlot(x=x,Jitter = 0.3,Boxpoints=StyleParam.Boxpoints.All,Name="Measured Label <br> Efficiencies - <br> without outliers")
    |> Chart.withShapes 
        [
            (Shape.init(StyleParam.ShapeType.Line, X0 = outlierBorders.Upper, X1 = outlierBorders.Upper, Y0 = -0.4, Y1 = 1.4))
            (Shape.init(StyleParam.ShapeType.Line, X0 = outlierBorders.Lower, X1 = outlierBorders.Lower, Y0 = -0.4, Y1 = 1.4))
        ]
    ]
|> Chart.Combine
|> Chart.withX_Axis (yAxis false "N15 / N15 - 1 m/z Peak instensity ratio" 20 16)
|> Chart.withY_Axis (xAxis false "" 20 16)
|> Chart.withMargin (Margin.init(Left=200))
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
|> Array.groupBy (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) -> prot,peptideSequence,charge)
|> Array.map 
    (fun ((prot,peptideSequence,charge),arr) ->
        
        prot,
        peptideSequence,
        arr
        |> Array.sortByDescending (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) -> experimentalDist.Dilution)
        |> Array.map (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) ->
            let normExperimental =
                experimentalDist
                //[(n15Minus1MZ,n15Minus1Quant);(n15Mz,n15Quant)]
                //|> List.unzip
                //[n15Minus1MZ;n15Mz],[n15Minus1Quant;n15Quant]
                |> fun x -> // (x,y)
                    List.zip
                        [x.N15Minus1MOverZ;x.N15MOverZ]
                        (
                            [x.N15Minus1Quant; x.N15Quant]
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
                Chart.Point(normExperimental,Name= sprintf "[%s] %s : Experimental Masses 1 to %s"  prot peptideSequence (string experimentalDist.Dilution))
            ]
            |> Chart.Combine
            |> Chart.withTitle (sprintf "[%s] : %s @ + %i" prot peptideSequence charge )
            |> Chart.withX_Axis (xAxis false "" 20 16)
            |> Chart.withY_Axis (yAxis false "" 20 16)
            |> Chart.withConfig config 
        )
    )
    |> Array.map ( fun (prot,pepSeq,charts) ->
        charts
        |> Chart.Stack charts.Length
        |> Chart.withSize (1500.,600.)
        |> Chart.Show
    )

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
        EFCollector: LabelEffCollector
    }

let createLabelEfficiencyPredictor p ps c pred eff exp efCollector=
    {
        Protein                     = p
        PeptideSequence             = ps
        Charge                      = c
        PredictedDistribution       = pred
        PredictedLabelEfficiency    = eff
        ExperimentalDistribution    = exp
        EFCollector                 = efCollector
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
        EFCollector             : LabelEffCollector
    }

let createLabelEfficiencyResult p ps c predLE predP aP mLE mP flP cf efCollector=
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
        EFCollector                 = efCollector
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
               lePredictor.EFCollector
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
    |> Chart.withTitle (sprintf "[%s] : %s @ z = %i & Dil = %s" leRes.Protein leRes.PeptideSequence leRes.Charge (string leRes.EFCollector.Dilution))
    |> Chart.withX_Axis (xAxis false "m/z" 20 16)
    |> Chart.withY_Axis (yAxis false "normalized probability" 20 16)
    |> Chart.withConfig config


let labelEfficiencyResultsFinal =
    labelEfficiency
    |> Array.map 
        (fun ((prot,(peptideSequence,charge)),experimentalDist,(le,ratio,dist)) ->
            let prepExperimentalDist =
                [
                    experimentalDist.N15Minus1MOverZ, experimentalDist.N15Minus1Quant;
                    experimentalDist.N15MOverZ, experimentalDist.N15Quant
                ]
            createLabelEfficiencyPredictor prot peptideSequence charge dist le prepExperimentalDist experimentalDist
        )
    |> getCorrectionFactors filteredOverallPredictedLabelEfficiency

labelEfficiencyResultsFinal
|> Array.map plotLabelEfficiencyResult
|> Array.item 7
|> Chart.withSize (1000.,1000.)
|> Chart.Show


labelEfficiencyResultsFinal
|> fun x -> x.[0 .. 7]
|> Array.map plotLabelEfficiencyResult
|> Array.map (Chart.withSize (1000.,1000.))
|> Array.map Chart.Show


let labelEfficiencyFrame =
    labelEfficiencyResultsFinal
    |> Array.map 
        (fun leRes ->
            (leRes.Protein,(leRes.PeptideSequence, leRes.Charge, leRes.EFCollector.Dilution)) => 
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
                (fun leRes -> (leRes.Protein,(leRes.PeptideSequence,leRes.Charge, leRes.EFCollector.Dilution)) => leRes.PredictedPattern)
            |> series

        let actualDistCol = 
            labelEfficiencyResultsFinal
            |> Array.map 
                (fun leRes -> (leRes.Protein,(leRes.PeptideSequence,leRes.Charge, leRes.EFCollector.Dilution)) => leRes.ActualPattern)
            |> series

        let medianLECol = 
            labelEfficiencyResultsFinal
            |> Array.map 
                (fun leRes -> (leRes.Protein,(leRes.PeptideSequence,leRes.Charge, leRes.EFCollector.Dilution)) => leRes.MedianLabelEfficiency)
            |> series

        let medianDistCol =
            labelEfficiencyResultsFinal
            |> Array.map 
                (fun leRes -> (leRes.Protein,(leRes.PeptideSequence,leRes.Charge, leRes.EFCollector.Dilution)) => leRes.MedianPattern)
            |> series

        let fullLabeDist = 
            labelEfficiencyResultsFinal
            |> Array.map 
                (fun leRes -> (leRes.Protein,(leRes.PeptideSequence,leRes.Charge, leRes.EFCollector.Dilution)) => leRes.FullLabeledPattern)
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

