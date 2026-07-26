using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WMAExcel;

namespace WMAExcelValidator
{
    public partial class MainForm : Form
    {
        private const string LastFileNameConfig = "last-validated-file.txt";
        private string currentFileName;

        public MainForm()
        {
            InitializeComponent();
            LoadLastFileName();
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls|All files (*.*)|*.*";
                dlg.Title = "Open WMA Excel file";

                if (dlg.ShowDialog(this) == DialogResult.OK)
                    ValidateFile(dlg.FileName);
            }
        }

        private void BtnValidate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(currentFileName))
            {
                textNotes.Clear();
                textNotes.Text = "Open or drop an Excel file first.";
                return;
            }

            textNotes.Clear();
            ValidateFile(currentFileName);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length == 0)
                return;

            ValidateFile(files[0]);
        }

        private void ValidateFile(string fileName)
        {
            currentFileName = fileName;
            textFileName.Text = fileName;
            btnValidate.Enabled = true;
            List<string> notes = new List<string>();
            SaveLastFileName(fileName, notes);

            try
            {
                if (!IsExcelFile(fileName))
                    notes.Add("Selected file is not an Excel file: " + fileName);

                ExcelExperiment experiment = new ExcelExperiment();
                if (!experiment.LoadFromFile(fileName))
                {
                    notes.Add("Failed to load Excel file: " + fileName);
                    ShowNotes(notes);
                    return;
                }

                foreach (ExcelInputData inputData in experiment.inputData)
                {
                    notes.Add("InputData_" + inputData.Index + ":");
                    int before = notes.Count;
                    Validation.Validate(inputData, notes);
                    AddNoIssuesNote(notes, before);
                }

                string rootDir = Path.GetDirectoryName(fileName);
                foreach (int inputDataNum in experiment.GetInputDataListSync())
                {
                    notes.Add("Assets for InputData_" + inputDataNum + ":");
                    int before = notes.Count;

                    if (experiment.SelectInputdata(inputDataNum))
                        Validation.ValidateAssets(experiment, rootDir, notes);
                    else
                        notes.Add("Failed to select InputData_" + inputDataNum + ".");

                    AddNoIssuesNote(notes, before);
                }
            }
            catch (Exception ex)
            {
                notes.Add("Validation failed: " + ex.Message);
            }

            ShowNotes(notes);
        }

        private void LoadLastFileName()
        {
            try
            {
                string configPath = GetLastFileNameConfigPath();
                if (!File.Exists(configPath))
                    return;

                string fileName = File.ReadAllText(configPath).Trim();
                if (string.IsNullOrWhiteSpace(fileName))
                    return;

                currentFileName = fileName;
                textFileName.Text = fileName;
                btnValidate.Enabled = true;
                textNotes.Text = "Last validated file restored. Click Validate to run checks again.";
            }
            catch
            {
                currentFileName = null;
                btnValidate.Enabled = false;
            }
        }

        private static void SaveLastFileName(string fileName, List<string> notes)
        {
            try
            {
                string configPath = GetLastFileNameConfigPath();
                Directory.CreateDirectory(Path.GetDirectoryName(configPath));
                File.WriteAllText(configPath, fileName ?? string.Empty, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                notes.Add("Failed to save last validated file name: " + ex.Message);
            }
        }

        private static string GetLastFileNameConfigPath()
        {
            return Path.Combine(Application.UserAppDataPath, LastFileNameConfig);
        }

        private static bool IsExcelFile(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            return string.Equals(ext, ".xlsx", StringComparison.OrdinalIgnoreCase)
                || string.Equals(ext, ".xls", StringComparison.OrdinalIgnoreCase);
        }

        private static void AddNoIssuesNote(List<string> notes, int previousCount)
        {
            if (notes.Count == previousCount)
                notes.Add("No issues found.");
        }

        private void ShowNotes(List<string> notes)
        {
            if (notes.Count == 0)
            {
                textNotes.Text = "No issues found.";
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (string note in notes)
                sb.AppendLine(note);

            textNotes.Text = sb.ToString();
        }
    }
}
