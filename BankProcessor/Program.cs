using System.Collections.Generic;
using DataCollector;

namespace BankProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            APIDataCollector collector = new APIDataCollector();
            DB db = new DB();

            List<Bank> bankList = collector.CollectBanks();
            db.StoreBanks(bankList);

        }
    }
}
