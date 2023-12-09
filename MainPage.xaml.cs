using Npgsql;

namespace gismeteo_gui;

public partial class MainPage : ContentPage
{
    string currentTable = "region";

    Database db = new Database();

    public MainPage()
    {
        InitializeComponent();
        ShowData();
    }

    void ShowData()
    {
        table.Children.Clear();

        Table tableObject = db.get_table(currentTable);

        int columns = tableObject.get_columns_number();
        int rows = tableObject.get_rows_number();

        string[] headers = new string[columns];
        tableObject.get_headers(headers, columns);

        string[,] data = new string[rows, columns];
        tableObject.get_table_data(data);

        // Definitions
        for (int col = 0; col < columns + 1; ++col)
        {
            table.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        }

        for (int row = 0; row < rows + 1; ++row)
        {
            table.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        table.ColumnDefinitions.Add(new ColumnDefinition(200));

        // Show
        for (int col = 0; col < columns; ++col)
        {
            table.Add(new Label() { Text = headers[col], FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center }, col, 0);
        }
        for (int row = 0; row < rows; ++row)
        {
            for (int col = 0; col < columns; ++col)
            {
                string value = data[row, col].ToString();
                // DB content
                table.Add(new Button() { Text = value }, col, row + 1);
            }
            table.Add(new Button { Text = "DELETE" }, columns, row + 1);
        }

        table.Add(new Button { Text = "ADD", Command = new Command(AddRow) }, columns + 1, 1);

        (scroll2 as IView).InvalidateMeasure();
    }


    private async void AddRow()
    {
        await DisplayAlert("Ошибка", "sasay kok", "OK");
    }

    void ChangeTable(string name)
    {
        if(currentTable != name)
        {
            currentTable = name;
            ShowData();
        }
    }
}


/*
using Npgsql;

namespace gismeteo_gui;

public partial class MainPage : ContentPage
{
    NpgsqlConnection con;
    string sql;
    NpgsqlCommand cmd;
    NpgsqlDataReader rdr;
    string currentTable;
    public MainPage()
    {
        InitializeComponent();
        ConnectToDB();
        cmd = new("", con);
        GetTables();
        ShowData();
    }

    void ConnectToDB()
    {
        string cs = "Host=localhost;Username=postgres;Password=passw;Database=postgres";
        con = new(cs);
        con.Open();

        Database db = new Database();
    }

    void GetTables()
    {
        sql = "select count(table_name) from information_schema.tables where table_schema = 'public'";
        cmd = new(sql, con);
        int tableCount = int.Parse(cmd.ExecuteScalar().ToString());

        sql = "select table_name from information_schema.tables where table_schema = 'public'";
        cmd = new(sql, con);
        rdr = cmd.ExecuteReader();
        for (int i = 0; i < tableCount; ++i)
        {
            rdr.Read();
            menu.AddColumnDefinition(new ColumnDefinition(GridLength.Auto));
            menu.Add(new Button()
            {
                Text = rdr.GetString(0),
                Command = new Command<string>(ChangeTable),
                CommandParameter = rdr.GetString(0)
            }, i);
        }
        (scroll1 as IView).InvalidateMeasure();
        rdr.Close();
        rdr = cmd.ExecuteReader();
        rdr.Read();
        currentTable = rdr.GetString(0);
        rdr.Close();
    }

    void ShowData()
    {
        table.Children.Clear();

        int row_limit = 50;

        // Get number of columns
        cmd.CommandText = string.Format("select count(*) from information_schema.columns where table_name = '{0}'", currentTable);
        int columns = int.Parse(cmd.ExecuteScalar().ToString());

        // Get number of rows
        cmd.CommandText = string.Format("select count(*) from {0}", currentTable);
        int rows = Math.Min(int.Parse(cmd.ExecuteScalar().ToString()), row_limit);

        // Get headers
        string[,] data = new string[rows, columns];
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

        // Definitions
        for (int col = 0; col < columns + 1; ++col)
        {
            table.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        }

        for (int row = 0; row < rows + 1; ++row)
        {
            table.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        table.ColumnDefinitions.Add(new ColumnDefinition(200));

        // Show
        for (int col = 0; col < columns; ++col)
        {
            table.Add(new Label() { Text = headers[col], FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center }, col, 0);
        }
        for (int row = 0; row < rows; ++row)
        {
            for (int col = 0; col < columns; ++col)
            {
                string value = data[row, col].ToString();
                // DB content
                table.Add(new Button() { Text = value, Command = new Command<string>(EditRow), CommandParameter = data[row, 0].ToString() + ";" + col.ToString() }, col, row + 1);
            }
            table.Add(new Button { Text = "DELETE", Command = new Command<string>(RemoveRow), CommandParameter = data[row, 0].ToString() }, columns, row + 1);
        }

        table.Add(new Button { Text = "ADD", Command = new Command(AddRow) }, columns + 1, 1);

        (scroll2 as IView).InvalidateMeasure();
    }

    private async void RemoveRow(string row_id)
    {
        int id = int.Parse(row_id);
        sql = string.Format("delete from {0} where id={1};", currentTable, id);
        cmd = new(sql, con);
        try
        {
            cmd.ExecuteNonQuery();
            ShowData();
        }
        catch (PostgresException e)
        {
            await DisplayAlert("Ошибка", e.MessageText, "OK");
        }
    }

    private async void EditRow(string inf_str)
    {
        string[] temp = inf_str.Split(";");
        int id = int.Parse(temp[0]);
        int col = int.Parse(temp[1]);

        cmd.CommandText = string.Format("select column_name from information_schema.columns where table_name = '{0}' and ordinal_position = {1};", currentTable, (col + 1).ToString());

        string columnName = cmd.ExecuteScalar().ToString();

        cmd.CommandText = string.Format("select {0} from {1} where id={2}", columnName, currentTable, id.ToString());
        string value = cmd.ExecuteScalar().ToString();

        string input = await DisplayPromptAsync("Изменить", columnName, "OK", "Отмена", null, -1, null, value);
        if (input is null || input == value)
        {
            return;
        }

        cmd.CommandText = string.Format("select data_type from information_schema.columns where table_name='{0}' and column_name='{1}'", currentTable, columnName);
        string type = cmd.ExecuteScalar().ToString();

        cmd.CommandText = string.Format("update {0} set {1} = cast('{2}' as {3}) where {4} = {5}", currentTable, columnName, input, type, "id", id);
        try
        {
            cmd.ExecuteNonQuery();
            ShowData();
        }
        catch (PostgresException e)
        {
            await DisplayAlert("Ошибка", e.MessageText, "OK");
        }

    }

    private async void AddRow()
    {
        await DisplayAlert("Ошибка", "sasay kok", "OK");
    }

    void ChangeTable(string name)
    {
        if (currentTable != name)
        {
            currentTable = name;
            ShowData();
        }
    }
}


*/