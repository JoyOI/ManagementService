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
                    Name = table.Column<string>(type: "varchar(127)", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Blobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    BlobId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Body = table.Column<byte[]>(type: "longblob", nullable: true),
                    BodyHash = table.Column<string>(type: "varchar(127)", nullable: true),
                    ChunkIndex = table.Column<int>(type: "int", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Remark = table.Column<string>(type: "longtext", nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StateMachineInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Exception = table.Column<string>(type: "longtext", nullable: true),
                    ExecutionKey = table.Column<string>(type: "longtext", nullable: true),
                    FromManagementService = table.Column<string>(type: "longtext", nullable: true),
                    Name = table.Column<string>(type: "varchar(127)", nullable: true),
                    ReRunTimes = table.Column<int>(type: "int", nullable: false),
                    Stage = table.Column<string>(type: "longtext", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    InitialBlobs = table.Column<string>(type: "longtext", nullable: true),
                    Limitation = table.Column<string>(type: "longtext", nullable: true),
                    StartedActors = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateMachineInstances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StateMachines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Body = table.Column<string>(type: "longtext", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "varchar(127)", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Limitation = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateMachines", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Actors_Name",
                table: "Actors",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blobs_BlobId",
                table: "Blobs",
                column: "BlobId");

            migrationBuilder.CreateIndex(
                name: "IX_Blobs_BodyHash",
                table: "Blobs",
                column: "BodyHash");

            migrationBuilder.CreateIndex(
                name: "IX_Blobs_BlobId_ChunkIndex",
                table: "Blobs",
                columns: new[] { "BlobId", "ChunkIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StateMachineInstances_Name",
                table: "StateMachineInstances",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_StateMachineInstances_Name_Status",
                table: "StateMachineInstances",
                columns: new[] { "Name", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StateMachines_Name",
                table: "StateMachines",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Actors");

            migrationBuilder.DropTable(
                name: "Blobs");

            migrationBuilder.DropTable(
                name: "StateMachineInstances");

            migrationBuilder.DropTable(
                name: "StateMachines");
        }
    }
}
