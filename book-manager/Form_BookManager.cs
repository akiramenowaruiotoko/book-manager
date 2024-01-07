using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace book_manager
{
    public partial class Form_BookManager : Form
    {
        // 接続文字列を設定
        private string connectionString = ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString;
        private DatabaseManager databaseManager;

        public Form_BookManager()
        {
            InitializeComponent();

            // データベースマネージャーのインスタンスを作成
            databaseManager = new DatabaseManager(connectionString);

            // フォームがロードされたときにデータを表示
            DisplayData();

            // ID列を読み取り専用に設定
            dataGridView1.Columns["id"].ReadOnly = true;
        }

        private void DisplayData()
        {
            try
            {
                // "basic_information" テーブルからデータを取得
                DataTable dataTable = databaseManager.SelectAllFromTable("basic_information");

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
                // 変更されたデータを取得
                var modifiedData = ((DataTable)dataGridView1.DataSource).GetChanges();

                if (modifiedData != null)
                {
                    foreach (DataRow row in modifiedData.Rows)
                    {
                        // UPDATE クエリを生成
                        string updateQuery = databaseManager.GenerateUpdateQuery(row, "basic_information");

                        // データベースに行を保存
                        if (databaseManager.SaveRowToTable(row, updateQuery))
                        {
                            // 更新が成功した場合、成功メッセージを表示
                            MessageBox.Show("変更がデータベースに保存されました。");
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

        public DataTable SelectAllFromTable(string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlDataAdapter adapter = new SqlDataAdapter($"SELECT * FROM {tableName}", connection))
            {
                DataTable dataTable = new DataTable();

                // データベースからデータを取得してDataTableに充填
                adapter.Fill(dataTable);

                return dataTable;
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

            // パラメータを含む UPDATE クエリを生成
            return $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE id = @Param0";
        }

        public bool SaveRowToTable(DataRow row, string query)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                // パラメータの追加
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"@Param{i}", row[i]);
                }

                // データベースに行を保存
                connection.Open();
                cmd.ExecuteNonQuery();
            }

            return true; // 更新が成功したことを示す
        }
    }
}
