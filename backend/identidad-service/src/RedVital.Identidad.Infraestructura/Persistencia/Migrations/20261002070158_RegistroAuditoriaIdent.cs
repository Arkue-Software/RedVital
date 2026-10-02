using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedVital.Identidad.Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class RegistroAuditoriaIdent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "registro_auditoria_ident",
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
                    table.PrimaryKey("PK_registro_auditoria_ident", x => x.id);
                    table.CheckConstraint("ck_auditoria_ident_actor_tipo", "actor_tipo IN ('usuario', 'sistema', 'anonimo')");
                    table.CheckConstraint("ck_auditoria_ident_resultado", "resultado IN ('permitido', 'denegado')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_registro_auditoria_ident_correlacion_id",
                table: "registro_auditoria_ident",
                column: "correlacion_id");

            migrationBuilder.CreateIndex(
                name: "IX_registro_auditoria_ident_ocurrido_en",
                table: "registro_auditoria_ident",
                column: "ocurrido_en");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "registro_auditoria_ident");
        }
    }
}
