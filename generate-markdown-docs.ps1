$cmdLets = (Get-Module pscx).ExportedCmdlets.Keys
$functions = (Get-Module pscx).ExportedFunctions.Keys
# Some cmdlets and commands have the same name
# Extract only uppercase items as the lowercase versiosn are typically
# aliases and better documented elsewhere.
$cmdLetsAndFunctions = $cmdLets + $functions | Select -uniq | Sort-Object
foreach ( $cmdLet in ($cmdLetsAndFunctions) ) {
  $description = (get-help $cmdLet).synopsis.replace('PSCX Cmdlet: ','')
  $output = @'
### `{0}`

{1}

'@ -f $cmdLet, $description
  echo $output
}
