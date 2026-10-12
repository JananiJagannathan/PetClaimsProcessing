using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetClaims.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPetTypeAndBreed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Breed",
                table: "Policies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PetType",
                table: "Policies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Breed",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "PetType",
                table: "Policies");
        }
    }
}
