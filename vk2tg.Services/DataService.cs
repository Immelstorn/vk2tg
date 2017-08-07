using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using vk2tg.Data.Models;
using vk2tg.Data.Models.DB;

namespace vk2tg.Services
{
    public class DataService
    {
        public async Task<DateTime> GetLastLogDate()
        {
            using (var db = new Vk2TgDbContext())
            {
                return await db.Logs.OrderByDescending(l => l.DateTime).Select(l => l.DateTime).FirstAsync();
            }
        }

        public async Task<bool> UserExists(long chatId)
        {
            using(var db = new Vk2TgDbContext())
            {
                return await db.Users.AnyAsync(u => u.ChatId == chatId);
            }
        }

        public async Task RegisterUser(long chatId, string username)
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

                subscription.Users.Remove(user);
                if(!subscription.Users.Any())
                {
                    db.Subscriptions.Remove(subscription);
                }
                await db.SaveChangesAsync();
                return true;
            }
        }

        public async Task<List<Subscription>> GetSubscriptionsToCheck()
        {
            using(var db = new Vk2TgDbContext())
            {
                return await db.Subscriptions.Where(s => s.Users.Any(u => !u.HasBlocked)).Include(s => s.Users).ToListAsync();
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

        public async Task AddErrorLog(Exception e)
        {
            using(var db = new Vk2TgDbContext())
            {
                db.ErrorLogs.Add(new ErrorLog {
                    DateTime = DateTime.UtcNow,
                    Message = e.Message,
                    StackTrace = e.StackTrace,
                    IsError = true
                });
                await db.SaveChangesAsync();
            }
        }

        public void AddErrorLogSync(Exception e)
        {
            using(var db = new Vk2TgDbContext())
            {
                db.ErrorLogs.Add(new ErrorLog {
                    DateTime = DateTime.UtcNow,
                    Message = e.Message,
                    StackTrace = e.StackTrace,
                    IsError = true
                });
                db.SaveChanges();
            }
        }

        public async Task AddErrorLog(string error)
        {
            using (var db = new Vk2TgDbContext())
            {
                db.ErrorLogs.Add(new ErrorLog
                {
                    DateTime = DateTime.UtcNow,
                    Message = error,
                    IsError = true
                });
               await  db.SaveChangesAsync();
            }
        }

        public async Task AddWait(string token, int seconds)
        {
            using (var db = new Vk2TgDbContext())
            {
                db.TokensWaits.Add(new TokensWait
                {
                   Token = token,
                   WaitUntil = DateTime.UtcNow.AddSeconds(seconds)
                });
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<string>> AllowedTokens(string[] tokens)
        {
            using(var db = new Vk2TgDbContext())
            {
                var delete = await db.TokensWaits.Where(w => w.WaitUntil < DateTime.UtcNow).ToListAsync();
                db.TokensWaits.RemoveRange(delete);
                await db.SaveChangesAsync();
                var tokensToWait = await db.TokensWaits.Select(t => t.Token).ToListAsync();

                return tokens.Except(tokensToWait).ToList();
            }
        }

        public async Task AddTraceLog(string message)
        {
            using (var db = new Vk2TgDbContext())
            {
                db.ErrorLogs.Add(new ErrorLog
                {
                    DateTime = DateTime.UtcNow,
                    Message = message,
                    IsError = false
                });
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateUserInfo(long chatId, string username, string firstName, string lastName)
        {
            using(var db = new Vk2TgDbContext())
            {
                var user = await db.Users.Where(u => u.ChatId == chatId).FirstOrDefaultAsync();
                if(user != null)
                {
                    user.Username = username;
                    user.FirstName = firstName;
                    user.LastName = lastName;
                    user.HasBlocked = false;
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task Block(long chatId)
        {
            using (var db = new Vk2TgDbContext())
            {
                var user = await db.Users.Where(u => u.ChatId == chatId).FirstOrDefaultAsync();
                if (user != null)
                {
                    user.HasBlocked = true;
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}