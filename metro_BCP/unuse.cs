//string result = "";
//result = processPing("stackoverflow.com", 4);
//Console.WriteLine(result);s
//getHttpContext(sURL);



//addXML("primary", "CA, US", "https://www.iplocation.net/", "198.72.229.177");

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
