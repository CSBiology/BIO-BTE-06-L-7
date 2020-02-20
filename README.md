# Jupyter_PraktikumBiotech

## Change Jupyter Notebook startup folder (Windows)

Use powershell <code>jupyter notebook --notebook-dir="D:\Freym\Source\Repos\Freymaurer\Jupyter_PraktikumBiotech\Notebooks"</code>.
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
