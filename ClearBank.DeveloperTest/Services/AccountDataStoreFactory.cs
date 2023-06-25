using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Factories;
using ClearBank.DeveloperTest.Types;
using Microsoft.Extensions.Options;

namespace ClearBank.DeveloperTest.Services
{
    public class AccountDataStoreFactory : IAccountDataStoreFactory
    {
        private readonly DataStoreOptions _options;

        public AccountDataStoreFactory(IOptions<DataStoreOptions> options)
        {
            _options = options.Value;    
        }

        public IAccountDataStore GetAccountDataStore()
        {
            IAccountDataStore account = null;
            if (_options.DataStoreType == DataStoreTypes.Backup)
                account = new BackupAccountDataStore();
            else
                account = new AccountDataStore();
                                
            return account;
        }
    }
}
