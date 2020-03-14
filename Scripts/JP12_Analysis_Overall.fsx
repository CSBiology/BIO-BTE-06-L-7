open BioFSharp.Elements

#load @"../IfSharp/References.fsx"
#load @"../IfSharp/Paket.Generated.Refs.fsx"
#load @"../AuxFsx/ProtAux.fsx"
#load @"JP12_Analysis_BN.fsx"
#load @"JP12_Analysis_IGD.fsx"
#load @"JP12_Analysis_LabelEfficiency.fsx"
#load @"JP12_Analysis_WC.fsx"

open BioFSharp
open Deedle
open FSharpAux
open FSharp.Stats
open FSharp.Plotly

open JP12_Analysis_BN
open JP12_Analysis_IGD
open JP12_Analysis_LabelEfficiency
open JP12_Analysis_WC

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
//============================= 5. Overall =======================================
//================================================================================


let overallRatioPlot =
    
    
    let wholeCell =
    
        let rbcl =
            rbcl_RatiosS_wholeCell
            |> Frame.toArray2D
            |> JaggedArray.ofArray2D
            |> Array.concat
    
        let rbcs =
            rbcs_RatiosSdsIgd_wholeCell
            |> Frame.toArray2D
            |> JaggedArray.ofArray2D
            |> Array.concat

        Array.zip rbcl rbcs
        |> Array.map (fun (l,s) -> (l/s))
    
    let SDS_Igd =
    
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
    

        Array.zip rbcl rbcs 
        |> Array.map 
            (fun (rbcl,rbcs) ->
                (rbcl/rbcs)
            )
    
    let BN =
    
        

        let rbcl =
            rbcl_RatiosBN
            |> Frame.toArray2D
            |> JaggedArray.ofArray2D
            |> Array.concat
    
        let rbcs =
            rbcs_RatiosBN
            |> Frame.toArray2D
            |> JaggedArray.ofArray2D
            |> Array.concat
    
        [|rbcl.[0] / rbcs.[0]|]

    [
        Chart.BoxPlot
            (
                x = BN,
                Name = "Native Complexes by BlueNative",
                Jitter = 0.3,
                Boxpoints = StyleParam.Boxpoints.All

            )
        Chart.BoxPlot
            (
                x = SDS_Igd,
                Name = "Separated Subunits by SDS Page",
                Jitter = 0.3,
                Boxpoints = StyleParam.Boxpoints.All
            )
        Chart.BoxPlot
            (
                x = wholeCell,
                Name = "Whole Cell Extracts",
                Jitter = 0.3,
                Boxpoints = StyleParam.Boxpoints.All
            )
    ]

overallRatioPlot
    |> Chart.Combine
    |> Chart.withLegend false
    |> Chart.withTitle "relative Quantification ratios (rbcL/RBCS2) - Comparison between all experiments"
    |> Chart.withX_Axis (yAxis false "relative quantification (rbcL/rbcS)" 20 16 (0.75 ,1.75))
    |> Chart.withY_Axis (xAxis false "" 20 16)
    |> Chart.withConfig config
    |> Chart.withSize (1000.,700.)
    |> Chart.withMarginSize 300.
    |> Chart.Show

//==================================================================
//================= Absolute Quantification ========================
//==================================================================

let qConcatSequence =
    "MASMTGGQQMGRDPSRSALPSNWKSVLPANWRDTDILAAFREVTLGFVDLMRFLFVAEAIYKLTYYTPDYVVRAYVSNESAIRLVAFDNQKYWTMWKAFPDAYVRVPLILGIWGGKIGQQLVNARSLVDEQENVKLGADSGALEFVPKDDYLNAPGETYSVKTPLANLVYWKALYGFDFLLSSKTNFGIGHRLSIFETGIKTAPAFVDLDTRIPAGPDLIVKNILVVGPVPGKIVAITALSEKYPIYFGGNRVLNTWADIINREWELSFRNTWADIINRLIFQYASFNNSRTALPADWRLVFPEEVLPRNILLNEGIRTWFDDADDWLRAAHHHHHHHKLAAALEHHHHHH"
    |> BioArray.ofAminoAcidString

let n15Amount =
    qConcatSequence
    |> BioSeq.toFormula
    |> initlabelN15Partial 0.998
    |> Formula.averageMass

let qConCatMolecularMass =
    qConcatSequence
    |> BioArray.toMonoisotopicMass
    |> fun x -> x + n15Amount

[<AutoOpen>]
module IsotopicDistribution =
    let ofElement (e: Element) =
            match e with
            | Mono  { X = x; Xcomp = xcomp} -> 
                [(1.,x)]
            | Di    
                {   
                    X       = x; 
                    Xcomp   = xcomp; 
                    X1      = x1; 
                    X1comp  = x1comp
                } -> 
                    [
                        (xcomp,x)
                        (x1comp,x1)
                    ] 

            | Tri   
                {   
                    X       = x; 
                    Xcomp   = xcomp; 
                    X1      = x1; 
                    X1comp  = x1comp
                    X2      = x2; 
                    X2comp  = x2comp
                } -> 
                    [
                        (xcomp,x)
                        (x1comp,x1)
                        (x2comp,x2)
                    ] 
            | Multi 
                {   
                    X       = x; 
                    Xcomp   = xcomp; 
                    X1      = x1; 
                    X1comp  = x1comp
                    X2      = x2; 
                    X2comp  = x2comp
                    XL      = rest
                } -> 
                    [
                        (xcomp,x)
                        (x1comp,x1)
                        (x2comp,x2)
                    ] @ (rest |> Array.map (fun x -> x.NatAbundance, x) |> Array.toList)

//Atomic Mass: 

//corresponds to standard atomic weight if natural abundances are used
//weighted average of 
let getAtomicWeight (e: Element) =
    e
    |> ofElement
    |> List.map (fun (ab,iso) -> ab,iso.Mass)
    |> List.sumBy 
        (fun (abundance,atomicMass) ->
            abundance * atomicMass
        ) 


//corresponds to standard atomic weight if natural abundances are used
//weighted average of 
let getAverageAtomicWeight (f: Formula.Formula) =
    f 
    |> Seq.sumBy 
        (fun elem -> getAtomicWeight elem.Key * elem.Value)

Elements.Table.Se
|> getAtomicWeight

Formula.parseFormulaString "N"
|> Formula.averageMass

Formula.parseFormulaString "N"
|> getAverageAtomicWeight

Formula.parseFormulaString "N"
|> initlabelN15Partial 0.998
|> Formula.monoisoMass

Formula.parseFormulaString "N"
|> initlabelN15Partial 0.998
|> getAverageAtomicWeight


//================================================================================
//============================= 6. Absolute Quantification =======================================
//================================================================================

[<Measure>] type g 
[<Measure>] type mg 
[<Measure>] type ug 
[<Measure>] type ng 
[<Measure>] type pg 
[<Measure>] type fg 

[<Measure>] type mol
[<Measure>] type mmol
[<Measure>] type umol
[<Measure>] type nmol
[<Measure>] type pmol
[<Measure>] type fmol

[<Measure>] type l
[<Measure>] type ml
[<Measure>] type ul
[<Measure>] type nl
[<Measure>] type pl
[<Measure>] type fl

[<Measure>] type cell


let qConCatMW =
    qConcatSequence
    |> BioSeq.toFormula
    |> initlabelN15Partial 0.998
    |> getAverageAtomicWeight 
    |> fun x -> x * 1.<g/mol>


"FLFVAEAIYK"
|> BioArray.ofAminoAcidString
|> BioSeq.toFormula
|> getAverageAtomicWeight


let qConCatGramsToMol (qcg:float<g>) =
    qcg / qConCatMW

let isdCellConcentration =
    7280000.<cell/ml>

let isdCellCount =
    isdCellConcentration * (15.1<ul> / 1000.<ul/ml> )

let isdQMass =
   
        "AddedQProtein[uq]" =>
            series
                [
                    10. => 0.25<ug>
                    5.  => 0.5<ug>
                    1.  => 2.5<ug>
                    0.5 => 5.<ug>
                ]


let isdQMole =
        "AddedQProtein[pmol]" =>
            series
                [
                    10. => ((0.25<ug> * (0.000001<g/ug>)) |> qConCatGramsToMol |> fun x -> (x * 1000000000000.<pmol/mol>))
                    5.  => ((0.5<ug>  * (0.000001<g/ug>)) |> qConCatGramsToMol |> fun x -> (x * 1000000000000.<pmol/mol>))
                    1.  => ((2.5<ug>  * (0.000001<g/ug>)) |> qConCatGramsToMol |> fun x -> (x * 1000000000000.<pmol/mol>))
                    0.5 => ((5.<ug>   * (0.000001<g/ug>)) |> qConCatGramsToMol |> fun x -> (x * 1000000000000.<pmol/mol>))
                ]
    
0.25<ug> * (0.000001<g/ug>) 
|> qConCatGramsToMol 
|> fun x -> (x * 1000000000000.<pmol/mol>) 
|> fun x -> x / isdCellCount

let isdCells =
        "Cell count"=>
            series
                [
                    10. => ( isdCellCount)
                    5.  => ( isdCellCount)
                    1.  => ( isdCellCount)
                    0.5 => ( isdCellCount)
                ]




let wholeCell = 
    rbcl_RatiosS_wholeCell
    |> Frame.merge rbcs_RatiosSdsIgd_wholeCell
    |> Frame.transpose
    |> Frame.addCol 
        "Ratio (rbcL/rbcs)" 
            (
                let rbcl : Series<float,float> = (rbcl_RatiosS_wholeCell |> Frame.transpose |> Frame.getCol "rbcL") 
                let rbcs : Series<float,float> = (rbcs_RatiosSdsIgd_wholeCell |> Frame.transpose |> Frame.getCol "RBCS2")
                rbcl/rbcs
            )

    |> Frame.transpose
    |> Frame.map (fun _ _ v -> Math.round 3 v)



let isd = 
    rbcl_RatiosS_wholeCell
    |> Frame.merge rbcs_RatiosSdsIgd_wholeCell
    |> Frame.transpose
    |> Frame.addCol 
        "Ratio (rbcL/rbcs)" 
            (
                let rbcl : Series<float,float> = (rbcl_RatiosS_wholeCell |> Frame.transpose |> Frame.getCol "rbcL") 
                let rbcs : Series<float,float> = (rbcs_RatiosSdsIgd_wholeCell |> Frame.transpose |> Frame.getCol "RBCS2")
                rbcl/rbcs
            )

    |> Frame.transpose
    |> Frame.map (fun _ _ v -> Math.round 3 v)
    |> Frame.transpose
    |> Frame.addCol (fst isdQMass) (snd isdQMass)
    |> Frame.addCol (fst isdQMole) (snd isdQMole)
    |> fun f ->
        let rbclMol =
            f.["rbcL"] * f.["AddedQProtein[pmol]"]

        let rbclPerCell =
            rbclMol
            |> Series.mapValues (fun v -> (v * 1.<pmol> * 1000.<fmol/pmol>) / isdCellCount)

        let rbcsMol =
            f.["RBCS2"] * f.["AddedQProtein[pmol]"]

        let rbcsPerCell =
            rbcsMol
            |> Series.mapValues (fun v -> (v * 1.<pmol> * 1000.<fmol/pmol>) / isdCellCount)


        f
        |> Frame.addCol (fst isdCells) (snd isdCells)
        |> Frame.addCol "Quantified rbcL [pmol]" rbclMol
        |> Frame.addCol "rbcL per Cell [fmol/cell]" rbclPerCell
        |> Frame.addCol "Quantified RBCS [pmol]" rbcsMol
        |> Frame.addCol "RBCS per Cell [fmol/cell]" rbcsPerCell



isd.SaveCsv(
    path = (source + @"\isdAlResults.txt"),
    includeRowKeys = true,
    keyNames = ["Ratio(14N Sample / 15N QProtein)"],
    separator = '\t'
    )
