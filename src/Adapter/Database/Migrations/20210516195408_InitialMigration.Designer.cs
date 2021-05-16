﻿// <auto-generated />
using System;
using Brighid.Discord.Adapter.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Brighid.Discord.Adapter.Database
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20210516195408_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("Brighid.Discord.Adapter.Requests.Bucket", b =>
                {
                    b.Property<string>("RemoteId")
                        .HasColumnType("varchar(255) CHARACTER SET utf8mb4");

                    b.Property<string>("ApiCategory")
                        .IsRequired()
                        .HasColumnType("varchar(1) CHARACTER SET utf8mb4");

                    b.Property<ulong>("Endpoints")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("HitsRemaining")
                        .HasColumnType("int");

                    b.Property<string>("MajorParameters")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTimeOffset>("ResetAfter")
                        .HasColumnType("datetime(6)");

                    b.HasKey("RemoteId");

                    b.ToTable("Buckets");
                });
#pragma warning restore 612, 618
        }
    }
}
