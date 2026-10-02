// RedVital — Servicio de Identidad
// T-330.4 — Serie de auditoría de identidad y operación interna para registrar
// las denegaciones del gateway.
//
// Estructura común de las cuatro series de auditoría del sistema (DD, Sección 5.3.6).
// Esta serie registra, además de lo que registran las otras: inicio de sesión
// exitoso y fallido, cierre, revocación y reutilización de secreto de renovación,
// alta de cuenta, cambio de rol, desactivación, asignación y retiro de
// jurisdicción (RF-22), rotación de la clave de firma, y las denegaciones F2
// que entrega el gateway (ADR-016).

namespace RedVital.Identidad.Dominio;

/// <summary>
/// Solo anexado: el rol de servicio de esta base tiene permiso únicamente de
/// inserción y lectura sobre esta tabla (ver scripts/roles_identidad.sql).
/// Ningún código de este servicio debe intentar actualizar ni borrar una fila —
/// no existe ningún método de repositorio para eso, a propósito.
/// </summary>
public class RegistroAuditoriaIdent
{
    public Guid Id { get; set; }

    /// <summary>"usuario", "sistema" o "anonimo".</summary>
    public required string ActorTipo { get; set; }

    /// <summary>
    /// Identificador del usuario si ActorTipo es "usuario"; nombre del proceso si
    /// es "sistema". Nulo para el actor "anonimo" — por ejemplo, una denegación del
    /// gateway por token ausente o inválido, donde no hay nadie identificable.
    /// </summary>
    public string? ActorId { get; set; }

    /// <summary>Rol vigente EN EL MOMENTO DEL HECHO, no el rol actual del usuario.</summary>
    public string? Rol { get; set; }

    /// <summary>Ruta territorial o institución a la que se intentó acceder.</summary>
    public string? JurisdiccionSolicitada { get; set; }

    /// <summary>Acción del catálogo de acciones sensibles de esta serie (DD, Tabla 16).</summary>
    public required string Operacion { get; set; }

    public required string RecursoTipo { get; set; }
    public Guid? RecursoId { get; set; }

    /// <summary>"permitido" o "denegado". El intento denegado se registra con el mismo detalle que el exitoso.</summary>
    public required string Resultado { get; set; }

    /// <summary>Identificador propagado desde el gateway (EC-20). Une las cuatro series sin compartir tabla.</summary>
    public required string CorrelacionId { get; set; }

    /// <summary>Dirección de red de origen, tomada del gateway.</summary>
    public string? Origen { get; set; }

    /// <summary>En tiempo universal coordinado: es la clave de orden de la consulta compuesta del auditor.</summary>
    public DateTimeOffset OcurridoEn { get; set; }

    // Nota importante, directamente del DD: esta entidad NO tiene ningún campo
    // para el contenido del recurso. Registra que se intentó acceder y con qué
    // resultado, nunca el dato al que se intentó acceder. No agregues un campo
    // de "detalle" o "payload" para facilitar depuración — convertiría la
    // bitácora en una segunda vía de fuga de lo que el sistema protege.
}
