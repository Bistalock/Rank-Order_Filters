- Add the license file.
- As of now, the dynamic link library MSVCR110.DLL is needed for libraries to run on machines
  without Visual Studio 2012 installed. 
  
  To do this,

   - Make program distributed through an MSI Intallation
   - create uninstall.exe
   - if not, create the "Microsoft.VC110.CRT" folder with dll and manifest
   - Keep readme file?
   - Maybe add a change.log file