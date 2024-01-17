using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Windows.Forms;

namespace book_manager
{
    public partial class Form_BookManager_Prototype : Form
    {
        private readonly DatabaseManager databaseManager;
        private readonly StringBuilder sql = new();
        private readonly string[] tableNames = { "basic_information", "rental_information" };

        public Form_BookManager_Prototype()
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
}
