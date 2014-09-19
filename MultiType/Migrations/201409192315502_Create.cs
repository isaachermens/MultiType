namespace MultiType.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Create : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.KeyAccuracies",
                c => new
                    {
                        KeyAccuracyId = c.Int(nullable: false, identity: true),
                        Key = c.Int(nullable: false),
                        Occurances = c.Int(nullable: false),
                        Errors = c.Int(nullable: false),
                        RaceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.KeyAccuracyId)
                .ForeignKey("dbo.Races", t => t.RaceId, cascadeDelete: true)
                .Index(t => t.RaceId);
            
            CreateTable(
                "dbo.Races",
                c => new
                    {
                        RaceId = c.Int(nullable: false, identity: true),
                        ContentLength = c.Int(nullable: false),
                        CharactersTyped = c.Int(nullable: false),
                        Multiplayer = c.Boolean(nullable: false),
                        Won = c.Boolean(nullable: false),
                        Wpm = c.Int(nullable: false),
                        TimeStamp = c.DateTime(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.RaceId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 4000),
                        LastName = c.String(nullable: false, maxLength: 4000),
                        Password = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.Lessons",
                c => new
                    {
                        LessonId = c.Int(nullable: false, identity: true),
                        Content = c.String(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        TimesCompleted = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.LessonId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Lessons", "UserId", "dbo.Users");
            DropForeignKey("dbo.Races", "UserId", "dbo.Users");
            DropForeignKey("dbo.KeyAccuracies", "RaceId", "dbo.Races");
            DropIndex("dbo.Lessons", new[] { "UserId" });
            DropIndex("dbo.Races", new[] { "UserId" });
            DropIndex("dbo.KeyAccuracies", new[] { "RaceId" });
            DropTable("dbo.Lessons");
            DropTable("dbo.Users");
            DropTable("dbo.Races");
            DropTable("dbo.KeyAccuracies");
        }
    }
}
