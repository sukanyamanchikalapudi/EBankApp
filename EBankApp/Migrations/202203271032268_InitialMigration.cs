namespace EBankApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Accounts",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    AccountNumber = c.String(),
                    AccountBalance = c.Int(nullable: false),
                    AccountType = c.Int(nullable: false),
                    UserId = c.Int(nullable: false),
                    LastModified = c.DateTime(),
                    CreatedOn = c.DateTime(),
                    CreatedBy = c.Int(),
                    ModifiedBy = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.Transactions",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    FromAccount = c.String(),
                    ToAccount = c.String(),
                    TransactionType = c.Int(nullable: false),
                    Credited = c.Int(nullable: false),
                    Debited = c.Int(nullable: false),
                    AccountId = c.Int(nullable: false),
                    LastModified = c.DateTime(),
                    CreatedOn = c.DateTime(),
                    CreatedBy = c.Int(),
                    ModifiedBy = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);

            CreateTable(
                "dbo.Users",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    FirstName = c.String(nullable: false),
                    LastName = c.String(nullable: false),
                    UserName = c.String(nullable: false),
                    Password = c.String(nullable: false),
                    PIN = c.String(nullable: false),
                    RoleId = c.Int(nullable: false),
                    LastModified = c.DateTime(),
                    CreatedOn = c.DateTime(),
                    CreatedBy = c.Int(),
                    ModifiedBy = c.Int(),
                })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Notifications",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Message = c.String(),
                    NotificationType = c.Int(nullable: false),
                    UserId = c.Int(nullable: false),
                    LastModified = c.DateTime(),
                    CreatedOn = c.DateTime(),
                    CreatedBy = c.Int(),
                    ModifiedBy = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.UserActivities",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.Int(nullable: false),
                    UserId = c.Int(nullable: false),
                    LastModified = c.DateTime(),
                    CreatedOn = c.DateTime(),
                    CreatedBy = c.Int(),
                    ModifiedBy = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.Roles",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    LastModified = c.DateTime(),
                    CreatedOn = c.DateTime(),
                    CreatedBy = c.Int(),
                    ModifiedBy = c.Int(),
                })
                .PrimaryKey(t => t.Id);

        }

        public override void Down()
        {
            DropForeignKey("dbo.UserActivities", "UserId", "dbo.Users");
            DropForeignKey("dbo.Notifications", "UserId", "dbo.Users");
            DropForeignKey("dbo.Accounts", "UserId", "dbo.Users");
            DropForeignKey("dbo.Transactions", "AccountId", "dbo.Accounts");
            DropIndex("dbo.UserActivities", new[] { "UserId" });
            DropIndex("dbo.Notifications", new[] { "UserId" });
            DropIndex("dbo.Transactions", new[] { "AccountId" });
            DropIndex("dbo.Accounts", new[] { "UserId" });
            DropTable("dbo.Roles");
            DropTable("dbo.UserActivities");
            DropTable("dbo.Notifications");
            DropTable("dbo.Users");
            DropTable("dbo.Transactions");
            DropTable("dbo.Accounts");
        }
    }
}
