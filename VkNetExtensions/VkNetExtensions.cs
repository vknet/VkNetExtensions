using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using VkNet.Abstractions;
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
	}
}
