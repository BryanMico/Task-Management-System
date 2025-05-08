namespace TMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProjectFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Projects", "ProjectCode", c => c.String(nullable: false, maxLength: 20));
            AddColumn("dbo.Projects", "CreatedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.Projects", "DueDate", c => c.DateTime());
            AddColumn("dbo.Projects", "UpdatedAt", c => c.DateTime());
            AddColumn("dbo.Projects", "Status", c => c.String(nullable: false, maxLength: 20));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Projects", "Status");
            DropColumn("dbo.Projects", "UpdatedAt");
            DropColumn("dbo.Projects", "DueDate");
            DropColumn("dbo.Projects", "CreatedAt");
            DropColumn("dbo.Projects", "ProjectCode");
        }
    }
}
