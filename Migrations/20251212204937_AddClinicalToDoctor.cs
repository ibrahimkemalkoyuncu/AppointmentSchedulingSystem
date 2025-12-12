using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentSchedulingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalToDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Clinicals_ClinicalId",
                table: "Doctors");

            migrationBuilder.AlterColumn<int>(
                name: "ClinicalId",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Clinicals_ClinicalId",
                table: "Doctors",
                column: "ClinicalId",
                principalTable: "Clinicals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Clinicals_ClinicalId",
                table: "Doctors");

            migrationBuilder.AlterColumn<int>(
                name: "ClinicalId",
                table: "Doctors",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Clinicals_ClinicalId",
                table: "Doctors",
                column: "ClinicalId",
                principalTable: "Clinicals",
                principalColumn: "Id");
        }
    }
}
