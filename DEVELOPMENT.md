# Desarrollo local (Windows / C#)

Requisitos: Git y SDK .NET 8, con soporte de escritorio Windows. El runtime por sí solo no permite compilar.

En esta copia se instaló un SDK local en `.tools/dotnet`; no se incluye en Git y no cambia el PATH del sistema.

## Ejecutar

Desde PowerShell, en la raíz del repositorio:

```powershell
.\scripts\dev.ps1
```

El script compila para x64 y ejecuta desde la carpeta de salida para que los iconos y ADB se encuentren correctamente. Utiliza el SDK local si existe, o `dotnet` del PATH. La configuración original Any CPU sigue generando la aplicación x86.

## Compilar y probar

Con un SDK disponible en PATH:

```powershell
dotnet build AdbFileManager.sln --configuration Release
dotnet build AdbFileManager/AdbFileManager.csproj --configuration Release -p:Platform=x64
dotnet test tests/AdbFileManager.Tests/AdbFileManager.Tests.csproj --configuration Release
```

Con el SDK local, sustituir `dotnet` por `.\.tools\dotnet\dotnet.exe` y establecer:

```powershell
$env:DOTNET_HOST_PATH = (Resolve-Path .\.tools\dotnet\dotnet.exe).Path
```

Las pruebas ejecutan un simulador como proceso independiente. Cubren argumentos literales, selección de dispositivo en ambos sentidos, opciones de copia, rutas Android, navegación, salida abundante en ambos canales, códigos de error, progreso fragmentado, cancelación, conflictos, persistencia y reintentos. No requieren ni modifican un teléfono. El flujo de GitHub Actions compila ambas arquitecturas y ejecuta las pruebas en Windows.

## Primera iteración

- Se eliminan la inyección de `HookDll.dll`, el servidor de pipes y el estado global del progreso.
- La copia predeterminada ejecuta ADB directamente, lee stdout/stderr en paralelo y comprueba el código de salida.
- Los argumentos se pasan separados; `-a` se usa únicamente con `pull` y se elimina `-p` de las transferencias.
- El dispositivo seleccionado se captura para todo el lote, tanto en `push` como en `pull`.
- Cerrar la ventana de progreso cancela la copia activa. Cerrar la aplicación espera su cancelación y deja intacto el servidor ADB compartido.
- Al fallar una copia se muestra el error, se detiene el lote y se liberan los controles para reintentar.
- Enter usa los metadatos del listado para distinguir archivos y carpetas. Subir desde `/` no lanza una excepción y navegar no repite la consulta por el cambio de texto de la ruta.
- Los binarios ADB incluidos en el repositorio se copian a la salida de compilación.

## Segunda iteración

- `AdbClient` concentra la ruta del ejecutable, los argumentos, la selección de dispositivo, la captura de salida y la validación de errores. La ventana principal muestra la versión incluida de ADB.
- Las copias se agregan a una cola visible y persistente. Se puede pausar, cancelar el elemento actual, reintentar fallidos o cancelados, limpiar completados y exportar un resumen JSON.
- Cada trabajo conserva el dispositivo seleccionado al momento de encolarlo. La cola se restaura pausada después de cerrar o reiniciar la aplicación.
- Los errores transitorios de conexión se reintentan automáticamente hasta dos veces y un fallo no impide procesar los siguientes trabajos.
- Si el destino existe se puede omitir o conservar ambos; un archivo también se puede reemplazar. La decisión puede aplicarse al lote actual.
- Las transferencias usan nombres temporales `.afm-*` y sólo mueven el resultado al destino después de completar ADB. Esto evita dejar el destino final parcialmente escrito si la copia falla o se cancela.
- El reemplazo de carpetas y los conflictos entre archivo y carpeta se bloquean para no borrar ni mezclar contenido sin una operación explícita.

## Límites y validación con un teléfono

ADB puede omitir porcentajes al redirigir su salida; en ese caso se muestra actividad indeterminada para el archivo actual. El total cuenta elementos seleccionados, no bytes. Los archivos temporales se eliminan al detectar un error o cancelación; una terminación forzada del proceso puede dejar un nombre `.afm-*`, pero no sustituye el destino existente.

No se ha medido aún una mejora de velocidad de transferencia real ni se ha validado la cola con un teléfono físico. Algunos comandos de navegación heredados siguen usando adaptadores síncronos sobre el cliente centralizado.

Antes de usarlo para un respaldo importante, comprobar con archivos de prueba:

1. Copiar en ambos sentidos un archivo grande y una carpeta con archivos pequeños; comparar contenido y fechas.
2. Con dos dispositivos conectados, comprobar que ambos sentidos utilizan el seleccionado.
3. Cancelar, desconectar durante una copia y volver a intentar; comprobar el error, los reintentos y que el destino final no quede parcial.
4. Navegar con Enter y doble clic por `Fotos.2026`, archivos sin extensión y la raíz `/`.
5. Provocar conflictos de archivo y carpeta en ambos sentidos; validar Omitir, Conservar ambos y Reemplazar archivo.
6. Cerrar la aplicación con trabajos pendientes, volver a abrirla y confirmar que la cola se restaura pausada.
7. Comparar el tiempo de la aplicación con `adb pull`/`adb push` directos, usando idéntico dispositivo, archivos y conexión.

La prueba física automatizada y su procedimiento de evidencia están descritos en `DEVICE_TESTING.md`. Se ejecutan con `scripts/device-smoke.ps1` y permanecen omitidos en la suite normal cuando no se define `AFM_DEVICE_SERIAL`.
