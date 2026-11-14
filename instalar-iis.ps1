# Instala y publica la API en IIS. Ejecutar en PowerShell como administrador:
#   powershell -ExecutionPolicy Bypass -File .\instalar-iis.ps1
param(
    [string]$Origen = "$PSScriptRoot\publish",
    [string]$Destino = "C:\inetpub\api_pacientes",
    [int]$Puerto = 5080
)

$ErrorActionPreference = "Stop"
$nombre = "ApiPacientes"

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Ejecuta este script como administrador." -ForegroundColor Red
    exit 1
}

if (-not (Test-Path "$Origen\ApiPacientes.dll")) {
    Write-Host "No se encontro la API publicada en $Origen" -ForegroundColor Red
    exit 1
}

Write-Host "1/6 Activando IIS..."
$features = "IIS-WebServerRole", "IIS-WebServer", "IIS-CommonHttpFeatures", "IIS-DefaultDocument",
            "IIS-HttpErrors", "IIS-StaticContent", "IIS-RequestFiltering", "IIS-ManagementConsole",
            "IIS-ManagementScriptingTools"
foreach ($f in $features) {
    Enable-WindowsOptionalFeature -Online -FeatureName $f -All -NoRestart | Out-Null
}

Write-Host "2/6 Instalando ASP.NET Core Hosting Bundle 8..."
if (-not (Test-Path "$env:ProgramFiles\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll")) {
    $instalador = "$env:TEMP\dotnet-hosting-8.exe"
    Invoke-WebRequest "https://aka.ms/dotnet/8.0/dotnet-hosting-win.exe" -OutFile $instalador
    Start-Process $instalador -ArgumentList "/install /quiet /norestart" -Wait
    net stop was /y | Out-Null
    net start w3svc | Out-Null
} else {
    Write-Host "   Ya estaba instalado."
}

Import-Module WebAdministration

Write-Host "3/6 Copiando archivos a $Destino..."
if (Test-Path "IIS:\Sites\$nombre") { Stop-Website $nombre }
if (Test-Path "IIS:\AppPools\$nombre") { Stop-WebAppPool $nombre -ErrorAction SilentlyContinue; Start-Sleep 2 }
New-Item -ItemType Directory -Force -Path $Destino | Out-Null
# No se toca App_Data para conservar la base existente
Get-ChildItem $Origen -Exclude "App_Data" | Copy-Item -Destination $Destino -Recurse -Force
New-Item -ItemType Directory -Force -Path "$Destino\App_Data" | Out-Null

Write-Host "4/6 Creando pool y sitio..."
if (-not (Test-Path "IIS:\AppPools\$nombre")) {
    New-WebAppPool $nombre | Out-Null
}
Set-ItemProperty "IIS:\AppPools\$nombre" -Name managedRuntimeVersion -Value ""
if (-not (Test-Path "IIS:\Sites\$nombre")) {
    New-Website -Name $nombre -PhysicalPath $Destino -ApplicationPool $nombre -Port $Puerto -IPAddress "*" | Out-Null
}

Write-Host "5/6 Permisos de escritura para SQLite..."
icacls "$Destino\App_Data" /grant "IIS AppPool\${nombre}:(OI)(CI)M" | Out-Null

Write-Host "6/6 Abriendo puerto $Puerto en el firewall..."
if (-not (Get-NetFirewallRule -DisplayName $nombre -ErrorAction SilentlyContinue)) {
    New-NetFirewallRule -DisplayName $nombre -Direction Inbound -Protocol TCP -LocalPort $Puerto -Action Allow | Out-Null
}

Start-WebAppPool $nombre
Start-Website $nombre

$ips = Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -notlike "127.*" -and $_.IPAddress -notlike "169.254.*" }
Write-Host ""
Write-Host "Listo. La API responde en:" -ForegroundColor Green
Write-Host "   http://localhost:$Puerto/swagger"
foreach ($ip in $ips) { Write-Host "   http://$($ip.IPAddress):$Puerto  (usar esta en la app si estas en la misma red)" }
