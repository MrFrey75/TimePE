using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using TimePE.Core.Models;

namespace TimePE.Core.Database.Migrations;

public interface IDatabaseInitializer
{
    void Initialize(string connectionString);
}

public class DatabaseInitializer : IDatabaseInitializer
{
    public void Initialize(string connectionString)
    {
        XpoDefault.DataLayer = XpoDefault.GetDataLayer(
            connectionString, 
            AutoCreateOption.DatabaseAndSchema);

        using var uow = new UnitOfWork();
        uow.UpdateSchema(
            typeof(Project),
            typeof(PayRate),
            typeof(TimeEntry),
            typeof(Incidental),
            typeof(Payment));
        uow.CreateObjectTypeRecords(
            typeof(Project),
            typeof(PayRate),
            typeof(TimeEntry),
            typeof(Incidental),
            typeof(Payment));
    }
}
