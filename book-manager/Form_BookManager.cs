using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;

namespace book_manager
{
    public partial class Form_BookManager : Form
    {
        // �f�[�^�x�[�X�ڑ���������擾
        private string connectionString = ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString;

        // �f�[�^�x�[�X�}�l�[�W���[�̃C���X�^���X���쐬
        private DatabaseManager databaseManager;

        // �t�H�[���̃R���X�g���N�^
        public Form_BookManager()
        {
            InitializeComponent();

            // �f�[�^�x�[�X�}�l�[�W���[�̃C���X�^���X���쐬
            databaseManager = new DatabaseManager(connectionString);

            // �t�H�[�������[�h���ꂽ�Ƃ��Ƀf�[�^��\��
            DisplayData();
        }

        // �f�[�^��\�����郁�\�b�h
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

        // �ۑ��{�^�����N���b�N���ꂽ�Ƃ��̃C�x���g�n���h��
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
                        // �V�����s�������̍s���𔻒�
                        if (row.RowState == DataRowState.Added)
                        {
                            // �V�����s�̏ꍇ�A�}�����\�b�h���Ăяo��
                            InsertData(row);
                        }
                        else if (row.RowState == DataRowState.Modified)
                        {
                            // �����̍s�̏ꍇ�AUPDATE �N�G���𐶐����ăf�[�^�x�[�X�ɕۑ�
                            string updateQuery = databaseManager.GenerateUpdateQuery(row, "basic_information");
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

        // �V�����f�[�^��}�����郁�\�b�h
        private void InsertData(DataRow newRow)
        {
            try
            {
                // INSERT �N�G���𐶐�
                string insertQuery = databaseManager.GenerateInsertQuery(newRow, "basic_information");

                // �f�[�^�x�[�X�ɐV�����s��}��
                if (databaseManager.InsertRowToTable(newRow, insertQuery))
                {
                    // �}�������������ꍇ�A�������b�Z�[�W��\��
                    MessageBox.Show("�V�����f�[�^���f�[�^�x�[�X�ɑ}������܂����B");
                }

                // �f�[�^�\�[�X�ɕύX�����������Ƃ�ʒm���ADataGridView���X�V
                ((DataTable)dataGridView1.DataSource).AcceptChanges();
            }
            catch (Exception ex)
            {
                // �G���[�n���h�����O�̋���
                MessageBox.Show("�f�[�^�x�[�X�ւ̑}�����ɃG���[���������܂���: " + ex.Message);
            }
        }

        private void Button_Reload_Click(object sender, EventArgs e)
        {
            DisplayData();
        }
    }

    public class DatabaseManager
    {
        // �f�[�^�x�[�X�ڑ�������
        private string connectionString;

        // �R���X�g���N�^
        public DatabaseManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        // �e�[�u�����炷�ׂẴf�[�^���擾���郁�\�b�h
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

        // UPDATE �N�G���𐶐����郁�\�b�h
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

        // INSERT �N�G���𐶐����郁�\�b�h
        public string GenerateInsertQuery(DataRow row, string tableName)
        {
            // �J�������ɍ��킹�� INSERT �N�G���𐶐�
            string[] columns = new string[row.ItemArray.Length];
            string[] values = new string[row.ItemArray.Length];
            for (int i = 0; i < row.ItemArray.Length; i++)
            {
                columns[i] = row.Table.Columns[i].ColumnName;
                values[i] = $"@Param{i}";
            }

            // �p�����[�^���܂� INSERT �N�G���𐶐�
            return $"INSERT INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
        }

        // �s���f�[�^�x�[�X�ɕۑ����郁�\�b�h
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

        // �V�����s���f�[�^�x�[�X�ɑ}�����郁�\�b�h
        public bool InsertRowToTable(DataRow row, string query)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                // �p�����[�^�̒ǉ�
                for (int i = 0; i < row.ItemArray.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"@Param{i}", row[i]);
                }

                // �f�[�^�x�[�X�ɐV�����s��}��
                connection.Open();
                cmd.ExecuteNonQuery();
            }

            return true; // �}���������������Ƃ�����
        }
    }
}
