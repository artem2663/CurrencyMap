using System;

namespace DataCollector
{
    public class BankRates
    {
        public string OrgBankID { get; private set; }
        public string CurrencyCode { get; private set; }
        public decimal RateAsk { get; private set; }
        public decimal RateBid { get; private set; }
        public DateTime SettingDate { get; private set; }

        public BankRates(string orgBankID, string currencyCode, decimal rateAsk, decimal rateBid, DateTime settingDate)
        {
            OrgBankID = orgBankID;
            CurrencyCode = currencyCode;
            RateAsk = rateAsk;
            RateBid = rateBid;
            SettingDate = settingDate;
        }
    }
}