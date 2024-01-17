using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace book_manager
{
    public class DatabaseManager
    {
        public readonly string ConnectionString;
        private readonly string[] tableNames;

        public DatabaseManager(string connectionString, string[] tableNames)
        {
            ConnectionString = connectionString;
            this.tableNames = tableNames;
        }

        public DataTable SelectFromTable(string sql)
        {
            using SqlConnection connection = new SqlConnection(ConnectionString);
            using SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
            using DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);
            return dataTable;
        }

        public void ManageRow(DataRow row)
        {
            using SqlConnection connection = new SqlConnection(ConnectionString);
            using SqlCommand cmd = new SqlCommand(null, connection);
            connection.Open();
            switch (row.RowState)
            {
                case DataRowState.Added:
                    ConfigureInsertCommand(cmd, row);
                    break;

                case DataRowState.Modified:
                    ConfigureUpdateCommand(cmd, row);
                    break;

                case DataRowState.Deleted:
                    ConfigureDeleteCommand(cmd, row);
                    break;

                default:
                    throw new ArgumentException("Unsupported DataRowState");
            }
        }

        private void ConfigureInsertCommand(SqlCommand cmd, DataRow row)
        {
            cmd.Parameters.Clear();

            string[] columns;
            string[] values;

            // Extract common logic for basic insertion
            void ConfigureBasicInsertion(int endIndex)
            {
                columns = new string[endIndex];
                values = new string[endIndex];

                for (int i = 0; i < endIndex; i++)
                {
                    columns[i] = row.Table.Columns[i + 1].ColumnName;
                    values[i] = $"@Param{i}";
                    cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);
                }

                cmd.CommandText = $"INSERT INTO {tableNames[0]} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
                cmd.ExecuteNonQuery();
            }

            // Basic only
            if (row.ItemArray.Length == 4)
            {
                ConfigureBasicInsertion(row.ItemArray.Length - 1);
            }
            // Basic + rental
            else
            {
                // Basic
                if (row.RowState == DataRowState.Added)
                {
                    ConfigureBasicInsertion(row.ItemArray.Length - 3);
                }

                // Rental data existence check
                if (row["name"] == DBNull.Value)
                {
                    return;
                }

                // Rental
                cmd.Parameters.Clear();
                columns = new string[row.ItemArray.Length - 3];
                values = new string[row.ItemArray.Length - 3];

                for (int i = 0; i < row.ItemArray.Length - 1; i++)
                {
                    if (i == 0)
                    {
                        columns[i] = row.Table.Columns[i + 1].ColumnName;
                        values[i] = $"@Param{i}";
                        cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);
                    }
                    else if (i >= 3)
                    {
                        columns[i - 2] = row.Table.Columns[i + 1].ColumnName;
                        values[i - 2] = $"@Param{i - 2}";
                        cmd.Parameters.AddWithValue($"@Param{i - 2}", row[i + 1]);
                    }
                }

                cmd.CommandText = $"INSERT INTO {tableNames[1]} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
                cmd.ExecuteNonQuery();
            }
        }

        private void ConfigureUpdateCommand(SqlCommand cmd, DataRow row)
        {
            cmd.Parameters.AddWithValue("@ParamID", row["id", DataRowVersion.Original]);

            if (row.ItemArray.Length == 6)
            {
                ConfigureUpdateBasicInfo(cmd, row);

                if (IsRentalInfoEmpty(row))
                {
                    // レンタル情報が空の場合、挿入
                    ConfigureInsertCommand(cmd, row);
                }
                else if (row["name"] == DBNull.Value)
                {
                    // レンタル情報が削除された場合、削除
                    cmd.CommandText = $"DELETE FROM {tableNames[1]} WHERE id = @ParamID";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    ConfigureUpdateRentalInfo(cmd, row);
                }
            }
            else
            {
                ConfigureUpdateBasicInfo(cmd, row);
            }
        }

        private void ConfigureUpdateBasicInfo(SqlCommand cmd, DataRow row)
        {
            string[] setClauses = new string[row.ItemArray.Length - 3];
            for (int i = 0; i < row.ItemArray.Length - 3; i++)
            {
                setClauses[i] = $"{row.Table.Columns[i + 1].ColumnName} = @Param{i}";
                cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);
            }
            cmd.CommandText = $"UPDATE {tableNames[0]} SET {string.Join(", ", setClauses)} WHERE id = @ParamID";
            cmd.ExecuteNonQuery();
        }

        private void ConfigureUpdateRentalInfo(SqlCommand cmd, DataRow row)
        {
            string[] setClauses = new string[row.ItemArray.Length - 3];
            cmd.Parameters.Clear();

            cmd.Parameters.AddWithValue("@ParamID", row["id", DataRowVersion.Original]);
            for (int i = 0; i < row.ItemArray.Length - 1; i++)
            {
                if (i == 0)
                {
                    setClauses[i] = $"{row.Table.Columns[i + 1].ColumnName} = @Param{i}";
                    cmd.Parameters.AddWithValue($"@Param{i}", row[i + 1]);
                }
                else if (i >= 3)
                {
                    setClauses[i - 2] = $"{row.Table.Columns[i + 1].ColumnName} = @Param{i - 2}";
                    cmd.Parameters.AddWithValue($"@Param{i - 2}", row[i + 1]);
                }
            }
            cmd.CommandText = $"UPDATE {tableNames[1]} SET {string.Join(", ", setClauses)} WHERE id = @ParamID";
            cmd.ExecuteNonQuery();
        }

        private bool IsRentalInfoEmpty(DataRow row)
        {
            return (row["name", DataRowVersion.Original] == DBNull.Value) && (row["loan_date", DataRowVersion.Original] == DBNull.Value);
        }

        private void ConfigureDeleteCommand(SqlCommand cmd, DataRow row)
        {
            cmd.Parameters.AddWithValue("@ParamID", row["id", DataRowVersion.Original]);

            // レンタル情報の削除
            cmd.CommandText = $"DELETE FROM {tableNames[1]} WHERE id = @ParamID";
            cmd.ExecuteNonQuery();

            // 基本情報の削除
            cmd.CommandText = $"DELETE FROM {tableNames[0]} WHERE id = @ParamID";
            cmd.ExecuteNonQuery();
        }
    }
}

