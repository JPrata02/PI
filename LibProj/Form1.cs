using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data.SQLite;
using System.Data;
using System.Linq;

namespace LibProj
{
    public partial class Form1 : Form
    {
      

        private static string dbPath = "C:\\Users\\prata\\Desktop\\Files\\libraby.db";
        private static string conString = "Data Source=" + dbPath + ";Version=3;New=False;Compress=True";

 

        private Timer updateTimer = new Timer();

      

        public Form1()
        {
            InitializeComponent();
            updateTimer = new Timer();
            updateTimer.Interval = 1000 * 20; 
            updateTimer.Tick += UpdateCurrentPeopleCountLabel;
            updateTimer.Tick += UpdateMonthUsersLabel;
            updateTimer.Tick += UpdateLabelDailyUsersAvg;
            updateTimer.Start();

            
            UpdateCurrentPeopleCountLabel(null, null);
            UpdateMonthUsersLabel(null, null);
            UpdateLabelDailyUsersAvg(null, null);
        }

        private void Form1_Load(object sender, EventArgs e)
        {


            ShowContagemPerDayColumnGraph();


            ShowContagemPerWeekDayDoughnutChart();
            

        }

        private void UpdateMonthUsersLabel(object sender, EventArgs e)
        {
            using (SQLiteConnection connection = new SQLiteConnection(conString))
            {
                connection.Open();

               
                string query = "SELECT COUNT(*) AS EntradaCount FROM mobilidade WHERE tipo = 'Entrada' AND strftime('%Y-%m', sqltime) = strftime('%Y-%m', 'now')";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        int entradaCount = Convert.ToInt32(result);
                        labelMonthUsersLabel.Text = $"{entradaCount}";
                    }
                    else
                    {
                        labelMonthUsersLabel.Text = "N/A";
                    }
                }
            }
        }

        private void UpdateLabelDailyUsersAvg(object sender, EventArgs e)
        {
            using (SQLiteConnection connection = new SQLiteConnection(conString))
            {
                connection.Open();

                
                string query = "SELECT AVG(DailyCount) AS AverageDailyUsers FROM (SELECT COUNT(*) AS DailyCount FROM mobilidade WHERE tipo = 'Entrada' GROUP BY strftime('%Y-%m-%d', sqltime))";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        double averageDailyUsers = Convert.ToDouble(result);
                        labelDailyUsersMedia.Text = $"{averageDailyUsers:F2}"; 
                    }
                    else
                    {
                        labelDailyUsersMedia.Text = "N/A";
                    }
                }
            }
        }

        private void ShowContagemPerDayColumnGraph()
        {
            using (SQLiteConnection connection = new SQLiteConnection(conString))
            {
                connection.Open();

                string query = "SELECT strftime('%d-%m', sqltime) as day, " +
                               "COUNT(CASE WHEN tipo = 'Entrada' THEN 1 END) AS Contagem " +
                               "FROM mobilidade GROUP BY day";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        chart1.Series.Clear(); 
                        chart1.Series.Add("Contagem");

                        while (reader.Read())
                        {
                            string day = reader["day"].ToString();
                            int peopleCount = Convert.ToInt32(reader["Contagem"]);

                            chart1.Series["Contagem"].Points.AddXY(day, peopleCount);
                        }
                        chart1.Titles.Clear();
                        chart1.Titles.Add("Contagem Mensal de Presenças");
                    }
                }
            }
        }

        private void ShowContagemPerWeekDayDoughnutChart()
        {
            using (SQLiteConnection connection = new SQLiteConnection(conString))
            {
                connection.Open();

                string query = "SELECT strftime('%w', sqltime) as weekday, " +
                               "COUNT(CASE WHEN tipo = 'Entrada' THEN 1 END) AS Contagem " +
                               "FROM mobilidade WHERE weekday <> '0' GROUP BY weekday";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        chart2.Series.Clear(); 
                        chart2.Series.Add("Contagem"); 
                        chart2.Series["Contagem"].ChartType = SeriesChartType.Doughnut;
                        chart2.Series["Contagem"].IsValueShownAsLabel = true; 

                        int totalPeopleCount = 0;

                        while (reader.Read())
                        {
                            int weekdayNumber = Convert.ToInt32(reader["weekday"]);
                            string weekdayName = GetWeekDayName(weekdayNumber);
                            int peopleCount = Convert.ToInt32(reader["Contagem"]);

                            DataPoint dataPoint = new DataPoint();
                            dataPoint.SetValueY(peopleCount);
                            dataPoint.AxisLabel = weekdayName;
                            chart2.Series["Contagem"].Points.Add(dataPoint);

                            totalPeopleCount += peopleCount;
                        }

                        chart2.Titles.Clear();
                        chart2.Titles.Add("Presenças por dia da semana(%)");

                        if (totalPeopleCount > 0)
                        {
                           
                            foreach (DataPoint point in chart2.Series["Contagem"].Points)
                            {
                                double percentage = (point.YValues[0] / totalPeopleCount) * 100;
                                point.Label = $"{point.AxisLabel}: {percentage:F2}%";
                            }
                        }
                    }
                }
            }
        }
        private string GetWeekDayName(int weekdayNumber)
        {
            string[] weekDays = { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sab" };
            return weekDays[weekdayNumber];
        }
        private void UpdateCurrentPeopleCountLabel(object sender, EventArgs e)
        {
            using (SQLiteConnection connection = new SQLiteConnection(conString))
            {
                connection.Open();

               
                string query = "SELECT tipo, COUNT(*) as Count FROM mobilidade WHERE sqltime >= datetime('now', 'start of day') GROUP BY tipo";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        int currentCount = 0;

                        while (reader.Read())
                        {
                            string tipo = reader["tipo"].ToString();
                            int count = Convert.ToInt32(reader["Count"]);

                            if (tipo == "Entrada")
                            {
                                currentCount += count;
                            }
                            else if (tipo == "Saida")
                            {
                                currentCount -= count;
                            }
                        }

                        labelPeopleCount.Text = $"{Math.Max(0, currentCount)}"; 
                    }
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {

            Form2 form2 = new Form2(conString);
            form2.Show();
            this.Hide();
        }
    }



  
}
