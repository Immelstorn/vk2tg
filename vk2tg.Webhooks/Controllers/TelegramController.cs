using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using vk2tg.Data.Models;
using vk2tg.Data.Models.DB;
using vk2tg.Services;

namespace vk2tg.Webhooks.Controllers
{
    public class TelegramController: ApiController
    {
        private readonly TgService _tgService = new TgService();
        private readonly VkService _vkService = new VkService();
        private readonly DataService _dataService = new DataService();

        public string Get()
        {
            return "ok!";
        }

        public async Task Post([FromBody] Update update)
        {
            try
            {
                if(update.Message != null)
                {
                    await ProcessUpdate(update);
                }
            }
            catch(AggregateException a)
            {
                foreach(var exception in a.InnerExceptions)
                {
                    Trace.TraceError("================================================================");
                    Trace.TraceError(exception.Message);
                    Trace.TraceError(exception.StackTrace);
                    try
                    {
                        _dataService.AddErrorLog(exception);
                    }
                    catch (Exception) { }
                }
            }
            catch(Exception e)
            {
                Trace.TraceError("================================================================");
                Trace.TraceError(e.Message);
                Trace.TraceError(e.StackTrace);
                try
                {
                    _dataService.AddErrorLog(e);
                }
                catch (Exception) { }
            }
        }

        private async Task ProcessUpdate(Update update)
        {
            await _dataService.UpdateUserInfo(update.Message.Chat.Id, update.Message.Chat.Username, update.Message.Chat.FirstName, update.Message.Chat.LastName);

            var chatId = update.Message.Chat.Id;
            if(update.Message.Entities.Count(e => e.Type == MessageEntityType.BotCommand) == 1)
            {
                var index = update.Message.Entities.FindIndex(e => e.Type == MessageEntityType.BotCommand);
                if(index != -1)
                {
                    var command = update.Message.EntityValues[index];
                    switch(command)
                    {
                        case "/start":
                            await Start(update);
                            return;

                        case "/help":
                            await _tgService.SendMessage(chatId, Texts.Help); 
                            return;

                        case "/subscribe":
                            var name = GetGroupName(update.Message.Text);
                            await Subscribe(name, chatId);
                            return;

                        case "/list":
                            var list = await _dataService.GetUserSubscriptions(chatId);
                            var sb = new StringBuilder();
                            if (list.Any())
                            {
                                sb.AppendLine(Texts.YourSubscriptions);
                                foreach (var item in list)
                                {
                                    sb.AppendLine($"_{item}_");
                                }
                            }
                            else
                            {
                                sb.AppendLine(Texts.NoSubscriptions);
                            }

                            await _tgService.SendMessage(chatId, sb.ToString(), ParseMode.Markdown);
                            return;

                        case "/unsubscribe":
                            var groupName = GetGroupName(update.Message.Text);
                            await Unsubscribe(groupName, chatId);
                            return;
                    }
                }
            }

            await _tgService.SendMessage(update.Message.Chat.Id, Texts.PleaseUseOnlyOneCommand); 
        }

        private async Task Unsubscribe(string result, long chatId)
        {
            if (string.IsNullOrEmpty(result))
            {
                await _tgService.SendMessage(chatId, Texts.CommandFormatShouldBeNext); 
            }
            else
            {
                var group = await _vkService.GetGroupInfo(result);
                if (group == null)
                {
                    await _tgService.SendMessage(chatId, string.Format(Texts.GroupIsNotFound, result));
                    return;
                }
                var unsubscribed = await _dataService.RemoveSubscription(-group.gid, chatId);
                await _tgService.SendMessage(chatId,
                                             unsubscribed
                                                 ? string.Format(Texts.YouAreUnsubscribed, result)
                                                 : string.Format(Texts.YouAreNotSubscribed, result)
                                            );
            }
        }

        private async Task Subscribe(string result, long chatId)
        {
            if(string.IsNullOrEmpty(result))
            {
                await _tgService.SendMessage(chatId, Texts.CommandFormatShouldBeNext);
            }
            else
            {
                var group = await _vkService.GetGroupInfo(result); 
                if(group == null)
                {
                    await _tgService.SendMessage(chatId, string.Format(Texts.GroupIsNotFound, result)); 
                    return;
                }
                var post = (await _vkService.GetPosts(-group.gid, 0, 1)).FirstOrDefault();
                if(post?.id == null)
                {
                    await _tgService.SendMessage(chatId, Texts.AccessDenied);
                    return;
                }
                var subscribed = await _dataService.AddSubscription(-group.gid, group.screen_name, group.name, post.id, chatId);

                await _tgService.SendMessage(chatId,
                                             subscribed
                                                 ? string.Format(Texts.YouAreSubscribed, result)
                                                 : string.Format(Texts.YouAreAlreadySubscribed, result)
                                            ); 
            }
        }

        private string GetGroupName(string message)
        {
            var body = message.Replace("/subscribe", string.Empty).Replace("/unsubscribe", string.Empty).Trim();

            if (string.IsNullOrEmpty(body))
            {
                return null;
            }

            if (body.Any(char.IsWhiteSpace))
            {
                return null;
            }

            var lastIndex = body.LastIndexOf('/');
            if (lastIndex > -1)
            {
                if (lastIndex > (body.Length - 1))
                {
                    return null;
                }

                body = body.Substring(lastIndex + 1, body.Length - lastIndex - 1);
            }

            if(body.Any(c => !char.IsLetterOrDigit(c) && c != '.' && c != '_'))
            {
                return null;
            }

            return body;
        }

        private async Task Start(Update update)
        {
            if (await _dataService.UserExists(update.Message.Chat.Id))
            {
                await _tgService.SendMessage(update.Message.Chat.Id, Texts.YouHaveAlreadyStarted); 
            }
            else
            {
                await _dataService.RegisterUser(update.Message.Chat.Id, update.Message.Chat.Username);
                await _tgService.SendMessage(update.Message.Chat.Id, Texts.Welcome); 
            }
        }
    }
}