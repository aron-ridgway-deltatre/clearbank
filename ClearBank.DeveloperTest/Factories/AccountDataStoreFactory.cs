using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Types;
using Microsoft.Extensions.Options;
using System;

namespace ClearBank.DeveloperTest.Factories
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
            //when the setting are used with a web api project we could use the validate on start if we expect there to always be a value.
            if (_options?.DataStoreType == null)
                throw new Exception("missing options");

            if (_options.DataStoreType == DataStoreTypes.Backup)
                return new BackupAccountDataStore();
            else if (_options.DataStoreType == DataStoreTypes.Account)
                return new AccountDataStore();
            else
                throw new Exception("datastore type not configured");
        }
    }
}
