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
