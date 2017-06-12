namespace vk2tg.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addUserFirstAndLastName : DbMigration
    {
        public override void Up()
        {
            AddColumn("vk2tg.Users", "FirstName", c => c.String());
            AddColumn("vk2tg.Users", "LastName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("vk2tg.Users", "LastName");
            DropColumn("vk2tg.Users", "FirstName");
        }
    }
}
