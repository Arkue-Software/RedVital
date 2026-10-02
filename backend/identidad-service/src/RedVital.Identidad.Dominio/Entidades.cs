// RedVital — Servicio de Identidad
// Entidades de db_identidad, conforme al diccionario de datos del DD V2.0, Sección 6.
// T-304.1 — esquema, migraciones EF Core y roles.

namespace RedVital.Identidad.Dominio;

/// <summary>
/// Cuenta de acceso de los perfiles U2 a U7. El donante anónimo U1 no tiene cuenta.
/// El Id es la reivindicación "sub" del token de acceso.
/// </summary>
public class Usuario
{
    public Guid Id { get; set; }

    /// <summary>Identificador de acceso. Único.</summary>
    public required string Correo { get; set; }

    /// <summary>
    /// Obligatorio para los roles distintos de donante. Nulo para el donante:
    /// su nombre es dato del contexto de donación y no se copia aquí (PD-01).
    /// </summary>
    public string? Nombre { get; set; }

    /// <summary>
    /// Resumen de la credencial con sal y función de derivación lenta.
    /// Nunca se guarda ni se registra la credencial en claro.
    /// </summary>
    public required string CredencialHash { get; set; }

    public required string RolCodigo { get; set; }
    public Rol? Rol { get; set; }

    /// <summary>La desactivación revoca todas las sesiones del usuario (ADR-008).</summary>
    public bool Activo { get; set; } = true;

    public DateTimeOffset? UltimoAccesoEn { get; set; }
    public DateTimeOffset CreadoEn { get; set; }
    public DateTimeOffset ActualizadoEn { get; set; }

    public ICollection<UsuarioJurisdiccion> Jurisdicciones { get; set; } = new List<UsuarioJurisdiccion>();
    public ICollection<Sesion> Sesiones { get; set; } = new List<Sesion>();
}

/// <summary>
/// Catálogo cerrado: los seis roles con sesión del SRS. El rol "servicio" de los
/// tokens de servicio NO vive aquí — corresponde a CredencialServicio.
/// </summary>
public class Rol
{
    /// <summary>donante, operador, admin_banco, coordinador, admin_nacional, auditor.</summary>
    public required string Codigo { get; set; }

    public required string Perfil { get; set; }

    /// <summary>
    /// Ámbito de jurisdicción admitido para este rol: "ninguno", "institucion"
    /// (exactamente una) o "territorio" (uno o varios, con el nivel admitido).
    /// Ver Tabla 13 del DD para el detalle exacto por rol.
    /// </summary>
    public required string AmbitoAdmitido { get; set; }
}

/// <summary>
/// Alcance de datos de un usuario (RF-22). Sin esta entidad, EC-01 no es administrable.
/// </summary>
public class UsuarioJurisdiccion
{
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    /// <summary>"territorio" o "institucion", conforme al rol del usuario (Tabla 13).</summary>
    public required string Ambito { get; set; }

    /// <summary>Código DIVIPOLA. Presente si Ambito es "territorio".</summary>
    public string? TerritorioCodigo { get; set; }

    /// <summary>
    /// Ruta derivada del código territorial (/00, /00/11, /00/11/11001).
    /// Es literalmente lo que viaja en el token como "territorio:&lt;ruta&gt;".
    /// Identidad NO consulta al Servicio Institucional para derivarla.
    /// </summary>
    public string? TerritorioRuta { get; set; }

    /// <summary>Presente si Ambito es "institucion". Viaja como "institucion:&lt;id&gt;".</summary>
    public Guid? InstitucionId { get; set; }

    /// <summary>Administrador nacional que la asignó. La asignación es acción sensible y se audita.</summary>
    public Guid AsignadaPor { get; set; }

    public DateTimeOffset VigenteDesde { get; set; }

    /// <summary>Nulo mientras esté vigente. Retirar una jurisdicción no borra el registro.</summary>
    public DateTimeOffset? VigenteHasta { get; set; }
}

/// <summary>
/// Sesión de renovación de ocho horas (ADR-008). Existe para poder revocar y para
/// detectar la reutilización de un secreto de renovación — NO es el token de acceso,
/// que no se guarda en ninguna tabla porque se valida por firma.
/// </summary>
public class Sesion
{
    /// <summary>Primera parte del token de renovación: &lt;sesion&gt;.&lt;secreto&gt;.</summary>
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    /// <summary>Resumen del secreto de renovación vigente. Cada renovación lo sustituye.</summary>
    public required string SecretoHash { get; set; }

    public DateTimeOffset EmitidaEn { get; set; }

    /// <summary>A lo sumo ocho horas después de EmitidaEn. Vigencia absoluta: la renovación no la extiende.</summary>
    public DateTimeOffset ExpiraEn { get; set; }

    public DateTimeOffset? RenovadaEn { get; set; }
    public DateTimeOffset? RevocadaEn { get; set; }

    /// <summary>cierre, desactivacion, cambio_rol, cambio_jurisdiccion, reutilizacion, rotacion_emergencia.</summary>
    public string? MotivoRevocacion { get; set; }

    /// <summary>Dirección de red del inicio de sesión.</summary>
    public string? Origen { get; set; }
}

/// <summary>
/// Credencial de cliente de un componente que invoca contratos sin usuario (ADR-008).
/// </summary>
public class CredencialServicio
{
    public Guid Id { get; set; }

    /// <summary>gateway, donacion o notificaciones. Es la reivindicación "sub" del token de servicio.</summary>
    public required string Cliente { get; set; }

    /// <summary>Resumen del secreto de cliente, que el componente recibe como archivo en /run/secrets.</summary>
    public required string SecretoHash { get; set; }

    public bool Activa { get; set; } = true;
    public DateTimeOffset CreadaEn { get; set; }
    public DateTimeOffset? RotadaEn { get; set; }
}
