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
using System.Drawing.Printing;

namespace EditMe
{
    public partial class MainForm : Form
    {
        private char DELIMITER = ',';
        public string opened_file_path = "";
        private string stringToPrint = "";
        public OpenFileDialog file_browser = new OpenFileDialog();
        public SaveFileDialog file_saver = new SaveFileDialog();

        public MainForm()
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

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFile();
        }

        private void imgSave_Click(object sender, EventArgs e)
        {
            saveFile();
        }
        #endregion 

        private void updateApplicationHeader()
        {
            this.Text = opened_file_path + " - EditMe";
        }

        public void new_file()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("");
            dgvCSV.DataSource = dt;
        }

        /// <summary>
        /// Returns a datatable of the CSV File parsed into the correct column and row
        /// </summary>
        /// <returns></returns>
        private DataTable openCSV(string file_name)
        {
            var dt = new DataTable();

            // Creating the columns
            //File.ReadLines(opened_file_path).Skip(1).Take(1)
            //    .SelectMany(x => x.Split(new[] { DELIMITER }, StringSplitOptions.RemoveEmptyEntries))
            //    .ToList()
            //    .ForEach(x => dt.Columns.Add(x.Trim()));
            opened_file_path = "C:\\StanceAnalyzer\\CSV_Files\\" + file_name;

            int max_columns = 0;
            int temp_count = 0;
            var lines = File.ReadLines(opened_file_path);
            foreach (var line in lines)
            {
                temp_count = line.Split(new[] { DELIMITER }, StringSplitOptions.RemoveEmptyEntries).Count();
                if (temp_count > max_columns)
                    max_columns = temp_count;
            }

            for (int i = 0; i < max_columns; i++)
            {
                dt.Columns.Add("");
            }

            // Adding the rows
            File.ReadLines(opened_file_path)
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
                try
                {
                    opened_file_path = file_saver.FileName;
                    saveFile();
                }
                catch
                {
                    MessageBox.Show("Save Failed", "Unable to save the file. Make sure the path selected exists.");
                }
            }
        }

        /// <summary>
        /// Takes the datagridview and saves it as a CSV file
        /// </summary>
        private void saveFile()
        {
            try
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
            catch (Exception e)
            {
                MessageBox.Show("Save Failed", "Unable to save due to the following: " + e.ToString());
            }
        }

        private void refreshFS()
        {
            if (!Directory.Exists("C:\\StanceAnalyzer\\CSV_Files\\"))
            {
                lblNoCSV.Visible = true;
                return;
            }
            string[] list_of_string = Directory.GetFiles("C:\\StanceAnalyzer\\CSV_Files\\");
            if (list_of_string.Count() == 0)
            {
                lblNoCSV.Visible = true;
                return;
            }
            lblNoCSV.Visible = false;
            dgvFS.Rows.Clear();
            list_of_string.ToList().ForEach(line => dgvFS.Rows.Add(Path.GetFileName(line)));
            dgvFS.ClearSelection();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            refreshFS();
        }

        private void dgvFS_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            btnOpen.Enabled = true;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                string selected_file = dgvFS.SelectedCells[0].Value.ToString();
                dgvCSV.DataSource = openCSV(selected_file);
                updateApplicationHeader();
            }
            catch
            {
                MessageBox.Show("Invalid CSV File", "File does not match CSV format");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists("C:\\StanceAnalyzer\\CSV_Files"))
                Directory.CreateDirectory("C:\\StanceAnalyzer\\CSV_Files");
            refreshFS();
        }

        private void dgvFS_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string selected_file = dgvFS.SelectedCells[0].Value.ToString();
            dgvCSV.DataSource = openCSV(selected_file);
            updateApplicationHeader();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (opened_file_path == "")
            {
                MessageBox.Show("Unable to print. Please open a CSV File before attempting to print.", "Print Message");
                return;
            }
            readFile();
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += new PrintPageEventHandler(this.pd_PrintPage);
            using (PrintDialog printDialog = new PrintDialog())
            {
                string dateTimeTemp = string.Format("{0:MM-dd-yyyy HH:mm:ss - ff}", DateTime.Now);
                printDialog.Document = pd;
                printDialog.Document.DocumentName = "-Main-" + dateTimeTemp;

                if (printDialog.ShowDialog(this) == DialogResult.OK)
                    printDialog.Document.Print();
            }
        }

        private void readFile()
        {
            using (FileStream stream = new FileStream(opened_file_path, FileMode.Open))
            using (StreamReader reader = new StreamReader(stream))
            {
                stringToPrint = reader.ReadToEnd();
            }
        }

        private void pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            int charactersOnPage = 0;
            int linesPerPage = 0;

            // Sets the value of charactersOnPage to the number of characters 
            // of stringToPrint that will fit within the bounds of the page.
            e.Graphics.MeasureString(stringToPrint, this.Font,
                e.MarginBounds.Size, StringFormat.GenericTypographic,
                out charactersOnPage, out linesPerPage);

            // Draws the string within the bounds of the page
            e.Graphics.DrawString(stringToPrint, this.Font, Brushes.Black,
                e.MarginBounds, StringFormat.GenericTypographic);

            // Remove the portion of the string that has been printed.
            stringToPrint = stringToPrint.Substring(charactersOnPage);

            // Check to see if more pages are to be printed.
            e.HasMorePages = (stringToPrint.Length > 0);
        }

    }
}