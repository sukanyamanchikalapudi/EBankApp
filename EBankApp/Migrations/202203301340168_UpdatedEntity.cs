namespace EBankApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedEntity : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Exchanges", "ExchangeValue_USD", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Exchanges", "ExchangeValue_USD", c => c.Int(nullable: false));
        }
    }
}
