param ([string]$SnapinPath = $(throw "Path to snapin dll is required"), 
       [string[]]$cmdletClassName = $(throw "Name of cmdlet classes is required"), 
       $OutputPath)
       
$ScriptDir = Split-Path $MyInvocation.MyCommand.Path -Parent
if (!$OutputPath)
{
	$OutputPath = Join-Path $ScriptDir ..\Help
}       

function New-HelpSkeleton {
	
	begin {
	
		$_Cmdlet = [Management.Automation.Cmdlet]
		$_CmdletAttribute = [Management.Automation.CmdletAttribute]
		$_ParameterAttribute = [Management.Automation.ParameterAttribute]
		
		if (!(test-path $OutputPath)) {
			new-item -type Directory $OutputPath
		}

		$OutputPath = (resolve-path $OutputPath)

		function Write-EmptyParaTags
		{
			""
			Write-InputOutputTypes InputType
			Write-InputOutputTypes ReturnType			
			Write-Examples
			'Note' | Write-EmptyParaTag
		}
		
		function Write-InputOutputTypes($typeName)
		{
			$output = new-object Text.StringBuilder
	
			& { 
				$output.AppendLine("    <${typeName}s>")
				$output.AppendLine("        <$typeName>")
				$output.AppendLine('            <Name></Name>')
				$output.AppendLine('            <Description><p></p></Description>')
				$output.AppendLine("        </$typeName>")		
				$output.AppendLine("    </${typeName}s>")
			} > $null
						
			$output.ToString()
		}		
		
		function Write-Examples
		{
			$output = new-object Text.StringBuilder
	
			& { 
				$output.AppendLine('    <Examples>')
				$output.AppendLine('        <Example Number="1">')
				$output.AppendLine('            <Code></Code>')
				$output.AppendLine('            <Remarks>')
				$output.AppendLine('                <p></p>')
				$output.AppendLine('            </Remarks>')
				$output.AppendLine('        </Example>')		
				$output.AppendLine('    </Examples>')
			} > $null
						
			$output.ToString()
		}
		
		filter Write-EmptyParaTag
		{
			$output = new-object Text.StringBuilder
			
			& { 
				$output.Append('    <')
				$output.Append($_)
				$output.AppendLine('s>')
				
				$output.Append('        <')
				$output.Append($_)
				$output.Append('><p></p></')
				$output.Append($_)
				$output.AppendLine('>')
				
				$output.Append('    </')
				$output.Append($_)
				$output.AppendLine('s>')
			} > $null
			
			$output.ToString()
		}
	}

	process 
	{
		$_.GetExportedTypes() | where { $_Cmdlet.IsAssignableFrom($_) } | foreach `
		{	
			if ($cmdletClassName -and ($cmdletClassName -notcontains $_.Name)) { return }
			
			$cmdlet = $_
		
			$_.GetCustomAttributes($_CmdletAttribute, $false) | foreach `
			{
				$filename = "$($_.VerbName + $_.NounName).xml"
				$filename = (join-path $OutputPath $filename)
				
				$description = ''
				$detailed = ''
				
				$cmdlet.GetCustomAttributes($false) |% {

					$type = $_.GetType().FullName
					
					if($type -eq 'System.ComponentModel.DescriptionAttribute') 
					{
						$description = $_.Description
					}
					
					if($type -eq 'Pscx.DetailedDescriptionAttribute') 
					{
						$detailed = $_.Text
					}					
				}
				
				"<?xml version='1.0' ?>"                       > $filename
				"<Cmdlet FullName='$($cmdlet.Fullname)'>"     >> $filename
				"    <Description>"                           >> $filename
				"         $description"                       >> $filename
				"    </Description>"                          >> $filename

				"    <DetailedDescription>"                   >> $filename
				"         $detailed"                          >> $filename
				"    </DetailedDescription>"                  >> $filename

				"    <Parameters>"                            >> $filename
				
				$cmdlet.GetProperties('Public,Instance,FlattenHierarchy') | foreach `
				{
					$pa = @($_.GetCustomAttributes($_ParameterAttribute, $false))[0]
					
					if($pa) {
						$description = ($pa.HelpMessage)
						$default = ''

						$_.GetCustomAttributes($false) |% {
							$type = $_.GetType().FullName
							
							if($type -eq 'System.ComponentModel.DefaultValueAttribute') {
								$default = $_.Value
							}
						}
						
						"        <Parameter Name='$($_.Name)'>" >> $filename
						"            <Description>"             >> $filename
						"                $description"          >> $filename
						"            </Description>"            >> $filename
						"            <DefaultValue>"            >> $filename
						"                $default"              >> $filename
						"            </DefaultValue>"           >> $filename
						"        </Parameter>"                  >> $filename
					}
				}
				
				"    </Parameters>" >> $filename

				Write-EmptyParaTags >> $filename				

				"</Cmdlet>"         >> $filename
			}
		}
	}
}

$path = Resolve-Path $SnapinPath
[System.Reflection.Assembly]::LoadFile($path) | New-HelpSkeleton
