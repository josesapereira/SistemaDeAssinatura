using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class migration_Documento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UsuarioInclusaoId",
                table: "Documento",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "IPDaAssinatura",
                table: "Assinatura",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Documento_UsuarioInclusaoId",
                table: "Documento",
                column: "UsuarioInclusaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documento_AspNetUsers_UsuarioInclusaoId",
                table: "Documento",
                column: "UsuarioInclusaoId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documento_AspNetUsers_UsuarioInclusaoId",
                table: "Documento");

            migrationBuilder.DropIndex(
                name: "IX_Documento_UsuarioInclusaoId",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "UsuarioInclusaoId",
                table: "Documento");

            migrationBuilder.DropColumn(
                name: "IPDaAssinatura",
                table: "Assinatura");
        }
    }
}
