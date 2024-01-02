namespace book_manager
{
    public partial class Form_BookManager : Form
    {
        public Form_BookManager()
        {
            InitializeComponent();
        }

        private void Form_BookManager_Load(object sender, EventArgs e)
        {
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
