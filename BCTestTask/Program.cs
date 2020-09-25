using Newtonsoft.Json;
using System;
using System.Data;

namespace BCTestTask
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Checking arguments
                if (args.Length < 4)
                    throw new Exception("3 arguments should be provided: server name,database name,table name,file path");
                string ServerName = args[0];
                string DatabaseName = args[1];
                string TableName = args[2];
                string FilePath = args[3];
                string ConnStr = $"Server={ServerName};Database={DatabaseName};Trusted_Connection=True;";
                using DB dB = new DB(ConnStr);

                //Check if the table exists
                if (!dB.CheckIfTableExists(TableName))
                    throw new Exception($"The table {TableName} has not been found in the database.");

                //Check if the file exists
                LogFile logFile = new LogFile(FilePath, TableName);

                //Check if CDC instance exists and get lsnFrom
                string lsnFrom = dB.GetFirstLSN(TableName);
                if (String.IsNullOrEmpty(lsnFrom))
                    throw new Exception($"The CDC instance is not enable for {TableName}.");

                if (logFile.LSN != String.Empty)
                    lsnFrom = logFile.LSN;


                //Get last LSN
                string lsnTo = dB.GetLastLSN();

                //Get CDC feed
                DataTable cdc = dB.GetCDCFeed(TableName, lsnFrom, lsnTo);
                logFile.Save(JsonConvert.SerializeObject(cdc), lsnTo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
