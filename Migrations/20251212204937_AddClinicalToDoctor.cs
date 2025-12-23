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

            // Güvenli Migration Adımı 1: Varsayılan bir Klinik oluştur (Eğer yoksa)
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM Clinicals WHERE Id = 1) INSERT INTO Clinicals (Name, Address, PhoneNumber) VALUES ('Genel Klinik', 'Merkez', '0000000000')");

            // Güvenli Migration Adımı 2: Mevcut "sahipsiz" doktorları bu kliniğe ata
            migrationBuilder.Sql("UPDATE Doctors SET ClinicalId = 1 WHERE ClinicalId IS NULL");

            migrationBuilder.AlterColumn<int>(
                name: "ClinicalId",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 1, // Yeni eklenecekler için de varsayılan 1
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
