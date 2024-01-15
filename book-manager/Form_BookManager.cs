using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;

namespace book_manager
{
    public partial class Form_BookManager : Form
    {
        private readonly DatabaseManager databaseManager;
        private readonly StringBuilder sql = new();
        private readonly string[] tableNames = { "basic_information", "rental_information"};

        public Form_BookManager()
        {
            InitializeComponent();
            databaseManager = new DatabaseManager(ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString, tableNames);
            comboBox_tableName.SelectedIndex = 0;

            BuildBaseQuery();
            sql.AppendLine("FROM");
            sql.AppendLine(tableNames[0] + " as b");
            DisplayData();
            dataGridView1.Columns["No"].ReadOnly = true;
        }

        private void BuildBaseQuery()
        {
            sql.Clear();
            sql.AppendLine("SELECT");
            sql.AppendLine("ROW_NUMBER() OVER(ORDER BY b.id ASC) No");
            sql.AppendLine(", b.id");
            sql.AppendLine(", b.book_name");
            sql.AppendLine(", price");
        }

        private void ComboBox_tableName_SelectedIndexChanged(object sender, EventArgs e)
        {
            sql.Clear();
            string? selectedTable = comboBox_tableName.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTable))
            {
                // Handle invalid selection
                return;
            }

            switch (selectedTable)
            {
                case "basic":
                    BuildBaseQuery();
                    sql.AppendLine("FROM");
                    sql.AppendLine(tableNames[0] + " as b");
                    break;
                case "basic + rental":
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
            DisplayData();
        }

        private void Button_Reload_Click(object sender, EventArgs e)
        {
            DisplayData();
        }

        private void DisplayData()
        {
            try
            {
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
                SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("データベースへの保存中にエラーが発生しました: " + ex.Message);
            }
        }

        private void SaveChanges()
        {
            var modifiedData = ((DataTable)dataGridView1.DataSource).GetChanges();

            if (modifiedData == null)
            {
                MessageBox.Show("変更がありません。");
                return;
            }

            foreach (DataRow row in modifiedData.Rows)
            {
                databaseManager.ManageRow(row);
            }
            ((DataTable)dataGridView1.DataSource).AcceptChanges();
            MessageBox.Show("変更がデータベースに保存されました。");
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
            using SqlConnection connection = new(ConnectionString);
            using SqlDataAdapter adapter = new(sql.ToString(), connection);
            using DataTable dataTable = new();
            adapter.Fill(dataTable);
            return dataTable;
        }

        public void ManageRow(DataRow row)
        {
            using SqlConnection connection = new(ConnectionString);
            using SqlCommand cmd = new(null, connection);
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
                    // Handle other cases or throw an exception if needed
                    throw new ArgumentException("Unsupported DataRowState");
            }
        }

        private void ConfigureInsertCommand(SqlCommand cmd, DataRow row)
        {
            cmd.Parameters.Clear();
            string[] columns = new string[row.ItemArray.Length - 1];
            string[] values = new string[row.ItemArray.Length - 1];

            // basic only
            if (row.ItemArray.Length == 4)
            {
                for (int i = 0; i < row.ItemArray.Length - 1; i++)
                {
                    columns[i] = row.Table.Columns[i + 1].ColumnName;
                    values[i] = $"@Param{i}";
                    cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);
                }
                cmd.CommandText = $"INSERT INTO {tableNames[0]} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
                cmd.ExecuteNonQuery();
            }
            // basic + rental
            else
            {
                cmd.Parameters.Clear();
                columns = new string[row.ItemArray.Length - 3];
                values = new string[row.ItemArray.Length - 3];

                // basic
                if (row.RowState == DataRowState.Added)
                {
                    for (int i = 0; i < row.ItemArray.Length - 3; i++)
                    {
                        columns[i] = row.Table.Columns[i + 1].ColumnName;
                        values[i] = $"@Param{i}";
                        cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);
                    }
                    cmd.CommandText = $"INSERT INTO {tableNames[0]} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
                    cmd.ExecuteNonQuery();
                }

                // rental data exist check
                if ((row["name"] == DBNull.Value))
                {
                    return;
                }

                // rental
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
                    }else if(i >= 3)
                    {
                        columns[i-2] = row.Table.Columns[i + 1].ColumnName;
                        values[i-2] = $"@Param{i-2}";
                        cmd.Parameters.AddWithValue($"@Param{i-2}", row[i + 1]);
                    }
                }
                cmd.CommandText = $"INSERT INTO {tableNames[1]} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
                cmd.ExecuteNonQuery();
            }
        }

        private void ConfigureUpdateCommand(SqlCommand cmd, DataRow row)
        {
            string[] setClauses = new string[row.ItemArray.Length - 1];
            cmd.Parameters.AddWithValue("@ParamID", row["id", DataRowVersion.Original]);

            // basic + rental
            if ( row.ItemArray.Length == 6)
            {
                setClauses = new string[row.ItemArray.Length - 3];
                // basic
                for (int i = 0; i < row.ItemArray.Length - 3; i++)
                {
                    setClauses[i] = $"{row.Table.Columns[i + 1].ColumnName} = @Param{i}";
                    cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);
                }
                cmd.CommandText = $"UPDATE {tableNames[0]} SET {string.Join(", ", setClauses)} WHERE id = @ParamID";
                cmd.ExecuteNonQuery();

                // rental exist check
                if ((row["name", DataRowVersion.Original] == DBNull.Value) && (row["loan_date", DataRowVersion.Original] == DBNull.Value))
                {
                    // insert rental
                    ConfigureInsertCommand(cmd, row);
                }
                // rental delete check
                else if (row["name"] == DBNull.Value )
                {
                    cmd.CommandText = $"DELETE FROM {tableNames[1]} WHERE id = @ParamID";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    // reset
                    setClauses = new string[row.ItemArray.Length - 3];
                    cmd.Parameters.Clear();

                    // rental
                    cmd.Parameters.AddWithValue("@ParamID", row["id", DataRowVersion.Original]);
                    for (int i = 0; i < row.ItemArray.Length - 1; i++)
                    {
                        if (i == 0)
                        {
                            setClauses[i] = $"{row.Table.Columns[i + 1].ColumnName} = @Param{i}";
                            cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);
                        }else if(i >= 3)
                        {
                            setClauses[i-2] = $"{row.Table.Columns[i+1].ColumnName} = @Param{i-2}";
                            cmd.Parameters.AddWithValue($"@Param{i-2}", row[i+1]);
                        }
                    }
                    cmd.CommandText = $"UPDATE {tableNames[1]} SET {string.Join(", ", setClauses)} WHERE id = @ParamID";
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                for (int i = 0; i < row.ItemArray.Length - 1; i++)
                {
                    setClauses[i] = $"{row.Table.Columns[i + 1].ColumnName} = @Param{i}";
                    cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);
                }
                cmd.CommandText = $"UPDATE {tableNames[0]} SET {string.Join(", ", setClauses)} WHERE id = @ParamID";
                cmd.ExecuteNonQuery();
            }
        }

        private void ConfigureDeleteCommand(SqlCommand cmd, DataRow row)
        {
            cmd.Parameters.AddWithValue("@ParamID", row["id", DataRowVersion.Original]);
            
            cmd.CommandText = $"DELETE FROM {tableNames[1]} WHERE id = @ParamID";
            cmd.ExecuteNonQuery();

            cmd.CommandText = $"DELETE FROM {tableNames[0]} WHERE id = @ParamID";
            cmd.ExecuteNonQuery();
        }
    }
}