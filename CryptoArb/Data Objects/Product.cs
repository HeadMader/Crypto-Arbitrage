using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoArb
{
	public class Product
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ID { get; set; }
		public string? USymbol { get; set; }
		public string? Symbol { get; set; }
		public string? Status { get; set; }
		public double Volume { get; set; }
		public double Liquidity { get; set; }
		public double LastPrice { get; set; }
		public string? baseAsset { get; set; }
		public string? baseAlt { get; set; }
		public string? quoteAsset { get; set; }
		public string? quoteAlt { get; set; }

		public int ExchangeId { get; set; }
		//[JsonProperty("time"), JsonConverter(typeof(DateTimeConverter))]
	}
}
