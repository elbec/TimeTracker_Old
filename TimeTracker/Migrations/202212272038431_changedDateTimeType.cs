namespace TimeTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changedDateTimeType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Recorders", "StartTime", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.Recorders", "EndTime", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Recorders", "EndTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Recorders", "StartTime", c => c.DateTime(nullable: false));
        }
    }
}
