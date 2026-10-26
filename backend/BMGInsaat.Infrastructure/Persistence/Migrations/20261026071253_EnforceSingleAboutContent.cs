using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BMGInsaat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleAboutContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_AboutContents_SingleRecord",
                table: "AboutContents",
                sql: "\"Id\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_AboutContents_SingleRecord",
                table: "AboutContents");
        }
    }
}
