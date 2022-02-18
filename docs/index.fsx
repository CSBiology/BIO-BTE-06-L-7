(**
# BIO-BTE-06-L-7
<br>
## Course objective and procedure

A typical modern proteomics workflow reaches from the experiment and measurement over deconvolution, identification, 
quantification, protein assembly and statistical analysis. This shows that independent from the different specialized 
workflows at hand, the computational part in proteomics is noticeable.

The objective of this course is to provide insights in both aspects. Now, is time to start the computational proteomics part. In the following notebooks you will learn: 

1. How to model growth for a defined cell number to relate the findings in your proteomics experiments to a meaningful basis.
2. Getting in-silico information about the proteome of interest to be able to make sense out of your experimental measurements.
3. How to access and look at your measurements from an m/z perspective and what it is that we actually measure.
4. How does peptide or protein identification work computationally?
5. What must be done to quantify a peptide?

The most hands-on approach to provide those insights is to use a combination of explanatory text, images, interactive charts, 
and program code. This will be combined within interactive notebooks, that allow you to play around with the code examples and learn.

In this course, we want to support your coding literacy by a practical learning approach without the attempt to detail every mechanisms of the 
programming language. This means that the focus is on reading, understanding, and learning to manipulate the code not on coding itself.

However, in the beginning you will get a rapid introduction into F# programming that should allow you to understand code 
and do some manipulations to achieve your desired outcome.

## Schedule

| Day      | Topic                                 | Notebooks        |
|----------|---------------------------------------|------------------|
| 21.02.21 | Getting started; Coding literacy      | 00a; 00b         |
| 22.02.21 | 1: Growth Model and Cell number       | 01a; 01b         |
| 23.02.21 | 2: In-silico proteome analysis        | 02a; 02b; 02c    |
| 24.02.21 | 3: Understanding the Data             | 03a; 03b; 03c    |
| 25.02.21 | 4: Protein and Peptide identification | 04a; 04b         |
| 28.02.21 | 5: Peptide quantification             | 05a              |
| 01.03.21 | Research Data Management              | 06a              |
| 02.03.21 | Analysing the course Experiment       | 07a              |
| 03.03.21 | Analysing the course Experiment       | 07b              |
| 04.03.21 | Analysing the course Experiment       | 07c              |

## Getting started

* Download the latest stable build for [Visual Studio Code](https://code.visualstudio.com/) and install it.
* Download the recommended [.NET SDK](https://dotnet.microsoft.com/download) and install it.
* Open Visual Studio Code, navigate to the "Extensions" tab and install
    * .NET Interactive Notebooks
    * Ionide-fsharp
    
    ![](https://raw.githubusercontent.com/CSBiology/BIO-BTE-06-L-7/main/docs/img/CodeExtensions.png)

* Download the current notebook from the page linked on the left.

    ![](https://raw.githubusercontent.com/CSBiology/BIO-BTE-06-L-7/main/docs/img/DownloadNotebook.png)

* In Visual Studio Code press `Strg + Shift + P` and klick on `.NET Interactive Open notebook`.

    ![](https://raw.githubusercontent.com/CSBiology/BIO-BTE-06-L-7/main/docs/img/OpenNotebook.png)

* Navigate to the location of your notebook and open it.
* Notebooks contain Text- and Codeblocks:
    * Adding a new Text- or Codeblock can be done by hovering at the upper or lower border of an existing block:

    ![](https://raw.githubusercontent.com/CSBiology/BIO-BTE-06-L-7/main/docs/img/AddingBlock.png)

    * Working with Textblocks:
        You can edit a Textblock by doubleklicking on it. Inside a Textblock you can write plain text or style it with [Markdown](https://en.wikipedia.org/wiki/Markdown).
        Once you are finished you can press the `Esc` button.
    * Working with Codeblocks:
        You can start editing any Codeblock by clicking in it. In there you can start writing your own code or edit existing code. Once you are done you can execute the Codeblock by pressing `Strg + Alt + Enter`.
        If you want to execute all codeblocks at once, you can press on the two arrows in the upper right corner of the notebook:

    ![](https://raw.githubusercontent.com/CSBiology/BIO-BTE-06-L-7/main/docs/img/ExecuteAll.png)
*)