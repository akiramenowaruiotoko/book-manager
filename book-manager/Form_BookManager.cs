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
        private readonly string[] tableNames = ["basic_information", "rental_information", "purchase_information"];

        public Form_BookManager()
        {
            InitializeComponent();
            databaseManager = new DatabaseManager(ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString, tableNames);
            DisplayData();
            dataGridView1.Columns["no"].ReadOnly = true;
            comboBox_tableName.SelectedIndex = 0;
        }

        private void ComboBox_tableName_SelectedIndexChanged(object sender, EventArgs e)
        {
            string? selectedTable = comboBox_tableName.SelectedItem!.ToString();
            switch (selectedTable)
            {
                case "basic":
                    break;
                case "basic + price":
                    break;
                case "basic + rental":
                    break;
                case "basic + price + purchase":
                    break;
                case "all information":
                    break;
                default: throw new ArgumentException("Unsupported selectedTable");
            }
            sql.AppendLine("SELECT");

        }
        private void Button_Reload_Click(object sender, EventArgs e)
        {
            DisplayData();
        }

        private void DisplayData()
        {
            try
            {
                DataTable dataTable = databaseManager.SelectAllFromTable();
                dataGridView1.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("データの表示中にエラーが発生しました: " + ex.Message);
            }
        }

        private void Botton_Save_Click(object sender, EventArgs e)
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

    public class DatabaseManager(string connectionString, string[] tableNames)
    {
        public readonly string ConnectionString = connectionString;
        private readonly string[] tableNames = tableNames;

        public DataTable SelectAllFromTable()
        {
            using SqlConnection connection = new(ConnectionString);
            using SqlDataAdapter adapter = new($"SELECT ROW_NUMBER() OVER(ORDER BY id ASC) No, * FROM {tableNames[0]}", connection);
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
            cmd.ExecuteNonQuery();
        }

        private void ConfigureInsertCommand(SqlCommand cmd, DataRow row)
        {
            // no列は除外
            string[] columns = new string[row.ItemArray.Length - 1];
            string[] values = new string[row.ItemArray.Length - 1];

            for (int i = 0; i < row.ItemArray.Length - 1; i++)
            {
                columns[i] = row.Table.Columns[i + 1].ColumnName;
                values[i] = $"@Param{i}";
                cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);
            }

            cmd.CommandText = $"INSERT INTO {tableNames[0]} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
        }

        private void ConfigureUpdateCommand(SqlCommand cmd, DataRow row)
        {
            string[] setClauses = new string[row.ItemArray.Length - 1];

            for (int i = 0; i < row.ItemArray.Length - 1; i++)
            {
                setClauses[i] = $"{row.Table.Columns[i + 1].ColumnName} = @Param{i}";
                cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);
            }

            cmd.Parameters.AddWithValue("@ParamID", row["id", DataRowVersion.Original]);

            cmd.CommandText = $"UPDATE {tableNames[0]} SET {string.Join(", ", setClauses)} WHERE id = @ParamID";
        }

        private void ConfigureDeleteCommand(SqlCommand cmd, DataRow row)
        {
            cmd.Parameters.AddWithValue("@ParamID", row["id", DataRowVersion.Original]);
            cmd.CommandText = $"DELETE FROM {tableNames[0]} WHERE id = @ParamID";
        }
    }
}
