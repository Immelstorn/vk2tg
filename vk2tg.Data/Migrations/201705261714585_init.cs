namespace vk2tg.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "vk2tg.Subscriptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SubscriptionId = c.String(),
                        LastPostId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "vk2tg.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ChatId = c.Long(nullable: false),
                        Username = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "vk2tg.UserSubscriptions",
                c => new
                    {
                        User_Id = c.Int(nullable: false),
                        Subscription_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.User_Id, t.Subscription_Id })
                .ForeignKey("vk2tg.Users", t => t.User_Id, cascadeDelete: true)
                .ForeignKey("vk2tg.Subscriptions", t => t.Subscription_Id, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.Subscription_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("vk2tg.UserSubscriptions", "Subscription_Id", "vk2tg.Subscriptions");
            DropForeignKey("vk2tg.UserSubscriptions", "User_Id", "vk2tg.Users");
            DropIndex("vk2tg.UserSubscriptions", new[] { "Subscription_Id" });
            DropIndex("vk2tg.UserSubscriptions", new[] { "User_Id" });
            DropTable("vk2tg.UserSubscriptions");
            DropTable("vk2tg.Users");
            DropTable("vk2tg.Subscriptions");
        }
    }
}
