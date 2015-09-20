#requires -Version 3
Set-StrictMode -Version Latest

# Aliases
Set-Alias gpv                 Get-PropertyValue -Description "PSCX.Deprecated alias"
Set-Alias rf                  Invoke-Reflector  -Description "PSCX.Deprecated alias"
Set-Alias rnd                 Get-Random        -Description "PSCX.Deprecated alias"
Set-Alias Send-MSMQMessage    Send-MSMQueue     -Description "PSCX.Deprecated alias"
Set-Alias Receive-MSMQMessage Receive-MSMQueue  -Description "PSCX.Deprecated alias"

# String resources
Import-LocalizedData -BindingVariable msgs -FileName Messages

#region Deprecation Function for Get-WebService Script

<#
.SYNOPSIS
    DEPRECATED: Generates a web service proxy object given the URL to its WSDL.
.DESCRIPTION
    DEPRECATED: Generates a web service proxy object given the URL to its WSDL.
    This function is a thin wrapper over the new, built-in PowerShell cmdlet New-WebServiceProxy. 
    Update your script to use this cmdlet. 
.PARAMETER Url
    The URL of the WSDL that describes the service's functionality.
.PARAMETER Anonymous
    Determines whether or not to use default credentials.
.PARAMETER Protocol
    This parameter is ignored.
.NOTES
    This command was deprecated in PSCX 2.0
#>
function Get-WebService
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true, Position = 0)]
        $Url, 
        
        [Parameter()]
        [switch]$Anonymous, 
        
        [Parameter()]
        [string]$Protocol
    )

    Begin {
        $warning = $msgs.DeprecatedRedirectedCmdWarningF2 -f $MyInvocation.MyCommand.Name,'New-WebServiceProxy'
        $pscmdlet.WriteWarning($warning)
    }

    Process {
        New-WebServiceProxy -Uri $Url -UseDefaultCredential:(!$Anonymous)
    }
}

#endregion Get-WebServiceProxy Function

#region DMTF Date & Time Conversion Functions

<#
.SYNOPSIS
    DEPRECATED: Uses the PowerShell ManagementDateTimeConverter to convert between WMI and .NET DateTime.
.DESCRIPTION
    DEPRECATED: Uses the PowerShell ManagementDateTimeConverter to convert between WMI and .NET DateTime.
    In its place you can use a simple [DateTime] cast to convert a DMTF date string to a .NET DateTime object.
.PARAMETER WmiDateTime
    The DMTF date string to convert to a .NET DateTime object.
.EXAMPLE
    C:\PS> [DateTime](gwmi win32_operatingsystem).LastBootUpTime
    This example shows how to use the replacement for this command i.e. casting to [DateTime].
.NOTES
    This command was deprecated in PSCX 2.0
#>
function ConvertFrom-WmiDateTime
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        $WmiDateTime
    )

    Begin {
        $warning = $msgs.DeprecatedFunctionWarningF2 -f $MyInvocation.MyCommand.Name,`
                       'In its place you can use a [DateTime] cast to convert a DMTF date string to a .NET DateTime object.'
        $pscmdlet.WriteWarning($warning)
    }

    Process {
        [System.Management.ManagementDateTimeConverter]::ToDateTime($WmiDateTime)
    }
}

<#
.SYNOPSIS
    DEPRECATED: Uses the PowerShell ManagementDateTimeConverter to convert between WMI and .NET TimeSpan.
.DESCRIPTION
    DEPRECATED: Uses the PowerShell ManagementDateTimeConverter to convert between WMI and .NET TimeSpan.
    In its place you can use a simple [TimeSpan] cast to convert a DMTF time interval string to a .NET TimeSpan object.
.PARAMETER WmiTimeSpan
    The DMTF time interval string to convert to a .NET TimeSpan object.
.NOTES
    This command was deprecated in PSCX 2.0
#>
function ConvertFrom-WmiTimeSpan
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        $WmiTimeSpan
    )

    Begin {
        $warning = $msgs.DeprecatedFunctionWarningF2 -f $MyInvocation.MyCommand.Name,`
                       'In its place you can use a [TimeSpan] cast to convert a DMTF time string to a .NET TimeSpan object.'
        $pscmdlet.WriteWarning($warning)
    }

    Process {
        [System.Management.ManagementDateTimeConverter]::ToTimeSpan($WmiTimeSpan)
    }
}

<#
.SYNOPSIS
    DEPRECATED: Uses the PowerShell ManagementDateTimeConverter to convert a .NET DateTime object to a WMI date/time string.
.DESCRIPTION
    DEPRECATED: Uses the PowerShell ManagementDateTimeConverter to convert a .NET DateTime object to a WMI date/time string.
    In its place you can use a simple [WmiDateTime] cast to convert a .NET DateTime object to a WMI date/time string.
.PARAMETER DateTime
    The .NET DateTime object to convert to a WMI date/time string.
.NOTES
    This command was deprecated in PSCX 2.0
#>
function ConvertTo-DmtfDateTime
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [DateTime]$DateTime
    )

    Begin {
        $warning = $msgs.DeprecatedFunctionWarningF2 -f $MyInvocation.MyCommand.Name,`
                       'In its place you can use a [WmiDateTime] cast to convert a .NET DateTime object to a WMI date/time string.'
        $pscmdlet.WriteWarning($warning)
    }
    
    Process {
        [System.Management.ManagementDateTimeConverter]::ToDmtfDateTime($DateTime)
    }
}

<#
.SYNOPSIS
    DEPRECATED: Uses the PowerShell ManagementDateTimeConverter to convert a .NET TimeSpan object to a WMI time interval string.
.DESCRIPTION
    DEPRECATED: Uses the PowerShell ManagementDateTimeConverter to convert a .NET TimeSpan object to a WMI time interval string.
    In its place you can use a simple [WmiTimeSpan] cast to convert a .NET TimeSpan object to a WMI time interval string.
.PARAMETER TimeSpan
    The .NET DateTime object to convert to a WMI date/time string.
.NOTES
    This command was deprecated in PSCX 2.0
#>
function ConvertTo-DmtfTimeInterval
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true, ValueFromPipeline=$true)]
        [TimeSpan]$TimeSpan
    )

    Begin {
        $warning = $msgs.DeprecatedFunctionWarningF2 -f $MyInvocation.MyCommand.Name,`
                       'In its place you can use a simple [WmiTimeSpan] cast to convert a .NET TimeSpan object to a WMI time interval string.'
        $pscmdlet.WriteWarning($warning)
    }
    
    Process {
        [System.Management.ManagementDateTimeConverter]::ToDmtfTimeInterval($TimeSpan)
    }
}

#endregion DMTF Date & Time Conversion Functions

<#
.ForwardHelpTargetName Microsoft.PowerShell.Utility\Get-Random
.ForwardHelpCategory   Cmdlet
#>
function Select-Random
{
    [CmdletBinding()]
    param([Parameter(ValueFromPipeline=$true)][psobject]$InputObject)

    Begin 
    {
        $collection = @()
        $warning = $msgs.DeprecatedFunctionWarningF2 -f $MyInvocation.MyCommand.Name,`
                       "In its place you can use PowerShell's Get-Random built-in cmdlet."
        $pscmdlet.WriteWarning($warning)
    }
    
    Process
    {
        $collection += $InputObject
    }
    
    End
    {
        $collection | Microsoft.PowerShell.Utility\Get-Random
    }
}

function Invoke-Reflector {
<#
    .SYNOPSIS
        Quickly load Reflector, with the specified Type or Command selected. 
    .DESCRIPTION
        Quickly load Reflector, with the specified Type or Command selected. The function will also
        ensure that Reflector has the Type or Command's containing Assembly loaded.
    .EXAMPLE
        # Opens System.String in Reflector. Will load its Assembly into Reflector if required.
        ps> [string] | invoke-reflector    
    .EXAMPLE
        # Opens GetChildItemCommand in Reflector. Will load its Assembly into Reflector if required.
        ps> gcm ls | invoke-reflector        
    .EXAMPLE
        # Opens GetChildItemCommand in Reflector. Will load its Assembly into Reflector if required.
        ps> invoke-reflector dir        
    .PARAMETER CommandName
        Accepts name of command. Does not accept pipeline input.
    .PARAMETER CommandInfo
        Accepts output from Get-Command (gcm). Accepts pipeline input.
    .PARAMETER Type
        Accepts a System.Type (System.RuntimeType). Accepts pipeline input.
    .PARAMETER ReflectorPath
        Optional. Defaults to Reflector.exe's location if it is found in your $ENV:PATH. If not found, you must specify.
    .INPUTS
        [System.Type]
        [System.Management.Automation.CommandInfo]
    .OUTPUTS
        None
#>
     [cmdletbinding(defaultparametersetname="name")]
     param(
         [parameter(
            parametersetname="name",
            position=0,
            mandatory=$true
         )]
         [validatenotnullorempty()]
         [string]$CommandName,
         
         [parameter(
            parametersetname="command",
            position=0,
            valuefrompipeline=$true,
            mandatory=$true
         )]
         [validatenotnull()]
         [management.automation.commandinfo]$CommandInfo,
         
         [parameter(
            parametersetname="type",
            position=0,
            valuefrompipeline=$true,
            mandatory=$true
         )]
         [validatenotnull()]
         [type]$Type,
         
         [parameter(
            position=1
         )]
         [validatenotnullorempty()]
         [string]$ReflectorPath = $((gcm reflector.exe -ea 0).definition)
     )
     
    end {
        # no process block; i only want
        # a single reflector instance
        
        if ($ReflectorPath -and (test-path $reflectorpath)) {

            $typeName = $null            
            $assemblyLocation = $null
            
            switch ($pscmdlet.parametersetname) {
            
                 { "name","command" -contains $_ } {
                
                    if ($CommandName) {
                        $CommandInfo = gcm $CommandName -ea 0
                    } else {
                        $CommandName = $CommandInfo.Name
                    }
                    
                    if ($CommandInfo -is [management.automation.aliasinfo]) {
                        
                        # expand aliases
                        while ($CommandInfo.CommandType -eq "Alias") {
                            $CommandInfo = gcm $CommandInfo.Definition
                        }                                                
                    }
                    
                    # can only reflect cmdlets, obviously.
                    if ($CommandInfo.CommandType -eq "Cmdlet") {
                    
                        $typeName = $commandinfo.implementingtype.fullname
                        $assemblyLocation = $commandinfo.implementingtype.assembly.location
                    
                    } elseif ($CommandInfo) {
                        write-warning "$CommandInfo is not a Cmdlet."
                    } else {                    
                        write-warning "Cmdlet $CommandName does not exist in current scope. Have you loaded its containing module or snap-in?"
                    }
                }
                
                "type" {
                    $typeName = $type.fullname
                    $assemblyLocation = $type.assembly.location
                }                                
            } # end switch
            
            
            if ($typeName -and $assemblyLocation) {
                & $reflectorPath /select:$typeName $assemblyLocation
            }
            
        } else {
            write-warning "Unable to find Reflector.exe. Please specify full path via -ReflectorPath."
        }
    } # end end
} # end function

<#
.ForwardHelpTargetName Get-ChildItem
.ForwardHelpCategory Cmdlet
#>
function Get-ChildItem
{
    [CmdletBinding(DefaultParameterSetName='Items', SupportsTransactions=$true)]
    param(
        [Parameter(ParameterSetName='Items', Position=0, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
        [System.String[]]
        ${Path},

        [Parameter(ParameterSetName='LiteralItems', Mandatory=$true, Position=0, ValueFromPipelineByPropertyName=$true)]
        [Alias('PSPath')]
        [System.String[]]
        ${LiteralPath},

        [Parameter(Position=1)]
        [System.String]
        ${Filter},

        [System.String[]]
        ${Include},

        [System.String[]]
        ${Exclude},

        [Switch]
        ${Recurse},

        [Switch]
        ${Force},

        [Switch]
        ${Name},

        [Switch]
        ${ContainerOnly},
        
        [Switch]
        ${LeafOnly}
    )
    
    DynamicParam
    {
        if ($path -match ".*CERT.*:")
        {
            $attributes = new-object System.Management.Automation.ParameterAttribute
            $attributes.ParameterSetName = "__AllParameterSets"
            $attributes.Mandatory = $false
            $attributeCollection = new-object -Type System.Collections.ObjectModel.Collection[System.Attribute]
            $attributeCollection.Add($attributes)
            $dynParam = new-object -Type System.Management.Automation.RuntimeDefinedParameter("CodeSigningCert", [switch], $attributeCollection)
            $paramDictionary = new-object -Type System.Management.Automation.RuntimeDefinedParameterDictionary
            $paramDictionary.Add("CodeSigningCert", $dynParam)
            return $paramDictionary
        }
    }

    begin
    {
        try 
        {
            $wrappedCmd = $ExecutionContext.InvokeCommand.GetCommand('Microsoft.PowerShell.Management\Get-ChildItem', [System.Management.Automation.CommandTypes]::Cmdlet)

            $outBuffer = $null
            if ($PSBoundParameters.TryGetValue('OutBuffer', [ref]$outBuffer))
            {
                $PSBoundParameters['OutBuffer'] = 1
            }
            
            if ($ContainerOnly -and $LeafOnly)
            {
                throw "The parameters ContainerOnly and LeafOnly are mutually exclusive"
            }
            elseif ($ContainerOnly)
            {
                [void]$PSBoundParameters.Remove("ContainerOnly")
                $scriptCmd = {& $wrappedCmd @PSBoundParameters | Where-Object {$_.PSIsContainer}}                    
            }
            elseif ($LeafOnly)
            {
                [void]$PSBoundParameters.Remove("LeafOnly")
                $scriptCmd = {& $wrappedCmd @PSBoundParameters | Where-Object {!$_.PSIsContainer}}        
            }
            else
            {
                $scriptCmd = {& $wrappedCmd @PSBoundParameters }
            }      
            
            $steppablePipeline = $scriptCmd.GetSteppablePipeline($myInvocation.CommandOrigin)
            $steppablePipeline.Begin($PSCmdlet)
        } 
        catch 
        {
            throw
        }
    }

    process
    {
        try 
        {
            $steppablePipeline.Process($_)
        } 
        catch 
        {
            throw
        }
    }

    end
    {
        try 
        {
            $steppablePipeline.End()
        } 
        catch 
        {
            throw
        }
    }
}

<#
.SYNOPSIS
    Gets the specified property's value from each input object.
.DESCRIPTION
    Gets the specified property's value from each input object.
    This filter is different from the Select-Object cmdlet in that it
    doesn't create a wrapper object (PSCustomObject) around the property.
    If you just want to get the property's value to assign it to another
    variable this filter will come in handy.  If you assigned the result
    of the Select-Object operation you wouldn't get the property's value.
    You would get an object that wraps that property and its value.
.PARAMETER InputObject
    Any object from which to get the specified property
.EXAMPLE
    C:\PS> $start = Get-History -Id 143 | Get-PropertyValue StartExecutionTime
    Gets the value of the StartExecutionTime property off of each HistoryInfo object.
.NOTES
    Aliases:  gpv
    Author:   Keith Hill  
#>
filter Get-PropertyValue([string] $propertyName) {
    $_.$propertyName
}

Export-ModuleMember -Alias * -Function *