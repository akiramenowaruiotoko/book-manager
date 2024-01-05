using System.Data;

namespace book_manager
{
    public partial class Form_BookManager : Form
    {
        public Form_BookManager()
        {
            // Form initialize
            InitializeComponent();
            // Prevent users from adding new rows to dataGridView1
            dataGridView1.AllowUserToAddRows = false;
            // Hide the leftmost column of dataGridView1
            dataGridView1.RowHeadersVisible = false;
        }

        // read DB table basic_information
        private void Form_BookManager_Load(object sender, EventArgs e)
        {
            DataTable dt = OperateDataBase.ReadDB("SELECT ROW_NUMBER() OVER(ORDER BY control_number ASC) no, * FROM basic_information;");
            // output dataGridView
            dataGridView1.DataSource = dt;
        }

        // show Form_AddBook
        private void Botton_addBook_Click(object sender, EventArgs e)
        {
            Form_AddBook form_AddBook = new();
            form_AddBook.Show();
        }
    }
}