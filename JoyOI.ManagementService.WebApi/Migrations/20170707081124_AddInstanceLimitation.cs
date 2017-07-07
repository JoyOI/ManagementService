using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace JoyOI.ManagementService.WebApi.Migrations
{
    public partial class AddInstanceLimitation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Limitation",
                table: "StateMachineInstances",
                type: "longtext",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Limitation",
                table: "StateMachineInstances");
        }
    }
}
