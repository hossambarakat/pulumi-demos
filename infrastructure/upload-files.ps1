
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]
    $ConnectionString,
    [Parameter(Mandatory=$true)]
    [string]
    $BaseUrl,
    
    [Parameter()]
    [string]
    $BuildDirectory = "../timezone-ui/ClientApp"
)
#Install-Module -Name Az -AllowClobber -Scope CurrentUser
#Connect-AzAccount

$context = New-AzStorageContext -ConnectionString $ConnectionString
$token = New-AzStorageContainerSASToken -Container "`$web" -Context $context -Permission rwld
$WebSASUrl = "$($context.BlobEndPoint)`$web$token"

Push-Location $BuildDirectory

$env:REACT_APP_BASE_URL = $BaseUrl
yarn build

azcopy rm "$WebSASUrl" --recursive
azcopy copy "./build/*" "$WebSASUrl" --recursive

Pop-Location

Write-Host "Done"
