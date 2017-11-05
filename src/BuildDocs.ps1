$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$packagesPath = Join-Path $scriptPath "..\temp\BuildTools"
$binPath = Join-Path $scriptPath "bin\Release\net46"
$nugetPath = Join-Path $packagesPath "nuget.exe"
$docfxSrcFiles = (Join-Path $binPath "UnityFx.Async.dll"), (Join-Path $binPath "UnityFx.Async.xml")
$docfxPath = Join-Path $packagesPath "docfx.console\tools\docfx.exe"
$docfxConfigPath = Join-Path $scriptPath "DocFx"
$docfxPublishTarget = Join-Path $scriptPath "..\docs\*"
$docfxSrcPath = Join-Path $docfxConfigPath "src"

# docfx workaround
$env:VisualStudioVersion = "15.0"
$env:VSINSTALLDIR = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\"

# download nuget.exe if not present
if (!(Test-Path $nugetPath))
{
	Write-Host "Install NuGet" -Foreground Blue
	Invoke-WebRequest "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -Outfile $nugetPath
}

# install docfx
Write-Host "Install/update DocFx" -Foreground Blue
& $nugetPath install -excludeversion docfx.console -outputdirectory $packagesPath

# run docfx
Write-Host "Run DocFx" -Foreground Blue
Remove-Item $docfxPublishTarget -Recurse
Copy-Item -Path $docfxSrcFiles -Destination $docfxSrcPath -Force
Push-Location $docfxConfigPath
& $docfxPath
Pop-Location 
