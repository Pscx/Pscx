# String catalog for scripts in the deprecated module
# If you localize this catalog put the resulting Messages.psd1 
# file in the appropriate <language>-<culture> subdir e.g. de-DE.

ConvertFrom-StringData @'
DeprecatedFunctionWarningF2={0} has been deprecated and will be removed in a future version of PSCX. {1}
DeprecatedRedirectedCmdWarningF2={0} has been deprecated and will be removed in a future version of PSCX. This function is a thin wrapper over the new, built-in PowerShell cmdlet {1}. Please update your script to use this cmdlet. 
'@