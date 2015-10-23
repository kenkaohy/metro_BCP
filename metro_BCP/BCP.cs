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
using System.Timers;

namespace metro_BCP
{
    public partial class BCP : MetroForm
    {
        /*
            TSQL to retrive xml schema from database:
            SELECT* from[BCP].[dbo].[BCP_Node] for XML PATH('Host'), root('Root');
        */
        public string sURL = ":8080/BCWebClient/";
        public string sServerURL = "http://www.rainymood.com/";
        public string sBrowser = "IExplore.exe";
        public string sDataPath = Application.StartupPath + @"\\data\hosts.xml";
        public List<Host> hostlist = new List<Host>();//list of host object load form 'hosts.xml'
        public Host primary = new Host();

        public BCP()
        {
            InitializeComponent();
        }

        private void BCP_Load(object sender, EventArgs e)
        {

            //Load the host list from host.xml.
            loadXML(sDataPath);

            //Bind the DataGrid.
            metroGrid1.DataSource = hostlist;

            //addXML("primary", "CA, US", "https://www.iplocation.net/", "198.72.229.177");

            //Retrieve the primary host.
            queryPrimaryHost(hostlist);

            //Load & Redirect
            //connectBCP(sBrowser, sURL);

            //Start background operation
            //this.backgroundWorker1.RunWorkerAsync();

        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            connectBCP(sBrowser, sURL, hostlist);
        }

        public static void connectBCP(string thisBrowser, string thisURL, List<Host> thisHosts)
        {
            try
            {
                Console.WriteLine(thisHosts.Find(h => h.status == "Active").ip + thisURL);
                System.Diagnostics.Process.Start(thisBrowser, thisHosts.Find(h => h.status == "Active").ip + thisURL);
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

            this.metroProgressSpinner1.Visible = true;
            this.labelProgress.Visible = true;


            PingAndUpdateHost(hostlist);

            //Initial background work
            //this.backgroundWorker1.RunWorkerAsync();
        }

        private void OnDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

            //var task = MakeAsyncRequest(sServerURL, "text/html");
            //Console.WriteLine("Got response from host:   " + sServerURL, task.Result);

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
                    System.Threading.Thread.Sleep(1000);
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
            System.Threading.Thread.Sleep(1000);
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
            System.Threading.Thread.Sleep(1000);
            this.SetText("Retriving http context...");
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader sr = new StreamReader(responseStream))
            {
                //Need to return this response 
                string strContent = sr.ReadToEnd();
                return strContent;
            }
        }


        public async void PingAndUpdateHost(List<Host> hosts)
        {
            Ping ping = new Ping();

            foreach (Host thisHost in hosts)
            {
                var reply = await ping.SendPingAsync(thisHost.ip, 1000);
                if (reply.Status == IPStatus.Success)
                {
                    updateHostStatus(thisHost.id,"Active");
                    this.SetText("Checking host health...\n" + thisHost.host_name + " is Active.");
                    //Console.WriteLine(thisHost.host_name + " is Active.");
                }
                else
                {
                    updateHostStatus(thisHost.id, "Inactive");
                    this.SetText("Checking host health...\n" + thisHost.host_name + " is Inactive.");
                    //Console.WriteLine(thisHost.host_name + " is Inactive.");
                }
            }

            //save updated host list to xml file
            saveXML(hostlist, sDataPath);

            this.metroProgressSpinner1.Visible = false;
            this.SetText("Checking complete.");
            System.Threading.Thread.Sleep(2000);
            this.metroButton1.Enabled = true;
        }

        //Parsing obj into XML file.
        public static void saveXML(object obj, string filepath)
        {
            XmlSerializer sr = new XmlSerializer(obj.GetType());
            TextWriter writer = new StreamWriter(filepath);
            sr.Serialize(writer, obj);
            writer.Close();
        }

        //Loading xml file info. 
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

        //add new host into xml file.
        public void addXML(string host_name, string location, string id, string ip, string status)
        {
            Host thishost = new Host()
            {
                id = id,
                host_name = host_name,
                location = location,
                ip = ip,
                status = status
            };
            hostlist.Add(thishost);
            saveXML(hostlist, sDataPath);
            loadXML(sDataPath);
        }

        //Get server host object.
        public void queryPrimaryHost(List<Host> thishostlist)
        {
            primary = thishostlist.FirstOrDefault(o => o.host_name == "primary");
        }

        //Update host status in hostlist
        public void updateHostStatus( string thisId, string thisStatus)
        {
            try
            {
                hostlist.Where(h => h.id == thisId).First().status = thisStatus;
            }
            catch (Exception e)
            {
                throw e;
            }
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

        private void welcomeMsg(object sender, EventArgs e)
        {
            //Display welcome message.
            MetroFramework.MetroMessageBox.Show(this, "Welcome to BCP Express.\n", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
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


//Local test start
//Host a = new Host()
//{
//    name = "primary",
//    location = "Chantilly, United States",
//    address = "http://www.rainymood.com/",
//    ip = "173.193.205.68",
//    online = true
//};
//Host b = new Host()
//{
//    name = "coffitivity",
//    location = "New York, United States",
//    address = "https://coffitivity.com/",
//    ip = "45.55.205.62",
//    online = true
//};
//Host c = new Host()
//{
//    name = "gettyimages",
//    location = "Germany",
//    address = "www.gettyimages.com/",
//    ip = "77.67.27.17",
//    online = true
//};
//Host d = new Host()
//{
//    name = "bridgepointhealth",
//    location = "Waterloo, Ontario, Canada",
//    address = "www.bridgepointhealth.ca/",
//    ip = "216.16.234.30",
//    online = true
//};
//Host f = new Host()
//{
//    name = "buzzfeed",
//    location = "Amsterdam, North Holland, Netherlands",
//    address = "www.buzzfeed.com",
//    ip = "23.74.196.25",
//    online = true
//};
//Host g = new Host()
//{
//    name = "offline site",
//    location = "moon",
//    address = "www.midosf.com",
//    ip = "192.1.1.1",
//    online = true
//};
//List<Host> saveing = new List<Host>();
//saveing.Add(a);
//            saveing.Add(b);
//            saveing.Add(c);
//            saveing.Add(d);
//            saveing.Add(f);
//            saveing.Add(g);
//            saveXML(saveing, sDataPath);
//Local test end


//    foreach (Host value in hostlist)
//{
//    Console.WriteLine(value.name);
//    Console.WriteLine(value.location);
//    Console.WriteLine(value.address);
//    Console.WriteLine(value.ip);
//    Console.WriteLine(value.online);
//    Console.WriteLine("______________");
//}

//public static void processPing(string thisAddress)
//{
//    // Ping's the local machine.
//    Ping pingSender = new Ping();
//    //IPAddress address = IPAddress.Loopback;
//    Uri uri = new Uri(thisAddress);
//    PingReply reply = pingSender.Send(uri.Host);

//    if (reply.Status == IPStatus.Success)
//    {
//        Console.WriteLine("Address: {0}", reply.Address.ToString());
//        Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
//        Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
//        Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
//        Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
//    }
//    else
//    {
//        Console.WriteLine(reply.Status);
//    }
//}