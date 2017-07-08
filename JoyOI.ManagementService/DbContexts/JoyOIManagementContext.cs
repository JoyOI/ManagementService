using JoyOI.ManagementService.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.DbContexts
{
    public class JoyOIManagementContext : DbContext
    {
        public DbSet<ActorEntity> Actors { get; set; }
        public DbSet<BlobEntity> Blobs { get; set; }
        public DbSet<StateMachineEntity> StateMachine { get; set; }
        public DbSet<StateMachineInstanceEntity> StateMachineInstances { get; set; }

        public JoyOIManagementContext() : base() { }

        public JoyOIManagementContext(DbContextOptions<JoyOIManagementContext> options) : base(options) { }

        public static string ConnectionString { get; set; }
        public static string MigrationAssembly { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(ConnectionString, b => b.MigrationsAssembly(MigrationAssembly));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var actorEntity = modelBuilder.Entity<ActorEntity>();
            actorEntity.HasIndex(x => x.Name).IsUnique();

            var blobsEntity = modelBuilder.Entity<BlobEntity>();
            blobsEntity.HasIndex(x => x.BlobId);
            blobsEntity.HasIndex(x => new { x.BlobId, x.ChunkIndex }).IsUnique();
            blobsEntity.HasIndex(x => x.BodyHash); // 考虑到并发上传, 不设唯一键

            var stateMachineEntity = modelBuilder.Entity<StateMachineEntity>();
            stateMachineEntity.Property(x => x._Limitation).HasColumnName("Limitation").HasColumnType("json");
            stateMachineEntity.Ignore(x => x.Limitation);
            stateMachineEntity.HasIndex(x => x.Name).IsUnique();

            var stateMachineInstanceEntity = modelBuilder.Entity<StateMachineInstanceEntity>();
            stateMachineInstanceEntity.Property(x => x._StartedActors).HasColumnName("FinishedActors").HasColumnType("json");
            stateMachineInstanceEntity.Property(x => x._InitialBlobs).HasColumnName("CurrentActor").HasColumnType("json");
            stateMachineInstanceEntity.Property(x => x._Limitation).HasColumnName("Limitation").HasColumnType("json");
            stateMachineInstanceEntity.Ignore(x => x.StartedActors);
            stateMachineInstanceEntity.Ignore(x => x.InitialBlobs);
            stateMachineInstanceEntity.Ignore(x => x.Limitation);
            stateMachineInstanceEntity.HasIndex(x => x.Name);
            stateMachineInstanceEntity.HasIndex(x => new { x.Name, x.Status });
        }
    }
}
