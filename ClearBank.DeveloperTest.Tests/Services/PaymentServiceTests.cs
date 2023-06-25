using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Factories;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Types;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;

namespace ClearBank.DeveloperTest.Tests.Services
{
    [TestFixture]
    internal class PaymentServiceTests
    {
        IOptions<DataStoreOptions> _options;
        Mock<IAccountDataStoreFactory> _accountDataStoreFactoryMock = new Mock<IAccountDataStoreFactory>();
        Account _account;
        Mock<IAccountDataStore> _accountDataStoreMock = new Mock<IAccountDataStore>();

        [SetUp]
        public void SetUp() 
        {
            //reset the mock ready for the next testcase to run
            _accountDataStoreMock.Reset();
        }

        [TestCase(PaymentScheme.FasterPayments, AllowedPaymentSchemes.FasterPayments)]
        [TestCase(PaymentScheme.Bacs, AllowedPaymentSchemes.Bacs)]
        [TestCase(PaymentScheme.Chaps, AllowedPaymentSchemes.Chaps)]
        public void MakePayment_ValidPaymentScheme_DeductsBalance_ReturnsSuccess(PaymentScheme paymentScheme, AllowedPaymentSchemes allowedPaymentScheme)
        {
           _account = new Account
            {
                AccountNumber = "account999",
                Balance = 500,
                Status = AccountStatus.Live,
                AllowedPaymentSchemes = allowedPaymentScheme
            };

            var originalBalance = _account.Balance;

            _accountDataStoreMock.Setup(s => s.GetAccount(It.IsAny<string>())).Returns(_account);
            _accountDataStoreMock.Setup(s => s.UpdateAccount(_account));
            _accountDataStoreFactoryMock.Setup(s => s.GetAccountDataStore()).Returns(_accountDataStoreMock.Object);

            var request = new MakePaymentRequest
            {
                Amount = 100,
                DebtorAccountNumber = _account.AccountNumber,
                PaymentDate = DateTime.UtcNow,
                PaymentScheme = paymentScheme,
            };

            var _sut = new PaymentService(_accountDataStoreFactoryMock.Object);

            var result = _sut.MakePayment(request);

            Assert.That(result.Success, Is.EqualTo(true));
            Assert.That(_account.Balance, Is.EqualTo(originalBalance - request.Amount));

            //verify the calls
            _accountDataStoreMock.Verify(v => v.GetAccount(It.IsAny<string>()), Times.Once);
            _accountDataStoreMock.Verify(v => v.UpdateAccount(_account), Times.Once);
            _accountDataStoreMock.VerifyNoOtherCalls();
        }

        [TestCase(PaymentScheme.FasterPayments, AllowedPaymentSchemes.Bacs)]
        [TestCase(PaymentScheme.FasterPayments, AllowedPaymentSchemes.Chaps)]
        [TestCase(PaymentScheme.Bacs, AllowedPaymentSchemes.FasterPayments)]
        [TestCase(PaymentScheme.Bacs, AllowedPaymentSchemes.Chaps)]
        [TestCase(PaymentScheme.Chaps, AllowedPaymentSchemes.Bacs)]
        [TestCase(PaymentScheme.Chaps, AllowedPaymentSchemes.FasterPayments)]
        public void MakePayment_InvalidValidPaymentScheme_BalanceRemainsTheSame_ReturnsFalse(PaymentScheme paymentScheme, AllowedPaymentSchemes allowedPaymentScheme)
        {
            _account = new Account
            {
                AccountNumber = "account999",
                Balance = 500,
                Status = AccountStatus.Live,
                AllowedPaymentSchemes = allowedPaymentScheme
            };

            var originalBalance = _account.Balance;

            _accountDataStoreMock.Setup(s => s.GetAccount(It.IsAny<string>())).Returns(_account);
            _accountDataStoreMock.Setup(s => s.UpdateAccount(_account));
            _accountDataStoreFactoryMock.Setup(s => s.GetAccountDataStore()).Returns(_accountDataStoreMock.Object);

            var request = new MakePaymentRequest
            {
                Amount = 100,
                DebtorAccountNumber = _account.AccountNumber,
                PaymentDate = DateTime.UtcNow,
                PaymentScheme = paymentScheme,
            };

            var _sut = new PaymentService(_accountDataStoreFactoryMock.Object);

            var result = _sut.MakePayment(request);

            Assert.That(result.Success, Is.EqualTo(false));
            Assert.That(_account.Balance, Is.EqualTo(originalBalance));
            //verify the calls
            _accountDataStoreMock.Verify(v => v.GetAccount(It.IsAny<string>()), Times.Once);
            _accountDataStoreMock.Verify(v => v.UpdateAccount(_account), Times.Never);
            _accountDataStoreMock.VerifyNoOtherCalls();
        }

        [Test]
        public void MakePayment_Chaps_AccountStatusLive_DeductsBalance_ReturnsSuccess()
        {
            _account = new Account
            {
                AccountNumber = "account999",
                Balance = 500,
                Status = AccountStatus.Live,
                AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps
            };

            var originalBalance = _account.Balance;

            _accountDataStoreMock.Setup(s => s.GetAccount(It.IsAny<string>())).Returns(_account);
            _accountDataStoreMock.Setup(s => s.UpdateAccount(_account));
            _accountDataStoreFactoryMock.Setup(s => s.GetAccountDataStore()).Returns(_accountDataStoreMock.Object);

            var request = new MakePaymentRequest
            {
                Amount = 100,
                DebtorAccountNumber = _account.AccountNumber,
                PaymentDate = DateTime.UtcNow,
                PaymentScheme = PaymentScheme.Chaps,
            };

            var _sut = new PaymentService(_accountDataStoreFactoryMock.Object);

            var result = _sut.MakePayment(request);

            Assert.That(result.Success, Is.EqualTo(true));
            Assert.That(_account.Balance, Is.EqualTo(originalBalance - request.Amount));
            //verify the calls
            _accountDataStoreMock.Verify(v => v.GetAccount(It.IsAny<string>()), Times.Once);
            _accountDataStoreMock.Verify(v => v.UpdateAccount(_account), Times.Once);
            _accountDataStoreMock.VerifyNoOtherCalls();
        }

        [TestCase(AccountStatus.InboundPaymentsOnly)]
        [TestCase(AccountStatus.Disabled)]
        public void MakePayment_Chaps_AccountStatusNotLive_BalanceRemainsTheSame_ReturnsFalse(AccountStatus status)
        {
            _account = new Account
            {
                AccountNumber = "account999",
                Balance = 500,
                Status = status,
                AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps
            };

            var originalBalance = _account.Balance;

            _accountDataStoreMock.Setup(s => s.GetAccount(It.IsAny<string>())).Returns(_account);
            _accountDataStoreMock.Setup(s => s.UpdateAccount(_account));
            _accountDataStoreFactoryMock.Setup(s => s.GetAccountDataStore()).Returns(_accountDataStoreMock.Object);

            var request = new MakePaymentRequest
            {
                Amount = 100,
                DebtorAccountNumber = _account.AccountNumber,
                PaymentDate = DateTime.UtcNow,
                PaymentScheme = PaymentScheme.Chaps,
            };

            var _sut = new PaymentService(_accountDataStoreFactoryMock.Object);

            var result = _sut.MakePayment(request);

            Assert.That(result.Success, Is.EqualTo(false));
            Assert.That(_account.Balance, Is.EqualTo(originalBalance));
            //verify the calls
            _accountDataStoreMock.Verify(v => v.GetAccount(It.IsAny<string>()), Times.Once);
            _accountDataStoreMock.Verify(v => v.UpdateAccount(_account), Times.Never);
            _accountDataStoreMock.VerifyNoOtherCalls();
        }

        [Test]
        public void MakePayment_FasterPAyment_BalanceLowerThanAmount_BalanceRemainsTheSame_ReturnsFalse()
        {
            _account = new Account
            {
                AccountNumber = "account999",
                Balance = 50,
                Status = AccountStatus.Live,
                AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments
            };

            var originalBalance = _account.Balance;

            _accountDataStoreMock.Setup(s => s.GetAccount(It.IsAny<string>())).Returns(_account);
            _accountDataStoreMock.Setup(s => s.UpdateAccount(_account));
            _accountDataStoreFactoryMock.Setup(s => s.GetAccountDataStore()).Returns(_accountDataStoreMock.Object);

            var request = new MakePaymentRequest
            {
                Amount = 100,
                DebtorAccountNumber = _account.AccountNumber,
                PaymentDate = DateTime.UtcNow,
                PaymentScheme = PaymentScheme.Chaps,
            };

            var _sut = new PaymentService(_accountDataStoreFactoryMock.Object);

            var result = _sut.MakePayment(request);

            Assert.That(result.Success, Is.EqualTo(false));
            Assert.That(_account.Balance, Is.EqualTo(originalBalance));
            //verify the calls
            _accountDataStoreMock.Verify(v => v.GetAccount(It.IsAny<string>()), Times.Once);
            _accountDataStoreMock.Verify(v => v.UpdateAccount(_account), Times.Never);
            _accountDataStoreMock.VerifyNoOtherCalls();
        }

        [Test]
        public void MakePayment_UnableToFindAccount_ReturnsException()
        {
            _account = new Account
            {
                AccountNumber = "account999",
                Balance = 500,
                Status = AccountStatus.Live,
                AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments
            };

            var originalBalance = _account.Balance;

            _accountDataStoreMock.Setup(s => s.GetAccount(It.IsAny<string>())).Returns(value: null);
            _accountDataStoreMock.Setup(s => s.UpdateAccount(_account));
            _accountDataStoreFactoryMock.Setup(s => s.GetAccountDataStore()).Returns(_accountDataStoreMock.Object);

            var request = new MakePaymentRequest
            {
                Amount = 100,
                DebtorAccountNumber = _account.AccountNumber,
                PaymentDate = DateTime.UtcNow,
                PaymentScheme = PaymentScheme.FasterPayments,
            };

            var _sut = new PaymentService(_accountDataStoreFactoryMock.Object);

            var ex = Assert.Throws<Exception>(() => _sut.MakePayment(request));

            Assert.That(ex.Message, Is.EqualTo("unable to find account"));
            _accountDataStoreMock.Verify(v => v.GetAccount(It.IsAny<string>()), Times.Once);
            _accountDataStoreMock.Verify(v => v.UpdateAccount(_account), Times.Never);
            _accountDataStoreMock.VerifyNoOtherCalls();
        }
    }
}
