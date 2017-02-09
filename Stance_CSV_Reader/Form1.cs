using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Stance_CSV_Reader
{
    public partial class Form1 : Form
    {
        private char DELIMITER = ',';
        public string opened_file_path = "";
        public OpenFileDialog file_browser = new OpenFileDialog();
        public SaveFileDialog file_saver = new SaveFileDialog();

        public Form1()
        {
            InitializeComponent();
            file_saver.DefaultExt = ".csv";
            file_saver.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
        }
        #region Event Handlers

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)       // Ctrl-S Save
            {
                e.SuppressKeyPress = true;  // Stops other controls on the form receiving event.
                saveFile(); // Saves the file
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAsFile();
        }

        private void mnuOpen_Click(object sender, EventArgs e)
        {
            open_file();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFile();
        }



        private void imgNew_Click(object sender, EventArgs e)
        {
            new_file();
        }

        private void imgOpen_Click(object sender, EventArgs e)
        {
            open_file();
        }

        private void imgSave_Click(object sender, EventArgs e)
        {
            saveFile();
        }
        #endregion 

        private void updateApplicationHeader()
        {
            this.Text = opened_file_path + " - Stance Reader";
        }

        public void new_file()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("");
            dgvCSV.DataSource = dt;
        }

        public void open_file()
        {
            if (Directory.Exists("C:\\StanceAnalyzer"))
            {
                file_browser.InitialDirectory = "C:\\StanceAnalyzer";
            }
            DialogResult result = file_browser.ShowDialog();
            if (result == DialogResult.OK)
            {
                opened_file_path = file_browser.FileName;
                Console.WriteLine(opened_file_path);
                dgvCSV.DataSource = openCSV();
                updateApplicationHeader();
            }
            
        }

        /// <summary>
        /// Returns a datatable of the CSV File
        /// </summary>
        /// <returns></returns>
        private DataTable openCSV()
        {
            var dt = new DataTable();
            // Creating the columns
            File.ReadLines(opened_file_path).Take(1)
                .SelectMany(x => x.Split(new[] { DELIMITER }, StringSplitOptions.RemoveEmptyEntries))
                .ToList()
                .ForEach(x => dt.Columns.Add(x.Trim()));

            // Adding the rows
            File.ReadLines(opened_file_path).Skip(1)
                .Select(x => x.Split(DELIMITER))
                .ToList()
                .ForEach(line => dt.Rows.Add(line));
            return dt;
        }

        /// <summary>
        /// Lets a user choose where to save a file and saves dgv to CSV.
        /// </summary>
        private void saveAsFile()
        {
            if (file_saver.ShowDialog() == DialogResult.OK)
            {
                opened_file_path = file_saver.FileName;
                saveFile();
            }
        }

        /// <summary>
        /// Takes the datagridview and saves it as a CSV file
        /// </summary>
        private void saveFile()
        {
            // If you're currently inside a cell editing, save that edit before building new csv string
            dgvCSV.EndEdit();
            var sb = new StringBuilder();

            var headers = dgvCSV.Columns.Cast<DataGridViewColumn>();
            sb.AppendLine(string.Join(",", headers.Select(column => column.HeaderText).ToArray()));

            foreach (DataGridViewRow row in dgvCSV.Rows)
            {
                if (!row.IsNewRow)
                {
                    var cells = row.Cells.Cast<DataGridViewCell>();
                    sb.AppendLine(string.Join(",", cells.Select(cell => cell.Value).ToArray()));
                }
            }
            System.IO.File.WriteAllText(opened_file_path, sb.ToString());
            updateApplicationHeader();
        }
    }
}
