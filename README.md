# Jupyter_PraktikumBiotech

## Installation (Windows Local Installation and Use)

1. Download [Anaconda](https://www.anaconda.com/download/) for Python 3.6

2. Launch Anaconda3-4.4.0-Windows-x86_64.exe (or later exe should work, file an issue if you have issues)
   Click through the installation wizard, choosing the given install location. At the 'advanced installation options' screen shown below, select "Add Anaconda to my PATH environment variable". The installer warns against this step, as it can clash with previously installed software, however it's currently essential for running IfSharp. Now install.

   This should also install Jupyter: you may check this by entering 'jupyter notebook' into the Anaconda console window. If Jupyter does not launch (it should launch in the browser), install using 'pip install jupyter', or by following [Jupyter](http://jupyter.readthedocs.io/en/latest/install.html) instructions.

   ![Installation screenshot](/docs/files/img/anaconda-installation.png)

3. Run the "IfSharp\ifsharp.exe" once when first downloading. 

Jupyter will start and a notebook with F# can be selected. This can be run via "jupyter notebook" in future

To start Jupyter with the correct notebooks use the command shown in [Powershell Startup Command](#Powershell-Startup-Command).

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
<div Style="text-align: justify ; font-size: 1.8rem ; margin-top: 2rem ; line-height : 1.5">
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
        Equation 1: Calculation of the doubling time. Growth rate is calculates as shwon in Equation 2::::wlsekhfgasdgufh aodshfgo adfhrsg oiuhdfoih gdh fiogoijd ifjigj disjgioj difojgoi jdisfjgoijd fijgi djfigj dosijfig jdfij iogjdfijg idfjijg jdpofkgod kfopgkj fpdojgpodj g
    </i>
</div>
```