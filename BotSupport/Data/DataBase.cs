using System.Data;
using System.Data.SqlClient;


namespace BotSupport
{
    public class DataBase
    {
        public SqlConnection CurrentSqlConnection { get; private set; } = null;
        private const string DataBaseServerName = "(localdb)\\MSSQLLocalDB";
        private const string DataBaseName = "Bot";
        
        public DataBase()
        {
            CurrentSqlConnection = new SqlConnection($"Data Source={DataBaseServerName};Initial Catalog={DataBaseName};Integrated Security=True");
        }

        public void Open()
        {
            if(CurrentSqlConnection.State == ConnectionState.Closed)
            {
                CurrentSqlConnection.Open();
                Console.WriteLine($"Connection opened. Status: {CurrentSqlConnection.State}");
            }
        }
        
        public void CloseConnection()
        {
            if(CurrentSqlConnection.State == ConnectionState.Open)
            {
                CurrentSqlConnection.Close();
                Console.WriteLine($"Connection closed. Status: {CurrentSqlConnection.State}");
            }
        }
    }
}
