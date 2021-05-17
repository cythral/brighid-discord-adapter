using System;

using Microsoft.EntityFrameworkCore.Migrations;

namespace Brighid.Discord.Adapter.Database
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Buckets",
                columns: table => new
                {
                    RemoteId = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", nullable: false),
                    ApiCategory = table.Column<string>(type: "varchar(1) CHARACTER SET utf8mb4", nullable: false),
                    Endpoints = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    MajorParameters = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    HitsRemaining = table.Column<int>(type: "int", nullable: false),
                    ResetAfter = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buckets", x => x.RemoteId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Buckets");
        }
    }
}
