namespace AdminCrawler.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitDB : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Articles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UrlSource = c.String(),
                        Title = c.String(),
                        Image = c.String(),
                        Description = c.String(),
                        Content = c.String(),
                        Category = c.String(),
                        CreatedAt = c.Long(nullable: false),
                        SourceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sources", t => t.SourceId, cascadeDelete: true)
                .Index(t => t.SourceId);
            
            CreateTable(
                "dbo.Sources",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Url = c.String(),
                        Tag = c.String(),
                        SelectorSubUrl = c.String(),
                        SubUrl = c.String(),
                        SelectorTitle = c.String(),
                        SelectorImage = c.String(),
                        SelectorDescription = c.String(),
                        SelectorContent = c.String(),
                        CategoryId = c.Int(nullable: false),
                        ArticleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Banners",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Articles", "SourceId", "dbo.Sources");
            DropForeignKey("dbo.Sources", "CategoryId", "dbo.Categories");
            DropIndex("dbo.Sources", new[] { "CategoryId" });
            DropIndex("dbo.Articles", new[] { "SourceId" });
            DropTable("dbo.Banners");
            DropTable("dbo.Categories");
            DropTable("dbo.Sources");
            DropTable("dbo.Articles");
        }
    }
}
