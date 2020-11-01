using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Configuration;


namespace DataCollector
{
    public class APIDataCollector
    {
        string banksUrl = ConfigurationManager.AppSettings["banksUrl"];
        string ratesJsonUrl = ConfigurationManager.AppSettings["ratesJsonUrl"];
        string banksNodePattern = "//div/span[contains(@class, 'org')]/a[@class='mark-normal mark-normal-org']";
        string depNodePattern = "//table/tr";


        HtmlParser htmlParser = new HtmlParser();
        List<Bank> bankList = new List<Bank>();

        public List<Bank> CollectBanks()
        {
            HtmlNodeCollection banksCollection = htmlParser.getData(banksUrl, banksNodePattern);

            foreach (HtmlNode bankNode in banksCollection)
            {
                string bankUrl = bankNode.Attributes["href"].Value + "/branches";

                CollectDepartments(bankNode.InnerText, bankUrl);

            }

            return bankList;
        }


        public void CollectDepartments(string bankName, string bankUrl)
        {
            HtmlNodeCollection depCollection = htmlParser.getData(bankUrl, depNodePattern);

            if (depCollection[0].SelectSingleNode("//li/img") == null)
                return;

            string orgBankID = depCollection[0].SelectSingleNode("//li/img").Attributes["src"].Value;
            orgBankID = orgBankID.Replace("https://finance.ua/org/emailimage/-/", "");
            orgBankID = orgBankID.Replace("/general", "");

            foreach (HtmlNode depNode in depCollection)
            {
                if (depNode.Attributes["lng"] == null || depNode.Attributes["lat"] == null)
                    continue;


                bankList.Add(new Bank
                    (
                        orgBankID,
                        bankName,
                        Convert.ToDecimal(depNode.Attributes["lng"].Value),
                        Convert.ToDecimal(depNode.Attributes["lat"].Value)
                    )
                );
            }
        }


        public List<BankRates> GetBankRates()
        {
            List<BankRates> ratesList = new List<BankRates>();

            WebClient httpClient = new WebClient();

            string json = httpClient.DownloadString(ratesJsonUrl);

            JObject jObj = JObject.Parse(json);

            DateTime settingDate = (DateTime)jObj["date"];

            JArray ratesArr = (JArray)jObj["organizations"];
            
            foreach(JObject rates in ratesArr)
            {
                if (rates["currencies"]["USD"] != null)
                {
                    //continue;
                    ratesList.Add(new BankRates(
                        rates["id"].ToString(), "USD",
                        Convert.ToDecimal(rates["currencies"]["USD"]["ask"]),
                        Convert.ToDecimal(rates["currencies"]["USD"]["bid"]),
                        settingDate
                    ));
                }

                
            }

            return ratesList;
        }

    }
}