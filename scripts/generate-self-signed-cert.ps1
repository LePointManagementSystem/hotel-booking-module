param(
    [string]$DnsName = "localhost",
    [string]$Password = "change-this-pfx-password",
    [string]$OutputDirectory = "certs",
    [int]$YearsValid = 2
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$targetDir = Join-Path $repoRoot $OutputDirectory

if (-not (Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir | Out-Null
}

$cert = New-SelfSignedCertificate `
    -DnsName $DnsName `
    -CertStoreLocation "cert:\CurrentUser\My" `
    -FriendlyName "HotelBookingPlatform Self-Signed TLS" `
    -TextExtension @("2.5.29.17={text}DNS=$DnsName") `
    -NotAfter (Get-Date).AddYears($YearsValid)

$securePassword = ConvertTo-SecureString -String $Password -Force -AsPlainText
$pfxPath = Join-Path $targetDir "hotelbooking-selfsigned.pfx"

Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $securePassword | Out-Null

Write-Host "Created certificate at: $pfxPath"
Write-Host "Use CERT_PASSWORD=$Password in your deployment environment."
