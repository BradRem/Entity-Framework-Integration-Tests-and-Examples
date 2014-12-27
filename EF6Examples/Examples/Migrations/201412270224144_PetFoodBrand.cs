namespace Examples.Migrations
{
    using Examples.Tests;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PetFoodBrand : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PetFoodBrand",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Pet", "FavoritePetFoodBrandId", c => c.Int(nullable: false));

            this.Sql("UPDATE Pet SET FavoritePetFoodBrandId = 1 WHERE Name = '" + LazyLoadingExamples.PET_FIDO + "'");
            this.Sql("UPDATE Pet SET FavoritePetFoodBrandId = 2 WHERE Name = '" + LazyLoadingExamples.PET_FLUFFY + "'");
            this.Sql("UPDATE Pet SET FavoritePetFoodBrandId = 1 WHERE Name = '" + LazyLoadingExamples.PET_PUFFY + "'");

            this.Sql("INSERT INTO PetFoodBrand (Name) VALUES ('" + LazyLoadingExamples.FOOD_ALPHA + "')");
            this.Sql("INSERT INTO PetFoodBrand (Name) VALUES ('" + LazyLoadingExamples.FOOD_BRAVO + "')");

            CreateIndex("dbo.Pet", "FavoritePetFoodBrandId");
            AddForeignKey("dbo.Pet", "FavoritePetFoodBrandId", "dbo.PetFoodBrand", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Pet", "FavoritePetFoodBrandId", "dbo.PetFoodBrand");
            DropIndex("dbo.Pet", new[] { "FavoritePetFoodBrandId" });
            DropColumn("dbo.Pet", "FavoritePetFoodBrandId");
            DropTable("dbo.PetFoodBrand");
        }
    }
}
