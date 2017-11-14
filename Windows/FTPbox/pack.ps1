# The following parameters should be passed from Visual Studio

param (
    [string]$Name,
    [string]$ProjectDir,
    [string]$TargetDir,
    [string]$TargetPath
)

# Load built assembly and get the version

$assembly = [System.Reflection.Assembly]::LoadFile($TargetPath)
$v = $assembly.GetName().Version;
$version = [string]::Format("{0}.{1}.{2}",$v.Major, $v.Minor, $v.Build)

# NuGet pack

$nuspec = "$ProjectDir$Name.nuspec"
Write-Host "Path to nuspec file: " $nuspec

nuget pack $nuspec -Version $version -Properties Configuration=Release -OutputDirectory $TargetDir -BasePath $TargetDir

# Squirrel Releasify

$icon = $ProjectDir + "regular.ico"
Write-Host "Path to ico file: " $icon

$nupkg = "FTPbox.$version.nupkg"
Write-Host "Path to generated nupkg: " $nupkg

New-Alias squirrel $ProjectDir\..\packages\squirrel.windows*\tools\Squirrel.exe -Force

squirrel --releasify $nupkg --setupIcon $icon --icon $icon | Write-Output

# Rename setup file

$setup = $TargetDir + "Releases\Setup.exe"
$newSetup = $TargetDir + "Releases\FTPbox-$version-Setup.exe"

Move-Item $setup $newSetup -Force
Write-Host "Setup path: " $newSetup

# Rename msi file

$msi = $TargetDir + "Releases\Setup.msi"
$newmsi = $TargetDir + "Releases\FTPbox-$version-Setup.msi"

Move-Item $msi $newmsi -Force
Write-Host "msi path: " $newmsi