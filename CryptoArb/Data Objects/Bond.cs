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
		public List<string> Sequencing { get; set; }
	}
	public enum BondType { IntraTringle, InterTriengle}
}
