using System.Configuration;
using System.Data.SqlClient;

namespace book_manager
{
    public partial class Form_BookManager : Form
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")] // consol　有効化
        private static extern bool AllocConsole();                 // consol　有効化

        public Form_BookManager()
        {
            InitializeComponent();
            AllocConsole();                                        // consol　有効化
        }
        // db connect
        public void Connect1()
        {
            // 接続文字列の取得
            var connectionString = ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            using (var command = connection.CreateCommand())
            {
                try
                {
                    // データベースの接続開始
                    connection.Open();

                    // SQLの実行
                    
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
                    // データベースの接続終了
                    connection.Close();
                }
            }
        }

        // dataGridView practice
            private void Form_BookManager_Load(object sender, EventArgs e)
        {
            // db接続確認
            Connect1();
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
