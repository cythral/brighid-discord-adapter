using System;

using Microsoft.EntityFrameworkCore.Migrations;

namespace Brighid.Discord.Adapter.Database
{
    /// <summary>
    /// Since we could have multiple rows with the same RemoteId but different
    /// major parameters, the RemoteId cannot be the primary key.  Instead we are defining
    /// a separate primary key with a Guid that has no relevance to the bucket ID returned by Discord.
    /// </summary>
    public partial class BucketPrimaryKeyChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Buckets",
                table: "Buckets");

            migrationBuilder.AlterColumn<string>(
                name: "MajorParameters",
                table: "Buckets",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "RemoteId",
                table: "Buckets",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Buckets",
                type: "binary(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Buckets",
                table: "Buckets",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Buckets_RemoteId",
                table: "Buckets",
                column: "RemoteId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Buckets",
                table: "Buckets");

            migrationBuilder.DropIndex(
                name: "IX_Buckets_RemoteId",
                table: "Buckets");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Buckets");

            migrationBuilder.AlterColumn<string>(
                name: "RemoteId",
                table: "Buckets",
                type: "varchar(255)",
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "MajorParameters",
                table: "Buckets",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Buckets",
                table: "Buckets",
                column: "RemoteId");
        }
    }
}
