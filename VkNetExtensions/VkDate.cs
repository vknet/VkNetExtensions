using System;
using VkNet.Abstractions;

namespace VkNetExtensions
{
	/// <summary>
	/// Класс для работы с закэшированным временем, полученным от сервера ВКонтакте
	/// </summary>
	public class VkDate
	{
		/// <summary>
		/// Последнее полученное время
		/// </summary>
		public static VkDate? VkDateObject;

		/// <summary>
		/// Закэшированный экземпляр VkApi
		/// </summary>
		public static IVkApi? VkApi;

		/// <summary>
		/// Время в ВК при запросе
		/// </summary>
		public DateTime? VkDateTime { get; }

		/// <summary>
		/// Локальное время, которое было при запросе
		/// </summary>
		public DateTime VkDateTimeLocal { get; }

		public VkDate(DateTime vkDateTime, DateTime? vkDateTimeLocal = null)
		{
			VkDateTime = vkDateTime;
			VkDateTimeLocal = vkDateTimeLocal ?? DateTime.UtcNow;
		}

		/// <summary>
		/// Текущее время в ВК в UTC
		/// </summary>
		/// <returns>время в UTC</returns>
		public DateTime Get()
		{
			return VkDateTime!.Value.Add(DateTime.UtcNow - VkDateTimeLocal);
		}

		/// <summary>
		/// Получает время с ВК и кэширует
		/// </summary>
		/// <param name="api"></param>
		public static void UpdateVkDate(IVkApi api)
		{
			VkDateObject = new VkDate(api.Utils.GetServerTime());
		}
	}
}
