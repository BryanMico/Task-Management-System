namespace TMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPasswordValidation : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "PasswordHash", c => c.String(nullable: false, maxLength: 64));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "PasswordHash", c => c.String(nullable: false));
        }
    }
}
