using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace JoyOI.ManagementService.WebApi.Migrations
{
    public partial class Add_StateMachineInstanceEntity_ParametersAndPriority : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "StateMachineInstances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Parameters",
                table: "StateMachineInstances",
                type: "longtext",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "StateMachineInstances");

            migrationBuilder.DropColumn(
                name: "Parameters",
                table: "StateMachineInstances");
        }
    }
}
