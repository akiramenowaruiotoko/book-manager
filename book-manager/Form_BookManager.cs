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
            // db接続確認
            DbConnect();
            // カラム数を指定
            dataGridView1.ColumnCount = 4;

            // カラム名を指定
            dataGridView1.Columns[0].HeaderText = "教科";
            dataGridView1.Columns[1].HeaderText = "点数";
            dataGridView1.Columns[2].HeaderText = "氏名";
            dataGridView1.Columns[3].HeaderText = "クラス名";

            // データを追加
            dataGridView1.Rows.Add("国語", 90, "田中　一郎", "A");
            dataGridView1.Rows.Add("数学", 50, "鈴木　二郎", "A");
            dataGridView1.Rows.Add("英語", 90, "佐藤　三郎", "B");
        }

    }
}
