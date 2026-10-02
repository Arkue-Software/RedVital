// RedVital — Servicio de Campañas
// Serie de auditoría de este servicio, misma estructura que las otras tres
// (ver RegistroAuditoriaIdent en redvital-identidad-service para la explicación
// completa de cada campo — aquí no se repite por no duplicar documentación
// entre repositorios).

namespace RedVital.Campanas.Dominio;

/// <summary>
/// Solo anexado: el rol de servicio de esta base tiene permiso únicamente de
/// inserción y lectura sobre esta tabla (ver scripts/roles_campana.sql).
/// </summary>
public class RegistroAuditoriaCamp
{
    public Guid Id { get; set; }
    public required string ActorTipo { get; set; }
    public string? ActorId { get; set; }
    public string? Rol { get; set; }
    public string? JurisdiccionSolicitada { get; set; }
    public required string Operacion { get; set; }
    public required string RecursoTipo { get; set; }
    public Guid? RecursoId { get; set; }
    public required string Resultado { get; set; }
    public required string CorrelacionId { get; set; }
    public string? Origen { get; set; }
    public DateTimeOffset OcurridoEn { get; set; }
}
