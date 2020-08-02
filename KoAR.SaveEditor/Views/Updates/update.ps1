try {
	$editorPid = $args[0] -as [int];
	$executable = (Get-Process -id $editorPid).Path;
	$directory = [System.IO.Path]::GetDirectoryName($executable);
	Stop-Process -id $editorPid;
	Wait-Process -id $editorPid;
	$zipFile = $args[1];
	Expand-Archive -Path $zipFile -DestinationPath $directory -Force;
	& $executable;
}
catch {
	Write-Host "An error occurred:"
	Write-Host $_
}
