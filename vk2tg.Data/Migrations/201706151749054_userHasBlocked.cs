namespace vk2tg.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userHasBlocked : DbMigration
    {
        public override void Up()
        {
            AddColumn("vk2tg.Users", "HasBlocked", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("vk2tg.Users", "HasBlocked");
        }
    }
}
