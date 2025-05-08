namespace TMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTaskTransferModels : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tasks", "TransferredBy", c => c.String());
            AddColumn("dbo.Tasks", "TransferredTo", c => c.String());
            AddColumn("dbo.Tasks", "TransferredDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Tasks", "TransferredDate");
            DropColumn("dbo.Tasks", "TransferredTo");
            DropColumn("dbo.Tasks", "TransferredBy");
        }
    }
}
