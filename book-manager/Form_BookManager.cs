using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace book_manager
{
    public partial class Form_BookManager : Form
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString;
        private readonly DatabaseManager databaseManager;

        public Form_BookManager()
        {
            InitializeComponent();
            databaseManager = new DatabaseManager(connectionString);
            DisplayData();
        }

        private void DisplayData()
        {
            try
            {
                dataGridView1.DataSource = databaseManager.SelectAllFromTable("basic_information");
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
                        if (row.RowState == DataRowState.Added)
                            InsertData(row);
                        else if (row.RowState == DataRowState.Modified)
                            UpdateData(row);
                        else if (row.RowState == DataRowState.Deleted)
                            DeleteData(row);
                    }

                    ((DataTable)dataGridView1.DataSource).AcceptChanges();
                    MessageBox.Show("変更がデータベースに保存されました。");
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

        private void InsertData(DataRow newRow)
        {
            try
            {
                string insertQuery = databaseManager.GenerateInsertQuery(newRow, "basic_information");
                databaseManager.InsertRowToTable(newRow, insertQuery);

                ((DataTable)dataGridView1.DataSource).AcceptChanges();
                MessageBox.Show("新しいデータがデータベースに挿入されました。");
            }
            catch (Exception ex)
            {
                MessageBox.Show("データベースへの挿入中にエラーが発生しました: " + ex.Message);
            }
        }

        private void UpdateData(DataRow updatedRow)
        {
            try
            {
                string updateQuery = databaseManager.GenerateUpdateQuery(updatedRow, "basic_information");
                databaseManager.SaveRowToTable(updatedRow, updateQuery);
            }
            catch (Exception ex)
            {
                MessageBox.Show("データベースへの更新中にエラーが発生しました: " + ex.Message);
            }
        }

        private void DeleteData(DataRow deletedRow)
        {
            try
            {
                string deleteQuery = databaseManager.GenerateDeleteQuery(deletedRow, "basic_information");
                databaseManager.DeleteRowFromTable(deletedRow, deleteQuery);

                ((DataTable)dataGridView1.DataSource).AcceptChanges();
                MessageBox.Show("データがデータベースから削除されました。");
            }
            catch (Exception ex)
            {
                MessageBox.Show("データベースからの削除中にエラーが発生しました: " + ex.Message);
            }
        }

        private void Form_BookManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            ApplyChangesToDatabase();
        }

        private void ApplyChangesToDatabase()
        {
            try
            {
                var modifiedData = ((DataTable)dataGridView1.DataSource).GetChanges();

                if (modifiedData != null)
                {
                    foreach (DataRow row in modifiedData.Rows)
                    {
                        if (row.RowState == DataRowState.Added)
                            InsertData(row);
                        else if (row.RowState == DataRowState.Modified)
                            UpdateData(row);
                        else if (row.RowState == DataRowState.Deleted)
                            DeleteData(row);
                    }

                    ((DataTable)dataGridView1.DataSource).AcceptChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("データベースへの反映中にエラーが発生しました: " + ex.Message);
            }
        }

        private void Button_Reload_Click(object sender, EventArgs e)
        {
            DisplayData();
        }
    }

    public class DatabaseManager
    {
        private readonly string connectionString;

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
                setClauses[i] = $"{row.Table.Columns[i].ColumnName} = @Param{i}";

            return $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE id = @Param0";
        }

        public string GenerateInsertQuery(DataRow row, string tableName)
        {
            string[] columns = new string[row.ItemArray.Length];
            string[] values = new string[row.ItemArray.Length];
            for (int i = 0; i < row.ItemArray.Length; i++)
            {
                columns[i] = row.Table.Columns[i].ColumnName;
                values[i] = $"@Param{i}";
            }

            return $"INSERT INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
        }

        public string GenerateDeleteQuery(DataRow row, string tableName)
        {
            return $"DELETE FROM {tableName} WHERE id = @Param0";
        }

        public bool SaveRowToTable(DataRow row, string query)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                    cmd.Parameters.AddWithValue($"@Param{i}", row[i]);

                connection.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        public bool InsertRowToTable(DataRow row, string query)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                for (int i = 0; i < row.ItemArray.Length; i++)
                    cmd.Parameters.AddWithValue($"@Param{i}", row[i]);

                connection.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        public bool DeleteRowFromTable(DataRow row, string query)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue($"@Param0", row["id", DataRowVersion.Original]);

                connection.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }
    }
}
