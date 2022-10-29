using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CryptoArb
{
	public class CaDb : DbContext
	{
		public CaDb() 
		{
			Database.EnsureCreated();
		}
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
			// Customize the ASP.NET Identity model and override the defaults if needed.
			// For example, you can rename the ASP.NET Identity table names and more.
			// Add your customizations after calling base.OnModelCreating(builder);
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
