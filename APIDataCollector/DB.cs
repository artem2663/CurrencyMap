using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Configuration;
using System.Linq;

namespace DataCollector
{
    public class DB
    {
        
        SqlConnection sqlCon = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlCon"].ConnectionString);


        public void StoreBanks(List<Bank> bankList)
        {
            SqlCommand cmd = new SqlCommand("prcBanksMerge", sqlCon);
            cmd.CommandType = CommandType.StoredProcedure;

            string[] props = { "OrgBankID", "BankName", "Lng", "Lat" };
            DataTable bankTable = ToDataTable(bankList, props);

            cmd.Parameters.AddWithValue("@Banks", bankTable);

            if (sqlCon.State == ConnectionState.Closed)
                sqlCon.Open();

            cmd.ExecuteNonQuery();

            sqlCon.Close();
        }


        public void StoreRates(List<BankRates> ratesList)
        {
            SqlCommand cmd = new SqlCommand("prcBankRatesInsert", sqlCon);
            cmd.CommandType = CommandType.StoredProcedure;

            DataTable dt = new DataTable();
            dt.Columns.Add("OrgBankID", typeof(string));
            dt.Columns.Add("CurrencyCode", typeof(string));
            dt.Columns.Add("Ask", typeof(decimal));
            dt.Columns.Add("Bid", typeof(decimal));
            dt.Columns.Add("SettingDate", typeof(DateTime));

            foreach(BankRates rates in ratesList)
                dt.Rows.Add(rates.OrgBankID, rates.CurrencyCode, rates.RateAsk, rates.RateBid, rates.SettingDate);


            SqlParameter param = new SqlParameter("@BankRates", SqlDbType.Structured)
            {
                TypeName = "dbo.udtBankRates",
                Value = dt
            };
            cmd.Parameters.Add(param);


            if (sqlCon.State == ConnectionState.Closed)
                sqlCon.Open();

            cmd.ExecuteNonQuery();

            sqlCon.Close();
        }





        #region to DataTable convertor
        DataTable ToDataTable<T>(List<T> items, string[] properties)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            Props = Props.Where(w => properties.Contains(w.Name)).ToArray();

            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                                ? Nullable.GetUnderlyingType(prop.PropertyType)
                                : prop.PropertyType
                           );
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
        #endregion
    }
}