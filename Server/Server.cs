using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Server
    {
        int port = 8000;
        string ip;
        ConsoleSpinner spinner = new ConsoleSpinner();
        TcpListener listener = new TcpListener(IPAddress.Any, 100);//bypass Compiler
        Process powerPoint;
        string archiveFilepath = @"C:\Presentations\Archive\";
        string filePath = @"C:\Presentations\";
        string fileName = "Presentation";
        string fileExt = ".pptx";

        public Server()
        {
            CreateDependencies();
            ip = GetLocalIPAddress();
            StartServer();
        }

        public void CreateDependencies()
        {
            Console.Out.Write("Checking dependencies... ");
            Directory.CreateDirectory(@"C:\Presentations\Archive");
            Console.Out.WriteLine("Done.");
        }

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

        public void ReceiveFile()
        {
            using (var client = listener.AcceptTcpClient())
            using (var stream = client.GetStream())
            using (var output = File.Create(filePath + fileName + fileExt))
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

        public void ArchivePresentation()
        {
            try
            {
                Console.Out.Write("Archiving old presentation... ");
                if (File.Exists(filePath + fileName + fileExt))
                {
                    File.Move(filePath + fileName + fileExt, archiveFilepath + fileName + DateTime.Now.ToString("ddMyy-HHmmss") + fileExt);
                }
                Console.Out.WriteLine("Done.");
            }
            catch (Exception)
            {

            }
        }

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

        // Return a string of the correct power point executable path
        public String GetPowerPointPath()
        {
            // C:\Program Files(x86)\Microsoft Office\Office14
            string powerPointPath = @"C:\Program Files\Microsoft Office\Office14\POWERPNT";
            string powerPointPath64 = @"C:\Program Files\Microsoft Office\Office16\POWERPNT";
            string powerPointPath86 = @"C:\Program Files (x86)\Microsoft Office\Office14\POWERPNT";
            string extension = ".EXE";


            if (File.Exists(powerPointPath + extension))
            {
                //Console.WriteLine("Office 14 Exists");
                return powerPointPath + extension;
            }
            else if (File.Exists(powerPointPath64 + extension))
            {
                //Console.WriteLine("Office 16 Exists");
                return powerPointPath64 + extension;
            }
            else if (File.Exists(powerPointPath86 + extension))
            {
                //Console.WriteLine("Office x86 Exists");
                return powerPointPath86 + extension;
            }
            else
            {
                Console.WriteLine("Office is not installed or could not be found.");
                return "";
            }

        }

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
    }
}

