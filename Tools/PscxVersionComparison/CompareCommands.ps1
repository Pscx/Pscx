param([string]$refPath=$(throw 'You must provide a path to a reference XML file'), 
      [string]$diffPath=$(throw 'You must provide a path to a second XML file - duh'))

$ref  = Import-CliXml $refPath | Sort Name
$diff = Import-CliXml $diffPath  | sort Name

Compare-Object $ref $diff -property Name,CommandType -sync 1000 | sort CommandType, Name