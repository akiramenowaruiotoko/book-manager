using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

            // フォームがロードされたときにデータを表示
            DisplayData();
        }

        private void DisplayData()
        {
            try
            {
                DataTable dataTable = databaseManager.SelectAllFromTableWithRowNumber("basic_information");

                // DataGridViewにデータを設定
                dataGridView1.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                // エラーハンドリングの強化
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

                        if (updateQuery != null)
                        {
                            if (databaseManager.SaveRowToTable(row, updateQuery))
                            {
                                // 更新が成功した場合、成功メッセージを表示
                                MessageBox.Show("変更がデータベースに保存されました。");
                            }
                        }
                    }

                    // データソースに変更があったことを通知し、DataGridViewを更新
                    ((DataTable)dataGridView1.DataSource).AcceptChanges();
                }
                else
                {
                    MessageBox.Show("変更がありません。");
                }
            }
            catch (Exception ex)
            {
                // エラーハンドリングの強化
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

        public DataTable SelectAllFromTableWithRowNumber(string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // ROW_NUMBER() を使用したクエリ
                string query = $"SELECT * FROM {tableName}";

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    return dataTable;
                }
            }
        }

        public string GenerateUpdateQuery(DataRow row, string tableName)
        {
            // カラム数に合わせて SET 句を生成
            string[] setClauses = new string[row.ItemArray.Length];
            for (int i = 0; i < row.ItemArray.Length; i++)
            {
                setClauses[i] = $"{row.Table.Columns[i].ColumnName} = @Param{i}";
            }

            // IDが変更された場合にメッセージを表示
            if (row.RowState != DataRowState.Added && row["id", DataRowVersion.Original].ToString() != row["id"].ToString())
            {
                MessageBox.Show("IDは変更できません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null; // メッセージを表示しているため、クエリは実行しない
            }

            // パラメータを含む UPDATE クエリを生成
            return $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE id = @Param0";
        }

        public bool SaveRowToTable(DataRow row, string query)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                if (query != null) // クエリがnullでない場合に実行
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        // パラメータの追加
                        for (int i = 0; i < row.ItemArray.Length; i++)
                        {
                            cmd.Parameters.AddWithValue($"@Param{i}", row[i]);
                        }

                        cmd.ExecuteNonQuery();
                    }

                    return true; // 更新が成功したことを示す
                }

                return false; // 更新が成功しなかったことを示す
            }
        }
    }
}
