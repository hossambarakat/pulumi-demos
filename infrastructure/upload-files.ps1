
[CmdletBinding()]
param (
    [Parameter(Mandatory=$false)]
    [string]
    $ConnectionString = "DefaultEndpointsProtocol=https;AccountName=coolwebsite;AccountKey=yGeCYEe1WrFWkKD92euhY2+5otVr7bUlj7WjFD1zk0t8efobY0nNm52zVfmCcYLsH585WVxVMW/XOFHb5vuDZg==;EndpointSuffix=core.windows.net",
    [Parameter()]
    [string]
    $BuildDirectory = "../timezone-ui"
)
#Install-Module -Name Az -AllowClobber -Scope CurrentUser
#Connect-AzAccount

$context = New-AzStorageContext -ConnectionString $ConnectionString
$token = New-AzStorageContainerSASToken -Container "`$web" -Context $context -Permission rwld
$WebSASUrl = "$($context.BlobEndPoint)`$web$token"

Push-Location $BuildDirectory

$env:REACT_APP_BASE_URL = "http://www.hoselbos.com/"
yarn build

azcopy rm "$WebSASUrl" --recursive
azcopy copy "./build/*" "$WebSASUrl" --recursive

Pop-Location

Write-Host "Done"
