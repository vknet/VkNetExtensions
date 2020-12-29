using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using VkNet.Abstractions;
using VkNet.Enums.Filters;

namespace VkNetExtensions
{
	/// <summary>
	/// Методы расширения для VkNet
	/// </summary>
	public static class VkNetExtensions
	{
		/// <summary>
		/// Получить дату регистрации пользователя или дату создания сообщества
		/// </summary>
		/// <param name="api"></param>
		/// <param name="id">ID пользователя или сообщества (у сообщества с минусом)</param>
		/// <returns></returns>
		public static async Task<DateTime?> GetRegistrationDateAsync(this IVkApi api, long id)
		{
			if (id < 0)
			{
				var info = (await api.Groups.GetByIdAsync(null, (-id).ToString(), GroupsFields.StartDate)).First();
				return info.StartDate.GetValueOrDefault();
			}

			var client = new HttpClient();
			var str = await client.GetStringAsync($"https://vk.com/foaf.php?id={id}");
			var doc = new HtmlDocument();
			doc.LoadHtml(str);
			try
			{
				var created = doc.DocumentNode.Descendants("ya:created").ToArray()[0];
				var dataStr = created.Attributes["dc:date"].Value;

				return Convert.ToDateTime(dataStr);
			}
			catch
			{
				return null;
			}
		}
	}
}
