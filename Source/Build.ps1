$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$solutionPath = Join-Path $scriptPath "UnityFx.sln"
$configuration = $args[0]
$outputPath = Join-Path $scriptPath "..\Build"
$binPath = Join-Path $outputPath (Join-Path "Bin" $configuration)
$binPath35 = Join-Path $binPath "net35"
$binPath46 = Join-Path $binPath "net46"
$assetsPath35 = Join-Path $scriptPath "..\UnityApp35\Assets\UnityFx"
$assetsPath46 = Join-Path $scriptPath "..\UnityApp46\Assets\UnityFx"
$msbuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MsBuild.exe"
$nugetPath = Join-Path $outputPath "nuget.exe"
$gitversionPath = Join-Path $outputPath "gitversion.commandline\tools\gitversion.exe"

# create output folders if needed
if (!(Test-Path $outputPath))
{
	New-Item $outputPath -ItemType Directory
}

if (!(Test-Path $binPath35))
{
	New-Item $binPath35 -ItemType Directory
}

if (!(Test-Path $binPath46))
{
	New-Item $binPath46 -ItemType Directory
}


# download nuget.exe if not present
if (!(Test-Path $nugetPath))
{
	Write-Host "Install NuGet" -Foreground Blue
	Invoke-WebRequest "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -Outfile $nugetPath
}

# install GitVersion
Write-Host "Install/update GetVersion" -Foreground Blue
& $nugetPath install -excludeversion gitversion.commandline -outputdirectory $outputPath

# run GitVersion
Write-Host "Update project version" -Foreground Blue
& $gitversionPath /l console /output buildserver /updateassemblyinfo

# install pdb2mdb
Write-Host "Install/update pdb2mdb" -Foreground Blue
& $nugetPath install -excludeversion Mono.pdb2mdb -outputdirectory $outputPath

# build projects
Write-Host "Building projects" -Foreground Blue
& $nugetPath restore $solutionPath
& $msbuildPath $solutionPath /m /t:Build /p:Configuration=$configuration

# fail if solution build failed
if ($LastExitCode -ne 0)
{
	if ($env:CI -eq 'True')
	{
		$host.SetShouldExit($LastExitCode)
	}
	else
	{
		exit($LastExitCode);
	}
}

# publish build results to .\Build\Bin
$filesToPublish35 =
	(Join-Path $scriptPath (Join-Path "UnityFx.Async\bin" (Join-Path $configuration "net35\UnityFx.Async.dll"))),
	(Join-Path $scriptPath (Join-Path "UnityFx.Async\bin" (Join-Path $configuration "net35\UnityFx.Async.xml")))

$filesToPublish46 =
	(Join-Path $scriptPath (Join-Path "UnityFx.Async\bin" (Join-Path $configuration "net46\UnityFx.Async.dll"))),
	(Join-Path $scriptPath (Join-Path "UnityFx.Async\bin" (Join-Path $configuration "net46\UnityFx.Async.xml")))

Copy-Item -Path $filesToPublish35 -Destination $binPath35 -Force
Copy-Item -Path $filesToPublish46 -Destination $binPath46 -Force
Copy-Item -Path $filesToPublish46 -Destination $assetsPath46 -Force

# revert AssemblyInfo's back
if ($env:CI -ne 'True')
{
	Write-Host "Reverting AssemblyInfo's" -Foreground Blue
	Get-ChildItem $scriptPath -re -in AssemblyInfo.cs | ForEach-Object { git checkout $_ } 
}
