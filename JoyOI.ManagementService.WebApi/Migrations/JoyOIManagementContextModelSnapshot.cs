using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using JoyOI.ManagementService.DbContexts;
using JoyOI.ManagementService.Model.Enums;

namespace JoyOI.ManagementService.WebApi.Migrations
{
    [DbContext(typeof(JoyOIManagementContext))]
    partial class JoyOIManagementContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("JoyOI.ManagementService.Model.Entities.ActorEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Body");

                    b.Property<DateTime>("CreateTime");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<DateTime>("UpdateTime");

                    b.HasKey("Id");

                    b.HasAlternateKey("Name");

                    b.ToTable("Actors");
                });

            modelBuilder.Entity("JoyOI.ManagementService.Model.Entities.BlobEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("BlobId");

                    b.Property<byte[]>("Body");

                    b.Property<int>("ChunkIndex");

                    b.Property<DateTime>("CreateTime");

                    b.Property<string>("Name");

                    b.Property<DateTime>("UpdateTime");

                    b.HasKey("Id");

                    b.HasAlternateKey("BlobId", "ChunkIndex");

                    b.HasIndex("BlobId");

                    b.ToTable("Blobs");
                });

            modelBuilder.Entity("JoyOI.ManagementService.Model.Entities.StateMachineEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Body");

                    b.Property<DateTime>("CreateTime");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<DateTime>("UpdateTime");

                    b.HasKey("Id");

                    b.HasAlternateKey("Name");

                    b.ToTable("StateMachine");
                });

            modelBuilder.Entity("JoyOI.ManagementService.Model.Entities.StateMachineInstanceEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<JsonObject<ActorInfo>>("CurrentActor");

                    b.Property<string>("CurrentContainer");

                    b.Property<string>("CurrentNode");

                    b.Property<DateTime>("EndTime");

                    b.Property<JsonObject<ActorInfo[]>>("FinishedActors");

                    b.Property<string>("Name");

                    b.Property<DateTime>("StartTime");

                    b.Property<int>("Status");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.HasIndex("Name", "Status");

                    b.ToTable("StateMachineInstances");
                });
        }
    }
}
