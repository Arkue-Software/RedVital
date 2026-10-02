using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RedVital.Identidad.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class InicialIdentidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "credencial_servicio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    secreto_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    rotada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credencial_servicio", x => x.id);
                    table.CheckConstraint("ck_credencial_servicio_cliente", "cliente IN ('gateway', 'donacion', 'notificaciones')");
                });

            migrationBuilder.CreateTable(
                name: "rol",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    perfil = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ambito_admitido = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol", x => x.codigo);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    correo = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    credencial_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    rol_id = table.Column<string>(type: "character varying(20)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    ultimo_acceso_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuario_rol_rol_id",
                        column: x => x.rol_id,
                        principalTable: "rol",
                        principalColumn: "codigo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sesion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    secreto_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    emitida_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expira_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    renovada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revocada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    motivo_revocacion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    origen = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesion", x => x.id);
                    table.CheckConstraint("ck_sesion_motivo_revocacion", "motivo_revocacion IS NULL OR motivo_revocacion IN ('cierre', 'desactivacion', 'cambio_rol', 'cambio_jurisdiccion', 'reutilizacion', 'rotacion_emergencia')");
                    table.ForeignKey(
                        name: "FK_sesion_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_jurisdiccion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ambito = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    territorio_codigo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    territorio_ruta = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    institucion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    asignada_por = table.Column<Guid>(type: "uuid", nullable: false),
                    vigente_desde = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    vigente_hasta = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_jurisdiccion", x => x.id);
                    table.CheckConstraint("ck_usuario_jurisdiccion_ambito", "ambito IN ('territorio', 'institucion')");
                    table.ForeignKey(
                        name: "FK_usuario_jurisdiccion_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "rol",
                columns: new[] { "codigo", "ambito_admitido", "perfil" },
                values: new object[,]
                {
                    { "admin_banco", "institucion (exactamente una)", "U4 — Administrador de banco" },
                    { "admin_nacional", "territorio nacional", "U6 — Administrador nacional" },
                    { "auditor", "territorio nacional, lectura limitada", "U7 — Auditor" },
                    { "coordinador", "territorio departamental o municipal, uno o varios", "U5 — Coordinador territorial" },
                    { "donante", "ninguno", "U2 — Donante registrado" },
                    { "operador", "institucion (exactamente una)", "U3 — Operador de banco" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_credencial_servicio_cliente",
                table: "credencial_servicio",
                column: "cliente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sesion_usuario_id",
                table: "sesion",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_correo",
                table: "usuario",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_rol_id",
                table: "usuario",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_jurisdiccion_usuario_id",
                table: "usuario_jurisdiccion",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "credencial_servicio");

            migrationBuilder.DropTable(
                name: "sesion");

            migrationBuilder.DropTable(
                name: "usuario_jurisdiccion");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "rol");
        }
    }
}
