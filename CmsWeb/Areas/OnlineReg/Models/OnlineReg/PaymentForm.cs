using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsData;
using CmsData.Finance;
using CmsData.Registration;
using CmsWeb.Code;
using Elmah;
using UtilityExtensions;

namespace CmsWeb.Areas.OnlineReg.Models
{
    public class PaymentForm
    {
        private bool? _noEChecksAllowed;
        private int? timeOut;
        public decimal? AmtToPay { get; set; }
        public decimal? Donate { get; set; }
        public decimal Amtdue { get; set; }
        public string Coupon { get; set; }
        public string CreditCard { get; set; }
        public string Expires { get; set; }
        public string CVV { get; set; }
        public string Routing { get; set; }
        public string Account { get; set; }

        /// <summary>
        ///     "B" for e-check and "C" for credit card, see PaymentType
        /// </summary>
        public string Type { get; set; }

        public bool AskDonation { get; set; }
        public bool AllowCoupon { get; set; }
        public string Terms { get; set; }
        public int DatumId { get; set; }
        public Guid FormId { get; set; }
        public string URL { get; set; }

        public int TimeOut
        {
            get
            {
                if (!timeOut.HasValue)
                    timeOut = Util.IsDebug() ? 16000000 : DbUtil.Db.Setting("RegTimeout", "180000").ToInt();
                return timeOut.Value;
            }
        }

        public string First { get; set; }
        public string MiddleInitial { get; set; }
        public string Last { get; set; }
        public string Suffix { get; set; }
        public string Description { get; set; }
        public bool PayBalance { get; set; }
        public int? OrgId { get; set; }
        public int? OriginalId { get; set; }
        public bool testing { get; set; }
        public bool? FinanceOnly { get; set; }
        public bool? IsLoggedIn { get; set; }
        public bool? CanSave { get; set; }
        public bool SavePayInfo { get; set; }
        public bool? AllowSaveProgress { get; set; }
        public bool? IsGiving { get; set; }
        public bool NoCreditCardsAllowed { get; set; }

        public bool NoEChecksAllowed
        {
            get
            {
                if (!_noEChecksAllowed.HasValue)
                    _noEChecksAllowed = DbUtil.Db.Setting("NoEChecksAllowed", "false") == "true";
                return _noEChecksAllowed.Value;
            }
        }

        public string Email { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }

        public IEnumerable<SelectListItem> Countries
        {
            get
            {
                var list = CodeValueModel.ConvertToSelect(CodeValueModel.GetCountryList().Where(c => c.Code != "NA"), null);
                list.Insert(0, new SelectListItem {Text = "(not specified)", Value = ""});
                return list;
            }
        }

        public string Phone { get; set; }
        public int? TranId { get; set; }

        public string AutocompleteOnOff
        {
            get
            {
#if DEBUG
                return "on";
#else
                return "off";
#endif
            }
        }

        public string FullName()
        {
            string n;
            n = MiddleInitial.HasValue()
                ? $"{First} {MiddleInitial} {Last}"
                : $"{First} {Last}";
            if (Suffix.HasValue())
                n = n + " " + Suffix;
            return n;
        }

        public Transaction CreateTransaction(CMSDataContext Db, decimal? amount = null, OnlineRegModel m = null)
        {
            if (!amount.HasValue)
                amount = AmtToPay;
            decimal? amtdue = null;
            if (Amtdue > 0)
                amtdue = Amtdue - (amount ?? 0);
            var ti = new Transaction
            {
                First = First,
                MiddleInitial = MiddleInitial.Truncate(1) ?? "",
                Last = Last,
                Suffix = Suffix,
                Donate = Donate,
                Regfees = AmtToPay,
                Amt = amount,
                Amtdue = amtdue,
                Emails = Email,
                Testing = testing,
                Description = Description,
                OrgId = OrgId,
                Url = URL,
                TransactionGateway = OnlineRegModel.GetTransactionGateway(),
                Address = Address.Truncate(50),
                Address2 = Address2.Truncate(50),
                City = City,
                State = State,
                Country = Country,
                Zip = Zip,
                DatumId = DatumId,
                Phone = Phone.Truncate(20),
                OriginalId = OriginalId,
                Financeonly = FinanceOnly,
                TransactionDate = DateTime.Now,
                PaymentType = Type,
                LastFourCC = Type == PaymentType.CreditCard ? CreditCard.Last(4) : null,
                LastFourACH = Type == PaymentType.Ach ? Account.Last(4) : null
            };

            Db.Transactions.InsertOnSubmit(ti);
            Db.SubmitChanges();
            if (OriginalId == null) // first transaction
                ti.OriginalId = ti.Id;
            return ti;
        }

        public static decimal AmountDueTrans(CMSDataContext db, Transaction ti)
        {
            var org = db.LoadOrganizationById(ti.OrgId);
            var tt = (from t in db.ViewTransactionSummaries
                      where t.RegId == ti.OriginalId
                      select t).FirstOrDefault();
            if (tt == null)
                return 0;
            if (org.IsMissionTrip ?? false)
                return (tt.IndAmt ?? 0) - (db.TotalPaid(tt.OrganizationId, tt.PeopleId) ?? 0);
            return tt.TotDue ?? 0;
        }

        public static PaymentForm CreatePaymentFormForBalanceDue(Transaction ti, decimal amtdue, string email)
        {
            PaymentInfo pi = null;
            if (ti.Person != null)
                pi = ti.Person.PaymentInfos.FirstOrDefault();
            if (pi == null)
                pi = new PaymentInfo();

            var pf = new PaymentForm
            {
                URL = ti.Url,
                PayBalance = true,
                AmtToPay = amtdue,
                Amtdue = 0,
                AllowCoupon = true,
                AskDonation = false,
                Description = ti.Description,
                OrgId = ti.OrgId,
                OriginalId = ti.OriginalId,
                Email = Util.FirstAddress(ti.Emails ?? email).Address,
                FormId = Guid.NewGuid(),
                First = ti.First,
                MiddleInitial = ti.MiddleInitial.Truncate(1) ?? "",
                Last = ti.Last,
                Suffix = ti.Suffix,
                Phone = ti.Phone,
                Address = ti.Address,
                Address2 = ti.Address2,
                City = ti.City,
                State = ti.State,
                Country = ti.Country,
                Zip = ti.Zip,
                testing = ti.Testing ?? false,
                TranId = ti.Id
            };

            if (pi.PeopleId == Util.UserPeopleId) // Is this the logged in user?
            {
                pf.CreditCard = pi.MaskedCard;
                pf.Expires = pi.Expires;
                pf.Account = pi.MaskedAccount;
                pf.Routing = pi.Routing;
                pf.SavePayInfo =
                    (pi.MaskedAccount != null && pi.MaskedAccount.StartsWith("X"))
                    || (pi.MaskedCard != null && pi.MaskedCard.StartsWith("X"));
            }

            ClearMaskedNumbers(pf, pi);

            pf.Type = pf.NoEChecksAllowed ? PaymentType.CreditCard : "";
            var org = DbUtil.Db.LoadOrganizationById(ti.OrgId);
            return pf;
        }

        public static PaymentForm CreatePaymentForm(OnlineRegModel m)
        {
            var r = m.GetTransactionInfo();
            if (r == null)
                return null;

            var pf = new PaymentForm
            {
                FormId = Guid.NewGuid(),
                AmtToPay = m.PayAmount() + (m.donation ?? 0),
                AskDonation = m.AskDonation(),
                AllowCoupon = !m.OnlineGiving(),
                PayBalance = false,
                Amtdue = m.TotalAmount() + (m.donation ?? 0),
                Donate = m.donation,
                Description = m.DescriptionForPayment,
                Email = r.Email,
                First = r.First,
                MiddleInitial = r.Middle,
                Last = r.Last,
                Suffix = r.Suffix,
                IsLoggedIn = m.UserPeopleId.HasValue,
                OrgId = m.List[0].orgid,
                URL = m.URL,
                testing = m.testing ?? false,
                Terms = m.Terms,
                Address = r.Address,
                Address2 = r.Address2,
                City = r.City,
                State = r.State,
                Country = r.Country,
                Zip = r.Zip,
                Phone = r.Phone
#if DEBUG2
                 CreditCard = "4111111111111111",
                 CVV = "123",
                 Expires = "1017",
                 Routing = "056008849",
                 Account = "12345678901234"
#endif
            };
            if (r.payinfo.PeopleId == m.UserPeopleId) // Is this the logged in user?
            {
                pf.CreditCard = r.payinfo.MaskedCard;
                pf.Account = r.payinfo.MaskedAccount;
                pf.Routing = r.payinfo.Routing;
                pf.Expires = r.payinfo.Expires;
                pf.SavePayInfo =
                    (r.payinfo.MaskedAccount != null && r.payinfo.MaskedAccount.StartsWith("X"))
                    || (r.payinfo.MaskedCard != null && r.payinfo.MaskedCard.StartsWith("X"));
                pf.Type = r.payinfo.PreferredPaymentType;
            }

            ClearMaskedNumbers(pf, r.payinfo);

            pf.AllowSaveProgress = m.AllowSaveProgress();
            pf.NoCreditCardsAllowed = m.NoCreditCardsAllowed();
            if (m.OnlineGiving())
            {
                pf.NoCreditCardsAllowed = DbUtil.Db.Setting("NoCreditCardGiving", "false").ToBool();
                pf.IsGiving = true;
                pf.FinanceOnly = true;
                pf.Type = r.payinfo.PreferredGivingType;
            }
            else if (m.ManageGiving() || m.OnlinePledge())
            {
                pf.FinanceOnly = true;
            }
            if (pf.NoCreditCardsAllowed)
                pf.Type = PaymentType.Ach; // bank account only
            else if (pf.NoEChecksAllowed)
                pf.Type = PaymentType.CreditCard; // credit card only
            pf.Type = pf.NoEChecksAllowed ? PaymentType.CreditCard : pf.Type;
            pf.DatumId = m.DatumId ?? 0;
            return pf;
        }

        private static void ClearMaskedNumbers(PaymentForm pf, PaymentInfo pi)
        {
            var gateway = DbUtil.Db.Setting("TransactionGateway", "");

            var clearBankDetails = false;
            var clearCreditCardDetails = false;

            switch (gateway.ToLower())
            {
                case "sage":
                    clearBankDetails = !pi.SageBankGuid.HasValue;
                    clearCreditCardDetails = !pi.SageCardGuid.HasValue;
                    break;
                case "transnational":
                    clearBankDetails = !pi.TbnBankVaultId.HasValue;
                    clearCreditCardDetails = !pi.TbnCardVaultId.HasValue;
                    break;
                case "authorizenet":
                    clearBankDetails = !pi.AuNetCustPayBankId.HasValue;
                    clearCreditCardDetails = !pi.AuNetCustPayId.HasValue;
                    break;
            }

            if (clearBankDetails)
            {
                pf.Account = string.Empty;
                pf.Routing = string.Empty;
            }

            if (clearCreditCardDetails)
            {
                pf.CreditCard = string.Empty;
                pf.CVV = string.Empty;
                pf.Expires = string.Empty;
            }
        }

        public static Transaction CreateTransaction(CMSDataContext Db, Transaction t, decimal? amount)
        {
            var amtdue = t.Amtdue - (amount ?? 0);
            var ti = new Transaction
            {
                Name = t.Name,
                First = t.First,
                MiddleInitial = t.MiddleInitial,
                Last = t.Last,
                Suffix = t.Suffix,
                Donate = t.Donate,
                Amtdue = amtdue,
                Amt = amount,
                Emails = Util.FirstAddress(t.Emails).Address,
                Testing = t.Testing,
                Description = t.Description,
                OrgId = t.OrgId,
                Url = t.Url,
                Address = t.Address,
                TransactionGateway = OnlineRegModel.GetTransactionGateway(),
                City = t.City,
                State = t.State,
                Zip = t.Zip,
                DatumId = t.DatumId,
                Phone = t.Phone,
                OriginalId = t.OriginalId ?? t.Id,
                Financeonly = t.Financeonly,
                TransactionDate = DateTime.Now,
                PaymentType = t.PaymentType,
                LastFourCC = t.LastFourCC,
                LastFourACH = t.LastFourACH
            };
            Db.Transactions.InsertOnSubmit(ti);
            Db.SubmitChanges();
            return ti;
        }

        public object Autocomplete(bool small = false)
        {
            if (small)
                return new
                {
                    AUTOCOMPLETE = AutocompleteOnOff,
                    @class = "short"
                };
            return new
            {
                AUTOCOMPLETE = AutocompleteOnOff
            };
        }

        public void PreventNegatives()
        {
            if (AmtToPay < 0)
                AmtToPay = 0;
            if (Donate < 0)
                Donate = 0;
            AllowCoupon = false;
        }

        public void PreventZero(ModelStateDictionary modelState)
        {
            if ((AmtToPay ?? 0) > 0 || (Donate ?? 0) > 0)
                return;
            DbUtil.Db.SubmitChanges();
            modelState.AddModelError("form", "amount zero");
        }

        public void ValidatePaymentForm(ModelStateDictionary modelState)
        {
            switch (Type)
            {
                case PaymentType.Ach:
                    PaymentValidator.ValidateBankAccountInfo(modelState, Routing, Account);
                    break;
                case PaymentType.CreditCard:
                    PaymentValidator.ValidateCreditCardInfo(modelState, this);
                    break;
                default:
                    modelState.AddModelError("Type", "Please select Bank Account or Credit Card.");
                    break;
            }
            ValidateBillingDetails(modelState);
        }

        public void ValidateBillingDetails(ModelStateDictionary modelState)
        {
            if (!First.HasValue())
                modelState.AddModelError("First", "First name is required.");

            if (!Last.HasValue())
                modelState.AddModelError("Last", "Last name is required");

            if (!Address.HasValue())
                modelState.AddModelError("Address", "Address is required.");

            if (!City.HasValue())
                modelState.AddModelError("City", "City is required");

            if (!State.HasValue())
                modelState.AddModelError("State", "State is required.");

            if (!Country.HasValue())
                modelState.AddModelError("Country", "Country is required.");

            if (!Zip.HasValue())
                modelState.AddModelError("Zip", "Zipcode is required.");
        }

        public void CheckStoreInVault(ModelStateDictionary modelState, int peopleid)
        {
            if (!IsLoggedIn.GetValueOrDefault() || !SavePayInfo)
                return;

            var gateway = DbUtil.Db.Gateway(testing);

            // we need to perform a $1 auth if this is a brand new credit card that we are going to store it in the vault.
            // otherwise we skip doing an auth just call store in vault just like normal.
            VerifyCardWithAuth(modelState, gateway, peopleid);
            if (!modelState.IsValid)
                return;

            InitializePaymentInfo(peopleid);

            gateway.StoreInVault(peopleid, Type, CreditCard,
                DbUtil.NormalizeExpires(Expires).ToString2("MMyy"), CVV, Routing, Account,
                IsGiving.GetValueOrDefault());
        }

        /// Perform a $1 authorization... the amount is randomized because some gateways will reject identical, subsequent amounts
        /// within a short period of time.
        private void VerifyCardWithAuth(ModelStateDictionary modelState, IGateway gateway, int peopleId)
        {
            if (Type != PaymentType.CreditCard)
                return;

            if (CreditCard.StartsWith("X"))
                return;

            var random = new Random();
            var dollarAmt = (decimal) random.Next(100, 199)/100;

            var transactionResponse = gateway.AuthCreditCard(peopleId, dollarAmt, CreditCard,
                DbUtil.NormalizeExpires(Expires).ToString2("MMyy"), "One Time Auth", 0, CVV, string.Empty,
                First, Last, Address, Address2, City, State, Country, Zip, Phone);

            if (!transactionResponse.Approved)
                modelState.AddModelError("form", transactionResponse.Message);

            // if we got this far that means the auth worked so now let's do a void for that auth.
            var voidResponse = gateway.VoidCreditCardTransaction(transactionResponse.TransactionId);
        }

        private void InitializePaymentInfo(int peopleId)
        {
            var person = DbUtil.Db.LoadPersonById(peopleId);
            var pi = person.PaymentInfo();
            if (pi == null)
            {
                pi = new PaymentInfo();
                person.PaymentInfos.Add(pi);
            }
            pi.SetBillingAddress(First, MiddleInitial, Last, Suffix, Address, Address2, City,
                State, Country, Zip, Phone);
        }

        public Transaction ProcessPaymentTransaction(OnlineRegModel m)
        {
            var ti = (m?.Transaction != null)
                ? CreateTransaction(DbUtil.Db, m.Transaction, AmtToPay)
                : CreateTransaction(DbUtil.Db);

            int? pid = null;
            if (m != null)
            {
                m.ParseSettings();

                pid = m.UserPeopleId;
                if (m.TranId == null)
                    m.TranId = ti.Id;
            }

            if (!pid.HasValue)
            {
                var pds = DbUtil.Db.FindPerson(First, Last, null, Email, Phone);
                if (pds.Count() == 1)
                    pid = pds.Single().PeopleId.Value;
            }

            TransactionResponse tinfo;
            var gw = DbUtil.Db.Gateway(testing);

            if (SavePayInfo)
                tinfo = gw.PayWithVault(pid ?? 0, AmtToPay ?? 0, Description, ti.Id, Type);
            else
            {
                tinfo = Type == PaymentType.Ach
                    ? PayWithCheck(gw, pid, ti)
                    : PayWithCreditCard(gw, pid, ti);
            }

            ti.TransactionId = tinfo.TransactionId;

            if (ti.Testing.GetValueOrDefault() && !ti.TransactionId.Contains("(testing)"))
                ti.TransactionId += "(testing)";

            ti.Approved = tinfo.Approved;

            if (!ti.Approved.GetValueOrDefault())
            {
                ti.Amtdue += ti.Amt;
                if (m != null && m.OnlineGiving())
                    ti.Amtdue = 0;
            }

            ti.Message = tinfo.Message;
            ti.AuthCode = tinfo.AuthCode;
            ti.TransactionDate = DateTime.Now;

            DbUtil.Db.SubmitChanges();
            return ti;
        }

        private TransactionResponse PayWithCreditCard(IGateway gateway, int? peopleId, Transaction transaction)
        {
            return gateway.PayWithCreditCard(peopleId ?? 0, AmtToPay ?? 0, CreditCard,
                DbUtil.NormalizeExpires(Expires).ToString2("MMyy"), Description, transaction.Id,
                CVV, Email, First, Last, Address, Address2,
                City, State, Country, Zip, Phone);
        }

        private TransactionResponse PayWithCheck(IGateway gw, int? pid, Transaction ti)
        {
            return gw.PayWithCheck(pid ?? 0, AmtToPay ?? 0, Routing, Account, Description, ti.Id, Email,
                First, MiddleInitial, Last, Suffix, Address, Address2, City, State, Country,
                Zip, Phone);
        }

        public RouteModel ProcessPayment(ModelStateDictionary modelState, OnlineRegModel m)
        {
            PreventNegatives();
            PreventZero(modelState);
            if (!modelState.IsValid)
                return RouteModel.ProcessPayment();

            try
            {
                ValidatePaymentForm(modelState);
                if (!modelState.IsValid)
                    return RouteModel.ProcessPayment();

                if (m?.UserPeopleId != null && m.UserPeopleId > 0)
                    CheckStoreInVault(modelState, m.UserPeopleId.Value);
                if (!modelState.IsValid)
                    return RouteModel.ProcessPayment();

                var ti = ProcessPaymentTransaction(m);

                if (ti.Approved == false)
                {
                    modelState.AddModelError("form", ti.Message);
                    return RouteModel.ProcessPayment();
                }

                HttpContext.Current.Session["FormId"] = FormId;
                if (m != null)
                {
                    m.DatumId = DatumId; // todo: not sure this is necessary
                    return m.FinishRegistration(ti);
                }

                OnlineRegModel.ConfirmDuePaidTransaction(ti, ti.TransactionId, true);

                return RouteModel.AmountDue(AmountDueTrans(DbUtil.Db, ti), ti);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                modelState.AddModelError("form", ex.Message);
                return RouteModel.ProcessPayment();
            }
        }

        public void CheckTesting()
        {
            // randomixe payment for testing only
            // prevents testing gateway from giving a duplicate tran error
            var random = new Random();
            AmtToPay += (decimal) random.Next(100, 199)/100;
        }
    }
}
