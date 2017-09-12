namespace vk2tg.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDonationMessage : DbMigration
    {
        public override void Up()
        {
            AddColumn("vk2tg.Users", "DonateMessageSent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("vk2tg.Users", "DonateMessageSent");
        }
    }
}
