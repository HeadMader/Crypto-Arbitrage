using Binance.Net.Clients;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Spot;
using CryptoArb.analysis;
using CryptoExchange.Net.CommonObjects;
using Kucoin.Net.Clients;
using Kucoin.Net.Objects.Models.Spot;
using Newtonsoft.Json;

namespace CryptoArb
{
	public class Kucoin : Exchange
	{
		public override int ID { get; set; } = 2;

		public override string Name { get; set; } = "Kucoin";

		public override List<Product> Products { get; set; } = new();

		KucoinClient client;
		public Kucoin()
		{
			client = new();
		}


		protected override async Task ProcessProducts()
		{
			var tickersTask = client.SpotApi.ExchangeData.GetTickersAsync();
			var symbolsTask = client.SpotApi.ExchangeData.GetSymbolsAsync();

			await Task.WhenAll(tickersTask, symbolsTask);

			var tickers = tickersTask.Result;
			var symbols = symbolsTask.Result;

			if (tickers.Success && symbols.Success)
			{
				Products.Clear();

				var tickersData = tickers.Data.Data.ToArray();
				var symbolsData = symbols.Data.ToArray();

				foreach (var ticker in tickersData)
				{
					var symbolName = ticker.Symbol;
					var symbol = symbolsData.FirstOrDefault((p) => p.Symbol == symbolName);
					if (symbol != null)
					{
						var product = ToProduct(ticker, symbol);
						//product.Save();
						if (product.Status == "Trading")
							Products.Add(product);
					}
				}
			}
		}

		protected override Product ToProduct<P, A>(P tickerInfo, A symbolInfo)
		{
			if (tickerInfo is KucoinAllTick tick && symbolInfo is KucoinSymbol symbol)
			{
				Product product = new();
				product.Symbol = tick.Symbol;
				product.USymbol = (tick.Symbol).Replace("-", ""); // ID is like this BTCUSDT not BTC-USDT 
				product.ExchangeId = ID;
				product.LastPrice = (double)(tick.LastPrice ?? 0);
				product.Volume = (double)(tick.Volume ?? 0);
				product.baseAsset = symbol.BaseAsset;
				product.quoteAsset = symbol.QuoteAsset;
				product.Status = symbol.EnableTrading ? "Trading" : "Break";
				return product;
			}
			else
			{

				throw new Exception("Not valid type");
			}
		}

	}

}
