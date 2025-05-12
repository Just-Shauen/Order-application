using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;


namespace AppOrder
{
    public partial class Form1 : Form
    {
        private string connectionString = "Data Source=customers.db;Version=3;";


        public Form1()
        {
            InitializeComponent();
            EnsureDatabaseSetup();
            LoadCustomers();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string name = txtName.Text;
            int age;
            if (int.TryParse(txtAge.Text, out age))
            {
                string query = "INSERT INTO Customers (Name, Age) VALUES (@Name, @Age)";

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    SQLiteCommand cmd = new SQLiteCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Age", age);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                LoadCustomers();
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите правильный возраст.");
            }
        }

        private void LoadCustomers()
        {
            listBoxCustomers.Items.Clear();
            string query = "SELECT Id, Name FROM Customers";

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                conn.Open();
                SQLiteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string customer = reader["Name"].ToString();
                    listBoxCustomers.Items.Add(new ListItem
                    {
                        Id = Convert.ToInt32(reader["Id"]),                                                                                                                                                                                                                                               
                        Name = customer
                    });
                }

                conn.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBoxCustomers.SelectedItem != null)
            {
                ListItem selectedCustomer = (ListItem)listBoxCustomers.SelectedItem;
                LoadOrders(selectedCustomer.Id);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите клиента.");
            }

        }
        private void EnsureDatabaseSetup()
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string createCustomersTable = @"
            CREATE TABLE IF NOT EXISTS Customers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Age INTEGER NOT NULL
            );";

                string createOrdersTable = @"
            CREATE TABLE IF NOT EXISTS Orders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerId INTEGER NOT NULL,
                OrderDetails TEXT NOT NULL,
                FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
            );";

                new SQLiteCommand(createCustomersTable, conn).ExecuteNonQuery();
                new SQLiteCommand(createOrdersTable, conn).ExecuteNonQuery();
            }
        }


        private void LoadOrders(int customerId)
        {
            dataGridViewOrders.Rows.Clear();
            dataGridViewOrders.Columns.Clear();
            dataGridViewOrders.Columns.Add("OrderDetails", "Детали заказа");

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT OrderDetails FROM Orders WHERE CustomerId = @CustomerId";
                var cmd = new SQLiteCommand(query, conn);
                cmd.Parameters.AddWithValue("@CustomerId", customerId);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    dataGridViewOrders.Rows.Add(reader["OrderDetails"].ToString());
                }
            }
        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (listBoxCustomers.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента.");
                return;
            }

            var selected = (ListItem)listBoxCustomers.SelectedItem;
            int customerId = selected.Id;

            dataGridViewOrders.Rows.Clear();
            string query = "SELECT OrderDetails FROM Orders WHERE CustomerId = @CustomerId";

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                cmd.Parameters.AddWithValue("@CustomerId", customerId);

                SQLiteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    dataGridViewOrders.Rows.Add(reader["OrderDetails"].ToString());
                }

                conn.Close();
            }
        }
        public class ListItem
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }


        private void FilterCustomers(string namePart)
        {
            listBoxCustomers.Items.Clear();
            string query = "SELECT Id, Name FROM Customers WHERE Name LIKE @NamePart";

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                cmd.Parameters.AddWithValue("@NamePart", "%" + namePart + "%");
                conn.Open();

                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    listBoxCustomers.Items.Add(new ListItem
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString()
                    });
                }

                conn.Close();
            }
        }


        private void txtName_TextChanged(object sender, EventArgs e)
        {
            FilterCustomers(txtName.Text);

        }
















        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {


        }

        

        private void txtAge_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text files (*.txt)|*.txt";
            sfd.FileName = "filtered_orders.txt";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                {
                    // Заголовки колонок
                    for (int i = 0; i < dataGridViewOrders.Columns.Count; i++)
                    {
                        sw.Write(dataGridViewOrders.Columns[i].HeaderText);
                        if (i < dataGridViewOrders.Columns.Count - 1)
                            sw.Write("\t"); // Используем табуляцию вместо запятой для разделения
                    }
                    sw.WriteLine();

                    // Данные строк (только отфильтрованные заказы)
                    foreach (DataGridViewRow row in dataGridViewOrders.Rows)
                    {
                        if (!row.IsNewRow && row.Visible) // Только видимые строки (отфильтрованные)
                        {
                            for (int i = 0; i < dataGridViewOrders.Columns.Count; i++)
                            {
                                sw.Write(row.Cells[i].Value?.ToString());
                                if (i < dataGridViewOrders.Columns.Count - 1)
                                    sw.Write("\t"); // Используем табуляцию для разделения
                            }
                            sw.WriteLine();
                        }
                    }
                }

                // Сообщение об успешном экспорте
                MessageBox.Show("Успешно экспортированы отфильтрованные заказы в файл " + sfd.FileName);
            }
        }

    }
}
