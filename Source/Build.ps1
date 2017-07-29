$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$solutionPath = Join-Path $scriptPath "UnityFx.sln"
$configuration = $args[0]
$outputPath = Join-Path $scriptPath "..\Build"
$binPath = Join-Path $outputPath (Join-Path "Bin" $configuration)
$binEditorPath = Join-Path $binPath "Editor"
$samplesPath = Join-Path $scriptPath "..\Samples"
$msbuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MsBuild.exe"
$nugetPath = Join-Path $outputPath "nuget.exe"
$gitversionPath = Join-Path $outputPath "gitversion.commandline\tools\gitversion.exe"

# create output folders if needed
if (!(Test-Path $outputPath))
{
	New-Item $outputPath -ItemType Directory
}

if (!(Test-Path $binPath))
{
	New-Item $binPath -ItemType Directory
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
$filesToPublish =
	(Join-Path $scriptPath (Join-Path "UnityFx.Async\bin" (Join-Path $configuration "UnityFx.Async.dll"))),
	(Join-Path $scriptPath (Join-Path "UnityFx.Async\bin" (Join-Path $configuration "UnityFx.Async.pdb"))),
	(Join-Path $scriptPath (Join-Path "UnityFx.Async\bin" (Join-Path $configuration "UnityFx.Async.XML")))

Copy-Item -Path $filesToPublish -Destination $binPath -Force

# revert AssemblyInfo's back
if ($env:CI -ne 'True')
{
	Write-Host "Reverting AssemblyInfo's" -Foreground Blue
	Get-ChildItem $scriptPath -re -in AssemblyInfo.cs | ForEach-Object { git checkout $_ } 
}
