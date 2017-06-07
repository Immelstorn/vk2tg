using System.Configuration;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace vk2tg.Services
{
    public class TgService
    {
        private readonly TelegramBotClient _bot = new TelegramBotClient(ConfigurationManager.AppSettings["TelegramToken"]);

        public async Task<Message> SendMessage(long chatId, string message, ParseMode parseMode=ParseMode.Default)
        {
            return await _bot.SendTextMessageAsync(chatId, message, parseMode: parseMode);
        }
    }
}