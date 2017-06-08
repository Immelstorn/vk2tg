namespace vk2tg.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeSubscriptionToId : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("vk2tg.Logs", "Subscription_Id", "vk2tg.Subscriptions");
            DropIndex("vk2tg.Logs", new[] { "Subscription_Id" });
            AddColumn("vk2tg.Logs", "SubscriptionId", c => c.Int(nullable: false));
            DropColumn("vk2tg.Logs", "Subscription_Id");
        }
        
        public override void Down()
        {
            AddColumn("vk2tg.Logs", "Subscription_Id", c => c.Int());
            DropColumn("vk2tg.Logs", "SubscriptionId");
            CreateIndex("vk2tg.Logs", "Subscription_Id");
            AddForeignKey("vk2tg.Logs", "Subscription_Id", "vk2tg.Subscriptions", "Id");
        }
    }
}
