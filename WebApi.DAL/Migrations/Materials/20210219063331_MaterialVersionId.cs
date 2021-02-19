using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApi.DAL.Migrations.Materials
{
    public partial class MaterialVersionId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Materialss",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterialName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    ActualVersion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materialss", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaterialVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    MetaFileSize = table.Column<double>(type: "float", nullable: false),
                    MetaDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialVersions_Materialss_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materialss",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialVersions_MaterialId",
                table: "MaterialVersions",
                column: "MaterialId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialVersions");

            migrationBuilder.DropTable(
                name: "Materialss");
        }
    }
}
