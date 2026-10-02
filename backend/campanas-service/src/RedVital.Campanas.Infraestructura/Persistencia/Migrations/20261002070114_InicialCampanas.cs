using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedVital.Campanas.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class InicialCampanas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "campania",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    institucion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    territorio_codigo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    territorio_ruta = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    nombre = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sede = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    inicia_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    termina_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    cupo_total = table.Column<int>(type: "integer", nullable: true),
                    cupo_reservado = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    publicada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    creada_por = table.Column<Guid>(type: "uuid", nullable: false),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campania", x => x.id);
                    table.CheckConstraint("ck_campania_estado", "estado IN ('borrador', 'publicada', 'cerrada', 'cancelada')");
                    table.CheckConstraint("ck_campania_fechas", "termina_en > inicia_en");
                });

            migrationBuilder.CreateTable(
                name: "registro_auditoria_camp",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    actor_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    rol = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    jurisdiccion_solicitada = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    operacion = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    recurso_tipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    recurso_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resultado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    correlacion_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    origen = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    ocurrido_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registro_auditoria_camp", x => x.id);
                    table.CheckConstraint("ck_auditoria_camp_actor_tipo", "actor_tipo IN ('usuario', 'sistema', 'anonimo')");
                    table.CheckConstraint("ck_auditoria_camp_resultado", "resultado IN ('permitido', 'denegado')");
                });

            migrationBuilder.CreateTable(
                name: "reserva_cupo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campania_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expira_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    confirmada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cerrada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reserva_cupo", x => x.id);
                    table.CheckConstraint("ck_reserva_estado", "estado IN ('pendiente', 'confirmada', 'liberada', 'cancelada')");
                    table.ForeignKey(
                        name: "FK_reserva_cupo_campania_campania_id",
                        column: x => x.campania_id,
                        principalTable: "campania",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_campania_estado_termina_en",
                table: "campania",
                columns: new[] { "estado", "termina_en" });

            migrationBuilder.CreateIndex(
                name: "IX_registro_auditoria_camp_correlacion_id",
                table: "registro_auditoria_camp",
                column: "correlacion_id");

            migrationBuilder.CreateIndex(
                name: "IX_registro_auditoria_camp_ocurrido_en",
                table: "registro_auditoria_camp",
                column: "ocurrido_en");

            migrationBuilder.CreateIndex(
                name: "IX_reserva_cupo_campania_id",
                table: "reserva_cupo",
                column: "campania_id");

            migrationBuilder.CreateIndex(
                name: "IX_reserva_cupo_estado_expira_en",
                table: "reserva_cupo",
                columns: new[] { "estado", "expira_en" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "registro_auditoria_camp");

            migrationBuilder.DropTable(
                name: "reserva_cupo");

            migrationBuilder.DropTable(
                name: "campania");
        }
    }
}
