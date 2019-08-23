namespace CmsShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CmsS : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.tblCategory", newName: "tblCategoryy");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.tblCategoryy", newName: "tblCategory");
        }
    }
}
