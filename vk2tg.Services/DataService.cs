using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using vk2tg.Data.Models;

namespace vk2tg.Services
{
    public class DataService
    {
        public async Task<bool> UserExists(long chatId)
        {
            using(var db = new Vk2TgDbContext())
            {
                return await db.Users.AnyAsync(u => u.ChatId == chatId);
            }
        }

        public async Task RegisterOrUpdateUser(long chatId, string username)
        {
            using(var db = new Vk2TgDbContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.ChatId == chatId);
                if(user == null)
                {
                    db.Users.Add(new User {
                                     ChatId = chatId,
                                     Username = username,
                                     CreatedAt = DateTime.UtcNow
                                 });
                }
                else
                {
                    user.Username = username;
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<string>> GetUserSubscriptions(long chatId)
        {
            using(var db = new Vk2TgDbContext())
            {
                var user = await db.Users.Include(u => u.Subscriptions).Where(u => u.ChatId == chatId).FirstOrDefaultAsync();
                return user?.Subscriptions.Select(s => s.SubscriptionName).ToList() ?? new List<string>();
            }
        }


        public async Task<bool> AddSubscription(long id, string name, string prettyName, long lastpostId, long chatId)
        {
            using(var db = new Vk2TgDbContext())
            {
                var user = await db.Users.Include(u => u.Subscriptions).Where(u => u.ChatId == chatId).FirstOrDefaultAsync();

                if(user.Subscriptions.Any(s => s.SubscriptionId == id))
                {
                    return false;
                }

                var subscription = await db.Subscriptions.Where(s => s.SubscriptionId == id).FirstOrDefaultAsync();
                if(subscription == null)
                {
                    subscription = new Subscription {
                        SubscriptionId = id,
                        SubscriptionName = name,
                        SubscriptionPrettyName = prettyName,
                        LastPostId = lastpostId
                    };
                    db.Subscriptions.Add(subscription);
                }

                user.Subscriptions.Add(subscription);

                await db.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> RemoveSubscription(long id, long chatId)
        {
            using(var db = new Vk2TgDbContext())
            {
                var user = await db.Users.Include(u => u.Subscriptions).Where(u => u.ChatId == chatId).FirstOrDefaultAsync();
                var subscription = user.Subscriptions.FirstOrDefault(s => s.SubscriptionId == id);
                if (subscription == null)
                {
                    return false;
                }

                user.Subscriptions.Remove(subscription);
                await db.SaveChangesAsync();
                return true;
            }
        }

        public async Task<List<Subscription>> GetSubscriptionsToCheck()
        {
            using(var db = new Vk2TgDbContext())
            {
                return await db.Subscriptions.Where(s => s.Users.Any()).Include(s => s.Users).ToListAsync();
            }
        }

        public async Task SetLastPost(long subscriptionId, long postId)
        {
            using(var db = new Vk2TgDbContext())
            {
                var subscription = await db.Subscriptions.FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);
                if(subscription != null)
                {
                    if(subscription.LastPostId < postId)
                    {
                        subscription.LastPostId = postId;
                        await db.SaveChangesAsync();
                    }
                }
            }
        }

        public async Task AddLog(int subscriptionId, long postID, string link)
        {
            using(var db = new Vk2TgDbContext())
            {
                db.Logs.Add(new Log {
                    DateTime = DateTime.UtcNow,
                    SubscriptionId = subscriptionId,
                    Link = link,
                    PostId = postID
                });
                await db.SaveChangesAsync();
            }
        }
    }
}