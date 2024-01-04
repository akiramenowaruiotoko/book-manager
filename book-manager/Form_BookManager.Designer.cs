namespace book_manager
{
    partial class Form_BookManager
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_BookManager));
            dataGridView1 = new DataGridView();
            Label_BookManagement = new Label();
            Button_AddBook = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resources.ApplyResources(dataGridView1, "dataGridView1");
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            // 
            // Label_BookManagement
            // 
            resources.ApplyResources(Label_BookManagement, "Label_BookManagement");
            Label_BookManagement.Name = "Label_BookManagement";
            // 
            // Button_AddBook
            // 
            resources.ApplyResources(Button_AddBook, "Button_AddBook");
            Button_AddBook.Name = "Button_AddBook";
            Button_AddBook.UseVisualStyleBackColor = true;
            Button_AddBook.Click += Botton_addBook_Click;
            // 
            // Form_BookManager
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(Button_AddBook);
            Controls.Add(Label_BookManagement);
            Controls.Add(dataGridView1);
            Name = "Form_BookManager";
            Load += Form_BookManager_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private Label Label_BookManagement;
        private Button Button_AddBook;
    }
}
