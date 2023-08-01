$fileNames = Get-ChildItem -Path $scriptPath -Recurse

foreach ($file in $fileNames)
{
	#Use relative path to allow subdirectories
	$relativePath = Resolve-Path -Relative $file.FullName

	if ($relativePath.EndsWith("vert") -Or $relativePath.EndsWith("frag") -Or $relativePath.EndsWith("comp"))
	{
		Write-Host "Compiling $relativePath"
		./bin/glslc -o $relativePath".spv" $relativePath
	}
}

Write-Host -NoNewLine 'Press any key to continue...';
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown');
