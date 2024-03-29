# VM 1/ VM 2: Practical class - Molecular Biotechnology II (BIO-BTE-06-L-7)

_This repository is currently undergoing drastic changes, moving from IFSharp notebooks to a static site/.NET interactive hybrid solution using FSharp.Formatting_

This repository contains the source of the documentation pages and notebooks hosted at https://csbiology.github.io/BIO-BTE-06-L-7/

## Installation (Windows Local Installation and Use)

1. Download [Anaconda](https://www.anaconda.com/download/) for Python 3.6

2. Launch Anaconda3-4.4.0-Windows-x86_64.exe (or later exe should work, file an issue if you have issues)
   Click through the installation wizard, choosing the given install location. At the 'advanced installation options' screen shown below, select "Add Anaconda to my PATH environment variable". The installer warns against this step, as it can clash with previously installed software, however it's currently essential for running IfSharp. Now install.

   This should also install Jupyter: you may check this by entering 'jupyter notebook' into the Anaconda console window. If Jupyter does not launch (it should launch in the browser), install using 'pip install jupyter', or by following [Jupyter](http://jupyter.readthedocs.io/en/latest/install.html) instructions.

3. Run the "IfSharp\ifsharp.exe" once when first downloading. 

Jupyter will start and a notebook with F# can be selected. This can be run via "jupyter notebook" in future

To start Jupyter with the correct notebooks use the command shown in [Powershell Startup Command](#Powershell-Startup-Command).

For more information see [the original ifsharp documentation](https://github.com/fsprojects/IfSharp#windows-local-installation-and-use).

## Useful Link

- Contains formatting help for markdown Jupyter. https://medium.com/ibm-data-science-experience/markdown-for-jupyter-notebooks-cheatsheet-386c05aeebed

- Useful for math formula. At the bottom is a copy-paste html code you can use for Jupyter. https://www.codecogs.com/latex/eqneditor.php

## Change Jupyter Notebook startup folder (Windows)

### Powershell Startup Command

Use powershell <code>jupyter notebook --notebook-dir="D:\this\will\be\your\path\Jupyter_PraktikumBiotech\Notebooks"</code>.
This method has to be used at every start.

https://stackoverflow.com/questions/15680463/change-ipython-jupyter-notebook-working-directory

or, but worse, ...

- Copy the Jupyter Notebook launcher from the menu to the desktop.
- Right click on the new launcher and change the Target field, change %USERPROFILE% to the full path of the folder which will contain all the notebooks.
- Double-click on the Jupyter Notebook desktop launcher (icon shows [IPy]) to start the Jupyter Notebook App. The notebook interface will appear in a new browser window or tab. A secondary terminal window (used only for error logging and for shut down) will be also opened.

https://jupyter-notebook-beginner-guide.readthedocs.io/en/latest/execute.html#change-jupyter-notebook-startup-folder-windows

## Set Password to Jupyter Notebook

Use powershell <code>jupyter notebook password</code>.
You will then be asked to give a password and after that to verify it. Both times the console will not show any typing but it will still recognize the input.

<b>Attention! I do not know any way yet to remove password again!</b>

## Style Guide

### Basic text container
```html 
<div Style="max-width: 85%">
</div>
```
### Text div

```html 
<div Style="text-align: justify ; font-size: 1.8rem ; margin-top: 2rem ; line-height: 1.5">
</div>
```

### Combined TextContainer and TextDiv

```html 
<div Style="max-width: 85%">
	<div Style="text-align: justify ; font-size: 1.8rem ; margin-top: 2rem ; line-height: 1.5">
	</div>
</div>
```

### In-Page Links
```html
<a href="#Comments">Comments</a>
```
&&

```html
<div Id="Comments">Comments</a>
```

### Centered Italic Text Fields
```html
<div Style="text-align: justify ; width: 90% ; margin-left: auto ; margin-right: auto ; margin-top: 2rem">
    <i>
        Equation 1: Calculation of the doubling time. Growth rate is calculates as shwon in.
    </i>
</div>
```

### Code Blocks

```html
<div Style="width: 90% ; margin-left: auto ; margin-right: auto ; margin-top: 1rem">

```fsharp
let x = 5
```

</div>
```


CommentColor: #008000
StringColor: #B21543
BoolColor: #077
Float/IntColor: #3a3

### In-Between Text Image Element

```html
<div Style="float: right ; display: inline-block ; font-size: 1.7rem ; color: #44546a ; width: 60% ; padding: 15px">
    <img src="img/SystemsBiology_Figure5.png" Style="width: 100%">
    <div Style="padding-left: 1rem ; padding-right: 1rem ; text-align: justify ; ">
        <b>Figure 5: Process of computational identification of peptides from their fragment spectra</b>
    </div>
</div>    
```

### Grey-Dotted Box

```html
<div Style="text-align: justify ; font-size: 1.5rem ; margin-top: 2rem ; margin-bottom: 2rem ; line-height: 1.3 ; width: 85% ; margin-left: auto ; margin-right: auto ; padding: 10px ; border: 2px dotted #708090 ; color: #708090">
</div>
```

### References

```html
<sup><a href="#1">1</a></sup>
```
&&
```html
<ol Style="max-width: 85% ; text-align: justify ; font-size: 1.8rem ; margin-top: 2rem ; line-height: 1.5">
    <li Value ="1" Id="1">
		exmp
	</li>
</ol>
```

### Table of Content 


```html
<h1 Style="font-size: 3rem" >Growth Curve and Doubling Time</h1>

1. [Plant Systems Biology](#Plant-Systems-Biology)
2. [Insert Growth Data and Display as Chart](#Insert-Growth-Data-and-Display-as-Chart)
3. [Calculation of growth rate and doubling time for cell cultures](#Calculation-of-growth-rate-and-doubling-time-for-cell-cultures)
4. [Exponential Fit](#Exponential-Fit)<br>
    4.1 [Exp Fit Theorie](#Exp-Fit-Theorie)<br>
    4.2 [Exp Fit Select Exponential Phase](#Exp-Fit-Select-Exponential-Phase)<br>
    4.3 [Exp Fit Calculate Doubling Time](#Exp-Fit-Calculate-Doubling-Time)<br>
5. [Logistic Fit](#Logistic-Fit)
6. [References](#References)
```

### Left-Right Buttons 

```html
<div style="width: 100% ; height: 60px ; display: flex ; align-content: center">
    <a href="JP01_Systems_Biology_FSharp_Introduction.ipynb" style="padding: 8px ; border: 2px solid blue ; border-radius: 5px ; font-size: 1.5rem ; cursor: pointer ; color: inherit; text-decoration: none ; margin-right: auto ; display: block ; height: 40px ; width: 80px ; text-align: center ; color: white ; background-color: blue">
        &#171; JP01
    </a>
    <a href="JP01_Systems_Biology_FSharp_Introduction.ipynb" style="padding: 8px ; border: 2px solid blue ; border-radius: 5px ; font-size: 1.5rem ; cursor: pointer ; color: inherit; text-decoration: none ; margin-left: auto ; display: block ; height: 40px ; width: 80px ; text-align: center ; color: white ; background-color: blue">
        JP01 &#187;
    </a>
</div>
```

### Back-Up from header

For the page title use the following:

```html
<h1 Id="PageTitle" Style="font-size: 3rem" >Systems Biology</h1>
```

For the Back-Up use the following:

```html
<h1 Style="font-size: 2rem ; display: inline-block" >References</h1><br>
<a href="#PageTitle" style="display: inline-block"><sup>&#8593;back</sup></a>
```
