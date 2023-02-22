#Script to speed up development and testing of plugins
#Move script to plugin directory you want to build
#Give the script the name of your listener
#Samuel Barrows

param (
    
    [Parameter(Mandatory=$false)]$listenname
)

if ($null -eq $listenname){
    $listenname = "Default"
}

[string]$pwd = $(Get-Location)

$dirname = $pwd -split "\\"

dotnet build *>&1
if ($LASTEXITCODE -eq 0){
    Write-Host
    Write-Host "Moving $($dirname[-1]).dll to $($listenname)'s plugin folder"
    Write-Host

    move-item -Force ".\bin\Debug\net6.0\$($dirname[-1]).dll" "..\..\server\data\listeners\$($listenname)\plugins"
    copy-item -Force "$($dirname[-1]).json" "..\..\server\data\listeners\$($listenname)\plugins"
} else {
    Write-Error "FAILED"
    exit
}
