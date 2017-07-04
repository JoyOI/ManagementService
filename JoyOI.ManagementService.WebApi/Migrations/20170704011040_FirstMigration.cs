using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JoyOI.ManagementService.WebApi.Migrations
{
    public partial class FirstMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Actors",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    Body = table.Column<string>(nullable: true),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Revision = table.Column<long>(nullable: false),
                    UpdateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actors", x => x.Id);
                    table.UniqueConstraint("AK_Actors_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "ActorHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    Body = table.Column<string>(nullable: true),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Revision = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActorHistories", x => x.Id);
                    table.UniqueConstraint("AK_ActorHistories_Name_Revision", x => new { x.Name, x.Revision });
                });

            migrationBuilder.CreateTable(
                name: "Blobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    Body = table.Column<byte[]>(nullable: true),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    UpdateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StateMachine",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    Body = table.Column<string>(nullable: true),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Revision = table.Column<long>(nullable: false),
                    UpdateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateMachine", x => x.Id);
                    table.UniqueConstraint("AK_StateMachine_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "StateMachineHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    Body = table.Column<string>(nullable: true),
                    CreateTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Revision = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateMachineHistories", x => x.Id);
                    table.UniqueConstraint("AK_StateMachineHistories_Name_Revision", x => new { x.Name, x.Revision });
                });

            migrationBuilder.CreateTable(
                name: "StateMachineInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    CurrentActor = table.Column<JsonObject<ActorInfo>>(nullable: true),
                    CurrentContainer = table.Column<string>(nullable: true),
                    CurrentNode = table.Column<string>(nullable: true),
                    EndTime = table.Column<DateTime>(nullable: false),
                    FinishedActors = table.Column<JsonObject<ActorInfo[]>>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    RefRevision = table.Column<long>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateMachineInstances", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StateMachineInstances_Name",
                table: "StateMachineInstances",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_StateMachineInstances_Name_Status",
                table: "StateMachineInstances",
                columns: new[] { "Name", "Status" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Actors");

            migrationBuilder.DropTable(
                name: "ActorHistories");

            migrationBuilder.DropTable(
                name: "Blobs");

            migrationBuilder.DropTable(
                name: "StateMachine");

            migrationBuilder.DropTable(
                name: "StateMachineHistories");

            migrationBuilder.DropTable(
                name: "StateMachineInstances");
        }
    }
}
