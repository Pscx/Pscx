$cmdLets = (Get-Module pscx).ExportedCmdlets.Keys
$functions = (Get-Module pscx).ExportedFunctions.Keys
$cmdLetsAndFunctions = $cmdLets + $functions | Select -uniq | Sort-Object

$nounsAndCommands = @{}

foreach ( $cmdLet in ($cmdLetsAndFunctions) ) {

  $noun = $cmdLet.split('-')[1]

  if ( ! $noun ) {
    break
  }

  $description = (get-help $cmdLet).synopsis.replace('PSCX Cmdlet: ','')
  $output = @'
#### `{0}`

{1}


'@ -f $cmdLet, $description

  # tar
  #   get-tar
  #   this command does

  $helpEntry = @{'name' = $cmdLet; 'output' = $output;}


  if ( ! $nounsAndCommands.ContainsKey($noun) ) {
    $nounsAndCommands[$noun] = @()
  }
  $nounsAndCommands[$noun] += $helpEntry
}

# OK now sort by
foreach($item in $nounsAndCommands.GetEnumerator() | Sort Name) {
  $noun = $item.Name
  $output = @'
### {0}


'@ -f $noun


  foreach ($commandNameAndDescription in $nounsAndCommands[$noun]) {
    $output += $commandNameAndDescription.output;
  }

  echo $output

}
