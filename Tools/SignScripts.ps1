param([string]$RootDir, [string]$CertThumbPrint = 'BB25149CDAF879A29DB6A011F6FC874AF32CBF51')

# This script is invoked by the Pscx project's post build event.
$cert = Get-ChildItem Cert:\CurrentUser\My\$CertThumbPrint

Get-ChildItem $RootDir -Recurse -Include *.ps1,*.psm1,*.ps1xml,*.psd1 -File |
    Where Name -ne 'Pscx.UserPreferences.ps1' |
    Foreach {
        Set-AuthenticodeSignature -Certificate $cert -TimestampServer http://timestamp.digicert.com $_.FullName
    }
