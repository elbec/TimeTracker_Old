namespace TimeTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedRecorderTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Recorders",
                c => new
                    {
                        RecorderId = c.Int(nullable: false, identity: true),
                        StartTime = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                        IsTimerRunning = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.RecorderId);
            
            AddColumn("dbo.Tasks", "timerData_RecorderId", c => c.Int());
            CreateIndex("dbo.Tasks", "timerData_RecorderId");
            AddForeignKey("dbo.Tasks", "timerData_RecorderId", "dbo.Recorders", "RecorderId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tasks", "timerData_RecorderId", "dbo.Recorders");
            DropIndex("dbo.Tasks", new[] { "timerData_RecorderId" });
            DropColumn("dbo.Tasks", "timerData_RecorderId");
            DropTable("dbo.Recorders");
        }
    }
}
