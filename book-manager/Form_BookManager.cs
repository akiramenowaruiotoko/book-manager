using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Windows.Forms;

namespace book_manager
{
    public partial class Form_BookManager : Form
    {
        private readonly DatabaseManager databaseManager;
        private readonly StringBuilder sql = new();
        private readonly string[] tableNames = { "basic_information", "rental_information" };

        public Form_BookManager()
        {
            InitializeComponent();
            // データベースマネージャーの初期化
            databaseManager = new DatabaseManager(ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString, tableNames);
            comboBox_tableName.SelectedIndex = 0;

            // 初期化メソッドの呼び出し
            Initialize();
        }

        private void Initialize()
        {
            // SQLクエリの基本構造を構築
            BuildBaseQuery();
            // FROM句の初期設定
            sql.AppendLine("FROM");
            sql.AppendLine(tableNames[0] + " as b");

            // データの表示
            DisplayData();

            // "No" 列を読み取り専用に設定
            dataGridView1.Columns["No"].ReadOnly = true;
        }

        private void BuildBaseQuery()
        {
            // SELECT句の基本構造を構築
            sql.Clear();
            sql.AppendLine("SELECT");
            sql.AppendLine("ROW_NUMBER() OVER(ORDER BY b.id ASC) No");
            sql.AppendLine(", b.id");
            sql.AppendLine(", b.book_name");
            sql.AppendLine(", price");
        }

        private void ComboBox_tableName_SelectedIndexChanged(object sender, EventArgs e)
        {
            // SQLクエリを再構築してデータを表示
            RebuildQueryAndDisplayData();
        }

        private void RebuildQueryAndDisplayData()
        {
            sql.Clear();
            string? selectedTable = comboBox_tableName.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTable))
            {
                // 無効な選択を処理
                return;
            }

            switch (selectedTable)
            {
                case "basic":
                    // 基本テーブルのクエリ構築
                    BuildBaseQuery();
                    sql.AppendLine("FROM");
                    sql.AppendLine(tableNames[0] + " as b");
                    break;
                case "basic + rental":
                    // 基本とレンタル情報を結合したクエリ構築
                    BuildBaseQuery();
                    sql.AppendLine(", r.name");
                    sql.AppendLine(", r.loan_date");
                    sql.AppendLine("FROM");
                    sql.AppendLine(tableNames[0] + " as b");
                    sql.AppendLine("LEFT JOIN");
                    sql.AppendLine(tableNames[1] + " as r ON b.id = r.id");
                    break;
                default:
                    throw new ArgumentException("Unsupported selectedTable");
            }
            // データの再表示
            DisplayData();
        }

        private void Button_Reload_Click(object sender, EventArgs e)
        {
            // データの再表示
            DisplayData();
        }

        private void DisplayData()
        {
            try
            {
                // データベースからデータを取得して表示
                DataTable dataTable = databaseManager.SelectFromTable(sql.ToString());
                dataGridView1.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("データの表示中にエラーが発生しました: " + ex.Message);
            }
        }

        private void Button_Save_Click(object sender, EventArgs e)
        {
            try
            {
                // 変更をデータベースに保存
                SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("データベースへの保存中にエラーが発生しました: " + ex.Message);
            }
        }

        private void SaveChanges()
        {
            try
            {
                // 変更されたデータを取得
                var modifiedData = ((DataTable)dataGridView1.DataSource).GetChanges();

                if (modifiedData == null)
                {
                    MessageBox.Show("変更がありません。");
                    return;
                }

                // 変更データの処理
                ProcessModifiedData(modifiedData);
                ((DataTable)dataGridView1.DataSource).AcceptChanges();
                MessageBox.Show("変更がデータベースに保存されました。");
            }
            catch (Exception ex)
            {
                MessageBox.Show("データベースへの保存中にエラーが発生しました: " + ex.Message);
            }
        }

        private void ProcessModifiedData(DataTable modifiedData)
        {
            // 各行の処理
            foreach (DataRow row in modifiedData.Rows)
            {
                databaseManager.ManageRow(row);
            }
        }
    }
    public class DatabaseManager
    {
        public readonly string ConnectionString;
        private readonly string[] tableNames;

        public DatabaseManager(string connectionString, string[] tableNames)
        {
            ConnectionString = connectionString;
            this.tableNames = tableNames;
        }

        public DataTable SelectFromTable(string sql)
        {
            using SqlConnection connection = new SqlConnection(ConnectionString);
            using SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
            using DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);
            return dataTable;
        }

        public void ManageRow(DataRow row)
        {
            using SqlConnection connection = new SqlConnection(ConnectionString);
            using SqlCommand cmd = new SqlCommand(null, connection);
            connection.Open();
            switch (row.RowState)
            {
                case DataRowState.Added:
                    ConfigureInsertCommand(cmd, row);
                    break;

                case DataRowState.Modified:
                    ConfigureUpdateCommand(cmd, row);
                    break;

                case DataRowState.Deleted:
                    ConfigureDeleteCommand(cmd, row);
                    break;

                default:
                    throw new ArgumentException("Unsupported DataRowState");
            }
        }



        private void ConfigureInsertCommand(SqlCommand cmd, DataRow row)
        {
            cmd.Parameters.Clear();

            string[] columns;
            string[] values;

            // Extract common logic for basic insertion
            void ConfigureBasicInsertion(int endIndex)
            {
                columns = new string[endIndex];
                values = new string[endIndex];

                for (int i = 0; i < endIndex; i++)
                {
                    columns[i] = row.Table.Columns[i + 1].ColumnName;
                    values[i] = $"@Param{i}";
                    cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);
                }

                cmd.CommandText = $"INSERT INTO {tableNames[0]} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
                cmd.ExecuteNonQuery();
            }

            // Basic only
            if (row.ItemArray.Length == 4)
            {
                ConfigureBasicInsertion(row.ItemArray.Length - 1);
            }
            // Basic + rental
            else
            {
                // Basic
                if (row.RowState == DataRowState.Added)
                {
                    ConfigureBasicInsertion(row.ItemArray.Length - 3);
                }

                // Rental data existence check
                if (row["name"] == DBNull.Value)
                {
                    return;
                }

                // Rental
                cmd.Parameters.Clear();
                columns = new string[row.ItemArray.Length - 3];
                values = new string[row.ItemArray.Length - 3];

                for (int i = 0; i < row.ItemArray.Length - 1; i++)
                {
                    if (i == 0)
                    {
                        columns[i] = row.Table.Columns[i + 1].ColumnName;
                        values[i] = $"@Param{i}";
                        cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);
                    }
                    else if (i >= 3)
                    {
                        columns[i - 2] = row.Table.Columns[i + 1].ColumnName;
                        values[i - 2] = $"@Param{i - 2}";
                        cmd.Parameters.AddWithValue($"@Param{i - 2}", row[i + 1]);
                    }
                }

                cmd.CommandText = $"INSERT INTO {tableNames[1]} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
                cmd.ExecuteNonQuery();
            }
        }









        private void ConfigureUpdateCommand(SqlCommand cmd, DataRow row)
        {
            cmd.Parameters.AddWithValue("@ParamID", row["id", DataRowVersion.Original]);

            if (row.ItemArray.Length == 6)
            {
                ConfigureUpdateBasicInfo(cmd, row);

                if (IsRentalInfoEmpty(row))
                {
                    // レンタル情報が空の場合、挿入
                    ConfigureInsertCommand(cmd, row);
                }
                else if (row["name"] == DBNull.Value)
                {
                    // レンタル情報が削除された場合、削除
                    cmd.CommandText = $"DELETE FROM {tableNames[1]} WHERE id = @ParamID";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    ConfigureUpdateRentalInfo(cmd, row);
                }
            }
            else
            {
                ConfigureUpdateBasicInfo(cmd, row);
            }
        }

        private void ConfigureUpdateBasicInfo(SqlCommand cmd, DataRow row)
        {
            string[] setClauses = new string[row.ItemArray.Length - 3];
            for (int i = 0; i < row.ItemArray.Length - 3; i++)
            {
                setClauses[i] = $"{row.Table.Columns[i + 1].ColumnName} = @Param{i}";
                cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);
            }
            cmd.CommandText = $"UPDATE {tableNames[0]} SET {string.Join(", ", setClauses)} WHERE id = @ParamID";
            cmd.ExecuteNonQuery();
        }

        private void ConfigureUpdateRentalInfo(SqlCommand cmd, DataRow row)
        {
            string[] setClauses = new string[row.ItemArray.Length - 3];
            cmd.Parameters.Clear();

            cmd.Parameters.AddWithValue("@ParamID", row["id", DataRowVersion.Original]);
            for (int i = 0; i < row.ItemArray.Length - 1; i++)
            {
                if (i == 0)
                {
                    setClauses[i] = $"{row.Table.Columns[i + 1].ColumnName} = @Param{i}";
                    cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);
                }
                else if (i >= 3)
                {
                    setClauses[i - 2] = $"{row.Table.Columns[i + 1].ColumnName} = @Param{i - 2}";
                    cmd.Parameters.AddWithValue($"@Param{i - 2}", row[i + 1]);
                }
            }
            cmd.CommandText = $"UPDATE {tableNames[1]} SET {string.Join(", ", setClauses)} WHERE id = @ParamID";
            cmd.ExecuteNonQuery();
        }

        private bool IsRentalInfoEmpty(DataRow row)
        {
            return (row["name", DataRowVersion.Original] == DBNull.Value) && (row["loan_date", DataRowVersion.Original] == DBNull.Value);
        }

        private void ConfigureDeleteCommand(SqlCommand cmd, DataRow row)
        {
            cmd.Parameters.AddWithValue("@ParamID", row["id", DataRowVersion.Original]);

            // レンタル情報の削除
            cmd.CommandText = $"DELETE FROM {tableNames[1]} WHERE id = @ParamID";
            cmd.ExecuteNonQuery();

            // 基本情報の削除
            cmd.CommandText = $"DELETE FROM {tableNames[0]} WHERE id = @ParamID";
            cmd.ExecuteNonQuery();
        }
    }
}
