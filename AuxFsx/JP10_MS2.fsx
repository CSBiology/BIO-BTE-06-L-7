open System
open System.IO

// Include CsbScaffold
#load @"..\IfSharp\Paket.Generated.Refs.fsx"
#load @"..\IfSharp\References.fsx"
// If you want to use the wrappers for unmanaged LAPACK functions from of FSharp.Stats 
// include the path to the .lib folder manually to your PATH environment variable and make sure you set FSI to 64 bit

// use the following lines of code to ensure that LAPACK functionalities are enabled if you want to use them
// fails with "MKL service either not available, or not started" if lib folder is not included in PATH.
//open FSharp.Stats
//FSharp.Stats.Algebra.LinearAlgebra.Service()
 
open BioFSharp
open BioFSharp.Mz

open FSharp.Plotly

open FSharp.Stats
open FSharp.Stats.Fitting
open FSharp.Stats.Fitting.LinearRegression
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

//BASE COLORS: #FF6F61 and #577284

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

type LadderedTaggedMass (iontype:Ions.IonTypeFlag,mass:float, number:int, charge: float) =
    member this.Iontype = iontype
    member this.MassOverCharge = mass
    member this.Number = number
    member this.Charge = charge

type LadderedPeakFamily<'a, 'b> = {
    MainPeak       : 'a
    DependentPeaks : 'b list
}

let createLadderedPeakFamily mainPeak dependentPeaks = {
    MainPeak       = mainPeak
    DependentPeaks = dependentPeaks
}

let ladderAndChargeElement (chargeList: float list) (sortedList: Mz.PeakFamily<Mz.TaggedMass.TaggedMass> list) =
    sortedList
    |> List.mapi (fun i taggedMass ->
        chargeList
        |> List.map (fun charge ->
            let mainPeak = taggedMass.MainPeak
            let dependentPeaks = taggedMass.DependentPeaks
            let newMainPeak = new LadderedTaggedMass(mainPeak.Iontype, Mass.toMZ mainPeak.Mass charge, i + 1, charge)
            let newDependentPeaks =
                dependentPeaks
                |> List.map (fun dependentPeak ->
                    new LadderedTaggedMass(dependentPeak.Iontype, Mass.toMZ dependentPeak.Mass charge, i + 1, charge)
                )
            let newPeakFamily =
                createLadderedPeakFamily newMainPeak newDependentPeaks
            newPeakFamily
        )
    )
    |> List.concat

let ladderElement (chargeList: float list) (ionList: Mz.PeakFamily<Mz.TaggedMass.TaggedMass> list) =
    let groupedList =
        ionList
        |> List.groupBy ( fun x -> 
            x.MainPeak.Iontype)
        |> List.map snd
        |> List.map List.sort
    groupedList
    |> List.collect (ladderAndChargeElement chargeList)

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


let folderPath =
    let source =
        __SOURCE_DIRECTORY__
    let parent =
        Directory.GetParent(source)
    let path = 
        parent.FullName + @"\AuxFiles\DavesTaskData\"
    path

///
let initPaddingParams paddYValue = 
    SignalDetection.Padding.createPaddingParameters paddYValue (Some 7) 0.05 150 95.

///
let initWaveletParametersMS2 paddYValue =
    SignalDetection.Wavelet.createWaveletParameters 10 paddYValue 0.1 90. 1.

///
let ms2PeakPicking (mzData:float []) (intensityData: float []) =
    if mzData.Length < 3 then
        [||],[||]
    else
    let yThreshold = Array.min intensityData
    let paddingParams = initPaddingParams yThreshold
    let paddedMz,paddedIntensity =
        SignalDetection.Padding.paddDataBy paddingParams mzData intensityData
    let waveletParameters = initWaveletParametersMS2 yThreshold
    BioFSharp.Mz.SignalDetection.Wavelet.toCentroidWithRicker2D waveletParameters paddedMz paddedIntensity

///
let ms2 =
    BioFSharp.IO.Mgf.readMgf (folderPath + @"ms2MGF.mgf")
    |> List.item 0

///
let centroidedMs2 = 
    ms2PeakPicking ms2.Mass ms2.Intensity

///
[
    Chart.Point(ms2.Mass,ms2.Intensity)
    Chart.Point(fst centroidedMs2,snd centroidedMs2)
]
|> Chart.Combine
//|> Chart.Show

/// PrecursorMZ that was picked in the MS1 full scan to generate a MS2
let ms2PrecursorMZ = 
    match BioFSharp.IO.Mgf.tryGetPrecursorMZ ms2 with 
    | Some mz -> mz 
    | None    -> 0.

/// This mass can now be used to obtain a peptide search in a database created by in silico digestion of the known proteom of a organism of choice.
/// This search was done beforehand and you can retrieve the result stored in the following file:
let lookUpResult =
     [
        BioFSharp.Mz.SearchDB.createLookUpResult 3439282 302200012 1190.654689 1190654689L "VYVVGEDGILK" (BioList.ofAminoAcidString "VYVVGEDGILK") 0
        BioFSharp.Mz.SearchDB.createLookUpResult 5411223 444660385 1190.670631 1190670631L "AIRGFCQRLK" (BioList.ofAminoAcidString "AIRGFCQRLK") 0
        BioFSharp.Mz.SearchDB.createLookUpResult 2343340 203080049 1190.670631 1190670631L "IGRGLFRNMK" (BioList.ofAminoAcidString "IGRGLFRNMK") 0
     ]

/// additional info about MS2:  https://www.dcbiosciences.com/news/interpreting-peaks-ms2-spectra/
let calcIonSeries massF aal =
    let ys = Fragmentation.Series.yOfBioList massF aal
    let bs = Fragmentation.Series.bOfBioList massF aal
    ys@bs

//BioItem.initMonoisoMassWithMemP

let testAA = BioFSharp.Mz.SearchDB.createLookUpResult 3439282 302200012 1190.654689 1190654689L "VYVVGEDGILK" (BioList.ofAminoAcidString "VYVVGEDGILK") 0

AminoAcids.averageMass testAA.BioSequence.Head

let private chartConfig =
    Config.init(StaticPlot = false, Responsive = true, Editable = true,Autosizable=true,ShowEditInChartStudio=true,ToImageButtonOptions = ToImageButtonOptions.init(Format = StyleParam.ImageFormat.SVG))
let private xAxis title = Axis.LinearAxis.init(Title=title,Showgrid=false,Showline=true,Zeroline=false,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside,Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=26.),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=20.))       
let private yAxis title = Axis.LinearAxis.init(Title=title,Showgrid=false,Showline=true,Zeroline=false,Tickmode=StyleParam.TickMode.Auto,Ticks= StyleParam.TickOptions.Inside,Tickfont=Font.init(StyleParam.FontFamily.Arial,Size=26.),Titlefont=Font.init(StyleParam.FontFamily.Arial,Size=20.)(*,AxisType = StyleParam.AxisType.Log*))

let createMS2Charts (lookUpResult:SearchDB.LookUpResult<AminoAcids.AminoAcid> list)=
    let rnd = System.Random()
    lookUpResult
    |> List.map (fun x -> calcIonSeries BioItem.initMonoisoMassWithMemP x.BioSequence)
    |> List.map (fun x -> ladderElement [1.] x)
    |> List.map (fun laal -> // laddered amino acid list
        laal 
        |> List.map (fun lpf ->
            lpf.MainPeak.MassOverCharge, lpf.MainPeak.Iontype, lpf.MainPeak.Number//, lpf.DependentPeaks |> List.map (fun lpf' -> lpf'.MassOverCharge, lpf'.Iontype, lpf'.Number)
        )
    )
    |> List.map (
        fun peptide ->
            let y,b = peptide |> List.partition (fun (mz,iontype,number) -> iontype = Ions.IonTypeFlag.Y)
            let createChart (ionInformation: (float*Ions.IonTypeFlag*int) list) name =
                let (mzList,nameList) = 
                    ionInformation 
                    |> List.sortBy (fun (mz,iontype,number) -> number)
                    |> List.map (fun (mz,iontype,number) -> mz, sprintf "%s%i" (iontype.ToString()) number)
                    |> List.unzip
                let rndList = List.init mzList.Length (fun _ -> rnd.Next(4575,56867))
                let column =
                    let dyn = Trace("bar")
                    dyn?x <- mzList
                    dyn?y <- rndList
                    dyn?text <- nameList
                    dyn?name <- name
                    dyn?textposition <- "auto"
                    dyn?width <- 10
                    dyn?opacity <- 0.8
                    dyn?textposition <- "outside"
                    dyn?fontsize <- 20.
                    dyn?constraintext <- "inside"
                    dyn
                GenericChart.ofTraceObject column
                |> Chart.withX_AxisStyle("m/z",MinMax=((List.min mzList) - 100.,(List.max mzList) + 100.))
            [
                createChart y "y ions"
                createChart b "b ions"
            ] 
            |> Chart.Combine 
            |> Chart.withConfig chartConfig
            |> Chart.withY_Axis (yAxis "Random Intensity")
            |> Chart.withX_Axis (xAxis "m/z")
            |> Chart.withSize (900.,600.)
    )
