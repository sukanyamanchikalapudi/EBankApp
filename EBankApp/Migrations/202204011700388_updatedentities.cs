namespace EBankApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedentities : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Accounts", "Currency", c => c.Int(nullable: false));
            AlterColumn("dbo.Accounts", "AccountBalance", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Accounts", "AccountBalance", c => c.Int(nullable: false));
            DropColumn("dbo.Accounts", "Currency");
        }
    }
}
