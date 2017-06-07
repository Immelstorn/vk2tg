using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using vk2tg.Data.Models;

namespace vk2tg.Data
{
    public class DbManager
    {
        public async Task AddUser(long chatId, string username)
        {
            using(var db = new Vk2TgDbContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.ChatId == chatId);

                if(user != null)
                {
                    if(!user.Username.Equals(username))
                    {
                        user.Username = username;
                        await db.SaveChangesAsync();
                    }
                    return;
                }

                db.Users.Add(new User {
                                 ChatId = chatId,
                                 Username = username
                             });

                await db.SaveChangesAsync();
            }
        }

        public async Task RemoveSubscription(long chatId, long subscriptionId)
        {
            using(var db = new Vk2TgDbContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.ChatId == chatId);
                if(user == null)
                {
                    throw new ArgumentException("User is not exists");
                }

                var subscription = await db.Subscriptions.FirstOrDefaultAsync(u => u.SubscriptionId == subscriptionId);
                if(subscription == null)
                {
                    throw new ArgumentException($"Subscription is not found");
                }


                if(!user.Subscriptions.Contains(subscription))
                {
                    throw new ArgumentException($"User is not subscribed to {subscriptionId}");
                }

                user.Subscriptions.Remove(subscription);

                await db.SaveChangesAsync();
            }
        }

        public async Task AddSubscription(long chatId, long subscriptionId)
        {
            using(var db = new Vk2TgDbContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.ChatId == chatId);
                if (user == null)
                {
                    throw new ArgumentException("User is not exists");
                }

                var subscription = await db.Subscriptions.FirstOrDefaultAsync(u => u.SubscriptionId == subscriptionId)
                        ?? db.Subscriptions.Add(new Subscription {
                                                    SubscriptionId = subscriptionId,
                                                    LastPostId = 0
                                                });

                if(user.Subscriptions.Contains(subscription))
                {
                    throw new ArgumentException($"User is already subscribed to {subscriptionId}");
                }

                user.Subscriptions.Add(subscription);

                await db.SaveChangesAsync();
            }
        }

        public async Task<List<Subscription>> GetUserSubscriptions(long chatId)
        {
            using(var db = new Vk2TgDbContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.ChatId == chatId);
                if(user == null)
                {
                    throw new ArgumentException("User is not exists");
                }

                return await db.Subscriptions.Where(s => s.Users.Contains(user)).ToListAsync();
            }
        }

        public async Task<List<Subscription>> GetSubscriptions()
        {
            using(var db = new Vk2TgDbContext())
            {
                return await db.Subscriptions.Include(s => s.Users).ToListAsync();
            }
        }

        public async Task SetLastPost(long subscriptionId, long lastPostId)
        {
            using(var db = new Vk2TgDbContext())
            {
                var subscription = await db.Subscriptions.FirstOrDefaultAsync(u => u.SubscriptionId == subscriptionId);
                if(subscription == null)
                {
                    throw new ArgumentException($"Subscription is not found");
                }

                subscription.LastPostId = lastPostId;
                await db.SaveChangesAsync();
            }
        }
    }
}