$editorPid = $args[0] -as [int];
$zipFile = $args[1];
try {	
	$executable = (Get-Process -id $editorPid).Path;
	$directory = [System.IO.Path]::GetDirectoryName($executable);
	Stop-Process -id $editorPid;
	Wait-Process -id $editorPid;
	Expand-Archive -Path $zipFile -DestinationPath $directory -Force;
	Start-Procsss -WorkingDirectory $directory -FilePath $executable;
}
catch {
	Write-Host "An error occurred:";
	Write-Host $_;
}
finally {
	Remove-Item –Path $zipFile;
	Remove-Item –Path $PSCommandPath;
}
