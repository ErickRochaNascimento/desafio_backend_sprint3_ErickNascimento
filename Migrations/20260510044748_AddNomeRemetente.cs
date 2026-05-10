using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BancoDigital.Migrations
{
    /// <inheritdoc />
    public partial class AddNomeRemetente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NomeRemetente",
                table: "Transacoes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomeRemetente",
                table: "Transacoes");
        }
    }
}
