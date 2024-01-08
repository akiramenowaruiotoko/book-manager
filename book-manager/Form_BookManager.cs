using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace book_manager
{
    public partial class Form_BookManager : Form
    {
        // SQL Server�̐ڑ�������
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString;

        // �f�[�^�x�[�X�Ǘ��p�̃I�u�W�F�N�g
        private readonly DatabaseManager databaseManager;

        // �ύX�O��ID��ێ����邽�߂̃f�B�N�V���i��
        private readonly Dictionary<string, string> originalIds = [];

        // �R���X�g���N�^
        public Form_BookManager()
        {
            InitializeComponent();
            // �f�[�^�x�[�X�}�l�[�W���̃C���X�^���X��
            databaseManager = new DatabaseManager(connectionString);
            // �t�H�[�������������ăf�[�^��\��
            DisplayData();
            // no���ҏW�s�ɐݒ�
            dataGridView1.Columns["no"].ReadOnly = true;

        }

        // �f�[�^��\�����郁�\�b�h
        private void DisplayData()
        {
            try
            {
                // �f�[�^�x�[�X����"basic_information"�e�[�u���̃f�[�^���擾
                DataTable dataTable = databaseManager.SelectAllFromTable("basic_information");

                // �ύX�O��ID��ێ�
                originalIds.Clear();
                foreach (DataRow row in dataTable.Rows)
                {
                    originalIds.Add(row["id", DataRowVersion.Original].ToString()!, row["id", DataRowVersion.Original].ToString()!);
                }

                // �擾�����f�[�^��DataGridView�ɕ\��
                dataGridView1.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("�f�[�^�̕\�����ɃG���[���������܂���: " + ex.Message);
            }
        }

        // �����[�h�{�^���̃N���b�N�C�x���g�n���h��
        private void Button_Reload_Click(object sender, EventArgs e)
        {
            // �f�[�^�̍ēǂݍ���
            DisplayData();
        }

        // �ۑ��{�^���̃N���b�N�C�x���g�n���h��
        private void Botton_Save_Click(object sender, EventArgs e)
        {
            try
            {
                // DataGridView�̕ύX���擾
                var modifiedData = ((DataTable)dataGridView1.DataSource).GetChanges();

                // �ύX������ꍇ�̏���
                if (modifiedData != null)
                {
                    foreach (DataRow row in modifiedData.Rows)
                    {
                        // �s�̏�Ԃɂ���ď����𕪊�
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
                    MessageBox.Show("�ύX������܂���B");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("�f�[�^�x�[�X�ւ̕ۑ����ɃG���[���������܂���: " + ex.Message);
            }
        }

        // �V�����f�[�^��}�����郁�\�b�h
        private void InsertData(DataRow newRow)
        {
            try
            {
                // �V�����f�[�^�̑}���N�G���𐶐����Ď��s
                string insertQuery = DatabaseManager.GenerateInsertQuery(newRow, "basic_information");
                databaseManager.InsertRowToTable(newRow, insertQuery);

                // �f�[�^�x�[�X�ւ̕ύX���m�肵�A���b�Z�[�W��\��
                ((DataTable)dataGridView1.DataSource).AcceptChanges();
                MessageBox.Show("�V�����f�[�^���f�[�^�x�[�X�ɑ}������܂����B");
            }
            catch (Exception ex)
            {
                MessageBox.Show("�f�[�^�x�[�X�ւ̑}�����ɃG���[���������܂���: " + ex.Message);
            }
        }

        // �f�[�^�̍X�V���\�b�h
        private void UpdateData(DataRow updatedRow)
        {
            try
            {
                // �ύX�O��ID���擾
                string originalId = updatedRow["id", DataRowVersion.Original].ToString()!;
                string? currentId = updatedRow["id", DataRowVersion.Current].ToString(); // id��null�̏ꍇ��catch�ŏ���

                // �ύX�O��ID���g�p���čX�V�N�G���𐶐�
                string updateQuery = DatabaseManager.GenerateUpdateQuery(updatedRow, "basic_information", originalIds[originalId]);
                databaseManager.UpdateRowToTable(updatedRow, updateQuery);

                // �f�[�^�x�[�X�ւ̕ύX���m�肵�A���b�Z�[�W��\��
                ((DataTable)dataGridView1.DataSource).AcceptChanges();
                MessageBox.Show("�ύX���f�[�^�x�[�X�ɕۑ�����܂����B");
            }
            catch (Exception ex)
            {
                MessageBox.Show("�f�[�^�x�[�X�ւ̍X�V���ɃG���[���������܂���: " + ex.Message);
            }
        }

        // �f�[�^�̍폜���\�b�h
        private void DeleteData(DataRow deletedRow)
        {
            try
            {
                // �폜�N�G���𐶐����Ď��s
                string deleteQuery = DatabaseManager.GenerateDeleteQuery(deletedRow, "basic_information");
                databaseManager.DeleteRowFromTable(deletedRow, deleteQuery);

                // �f�[�^�x�[�X�ւ̕ύX���m�肵�A���b�Z�[�W��\��
                ((DataTable)dataGridView1.DataSource).AcceptChanges();
                MessageBox.Show("�f�[�^���f�[�^�x�[�X����폜����܂����B");
            }
            catch (Exception ex)
            {
                MessageBox.Show("�f�[�^�x�[�X����̍폜���ɃG���[���������܂���: " + ex.Message);
            }
        }
    }

    // �f�[�^�x�[�X����p�̃N���X
    public class DatabaseManager(string connectionString)
    {
        // SQL Server�̐ڑ�������
        private readonly string connectionString = connectionString;

        // DB�e�[�u�����炷�ׂẴf�[�^���擾���郁�\�b�h
        public DataTable SelectAllFromTable(string tableName)
        {
            // SqlConnection��SqlDataAdapter���g�p���ăf�[�^���擾
            using SqlConnection connection = new(connectionString);
            using SqlDataAdapter adapter = new($"SELECT ROW_NUMBER() OVER(ORDER BY id ASC) no, * FROM {tableName}", connection); // no���ǉ�
            DataTable dataTable = new();
            adapter.Fill(dataTable);
            return dataTable;
        }

        // �X�V�N�G���𐶐����郁�\�b�h
        public static string GenerateUpdateQuery(DataRow row, string tableName, string currentId)
        {
            // �X�V�N�G����SET��𐶐� (No��͐������Ȃ�)
            string[] setClauses = new string[row.ItemArray.Length - 1];
            for (int i = 0; i < row.ItemArray.Length - 1; i++) 
                setClauses[i] = $"{row.Table.Columns[i + 1].ColumnName} = @Param{i}";

            // �X�V�N�G����Ԃ�
            return $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE id = '{currentId}'";
        }

        // �}���N�G���𐶐����郁�\�b�h
        public static string GenerateInsertQuery(DataRow row, string tableName)
        {
            // �}���N�G���̃J�����ƒl�𐶐� (No��͐������Ȃ�)
            string[] columns = new string[row.ItemArray.Length - 1];
            string[] values = new string[row.ItemArray.Length - 1];
            for (int i = 0; i < row.ItemArray.Length - 1; i++)
            {
                columns[i] = row.Table.Columns[i + 1].ColumnName;
                values[i] = $"@Param{i}";
            }

            // �}���N�G����Ԃ�
            return $"INSERT INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
        }


        // �폜�N�G���𐶐����郁�\�b�h
        public static string GenerateDeleteQuery(DataRow row, string tableName)
        {
            // �폜�N�G����Ԃ�
            return $"DELETE FROM {tableName} WHERE id = @Param0";
        }

        // �e�[�u���̍s���X�V���郁�\�b�h
        public bool UpdateRowToTable(DataRow row, string query)
        {
            // �X�V�N�G�������s
            using SqlConnection connection = new(connectionString);
            using SqlCommand cmd = new(query, connection);
            for (int i = 0; i < row.ItemArray.Length - 1; i++)
                cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);

            connection.Open();
            cmd.ExecuteNonQuery();

            return true;
        }

        // �e�[�u���ɐV�����s��}�����郁�\�b�h
        public bool InsertRowToTable(DataRow row, string query)
        {
            // �}���N�G�������s
            using SqlConnection connection = new(connectionString);
            using SqlCommand cmd = new(query, connection);
            for (int i = 0; i < row.ItemArray.Length - 1; i++)
                cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);

            connection.Open();
            cmd.ExecuteNonQuery();

            return true;
        }

        // �e�[�u������s���폜���郁�\�b�h
        public bool DeleteRowFromTable(DataRow row, string query)
        {
            // �폜�N�G�������s
            using SqlConnection connection = new(connectionString);
            using SqlCommand cmd = new(query, connection);
            cmd.Parameters.AddWithValue($"@Param0", row["id", DataRowVersion.Original]);

            connection.Open();
            cmd.ExecuteNonQuery();

            return true;
        }
    }
}
