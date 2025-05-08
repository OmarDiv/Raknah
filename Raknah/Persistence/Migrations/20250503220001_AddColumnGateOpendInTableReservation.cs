using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Raknah.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnGateOpendInTableReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGateOpened",
                table: "Reservations",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGateOpened",
                table: "Reservations");
        }
    }
}
