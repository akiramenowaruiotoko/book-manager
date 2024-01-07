using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace book_manager
{
    public partial class Form_BookManager : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString;
        private DatabaseManager databaseManager;

        public Form_BookManager()
        {
            InitializeComponent();

            databaseManager = new DatabaseManager(connectionString);

            DisplayData();

            dataGridView1.Columns["id"].ReadOnly = true;
        }

        private void DisplayData()
        {
            try
            {
                DataTable dataTable = databaseManager.SelectAllFromTable("basic_information");
                dataGridView1.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("データの表示中にエラーが発生しました: " + ex.Message);
            }
        }

        private void Botton_SaveData_Click(object sender, EventArgs e)
        {
            try
            {
                var modifiedData = ((DataTable)dataGridView1.DataSource).GetChanges();

                if (modifiedData != null)
                {
                    foreach (DataRow row in modifiedData.Rows)
                    {
                        string updateQuery = databaseManager.GenerateUpdateQuery(row, "basic_information");
                        if (databaseManager.SaveRowToTable(row, updateQuery))
                        {
                            MessageBox.Show("変更がデータベースに保存されました。");
                        }
                    }

                    ((DataTable)dataGridView1.DataSource).AcceptChanges();
                }
                else
                {
                    MessageBox.Show("変更がありません。");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("データベースへの保存中にエラーが発生しました: " + ex.Message);
            }
        }
    }

    public class DatabaseManager
    {
        private string connectionString;

        public DatabaseManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public DataTable SelectAllFromTable(string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlDataAdapter adapter = new SqlDataAdapter($"SELECT * FROM {tableName}", connection))
            {
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        public string GenerateUpdateQuery(DataRow row, string tableName)
        {
            string[] setClauses = new string[row.ItemArray.Length];
            for (int i = 0; i < row.ItemArray.Length; i++)
            {
                setClauses[i] = $"{row.Table.Columns[i].ColumnName} = @Param{i}";
            }

            return $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE id = @Param0";
        }

        public bool SaveRowToTable(DataRow row, string query)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"@Param{i}", row[i]);
                }

                connection.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }
    }
}
