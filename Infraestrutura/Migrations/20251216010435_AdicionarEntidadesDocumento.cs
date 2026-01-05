using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarEntidadesDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoDeDocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataInclusao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NomeDoArquivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HashDocumentoOriginal = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StatusDocumento = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documento_TipoDocumento_TipoDeDocumentoId",
                        column: x => x.TipoDeDocumentoId,
                        principalTable: "TipoDocumento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assinante",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssinanteId = table.Column<long>(type: "bigint", nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assinante", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assinante_AspNetUsers_AssinanteId",
                        column: x => x.AssinanteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assinante_Documento_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assinatura",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataDaAssinatura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TipoAssinatura = table.Column<int>(type: "int", nullable: false),
                    AssinanteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HashDocumentoAssinado = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HashNomeArquivoReconhecimentoFacial = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NomeArquivoReconhecimentoFacial = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    SistemaDeAssinatura = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VersaoSistema = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SistemaOperacional = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assinatura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assinatura_Assinante_AssinanteId",
                        column: x => x.AssinanteId,
                        principalTable: "Assinante",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assinatura_Documento_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "Documento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assinante_AssinanteId_DocumentoId",
                table: "Assinante",
                columns: new[] { "AssinanteId", "DocumentoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assinante_DocumentoId",
                table: "Assinante",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Assinatura_AssinanteId",
                table: "Assinatura",
                column: "AssinanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Assinatura_DocumentoId",
                table: "Assinatura",
                column: "DocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Documento_TipoDeDocumentoId",
                table: "Documento",
                column: "TipoDeDocumentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assinatura");

            migrationBuilder.DropTable(
                name: "Assinante");

            migrationBuilder.DropTable(
                name: "Documento");
        }
    }
}
