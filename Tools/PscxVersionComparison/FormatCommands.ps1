param(
	[string]$refPath = 'Pscx1_0Commands.xml', 
	[string]$diffPath = 'Pscx1_1Commands.xml'
)

function Remove-Aliases($cmdal) {

	$script:aliases = $($script:aliases |? {
		$cmdal -notcontains $_.Name
	})
}

function Get-CommandNoun($command) {
	$command.Split('-', 2)[1]
}

function Get-AliasCommand($al) {
	$diff |? { $_.Name -eq $al } |% { $_.ResolvedCommand.Name }
}

function Get-CommandAliasNames($command) {
	$diff |? { $_.ResolvedCommand.Name -eq $command } |% { $_.Name }
}

function Format-CommandAliases($cmdal) {
	$result = ''
	$OFS = ', '

	if ($cmdal) {
		$result += ' ('
		$result += "$cmdal"
		$result += ')'
	}
	
	$result
}

$ref  = Import-CliXml $refPath | Sort Name
$diff = Import-CliXml $diffPath  | sort Name

$comparisonResult = Compare-Object $ref $diff -property Name,CommandType -sync 1000 | Sort-Object SideIndicator 
$changes = $comparisonResult | Group-Object CommandType

$script:aliases   = $null
$script:cmdlets   = $null
$script:filters   = $null
$script:functions = $null
$script:scripts   = $null

$IsNew     = { $_.SideIndicator -eq '=>' }
$IsRemoved = { $_.SideIndicator -eq '<=' }

$changes |% {
	if ($_.Name -eq 'Alias')          { $aliases   = @($_.Group) }
	if ($_.Name -eq 'Cmdlet')         { $cmdlets   = @($_.Group) }
	if ($_.Name -eq 'Filter')         { $filters   = @($_.Group) }
	if ($_.Name -eq 'Function')       { $functions = @($_.Group) }
	if ($_.Name -eq 'ExternalScript') { $scripts   = @($_.Group) }
}

""
"NEW CMDLETS"
$cmdlets | ? $IsNew | Sort { Get-CommandNoun $_.Name } | %{

	$cmdname = $_.Name
	$result = "    $cmdname"
	
	$cmdal = @(Get-CommandAliasNames $cmdname)
	$result += Format-CommandAliases $cmdal

	Remove-Aliases $cmdal

	$result
}

""
"NEW FUNCTIONS AND FILTERS"
@($functions + $filters) | ? $IsNew | Sort Name | %{

	$name = $_.Name
	$result = "    $name"
	
	$cmdal = @(Get-CommandAliasNames $name)
	$result += Format-CommandAliases $cmdal
	
	Remove-Aliases $cmdal
	
	$result
}

""
"NEW SCRIPTS:"
$scripts | ? $IsNew | Sort Name | %{

	$name = $_.Name
	$result = "    $name"
	
	$cmdal = @(Get-CommandAliasNames $name)
	$result += Format-CommandAliases $cmdal
	
	Remove-Aliases $cmdal
	
	$result
}

""
"NEW ALIASES"
$aliases | ? $IsNew | %{ 
	"    $($_.Name) ($(Get-AliasCommand $_.Name))" 
}

""
"REMOVED STUFF"
$comparisonResult | ? $IsRemoved | Sort Name | %{
	"    $($_.Name)"
}