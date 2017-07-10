using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace JoyOI.ManagementService.WebApi.Migrations
{
    public partial class Add_StateMachineInstanceEntity_ExecutionKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExecutionKey",
                table: "StateMachineInstances",
                type: "longtext",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutionKey",
                table: "StateMachineInstances");
        }
    }
}
