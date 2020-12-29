using System.Text;

namespace VkNetExtensions
{
	/// <summary>
	/// В этом файле нет методов расширения для VkNet, но сами методы расширения используют методы отсюда.
	/// </summary>
	public static class AdditionalMethodsForExtensions
	{
		/// <summary>
		/// Получить строку в байтах
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static byte[] GetFileTxtByte(string text)
		{
			return new UTF8Encoding(true).GetBytes(text);
		}
	}
}
