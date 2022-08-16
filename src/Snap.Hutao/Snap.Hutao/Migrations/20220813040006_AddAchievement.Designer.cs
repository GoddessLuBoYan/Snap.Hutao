﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Snap.Hutao.Context.Database;

#nullable disable

namespace Snap.Hutao.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20220813040006_AddAchievement")]
    partial class AddAchievement
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.8");

            modelBuilder.Entity("Snap.Hutao.Model.Entity.Achievement", b =>
                {
                    b.Property<Guid>("InnerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("Current")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("Time")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("UserId")
                        .HasColumnType("TEXT");

                    b.HasKey("InnerId");

                    b.HasIndex("UserId");

                    b.ToTable("achievements");
                });

            modelBuilder.Entity("Snap.Hutao.Model.Entity.SettingEntry", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.ToTable("settings");
                });

            modelBuilder.Entity("Snap.Hutao.Model.Entity.User", b =>
                {
                    b.Property<Guid>("InnerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Cookie")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsSelected")
                        .HasColumnType("INTEGER");

                    b.HasKey("InnerId");

                    b.ToTable("users");
                });

            modelBuilder.Entity("Snap.Hutao.Model.Entity.Achievement", b =>
                {
                    b.HasOne("Snap.Hutao.Model.Entity.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
