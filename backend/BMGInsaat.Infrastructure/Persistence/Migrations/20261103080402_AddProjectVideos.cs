using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BMGInsaat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectVideos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectVideos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TitleTr = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TitleEn = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DescriptionTr = table.Column<string>(type: "text", nullable: false),
                    DescriptionEn = table.Column<string>(type: "text", nullable: false),
                    VideoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectVideos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVideos_IsPublished_DisplayOrder",
                table: "ProjectVideos",
                columns: new[] { "IsPublished", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectVideos");
        }
    }
}
