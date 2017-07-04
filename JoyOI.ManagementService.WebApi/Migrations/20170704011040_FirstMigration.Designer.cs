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
    [Migration("20170704011040_FirstMigration")]
    partial class FirstMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

                    b.Property<long>("Revision")
                        .IsConcurrencyToken();

                    b.Property<DateTime>("UpdateTime");

                    b.HasKey("Id");

                    b.HasAlternateKey("Name");

                    b.ToTable("Actors");
                });

            modelBuilder.Entity("JoyOI.ManagementService.Model.Entities.ActorHistoryEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Body");

                    b.Property<DateTime>("CreateTime");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<long>("Revision");

                    b.HasKey("Id");

                    b.HasAlternateKey("Name", "Revision");

                    b.ToTable("ActorHistories");
                });

            modelBuilder.Entity("JoyOI.ManagementService.Model.Entities.BlobEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("Body");

                    b.Property<DateTime>("CreateTime");

                    b.Property<string>("Name");

                    b.Property<DateTime>("UpdateTime");

                    b.HasKey("Id");

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

                    b.Property<long>("Revision")
                        .IsConcurrencyToken();

                    b.Property<DateTime>("UpdateTime");

                    b.HasKey("Id");

                    b.HasAlternateKey("Name");

                    b.ToTable("StateMachine");
                });

            modelBuilder.Entity("JoyOI.ManagementService.Model.Entities.StateMachineHistoryEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Body");

                    b.Property<DateTime>("CreateTime");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<long>("Revision");

                    b.HasKey("Id");

                    b.HasAlternateKey("Name", "Revision");

                    b.ToTable("StateMachineHistories");
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

                    b.Property<long>("RefRevision");

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
