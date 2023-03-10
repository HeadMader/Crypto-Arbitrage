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
		/// <summary>
		/// Exchange ID
		/// </summary>
		[Key]
		public abstract int ID { get; set; }

		/// <summary>
		/// Exchange name
		/// </summary>
		public abstract string Name { get; set; }

		/// <summary>
		/// List of products
		/// </summary>
		public abstract List<Product> Products { get; set; }

		/// <summary>
		/// Update exchange products
		/// </summary>
		public async Task UpdateProducts()
		{
			await ProcessProducts();
		}

		/// <summary>
		/// Get products from exchange
		/// </summary>
		protected abstract Task ProcessProducts();

		/// <summary>
		/// Converts product data from exchange to <see cref="Product"></see>
		/// </summary>
		/// <typeparam name="P"></typeparam>
		/// <typeparam name="A"></typeparam>
		/// <param name="productExchangeObj"></param>
		/// <param name="productExchangeObj2"></param>
		/// <returns><see cref="Product"></see></returns>
		protected abstract Product ToProduct<P, A>(P productExchangeObj, A productExchangeObj2);
	}
}