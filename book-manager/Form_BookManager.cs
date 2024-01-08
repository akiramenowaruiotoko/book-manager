using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace book_manager
{
    public partial class Form_BookManager : Form
    {
        // SQL Serverの接続文字列
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString;

        // データベース管理用のオブジェクト
        private readonly DatabaseManager databaseManager;

        // 変更前のIDを保持するためのディクショナリ
        private readonly Dictionary<string, string> originalIds = [];

        // コンストラクタ
        public Form_BookManager()
        {
            InitializeComponent();
            // データベースマネージャのインスタンス化
            databaseManager = new DatabaseManager(connectionString);
            // フォームを初期化してデータを表示
            DisplayData();
            // no列を編集不可に設定
            dataGridView1.Columns["no"].ReadOnly = true;

        }

        // データを表示するメソッド
        private void DisplayData()
        {
            try
            {
                // データベースから"basic_information"テーブルのデータを取得
                DataTable dataTable = databaseManager.SelectAllFromTable("basic_information");

                // 変更前のIDを保持
                originalIds.Clear();
                foreach (DataRow row in dataTable.Rows)
                {
                    originalIds.Add(row["id", DataRowVersion.Original].ToString()!, row["id", DataRowVersion.Original].ToString()!);
                }

                // 取得したデータをDataGridViewに表示
                dataGridView1.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("データの表示中にエラーが発生しました: " + ex.Message);
            }
        }

        // リロードボタンのクリックイベントハンドラ
        private void Button_Reload_Click(object sender, EventArgs e)
        {
            // データの再読み込み
            DisplayData();
        }

        // 保存ボタンのクリックイベントハンドラ
        private void Botton_Save_Click(object sender, EventArgs e)
        {
            try
            {
                // DataGridViewの変更を取得
                var modifiedData = ((DataTable)dataGridView1.DataSource).GetChanges();

                // 変更がある場合の処理
                if (modifiedData != null)
                {
                    foreach (DataRow row in modifiedData.Rows)
                    {
                        // 行の状態によって処理を分岐
                        if (row.RowState == DataRowState.Added)
                            InsertData(row);
                        else if (row.RowState == DataRowState.Modified)
                            UpdateData(row);
                        else if (row.RowState == DataRowState.Deleted)
                            DeleteData(row);
                    }
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

        // 新しいデータを挿入するメソッド
        private void InsertData(DataRow newRow)
        {
            try
            {
                // 新しいデータの挿入クエリを生成して実行
                string insertQuery = DatabaseManager.GenerateInsertQuery(newRow, "basic_information");
                databaseManager.InsertRowToTable(newRow, insertQuery);

                // データベースへの変更を確定し、メッセージを表示
                ((DataTable)dataGridView1.DataSource).AcceptChanges();
                MessageBox.Show("新しいデータがデータベースに挿入されました。");
            }
            catch (Exception ex)
            {
                MessageBox.Show("データベースへの挿入中にエラーが発生しました: " + ex.Message);
            }
        }

        // データの更新メソッド
        private void UpdateData(DataRow updatedRow)
        {
            try
            {
                // 変更前のIDを取得
                string originalId = updatedRow["id", DataRowVersion.Original].ToString()!;
                string? currentId = updatedRow["id", DataRowVersion.Current].ToString(); // idがnullの場合はcatchで処理

                // 変更前のIDを使用して更新クエリを生成
                string updateQuery = DatabaseManager.GenerateUpdateQuery(updatedRow, "basic_information", originalIds[originalId]);
                databaseManager.UpdateRowToTable(updatedRow, updateQuery);

                // データベースへの変更を確定し、メッセージを表示
                ((DataTable)dataGridView1.DataSource).AcceptChanges();
                MessageBox.Show("変更がデータベースに保存されました。");
            }
            catch (Exception ex)
            {
                MessageBox.Show("データベースへの更新中にエラーが発生しました: " + ex.Message);
            }
        }

        // データの削除メソッド
        private void DeleteData(DataRow deletedRow)
        {
            try
            {
                // 削除クエリを生成して実行
                string deleteQuery = DatabaseManager.GenerateDeleteQuery(deletedRow, "basic_information");
                databaseManager.DeleteRowFromTable(deletedRow, deleteQuery);

                // データベースへの変更を確定し、メッセージを表示
                ((DataTable)dataGridView1.DataSource).AcceptChanges();
                MessageBox.Show("データがデータベースから削除されました。");
            }
            catch (Exception ex)
            {
                MessageBox.Show("データベースからの削除中にエラーが発生しました: " + ex.Message);
            }
        }
    }

    // データベース操作用のクラス
    public class DatabaseManager(string connectionString)
    {
        // SQL Serverの接続文字列
        private readonly string connectionString = connectionString;

        // DBテーブルからすべてのデータを取得するメソッド
        public DataTable SelectAllFromTable(string tableName)
        {
            // SqlConnectionとSqlDataAdapterを使用してデータを取得
            using SqlConnection connection = new(connectionString);
            using SqlDataAdapter adapter = new($"SELECT ROW_NUMBER() OVER(ORDER BY id ASC) no, * FROM {tableName}", connection); // no列を追加
            DataTable dataTable = new();
            adapter.Fill(dataTable);
            return dataTable;
        }

        // 更新クエリを生成するメソッド
        public static string GenerateUpdateQuery(DataRow row, string tableName, string currentId)
        {
            // 更新クエリのSET句を生成 (No列は生成しない)
            string[] setClauses = new string[row.ItemArray.Length - 1];
            for (int i = 0; i < row.ItemArray.Length - 1; i++) 
                setClauses[i] = $"{row.Table.Columns[i + 1].ColumnName} = @Param{i}";

            // 更新クエリを返す
            return $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE id = '{currentId}'";
        }

        // 挿入クエリを生成するメソッド
        public static string GenerateInsertQuery(DataRow row, string tableName)
        {
            // 挿入クエリのカラムと値を生成 (No列は生成しない)
            string[] columns = new string[row.ItemArray.Length - 1];
            string[] values = new string[row.ItemArray.Length - 1];
            for (int i = 0; i < row.ItemArray.Length - 1; i++)
            {
                columns[i] = row.Table.Columns[i + 1].ColumnName;
                values[i] = $"@Param{i}";
            }

            // 挿入クエリを返す
            return $"INSERT INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
        }


        // 削除クエリを生成するメソッド
        public static string GenerateDeleteQuery(DataRow row, string tableName)
        {
            // 削除クエリを返す
            return $"DELETE FROM {tableName} WHERE id = @Param0";
        }

        // テーブルの行を更新するメソッド
        public bool UpdateRowToTable(DataRow row, string query)
        {
            // 更新クエリを実行
            using SqlConnection connection = new(connectionString);
            using SqlCommand cmd = new(query, connection);
            for (int i = 0; i < row.ItemArray.Length - 1; i++)
                cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);

            connection.Open();
            cmd.ExecuteNonQuery();

            return true;
        }

        // テーブルに新しい行を挿入するメソッド
        public bool InsertRowToTable(DataRow row, string query)
        {
            // 挿入クエリを実行
            using SqlConnection connection = new(connectionString);
            using SqlCommand cmd = new(query, connection);
            for (int i = 0; i < row.ItemArray.Length - 1; i++)
                cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);

            connection.Open();
            cmd.ExecuteNonQuery();

            return true;
        }

        // テーブルから行を削除するメソッド
        public bool DeleteRowFromTable(DataRow row, string query)
        {
            // 削除クエリを実行
            using SqlConnection connection = new(connectionString);
            using SqlCommand cmd = new(query, connection);
            cmd.Parameters.AddWithValue($"@Param0", row["id", DataRowVersion.Original]);

            connection.Open();
            cmd.ExecuteNonQuery();

            return true;
        }
    }
}
