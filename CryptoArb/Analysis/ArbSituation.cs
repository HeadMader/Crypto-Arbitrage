﻿using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;

namespace CryptoArb.analysis
{
	public static class ArbSituation
	{

		public async static Task<List<Bond>> TriangularSearch(int? exchangeId = 0, float? volumeFilterUSD = 0, float? profitFilter = 0)
		{
			List<Bond> bonds = new();
			List<Exchange> exchanges = new();

			using (var db = new CaDb())
			{
				exchanges = db.Exchanges!.Include(e => e.Products).
				Where(e => e.ID == exchangeId).ToList() ??
				db.Exchanges!.Include(e => e.Products).ToList();
			}

			foreach (Exchange ex in exchanges)
			{
				Stopwatch stopwatch = new Stopwatch();

				stopwatch.Start();
				int id = 0;
				foreach (Product product in ex.Products)
				{
					double currentPrice = product.LastPrice;
					var baseAsset = product.baseAsset;
					var quoteAsset = product.quoteAsset;

					//get products that have as base asset base asset of the product
					List<Product> baseAssetProducts = ex.Products.
						Where(c => c.baseAsset == baseAsset &&
						c.quoteAsset != quoteAsset).ToList();

					//get products that have as base asset quote asset of the product
					List<Product> quoteAssetProducts = ex.Products.
						Where(c => c.baseAsset == quoteAsset &&
						c.quoteAsset != baseAsset).ToList();

					double exRate = product.quoteAsset!.Contains("USD") ? product.LastPrice :
						baseAssetProducts.FirstOrDefault(p => p.quoteAsset!.Contains("USD"),
						new Product())!.LastPrice;

					double productVolumeUSD = product.Volume * exRate;

					foreach (Product baseAssetProduct in baseAssetProducts)
					{
						Product? quoteAssetProduct = quoteAssetProducts.FirstOrDefault(q => q.quoteAsset == baseAssetProduct.quoteAsset);

						if (quoteAssetProduct == null)
							continue;

						double baseProductPrice = baseAssetProduct.LastPrice;
						double quoteProductPrice = quoteAssetProduct.LastPrice;
						double priceRetetiveToOtherCurrency = baseProductPrice / quoteProductPrice;

						double profit;
						double result = 1 - priceRetetiveToOtherCurrency / currentPrice;

						profit = Math.Abs(result) * 100;

						if (profit > (profitFilter ?? 0))
						{
							if (productVolumeUSD > (volumeFilterUSD ?? 0))
							{
								List<string> sequencing = new() {
								quoteAssetProduct.quoteAsset!,
								result > 0 ? baseAsset! : quoteAsset!,
								result > 0 ? quoteAsset! : baseAsset!,
								quoteAssetProduct.quoteAsset!
								};
								bonds.Add(new Bond(
									id,
									BondType.IntraTringle,
									product.USymbol,
									baseAssetProduct.USymbol,
									(float)profit,
									product.Volume,
									Math.Round(productVolumeUSD, 0),
									ex.ID,
									sequencing
									));
								id++;
							}
						}
					}
				}
				stopwatch.Stop();
				Console.WriteLine(stopwatch.ElapsedMilliseconds);

			}
			return bonds;
		}

	//	public async static Task<List<Bond>> SortTrBond(List<Bond> bonds, int symbol, int profit, int volume, int baseAsset,int quoteAsset) 
	//	{ 
		
	//	} 
	}
}
