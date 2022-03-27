namespace EBankApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedExchangeEntity : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Exchanges",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CurrencyCode = c.String(),
                        ExchangeValue_USD = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Exchanges");
        }
    }
}
