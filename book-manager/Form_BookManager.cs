using System.Configuration;
using System.Data.SqlClient;

namespace book_manager
{
    public partial class Form_BookManager : Form
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")] // consol�@�L����
        private static extern bool AllocConsole();                 // consol�@�L����

        public Form_BookManager()
        {
            InitializeComponent();
            AllocConsole();                                        // consol�@�L����
        }
        // db connect
        public void Connect1()
        {
            // �ڑ�������̎擾
            var connectionString = ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            using (var command = connection.CreateCommand())
            {
                try
                {
                    // �f�[�^�x�[�X�̐ڑ��J�n
                    connection.Open();

                    // SQL�̎��s
                    
                    command.CommandText = @"SELECT count(*) FROM basic_information";
                    command.ExecuteNonQuery();
                    textBox1.Text = "aaa";
                    Console.WriteLine(textBox1.Text);
                    Console.ReadKey();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                    throw;
                }
                finally
                {
                    // �f�[�^�x�[�X�̐ڑ��I��
                    connection.Close();
                }
            }
        }

        // dataGridView practice
            private void Form_BookManager_Load(object sender, EventArgs e)
        {
            // db�ڑ��m�F
            Connect1();
            // �J���������w��
            dataGridView1.ColumnCount = 4;

            // �J���������w��
            dataGridView1.Columns[0].HeaderText = "����";
            dataGridView1.Columns[1].HeaderText = "�_��";
            dataGridView1.Columns[2].HeaderText = "����";
            dataGridView1.Columns[3].HeaderText = "�N���X��";

            // �f�[�^��ǉ�
            dataGridView1.Rows.Add("����", 90, "�c���@��Y", "A");
            dataGridView1.Rows.Add("���w", 50, "��؁@��Y", "A");
            dataGridView1.Rows.Add("�p��", 90, "�����@�O�Y", "B");
        }

    }
}
