namespace vk2tg.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addLogs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "vk2tg.Logs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateTime = c.DateTime(nullable: false),
                        Link = c.String(),
                        PostId = c.Long(nullable: false),
                        Subscription_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("vk2tg.Subscriptions", t => t.Subscription_Id)
                .Index(t => t.Subscription_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("vk2tg.Logs", "Subscription_Id", "vk2tg.Subscriptions");
            DropIndex("vk2tg.Logs", new[] { "Subscription_Id" });
            DropTable("vk2tg.Logs");
        }
    }
}
