using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeAssignments");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentHierarchies_DepartmentId_ValidFrom",
                table: "DepartmentHierarchies");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Positions");

            migrationBuilder.CreateTable(
                name: "EmployeeDepartmentAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDepartmentAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeDepartmentAssignments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeDepartmentAssignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePositionAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePositionAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeePositionAssignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeePositionAssignments_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentHierarchies_DepartmentId_ValidFrom_ValidTo",
                table: "DepartmentHierarchies",
                columns: new[] { "DepartmentId", "ValidFrom", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDepartmentAssignments_DepartmentId",
                table: "EmployeeDepartmentAssignments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDepartmentAssignments_EmployeeId_ValidFrom_ValidTo",
                table: "EmployeeDepartmentAssignments",
                columns: new[] { "EmployeeId", "ValidFrom", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePositionAssignments_EmployeeId_ValidFrom_ValidTo",
                table: "EmployeePositionAssignments",
                columns: new[] { "EmployeeId", "ValidFrom", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePositionAssignments_PositionId",
                table: "EmployeePositionAssignments",
                column: "PositionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeDepartmentAssignments");

            migrationBuilder.DropTable(
                name: "EmployeePositionAssignments");

            migrationBuilder.DropIndex(
                name: "IX_DepartmentHierarchies_DepartmentId_ValidFrom_ValidTo",
                table: "DepartmentHierarchies");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Positions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EmployeeAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeAssignments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeAssignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeAssignments_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentHierarchies_DepartmentId_ValidFrom",
                table: "DepartmentHierarchies",
                columns: new[] { "DepartmentId", "ValidFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAssignments_DepartmentId",
                table: "EmployeeAssignments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAssignments_EmployeeId_ValidFrom",
                table: "EmployeeAssignments",
                columns: new[] { "EmployeeId", "ValidFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAssignments_PositionId",
                table: "EmployeeAssignments",
                column: "PositionId");
        }
    }
}
