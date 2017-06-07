namespace vk2tg.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeSubscriptionIdType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("vk2tg.Subscriptions", "SubscriptionId", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("vk2tg.Subscriptions", "SubscriptionId", c => c.String());
        }
    }
}
