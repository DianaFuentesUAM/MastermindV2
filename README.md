# Mastermind V2 — Programación Avanzada

Proyecto en C# (.NET 8), biblioteca de lógica, aplicación de consola y pruebas xUnit.

## Preparación

Instalar **.NET SDK 8** y usar la terminal de VS Code en la carpeta `MastermindV2`.

```bash
dotnet restore Mastermind.Tests/Mastermind.Tests.csproj
dotnet test Mastermind.Tests/Mastermind.Tests.csproj
dotnet run --project Mastermind.Console/Mastermind.Console.csproj
```

## Colores predeterminados

Y = Yellow, C = Cyan, B = Black, P = Purple, W = White, R = Red, N = Brown, G = Green.
La configuración de colores se puede modificar al iniciar. Por defecto se permiten 4 posiciones; también se permite 6.

## Reglas

- Se evalúan primero coincidencias en posición exacta; luego las coincidencias de color en distinta posición, sin repetir.
- La sobrecarga opcional `EvaluarIntento(secreto, intento, cheatCode = null)` preserva llamadas anteriores de dos parámetros.
- Cheat code: se valida contra el archivo `Mastermind.Console/cheatcodes.json`, verifica saldo y que no se haya utilizado en la sesión; se usa **como máximo uno por partida**. Si la posición aleatoria estaba correcta, muestra `Sin suerte`, pero consume el uso igualmente.
- El log se imprime en pantalla y queda en `logs/mastermind.log` cuando se ejecuta la aplicación.
- Los intentos disponibles se reducen en memoria. **No se persistirán en el JSON tras cerrar la aplicación**. Eso puede implementarse como mejora opcional según la política del profesor.
- El "patrón secreto" se ingresa en la consola y se imprime en el log porque así lo pide la tarea; no es una interfaz segura para juego competitivo.

## Git: secuencia exigida de commits

No se incluyen commits reales en el ZIP porque deben hacerse desde tu repositorio al realizar cada fase. Conserva estos pasos en tu propio GitHub:

1. Algoritmo básico: iniciar con evaluación exactas y colores.
2. Pruebas unitarias iniciales: agregar pruebas de coincidencias y duplicados.
3. Requisitos avanzados: validar, leer JSON, agregar cheat code, logging.
4. Ampliar pruebas: cheat válido/repetido/inútil, errores, tamaño 6 y logging.
5. `git push -u origin main` (después de configurar un remoto propio).

**Importante:** Si importas este ZIP ya terminado y haces cuatro commits sin cambios reales, eso no demuestra el desarrollo incremental. Para evidenciar el flujo, introduce el trabajo por etapas y captura commits reales.

## Alcance de verificación

Se incluyen pruebas xUnit para ejecutar localmente. El proyecto no se ha compilado ni probado en este entorno, donde no está disponible `dotnet`. Ejecuta `dotnet test` en tu equipo y utiliza su resultado real como evidencia.
