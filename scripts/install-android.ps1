<#
.SYNOPSIS
    Instala el APK debug de SnapClone en un dispositivo conectado y lo inicia.

.DESCRIPTION
    Valida que haya un dispositivo visible para adb, instala el APK mas
    reciente (o el indicado en -ApkPath) y lanza la actividad principal.

.PARAMETER ApkPath
    Ruta del APK. Si se omite, se usa el APK firmado mas reciente de Debug.

.PARAMETER AndroidSdk
    Ruta del Android SDK. Por defecto %LOCALAPPDATA%\Android\Sdk.
#>
[CmdletBinding()]
param(
    [string]$ApkPath,
    [string]$AndroidSdk = $(if ($env:ANDROID_HOME) { $env:ANDROID_HOME } else { "$env:LOCALAPPDATA\Android\Sdk" })
)

$ErrorActionPreference = "Continue"
$repoRoot = Split-Path -Parent $PSScriptRoot
$packageId = "com.integritysolution.snapclone"
$adb = Join-Path $AndroidSdk "platform-tools\adb.exe"

if (-not (Test-Path $adb)) {
    throw "No se encontro adb en '$adb'. Instala Android SDK Platform-Tools o pasa -AndroidSdk <ruta>."
}

if (-not $ApkPath) {
    $outputDir = Join-Path $repoRoot "SnapClone.Android\bin\Debug\net9.0-android"
    $latest = Get-ChildItem $outputDir -Filter "*-Signed.apk" -Recurse -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if (-not $latest) {
        throw "No hay APK en '$outputDir'. Ejecuta scripts\build-android.ps1 primero."
    }

    $ApkPath = $latest.FullName
}

if (-not (Test-Path $ApkPath)) {
    throw "No existe el APK indicado: '$ApkPath'."
}

$devices = & $adb devices | Where-Object { $_ -match '^\S+\s+device$' }
if (-not $devices) {
    throw "No hay ningun dispositivo Android conectado. Conecta el telefono con depuracion USB activada y autoriza la maquina; luego repite este script."
}

Write-Host "Instalando $ApkPath en:"
$devices | ForEach-Object { Write-Host "  $_" }

& $adb install -r $ApkPath
if ($LASTEXITCODE -ne 0) {
    throw "adb install fallo (codigo $LASTEXITCODE)."
}

$activity = (& $adb shell cmd package resolve-activity --brief $packageId) | Select-Object -Last 1
if (-not $activity -or $activity -notmatch '/') {
    throw "No se pudo resolver la actividad de lanzamiento de '$packageId'."
}

& $adb shell am start -n $activity.Trim()
if ($LASTEXITCODE -ne 0) {
    throw "No se pudo iniciar la aplicacion (codigo $LASTEXITCODE)."
}

Write-Host "SnapClone iniciado en el dispositivo."
