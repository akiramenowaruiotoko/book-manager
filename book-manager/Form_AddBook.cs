using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace book_manager
{
    public partial class Form_AddBook : Form
    {
        public Form_AddBook()
        {
            InitializeComponent();
            // Prevent users from adding new rows to dataGridView1
            dataGridView1.AllowUserToAddRows = false;
            // Hide the leftmost column of dataGridView1
            dataGridView1.RowHeadersVisible = false;
        }

        private void Button_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
