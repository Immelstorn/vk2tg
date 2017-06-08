namespace vk2tg.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPrettyName : DbMigration
    {
        public override void Up()
        {
            AddColumn("vk2tg.Subscriptions", "SubscriptionPrettyName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("vk2tg.Subscriptions", "SubscriptionPrettyName");
        }
    }
}
