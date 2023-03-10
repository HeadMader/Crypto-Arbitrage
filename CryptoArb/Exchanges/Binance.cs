using Binance.Net.Clients;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Spot;
using Newtonsoft.Json.Linq;
using System.Net;

namespace CryptoArb
{
	public class Binance : Exchange
	{
		public override int ID { get; set; } = 1;

		public override string Name { get; set; } = "Binance";

		public override List<Product> Products { get; set; } = new();

		BinanceClient client = new();

		static string key = Settings.Default.ApiKey;
		static string apikey = Settings.Default.ApiSecretKey;

		#region account control methods
#if DEBUG
		public static async Task<(string, HttpStatusCode)> CheckWallet()
		{
			var values = new Dictionary<string, string>
				{
					{ "recvWindow", "5000" },
				};

			Dictionary<string, string> headers = new()
				{
					{"X-MBX-APIKEY",key}
				};

			var response = await Request("https://api.binance.com/sapi/v1/capital/config/getall", "GET", BinanceValues(values), headers);

			return (await response.Content.ReadAsStringAsync(), response.StatusCode);
		}

		public static async Task<(string, HttpStatusCode)> CheckApiKey()
		{
			var values = new Dictionary<string, string>
				{
					{ "recvWindow", "5000" },
				};

			Dictionary<string, string> headers = new()
				{
					{"X-MBX-APIKEY",key}
				};

			var response = await Request("https://api.binance.com/sapi/v1/account/apiTradingStatus", "GET", BinanceValues(values), headers);

			return (await response.Content.ReadAsStringAsync(), response.StatusCode);

		}

		public static async Task<(string, HttpStatusCode)> CheckUserAsset()
		{
			var values = new Dictionary<string, string>
				{
					{ "recvWindow", "5000" },
				};

			Dictionary<string, string> headers = new()
				{
					{"X-MBX-APIKEY",key}
				};

			var response = await Request("https://api.binance.com/sapi/v3/asset/getUserAsset", "POST", BinanceValues(values), headers);

			return (await response.Content.ReadAsStringAsync(), response.StatusCode);

		}

		public static async Task<(string, HttpStatusCode)> AccountSnapshot(double days)
		{
			var timeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			var startTime = timeStamp - TimeSpan.FromDays(days).TotalMilliseconds;

			var values = new Dictionary<string, string>
				{
					{ "type", "SPOT" },
					{ "recvWindow", "5000" },
					{ "startTime",$"{startTime}"},
				};

			Dictionary<string, string> headers = new()
				{
					{"X-MBX-APIKEY",key}
				};

			var response = await Request("https://api.binance.com/sapi/v1/accountSnapshot", "GET", BinanceValues(values), headers);

			return (await response.Content.ReadAsStringAsync(), response.StatusCode);

		}

		public static async Task<(string, HttpStatusCode)> EnableFastWithdraw()
		{
			Dictionary<string, string> headers = new()
				{
					{"X-MBX-APIKEY",key}
				};

			var response = await Request("https://api.binance.com/sapi/v1/account/enableFastWithdrawSwitch", "POST", BinanceValues(null), headers);

			return (await response.Content.ReadAsStringAsync(), response.StatusCode);
		}

		public static async Task<(string, HttpStatusCode)> Withdraw(Dictionary<string, string> withdrawData)
		{
			var values = new Dictionary<string, string>
				{
					{ "coin", "USDT" },
					{ "address", "0xf587ed4f5091156e4251dd466358117a49a3539c" },
					{ "amount", "1"},
				};

			Dictionary<string, string> headers = new()
				{
					{"X-MBX-APIKEY",key}
				};

			var response = await Request("https://api.binance.com/sapi/v1/capital/withdraw/apply", "POST", BinanceValues(values), headers);

			return (await response.Content.ReadAsStringAsync(), response.StatusCode);

		}

		public static async Task<(string, HttpStatusCode)> Trade(Dictionary<string, string> tradeData)
		{
			Dictionary<string, string> headers = new()
				{
					{"X-MBX-APIKEY",key}
				};

			var response = await Request("https://api.binance.com/api/v3/order", "POST", BinanceValues(tradeData), headers);

			return (await response.Content.ReadAsStringAsync(), response.StatusCode);
		}

		/// <summary>
		/// Create vary simple requests
		/// </summary>
		/// <param name="Url"></param>
		/// <param name="httpMethod"></param>
		/// <param name="values"></param>
		/// <param name="headers"></param>
		/// <returns><see cref="HttpResponseMessage"/></returns>
		private async static Task<HttpResponseMessage> Request(string Url, string httpMethod,
			IEnumerable<KeyValuePair<string, string>>? values = null,
			IEnumerable<KeyValuePair<string, string>>? headers = null)
		{
			using (var httpClient = new HttpClient())
			{
				httpClient.BaseAddress = new Uri(Url);
				HttpResponseMessage response = null!;
				HttpContent content = null!;


				if (headers != null)
					foreach (var header in headers)
					{
						httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
					}

				if (values != null)
				{
					content = new FormUrlEncodedContent(values);
				}


				switch (httpMethod)
				{
					case "POST":
						response = await httpClient.PostAsync(Url, content);
						break;
					case "GET":
						var s = "";
						if (content != null)
							s = "?" + content.ReadAsStringAsync().Result;
						response = await httpClient.GetAsync(s);
						break;
				}
				return response;
			}
		}

		/// <summary>
		/// Adds specific values that Binance needs for request
		/// </summary>
		/// <param name="values">your values for request</param>
		/// <returns><see cref="IEnumerable{KeyValuePair}"/> with Binance specific values</returns>
		private static IEnumerable<KeyValuePair<string, string>> BinanceValues(IEnumerable<KeyValuePair<string, string>>? values)
		{
			var timeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			var baseAndUserValues = new Dictionary<string, string>();

			var baseValues = new Dictionary<string, string>
				{
					{ "timestamp", $"{timeStamp}" },
			};

			if (values != null)
				baseAndUserValues = values.Concat(baseValues).ToDictionary(x => x.Key, x => x.Value);
			else baseAndUserValues = baseValues;

			var query = string.Join("&", baseAndUserValues.Select(kvp => $"{kvp.Key}={kvp.Value}"));

			var signature = Cryptography.HashHMACSHA256(apikey, query!);
			baseAndUserValues.Add("signature", signature);

			return baseAndUserValues;
		}
#endif
		#endregion



		protected override async Task ProcessProducts()
		{
			var tickersTask = client.SpotApi.ExchangeData.GetTickersAsync();
			var symbolsTask = client.SpotApi.ExchangeData.GetExchangeInfoAsync();

			await Task.WhenAll(tickersTask, symbolsTask);

			var tickers = tickersTask.Result;
			var symbols = symbolsTask.Result;
			if (tickers.Success && symbols.Success)
			{
				Products.Clear();

				var tickersData = tickers.Data.ToArray();
				var symbolsData = symbols.Data.Symbols.ToArray();

				for (int i = 0; i < tickersData.Count(); i++)
				{
					var ticker = tickersData[i];
					var symbol = symbolsData.FirstOrDefault((p) => p.Name == ticker.Symbol);
					if (symbol != null)
					{
						var product = ToProduct(ticker, symbol);

						if (product.Status == "Trading")
						{
							Products.Add(product);
						}
					}
				}
			}
		}

		/// <summary>
		/// Converts product data from exchange to <see cref="Product"></see>
		/// </summary>
		/// <typeparam name="P"></typeparam>
		/// <typeparam name="A"></typeparam>
		/// <param name="tickerInfo">ticker info of symbol</param>
		/// <param name="symbolInfo">symbol info</param>
		/// <returns>converted symbol</returns>
		/// <exception cref="Exception"></exception>
		protected override Product ToProduct<P, A>(P tickerInfo, A symbolInfo)
		{
			if (tickerInfo is IBinanceTick tick && symbolInfo is BinanceSymbol symbol)
			{
				Product product = new();
				product.Symbol = tick.Symbol;
				product.USymbol = tick.Symbol; // ID is like this BTCUSDT not BTC-USDT 
				product.ExchangeId = ID;
				product.LastPrice = (double)tick.LastPrice;
				product.Volume = (double)tick.Volume;
				product.baseAsset = symbol.BaseAsset;
				product.quoteAsset = symbol.QuoteAsset;
				product.Status = symbol.Status.ToString();
				return product;
			}
			else
			{
				throw new Exception("Not valid type");
			}
		}
	}
}