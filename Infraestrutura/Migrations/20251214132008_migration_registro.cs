using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class migration_registro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomeDaFotoRegistrada",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "RE",
                table: "RegistroAbility",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Cargo",
                table: "RegistroAbility",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CargoId",
                table: "RegistroAbility",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CentroDeCusto",
                table: "RegistroAbility",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Coordenador",
                table: "RegistroAbility",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DatadeDemissao",
                table: "RegistroAbility",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DatadeInclusao",
                table: "RegistroAbility",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Departamento",
                table: "RegistroAbility",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gerente",
                table: "RegistroAbility",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RECoordenador",
                table: "RegistroAbility",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "REGerente",
                table: "RegistroAbility",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RESupervisor",
                table: "RegistroAbility",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReTelefonica",
                table: "RegistroAbility",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Setor",
                table: "RegistroAbility",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Supervisor",
                table: "RegistroAbility",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cargo",
                table: "RegistroAbility");

            migrationBuilder.DropColumn(
                name: "CargoId",
                table: "RegistroAbility");

            migrationBuilder.DropColumn(
                name: "CentroDeCusto",
                table: "RegistroAbility");

            migrationBuilder.DropColumn(
                name: "Coordenador",
                table: "RegistroAbility");

            migrationBuilder.DropColumn(
                name: "DatadeDemissao",
                table: "RegistroAbility");

            migrationBuilder.DropColumn(
                name: "DatadeInclusao",
                table: "RegistroAbility");

            migrationBuilder.DropColumn(
                name: "Departamento",
                table: "RegistroAbility");

            migrationBuilder.DropColumn(
                name: "Gerente",
                table: "RegistroAbility");

            migrationBuilder.DropColumn(
                name: "RECoordenador",
                table: "RegistroAbility");

            migrationBuilder.DropColumn(
                name: "REGerente",
                table: "RegistroAbility");

            migrationBuilder.DropColumn(
                name: "RESupervisor",
                table: "RegistroAbility");

            migrationBuilder.DropColumn(
                name: "ReTelefonica",
                table: "RegistroAbility");

            migrationBuilder.DropColumn(
                name: "Setor",
                table: "RegistroAbility");

            migrationBuilder.DropColumn(
                name: "Supervisor",
                table: "RegistroAbility");

            migrationBuilder.AlterColumn<string>(
                name: "RE",
                table: "RegistroAbility",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "NomeDaFotoRegistrada",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
