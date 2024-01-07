using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace book_manager
{
    public partial class Form_BookManager : Form
    {
        // �ڑ��������ݒ�
        private string connectionString = ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString;
        private DatabaseManager databaseManager;

        public Form_BookManager()
        {
            InitializeComponent();

            // �f�[�^�x�[�X�}�l�[�W���[�̃C���X�^���X���쐬
            databaseManager = new DatabaseManager(connectionString);

            // �t�H�[�������[�h���ꂽ�Ƃ��Ƀf�[�^��\��
            DisplayData();

            // ID���ǂݎ���p�ɐݒ�
            dataGridView1.Columns["id"].ReadOnly = true;
        }

        private void DisplayData()
        {
            try
            {
                // "basic_information" �e�[�u������f�[�^���擾
                DataTable dataTable = databaseManager.SelectAllFromTable("basic_information");

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
                // �ύX���ꂽ�f�[�^���擾
                var modifiedData = ((DataTable)dataGridView1.DataSource).GetChanges();

                if (modifiedData != null)
                {
                    foreach (DataRow row in modifiedData.Rows)
                    {
                        // UPDATE �N�G���𐶐�
                        string updateQuery = databaseManager.GenerateUpdateQuery(row, "basic_information");

                        // �f�[�^�x�[�X�ɍs��ۑ�
                        if (databaseManager.SaveRowToTable(row, updateQuery))
                        {
                            // �X�V�����������ꍇ�A�������b�Z�[�W��\��
                            MessageBox.Show("�ύX���f�[�^�x�[�X�ɕۑ�����܂����B");
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

        public DataTable SelectAllFromTable(string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlDataAdapter adapter = new SqlDataAdapter($"SELECT * FROM {tableName}", connection))
            {
                DataTable dataTable = new DataTable();

                // �f�[�^�x�[�X����f�[�^���擾����DataTable�ɏ[�U
                adapter.Fill(dataTable);

                return dataTable;
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

            // �p�����[�^���܂� UPDATE �N�G���𐶐�
            return $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE id = @Param0";
        }

        public bool SaveRowToTable(DataRow row, string query)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                // �p�����[�^�̒ǉ�
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"@Param{i}", row[i]);
                }

                // �f�[�^�x�[�X�ɍs��ۑ�
                connection.Open();
                cmd.ExecuteNonQuery();
            }

            return true; // �X�V�������������Ƃ�����
        }
    }
}
