using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> machineList = new Dictionary<string, string>();

        public Form1()
        {
            InitializeComponent();
            LoadMachines();
        }

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
                    if (!cbxMachineName.Items.Contains(cbxMachineName.Text))
                    {
                        cbxMachineName.Items.Add(cbxMachineName.Text);
                    }
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

        private void LoadMachines()
        {
            cbxMachineName.Items.Clear();
            foreach (KeyValuePair<string, string> machine in machineList)
            {
                cbxMachineName.Items.Add(machine.Key);
            }
        }

        private void UpdateMachine()
        {
            machineList[cbxMachineName.Text] = txtIPAddress.Text;
        }

        private void AddMachine()
        {
            if (cbxMachineName.Text != "" && txtIPAddress.Text != "")
            {
                machineList.Add(cbxMachineName.Text, txtIPAddress.Text);
                LoadMachines();
            }
            else
            {
                MessageBox.Show("Please enter a machine name and IP address.", "Message");
            }
        }
        private void DeleteMachine()
        {
            try
            {
                machineList.Remove(cbxMachineName.Text);
                LoadMachines();
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
    }
}
