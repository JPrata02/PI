using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data.SQLite;
using System.Data;

namespace LibProj
{
    public partial class Form1 : Form
    {
      

        private static string dbPath = "C:\\Users\\prata\\Desktop\\Files\\libraby.db";
        private static string conString = "Data Source=" + dbPath + ";Version=3;New=False;Compress=True";

 

        private Timer updateTimer = new Timer();

        string teste = "teste";

        string doughnutSeriesName = "Contagem";
        string columnSeriesName = "Contagem";

        List<string> xDoughnut = new List<string> { "Seg", "Ter", "Qua", "Qui", "Sex" };
        List<string> yDoughnut = new List<string> { "20", "20", "15", "10", "35" };

        List<string> xColumn = new List<string> { "1(Seg)", "2(Ter)", "3(Qua)", "4(Qui)", "5(Sex)" };
        List<string> yColumn = new List<string> { "20", "20", "15", "10", "35" };
        public Form1()
        {
            InitializeComponent();
           
       
        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
           
            LoadColumnChart();
        
            LoadDoughnutChart();
       

        }

     

        private void LoadColumnChart()
        {
            chart1.Series.Clear();


            chart1.Series.Add(columnSeriesName);
            chart1.Series[columnSeriesName].Color = Color.Green;

            if (xColumn.Count != yColumn.Count)
            {
                MessageBox.Show("The lists xColumn and yColumn must have the same number of elements.");
                return;
            }


            for (int i = 0; i < xColumn.Count; i++)
            {
                chart1.Series[columnSeriesName].Points.AddXY(xColumn[i], yColumn[i]);
            }


            chart1.Series[columnSeriesName].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;

            chart1.BackColor = Color.LightBlue;
            chart1.ChartAreas[0].BackColor = Color.LightBlue;

            chart1.Invalidate();
        }

        private void LoadDoughnutChart()
        {
            chart2.Series.Clear();
            chart2.Series.Add(doughnutSeriesName);
          
          

            for (int i = 0; i < xDoughnut.Count; i++)
            {
                chart2.Series[doughnutSeriesName].IsValueShownAsLabel = true;
                chart2.Series[doughnutSeriesName].Points.AddXY(xDoughnut[i], yDoughnut[i]);
            }

            chart2.Series[doughnutSeriesName].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Doughnut;
            chart2.Series[doughnutSeriesName]["DoughnutRadius"] = "30%";

            chart1.BackColor = Color.LightBlue;
            chart1.ChartAreas[0].BackColor = Color.LightBlue;
            chart2.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            Form2 form2 = new Form2(conString);
            form2.Show();
            this.Hide();
        }
    }



  
}
