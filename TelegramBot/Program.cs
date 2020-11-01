using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Entities;

namespace TelegramBot
{

    class Program
    {
        static TelegramBotClient client = new TelegramBotClient(ConfigurationManager.AppSettings["TelegramApiKey"]);

        static Dictionary<long, UserRequest> listRequests = new Dictionary<long, UserRequest>();

        static void Main(string[] args)
        {
            
            client.OnMessage += Client_OnMessage;

            client.StartReceiving();
            Console.ReadLine();
            client.StopReceiving();

        }


        static UserRequest GetUserRequest(long chatID, MessageEventArgs e)
        {
            string[] requestsArr = { "/buy", "/sell", "/buy_eur", "/sell_eur", "/relocate", "/feedback" };


            UserRequest userRequest;

            listRequests.TryGetValue(chatID, out userRequest);
      
            if (userRequest == null)
            {
                userRequest = new UserRequest();
                listRequests.Add(chatID, userRequest);
            }

            if (e.Message.Location != null)
                userRequest.Location = e.Message.Location;

            if (Array.Exists(requestsArr, el => el == e.Message.Text))
                userRequest.Request = e.Message.Text;

            if (e.Message.Text == "/start")
            {
                userRequest.Location = null;
                userRequest.Request = null;
            }

            return userRequest;
        }



        static string DistanceAdjuster(decimal d)
        {
            string m = d < 1 ? "" : "к";
            if (d < 1)
                d = Math.Round(d * 1000, 0);
            else
                d = Math.Round(d, 2);

            return String.Concat(d, " ", m, "м");
        }



        private static void Client_OnMessage(object sender, MessageEventArgs message)
        {
            long chatID = message.Message.Chat.Id;
            string userMessage = message.Message.Text;
            string botMessage = "";
            decimal bankLat = 0,
                    bankLng = 0,
                    distance = 0,
                    lat = 0, 
                    lng = 0;

            ReplyKeyboardMarkup loc = null;

            Dictionary<string, string> dbRatesInfo;

            List<Bank> dbBankRates;


            UserRequest userRequest = GetUserRequest(chatID, message);

            DB db = new DB();


            if (userRequest.Location != null)
            {
                lat = Convert.ToDecimal(userRequest.Location.Latitude);
                lng = Convert.ToDecimal(userRequest.Location.Longitude);
            }

            if (userMessage != null)
                db.SetLogin(chatID, lat, lng, userMessage);

            MemoryStream img = new MemoryStream();

            if(userRequest.Request == "/relocate" && message.Message.Location != null)
                botMessage = "Дякую, Вашу локацію оновлено.";
            
            else if (userMessage == "/statistics")
            {
                List<double> ratesList = db.GetLastRates();
                double rateLast = ratesList.Last();
                double rateDelta = Math.Round((rateLast / ratesList.First() - 1) * 100, 2);

                string[] prediction = db.GetPrediction().Split(',');

                botMessage = String.Concat(
                    "На сьогодні середній курс $ = ", Math.Round(rateLast,2), " грн/дол. ", 
                    "За останні два тижні курс ", (rateDelta < 0 ? "знизився" : "підвищився"), " на ", Math.Abs(rateDelta), "%.\n"
                    
                    );


                ChartDrawer chart = new ChartDrawer();
                Image chartImg = chart.GetChart(ratesList.ToArray());
                chartImg.Save(img, ImageFormat.Png);
                img.Position = 0;


            }
            else if (userRequest.Request == null)
            {
                botMessage = String.Concat(
                   "<b>Вітаю! Я Ваш валютний асистент</b>\n",
                    "Оберіть одну з опцій: \n",
                    "- я хочу купити $: /buy\n",
                    "- я хочу продати $: /sell\n",
                    "- я хочу купити €: /buy_eur\n",
                    "- я хочу продати €: /sell_eur\n",
                    "- змінити локацію: /relocate\n",
                    "- статистика: /statistics\n",
                    "- зворотній зв'язок: /feedback\n\n",
                    "<i>*дані надані сайтом finance.ua</i>\n\n"
                );
            }
            else if (userRequest.Location == null || userMessage == "/relocate")
            {
                botMessage = "Будь-ласка, відправте вашу локацію";
                KeyboardButton k = new KeyboardButton("Відправити");
                k.RequestLocation = true;
                loc = new ReplyKeyboardMarkup(new[] { k });
                loc.ResizeKeyboard = true;
            }
            else if (userRequest.Location != null && 
                    (userRequest.Request == "/sell" 
                     || userRequest.Request == "/buy"
                     || userRequest.Request == "/sell_eur"
                     || userRequest.Request == "/buy_eur"))
            {
                if (userMessage != null)
                    Decimal.TryParse(Regex.Match(userMessage, @"\d+").Value, out distance);

                if (distance > 99)
                    distance /= 1000;

                if (distance == 0)
                    botMessage = "Будь-ласка, відправте радіус пошуку.\nНа приклад, 1км";
            }

            
            if (userRequest.Location != null && distance > 0)
            {
                lat = Convert.ToDecimal(userRequest.Location.Latitude);
                lng = Convert.ToDecimal(userRequest.Location.Longitude);

                byte orderType = (userRequest.Request == "/buy" || userRequest.Request == "/buy_eur")  ? (byte)1 : (byte)2;

                string currencyCode = userRequest.Request.Contains("eur") ? "EUR" : "USD";
                string currencyHTML = currencyCode == "USD" ? "$" : "€";

                dbBankRates = db.GetBanksNearby(lat, lng, orderType, distance, currencyCode);

                if (dbBankRates.Count == 0)
                    botMessage = "В даному околі не знайдено жодного банку. Будь-ласка, оберіть більший радіус";
                else
                {
                    bankLat = dbBankRates.First().Lat; //Convert.ToDecimal(dbBankRates["Lat"]);
                    bankLng = dbBankRates.First().Lng;

                    botMessage = String.Concat(
                        "Найкращий курс в обраній локації:\n",
                        dbBankRates.First().BankName,
                        "\nВідстань: ", DistanceAdjuster(dbBankRates.First().Distance),
                        "\nКурс ", currencyHTML, ": ", dbBankRates.First().Rate,
                        "\n\nІнші варіанти в околі\n");

                    foreach(Bank item in dbBankRates.Skip(1))
                        botMessage = String.Concat(botMessage, "💰 ", item.BankName, " : ", DistanceAdjuster(item.Distance), " : ", item.Rate, "\n");

                    

                    dbRatesInfo = db.GetRatesInfo(currencyCode);

                    botMessage = String.Concat(
                        botMessage, "\n<i>*оптимальний курс на ринку: ",
                        dbRatesInfo[(orderType == 1 ? "OptimalAsk" : "OptimalBid")], "</i>");

                }

            }


            if (userRequest.Request == "/feedback" && userMessage != "/feedback")
            {
                botMessage = "Дякую, Ваше повідомлення найближчим часом буде оброблено і надана відповідь.";
                userRequest.Request = null;
            }

            if (userMessage == "/feedback")
            {
                botMessage = "Напишіть, будь-ласка, Ваше повідомлення:";
            }

            



            if (userMessage.Contains("subscribe"))
            {
                bool isUnsubscribe = (userMessage == "/subscribe") ? false : true;
                db.Unsubscribe(chatID, isUnsubscribe);
                botMessage = (isUnsubscribe) ? "Ви успішно відписані від розсилки" : "Ви успішно підписані на розсилку";
            }


            if (botMessage.Length > 0)
                client.SendTextMessageAsync(chatID, botMessage, ParseMode.Html, replyMarkup: loc);


            if (userMessage == "/statistics")
                client.SendPhotoAsync(chatID, new FileToSend("chart", img));


            if(bankLat > 0)
                client.SendLocationAsync(chatID, (float)bankLng, (float)bankLat);
            

        }


    }

    
}
