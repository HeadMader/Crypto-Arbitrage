namespace CryptoArb
{
	public class Bond
	{
		public int ID { get; set; }	
		public BondType BondType { get; set; }
		public string? USymbol { get; set; }
		public string? BaseProduct { get; set; }
		public float Profit { get; set; }
		public double Volume { get; set; }
		public double VolumeUSD { get; set; }
		public int ExchangeId { get; set; }
		public List<string> Sequencing { get; set; } = null!;

		public Bond(int id, BondType bondType, string? uSymbol, string? baseProduct, float profit, double volume, double volumeUSD, int exchangeId, List<string> sequencing)
		{
			ID = id;
			BondType = bondType;
			USymbol = uSymbol;
			BaseProduct = baseProduct;
			Profit = profit;
			Volume = volume;
			VolumeUSD = volumeUSD;
			ExchangeId = exchangeId;
			Sequencing = sequencing;
		}
	}
	public enum BondType { IntraTringle, InterTriengle}
}
