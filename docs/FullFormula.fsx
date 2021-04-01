let calcAbsoluteAbundance μgChlorophPerMlCult cellCountPerMlCult μgChlorophPerμlSample μgProtPerμlSample μgQProtSpike μgloadedProtein molWeightQProt molWeightTargetProt ratio1415N =
    let chlorophPerCell : float = μgChlorophPerMlCult / cellCountPerMlCult
    let cellsPerμlSample = μgChlorophPerμlSample / chlorophPerCell
    let μgProteinPerCell = μgProtPerμlSample / cellsPerμlSample
    let molQProtSpike = μgQProtSpike * 10. ** -6. / molWeightQProt
    let molProtPerBand = ratio1415N * molQProtSpike
    let molProtIn1μgLoadedProt = molProtPerBand / μgloadedProtein
    let gTargetProtIn1μgLoadedProt = molWeightTargetProt * molProtIn1μgLoadedProt
    let molProteinPerCell = molProtIn1μgLoadedProt * μgProteinPerCell
    let proteinsPerCell = molProteinPerCell * 6.022 * 10. ** 23.
    let attoMolProteinPerCell = molProteinPerCell * (10. ** 18.)
    let attoMolProteinPerBand = molProtPerBand * (10. ** 18.)
    {|
        MassTargetProteinInLoadedProtein    = gTargetProtIn1μgLoadedProt
        ProteinsPerCell                     = proteinsPerCell
        AttoMolProteinPerCell               = attoMolProteinPerCell
        AttoMolProteinPerBand               = attoMolProteinPerBand
    |}