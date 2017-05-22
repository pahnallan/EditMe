using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Data;

namespace EditMe
{
    class FileHandler
    {
        char DELIMITER = ',';
        public bool file_valid = false;
        string parent_directory = "";

        public OpenFileDialog file_browser = new OpenFileDialog();
        public SaveFileDialog file_saver = new SaveFileDialog();

        public FileHandler()
        {
            file_saver.DefaultExt = ".csv";
            file_saver.Filter = "CSV files (*.csv)|*.csv";
        }

        public bool openFile()
        {
            file_browser.InitialDirectory = "./";
            file_valid = file_browser.ShowDialog() == DialogResult.OK && file_browser.FileName.Contains(".csv");
            parent_directory = file_valid ? parent_directory = Path.GetDirectoryName(file_browser.FileName) : "";
            return file_valid;
        }

        public bool isOpened()
        {
            return file_valid;
        }

        public string getFileName()
        {
            return file_valid ? file_browser.FileName : "";
        }

        public string getParentDir()
        {
            return file_valid ? parent_directory : "";
        }

        public DataTable getFileAsDT()
        {
            var dt = new DataTable();

            int max_columns = 0;
            int temp_count = 0;
            var lines = File.ReadLines(file_browser.FileName);
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
            lines
                .Select(x => x.Split(DELIMITER))
                .ToList()
                .ForEach(line => dt.Rows.Add(line));
            return dt;
        }

        public List<string> getSiblingFiles()
        {
            return Directory.GetFiles(parent_directory).ToList().FindAll(f => f.Contains(".csv"));
        }
    }
}
