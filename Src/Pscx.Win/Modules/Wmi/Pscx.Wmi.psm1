Set-StrictMode -Version Latest

$acceleratorsType = [psobject].Assembly.GetType('System.Management.Automation.TypeAccelerators')

# If these accelerators have already been defined, don't override (and don't error)
function AddAccelerator($name, $type)
{
    if (!$acceleratorsType::Get.ContainsKey($name))
    {
        $acceleratorsType::Add($name, $type)
    }
}

AddAccelerator "wmidatetime"  ([Pscx.Win.Fwk.TypeAccelerators.WmiDateTime])
AddAccelerator "wmitimespan"  ([Pscx.Win.Fwk.TypeAccelerators.WmiTimeSpan])

# Export nothing
Export-ModuleMember
