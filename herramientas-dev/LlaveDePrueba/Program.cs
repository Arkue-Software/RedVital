// RedVital — Utilidad de desarrollo
// T-303.1 — Generar par de llaves de prueba para tokens U3, U4 y de servicio.
//
// ADVERTENCIA, léela antes de usar esto:
// Esta llave es EXCLUSIVA de Desarrollo y de la suite de pruebas automatizadas.
// No es secreta de verdad: vive en el repositorio y cualquiera puede leerla.
// Si esta llave llegara a aceptarse en QA, cualquiera podría fabricar un token
// válido con ella. La configuración de QA debe rechazarla explícitamente
// (eso es T-303.3, no esta tarea).
//
// Uso:
//   dotnet run -- generar-llave
//   dotnet run -- emitir-token --perfil operador
//   dotnet run -- emitir-token --perfil admin_banco
//   dotnet run -- emitir-token --perfil servicio --cliente gateway

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace RedVital.Herramientas.LlaveDePrueba;

public static class Program
{
    // Rutas relativas al propio proyecto. En el equipo, el guion de
    // inicialización de redvital-infra las copia al perfil de cada servicio
    // (ver T-303.2 y la guía de entorno).
    private const string RutaLlavePrivada = "llaves/dev-signing-key.private.pem";
    private const string RutaLlavePublica = "llaves/dev-signing-key.public.pem";

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            MostrarAyuda();
            return 1;
        }

        switch (args[0])
        {
            case "generar-llave":
                GenerarLlave();
                return 0;

            case "emitir-token":
                return EmitirToken(args);

            default:
                MostrarAyuda();
                return 1;
        }
    }

    // ------------------------------------------------------------
    // Generación del par de llaves (lo que pide T-303.1 en sentido estricto)
    // ------------------------------------------------------------
    private static void GenerarLlave()
    {
        Directory.CreateDirectory("llaves");

        // RSA 3072 bits: el mismo tamaño de clave que fija la entrada 29 del
        // Tech Radar para la llave real de producción. Usar el mismo tamaño
        // aquí evita que el código de validación tenga que distinguir "llave
        // de prueba" de "llave real" por su longitud — la única diferencia
        // debe ser dónde se confía en cada ambiente, no la forma de la llave.
        using var rsa = RSA.Create(3072);

        var llavePrivadaPem = rsa.ExportPkcs8PrivateKeyPem();
        var llavePublicaPem = rsa.ExportSubjectPublicKeyInfoPem();

        File.WriteAllText(RutaLlavePrivada, llavePrivadaPem);
        File.WriteAllText(RutaLlavePublica, llavePublicaPem);

        Console.WriteLine($"Llave privada de prueba escrita en: {RutaLlavePrivada}");
        Console.WriteLine($"Llave pública de prueba escrita en: {RutaLlavePublica}");
        Console.WriteLine();
        Console.WriteLine("Recuerda: esta llave es de Desarrollo y de pruebas exclusivamente.");
        Console.WriteLine("No la copies al perfil de QA. Eso es precisamente lo que T-303.3 verifica que NO pase.");
    }

    // ------------------------------------------------------------
    // Emisión de tokens de prueba con las ocho reivindicaciones de ADR-008
    // ------------------------------------------------------------
    private static int EmitirToken(string[] args)
    {
        if (!File.Exists(RutaLlavePrivada))
        {
            Console.Error.WriteLine("No existe la llave de prueba. Corre primero: dotnet run -- generar-llave");
            return 1;
        }

        var perfil = ObtenerArgumento(args, "--perfil");
        if (perfil is null)
        {
            Console.Error.WriteLine("Falta --perfil (operador | admin_banco | servicio)");
            return 1;
        }

        using var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(RutaLlavePrivada));
        var credencialFirma = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

        var ahora = DateTimeOffset.UtcNow;
        var claims = new List<Claim>
        {
            // Las ocho reivindicaciones exactas de ADR-008. No agregues ninguna
            // de más, en particular ningún dato personal (nombre, correo,
            // documento) — el token no las transporta, ni siquiera en pruebas.
            new("iss", "https://identidad.redvital.local"),
            new("aud", "redvital"),
            new("iat", ahora.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("exp", ahora.AddMinutes(15).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("jti", Guid.NewGuid().ToString()),
        };

        switch (perfil)
        {
            case "operador": // U3 — Operador de banco
                claims.Add(new Claim("sub", "11111111-1111-1111-1111-111111111111"));
                claims.Add(new Claim("role", "operador"));
                claims.Add(new Claim("jurisdiction", "institucion:22222222-2222-2222-2222-222222222222"));
                break;

            case "admin_banco": // U4 — Administrador de banco
                claims.Add(new Claim("sub", "33333333-3333-3333-3333-333333333333"));
                claims.Add(new Claim("role", "admin_banco"));
                claims.Add(new Claim("jurisdiction", "institucion:22222222-2222-2222-2222-222222222222"));
                break;

            case "servicio": // Token de servicio, para procesos sin usuario
                var cliente = ObtenerArgumento(args, "--cliente") ?? "gateway";
                if (cliente is not ("gateway" or "donacion" or "notificaciones"))
                {
                    Console.Error.WriteLine("--cliente debe ser gateway, donacion o notificaciones");
                    return 1;
                }
                claims.Add(new Claim("sub", cliente));
                claims.Add(new Claim("role", "servicio"));
                claims.Add(new Claim("jurisdiction", ""));
                break;

            default:
                Console.Error.WriteLine("Perfil no reconocido. Usa: operador | admin_banco | servicio");
                return 1;
        }

        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: credencialFirma);

        var tokenEscrito = new JwtSecurityTokenHandler().WriteToken(token);

        Console.WriteLine(tokenEscrito);
        return 0;
    }

    private static string? ObtenerArgumento(string[] args, string nombre)
    {
        var indice = Array.IndexOf(args, nombre);
        return indice >= 0 && indice + 1 < args.Length ? args[indice + 1] : null;
    }

    private static void MostrarAyuda()
    {
        Console.WriteLine("Uso:");
        Console.WriteLine("  dotnet run -- generar-llave");
        Console.WriteLine("  dotnet run -- emitir-token --perfil operador");
        Console.WriteLine("  dotnet run -- emitir-token --perfil admin_banco");
        Console.WriteLine("  dotnet run -- emitir-token --perfil servicio --cliente gateway");
    }
}
