using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using VkNet.Abstractions;
using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Attachments;

namespace VkNetExtensions
{
	/// <summary>
	/// Методы расширения для VkNet
	/// </summary>
	public static class VkNetExtensions
	{
		/// <summary>
		/// С этого числа начинаются ID бесед
		/// </summary>
		public const long COUNT_CONVERSATION = 2000000000;

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

			using var client = new HttpClient();
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

		/// <summary>
		/// Получить реплай сообщение (если есть) или пересланные сообщения
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static IEnumerable<Message> GetForwardedMessages(this Message message)
		{
			if (message.ReplyMessage != null) return new[] {message.ReplyMessage};

			return message.ForwardedMessages;
		}

		/// <summary>
		/// Возвращает зрительный номер беседы (не настоящий) из реального или зрительного
		/// </summary>
		public static long GetVisualPeerId(this long peerId)
		{
			if (peerId < COUNT_CONVERSATION) return peerId;

			return peerId - COUNT_CONVERSATION;
		}

		/// <summary>
		/// Возвращает зрительный номер беседы (не настоящий) из реального или зрительного
		/// </summary>
		public static long? GetVisualPeerId(this long? peerId)
		{
			if (peerId == null) return null;

			return GetVisualPeerId(peerId.Value);

		}

		/// <summary>
		/// Возвращает реальный номер беседы из зрительного или реального
		/// </summary>
		public static long? GetRealPeerId(this long? peerId)
		{
			if (peerId == null) return null;

			return GetRealPeerId(peerId.Value);
		}

		/// <summary>
		/// Возвращает реальный номер беседы из зрительного или реального
		/// </summary>
		public static long GetRealPeerId(this long peerId)
		{
			if (peerId > COUNT_CONVERSATION) return peerId;

			return peerId + COUNT_CONVERSATION;
		}

		/// <summary>
		/// Возвращает текущее время с серверов ВК.
		/// Делает запрос лишь при первом использовании, в остальных случаях считается через закэшированный результат.
		/// IVkApi обязательно передавать только при первом использовании, экземпляр будет закэширован.
		/// Если нужно будет обновить время, используйте VkDate.UpdateVkDate(api).
		/// </summary>
		/// <param name="api"></param>
		/// <param name="timezone">Часовой пояс</param>
		/// <returns></returns>
		public static DateTime DateTime(IVkApi? api = null, int timezone = 0)
		{
			if (api == null)
			{
				api = VkDate.VkApi;

				if (api == null)
				{
					throw new Exception("IVkApi == null");
				}
			} else
			{
				VkDate.VkApi = api;
			}

			if (VkDate.VkDateObject == null) VkDate.UpdateVkDate(api);

			return VkDate.VkDateObject!.Get().AddHours(timezone);
		}

		/// <summary>
		///     Загружает документ на сервер ВК.
		/// </summary>
		/// <param name="vkApi"></param>
		/// <param name="data">Attachment, байты которого будут отправлены на сервер</param>
		/// <param name="docMessageType">Тип документа - документ или аудиосообщение</param>
		/// <param name="peerId">Идентификатор назначения</param>
		/// <param name="filename">Итоговое название документа</param>
		/// <returns>Attachment для отправки вместе с сообщением</returns>
		public static async Task<Attachment?> LoadDocumentToChatAsync(IVkApi vkApi, byte[] data,
																	DocMessageType docMessageType, long peerId, string filename)
		{
			var uploadServer = vkApi.Docs.GetMessagesUploadServer(peerId, docMessageType);
			var r = await AdditionalMethodsForExtensions.UploadFile(uploadServer.UploadUrl, data, filename);
			var documents = await vkApi.Docs.SaveAsync(r, filename, null);

			if (!documents.Any()) return null;

			return documents.First();
		}

		/// <summary>
		///     Загружает txt файл на сервер вк
		/// </summary>
		/// <param name="vkApi"></param>
		/// <param name="text">Текст в txt документе</param>
		/// <param name="peerId">Идентификатор назначения</param>
		/// <param name="filename">Итоговое название документа</param>
		/// <param name="txt">Подставлять ли тип .txt</param>
		/// <returns></returns>
		public static async Task<Attachment?> LoadTxtDocumentToChatAsync(IVkApi vkApi, string text, long peerId, string filename,
																		bool txt = true)
		{
			var data = AdditionalMethodsForExtensions.GetFileTxtByte(text);

			return await LoadDocumentToChatAsync(vkApi,
				data,
				DocMessageType.Doc,
				peerId,
				$"{filename}{(txt ? ".txt" : "")}");
		}

		/// <summary>
		/// Получает кликабельную ссылку на юзера или сообщество
		/// </summary>
		/// <param name="id">id пользователя или сообщества (у сообщества с минусом)</param>
		/// <param name="text">Текст ссылки</param>
		/// <returns></returns>
		public static string GetClickableLinkById(this long id, string text)
		{
			return $"[{(id > 0 ? $"id{id}" : $"club{-id}")}|{text.Replace("]", "&#93;")}]";
		}

		/// <summary>
		/// Возвращает из аргумента id пользователя или сообщества, если найдёт.
		/// </summary>
		/// <param name="api"></param>
		/// <param name="argument"></param>
		/// <returns>ID пользователя или сообщества, или null</returns>
		public static async Task<long?> GetUserOrCommunityIdFromArgument(IVkApi api, string argument)
		{
			try
			{
				if (argument.Contains("[id"))
				{
					var indexStart = argument.IndexOf('d') + 1;
					var indexEnd = argument.IndexOf('|');
					var id = argument.Substring(indexStart, indexEnd - indexStart);

					return Convert.ToInt64(id);
				} else if (argument.Contains("club"))
				{
					var indexStart = argument.IndexOf('b') + 1;
					var indexEnd = argument.IndexOf('|');
					var id = argument.Substring(indexStart, indexEnd - indexStart);

					return -Convert.ToInt64(id);
				}
			}
			catch
			{
				// ignored
			}

			if (argument.Contains("vk.com/"))
			{
				if (argument.Contains("vk.com/id") && long.TryParse(argument.Substring(argument.LastIndexOf('d') + 1), out var id3))
					return id3;

				var id = await GetIdForUserOrCommunitiesFromTextLink(api, argument);

				if (id != 0) return id;
			}

			return null;
		}

		/// <summary>
		/// Получает id пользователя или сообщества из ссылки подобной "vk.com/durov".
		/// Возвращает 0, если это ссылка не на пользователя или сообщество.
		/// </summary>
		/// <param name="api"></param>
		/// <param name="link">Например "vk.com/durov" или "https://vk.com/durov".</param>
		/// <returns>id пользователя или сообщества.</returns>
		public static async Task<long> GetIdForUserOrCommunitiesFromTextLink(IVkApi api, string link)
		{
			var id = await api.Utils.ResolveScreenNameAsync(link.Substring(link.LastIndexOf('/') + 1));

			if (id?.Id == null || id.Type == VkObjectType.Application) return 0;

			if (id.Type == VkObjectType.Group) return -id.Id.Value;

			return id.Id.Value;
		}
	}
}
