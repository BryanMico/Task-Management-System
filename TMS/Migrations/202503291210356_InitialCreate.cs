namespace TMS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Backlogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ActionType = c.String(nullable: false),
                        Details = c.String(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false),
                        PasswordHash = c.String(nullable: false),
                        RoleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Tasks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        DueDate = c.DateTime(nullable: false),
                        Status = c.String(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        UpdatedDate = c.DateTime(),
                        Priority = c.String(nullable: false),
                        IsCompleted = c.Boolean(nullable: false),
                        CreatedById = c.Int(nullable: false),
                        AssignedToId = c.Int(),
                        ProjectId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.AssignedToId)
                .ForeignKey("dbo.Users", t => t.CreatedById)
                .ForeignKey("dbo.Projects", t => t.ProjectId)
                .Index(t => t.CreatedById)
                .Index(t => t.AssignedToId)
                .Index(t => t.ProjectId);
            
            CreateTable(
                "dbo.Projects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        CreatedById = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.CreatedById)
                .Index(t => t.CreatedById);
            
            CreateTable(
                "dbo.Roles",
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
                .PrimaryKey(t => new { t.UserId, t.SpecializedRoleId })
                .ForeignKey("dbo.SpecializedRoles", t => t.SpecializedRoleId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.SpecializedRoleId);
            
            CreateTable(
                "dbo.SpecializedRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Backlogs", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserSpecializedRoles", "UserId", "dbo.Users");
            DropForeignKey("dbo.UserSpecializedRoles", "SpecializedRoleId", "dbo.SpecializedRoles");
            DropForeignKey("dbo.Users", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.Tasks", "ProjectId", "dbo.Projects");
            DropForeignKey("dbo.Projects", "CreatedById", "dbo.Users");
            DropForeignKey("dbo.Tasks", "CreatedById", "dbo.Users");
            DropForeignKey("dbo.Tasks", "AssignedToId", "dbo.Users");
            DropIndex("dbo.UserSpecializedRoles", new[] { "SpecializedRoleId" });
            DropIndex("dbo.UserSpecializedRoles", new[] { "UserId" });
            DropIndex("dbo.Projects", new[] { "CreatedById" });
            DropIndex("dbo.Tasks", new[] { "ProjectId" });
            DropIndex("dbo.Tasks", new[] { "AssignedToId" });
            DropIndex("dbo.Tasks", new[] { "CreatedById" });
            DropIndex("dbo.Users", new[] { "RoleId" });
            DropIndex("dbo.Backlogs", new[] { "UserId" });
            DropTable("dbo.SpecializedRoles");
            DropTable("dbo.UserSpecializedRoles");
            DropTable("dbo.Roles");
            DropTable("dbo.Projects");
            DropTable("dbo.Tasks");
            DropTable("dbo.Users");
            DropTable("dbo.Backlogs");
        }
    }
}
