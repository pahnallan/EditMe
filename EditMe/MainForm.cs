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
        private FileHandler fh = new FileHandler();
        private string stringToPrint = "";
        private string opened_file_path = "";

        public MainForm()
        {
            InitializeComponent();
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

        private void imgSave_Click(object sender, EventArgs e)
        {
            saveFile();
        }
        #endregion 
     

        public void new_file()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("");
            dgvCSV.DataSource = dt;
        }

        /// <summary>
        /// Takes the datagridview and saves it as a CSV file
        /// </summary>
        private void saveFile()
        {
            try
            {

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
                if (fh.openFile())
                {
                    dgvCSV.DataSource = fh.getFileAsDT();
                    fillFileSystemDir(fh.getSiblingFiles());
                }
            }
            catch
            {
                MessageBox.Show("Invalid CSV File", "File does not match CSV format");
            }
        }

        private void fillFileSystemDir(List<string> csv_files)
        {
            dgvFS.Rows.Clear();
            csv_files.ForEach(line => dgvFS.Rows.Add(Path.GetFileName(line)));
            lblNoCSV.Visible = dgvFS.Rows.Count <= 0;
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
            //dgvCSV.DataSource = openCSV(selected_file);
            //updateApplicationHeader();
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