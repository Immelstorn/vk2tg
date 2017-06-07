namespace vk2tg.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addUserCreatedDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("vk2tg.Users", "CreatedAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("vk2tg.Users", "CreatedAt");
        }
    }
}
