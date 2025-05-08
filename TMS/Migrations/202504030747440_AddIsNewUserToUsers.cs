namespace TMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsNewUserToUsers : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "IsNewUser", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "IsNewUser");
        }
    }
}
