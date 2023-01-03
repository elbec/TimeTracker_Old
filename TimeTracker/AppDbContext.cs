using System;
using System.Data.Entity;

namespace TimeTracker
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("AppDbContext")
        {

        }

        public DbSet<Task> Tasks { get; set; }
        public DbSet<Recorder> Recorders { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<AppDbContext>(null);
            modelBuilder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));

            base.OnModelCreating(modelBuilder);
            
        }
    }
}
