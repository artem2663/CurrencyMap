namespace DataCollector
{
    public class Bank
    {
        public string OrgBankID { get; private set; }
        public string BankName { get; private set; }
        public decimal Lng { get; private set; }
        public decimal Lat { get; private set; }


        public Bank(string orgBankID, string bankName, decimal lng, decimal lat)
        {
            OrgBankID = orgBankID;
            BankName = bankName;
            Lng = lng;
            Lat = lat;
        }


    }
}