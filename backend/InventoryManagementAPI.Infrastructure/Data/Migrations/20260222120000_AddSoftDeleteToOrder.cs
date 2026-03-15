using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagementAPI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar columnas para implementar soft delete en Orders
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            // Crear índices para mejorar queries de soft delete
            migrationBuilder.CreateIndex(
                name: "IX_Orders_IsDeleted",
                table: "Orders",
                column: "IsDeleted");

            // Índice compuesto para queries que filtran por usuario y estado de eliminación
            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId_IsDeleted",
                table: "Orders",
                columns: new[] { "UserId", "IsDeleted" });

            // Agregar constraint para validar que DeletedAt solo se establece cuando IsDeleted = true
            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_DeletedAt_Consistency",
                table: "Orders",
                sql: "(IsDeleted = 0 AND DeletedAt IS NULL) OR (IsDeleted = 1 AND DeletedAt IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar constraint
            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_DeletedAt_Consistency",
                table: "Orders");

            // Eliminar índices
            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId_IsDeleted",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_IsDeleted",
                table: "Orders");

            // Eliminar columnas
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Orders");
        }
    }
}
