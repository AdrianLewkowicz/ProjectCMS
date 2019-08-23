namespace CmsShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Product1 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.tblProducts", newName: "tblProductss");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.tblProductss", newName: "tblProducts");
        }
    }
}
