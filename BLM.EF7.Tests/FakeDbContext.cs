using BLM.NetStandard.Tests;
using Microsoft.EntityFrameworkCore;

namespace BLM.EF7.Tests
{
    public class FakeDbContext : DbContext
    {
        public FakeDbContext(DbContextOptions options) : base(options)
        {

        }

        /// <summary>
        /// In EFCore 1.x there is no automated inheritance, only if we're providing it
        /// </summary>
        /// <param name="mb"></param>
        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);
            mb.Entity<LogicalDeleteEntity>().HasBaseType<MockEntity>();
        }

        public virtual DbSet<MockEntity> MockEntities { get; set; }
        public virtual DbSet<MockNestedEntity> MockNestedEntities { get; set; }
        public virtual DbSet<MockInterpretedEntity> MockInterpretedEntities { get; set; }
    }

}
