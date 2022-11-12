using CryptoArb.analysis;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;

namespace CryptoArb
{

	public static class Program
	{
		
		public static void Main(string[] args)
		{
			Init();

			var builder = WebApplication.CreateBuilder();

			var app = builder.Build();
		
			Task.Run(DBUpdate);


			app.Run(async (context) =>
			{
				var response = context.Response;
				var request = context.Request;
				var path = request.Path;

				if (path == "/api/intraexchangebond" && request.Method == "GET")
				{
					var query = request.Query;
					int exchangeId= 0;
					float volume = 0;
					float profit = 0;

					int.TryParse(query["exchangeId"], out exchangeId);
					float.TryParse(query["volume"], out volume);
					float.TryParse(query["profit"], out profit);





					//((Binance)ExchangesManager.Exchanges.FirstOrDefault(e => e.ID == exchangeId)).AccountSnapshot();
					

					List<Bond> bonds = await ArbSituation.TriangularSearch(exchangeId, volume, profit);
					//bonds.OrderBy(f => f.Profit);
					//bonds.Sort((p,p2)=>)
					await response.WriteAsJsonAsync(bonds);

				}
				else if (path == "/api/priceskucoin" && request.Method == "GET")
				{
					//var pricesKucoin = x[1].GetBaseProductInfo().Result;
					//await response.WriteAsJsonAsync(pricesKucoin);
				}else if (path == "/css/style.css"  && request.Method == "GET")
				{
					response.ContentType = "text/css; charset=utf-8";
					await response.SendFileAsync("css/style.css");
				}
				else if (path == "/JavaScript/JavaScript.js" && request.Method == "GET")
				{
					response.ContentType = "text/javaScript; charset=utf-8";
					await response.SendFileAsync("JavaScript/JavaScript.js");
				}

				else
				{
					response.ContentType = "text/html; charset=utf-8";
					await response.SendFileAsync("html/index.html");

				}

			});
			app.Run();


		}
		private static async void DBUpdate()
		{
			while (true)
			{
				using (CaDb db = new())
				{
					await Task.Run(async() =>
					{
						List<Task> tasks = new();
						foreach (var ex in ExchangesManager.Exchanges)
						{
							tasks.Add(ex.UpdateProducts());
						}
						await Task.WhenAll(tasks.ToArray());
					});

					var exchanges = ExchangesManager.Exchanges;
					var allp = db.Products!;

					db.Products!.RemoveRange(allp);
					db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE SET SEQ = 0 WHERE NAME = 'Products'");

					db.Products!.AddRange(exchanges[0].Products);
					db.Products.AddRange(exchanges[1].Products);
					db.SaveChanges();

				}
			}
		}

		private static void Init()
		{
			ExchangesManager.Exchanges.Add(new Binance());
			ExchangesManager.Exchanges.Add(new Kucoin());

			using (CaDb db = new())
			{
				db.Database.EnsureDeleted();
				db.Database.EnsureCreated();

				var exchanges = ExchangesManager.Exchanges;

				db.Exchanges!.AddRange(exchanges);

				db.SaveChanges();

			}

		}
	}
}