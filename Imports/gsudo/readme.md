# Sudo for Windows

Sudo utility embedded into PSCX is leveraged from [GSudo Project](https://github.com/gerardog/gsudo). The version chosen is 1.7 - as stable and robust for Windows/PowerShell mix (note current version is 2.0.4, actively developed).

## Integration

Download the `ZIP` from the release of choice (1.7 present in this folder) and extract the contents of the `x64` folder. Then update the `.psm1` file to include statically the contents of `invoke-gsudo.ps1` script (i.e. copy it into the module file as a function) - we're not going to use the _invoke-gsudo_ script independently. Then you can delete the `invoke-gsudo.ps1` file.

Copy these files into `Src/Pscx.Win/Modules/Sudo` and change their name to _Pscx.Sudo.psd1_ and _Pscx.Sudo.psm1_ respectively. The name of the (sub-)module is therefore __Pscx\Sudo__ and this should be reflected into the module files. As changes get integrated from the original project GgitHub source, the rename must be carried forward.

The executable (`gsudo.exe`) is copied into the output folder renamed as `sudo.exe` (see `pscxwin-postbuild.ps1` script).
