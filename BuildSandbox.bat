powershell .\src\Build.ps1 Release
rd unity\Sandbox\Assets\Plugins /S /Q
xcopy bin\AssetStore\netstandard2.0 unity\Sandbox /S
