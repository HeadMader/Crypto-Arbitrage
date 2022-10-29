using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Spot;
using CryptoExchange.Net.CommonObjects;


namespace CryptoArb
{
	public abstract class Exchange
	{
		[Key]
		public abstract int ID { get; set; }
		public abstract string Name { get; set; }

		public abstract List<Product> Products { get; set; }

		public async Task UpdateProducts()
		{
			await ProcessProducts();
		}
		protected abstract Task ProcessProducts();
		protected abstract Product ToProduct<P, A>(P productExchangeObj, A productExchangeObj2);
	}
}