using Binance.Net.Clients;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Spot;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net;
using System.Net;
using Binance.Net.Objects;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.Xml;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace CryptoArb
{
	public class Binance : Exchange
	{
		public override int ID { get; set; } = 1;

		public override string Name { get; set; } = "Binance";

		public override List<Product> Products { get; set; } = new();

		BinanceClient client = new();

		public async Task CheckWallet()
		{
			var key = "XfPsqP0FLyTR2TbNpfWdczxSU6DueoT6ryn0K0dRc9yV0ilgsBwBEUgrcat9OmGo";
			var apikey = "oEBgQ1I4e6IT2nNpCqIc4aQNItgWLYvn3GSh2ExUg5MW9K4QFqMFfUzzx2erxian";

			var timeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

			var values = new Dictionary<string, string>
				{
					{ "recvWindow", "5000" },
					{ "timestamp", $"{timeStamp}" },
				};

			var query = await new FormUrlEncodedContent(values).ReadAsStringAsync();

			var signature = Cryptography.HashHMACSHA256(key, query);

			values.Add("signature", signature);

			Dictionary<string, string> headers = new()
				{
					{"X-MBX-APIKEY",apikey}
				};
			var content = await Request("https://api.binance.com/sapi/v1/capital/config/getall", "GET", values, headers);

			Console.WriteLine(await content.ReadAsStringAsync());

		}
		public async Task AccountSnapshot()
		{
			var key = "XfPsqP0FLyTR2TbNpfWdczxSU6DueoT6ryn0K0dRc9yV0ilgsBwBEUgrcat9OmGo";
			var apikey = "oEBgQ1I4e6IT2nNpCqIc4aQNItgWLYvn3GSh2ExUg5MW9K4QFqMFfUzzx2erxian";

			var timeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

			var values = new Dictionary<string, string>
				{
					{ "type", "SPOT" },
					{ "startTime","1666137599000"},
					{ "recvWindow", "5000" },
					{ "timestamp", $"{timeStamp}" },
				};

			var query = await new FormUrlEncodedContent(values).ReadAsStringAsync();

			var signature = Cryptography.HashHMACSHA256(key, query);

			values.Add("signature", signature);

			Dictionary<string, string> headers = new()
				{
					{"X-MBX-APIKEY",apikey}
				};
			var content = await Request("https://api.binance.com/sapi/v1/accountSnapshot", "GET", values, headers);

			Console.WriteLine(await content.ReadAsStringAsync());

		}
		public async Task Wi()
		{
			var key = "XfPsqP0FLyTR2TbNpfWdczxSU6DueoT6ryn0K0dRc9yV0ilgsBwBEUgrcat9OmGo";
			var apikey = "oEBgQ1I4e6IT2nNpCqIc4aQNItgWLYvn3GSh2ExUg5MW9K4QFqMFfUzzx2erxian";

			var timeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

			var values = new Dictionary<string, string>
				{
					{ "type", "SPOT" },
					{ "startTime","1666137599000"},
					{ "recvWindow", "5000" },
					{ "timestamp", $"{timeStamp}" },
				};

			var query = await new FormUrlEncodedContent(values).ReadAsStringAsync();

			var signature = Cryptography.HashHMACSHA256(key, query);

			values.Add("signature", signature);

			Dictionary<string, string> headers = new()
				{
					{"X-MBX-APIKEY",apikey}
				};
			var content = await Request("https://api.binance.com/sapi/v1/accountSnapshot", "GET", values, headers);

			Console.WriteLine(await content.ReadAsStringAsync());

		}

		public async Task<HttpContent> Request(string Url, string httpMethod,
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
					content = new FormUrlEncodedContent(values);

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
				return response.Content;
			}
		}

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

