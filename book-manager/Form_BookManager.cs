using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace book_manager
{
    public partial class Form_BookManager : Form
    {
        // データベース接続文字列を取得
        private string connectionString = ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString;

        // データベースマネージャーのインスタンスを作成
        private DatabaseManager databaseManager;

        // フォームのコンストラクタ
        public Form_BookManager()
        {
            InitializeComponent();

            // データベースマネージャーのインスタンスを作成
            databaseManager = new DatabaseManager(connectionString);

            // フォームがロードされたときにデータを表示
            DisplayData();
        }

        // データを表示するメソッド
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

        // 保存ボタンがクリックされたときのイベントハンドラ
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
                        // 新しい行か既存の行かを判定
                        if (row.RowState == DataRowState.Added)
                        {
                            // 新しい行の場合、挿入メソッドを呼び出し
                            InsertData(row);
                        }
                        else if (row.RowState == DataRowState.Modified)
                        {
                            // 既存の行の場合、UPDATE クエリを生成してデータベースに保存
                            string updateQuery = databaseManager.GenerateUpdateQuery(row, "basic_information");
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

        // 新しいデータを挿入するメソッド
        private void InsertData(DataRow newRow)
        {
            try
            {
                // INSERT クエリを生成
                string insertQuery = databaseManager.GenerateInsertQuery(newRow, "basic_information");

                // データベースに新しい行を挿入
                if (databaseManager.InsertRowToTable(newRow, insertQuery))
                {
                    // 挿入が成功した場合、成功メッセージを表示
                    MessageBox.Show("新しいデータがデータベースに挿入されました。");
                }

                // データソースに変更があったことを通知し、DataGridViewを更新
                ((DataTable)dataGridView1.DataSource).AcceptChanges();
            }
            catch (Exception ex)
            {
                // エラーハンドリングの強化
                MessageBox.Show("データベースへの挿入中にエラーが発生しました: " + ex.Message);
            }
        }

        private void Button_Reload_Click(object sender, EventArgs e)
        {
            DisplayData();
        }
    }

    public class DatabaseManager
    {
        // データベース接続文字列
        private string connectionString;

        // コンストラクタ
        public DatabaseManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        // テーブルからすべてのデータを取得するメソッド
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

        // UPDATE クエリを生成するメソッド
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

        // INSERT クエリを生成するメソッド
        public string GenerateInsertQuery(DataRow row, string tableName)
        {
            // カラム数に合わせて INSERT クエリを生成
            string[] columns = new string[row.ItemArray.Length];
            string[] values = new string[row.ItemArray.Length];
            for (int i = 0; i < row.ItemArray.Length; i++)
            {
                columns[i] = row.Table.Columns[i].ColumnName;
                values[i] = $"@Param{i}";
            }

            // パラメータを含む INSERT クエリを生成
            return $"INSERT INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
        }

        // 行をデータベースに保存するメソッド
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

        // 新しい行をデータベースに挿入するメソッド
        public bool InsertRowToTable(DataRow row, string query)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                // パラメータの追加
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"@Param{i}", row[i]);
                }

                // データベースに新しい行を挿入
                connection.Open();
                cmd.ExecuteNonQuery();
            }

            return true; // 挿入が成功したことを示す
        }
    }
}
