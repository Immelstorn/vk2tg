using System.Data.Entity;
using vk2tg.Data.Models.DB;

namespace vk2tg.Data.Models
{
    public class Vk2TgDbContext:DbContext
    {
        public Vk2TgDbContext():base("name=DefaultConnectionString")
        {
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }

        public DbSet<TokensWait> TokensWaits { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("vk2tg");
        }
    }
}
