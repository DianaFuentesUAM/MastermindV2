
using Mastermind.Core;
using Xunit;

namespace Mastermind.Tests;

public sealed class MastermindTests
{
    private static MastermindEvaluator Crear(
        int tamano = 4,
        string colores = "YCBPWRNG",
        IEnumerable<CheatEntry>? cheats = null,
        Random? random = null,
        Action<string>? log = null)
        => new(
            new GameConfiguration
            {
                Tamano = tamano,
                ColoresValidos = colores
            },
            cheats,
            random,
            log ?? (_ => { })
        );

    // PRUEBA 1: Coincidencia completa
    [Fact]
    public void Completo()
    {
        var r = Crear().EvaluarIntento("RGBY", "RGBY");

        Assert.Equal(4, r.BienPosicionados);
        Assert.Equal(0, r.ColorCorrecto);
    }

    // PRUEBA 2: Coincidencia parcial
    [Fact]
    public void Parcial_EjemploDelProfesor()
    {
        var r = Crear().EvaluarIntento("RGBY", "RBYG");

        Assert.Equal(1, r.BienPosicionados);
        Assert.Equal(3, r.ColorCorrecto);
    }

    // PRUEBA 3: Ninguna coincidencia
    [Fact]
    public void SinCoincidencias()
    {
        var r = Crear().EvaluarIntento("RGBY", "CCCC");

        Assert.Equal(0, r.BienPosicionados);
        Assert.Equal(0, r.ColorCorrecto);
    }

    // PRUEBA 4: Manejo de duplicados
    [Fact]
    public void Duplicados_NoSeCuentanDosVeces()
    {
        var r = Crear().EvaluarIntento("RRBG", "BRRR");

        Assert.Equal(1, r.BienPosicionados);
        Assert.Equal(2, r.ColorCorrecto);
    }

    // PRUEBA 5: Tamaño de seis posiciones
    [Fact]
    public void TamanoSeis()
    {
        var r = Crear(6).EvaluarIntento(
            "RGBYCP", "RGBYPC");

        Assert.Equal(4, r.BienPosicionados);
        Assert.Equal(2, r.ColorCorrecto);
    }

    // PRUEBA 6: Patrón con tamaño incorrecto
    [Fact]
    public void PatronConTamanoIncorrecto()
    {
        Assert.Throws<ArgumentException>(() =>
            Crear().EvaluarIntento("RGB", "RGBY"));
    }

    // PRUEBA 7: Color no permitido
    [Fact]
    public void IntentoConColorInvalido()
    {
        Assert.Throws<ArgumentException>(() =>
            Crear().EvaluarIntento("RGBY", "RGBZ"));
    }

    // PRUEBA 8: Tamaño de configuración inválido
    [Fact]
    public void ConfiguracionTamanoInvalido()
    {
        Assert.Throws<ArgumentException>(() => Crear(5));
    }

    // PRUEBA 9: Colores repetidos en configuración
    [Fact]
    public void ConfiguracionColoresRepetidos()
    {
        Assert.Throws<ArgumentException>(() =>
            Crear(colores: "RRBG"));
    }

    // PRUEBA 10: Patrón nulo
    [Fact]
    public void NullNoValido()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Crear().EvaluarIntento(null!, "RGBY"));
    }

    // PRUEBA 11: Retrocompatibilidad
    [Fact]
    public void RetrocompatibleDosArgumentos()
    {
        var resultado = Crear().EvaluarIntento(
            "RGBY", "RGBY");

        Assert.Equal(4, resultado.BienPosicionados);
    }

    // PRUEBA 12: Cheat code válido
    [Fact]
    public void CheatValidoConsumeIntento()
    {
        var cheat = new CheatEntry
        {
            Codigo = "PISTA",
            IntentosDisponibles = 1
        };

        var r = Crear(
            cheats: new[] { cheat },
            random: new Random(7)
        ).EvaluarIntento("RGBY", "CCCC", "PISTA");

        Assert.Equal("Pista revelada", r.MensajeCheat);
        Assert.Equal(0, cheat.IntentosDisponibles);
        Assert.NotNull(r.ColorRevelado);
    }

    // PRUEBA 13: Cheat repetido
    [Fact]
    public void CheatRepetidoEnPartidaFalla()
    {
        var j = Crear(cheats: new[]
        {
            new CheatEntry
            {
                Codigo = "PISTA",
                IntentosDisponibles = 3
            }
        });

        j.EvaluarIntento("RGBY", "CCCC", "PISTA");

        Assert.Throws<InvalidOperationException>(() =>
            j.EvaluarIntento("RGBY", "CCCC", "PISTA"));
    }

    // PRUEBA 14: Segundo cheat diferente
    [Fact]
    public void SegundoCheatDistintoTambienFalla()
    {
        var j = Crear(cheats: new[]
        {
            new CheatEntry
            {
                Codigo = "A",
                IntentosDisponibles = 1
            },
            new CheatEntry
            {
                Codigo = "B",
                IntentosDisponibles = 1
            }
        });

        j.EvaluarIntento("RGBY", "CCCC", "A");

        Assert.Throws<InvalidOperationException>(() =>
            j.EvaluarIntento("RGBY", "CCCC", "B"));
    }

    // PRUEBA 15: Cheat inútil
    [Fact]
    public void CheatInutilIgualmenteSeConsume()
    {
        var cheat = new CheatEntry
        {
            Codigo = "PISTA",
            IntentosDisponibles = 1
        };

        var r = Crear(
            cheats: new[] { cheat }
        ).EvaluarIntento("RGBY", "RGBY", "PISTA");

        Assert.Equal("Sin suerte", r.MensajeCheat);
        Assert.Equal(0, cheat.IntentosDisponibles);
        Assert.Null(r.ColorRevelado);
    }

    // PRUEBA 16: Cheat desconocido
    [Fact]
    public void CheatDesconocido()
    {
        Assert.Throws<ArgumentException>(() =>
            Crear().EvaluarIntento(
                "RGBY", "CCCC", "INVENTADO"));
    }

    // PRUEBA 17: Cheat agotado
    [Fact]
    public void CheatAgotado()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Crear(cheats: new[]
            {
                new CheatEntry
                {
                    Codigo = "A",
                    IntentosDisponibles = 0
                }
            }).EvaluarIntento("RGBY", "CCCC", "A"));
    }

    // PRUEBA 18: Registro con fecha y hora
    [Fact]
    public void RegistroIncluyeResultadoYPatrones()
    {
        string registro = "";

        Crear(log: s => registro = s)
            .EvaluarIntento("RGBY", "RBYG");

        Assert.Contains("intento RBYG", registro);
        Assert.Contains("patrón RGBY", registro);
        Assert.Contains("1 bien posicionados", registro);
        Assert.Contains("3 con color correcto", registro);

        Assert.Matches(
            @"\d{2}-\d{2}-\d{4} \d{2}:\d{2}:\d{2}",
            registro);
    }

    // PRUEBA 19: Cheat code vacío
    [Fact]
    public void CheatVacioDebeSerRechazado()
    {
        var evaluador = Crear();

        Assert.Throws<ArgumentException>(() =>
            evaluador.EvaluarIntento("RGBY", "RBYG", ""));
    }

    // PRUEBA 20: Letras minúsculas
    [Fact]
    public void IntentoEnMinusculasDebeSerRechazado()
    {
        var evaluador = Crear();

        Assert.Throws<ArgumentException>(() =>
            evaluador.EvaluarIntento("RGBY", "rgby"));
    }
}


