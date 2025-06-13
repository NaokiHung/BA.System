using Microsoft.EntityFrameworkCore;
using BA.Server.Core.Entities;

namespace BA.Server.Data.Contexts
{
    public class ExpenseDbContext : DbContext
    {
        public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options) : base(options)
        {
        }
        
        public DbSet<MonthlyBudget> MonthlyBudgets { get; set; }
        public DbSet<CashExpense> CashExpenses { get; set; }
        public DbSet<CreditCardExpense> CreditCardExpenses { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // MonthlyBudget 複合主鍵
            modelBuilder.Entity<MonthlyBudget>()
                .HasKey(e => new { e.UserId, e.Year, e.Month });
            
            // CashExpense 索引
            modelBuilder.Entity<CashExpense>()
                .HasIndex(e => new { e.UserId, e.Year, e.Month });

            // CreditCardExpense 索引
            modelBuilder.Entity<CreditCardExpense>()
                .HasIndex(e => new { e.UserId, e.Year, e.Month });
        }
    }
}