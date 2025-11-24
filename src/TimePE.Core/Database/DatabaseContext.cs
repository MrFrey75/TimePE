using DevExpress.Xpo;
using DevExpress.Xpo.DB;

namespace TimePE.Core.Database;

public static class ConnectionHelper
{
    private static IDataLayer? _dataLayer;

    public static IDataLayer GetDataLayer(string connectionString, AutoCreateOption autoCreateOption = AutoCreateOption.DatabaseAndSchema)
    {
        if (_dataLayer == null)
        {
            var dataStore = XpoDefault.GetConnectionProvider(connectionString, autoCreateOption);
            _dataLayer = new SimpleDataLayer(dataStore);
        }
        return _dataLayer;
    }

    public static Session CreateSession(string connectionString)
    {
        return new Session(GetDataLayer(connectionString));
    }

    public static UnitOfWork CreateUnitOfWork(string connectionString)
    {
        return new UnitOfWork(GetDataLayer(connectionString));
    }

    public static void ResetDataLayer()
    {
        _dataLayer = null;
    }
}
