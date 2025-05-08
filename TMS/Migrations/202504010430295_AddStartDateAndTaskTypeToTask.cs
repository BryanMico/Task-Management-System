namespace TMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStartDateAndTaskTypeToTask : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tasks", "StartDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Tasks", "TaskType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tasks", "TaskType");
            DropColumn("dbo.Tasks", "StartDate");
        }
    }
}
