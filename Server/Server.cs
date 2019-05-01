using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Server
    {
        private ConsoleSpinner spinner = new ConsoleSpinner();
        private TcpListener listener = new TcpListener(IPAddress.Any, 100);
        private Process powerPoint;
        private const string archiveFilepath = @"C:\Presentations\Archive\Presentation";
        private const string filePath = @"C:\Presentations\Presentation";
        private const string fileExt = ".pptx";
        private const int port = 8000;
        private string ip;

        public Server()
        {
            CreateDependencies();
            ip = GetLocalIPAddress();
            StartServer();
        }

        #region Server
        /// <summary>
        /// Create any required local directories if not already.
        /// </summary>
        public void CreateDependencies()
        {
            Console.Out.Write("Checking dependencies... ");
            Directory.CreateDirectory(@"C:\Presentations\Archive");
            Console.Out.WriteLine("Done.");
        }

        /// <summary>
        /// Returns the IP address of the local machine.
        /// </summary>
        /// <returns>IP address in string format</returns>
        public string GetLocalIPAddress()
        {
            Console.Out.Write("Getting local IP address... ");
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Console.Out.WriteLine("Done.");
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        /// <summary>
        /// Attempt to connect the listener to the IP address on the specified port.
        /// </summary>
        public void StartServer()
        {
            Console.Write("Opening port: " + port + "... ");
            try
            {
                listener = new TcpListener(IPAddress.Parse(ip), port);
            }
            catch
            {
                Console.WriteLine("Error: Unable to open port.");
                Console.ReadLine();
                Environment.Exit(0);
            }
            Console.WriteLine("Done.");
            StartListening();
        }

        /// <summary>
        /// Start the listener and wait for a connection.
        /// </summary>
        public void StartListening()
        {
            listener.Start();
            Console.Write("Waiting for connection on: " + ip + ":" + port);
            while (true)
            {
                if (listener.Pending())
                {
                    Console.WriteLine("... Connected!");
                    Console.CursorVisible = true;
                    ClosePresentation();
                    System.Threading.Thread.Sleep(2000);
                    ArchivePresentation();
                    break;
                }
                spinner.Run();
            }
            ReceiveFile();
        }
        #endregion

        #region FileOperations
        /// <summary>
        /// Move the old presentation file into the archive folder with a corresponding time stamp.
        /// </summary>
        public void ArchivePresentation()
        {
            try
            {
                Console.Out.Write("Archiving old presentation... ");
                if (File.Exists(filePath + fileExt))
                {
                    File.Move(filePath + fileExt, archiveFilepath + DateTime.Now.ToString("ddMyy-HHmmss") + fileExt);
                }
                Console.Out.WriteLine("Done.");
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Receive the file from the client and save it at the specified directory.
        /// </summary>
        public void ReceiveFile()
        {
            using (var client = listener.AcceptTcpClient())
            using (var stream = client.GetStream())
            using (var output = File.Create(filePath + fileExt))
            {
                Console.Write("Reciving file... ");

                // read the file in chunks of 1KB (as default)
                var buffer = new byte[1024];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, bytesRead);
                }
            }

            Console.WriteLine("Done.");
            Console.Write("Closing port... ");
            listener.Stop();
            Console.WriteLine("Done.");
            OpenPresentation();
            StartListening();
        }
        #endregion
               
        #region PowerPoint
        /// <summary>
        /// Returns a string of the filepath corresponding to the installed power point executable path.
        /// </summary>
        /// <returns></returns>
        public String GetPowerPointPath()
        {
            string powerPointPath = @"C:\Program Files\Microsoft Office\Office14\POWERPNT.EXE";
            string powerPointPath64 = @"C:\Program Files\Microsoft Office\Office16\POWERPNT.EXE";
            string powerPointPath86 = @"C:\Program Files (x86)\Microsoft Office\Office14\POWERPNT.EXE";

            if (File.Exists(powerPointPath))
            {
                return powerPointPath;
            }
            else if (File.Exists(powerPointPath64))
            {
                return powerPointPath64;
            }
            else if (File.Exists(powerPointPath86))
            {
                return powerPointPath86;
            }
            else
            {
                Console.WriteLine("Office is not installed or could not be found.");
                return "";
            }

        }

        /// <summary>
        /// Opens the current presentation file.
        /// </summary>
        public void OpenPresentation()
        {
            try
            {
                Console.Write("Opening PowerPoint presentation... ");
                string powerPointPath = GetPowerPointPath();
                string powerPointFilePath = @"C:\Presentations\Presentation.pptx";

                powerPoint = new Process();
                powerPoint.StartInfo.FileName = powerPointPath;
                powerPoint.StartInfo.Arguments = " /S " + powerPointFilePath;
                powerPoint.Start();
                Console.WriteLine("Done.");
            }
            catch (Exception)
            {

            }

        }

        /// <summary>
        /// Closes power point presentation if currently running.
        /// </summary>
        public void ClosePresentation()
        {
            try
            {
                Console.Write("Closing PowerPoint presentation... ");
                powerPoint.Kill();
                Console.WriteLine("Done.");
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("No presentation running.");
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("No presentation running.");
            }
        }
        #endregion
    }
}

