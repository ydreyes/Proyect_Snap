<#
.SYNOPSIS
    Compila el head Android de SnapClone y muestra la ruta del APK firmado.

.DESCRIPTION
    Prepara JAVA_HOME/ANDROID_HOME para la sesion y falla con un mensaje claro
    cuando falta el JDK 17, la plataforma android-35 o el workload de .NET 9.

.PARAMETER Configuration
    Configuracion de MSBuild. Por defecto Debug (APK firmado con la debug key).

.PARAMETER JavaHome
    Ruta del JDK 17. Si se omite, se usa JAVA_HOME o se busca en
    %LOCALAPPDATA%\Microsoft\jdk-17.

.PARAMETER AndroidSdk
    Ruta del Android SDK. Por defecto %LOCALAPPDATA%\Android\Sdk.
#>
[CmdletBinding()]
param(
    [string]$Configuration = "Debug",
    [string]$JavaHome = $env:JAVA_HOME,
    [string]$AndroidSdk = $(if ($env:ANDROID_HOME) { $env:ANDROID_HOME } else { "$env:LOCALAPPDATA\Android\Sdk" })
)

$ErrorActionPreference = "Continue"
$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "SnapClone.Android\SnapClone.Android.csproj"

function Resolve-JavaHome {
    param([string]$Candidate)

    if ($Candidate -and (Test-Path (Join-Path $Candidate "bin\java.exe"))) {
        return $Candidate
    }

    $searchRoot = Join-Path $env:LOCALAPPDATA "Microsoft\jdk-17"
    if (Test-Path $searchRoot) {
        $found = Get-ChildItem $searchRoot -Directory |
            Sort-Object Name -Descending |
            Where-Object { Test-Path (Join-Path $_.FullName "bin\java.exe") } |
            Select-Object -First 1

        if ($found) {
            return $found.FullName
        }
    }

    return $null
}

$resolvedJavaHome = Resolve-JavaHome -Candidate $JavaHome
if (-not $resolvedJavaHome) {
    throw "No se encontro un JDK 17. Instala Microsoft OpenJDK 17 o pasa -JavaHome <ruta>. Ver README.md."
}

$javaVersion = (& (Join-Path $resolvedJavaHome "bin\java.exe") -version 2>&1 | Out-String)
if ($javaVersion -notmatch 'version "17\.') {
    throw "El JDK en '$resolvedJavaHome' no es la version 17 ($javaVersion). .NET para Android 35 requiere JDK 17; ver README.md."
}

if (-not (Test-Path (Join-Path $AndroidSdk "platforms\android-35\android.jar"))) {
    throw "Falta la plataforma android-35 en '$AndroidSdk'. Instalala con: sdkmanager ""platforms;android-35"". Ver README.md."
}

$env:JAVA_HOME = $resolvedJavaHome
$env:ANDROID_HOME = $AndroidSdk
$env:ANDROID_SDK_ROOT = $AndroidSdk

Push-Location $repoRoot
try {
    & dotnet workload list | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "El SDK de .NET no esta disponible. Instala el SDK 9.0.3xx y ejecuta: dotnet workload install android. Ver README.md."
    }

    & dotnet build $projectPath -c $Configuration --nologo
    if ($LASTEXITCODE -ne 0) {
        throw "La compilacion Android fallo (codigo $LASTEXITCODE). Revisa la salida anterior."
    }
}
finally {
    Pop-Location
}

$outputDir = Join-Path $repoRoot "SnapClone.Android\bin\$Configuration\net9.0-android"
$apk = Get-ChildItem $outputDir -Filter "*-Signed.apk" -Recurse -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $apk) {
    throw "La compilacion termino pero no se encontro ningun APK firmado en '$outputDir'."
}

Write-Host ""
Write-Host "APK: $($apk.FullName)"
