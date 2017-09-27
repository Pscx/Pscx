$cmdLets = (Get-Module pscx).ExportedCmdlets.Keys
$commands = (Get-Module pscx).ExportedCommands.Keys
$cmdLetsAndCommands = $cmdLets + $commands | Select -uniq | Sort-Object | where{ $_ -match "^[A-Z]"}
foreach ( $cmdLet in ($cmdLetsAndCommands) ) {
  $description = (get-help $cmdLet).synopsis.replace('PSCX Cmdlet: ','')
  $output = @'
### `{0}`

{1}

'@ -f $cmdLet, $description
  echo $output
}
