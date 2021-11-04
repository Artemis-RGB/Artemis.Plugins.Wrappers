$key = $args[0]
Write-Output ('Unpatching LGS DLL at ' + $key)

$exists = Test-Path $key;
if ($exists -eq $False) {
	Write-Output 'Not patched!'
}
else {
	$originalValue = (Get-ItemProperty -Path $key).Artemis
	# Put back the original value if found
	if ($originalValue -ne $null) {
		Write-Output 'Restoring original value'
		Set-ItemProperty -Path $key -Name '(Default)' -Value $originalValue;
		Remove-ItemProperty -Path $key -Name 'Artemis'
	} 
	# Otherwise get rid of the entire key
	else {
		Remove-Item -Path $key -Force
	}	
}

Start-Sleep 1