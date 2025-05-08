namespace TMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Admincreated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "CreatedByAdminId", c => c.Int());
            CreateIndex("dbo.Users", "CreatedByAdminId");
            AddForeignKey("dbo.Users", "CreatedByAdminId", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "CreatedByAdminId", "dbo.Users");
            DropIndex("dbo.Users", new[] { "CreatedByAdminId" });
            DropColumn("dbo.Users", "CreatedByAdminId");
        }
    }
}
