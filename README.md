# Jupyter_PraktikumBiotech

## Installation (Windows Local Installation and Use)

1. Download [Anaconda](https://www.anaconda.com/download/) for Python 3.6

2. Launch Anaconda3-4.4.0-Windows-x86_64.exe (or later exe should work, file an issue if you have issues)
   Click through the installation wizard, choosing the given install location. At the 'advanced installation options' screen shown below, select "Add Anaconda to my PATH environment variable". The installer warns against this step, as it can clash with previously installed software, however it's currently essential for running IfSharp. Now install.

   This should also install Jupyter: you may check this by entering 'jupyter notebook' into the Anaconda console window. If Jupyter does not launch (it should launch in the browser), install using 'pip install jupyter', or by following [Jupyter](http://jupyter.readthedocs.io/en/latest/install.html) instructions.

3. Run the "IfSharp\ifsharp.exe" once when first downloading. 

Jupyter will start and a notebook with F# can be selected. This can be run via "jupyter notebook" in future

To start Jupyter with the correct notebooks use the command shown in [Powershell Startup Command](#Powershell-Startup-Command).

For more information see [the original ifsharp documentation](https://github.com/fsprojects/IfSharp#windows-local-installation-and-use).

## Change Jupyter Notebook startup folder (Windows)

### Powershell Startup Command

Use powershell <code>jupyter notebook --notebook-dir="D:\this\will\be\your\path\Jupyter_PraktikumBiotech\Notebooks"</code>.
This method has to be used at every start.

https://stackoverflow.com/questions/15680463/change-ipython-jupyter-notebook-working-directory
