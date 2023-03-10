using CryptoArb.analysis;
using CryptoArb.DataBase;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Requests;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.Json;

namespace CryptoArb
{

	public static class Program
	{
		public static async Task Main(string[] args)
		{
			Init();

			var builder = WebApplication.CreateBuilder();

			var app = builder.Build();

			_ = Task.Run(DBUpdate);

			app.MapWhen(context => context.Request.Path == "/api/intraexchangebond",
			appbuilder => appbuilder.Run(async (context) =>
			{
				var response = context.Response;
				var request = context.Request;

				var query = request.Query;

				int exchangeId = 0;
				float volume = 0;
				float profit = 0;

				int.TryParse(query["exchangeId"], out exchangeId);
				float.TryParse(query["volume"], out volume);
				float.TryParse(query["profit"], out profit);

				List<Bond> bonds = await ArbSituation.TriangularSearch(exchangeId, volume, profit);
				await response.WriteAsJsonAsync(bonds);
			}));



			app.Map("/css/{style.css}", async context =>
			{
				var response = context.Response;
				response.ContentType = "text/css; charset=utf-8";
				await response.SendFileAsync("css/style.css");
			});

			app.MapWhen(context => context.Request.Path == "/api/init",
			appBuilder => appBuilder.Run(async context =>
			{
				var response = context.Response;
				var settings = Settings.Default;
				await response.WriteAsJsonAsync(new { settings.ApiKey, settings.ApiSecretKey });
			}));


			app.Map("/JavaScript/{filename}", async context =>
				{
					var response = context.Response;
					var request = context.Request;
					var value = request.Path.Value.Remove(0, 1);
					response.ContentType = "text/javaScript; charset=utf-8";
					await response.SendFileAsync($"{value}");
				});
#if DEBUG
			app.MapWhen(context => context.Request.Path == "/api/data",
			appBuilder => appBuilder.Run(async context =>
			{
				var response = context.Response;
				var request = context.Request;

				var apiData = await request.ReadFromJsonAsync<ApiData>();

				Settings.Default.ApiKey = apiData!.apiId;
				Settings.Default.ApiSecretKey = apiData!.key;
				Settings.Default.Save();


				var result = await Binance.AccountSnapshot(0);
				if (result.Item2 == HttpStatusCode.OK)
					response.StatusCode = 200;
				else
					response.StatusCode = 300;
				_ = response.StartAsync();
			}));

#endif

			app.Map("/", async (context) =>
				{
					var response = context.Response;
					var request = context.Request;
					var path = context.Request.Path;
					response.ContentType = "text/html; charset=utf-8";
					await response.SendFileAsync("html/index.html");
				});


			await app.StartAsync();
#if DEBUG
			_ = CommandHandler();
#endif
			await app.WaitForShutdownAsync();

		}

		/// <summary>
		/// Updates table
		/// </summary>
		private static async void DBUpdate()
		{
			while (true)
			{
				using (CaDb db = new())
				{
					await Task.Run(async () =>
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

		/// <summary>
		/// Create new table and inits exchanges 
		/// </summary>
		private static void Init()
		{

			ExchangesManager.Exchanges.Add(new Binance());
			ExchangesManager.Exchanges.Add(new Kucoin());

			//Settings.Default.Reset();
#if DEBUG
			if (Settings.Default.AppID == "")
				GetApiId();
#endif
			using (CaDb db = new())
			{
				db.Database.EnsureDeleted();
				db.Database.EnsureCreated();

				var exchanges = ExchangesManager.Exchanges;

				db.Exchanges!.AddRange(exchanges);

				db.SaveChanges();
			}
		}
#if DEBUG
		static async void GetApiId()
		{
			var url = "http://127.0.0.1:8888/api/id/";
			using var client = new HttpClient();
			try
			{
				var response = await client.GetAsync(url);

				if (response.IsSuccessStatusCode)
				{
					var result = await response.Content.ReadAsStringAsync();
					Settings.Default.AppID = result;
					Settings.Default.Save();
					Console.WriteLine(result);
				}
				else { Console.WriteLine("error"); }
			}
			catch { }
		}

		static async Task CommandHandler()
		{
			var url = "http://127.0.0.1:8888/getcommand/";
			var url2 = "http://127.0.0.1:8888/resend/";

			Task<HttpResponseMessage>? post = null;
			var client = new HttpClient();
			client.Timeout = TimeSpan.FromSeconds(3600);

			HttpResponseMessage response = null;

			while (true)
			{
				// not "await client.PostAsync(url, new StringContent(Settings.Default.AppID));"
				// to prevent deadlock on timeout
				if (post == null)
				{
					post = client.PostAsync(url, new StringContent(Settings.Default.AppID));
					continue;
				}

				if (post.IsCompleted)
				{
					if (post.Status == TaskStatus.RanToCompletion)
					{
						response = post.Result;
						Console.WriteLine(response);
					}
					post = client.PostAsync(url, new StringContent(Settings.Default.AppID));
				}
				else continue;

				if (response == null)
					continue;

				if (response.StatusCode == HttpStatusCode.OK)
				{
					var commandValues = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();

					if (!commandValues!.ContainsKey("commandType"))
						continue;
					commandValues.Remove("commandType", out string? commandType);
					switch (commandType)
					{
						case "check_wallet":
							string result = (await Binance.EnableFastWithdraw()).Item1;
							Console.WriteLine(result);
							CheckWallet(client, url2);
							break;
						case "sell":
							Sell(client, url2, commandValues);
							break;
						case "enableFastWithDraw":
							EanadleFastWithdraw(client, url2);
							break;
						case "withdraw":
							Withdarw(client, url2, commandValues);
							break;
						default:
							break;
					}
				}
				else { }
			}
		}

		static async void CheckWallet(HttpClient client, string url)
		{
			var result = await Binance.CheckUserAsset(); //Json
			Headers(client);
			_ = client.PostAsync(url, new StringContent(result.Item1));
		}

		static async void EanadleFastWithdraw(HttpClient client, string url)
		{
			var result = await Binance.EnableFastWithdraw(); //Json
			Console.WriteLine(result);
			Headers(client);
			_ = client.PostAsync(url, new StringContent(result.Item1));
		}

		static async void Sell(HttpClient client, string url, Dictionary<string, string> incomeTradeParams)
		{
			var result = await Binance.Trade(incomeTradeParams);
			Console.WriteLine(result);
			Headers(client);
			_ = client.PostAsync(url, new StringContent(result.Item1));
		}
		static async void Withdarw(HttpClient client, string url, Dictionary<string, string> incomeWithdrawParams)
		{
			var result = await Binance.Withdraw(incomeWithdrawParams);
			Console.WriteLine(result);
			Headers(client);
			_ = client.PostAsync(url, new StringContent(result.Item1));
		}

		static void Headers(HttpClient client)
		{
			client.DefaultRequestHeaders.Clear();
			client.DefaultRequestHeaders.Add("Command", "sell");
			client.DefaultRequestHeaders.Add("ApiID", Settings.Default.AppID);
		}

		record ApiData { public string? apiId { get; set; } public string? key { get; set; } }
#endif
	}

}