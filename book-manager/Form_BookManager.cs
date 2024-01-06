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
                MessageBox.Show("データの表示中にエラーが発生しました: " + ex.Message);
            }
        }

        private void Botton_SaveData_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable modifiedData = ((DataTable)dataGridView1.DataSource).GetChanges();

                if (modifiedData != null)
                {
                    foreach (DataRow row in modifiedData.Rows)
                    {
                        // データが挿入されるか、更新されるかの判定
                        databaseManager.SaveRowToTable(row, "basic_information");
                    }

                    // データソースに変更があったことを通知し、DataGridViewを更新
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

        // 他のメソッドやクラスの実装

        // カラムの型に応じてデフォルトの値を取得するメソッド
        private object GetDefaultValueForColumnType(DataGridViewColumn column)
        {
            Type columnType = column.ValueType;

            if (columnType == typeof(int))
            {
                return 0;
            }
            else if (columnType == typeof(string))
            {
                return string.Empty;
            }
            else if (columnType == typeof(DateTime))
            {
                return DateTime.Now;
            }
            // 他の型に対する処理も追加可能

            return DBNull.Value;
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

        public void SaveRowToTable(DataRow row, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // データが挿入されるか、更新されるかの判定
                string query = (row.RowState == DataRowState.Added)
                    ? GenerateInsertQuery(row, tableName)
                    : GenerateUpdateQuery(row, tableName);

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    // パラメータの追加
                    for (int i = 0; i < row.ItemArray.Length; i++)
                    {
                        cmd.Parameters.AddWithValue($"@Param{i}", row[i]);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string GenerateInsertQuery(DataRow row, string tableName)
        {
            // カラム数に合わせてパラメータを生成
            string[] paramNames = new string[row.ItemArray.Length];
            for (int i = 0; i < row.ItemArray.Length; i++)
            {
                paramNames[i] = $"@Param{i}";
            }

            // カラム名も指定して、パラメータを含む INSERT クエリを生成
            string[] columnNames = row.Table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray();

            // VALUES 句に含まれる値の数を、列の数に合わせる
            string valuesClause = string.Join(", ", paramNames);

            return $"INSERT INTO {tableName} ({string.Join(", ", columnNames)}) VALUES ({valuesClause})";
        }

        private string GenerateUpdateQuery(DataRow row, string tableName)
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
    }
}
