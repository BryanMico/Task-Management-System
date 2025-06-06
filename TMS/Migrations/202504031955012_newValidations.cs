namespace TMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newValidations : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Projects", "Description", c => c.String(maxLength: 300));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Projects", "Description", c => c.String(maxLength: 500));
        }
    }
}
