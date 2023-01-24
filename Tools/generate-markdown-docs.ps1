# Disable ANSI colors - same as $env:TERM = xterm-mono
$PSStyle.OutputRendering = [System.Management.Automation.OutputRendering]::PlainText

$cmdLets = (Get-Module pscx).ExportedCmdlets.Keys
$functions = (Get-Module pscx).ExportedFunctions.Keys
$cmdLetsAndFunctions = $cmdLets + $functions | Select -uniq | Sort-Object

$nounsAndCommands = @{}

foreach ( $cmdLet in ($cmdLetsAndFunctions) ) {

  $noun = $cmdLet.split('-')[1]

  if ( ! $noun ) {
    continue
  }

  $description = (get-help $cmdLet).synopsis.replace('PSCX Cmdlet: ','')

  # {
  #   tar: [
  #     {
  #       name: 'get-tar'
  #       description: 'this command does'
  #     },
  #     ....
  #   ],
  #   ...
  # }

  $helpEntry = @{'name' = $cmdLet; 'description' = $description;}

  if ( ! $nounsAndCommands.ContainsKey($noun) ) {
    $nounsAndCommands[$noun] = @()
  }
  $nounsAndCommands[$noun] += $helpEntry
}

$output = ''

# Now sort by the nouns and spit out the headings and documentation
foreach($item in $nounsAndCommands.GetEnumerator() | Sort Name) {
  $noun = $item.Name
  $output += @'
### {0}


'@ -f $noun
  foreach ($commandNameAndDescription in $nounsAndCommands[$noun]) {
    $output += @'
#### `{0}`

{1}


'@ -f $commandNameAndDescription.name, $commandNameAndDescription.description
  }
}

echo $output
