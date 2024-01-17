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
            // �f�[�^�x�[�X�}�l�[�W���[�̏�����
            databaseManager = new DatabaseManager(ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString, tableNames);
            comboBox_tableName.SelectedIndex = 0;

            // ���������\�b�h�̌Ăяo��
            Initialize();
        }

        private void Initialize()
        {
            // SQL�N�G���̊�{�\�����\�z
            BuildBaseQuery();
            // FROM��̏����ݒ�
            sql.AppendLine("FROM");
            sql.AppendLine(tableNames[0] + " as b");

            // �f�[�^�̕\��
            DisplayData();

            // "No" ���ǂݎ���p�ɐݒ�
            dataGridView1.Columns["No"].ReadOnly = true;
        }

        private void BuildBaseQuery()
        {
            // SELECT��̊�{�\�����\�z
            sql.Clear();
            sql.AppendLine("SELECT");
            sql.AppendLine("ROW_NUMBER() OVER(ORDER BY b.id ASC) No");
            sql.AppendLine(", b.id");
            sql.AppendLine(", b.book_name");
            sql.AppendLine(", price");
        }

        private void ComboBox_tableName_SelectedIndexChanged(object sender, EventArgs e)
        {
            // SQL�N�G�����č\�z���ăf�[�^��\��
            RebuildQueryAndDisplayData();
        }

        private void RebuildQueryAndDisplayData()
        {
            sql.Clear();
            string? selectedTable = comboBox_tableName.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTable))
            {
                // �����ȑI��������
                return;
            }

            switch (selectedTable)
            {
                case "basic":
                    // ��{�e�[�u���̃N�G���\�z
                    BuildBaseQuery();
                    sql.AppendLine("FROM");
                    sql.AppendLine(tableNames[0] + " as b");
                    break;
                case "basic + rental":
                    // ��{�ƃ����^���������������N�G���\�z
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
            // �f�[�^�̍ĕ\��
            DisplayData();
        }

        private void Button_Reload_Click(object sender, EventArgs e)
        {
            // �f�[�^�̍ĕ\��
            DisplayData();
        }

        private void DisplayData()
        {
            try
            {
                // �f�[�^�x�[�X����f�[�^���擾���ĕ\��
                DataTable dataTable = databaseManager.SelectFromTable(sql.ToString());
                dataGridView1.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("�f�[�^�̕\�����ɃG���[���������܂���: " + ex.Message);
            }
        }

        private void Button_Save_Click(object sender, EventArgs e)
        {
            try
            {
                // �ύX���f�[�^�x�[�X�ɕۑ�
                SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("�f�[�^�x�[�X�ւ̕ۑ����ɃG���[���������܂���: " + ex.Message);
            }
        }

        private void SaveChanges()
        {
            try
            {
                // �ύX���ꂽ�f�[�^���擾
                var modifiedData = ((DataTable)dataGridView1.DataSource).GetChanges();

                if (modifiedData == null)
                {
                    MessageBox.Show("�ύX������܂���B");
                    return;
                }

                // �ύX�f�[�^�̏���
                ProcessModifiedData(modifiedData);
                ((DataTable)dataGridView1.DataSource).AcceptChanges();
                MessageBox.Show("�ύX���f�[�^�x�[�X�ɕۑ�����܂����B");
            }
            catch (Exception ex)
            {
                MessageBox.Show("�f�[�^�x�[�X�ւ̕ۑ����ɃG���[���������܂���: " + ex.Message);
            }
        }

        private void ProcessModifiedData(DataTable modifiedData)
        {
            // �e�s�̏���
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
                    // �����^����񂪋�̏ꍇ�A�}��
                    ConfigureInsertCommand(cmd, row);
                }
                else if (row["name"] == DBNull.Value)
                {
                    // �����^����񂪍폜���ꂽ�ꍇ�A�폜
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

            // �����^�����̍폜
            cmd.CommandText = $"DELETE FROM {tableNames[1]} WHERE id = @ParamID";
            cmd.ExecuteNonQuery();

            // ��{���̍폜
            cmd.CommandText = $"DELETE FROM {tableNames[0]} WHERE id = @ParamID";
            cmd.ExecuteNonQuery();
        }
    }
}
