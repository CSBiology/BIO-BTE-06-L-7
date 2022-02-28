(**
# NB06a Research Data Management

[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/CSBiology/BIO-BTE-06-L-7/gh-pages?filepath=NB06a_Research_Data_Management.ipynb)

[Download Notebook](https://github.com/CSBiology/BIO-BTE-06-L-7/releases/download/NB06a/NB06a_Research_Data_Management.ipynb)

1. ARC
2. ISA-Xlsx
    1. Structure of ISA-Xlsx
3. Swate
4. Editing an ARC

*)

(**
The NFDI (Nationale Forschungsdateninfrastruktur) is a german infrastructure that targets standardization of Research Data Management (RDM). The CSB group is part of DataPLANT, a consortium within the NFDI, which tackles the field of basic plant research. To achieve the already introduced FAIR principles, DataPLANT envisioned the **Annotated Research Context** (short: **ARC**\).

# ARC

![](https://raw.githubusercontent.com/CSBiology/BIO-BTE-06-L-7/main/docs/img/ARC.png)

The ARC is, basically, a folder and file structure. Its design shall cover the entire research cycle, from experimental to computational analyses, actual data and metadata, as well as resulting publications.

![](https://github.com/CSBiology/BIO-BTE-06-L-7/raw/main/docs/img/ARCfolderStructure.jpg)  
***Figure 8: An exemplary ARC, schematically visualized***

Some parts of the ARC are an implementation of the ISA model you have seen before, e.g. the _Investigation_ file, _Study_ files, and the _assays_ folder.  
In the _assays_ folder, all the assays of the research project are stored. Each assay has its own folder whose foldername represents the _AssayID_. In Figure 1, this is "proteomics". In each assay's folder are the subfolders _dataset_ (which stores all corresponding datasets) and _protocol_ (for ISA protocol information), as well as this assay's _Assay_ file whose name is always "assay.isa.xlsx".  
The _externals_ folder consists of files that are not directly part of any other ARC structure but related to the ARC in some way.  
In the _workflows_ folder, computational workflows are stored. Here (Figure 8), it is a script named "norm", written in the programming language python and located in the "PythonCapsule" folder. This is the directory where you could, e.g., place your Jupyter Notebooks.  
The results of computational evaluations and calculations are placed in the _runs_ folder, where each subfolder represents a single computational run.  
Finally, there's the _Investigation_ file which is located in the ARC's root folder. In this file, all the information about the ARC itself and its research project are contained. That is, e.g., the names of the researchers involved, the name of the group, etc.

# ISA-Xlsx

The _Assay_ files are, from an experimenter's perspective, the most important files of each ARC since they contain the information about

  - the sample that was used
  - the condition of the sample, i.e. how the sample was treated
  - the way how the data was obtained from the sample

In each _Assay_ file, the metadata of the respective experiment is documented thoroughly. To do so, the Xlsx file format is suitable. The common way to work with Xlsx files are spreadsheet programs like Microsoft Excel or LibreOffice.  

## The structure of ISA-Xlsx

A practical implementation of the [ISA standard](https://isa-specs.readthedocs.io/en/latest/isamodel.html) is [ISA-Xlsx](https://github.com/nfdi4plants/ARC-specification/blob/main/ARC%20specification.md#isa-xlsx-format). In this, the [format of the Assay files](https://isa-specs.readthedocs.io/en/latest/isatab.html#study-and-assay-files) served as the blueprint. Here, ISA is realized in a very similar manner.

An _Assay_ file consists of at least one annotation table (like in Figure 9). 1 annotation table per Xlsx worksheet is allowed. Each annotation table represents an ISA process which is the depiction of a sequence of steps from the input to the output.  
Annotation tables are row-oriented. This means that the first row consists entirely of each column's header. Annotation tables are ordered from left to right and always begin with a source column ("Source Name") and end with a sink column ("Sample Name" or "Data File Name"). In between are _Building Blocks_ that consist of an ISA term ("Characteristics", "Parameter", or "Factor") and an ontology term in square brackets.  
The cells below the headers are the values.

![](https://github.com/CSBiology/BIO-BTE-06-L-7/raw/main/docs/img/ExemplaryAssayFile.png)  
***Figure 9: An annotation table of an exemplary Assay file.***

The different types of columns:

- the source column: **Source Name**  
The values of this column describe the name of the _input_ of the process. This can be, in a lab experiment, the name of the biological source, e.g. the name of a specimen in a petri dish.
- the sink columns
  - **Sample Name**  
    A Sample Name describes the name of the _output_ of a process. For example, if a process ends in the specimen being stored in an eppendorf tube, it could be the name of this eppi.
  - **Data File Name**  
    Like the Sample Name except that it is used when the output is a file. The whole file's name shall be used as Data File Name, that also includes the file extension.
- the _Building Blocks_:
  - **Characteristics**  
    Characteristics are _Building Blocks_ that _describe the specimen_ used, e.g. the species, the strain, the genotype, the number of leaves (if it is a plant), its mass, etc.
  - **Parameter**  
    _Building Blocks_ of this type _describe a method being applied to the specimen_, e.g. the number of hours in daylight a plant is exposed to per day, the weight of the food a mouse is fed with per meal, a specific condition we apply to our specimen, the type of machine we use to get a result from our specimen, this machine's settings, etc.
  - **Factor**  
    Factors are specific _Building Blocks_ that can be Parameters of Characteristics of special importance in this experiment or research project. We won't use them in this practical course.

![](https://github.com/CSBiology/BIO-BTE-06-L-7/raw/main/docs/img/ParameterBuildingBlock_filled.png)  
***Figure 11: Building Block "Parameter [instrument model]", filled with values.*** 

Ideally, an annotation table describes its experiment's process chronologically so that a researcher trying to replicate it can easily do so. The worksheets an _Assay_ file consists of should be ordered chronologically, too.  

While you could do all this data entry by hand, it is much easier if you use a tool that automatizes much of the work. For this, an MS Excel add-in exists that facilitates the work with Xlsx files massively.

# Swate

**Swate** (Swate workflow annotation tool for Excel) was developed by the CSB group as an MS Excel extension for facilitating the work with _Assay_ files.  
You either need to have Microsoft Office installed to work with the desktop application or have a Microsoft 365 account to work online in a browser of your choice.

Head over to the [installing chapter for Swate](https://github.com/nfdi4plants/Swate#installuse) and follow the instructions there.

After you successfully installed Swate, you can create an exemplary _Assay_ file to get a feeling of how to use Swate.  

1. Create a new Xlsx file and open it (online or desktop)  
2. Start Swate Core by opening it via the Data tab  
![](https://github.com/CSBiology/BIO-BTE-06-L-7/raw/main/docs/img/OpenSwateCore.png)  
3. [Create a new annotation table](https://github.com/nfdi4plants/Swate/wiki/Docs02-Annotation-Table)  
![](https://github.com/CSBiology/BIO-BTE-06-L-7/raw/main/docs/img/CreateAnnotationTable.jpg)  
4. [Add Building Blocks](https://github.com/nfdi4plants/Swate/wiki/Docs03-Building-Blocks)  
![](https://github.com/CSBiology/BIO-BTE-06-L-7/raw/main/docs/img/AddBuildingBlock.jpg)  
5. Search the term database (B) for a term fitting. If you don't find one, just write a custom term that suits your needs.  
You can add units to them before creation (D) or afterwards:  
<br>
![](https://github.com/CSBiology/BIO-BTE-06-L-7/raw/main/docs/img/UpdateUnit.jpg)  
6. [Fill cells with terms](https://github.com/nfdi4plants/Swate/wiki/docs04-Ontology-Term-Search)  
![](https://github.com/CSBiology/BIO-BTE-06-L-7/raw/main/docs/img/TermInsert.jpg)  
<br>
Like with Building Blocks, rather write down your own terms before choosing a term from the database that doesn't fit well and annotates the experiment worse.
7. You can also type values into cells without using the Ontology Term Search, but don't forget to update them afterwards via the _Update Ontology Terms_ function:  
![](https://github.com/CSBiology/BIO-BTE-06-L-7/raw/main/docs/img/UpdateOntologyTerms.png)
8. Use _Autoformat Table_ (A) to automatically format an annotation table. _Remove Building Block_ (B) deletes whole Building Blocks while _Get Building Block Information_ gives you information about the terms and size of a Building Block:  
![](https://github.com/CSBiology/BIO-BTE-06-L-7/raw/main/docs/img/AutoformatTable_RemoveBuildingBlock_GetBuildingBlockInformation.png)

# Editing an ARC

After you learned about the ARC, its structure and the _Assay_ file, you will now transform a given protocol into such an _Assay_ file via using Swate.  
There's a protocol of the method section of a common paper in this course's ARC's assay in the _protocols_ folder. Read it and build a Swate table according to it in the _Assay_ file. Annotate the _Investigation_ file in the ARC's root folder accordingly.

Download this course's ARC from [here](https://github.com/CSBiology/BIO-BTE-06-L-7/releases/download/RDMWorkshopARC/RDMWorkshopARC-2022.zip).

*)