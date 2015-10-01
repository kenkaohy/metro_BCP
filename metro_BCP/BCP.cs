
using System;
using System.IO;
using System.Net;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;
using System.Net.NetworkInformation;
using System.Xml;
using System.Xml.Serialization;
using BCP;
using System.Linq;

namespace metro_BCP
{
    public partial class BCP : MetroForm
    {

        public string sURL = "http://www.rainymood.com/";
        public string sServerURL = "http://www.rainymood.com/";
        public string sBrowser = "IExplore.exe";
        public string sDataPath = Application.StartupPath + @"\\data\hosts.xml";
        public List<Host> hostlist = new List<Host>();
        public Host primary = new Host();

        public BCP()
        {
            InitializeComponent();
            //Load the host list from host.xml.
            loadXML(sDataPath);
        }

        private void BCP_Load(object sender, EventArgs e)
        {
            //Display welcome message.
            MetroFramework.MetroMessageBox.Show(this, "Welcome to BCP Express.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            

            foreach (Host value in hostlist)
            {
                Console.WriteLine(value.name);
                Console.WriteLine(value.location);
                Console.WriteLine(value.address);
                Console.WriteLine(value.ip);
            }

            //Bind the datagrid.
            metroGrid1.DataSource = hostlist;

            //addXML("primary", "CA, US", "https://www.iplocation.net/", "198.72.229.177");

            //Retrieve the primary host.
            queryPrimaryHost(hostlist);
            Console.WriteLine("Server URL: " + primary.address);

            //Load & Redirect
            //connectBCP(sBrowser, sURL);

            //Start background operation
            //this.backgroundWorker1.RunWorkerAsync();


        }

        private void metroButton1_Click(object sender, EventArgs e)
        {

            connectBCP(sBrowser, sURL);
        }

        public static void connectBCP(string thisBrowser, string thisURL)
        {
            try
            {

                System.Diagnostics.Process.Start(thisBrowser, thisURL);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void metroButton2_Click(object sender, EventArgs e)
        {

            // change button states
            this.metroButton1.Enabled = false;
            this.metroButton2.Enabled = false;

            //Start background operation
            this.metroProgressSpinner1.Visible = true;
            this.labelProgress.Visible = true;
            this.SetText("Back ground Worker initializing...");
            this.backgroundWorker1.RunWorkerAsync();

        }

        private void OnDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

            this.SetText("Requesting host list...");
            var task = MakeAsyncRequest(sServerURL, "text/html");
            Console.WriteLine("Got response from host:   " + sServerURL, task.Result);

            //for (int i = 0; i < 10; i++)
            //{
            //    if (this.backgroundWorker1.CancellationPending)
            //    {
            //        e.Cancel = true;
            //        break;
            //    }
            //    // report progress
            //    this.backgroundWorker1.ReportProgress(-1, string.Format("Performing step {0}...", i + 1));

            //    //// simulate operation step
            //    //System.Threading.Thread.Sleep(rand.Next(100, 1000));
            //    //if (this.simulateError)
            //    //{
            //    //    this.simulateError = false;
            //    //    throw new Exception("Unexpected error!");
            //    //}
            //}
        }
        private void OnProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            if (e.UserState is String)
            {
                this.labelProgress.Text = (String)e.UserState;
            }
        }

        private void OnRunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            // show result indication
            if (e.Cancelled)
            {
                this.SetText("Operation cancelled by the user!");
                //this.pictureBox.Image = Properties.Resources.WarningImage;
            }
            else
            {
                if (e.Error != null)
                {
                    this.SetText("Operation failed: " + e.Error.Message);
                    //this.pictureBox.Image = Properties.Resources.ErrorImage;
                }
                else
                {
                    this.SetText("Operation finished successfuly!");
                    //this.pictureBox.Image = Properties.Resources.InformationImage;
                }
            }

            // restore button states
            this.metroButton1.Enabled = true;
            //restore spinner status
            this.metroProgressSpinner1.Visible = false;
        }


        //Send http request.
        public Task<string> MakeAsyncRequest(string url, string contentType)
        {
            this.SetText("Sending http request...");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = contentType;
            request.Method = WebRequestMethods.Http.Get;
            request.Timeout = 20000;
            request.Proxy = null;

            Task<WebResponse> task = Task.Factory.FromAsync(
                request.BeginGetResponse,
                asyncResult => request.EndGetResponse(asyncResult),
                (object)null);

            return task.ContinueWith(t => ReadStreamFromResponse(t.Result));
        }

        //Read content from http response.
        private string ReadStreamFromResponse(WebResponse response)
        {
            this.SetText("Retriving http context...");
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(responseStream))
            {
                //Need to return this response 
                string strContent = sr.ReadToEnd();
                return strContent;
            }
        }


        //Parsing obj into XML file.
        public static void saveXML(object obj, string filepath)
        {
            XmlSerializer sr = new XmlSerializer(obj.GetType());
            TextWriter writer = new StreamWriter(filepath);
            sr.Serialize(writer, obj);
            writer.Close();
        }

        //Loading xml file infor. 
        public void loadXML(string filepath)
        {
            if (File.Exists(filepath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(List<Host>));
                FileStream read = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
                hostlist = (List<Host>)xs.Deserialize(read);
                read.Close();
            }
        }

        public void addXML(string name, string location, string address, string ip)
        {
            Host thishost = new Host()
            {
                name = name,
                location = location,
                address = address,
                ip = ip
            };
            hostlist.Add(thishost);
            saveXML(hostlist, sDataPath);
            loadXML(sDataPath);
        }

        public void queryPrimaryHost(List<Host> thishostlist)
        {
            primary = thishostlist.FirstOrDefault(o => o.name == "primary");
        }



        // This delegate enables asynchronous calls for setting
        // the text property on a TextBox control.
        delegate void SetTextCallback(string text);

        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.labelProgress.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.labelProgress.Text = text;
            }
        }
    }
}



//string result = "";
//result = processPing("stackoverflow.com", 4);
//Console.WriteLine(result);s
//getHttpContext(sURL);


//public static string processPing(string thisHost, int thisEchoNum)
//{
//    string res = thisHost + " is down.";
//    int timeout = 120;
//    Ping pingSender = new Ping();

//    for (int i = 0; i < thisEchoNum; i++)
//    {
//        PingReply reply = pingSender.Send(thisHost, timeout);
//        if (reply.Status == IPStatus.Success)
//        {
//            res = reply.Status.ToString();
//        }
//    }
//    return res;
//}

//if (File.Exists(sDataPath))
//{
//    Console.WriteLine("host.xml exist.");
//    try
//    {
//        saveXML(saveing, sDataPath);
//    }
//    catch(Exception ex)
//    {
//        MessageBox.Show(ex.Message);
//    }

//}


//Host a = new Host()
//{
//    name = "rainymood",
//    location = "Chantilly, United States",
//    address = "http://www.rainymood.com/",
//    ip = "173.193.205.68"
//};


//Host b = new Host()
//{
//    name = "coffitivity",
//    location = "New York, United States",
//    address = "https://coffitivity.com/",
//    ip = "45.55.205.62"
//};


//Host c = new Host()
//{
//    name = "gettyimages",
//    location = "Germany",
//    address = "www.gettyimages.com/",
//    ip = "77.67.27.17"
//};


//Host d = new Host()
//{
//    name = "bridgepointhealth",
//    location = "Waterloo, Ontario, Canada",
//    address = "www.bridgepointhealth.ca/",
//    ip = "216.16.234.30"
//};


//Host f = new Host()
//{
//    name = "buzzfeed",
//    location = "Amsterdam, North Holland, Netherlands",
//    address = "www.buzzfeed.com",
//    ip = "23.74.196.25"
//};


//List<Host> saveing = new List<Host>();
//saveing.Add(a);
//            saveing.Add(b);
//            saveing.Add(c);
//            saveing.Add(d);
//            saveing.Add(f);
