using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Windows.Forms;

namespace book_manager
{
    public partial class Form_BookManager_Prototype : Form
    {
        private readonly DatabaseManager databaseManager;
        private readonly StringBuilder sql = new();
        private readonly string[] tableNames = { "basic_information", "rental_information" };

        public Form_BookManager_Prototype()
        {
            InitializeComponent();
            // データベースマネージャーの初期化
            databaseManager = new DatabaseManager(ConfigurationManager.ConnectionStrings["sqlsvr"].ConnectionString, tableNames);
            comboBox_tableName.SelectedIndex = 0;

            // 初期化メソッドの呼び出し
            Initialize();
        }

        private void Initialize()
        {
            // SQLクエリの基本構造を構築
            BuildBaseQuery();
            // FROM句の初期設定
            sql.AppendLine("FROM");
            sql.AppendLine(tableNames[0] + " as b");

            // データの表示
            DisplayData();

            // "No" 列を読み取り専用に設定
            dataGridView1.Columns["No"].ReadOnly = true;
        }

        private void BuildBaseQuery()
        {
            // SELECT句の基本構造を構築
            sql.Clear();
            sql.AppendLine("SELECT");
            sql.AppendLine("ROW_NUMBER() OVER(ORDER BY b.id ASC) No");
            sql.AppendLine(", b.id");
            sql.AppendLine(", b.book_name");
            sql.AppendLine(", price");
        }

        private void ComboBox_tableName_SelectedIndexChanged(object sender, EventArgs e)
        {
            // SQLクエリを再構築してデータを表示
            RebuildQueryAndDisplayData();
        }

        private void RebuildQueryAndDisplayData()
        {
            sql.Clear();
            string? selectedTable = comboBox_tableName.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTable))
            {
                // 無効な選択を処理
                return;
            }

            switch (selectedTable)
            {
                case "basic":
                    // 基本テーブルのクエリ構築
                    BuildBaseQuery();
                    sql.AppendLine("FROM");
                    sql.AppendLine(tableNames[0] + " as b");
                    break;
                case "basic + rental":
                    // 基本とレンタル情報を結合したクエリ構築
                    BuildBaseQuery();
                    sql.AppendLine(", r.name");
                    sql.AppendLine(", r.loan_date");
                    sql.AppendLine("FROM");
                    sql.AppendLine(tableNames[0] + " as b");
                    sql.AppendLine("LEFT JOIN");
                    sql.AppendLine(tableNames[1] + " as r ON b.id = r.id");
                    break;
                default:
                    throw new ArgumentException("Unsupported selectedTable");
            }
            // データの再表示
            DisplayData();
        }

        private void Button_Reload_Click(object sender, EventArgs e)
        {
            // データの再表示
            DisplayData();
        }

        private void DisplayData()
        {
            try
            {
                // データベースからデータを取得して表示
                DataTable dataTable = databaseManager.SelectFromTable(sql.ToString());
                dataGridView1.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("データの表示中にエラーが発生しました: " + ex.Message);
            }
        }

        private void Button_Save_Click(object sender, EventArgs e)
        {
            try
            {
                // 変更をデータベースに保存
                SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("データベースへの保存中にエラーが発生しました: " + ex.Message);
            }
        }

        private void SaveChanges()
        {
            try
            {
                // 変更されたデータを取得
                var modifiedData = ((DataTable)dataGridView1.DataSource).GetChanges();

                if (modifiedData == null)
                {
                    MessageBox.Show("変更がありません。");
                    return;
                }

                // 変更データの処理
                ProcessModifiedData(modifiedData);
                ((DataTable)dataGridView1.DataSource).AcceptChanges();
                MessageBox.Show("変更がデータベースに保存されました。");
            }
            catch (Exception ex)
            {
                MessageBox.Show("データベースへの保存中にエラーが発生しました: " + ex.Message);
            }
        }

        private void ProcessModifiedData(DataTable modifiedData)
        {
            // 各行の処理
            foreach (DataRow row in modifiedData.Rows)
            {
                databaseManager.ManageRow(row);
            }
        }
    }
}
