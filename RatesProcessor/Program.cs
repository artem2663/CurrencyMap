using System.Collections.Generic;
using DataCollector;

namespace RatesProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            APIDataCollector collector = new APIDataCollector();
            DB db = new DB();

            List<BankRates> ratesList = collector.GetBankRates();
            db.StoreRates(ratesList);
        }
    }
}
