namespace TimeTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changedMultipleRecorderPerTask : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Tasks", "timerData_RecorderId", "dbo.Recorders");
            DropIndex("dbo.Tasks", new[] { "timerData_RecorderId" });
            AddColumn("dbo.Recorders", "Task_id", c => c.Int());
            CreateIndex("dbo.Recorders", "Task_id");
            AddForeignKey("dbo.Recorders", "Task_id", "dbo.Tasks", "id");
            DropColumn("dbo.Tasks", "timerData_RecorderId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Tasks", "timerData_RecorderId", c => c.Int());
            DropForeignKey("dbo.Recorders", "Task_id", "dbo.Tasks");
            DropIndex("dbo.Recorders", new[] { "Task_id" });
            DropColumn("dbo.Recorders", "Task_id");
            CreateIndex("dbo.Tasks", "timerData_RecorderId");
            AddForeignKey("dbo.Tasks", "timerData_RecorderId", "dbo.Recorders", "RecorderId");
        }
    }
}
