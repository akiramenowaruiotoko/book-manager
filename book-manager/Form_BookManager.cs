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

            // �t�H�[�������[�h���ꂽ�Ƃ��Ƀf�[�^��\��
            DisplayData();
        }

        private void DisplayData()
        {
            try
            {
                DataTable dataTable = databaseManager.SelectAllFromTableWithRowNumber("basic_information");

                // DataGridView�Ƀf�[�^��ݒ�
                dataGridView1.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("�f�[�^�̕\�����ɃG���[���������܂���: " + ex.Message);
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
                        // �f�[�^���}������邩�A�X�V����邩�̔���
                        databaseManager.SaveRowToTable(row, "basic_information");
                    }

                    // �f�[�^�\�[�X�ɕύX�����������Ƃ�ʒm���ADataGridView���X�V
                    ((DataTable)dataGridView1.DataSource).AcceptChanges();

                    MessageBox.Show("�ύX���f�[�^�x�[�X�ɕۑ�����܂����B");
                }
                else
                {
                    MessageBox.Show("�ύX������܂���B");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("�f�[�^�x�[�X�ւ̕ۑ����ɃG���[���������܂���: " + ex.Message);
            }
        }

        // ���̃��\�b�h��N���X�̎���

        // �J�����̌^�ɉ����ăf�t�H���g�̒l���擾���郁�\�b�h
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
            // ���̌^�ɑ΂��鏈�����ǉ��\

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

                // ROW_NUMBER() ���g�p�����N�G��
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

                // �f�[�^���}������邩�A�X�V����邩�̔���
                string query = (row.RowState == DataRowState.Added)
                    ? GenerateInsertQuery(row, tableName)
                    : GenerateUpdateQuery(row, tableName);

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    // �p�����[�^�̒ǉ�
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
            // �J�������ɍ��킹�ăp�����[�^�𐶐�
            string[] paramNames = new string[row.ItemArray.Length];
            for (int i = 0; i < row.ItemArray.Length; i++)
            {
                paramNames[i] = $"@Param{i}";
            }

            // �J���������w�肵�āA�p�����[�^���܂� INSERT �N�G���𐶐�
            string[] columnNames = row.Table.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray();

            // VALUES ��Ɋ܂܂��l�̐����A��̐��ɍ��킹��
            string valuesClause = string.Join(", ", paramNames);

            return $"INSERT INTO {tableName} ({string.Join(", ", columnNames)}) VALUES ({valuesClause})";
        }

        private string GenerateUpdateQuery(DataRow row, string tableName)
        {
            // �J�������ɍ��킹�� SET ��𐶐�
            string[] setClauses = new string[row.ItemArray.Length];
            for (int i = 0; i < row.ItemArray.Length; i++)
            {
                setClauses[i] = $"{row.Table.Columns[i].ColumnName} = @Param{i}";
            }

            // �p�����[�^���܂� UPDATE �N�G���𐶐�
            return $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE id = @Param0";
        }
    }
}
