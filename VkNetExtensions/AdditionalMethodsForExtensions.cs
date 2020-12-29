using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

		/// <summary>
		///     Загружает массив байт на указанный url
		/// </summary>
		/// <param name="url">Адрес для загрузки</param>
		/// <param name="data">Массив данных для загрузки</param>
		/// <returns>Строка, которую вернул сервер.</returns>
		public static async Task<string> UploadFile(string url, byte[] data, string filename)
		{
			var index = filename.LastIndexOf('.') + 1;
			var format = filename.Substring(index, filename.Length - index);
			using var client = new HttpClient();

			throw new NotImplementedException();

			// todo расскомментить и реализовать без flurl
			// return await url.PostMultipartAsync
			// (
			// 	mp => mp.AddFile("file", new MemoryStream(data), $"file.{format}")
			// ).ReceiveString();
		}
	}
}
