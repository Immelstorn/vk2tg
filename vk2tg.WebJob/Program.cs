using System;
using System.Diagnostics;
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
                    try
                    {
                        _dataService.AddErrorLog(exception);
                    }
                    catch(Exception) { }
                }
            }
            catch (Exception e)
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

        private static async Task AsyncMain()
        {
            _dataService.AddTraceLog("Sync started");
            var subscriptions = await _dataService.GetSubscriptionsToCheck();
            _dataService.AddTraceLog("Subscriptions: " + subscriptions.Count);

            foreach (var subscription in subscriptions)
            {
                _dataService.AddTraceLog("Processing subscription: " + subscription.SubscriptionName);

                var posts = _vkService.GetPosts(subscription.SubscriptionId, subscription.LastPostId);
                _dataService.AddTraceLog("Posts: " + posts.Count);

                foreach (var post in posts)
                {
                    _dataService.AddTraceLog("Processing post: " + post.id);
                    var link = _telegraphService.CreatePage(post, subscription.SubscriptionName, subscription.SubscriptionPrettyName ?? subscription.SubscriptionName);
                    await _dataService.AddLog(subscription.Id, post.id, link);
                    _dataService.AddTraceLog("Sending to users: " + subscription.Users.Count);

                    foreach (var user in subscription.Users)
                    {
                        _dataService.AddTraceLog("Sending to user: " + user.ChatId);
                        await _tgService.SendMessage(user.ChatId, link);
                    }

                    _dataService.AddTraceLog("Set last post for subscription " + subscription.SubscriptionName + " - " + post.id);
                    await _dataService.SetLastPost(subscription.SubscriptionId, post.id);
                }
            }
            _dataService.AddTraceLog("Sync finished");
        }
    }
}
