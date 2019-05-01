using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> machineList = new Dictionary<string, string>();
        private static string machineListFilePath = @"C:\Presentations\Machines.csv";

        public Form1()
        {
            InitializeComponent();
            LoadMachines(machineListFilePath);
        }

        #region Core
        /// <summary>
        /// Opens a FileDialogBox, confirms a file has been selected, and sets the selected files path to the related textbox.
        /// </summary>
        private void SelectFile()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"C:\Presentations\",
                Title = "Select",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "pptx",
                Filter = "Power Point Presentation Files (*.pptx)|*.pptx",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtSelectedFile.Text = openFileDialog1.FileName;
            }
        }

        /// <summary>
        /// Sends the file matching the given filename to the requested IP address.
        /// </summary>
        /// <param name="filename"></param>
        private void SendFile(String filename)
        {
            if (txtSelectedFile.Text != "")
            {
                txtStatus.Text = "Connecting... ";
                int port = 8000;
                string ip = txtIPAddress.Text;
                try
                {
                    IPAddress.Parse(ip);
                    txtStatus.Text = "Sending File... ";
                    TcpClient soc = new TcpClient(ip, port);
                    soc.Client.SendFile(filename);
                    txtStatus.Text = "File Uploaded.";
                    soc.Close();
                }
                catch (SocketException err)
                {
                    txtStatus.Text = "Status";
                    MessageBox.Show("An error has occurred.\nPlease contact your system administrator with this message.\n\nError: " + err.Message, "Error!");
                }
            }
            else
            {
                string message = "Please select a file!";
                string title = "No file selected";
                MessageBox.Show(message, title);
            }

        }
        #endregion

        #region Machines
        /// <summary>
        /// Loads machine names and IP addresses from a local .csv file
        /// </summary>
        /// <param name="filepath"></param>
        private void LoadMachines(string filepath)
        {
            using (var reader = new StreamReader(filepath))
            using (var csv = new CsvReader(reader))
            {
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    machineList.Add(csv.GetField("Key"), csv.GetField("Value"));
                }
            }
            UpdateFields();
        }

        /// <summary>
        /// Saves machine names and IP addresses to a local .csv file
        /// </summary>
        /// <param name="filepath"></param>
        private void SaveMachines(string filepath)
        {
            using (var writer = new StreamWriter(filepath))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(machineList);
            }
        }

        /// <summary>
        /// Update the machine name comboBox to display the local records and saves them to the local .csv file
        /// </summary>
        private void UpdateFields()
        {
            SaveMachines(machineListFilePath);
            cbxMachineName.Items.Clear();
            foreach (KeyValuePair<string, string> machine in machineList)
            {
                cbxMachineName.Items.Add(machine.Key);
            }
        }

        /// <summary>
        /// Updates the current record in the dictionary of machines with a new value
        /// </summary>
        private void UpdateMachine()
        {
            machineList[cbxMachineName.Text] = txtIPAddress.Text;
            UpdateFields();
        }

        /// <summary>
        /// Adds a new record into the dictionary of machines
        /// </summary>
        private void AddMachine()
        {
            if (cbxMachineName.Text != "" && txtIPAddress.Text != "")
            {
                machineList.Add(cbxMachineName.Text, txtIPAddress.Text);
                UpdateFields();
            }
            else
            {
                MessageBox.Show("Please enter a machine name and IP address.", "Message");
            }
        }

        /// <summary>
        /// Deletes the current machine from the dictionary of machines
        /// </summary>
        private void DeleteMachine()
        {
            try
            {
                machineList.Remove(cbxMachineName.Text);
                UpdateFields();
            }
            catch (ArgumentOutOfRangeException err)
            {
                MessageBox.Show("No matching machine found.", "Message");
            }
            catch (Exception err)
            {
                MessageBox.Show("An error has occurred.\nPlease contact your system adminitrator.\n\nError:" + err, "Error");
            }

        }
        #endregion

        #region EventListeners
        private void BtnSelect_Click(object sender, EventArgs e)
        {
            SelectFile();
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            SendFile(txtSelectedFile.Text);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            DeleteMachine();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cbxMachineName.Items.Contains(cbxMachineName.Text))
            {
                UpdateMachine();
            }
            else
            {
                AddMachine();
            }
        }

        private void CbxMachineName_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtIPAddress.Text = machineList[cbxMachineName.Text];
        }
        #endregion
    }
}
