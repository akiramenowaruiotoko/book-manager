namespace book_manager
{
    partial class Form_AddBook
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dataGridView1 = new DataGridView();
            Column1 = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            Column3 = new DataGridViewTextBoxColumn();
            Column4 = new DataGridViewTextBoxColumn();
            Column5 = new DataGridViewTextBoxColumn();
            Column6 = new DataGridViewTextBoxColumn();
            Label_AddBook = new Label();
            Button_Add = new Button();
            Button_Close = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2, Column3, Column4, Column5, Column6 });
            dataGridView1.Location = new Point(32, 157);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 82;
            dataGridView1.Size = new Size(1558, 174);
            dataGridView1.TabIndex = 0;
            // 
            // Column1
            // 
            Column1.HeaderText = "Column1";
            Column1.MinimumWidth = 10;
            Column1.Name = "Column1";
            Column1.Width = 200;
            // 
            // Column2
            // 
            Column2.HeaderText = "Column2";
            Column2.MinimumWidth = 10;
            Column2.Name = "Column2";
            Column2.Width = 200;
            // 
            // Column3
            // 
            Column3.HeaderText = "Column3";
            Column3.MinimumWidth = 10;
            Column3.Name = "Column3";
            Column3.Width = 200;
            // 
            // Column4
            // 
            Column4.HeaderText = "Column4";
            Column4.MinimumWidth = 10;
            Column4.Name = "Column4";
            Column4.Width = 200;
            // 
            // Column5
            // 
            Column5.HeaderText = "Column5";
            Column5.MinimumWidth = 10;
            Column5.Name = "Column5";
            Column5.Width = 200;
            // 
            // Column6
            // 
            Column6.HeaderText = "Column6";
            Column6.MinimumWidth = 10;
            Column6.Name = "Column6";
            Column6.Width = 200;
            // 
            // Label_AddBook
            // 
            Label_AddBook.AutoSize = true;
            Label_AddBook.Font = new Font("Yu Gothic UI", 20F);
            Label_AddBook.Location = new Point(32, 9);
            Label_AddBook.Name = "Label_AddBook";
            Label_AddBook.Size = new Size(266, 72);
            Label_AddBook.TabIndex = 1;
            Label_AddBook.Text = "Add Book";
            // 
            // Button_Add
            // 
            Button_Add.Location = new Point(683, 46);
            Button_Add.Name = "Button_Add";
            Button_Add.Size = new Size(200, 73);
            Button_Add.TabIndex = 2;
            Button_Add.Text = "Add";
            Button_Add.UseVisualStyleBackColor = true;
            // 
            // Button_Close
            // 
            Button_Close.Location = new Point(918, 46);
            Button_Close.Name = "Button_Close";
            Button_Close.Size = new Size(200, 73);
            Button_Close.TabIndex = 3;
            Button_Close.Text = "Close";
            Button_Close.UseVisualStyleBackColor = true;
            Button_Close.Click += Button_Close_Click;
            // 
            // Form_AddBook
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1734, 450);
            Controls.Add(Button_Close);
            Controls.Add(Button_Add);
            Controls.Add(Label_AddBook);
            Controls.Add(dataGridView1);
            Name = "Form_AddBook";
            Text = "Form_AddBook";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Column3;
        private DataGridViewTextBoxColumn Column4;
        private DataGridViewTextBoxColumn Column5;
        private DataGridViewTextBoxColumn Column6;
        private Label Label_AddBook;
        private Button Button_Add;
        private Button Button_Close;
    }
}