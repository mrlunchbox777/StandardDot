$allDirectories = [System.IO.Directory]::GetDirectories("src")
$directories = New-Object System.Collections.ArrayList

foreach ($directory in $allDirectories)
{
	$directoryName = $directory.ToLower()
	if ((-not $directoryName.Contains(".")) -and (-not $directoryName.Contains("test")))
	{
		$directories.Add($directory)
	}
}

foreach($directory in $directories)
{
	$buildCake = $directory + "/build.cake"
	Write-Host $buildCake
	if (-not [System.IO.File]::Exists($buildCake))
	{
		continue;
	}

	$buildMe = $directory + "/build.me"
	Write-Host $buildMe
	if ([System.IO.File]::Exists($buildMe))
	{
		[System.IO.File]::Delete($buildMe)
	}
	else
	{
		[System.IO.File]::WriteAllText($buildMe, "")
	}
}