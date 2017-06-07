using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vk2tg.Services;

namespace vk2tg.WebJob
{
    class Program
    {
        private static readonly DataService _dataService = new DataService();
        private static readonly VkService _vkService = new VkService();
        private static readonly TelegraphService _telegraphService = new TelegraphService();
        private static readonly TgService _tgService = new TgService();

        static void Main(string[] args)
        {
            try
            {
                Task.Run(AsyncMain).GetAwaiter().GetResult();
            }
            catch (AggregateException a)
            {
                foreach (var exception in a.InnerExceptions)
                {
                    Trace.TraceError("================================================================");
                    Trace.TraceError(exception.Message);
                    Trace.TraceError(exception.StackTrace);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("================================================================");
                Trace.TraceError(e.Message);
                Trace.TraceError(e.StackTrace);
            }
        }

        private static async Task AsyncMain()
        {
            var subscriptions = await _dataService.GetSubscriptionsToCheck();
            foreach (var subscription in subscriptions)
            {
                var posts = _vkService.GetPosts(subscription.SubscriptionId, subscription.LastPostId);

                foreach (var post in posts)
                {
                    var link = _telegraphService.CreatePage(post, subscription.SubscriptionName);
                    foreach (var user in subscription.Users)
                    {
                        await _tgService.SendMessage(user.ChatId, link);
                    }

                    await _dataService.SetLastPost(subscription.SubscriptionId, post.id);
                }
            }
        }
    }
}
