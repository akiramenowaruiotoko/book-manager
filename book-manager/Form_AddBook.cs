using System.Data;

namespace book_manager
{
    public partial class Form_AddBook : Form
    {
        public Form_AddBook()
        {
            InitializeComponent();
        }

        // read DB table basic_information only column
        private void Form_AddBook_Load(object sender, EventArgs e)
        {
            DataTable addDataTable = OperateDataBase.ConnectDB("SELECT * FROM basic_information where 1 != '1';");
            // output dataGridView
            dataGridView1.DataSource = addDataTable;
        }
        private void Button_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}