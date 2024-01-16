using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;

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
                                    .Where(row => int.Parse(row.Field<string>("hour_of_day")) == hour  // Convert to int for comparison
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

        private void button3_Click(object sender, EventArgs e)
        {
            

                Form1 form1 = new Form1();
                form1.Show();
                this.Hide();
           
        }

        private void csvWrite(object sender, EventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";
            saveFileDialog.Title = "Salvar ficheiro CSV";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                using (StreamWriter streamWriter = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    {
                        streamWriter.Write(dataGridView1.Columns[i].HeaderText);
                        if (i < dataGridView1.Columns.Count - 1)
                            streamWriter.Write(",");
                    }
                    streamWriter.WriteLine();

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        for (int i = 0; i < dataGridView1.Columns.Count; i++)
                        {
                            streamWriter.Write(row.Cells[i].Value);
                            if (i < dataGridView1.Columns.Count - 1)
                                streamWriter.Write(",");
                        }
                        streamWriter.WriteLine();
                    }
                }

                MessageBox.Show("Ficheiro CSV exportado com sucesso.", "Exportado com sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void excelWrite(object sender, EventArgs e)
        {
            try
            {
                
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
                    saveFileDialog.Title = "Salvar ficheiro Excel";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveFileDialog.FileName;

                        using (OfficeOpenXml.ExcelPackage package = new ExcelPackage())
                        {
                            OfficeOpenXml.ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Planilha1");

                            
                            for (int i = 1; i <= dataGridView1.Columns.Count; i++)
                            {
                                worksheet.Cells[1, i].Value = dataGridView1.Columns[i - 1].HeaderText;
                            }

                           
                            for (int i = 1; i <= dataGridView1.Rows.Count; i++)
                            {
                                for (int j = 1; j <= dataGridView1.Columns.Count; j++)
                                {
                                    worksheet.Cells[i + 1, j].Value = dataGridView1.Rows[i - 1].Cells[j - 1].Value;
                                }
                            }

                            
                            package.SaveAs(new FileInfo(filePath));
                        }

                        MessageBox.Show("Ficheiro Excel exportado com sucesso.", "Exportado com sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro ao exportar para o Excel: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
