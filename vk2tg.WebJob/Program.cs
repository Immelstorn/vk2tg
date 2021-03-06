﻿using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using vk2tg.Services;

namespace vk2tg.WebJob
{
    class Program
    {
        private static readonly DataService _dataService = new DataService();
        private static readonly VkService _vkService = new VkService();
        private static readonly TelegraphService _telegraphService = new TelegraphService();
        private static readonly TgService _tgService = new TgService();
        private static readonly int DaysToKeepBlobs = 14;

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
                        _dataService.AddErrorLogSync(exception);
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
                    _dataService.AddErrorLogSync(e);
                }
                catch (Exception) { }
            }
        }

        private static async Task DeleteOldBlobs()
        {
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["BlobStorage"]);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("vk2tg");
            var blobs = container.ListBlobs("", true).OfType<CloudBlockBlob>().Where(b => b.Properties.LastModified.Value.DateTime < DateTime.UtcNow.AddDays(-DaysToKeepBlobs)).ToList();

            foreach (var blob in blobs)
            {
                await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.None, AccessCondition.GenerateIfNotModifiedSinceCondition(DateTime.Now.AddDays(-DaysToKeepBlobs)), null, null);
            }
        }

        private static async Task AsyncMain()   
        {
            await _dataService.AddTraceLog("Sync started");

            var subscriptions = await _dataService.GetSubscriptionsToCheck();

            foreach (var subscription in subscriptions)
            {
                var posts = await _vkService.GetPosts(subscription.SubscriptionId, subscription.LastPostId);

                foreach (var post in posts)
                {
                    try
                    {
                        var lastLogDate = await _dataService.GetLastLogDate();
                        if (lastLogDate.Date < DateTime.Now.Date)
                        {
                            await DeleteOldBlobs();
                        }

//                        var link = await _telegraphService.CreatePage(post, subscription.SubscriptionName, subscription.SubscriptionPrettyName ?? subscription.SubscriptionName);
//                        await _dataService.AddLog(subscription.Id, post.id, link);
//
//                        foreach(var user in subscription.Users.Where(u => !u.HasBlocked))
//                        {
//                            try
//                            {
//                                await _tgService.SendMessage(user.ChatId, link);
//                            }
//                            catch(Exception e)
//                            {
//                                if(e.Message.Equals("Forbidden: bot was blocked by the user"))
//                                {
//                                    await _dataService.Block(user.ChatId);
//                                }
//                                else
//                                {
//                                    throw;
//                                }
//                            }
//                        }

                        await _dataService.SetLastPost(subscription.SubscriptionId, post.id);
                    }
                    catch (AggregateException a)
                    {
                        foreach (var exception in a.InnerExceptions)
                        {
                            try
                            {
                                await _dataService.AddErrorLog(exception);
                            }
                            catch (Exception) { }
                        }
                    }
                    catch (Exception e)
                    {
                        try
                        {
                           await _dataService.AddErrorLog(e);
                        }
                        catch (Exception) { }
                    }
                }
            }
            await _dataService.AddTraceLog("Sync finished");
        }
    }
}
