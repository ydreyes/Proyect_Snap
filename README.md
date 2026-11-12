# SnapClone

Juego de cartas por turnos con estructura roguelike, inspirado en el modo draft de Marvel Snap. Corre en escritorio con MonoGame DesktopGL y en Android con MonoGame Android. Toda la presentacion es procedural: no hay imagenes externas; las cartas, los fondos, los paneles y las escenas se dibujan por codigo.

## Stack

- .NET 9 (`net9.0` en escritorio, `net9.0-android` en Android).
- MonoGame 3.8.6.
- FontStashSharp 1.6.2 con las fuentes TTF embebidas.
- OpenSpec para especificaciones y cambios.

## Requisitos

### Escritorio

- **.NET SDK 9.0.3xx**. El repo fija la banda con `global.json`
  (`rollForward: latestFeature`), asi que `dotnet --version` dentro del repo
  debe reportar `9.0.3xx` (probado con `9.0.320`).
- No hace falta el workload de Android, el Android SDK ni el JDK para compilar
  y ejecutar la version de escritorio.

### Android

- **Workload de Android** para .NET 9:

  ```powershell
  dotnet workload install android
  ```

- **JDK 17** (Microsoft OpenJDK 17). .NET para Android 35 no acepta JDK 21+.
- **Android SDK** con la plataforma `android-35` y `platform-tools`:

  ```powershell
  sdkmanager "platforms;android-35" "platform-tools"
  ```

- Un dispositivo con depuracion USB activada o un emulador. El APK debug se
  firma con la debug key.

Instalacion del JDK 17 en Windows sin privilegios de administrador:

```powershell
curl.exe -L -o "$env:TEMP\microsoft-jdk-17.zip" "https://aka.ms/download-jdk/microsoft-jdk-17-windows-x64.zip"
Expand-Archive "$env:TEMP\microsoft-jdk-17.zip" "$env:LOCALAPPDATA\Microsoft\jdk-17" -Force
```

## Escritorio (PC)

```powershell
dotnet restore
dotnet build SnapClone.sln
dotnet run --project SnapClone\SnapClone.csproj
```

`SnapClone.sln` contiene solo el proyecto de escritorio. Android y el harness se
compilan aparte.

## Verificaciones (harness)

`SnapClone.Harness` es una aplicacion de consola que valida el contenido y las
mecanicas sin abrir ventana ni tocar disco real:

```powershell
dotnet run --project SnapClone.Harness\SnapClone.Harness.csproj
```

Verifica catalogos, efectos de cartas, perks, series 1-3, items, tienda,
eventos, persistencia y fundamentos de UI. Devuelve `0` cuando todo pasa y `1`
con la lista de fallos.

## Android

Compilar el APK debug:

```powershell
scripts\build-android.ps1
```

El script prepara `JAVA_HOME`/`ANDROID_HOME` para la sesion, compila en `Debug` y
termina imprimiendo la ruta absoluta del APK firmado:

```text
SnapClone.Android\bin\Debug\net9.0-android\com.integritysolution.snapclone-Signed.apk
```

Instalar y lanzar en un dispositivo conectado:

```powershell
scripts\install-android.ps1
```

Instalacion manual:

```powershell
adb install -r SnapClone.Android\bin\Debug\net9.0-android\com.integritysolution.snapclone-Signed.apk
```

### Datos de la aplicacion

| Campo | Valor |
|---|---|
| Application id | `com.integritysolution.snapclone` |
| Nombre visible | SnapClone |
| minSdk | 24 (Android 7.0) |
| targetSdk | 35 |
| ABI | `arm64-v8a` y `armeabi-v7a` |
| Orientacion | portrait bloqueado, pantalla completa inmersiva |
| Datos en escritorio | `%LOCALAPPDATA%\SnapClone` |
| Datos en Android | directorio `files` privado de la app |

El guardado usa escritura atomica: escribe un `.tmp`, conserva el documento
anterior como `.bak` y mueve el temporal sobre el destino. Los archivos
principales son `profile.json` y `run.json`.

## Estructura

```text
SnapClone/            motor, contenido y escenas (net9.0, DesktopGL)
SnapClone.Android/    host Android (net9.0-android), enlaza los .cs compartidos
SnapClone.Harness/    verificaciones headless (net9.0)
scripts/              build-android.ps1 e install-android.ps1
openspec/             especificaciones y cambios
docs/                 diseno y capturas
```

Carpetas principales del proyecto compartido:

- `Core/`: servicios, layout y maquina de estados.
- `Data/`: definiciones, registros y carga de contenido.
- `Effects/`: sistema de efectos e implementaciones por serie.
- `Gameplay/`: tablero, cartas, combate, draft, run, meta, IA y acciones.
- `Persistence/`: almacenes JSON, DTOs y checkpoints.
- `Scenes/`: pantallas del juego.
- `UI/`: componentes, renderizado, tema y primitivas.

El contenido (cartas, zonas, perks, enemigos, items y eventos) se registra en
`SnapClone/Data/Content/ContentLoader.cs`. Los textos de contenido estan en
espanol ASCII para evitar problemas con las fuentes.

## OpenSpec

El proyecto usa OpenSpec para separar especificaciones durables y cambios en
curso:

```powershell
openspec list
openspec list --specs
openspec show <spec-id> --type spec
openspec validate --all
```

Los cambios viven en `openspec/changes/` y las capacidades durables en
`openspec/specs/`. El flujo habitual es proponer, aplicar y archivar.

## Solucion de problemas

- `dotnet --version` debe caer en la banda `9.0.3xx`; si no, revisa
  `global.json` y el SDK instalado.
- Si el build de Android falla, comprueba que `JAVA_HOME` apunte a JDK 17 y que
  exista `platforms/android-35/android.jar`. Los scripts lo validan.
- `adb devices` debe mostrar el dispositivo como `device` (no `unauthorized`).
- El APK contiene binarios ARM. En un emulador `x86_64` se ejecuta por
  traduccion: puede tardar mucho o provocar un ANR. Para pruebas en emulador,
  usa una imagen `arm64-v8a` o agrega `android-x64` a `RuntimeIdentifiers` solo
  para esa prueba local.
- Si la app instala pero no abre, captura el log antes de cerrarla:

  ```powershell
  adb logcat -c
  adb shell am start -n com.integritysolution.snapclone/crc6494337cccf7d1a104.MainActivity
  adb logcat -d > snapclone-log.txt
  ```

  Busca `AndroidRuntime`, `mono`, `AndroidGameView` o `MakeCurrent`.
- Para una compilacion limpia: `dotnet clean` y volver a compilar. `bin/` y
  `obj/` son generados y estan ignorados por git.

## Notas de repositorio

- `bin/`, `obj/`, `.vs/`, APK/AAB, keystores y logs locales no se versionan.
- Las fuentes incluyen sus licencias OFL en `SnapClone/Content/Fonts/`.
- `requirements.txt` es el brief original del prototipo, no una lista de
  dependencias de Python.
