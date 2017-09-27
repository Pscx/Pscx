$cmdLets = (Get-Module pscx).ExportedCmdlets.Keys
$commands = (Get-Module pscx).ExportedCommands.Keys
# Some cmdlets and commands have the same name
# Extract only uppercase items as the lowercase versiosn are typically
# aliases and better documented elsewhere.
$cmdLetsAndCommands = $cmdLets + $commands | Select -uniq | Sort-Object | where { $_ -cmatch "^[A-Z]"}
foreach ( $cmdLet in ($cmdLetsAndCommands) ) {
  $description = (get-help $cmdLet).synopsis.replace('PSCX Cmdlet: ','')
  $output = @'
### `{0}`

{1}

'@ -f $cmdLet, $description
  echo $output
}
