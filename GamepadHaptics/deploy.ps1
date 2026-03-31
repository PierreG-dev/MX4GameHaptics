$ErrorActionPreference = "Stop"

$PluginName = "GamepadHaptics"
$PluginBase = "$env:LocalAppData\Logi\LogiPluginService\Plugins\$PluginName"
$WinDir = "$PluginBase\win"
$MetaDir = "$PluginBase\metadata"

# --- 1. ARRET FORCE DU SERVICE ---
Write-Host "=== Arret des processus Logi/Loupedeck ===" -ForegroundColor Yellow
$processNames = @("LogiPluginService", "logioptionsplus", "LoupedeckConfig", "LoupedeckService", "LogiOptionsPlus_Agent")
foreach ($p in $processNames) {
    Stop-Process -Name $p -Force -ErrorAction SilentlyContinue
}
Write-Host "Attente de liberation des fichiers (5s)..." -ForegroundColor Gray
Start-Sleep -Seconds 5

# --- 2. BUILD ---
Write-Host "=== Build & Publish (Release win-x64) ===" -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --no-self-contained -o "./publish"
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERREUR : Echec du build dotnet." -ForegroundColor Red
    exit 1
}

# --- 3. NETTOYAGE DES DLL DUPLICATES ---
# On retire les DLL que Logi fournit déjà pour éviter les conflits
$excludeDlls = @("PluginApi.dll", "LoupedeckShared.dll", "LoupedeckService.dll", "NativeApi.dll", "LogiEventTracing.dll", "Newtonsoft.Json.dll", "SkiaSharp.dll", "YamlDotNet.dll")
foreach ($dll in $excludeDlls) {
    $p = "./publish/$dll"
    if (Test-Path $p) { Remove-Item $p -Force }
}

# --- 4. RECHERCHE VIGEM NATIVE (Correctif) ---
Write-Host "=== Verification ViGEm Native ===" -ForegroundColor Yellow
$nugetVigem = "$env:USERPROFILE\.nuget\packages\nefarius.vigem.client"
$vigemNative = Get-ChildItem -Path $nugetVigem -Recurse -Filter "ViGEmClient.dll" | 
               Where-Object { $_.FullName -like "*win-x64*" -and $_.FullName -like "*native*" } | 
               Select-Object -First 1

if ($vigemNative) {
    Copy-Item $vigemNative.FullName "./publish/" -Force
    Write-Host "  [OK] ViGEmClient.dll native trouvee et incluse." -ForegroundColor Green
} else {
    Write-Host "  [!] ATTENTION: ViGEmClient.dll introuvable dans NuGet. Le plugin risque de ne pas demarrer." -ForegroundColor Red
}

# --- 5. DEPLOIEMENT ---
Write-Host "=== Deploiement vers Logi Service ===" -ForegroundColor Yellow
if (!(Test-Path $WinDir)) { New-Item -ItemType Directory -Path $WinDir -Force | Out-Null }
if (!(Test-Path $MetaDir)) { New-Item -ItemType Directory -Path $MetaDir -Force | Out-Null }

# On essaie de vider le dossier, si ça rate on prévient
try {
    Remove-Item "$WinDir\*" -Recurse -Force -ErrorAction Stop
} catch {
    Write-Host "ERREUR : Impossible de vider le dossier /win. Le fichier est encore verrouille." -ForegroundColor Red
    Write-Host "Tentez de fermer 'Logi Options+' manuellement dans la barre des taches." -ForegroundColor Yellow
    exit 1
}

Copy-Item "./publish/*" $WinDir -Recurse -Force
Write-Host "Fichiers copies avec succes." -ForegroundColor Green

# --- 6. REDEMARRAGE ET LOGS ---
Write-Host "=== Redemarrage du service ===" -ForegroundColor Yellow
$servicePath = "C:\Program Files\Logi\LogiPluginService\LogiPluginService.exe"
if (Test-Path $servicePath) {
    Start-Process $servicePath
    Write-Host "Service relance. Attente de l'initialisation (8s)..." -ForegroundColor Gray
    Start-Sleep -Seconds 8
}

Write-Host "=== Dernieres lignes du log du plugin ===" -ForegroundColor Cyan
$logPath = "$env:LocalAppData\Logi\LogiPluginService\Logs\plugin_logs\$PluginName.log"
if (Test-Path $logPath) {
    Get-Content $logPath -Tail 20
} else {
    Write-Host "Aucun fichier log genere pour $PluginName." -ForegroundColor Red
}