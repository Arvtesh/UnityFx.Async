$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$packagesPath = Join-Path $scriptPath "..\temp\BuildTools"
$nugetPath = Join-Path $packagesPath "nuget.exe"
$docfxPath = Join-Path $packagesPath "docfx.console\tools\docfx.exe"
$docfxConfigPath = Join-Path $scriptPath "DocFx"
$docfxPublishTarget = Join-Path $scriptPath "..\docs\*"

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

if (Test-Path $docfxPublishTarget)
{
	Remove-Item $docfxPublishTarget -Recurse
}

Push-Location $docfxConfigPath
& $docfxPath
Pop-Location

