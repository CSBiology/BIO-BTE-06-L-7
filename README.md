# Jupyter_PraktikumBiotech

## Change Jupyter Notebook startup folder (Windows)

use powershell <code>jupyter notebook --notebook-dir="D:\Freym\Source\Repos\Freymaurer\Jupyter_PraktikumBiotech\Notebooks"</code>

https://stackoverflow.com/questions/15680463/change-ipython-jupyter-notebook-working-directory

or, but worse, ...

- Copy the Jupyter Notebook launcher from the menu to the desktop.
- Right click on the new launcher and change the Target field, change %USERPROFILE% to the full path of the folder which will contain all the notebooks.
- Double-click on the Jupyter Notebook desktop launcher (icon shows [IPy]) to start the Jupyter Notebook App. The notebook interface will appear in a new browser window or tab. A secondary terminal window (used only for error logging and for shut down) will be also opened.

https://jupyter-notebook-beginner-guide.readthedocs.io/en/latest/execute.html#change-jupyter-notebook-startup-folder-windows
