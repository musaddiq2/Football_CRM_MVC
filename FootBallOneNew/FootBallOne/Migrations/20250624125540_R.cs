using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootBallOne.Migrations
{
    /// <inheritdoc />
    public partial class R : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_registrationManagements",
                table: "registrationManagements");

            migrationBuilder.RenameTable(
                name: "registrationManagements",
                newName: "RGManagements");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RGManagements",
                table: "RGManagements",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RGManagements",
                table: "RGManagements");

            migrationBuilder.RenameTable(
                name: "RGManagements",
                newName: "registrationManagements");

            migrationBuilder.AddPrimaryKey(
                name: "PK_registrationManagements",
                table: "registrationManagements",
                column: "Id");
        }
    }
}
