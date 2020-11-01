using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using TelegramBot.Entities;

namespace TelegramBot
{
    public class DB
    {

        SqlConnection sqlCon = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlCon"].ConnectionString);

        public List<Bank> GetBanksNearby(decimal lat, decimal lng, byte orderType, decimal radius, string currencyCode)
        {
            List<Bank> bankList = new List<Bank>();
            SqlCommand cmd = new SqlCommand("prcBanksGetNearby", sqlCon);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Lat", lat);
            cmd.Parameters.AddWithValue("@Lng", lng);
            cmd.Parameters.AddWithValue("@OrderType", orderType);
            cmd.Parameters.AddWithValue("@Radius", radius);
            cmd.Parameters.AddWithValue("@CurrencyCode", currencyCode);

            if (sqlCon.State == ConnectionState.Closed)
                sqlCon.Open();

            SqlDataReader dataReader = cmd.ExecuteReader();
            while(dataReader.Read())
            {
                bankList.Add(
                    new Bank(
                        dataReader["BankName"].ToString(),
                        Convert.ToDecimal(dataReader["Distance"]),
                        Convert.ToDecimal(dataReader["Rate"]),
                        Convert.ToDecimal(dataReader["Lat"]),
                        Convert.ToDecimal(dataReader["Lng"])
                   )
               );
            }

            sqlCon.Close();
  
            return bankList;
        }


        public string GetPrediction()
        {
            SqlCommand cmd = new SqlCommand("prcPredictionGet", sqlCon);
            cmd.CommandType = CommandType.StoredProcedure;
            if (sqlCon.State == ConnectionState.Closed)
                sqlCon.Open();

            SqlDataReader dataReader = cmd.ExecuteReader();

            dataReader.Read();

            string predictionResult = dataReader["PredictionResult"].ToString();

            sqlCon.Close();

            return predictionResult;
        }


        public List<double> GetLastRates()
        {
            SqlCommand cmd = new SqlCommand("prcRatesHistoryGet", sqlCon);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CurrencyCode", "USD");
            cmd.Parameters.AddWithValue("@PeriodDays", 14);

            if (sqlCon.State == ConnectionState.Closed)
                sqlCon.Open();

            SqlDataReader dataReader = cmd.ExecuteReader();

            List<double> ratesList = new List<double>();

            while (dataReader.Read())
                ratesList.Add(Convert.ToDouble(dataReader["AvgAsk"]));

            sqlCon.Close();

            return ratesList;
        }


        public Dictionary<string, string> GetRatesInfo(string currencyCode)
        {
            SqlCommand cmd = new SqlCommand("prcRatesCurrentInfoGet", sqlCon);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CurrencyCode", currencyCode);
            if (sqlCon.State == ConnectionState.Closed)
                sqlCon.Open();

            SqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Read();

            var dict = new Dictionary<string, string>();

            dict["AvgAsk"] = dataReader["AvgAsk"].ToString();
            dict["AvgBid"] = dataReader["AvgBid"].ToString();
            dict["OptimalBid"] = dataReader["OptimalBid"].ToString();
            dict["OptimalAsk"] = dataReader["OptimalAsk"].ToString();
            dict["LastUpdate"] = dataReader["LastUpdate"].ToString();

            sqlCon.Close();

            return dict;
        }


        public void SetLogin(long chatID, decimal lat, decimal lng, string activityType, string userInfo="")
        {
            SqlCommand cmd = new SqlCommand("prcLoginSet", sqlCon);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@chatID", chatID);
            cmd.Parameters.AddWithValue("@Lat", lat);
            cmd.Parameters.AddWithValue("@Lng", lng);
            cmd.Parameters.AddWithValue("@ActivityType", activityType);
            cmd.Parameters.AddWithValue("@userInfo", userInfo);

            if (sqlCon.State == ConnectionState.Closed)
                sqlCon.Open();

            cmd.ExecuteNonQuery();

            sqlCon.Close();
        }


        public void Unsubscribe(long chatID, bool isUnsubscribe)
        {
            SqlCommand cmd = new SqlCommand("prcUserUnsubscribe", sqlCon);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@chatID", chatID);
            cmd.Parameters.AddWithValue("@IsUnsubscribe", isUnsubscribe);

            if (sqlCon.State == ConnectionState.Closed)
                sqlCon.Open();

            cmd.ExecuteNonQuery();

            sqlCon.Close();
        }

    }
}
