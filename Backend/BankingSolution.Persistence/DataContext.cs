using BankingSolution.Logic.Poco;
using Microsoft.EntityFrameworkCore;

namespace BankingSolution.Persistence
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions options): base(options)
        {
            
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transaction>().Property(a => a.Id).ValueGeneratedOnAdd();
        }
    }
}