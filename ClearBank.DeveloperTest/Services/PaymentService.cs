using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Factories;
using ClearBank.DeveloperTest.Types;
using System;

namespace ClearBank.DeveloperTest.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IAccountDataStoreFactory _accountDataStoreFactory;
        public PaymentService(IAccountDataStoreFactory accountDataStoreFactory)
        {
            _accountDataStoreFactory = accountDataStoreFactory;
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            IAccountDataStore accountDataStore = _accountDataStoreFactory.GetAccountDataStore();
            Account account = accountDataStore.GetAccount(request.DebtorAccountNumber);

            if (account == null)
                throw new Exception("unable to find account");

            var result = BuildPaymentResult(request, account);

            if (result.Success)
            {
                account.Balance -= request.Amount;
                accountDataStore.UpdateAccount(account);
            }

            return result;
        }

        private MakePaymentResult BuildPaymentResult(MakePaymentRequest request, Account account)
        {
            var result = new MakePaymentResult();
                
            switch (request.PaymentScheme)
            {
                case PaymentScheme.Bacs:
                    if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs))
                        result.Success = false;
                    break;

                case PaymentScheme.FasterPayments:
                    if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments))
                        result.Success = false;
                    else if (account.Balance < request.Amount)
                        result.Success = false;
                    break;

                case PaymentScheme.Chaps:
                    if (!account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps))
                        result.Success = false;
                    else if (account.Status != AccountStatus.Live)
                        result.Success = false;
                    break;
            }

            return result;
        }
    }
}
