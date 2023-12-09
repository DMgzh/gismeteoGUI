using System;
using Npgsql;

namespace gismeteo_gui
{
	public class Database
	{
        // "Host=localhost;Username=postgres;Password=passw;Database=postgres"
        NpgsqlConnection con;
        NpgsqlCommand cmd;
        string currentTable="region";

        string host, username, password, database;
        public Database()
		{
            this.host = "localhost";
            this.username = "postgres";
            this.password = "passw";
            this.database = "postgres";
            Connect();

            this.cmd = new("", con);

        }
		public Database(string host, string username, string password, string database)
		{
			this.host = host;
			this.username = username;
			this.password = password;
			this.database = database;
            Connect();

            this.cmd = new("", con);

		}
		private void Connect()
		{
            string cs = $"Host={host};Username={username};Password={password};Database={database}";
            con = new(cs);
            con.Open();
        }

		/* public void change_table(string table_name)
		{
			currentTable = table_name;
		}*/

		public void get_table_names(List<string> names)
		{
			cmd.CommandText = string.Format("select count(*) from information_schema.columns where table_name = '{}'", currentTable);
			
        }

		public Table get_table(string table_name)
		{
			return (new Table(table_name, cmd));
		}


	}

	public class Table
	{
        string currentTable;
		NpgsqlCommand cmd;
        NpgsqlDataReader rdr;

        public Table(string table_name, NpgsqlCommand cmd)
		{
			this.currentTable = table_name;
			this.cmd = cmd;
		}

		public void get_table_data(string[,] data)
		{
            int row_limit = 50;

            // Get number of columns
            cmd.CommandText = string.Format("select count(*) from information_schema.columns where table_name = '{0}'", currentTable);
            int columns = int.Parse(cmd.ExecuteScalar().ToString());

            // Get number of rows
            cmd.CommandText = string.Format("select count(*) from {0}", currentTable);
            int rows = Math.Min(int.Parse(cmd.ExecuteScalar().ToString()), row_limit);

            // Get headers
            string[] headers = new string[columns];
            cmd.CommandText = string.Format("select column_name from information_schema.columns where table_name = '{0}' order by ordinal_position", currentTable);
            rdr = cmd.ExecuteReader();
            for (int col = 0; col < columns; col++)
            {
                rdr.Read();
                headers[col] = rdr.GetString(0);
            }
            rdr.Close();

            // Get data from db
            cmd.CommandText = string.Format("select {0} from {1} order by id limit {2}", string.Join(", ", headers), currentTable, row_limit);
            rdr = cmd.ExecuteReader();
            for (int row = 0; row < rows; ++row)
            {
                rdr.Read();
                for (int col = 0; col < columns; ++col)
                {
                    data[row, col] = rdr.GetValue(col).ToString();
                }
            }
            rdr.Close();
        }

        public int get_columns_number()
        {
            int n;

            cmd.CommandText = string.Format("select count(*) from information_schema.columns where table_name = '{0}'", currentTable);
            n = int.Parse(cmd.ExecuteScalar().ToString());

            return n;
        }

        public int get_rows_number(int row_limit=50)
        {
            int n;

            cmd.CommandText = string.Format("select count(*) from {0}", currentTable);
            n = Math.Min(int.Parse(cmd.ExecuteScalar().ToString()), row_limit);

            return n;
        }

        public void get_headers(string[] headers, int columns_number)
        {
            cmd.CommandText = string.Format("select column_name from information_schema.columns where table_name = '{0}' order by ordinal_position", currentTable);
            rdr = cmd.ExecuteReader();
            for (int col = 0; col < columns_number; col++)
            {
                rdr.Read();
                headers[col] = rdr.GetString(0);
            }
            rdr.Close();
        }


        public void edit(int row, int col)
        {

        }

	}
}

