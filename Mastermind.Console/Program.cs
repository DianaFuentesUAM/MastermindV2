using Mastermind.Core;

Console.WriteLine("MASTERMIND V2 — Configuración de partida");
Console.Write("Tamaño (4 o 6): ");
if (!int.TryParse(Console.ReadLine(), out int tamano)) { Console.WriteLine("Tamaño inválido."); return; }
Console.Write("Colores disponibles en mayúscula, sin repetir (Enter = YCBPWRNG): ");
string colores = Console.ReadLine() is { Length: > 0 } entrada ? entrada : "YCBPWRNG";
try
{
    var conf = new GameConfiguration { Tamano = tamano, ColoresValidos = colores };
    var cheats = MastermindEvaluator.CargarCheats(Path.Combine(AppContext.BaseDirectory, "cheatcodes.json"));
    Directory.CreateDirectory("logs");
    var juego = new MastermindEvaluator(conf, cheats, registrar: s =>
    {
        Console.WriteLine(s);
        File.AppendAllText(Path.Combine("logs", "mastermind.log"), s + Environment.NewLine);
    });
    Console.Write($"Patrón secreto ({tamano} caracteres): ");
    string patron = Console.ReadLine() ?? "";
    while (true)
    {
        Console.Write($"Intento del jugador ({tamano} caracteres; Enter para salir): ");
        string intento = Console.ReadLine() ?? "";
        if (intento.Length == 0) break;
        Console.Write("Cheat code opcional (Enter = ninguno): ");
        string? cheat = Console.ReadLine();
        if (cheat == "") cheat = null;
        try
        {
            var r = juego.EvaluarIntento(patron, intento, cheat);
            Console.WriteLine($"Exactas: {r.BienPosicionados}; color correcto en otra posición: {r.ColorCorrecto}; cheat: {r.MensajeCheat}");
            if (r.ColorRevelado.HasValue)
                Console.WriteLine($"Pista: posición {r.PosicionRevelada!.Value + 1} = {r.ColorRevelado}");
            if (r.BienPosicionados == tamano) { Console.WriteLine("¡Ganaste!"); break; }
        }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException)
        { Console.WriteLine("Error: " + e.Message); }
    }
}
catch (Exception e) when (e is ArgumentException or IOException or System.Text.Json.JsonException)
{ Console.WriteLine("Configuración inválida: " + e.Message); }
