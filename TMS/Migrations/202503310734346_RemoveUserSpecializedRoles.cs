namespace TMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUserSpecializedRoles : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserSpecializedRoles", "SpecializedRoleId", "dbo.SpecializedRoles");
            DropForeignKey("dbo.UserSpecializedRoles", "UserId", "dbo.Users");
            DropIndex("dbo.UserSpecializedRoles", new[] { "UserId" });
            DropIndex("dbo.UserSpecializedRoles", new[] { "SpecializedRoleId" });
            DropTable("dbo.UserSpecializedRoles");
            DropTable("dbo.SpecializedRoles");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.SpecializedRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserSpecializedRoles",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        SpecializedRoleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.SpecializedRoleId });
            
            CreateIndex("dbo.UserSpecializedRoles", "SpecializedRoleId");
            CreateIndex("dbo.UserSpecializedRoles", "UserId");
            AddForeignKey("dbo.UserSpecializedRoles", "UserId", "dbo.Users", "Id", cascadeDelete: true);
            AddForeignKey("dbo.UserSpecializedRoles", "SpecializedRoleId", "dbo.SpecializedRoles", "Id", cascadeDelete: true);
        }
    }
}
