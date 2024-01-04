using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace book_manager
{
    internal class OperateDataBase
    {
        /// <summary>
        /// read DataBase and output dt
        /// </summary>
        public static DataTable ConnectDB(string sqlText)
        {
            // DataTable create
            DataTable dt = new();

            // get connectionstring from App.config file
            string connectionString = ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString;

            // connection instance generation
            using SqlConnection connection = new(connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            try
            {
                // start connect DB
                connection.Open();

                // set table information in DataTable with SqlDataAdapter.Fill
                cmd.CommandText = sqlText;
                SqlDataAdapter adapter = new(cmd);
                adapter.Fill(dt);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }
            finally
            {
                // close db connection
                connection.Close();
            }
            return dt;
        }
    }
}