namespace vk2tg.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addtokensWaits : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "vk2tg.TokensWaits",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Token = c.String(),
                        WaitUntil = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("vk2tg.TokensWaits");
        }
    }
}
