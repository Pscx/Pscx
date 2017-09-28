$cmdLets = (Get-Module pscx).ExportedCmdlets.Keys
$functions = (Get-Module pscx).ExportedFunctions.Keys
$cmdLetsAndFunctions = $cmdLets + $functions | Select -uniq | Sort-Object
foreach ( $cmdLet in ($cmdLetsAndFunctions) ) {
  $description = (get-help $cmdLet).synopsis.replace('PSCX Cmdlet: ','')
  $output = @'
### `{0}`

{1}

'@ -f $cmdLet, $description
  echo $output
}
