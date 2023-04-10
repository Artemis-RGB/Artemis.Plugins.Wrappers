$key = $args[0]
$value = $args[1]
$keyParts = $key.Split('\')
$keyName = $keyParts[-1].ToString()
$keyPath = $key.Substring(0, ($key.Length - $keyName.Length - 1));

Write-Output ('Patching LGS DLL at ' + $key)

$exists = Test-Path $key;
if ($exists -eq $False) {
	# -Force is supplied to enforce creating the entire tree
	New-Item -Path $keyPath -Name $keyName -Value $value -Force
}
else {
	# Back up the original value first
	$originalValue = (Get-Item -Path $key).GetValue('')
	if ($originalValue -eq $value) {
		Write-Output 'Already patched!'
	}
 	else {
		Set-ItemProperty -Path $key -Name Artemis -Value $originalValue
		Set-Item -Path $key -Value $value
	}
}

Start-Sleep 1