using System.Configuration;
using System.Data;
using System.Data.SqlClient;

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

        // read DataBase
        private void Form_BookManager_Load(object sender, EventArgs e)
        {
            readDataBase();
        }

        /// <summary>
        /// read DataBase and output dt
        /// </summary>
        public void readDataBase()
        {
            // get connectionstring from App.config file
            string connectionString = ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString;

            // connection instance generation
            using SqlConnection connection = new(connectionString);
            using SqlCommand cmd = connection.CreateCommand();
            try
            {
                // start connect DB
                connection.Open();

                // read DB and output dt
                // DataTable create
                DataTable dt = new();

                // set table information in DataTable with SqlDataAdapter.Fill
                cmd.CommandText = "SELECT * FROM basic_information;";
                SqlDataAdapter adapter = new(cmd);
                adapter.Fill(dt);

                // output dataGridView
                dataGridView1.DataSource = dt;
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

        private void Botton_addBook_Click(object sender, EventArgs e)
        {
            Form_AddBook form_AddBook = new();
            form_AddBook.Show();
        }
    }
}
