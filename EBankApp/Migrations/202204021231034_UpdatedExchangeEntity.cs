namespace EBankApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedExchangeEntity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Exchanges", "ExchangeValue_GBP", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Exchanges", "ExchangeValue_GBP");
        }
    }
}
