﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Brighid.Discord.Adapter.Database
{
    /// <summary>
    /// This removes the unique index on the RemoteId column of the Buckets table.
    /// </summary>
    public partial class BucketsRemoveUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Buckets_RemoteId",
                table: "Buckets");

            migrationBuilder.CreateIndex(
                name: "IX_Buckets_RemoteId",
                table: "Buckets",
                column: "RemoteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Buckets_RemoteId",
                table: "Buckets");

            migrationBuilder.CreateIndex(
                name: "IX_Buckets_RemoteId",
                table: "Buckets",
                column: "RemoteId",
                unique: true);
        }
    }
}
