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
                // �G���[�n���h�����O�̋���
                MessageBox.Show("�f�[�^�̕\�����ɃG���[���������܂���: " + ex.Message);
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
                                // �X�V�����������ꍇ�A�������b�Z�[�W��\��
                                MessageBox.Show("�ύX���f�[�^�x�[�X�ɕۑ�����܂����B");
                            }
                        }
                    }

                    // �f�[�^�\�[�X�ɕύX�����������Ƃ�ʒm���ADataGridView���X�V
                    ((DataTable)dataGridView1.DataSource).AcceptChanges();
                }
                else
                {
                    MessageBox.Show("�ύX������܂���B");
                }
            }
            catch (Exception ex)
            {
                // �G���[�n���h�����O�̋���
                MessageBox.Show("�f�[�^�x�[�X�ւ̕ۑ����ɃG���[���������܂���: " + ex.Message);
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

        public string GenerateUpdateQuery(DataRow row, string tableName)
        {
            // �J�������ɍ��킹�� SET ��𐶐�
            string[] setClauses = new string[row.ItemArray.Length];
            for (int i = 0; i < row.ItemArray.Length; i++)
            {
                setClauses[i] = $"{row.Table.Columns[i].ColumnName} = @Param{i}";
            }

            // ID���ύX���ꂽ�ꍇ�Ƀ��b�Z�[�W��\��
            if (row.RowState != DataRowState.Added && row["id", DataRowVersion.Original].ToString() != row["id"].ToString())
            {
                MessageBox.Show("ID�͕ύX�ł��܂���B", "�G���[", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null; // ���b�Z�[�W��\�����Ă��邽�߁A�N�G���͎��s���Ȃ�
            }

            // �p�����[�^���܂� UPDATE �N�G���𐶐�
            return $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE id = @Param0";
        }

        public bool SaveRowToTable(DataRow row, string query)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                if (query != null) // �N�G����null�łȂ��ꍇ�Ɏ��s
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        // �p�����[�^�̒ǉ�
                        for (int i = 0; i < row.ItemArray.Length; i++)
                        {
                            cmd.Parameters.AddWithValue($"@Param{i}", row[i]);
                        }

                        cmd.ExecuteNonQuery();
                    }

                    return true; // �X�V�������������Ƃ�����
                }

                return false; // �X�V���������Ȃ��������Ƃ�����
            }
        }
    }
}
