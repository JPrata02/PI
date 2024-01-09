using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibProj
{
    public partial class Form2 : Form
    {
        private static SQLiteConnection conStr;

        public Form2(string connectionString)
        {
            InitializeComponent();
            conStr = new SQLiteConnection(connectionString);
        }
     

        private void button1_Click(object sender, EventArgs e)
        {
            DateTime startDate = dateTimePickerStart.Value;
            DateTime endDate = dateTimePickerEnd.Value;


            showCount(startDate, endDate);

        }


        private void showCount(DateTime startDate, DateTime endDate)
        {
            using (SQLiteConnection connection = new SQLiteConnection(conStr))
            {
                connection.Open();

                string query = "SELECT *, strftime('%H', sqltime) as hour_of_day FROM mobilidade";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        DataTable resultTable = new DataTable();
                        resultTable.Columns.Add("Dia da Semana/Mês", typeof(string));

                        for (int hour = 9; hour <= 18; hour++)
                        {
                            resultTable.Columns.Add(hour.ToString(), typeof(int));
                        }

                        DateTime currentDate = startDate;
                        int cumulativeCount = 0;

                        while (currentDate <= endDate)
                        {
                            DataRow newRow = resultTable.NewRow();
                            newRow["Dia da Semana/Mês"] = $"{currentDate:dd/MM (ddd)}";

                            for (int hour = 9; hour <= 18; hour++)
                            {
                                DateTime currentDateTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, hour, 0, 0);
                                DateTime nextDateTime = currentDateTime.AddHours(1);

                               
                                var hourRows = dataTable.AsEnumerable()
                                    .Where(row => row.Field<string>("hour_of_day") == hour.ToString()
                                               && row.Field<DateTime>("sqltime") >= currentDateTime
                                               && row.Field<DateTime>("sqltime") < nextDateTime);
                                int totalCount = 0;

                                foreach (DataRow row in hourRows)
                                {
                                    int entradaCount = row.Field<string>("tipo") == "Entrada" ? 1 : 0;
                                    int saidaCount = row.Field<string>("tipo") == "Saida" ? 1 : 0;

                                    totalCount += entradaCount - saidaCount;
                                }

                                cumulativeCount += totalCount; 
                                newRow[hour.ToString()] = cumulativeCount;
                            }

                            resultTable.Rows.Add(newRow);

                            currentDate = currentDate.AddDays(1);
                            cumulativeCount = 0; 
                        }

                        dataGridView1.DataSource = resultTable;
                    }
                }
            }

        }



        private void Form2_Load(object sender, EventArgs e)
        { 
            
        }
    }
}
