using System.Configuration;
using System.Data.SqlClient;

namespace book_manager
{
    public partial class Form_BookManager : Form
    {
        /// <summary>
        ///consol activation
        /// </summary>
        [System.Runtime.InteropServices.DllImport("kernel32.dll")] 
        private static extern bool AllocConsole();

        public Form_BookManager()
        {
            // Form initialize
            InitializeComponent();
            // consol execution
            AllocConsole();
        }

        /// <summary>
        /// db connect
        /// </summary>
        public void DbConnect()
        {
            // get connectionstring from App.config file
            string connectionString = ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString;

            // connection instance generation
            using SqlConnection connection = new(connectionString);
            try
            {
                // start connect DB
                connection.Open();

                /// <summary>
                /// read DB
                /// </summary>
                // setting SQL query
                string queryString = "SELECT book_name FROM basic_information where no = '05';";
                // command instance generation
                SqlCommand command = new(queryString, connection);
                // read sql data and output
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        textBox1.Text = String.Format("{0}", reader[0]);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }
            finally
            {
                // close db connection
                connection.Close();
            }
        }

        // dataGridView practice
            private void Form_BookManager_Load(object sender, EventArgs e)
        {
            // db�ڑ��m�F
            DbConnect();
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
