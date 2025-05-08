namespace TMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTaskNotesToTask : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tasks", "TaskNotes", c => c.String(maxLength: 1000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tasks", "TaskNotes");
        }
    }
}
