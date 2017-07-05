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

            var stateMachineEntity = modelBuilder.Entity<StateMachineEntity>();
            stateMachineEntity.HasIndex(x => x.Name).IsUnique();

            var stateMachineInstanceEntity = modelBuilder.Entity<StateMachineInstanceEntity>();
            stateMachineInstanceEntity.Property(x => x._FinishedActors).HasColumnName("FinishedActors");
            stateMachineInstanceEntity.Property(x => x._CurrentActor).HasColumnName("CurrentActor");
            stateMachineInstanceEntity.Ignore(x => x.FinishedActors);
            stateMachineInstanceEntity.Ignore(x => x.CurrentActor);
            stateMachineInstanceEntity.HasIndex(x => x.Name);
            stateMachineInstanceEntity.HasIndex(x => new { x.Name, x.Status });
        }
    }
}
