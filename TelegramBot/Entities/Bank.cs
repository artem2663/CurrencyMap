namespace TelegramBot.Entities
{
    public class Bank
    {
        public string BankName { get; private set; }
        public decimal Distance { get; private set; }
        public decimal Rate { get; private set; }
        public decimal Lat { get; private set; }
        public decimal Lng { get; private set; }


        public Bank(string bankName, decimal distance, decimal rate, decimal lat, decimal lng)
        {
            BankName = bankName;
            Distance = distance;
            Rate = rate;
            Lat = lat;
            Lng = lng;
        }
    }
}
