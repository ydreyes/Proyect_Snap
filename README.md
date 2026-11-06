# SnapClone

Juego de cartas por turnos en MonoGame (DesktopGL y Android).

## Requisitos

- **.NET SDK 9.0.3xx**. El repo fija la banda con `global.json` (`rollForward: latestFeature`), asi que `dotnet --version` dentro del repo debe reportar `9.0.3xx`.
- **Workload de Android** para .NET 9: `dotnet workload install android`.
- **JDK 17** (Microsoft OpenJDK 17). .NET para Android 35 no acepta JDK 21+.
- **Android SDK** con la plataforma `android-35` y `platform-tools`:
  ```
  sdkmanager "platforms;android-35" "platform-tools"
  ```
- Para instalar en el telefono: depuracion USB activada y el dispositivo autorizado.

Instalacion del JDK 17 en Windows (sin privilegios de administrador):

```powershell
curl.exe -L -o "$env:TEMP\microsoft-jdk-17.zip" "https://aka.ms/download-jdk/microsoft-jdk-17-windows-x64.zip"
Expand-Archive "$env:TEMP\microsoft-jdk-17.zip" "$env:LOCALAPPDATA\Microsoft\jdk-17" -Force
```

## Escritorio (PC)

```powershell
dotnet build SnapClone.sln
dotnet run --project SnapClone\SnapClone.csproj
```

La compilacion de escritorio **no** requiere el workload de Android, el Android SDK ni el JDK.

## Android

Compilar el APK debug (firma con la debug key, ABI `arm64-v8a`):

```powershell
scripts\build-android.ps1
```

El script prepara `JAVA_HOME`/`ANDROID_HOME` para la sesion y termina imprimiendo la ruta absoluta del APK, por ejemplo:

```
SnapClone.Android\bin\Debug\net9.0-android\com.integritysolution.snapclone-Signed.apk
```

Instalar y lanzar en un dispositivo conectado:

```powershell
scripts\install-android.ps1
```

Tambien se puede instalar manualmente:

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
| ABI | `arm64-v8a` |
| Orientacion | portrait bloqueado, pantalla completa inmersiva |

### Estructura

- `SnapClone/`: proyecto de escritorio (`net9.0`, MonoGame DesktopGL) con todo el juego y el contenido compartido.
- `SnapClone.Android/`: head Android (`net9.0-android`, MonoGame Android). Enlaza los `.cs` de `SnapClone/` (salvo `Program.cs`) y compila el mismo `Content/Content.mgcb` con `/platform:Android`.
