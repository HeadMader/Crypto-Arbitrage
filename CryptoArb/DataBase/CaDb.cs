using Microsoft.EntityFrameworkCore;

namespace CryptoArb.DataBase
{
    public class CaDb : DbContext
    {
        public CaDb()
        {
            Database.EnsureCreated();
        }

        /// <summary>
        /// Database config
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=products.sqlite");
                optionsBuilder.EnableSensitiveDataLogging(true);
            }
        }

        public DbSet<Exchange>? Exchanges { get; set; }
        public DbSet<Product>? Products { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Binance>().HasBaseType<Exchange>();
            builder.Entity<Kucoin>().HasBaseType<Exchange>();

            base.OnModelCreating(builder);
        }


    }
    //public static class DBExtentions
    //{
    //	public static void Save(this Product product)
    //	{
    //		using (CaDb db = new())
    //		{
    //			try
    //			{
    //				Product? p = db.Products!.FirstOrDefault(p => p.Symbol == product.Symbol && p.Exchange == product.Exchange);
    //				if (p != null)
    //				{
    //					db.Products!.Remove(p);
    //				}
    //				db.Products!.Add(product);
    //				db.SaveChanges();
    //			}
    //			catch (Exception ex)
    //			{
    //				return;
    //			}
    //		}
    //	}
    //}
}
