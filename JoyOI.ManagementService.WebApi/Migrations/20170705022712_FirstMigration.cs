using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

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
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Body = table.Column<string>(type: "longtext", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "varchar(127)", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actors", x => x.Id);
                    table.UniqueConstraint("AK_Actors_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Blobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    BlobId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Body = table.Column<byte[]>(type: "longblob", nullable: true),
                    ChunkIndex = table.Column<int>(type: "int", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blobs", x => x.Id);
                    table.UniqueConstraint("AK_Blobs_BlobId_ChunkIndex", x => new { x.BlobId, x.ChunkIndex });
                });

            migrationBuilder.CreateTable(
                name: "StateMachine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Body = table.Column<string>(type: "longtext", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "varchar(127)", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateMachine", x => x.Id);
                    table.UniqueConstraint("AK_StateMachine_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "StateMachineInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    CurrentActor = table.Column<JsonObject<ActorInfo>>(type: "json", nullable: true),
                    CurrentContainer = table.Column<string>(type: "longtext", nullable: true),
                    CurrentNode = table.Column<string>(type: "longtext", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FinishedActors = table.Column<JsonObject<ActorInfo[]>>(type: "json", nullable: true),
                    Name = table.Column<string>(type: "varchar(127)", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateMachineInstances", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Blobs_BlobId",
                table: "Blobs",
                column: "BlobId");

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
                name: "Blobs");

            migrationBuilder.DropTable(
                name: "StateMachine");

            migrationBuilder.DropTable(
                name: "StateMachineInstances");
        }
    }
}
