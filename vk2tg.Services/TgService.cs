using System;
using System.Configuration;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace vk2tg.Services
{
    public class TgService
    {
        private readonly TelegramBotClient _bot = new TelegramBotClient(ConfigurationManager.AppSettings["TelegramToken"]);

        public async Task<Message> SendMessage(long chatId, string message, ParseMode parseMode=ParseMode.Default)
        {
            return await _bot.SendTextMessageAsync(chatId, message, parseMode: parseMode);
        }

        public async Task<Message> AnswerBitcoinCallback(string callback, long chatId)
        {
            await _bot.AnswerCallbackQueryAsync(callback);
            await _bot.SendPhotoAsync(chatId, new FileToSend(new Uri("https://i.imgur.com/3IhYeZ4.jpg")));
            return await _bot.SendTextMessageAsync(chatId, "Bitcoin wallet:\n16piobF1NcTgbU7umnsvHQvdF6qUM1g2dq");
        }

        public async Task<Message> SendMessage(long chatId, string message, InlineKeyboardMarkup markup)
        {
            return await _bot.SendTextMessageAsync(chatId, message, replyMarkup: markup);
        }
    }
}