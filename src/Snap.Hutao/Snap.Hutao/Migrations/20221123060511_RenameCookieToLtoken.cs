﻿// <auto-generated />
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Snap.Hutao.Migrations
{
    /// <inheritdoc />
    public partial class RenameCookieToLtoken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Cookie",
                table: "users",
                newName: "Ltoken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Ltoken",
                table: "users",
                newName: "Cookie");
        }
    }
}
