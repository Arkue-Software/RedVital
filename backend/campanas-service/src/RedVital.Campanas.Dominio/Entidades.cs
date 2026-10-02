// RedVital — Servicio de Campañas
// Entidades de db_campana, conforme al diccionario de datos del DD V2.0.
// T-312.1 — esquema, migraciones EF Core y roles.

namespace RedVital.Campanas.Dominio;

/// <summary>
/// Campaña de donación (RF-08). El cupo y sus reservas viven en la misma base
/// a propósito: reservar un cupo nunca exige una transacción distribuida.
/// </summary>
public class Campania
{
    public Guid Id { get; set; }

    /// <summary>Referencia lógica a la institución organizadora, sin clave foránea (vive en otra base).</summary>
    public Guid InstitucionId { get; set; }

    /// <summary>Código DIVIPOLA de la sede, obtenido del Servicio Institucional al crear la campaña y fijado con ella.</summary>
    public required string TerritorioCodigo { get; set; }

    /// <summary>Ruta derivada, para filtrar por jurisdicción sin llamar a otro servicio.</summary>
    public required string TerritorioRuta { get; set; }

    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public required string Sede { get; set; }

    public DateTimeOffset IniciaEn { get; set; }
    public DateTimeOffset TerminaEn { get; set; }

    /// <summary>Nulo significa sin cupo limitado.</summary>
    public int? CupoTotal { get; set; }

    /// <summary>
    /// Se actualiza en la MISMA transacción que crea, confirma, cancela o libera
    /// una reserva, con bloqueo de fila (ver ReservaCupoRepositorio). Nunca se
    /// recalcula sumando reservas: ese recálculo es la fuente más común de
    /// condición de carrera en este tipo de modelo.
    /// </summary>
    public int CupoReservado { get; set; }

    /// <summary>borrador, publicada, cerrada, cancelada.</summary>
    public required string Estado { get; set; }

    public DateTimeOffset? PublicadaEn { get; set; }
    public Guid CreadaPor { get; set; }
    public DateTimeOffset CreadaEn { get; set; }
    public DateTimeOffset ActualizadaEn { get; set; }

    public ICollection<ReservaCupo> Reservas { get; set; } = new List<ReservaCupo>();
}

/// <summary>Reserva de un donante en una campaña publicada (RF-15).</summary>
public class ReservaCupo
{
    public Guid Id { get; set; }

    public Guid CampaniaId { get; set; }
    public Campania? Campania { get; set; }

    /// <summary>
    /// Referencia al "sub" del donante que reserva. Campañas no conoce donantes,
    /// solo una cuenta de Identidad — no hay relación de navegación hacia un
    /// "Donante" porque esa entidad vive en otra base (db_donacion).
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>pendiente, confirmada, liberada, cancelada.</summary>
    public required string Estado { get; set; }

    public DateTimeOffset CreadaEn { get; set; }

    /// <summary>El menor entre 24 horas después de CreadaEn y el inicio de la campaña.</summary>
    public DateTimeOffset ExpiraEn { get; set; }

    public DateTimeOffset? ConfirmadaEn { get; set; }
    public DateTimeOffset? CerradaEn { get; set; }
}
