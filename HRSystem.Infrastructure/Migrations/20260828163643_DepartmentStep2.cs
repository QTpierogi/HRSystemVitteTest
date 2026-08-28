using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DepartmentStep2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Departments");

            migrationBuilder.AddColumn<DateTime>(
                name: "DissolvedDate",
                table: "Departments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId1",
                table: "DepartmentHierarchies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentHierarchies_DepartmentId1",
                table: "DepartmentHierarchies",
                column: "DepartmentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentHierarchies_Departments_DepartmentId1",
                table: "DepartmentHierarchies",
                column: "DepartmentId1",
                principalTable: "Departments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentHierarchies_Departments_DepartmentId1",
                table: "DepartmentHierarchies");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentHierarchies_DepartmentId1",
                table: "DepartmentHierarchies");

            migrationBuilder.DropColumn(
                name: "DissolvedDate",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "DepartmentId1",
                table: "DepartmentHierarchies");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Departments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
