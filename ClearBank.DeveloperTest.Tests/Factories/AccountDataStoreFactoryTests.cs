using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Factories;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;

namespace ClearBank.DeveloperTest.Tests.Factories
{
    [TestFixture]
    internal class AccountDataStoreFactoryTests
    {
        IOptions<DataStoreOptions> _options;

        [SetUp]
        public void SetUp() 
        {
            _options = Options.Create(new DataStoreOptions { DataStoreType = Types.DataStoreTypes.Backup });
        }

        [Test]
        public void GetAccountDataStore_Builds_BackupAccountDataStore()
        {
            var _sut = new AccountDataStoreFactory(_options);

            var dataStore = _sut.GetAccountDataStore();

            Assert.That(dataStore, Is.TypeOf<BackupAccountDataStore>());
        }

        [Test]
        public void GetAccountDataStore_Builds_AccountDataStore()
        {
            _options.Value.DataStoreType = Types.DataStoreTypes.Account;

            var _sut = new AccountDataStoreFactory(_options);

            var dataStore = _sut.GetAccountDataStore();

            Assert.That(dataStore, Is.TypeOf<AccountDataStore>());
        }

        [Test]
        public void GetAccountDataStore_MissingOpions_ThrowsException()
        {
            _options.Value.DataStoreType = null;

            var _sut = new AccountDataStoreFactory(_options);

           var ex = Assert.Throws<Exception>( () => _sut.GetAccountDataStore());

           Assert.That(ex.Message, Is.EqualTo("missing options"));
        }

        [Test]
        public void GetAccountDataStore_UnknownType_ThrowsException()
        {
            _options.Value.DataStoreType = Types.DataStoreTypes.Unknown;

            var _sut = new AccountDataStoreFactory(_options);

            var ex = Assert.Throws<Exception>(() => _sut.GetAccountDataStore());

            Assert.That(ex.Message, Is.EqualTo("datastore type not configured"));
        }
    }
}
