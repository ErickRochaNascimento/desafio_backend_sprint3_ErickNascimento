using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BancoDigital.Migrations
{
    /// <inheritdoc />
    public partial class Destinatario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NomeDestinatario",
                table: "Transacoes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomeDestinatario",
                table: "Transacoes");
        }
    }
}
