using Mastermind.Core;
using Xunit;

namespace Mastermind.Tests;

public sealed class MastermindTests
{
    private static MastermindEvaluator Crear(int tamano = 4, string colores = "YCBPWRNG",
        IEnumerable<CheatEntry>? cheats = null, Random? random = null, Action<string>? log = null)
        => new(new GameConfiguration { Tamano = tamano, ColoresValidos = colores }, cheats, random, log ?? (_ => { }));

    [Fact] public void Completo() { var r = Crear().EvaluarIntento("RGBY", "RGBY"); Assert.Equal(4, r.BienPosicionados); Assert.Equal(0, r.ColorCorrecto); }
    [Fact] public void Parcial_EjemploDelProfesor() { var r = Crear().EvaluarIntento("RGBY", "RBYG"); Assert.Equal(1, r.BienPosicionados); Assert.Equal(3, r.ColorCorrecto); }
    [Fact] public void SinCoincidencias() { var r = Crear().EvaluarIntento("RGBY", "CCCC"); Assert.Equal(0, r.BienPosicionados); Assert.Equal(0, r.ColorCorrecto); }
    [Fact] public void Duplicados_NoSeCuentanDosVeces() { var r = Crear().EvaluarIntento("RRBG", "BRRR"); Assert.Equal(1, r.BienPosicionados); Assert.Equal(2, r.ColorCorrecto); }
    [Fact] public void TamanoSeis() { var r = Crear(6).EvaluarIntento("RGBYCP", "RGBYPC"); Assert.Equal(4, r.BienPosicionados); Assert.Equal(2, r.ColorCorrecto); }
    [Fact] public void PatronConTamanoIncorrecto() => Assert.Throws<ArgumentException>(() => Crear().EvaluarIntento("RGB", "RGBY"));
    [Fact] public void IntentoConColorInvalido() => Assert.Throws<ArgumentException>(() => Crear().EvaluarIntento("RGBY", "RGBZ"));
    [Fact] public void ConfiguracionTamanoInvalido() => Assert.Throws<ArgumentException>(() => Crear(5));
    [Fact] public void ConfiguracionColoresRepetidos() => Assert.Throws<ArgumentException>(() => Crear(colores: "RRBG"));
    [Fact] public void NullNoValido() => Assert.Throws<ArgumentNullException>(() => Crear().EvaluarIntento(null!, "RGBY"));
    [Fact] public void RetrocompatibleDosArgumentos() => Assert.Equal(4, Crear().EvaluarIntento("RGBY", "RGBY").BienPosicionados);
    [Fact] public void CheatValidoConsumeIntento()
    {
        var cheat = new CheatEntry { Codigo = "PISTA", IntentosDisponibles = 1 };
        var r = Crear(cheats: new[] { cheat }, random: new Random(7)).EvaluarIntento("RGBY", "CCCC", "PISTA");
        Assert.Equal("Pista revelada", r.MensajeCheat);
        Assert.Equal(0, cheat.IntentosDisponibles);
        Assert.NotNull(r.ColorRevelado);
    }
    [Fact] public void CheatRepetidoEnPartidaFalla()
    {
        var j = Crear(cheats: new[] { new CheatEntry { Codigo = "PISTA", IntentosDisponibles = 3 } });
        j.EvaluarIntento("RGBY", "CCCC", "PISTA");
        Assert.Throws<InvalidOperationException>(() => j.EvaluarIntento("RGBY", "CCCC", "PISTA"));
    }
    [Fact] public void SegundoCheatDistintoTambienFalla()
    {
        var j = Crear(cheats: new[] { new CheatEntry { Codigo = "A", IntentosDisponibles = 1 }, new CheatEntry { Codigo = "B", IntentosDisponibles = 1 } });
        j.EvaluarIntento("RGBY", "CCCC", "A");
        Assert.Throws<InvalidOperationException>(() => j.EvaluarIntento("RGBY", "CCCC", "B"));
    }
    [Fact] public void CheatInutilIgualmenteSeConsume()
    {
        var cheat = new CheatEntry { Codigo = "PISTA", IntentosDisponibles = 1 };
        var r = Crear(cheats: new[] { cheat }).EvaluarIntento("RGBY", "RGBY", "PISTA");
        Assert.Equal("Sin suerte", r.MensajeCheat);
        Assert.Equal(0, cheat.IntentosDisponibles);
        Assert.Null(r.ColorRevelado);
    }
    [Fact] public void CheatDesconocido() => Assert.Throws<ArgumentException>(() => Crear().EvaluarIntento("RGBY", "CCCC", "INVENTADO"));
    [Fact] public void CheatAgotado() => Assert.Throws<InvalidOperationException>(() => Crear(cheats: new[] { new CheatEntry { Codigo = "A", IntentosDisponibles = 0 } }).EvaluarIntento("RGBY", "CCCC", "A"));
    [Fact] public void RegistroIncluyeResultadoYPatrones()
    {
        string registro = "";
        Crear(log: s => registro = s).EvaluarIntento("RGBY", "RBYG");
        Assert.Contains("intento RBYG", registro);
        Assert.Contains("patrón RGBY", registro);
        Assert.Contains("1 bien posicionados", registro);
        Assert.Contains("3 con color correcto", registro);
        Assert.Matches(@"\d{2}-\d{2}-\d{4} \d{2}:\d{2}:\d{2}", registro);
    }
}
