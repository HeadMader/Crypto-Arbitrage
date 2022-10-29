using System.Security.Cryptography;
using System.Text;

namespace CryptoArb
{
	public static class Cryptography
	{
		public static string HashHMACSHA256(string key, string text)
		{
			var keyBite = Encoding.UTF8.GetBytes(key);
			var textBite = Encoding.UTF8.GetBytes(text);
			var hash = new HMACSHA256(keyBite);

			return BitConverter.ToString(hash.ComputeHash(textBite)).Replace("-", "").ToLower();
		}
	}
}
