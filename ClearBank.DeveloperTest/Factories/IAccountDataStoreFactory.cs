using ClearBank.DeveloperTest.Data;

namespace ClearBank.DeveloperTest.Factories
{
    public interface IAccountDataStoreFactory
    {
        IAccountDataStore GetAccountDataStore();
    }
}
