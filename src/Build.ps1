$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$solutionPath = Join-Path $scriptPath "UnityFx.sln"
$configuration = $args[0]
$packagesPath = Join-Path $scriptPath "..\temp\BuildTools"
$binPath = Join-Path $scriptPath "..\bin"
$binPath35 = Join-Path $binPath "net35"
$binPath46 = Join-Path $binPath "net46"
$assetsPath35 = Join-Path $scriptPath "..\AssetStore\UnityPackage35\Assets\UnityFx"
$assetsPath46 = Join-Path $scriptPath "..\AssetStore\UnityPackage46\Assets\UnityFx"
$assetsPathTests = Join-Path $scriptPath "..\Tests\UnityTests\Assets\UnityFx"
$msbuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MsBuild.exe"
$nugetPath = Join-Path $packagesPath "nuget.exe"
$gitversionPath = Join-Path $packagesPath "gitversion.commandline\tools\gitversion.exe"

Write-Host "BasePath:" $scriptPath
Write-Host "PackagePath:" $packagesPath
Write-Host "BinPath:" $binPath

# create output folders if needed
if (!(Test-Path $packagesPath))
{
	New-Item $packagesPath -ItemType Directory
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

# install & run GitVersion
Write-Host "Install/update GetVersion" -Foreground Blue
& $nugetPath install -excludeversion gitversion.commandline -outputdirectory $packagesPath
& $gitversionPath /l console /output buildserver

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

# publish build results to .\bin
$filesToPublish35 =
	(Join-Path $scriptPath (Join-Path "bin" (Join-Path $configuration "net35\UnityFx.Async.dll"))),
	(Join-Path $scriptPath (Join-Path "bin" (Join-Path $configuration "net35\UnityFx.Async.xml")))

$filesToPublish46 =
	(Join-Path $scriptPath (Join-Path "bin" (Join-Path $configuration "net46\UnityFx.Async.dll"))),
	(Join-Path $scriptPath (Join-Path "bin" (Join-Path $configuration "net46\UnityFx.Async.xml")))

Copy-Item -Path $filesToPublish35 -Destination $assetsPath35 -Force
Copy-Item -Path $filesToPublish35 -Destination $binPath35 -Force
Copy-Item -Path $filesToPublish46 -Destination $assetsPath46 -Force
Copy-Item -Path $filesToPublish46 -Destination $binPath46 -Force
Copy-Item -Path $filesToPublish46 -Destination $assetsPathTests -Force
