using System.Text.Json;

namespace Mastermind.Core;

public sealed record EvaluationResult(int BienPosicionados, int ColorCorrecto,
    string MensajeCheat = "No utilizado", int? PosicionRevelada = null, char? ColorRevelado = null);

public sealed class GameConfiguration
{
    public string ColoresValidos { get; init; } = "YCBPWRNG";
    public int Tamano { get; init; } = 4;
    public void Validar()
    {
        if (Tamano is not (4 or 6)) throw new ArgumentException("El tamaño debe ser 4 o 6.");
        if (string.IsNullOrWhiteSpace(ColoresValidos) || ColoresValidos.Distinct().Count() != ColoresValidos.Length)
            throw new ArgumentException("Los colores configurados deben ser únicos.");
        if (ColoresValidos.Any(c => !char.IsAsciiLetterUpper(c)))
            throw new ArgumentException("Los colores deben ser letras mayúsculas A-Z.");
    }
}

public sealed class CheatEntry
{
    public string Codigo { get; set; } = "";
    public int IntentosDisponibles { get; set; }
}

public sealed class MastermindEvaluator
{
    private readonly GameConfiguration _config;
    private readonly Dictionary<string, CheatEntry> _cheats;
    private readonly HashSet<string> _usadosEnSesion = new(StringComparer.OrdinalIgnoreCase);
    private readonly Random _random;
    private readonly Action<string> _registrar;
    private bool _cheatConsumidoEnPartida;

    public MastermindEvaluator(GameConfiguration? config = null, IEnumerable<CheatEntry>? cheats = null,
        Random? random = null, Action<string>? registrar = null)
    {
        _config = config ?? new GameConfiguration();
        _config.Validar();
        _cheats = (cheats ?? Array.Empty<CheatEntry>()).ToDictionary(c => c.Codigo, StringComparer.OrdinalIgnoreCase);
        _random = random ?? Random.Shared;
        _registrar = registrar ?? Console.WriteLine;
    }

    public static List<CheatEntry> CargarCheats(string ruta)
    {
        var json = File.ReadAllText(ruta);
        return JsonSerializer.Deserialize<List<CheatEntry>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new JsonException("Archivo JSON vacío o inválido.");
    }

    // La firma de dos parámetros sigue funcionando: retrocompatibilidad.
    public EvaluationResult EvaluarIntento(string patronSecreto, string intentoJugador, string? cheatCode = null)
    {
        ValidarPatron(patronSecreto, nameof(patronSecreto));
        ValidarPatron(intentoJugador, nameof(intentoJugador));

        int exactas = 0;
        var restantesSecreto = new Dictionary<char, int>();
        var restantesIntento = new Dictionary<char, int>();

        for (int i = 0; i < _config.Tamano; i++)
        {
            if (patronSecreto[i] == intentoJugador[i]) exactas++;
            else
            {
                Agregar(restantesSecreto, patronSecreto[i]);
                Agregar(restantesIntento, intentoJugador[i]);
            }
        }

        int colorCorrecto = restantesIntento.Sum(par =>
            Math.Min(par.Value, restantesSecreto.GetValueOrDefault(par.Key)));

        string estado = "No utilizado";
        int? posicion = null;
        char? color = null;
        if (cheatCode is not null)
        {
            if (string.IsNullOrWhiteSpace(cheatCode)) throw new ArgumentException("El cheat code no puede estar vacío.");
            if (_cheatConsumidoEnPartida) throw new InvalidOperationException("El cheat code ya se consumió en esta partida.");
            if (!_cheats.TryGetValue(cheatCode, out var entrada)) throw new ArgumentException("Cheat code no válido.");
            if (_usadosEnSesion.Contains(cheatCode)) throw new InvalidOperationException("Cheat code ya usado en la sesión.");
            if (entrada.IntentosDisponibles <= 0) throw new InvalidOperationException("Cheat code sin intentos disponibles.");

            // Consumo antes de conocer si la posición aporta una pista útil.
            entrada.IntentosDisponibles--;
            _cheatConsumidoEnPartida = true;
            _usadosEnSesion.Add(cheatCode);
            posicion = _random.Next(_config.Tamano);
            if (patronSecreto[posicion.Value] == intentoJugador[posicion.Value]) estado = "Sin suerte";
            else
            {
                color = patronSecreto[posicion.Value];
                estado = "Pista revelada";
            }
        }

        _registrar($"En la fecha {DateTime.Now:dd-MM-yyyy HH:mm:ss} se evaluó el intento {intentoJugador} contra el patrón {patronSecreto}. Resultado: {exactas} bien posicionados – {colorCorrecto} con color correcto.");
        return new EvaluationResult(exactas, colorCorrecto, estado, posicion, color);
    }

    private static void Agregar(Dictionary<char, int> d, char c) => d[c] = d.GetValueOrDefault(c) + 1;

    private void ValidarPatron(string valor, string nombre)
    {
        if (valor is null) throw new ArgumentNullException(nombre);
        if (valor.Length != _config.Tamano)
            throw new ArgumentException($"{nombre} debe tener exactamente {_config.Tamano} caracteres.", nombre);
        if (valor.Any(c => !_config.ColoresValidos.Contains(c)))
            throw new ArgumentException($"{nombre} contiene colores no permitidos. Disponibles: {_config.ColoresValidos}.", nombre);
    }
}
