///////////////////////////////////////////////////////////////////////
// MainWindow.cs - WPF User Interface for WCF Communicator           //
// Language:    C#, 2014, .Net Framework 4.0                         //
// Platform:    Dell Inspiron 3521                                   //
// Author:      Nirav Gandhi, MS in CE, Syracuse University          //
//              (315) 395-4842, nigandhi@syr.edu                     //   
// Source:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operation:
 * ==================
 * Support various events in order to process data on user interaction
 * with the GUI. It communicates with the server to and forth by sending 
 * Http messages to the server and recieving the same. 
 * Here, the sender and reciever functions run on seperate threads in order
 * to make the client processing concurrent and efficient.
 * 
 * Required Files:
 * ===============
 * BasicHttClient.cs, BlockingQueue.cs, CommService.svc.cs, ICommService.cs
 * ListofServers.xml
 * 
 * Maintenance History:
 * ====================
 * ver 2.3 : 26 Nov 14
 * -added send recieve thread in order to support two way 
 * communication with server with minimal computation.
 * ver 2.2 : 30 Oct 11
 * - added send thread to keep UI from freezing on slow sends
 * - added more comments to clarify what code is doing
 * ver 2.1 : 16 Oct 11
 * - cosmetic changes, posted to the college server but not
 *   distributed in class
 * ver 2.0 : 06 Nov 08
 * - fixed bug that had local and remote ports swapped
 * - made Receive thread background so it would not keep 
 *   application alive after close button is clicked
 * - now closing sender and receiver channels on window
 *   unload
 * ver 1.0 : 17 Jul 07
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xaml;
using System.Xml.Linq;
using System.ServiceModel;
using CommunicationPrototype;
using System.Threading;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<string> listofServers = new List<string>();
        public List<string> listofServer_ { get { return listofServers; } }
        private string portno;
        public string portno_ { get { return portno; } set { portno = value; } }
        private List<string> typetables = new List<string>();
        public List<string> typetables_ { get { return typetables; } set { typetables = value; } }
        private List<string> connectedservers = new List<string>();
        public List<string> connectedservers_ { get { return connectedservers; } set { connectedservers = value; } }
        private List<string> connectedservers1 = new List<string>();
        public List<string> connectedservers1_ { get { return connectedservers1; } set { connectedservers1 = value; } }

        private List<string> xmlfiles = new List<string>();
        public List<string> xmlfiles_ { get { return xmlfiles; } set { xmlfiles = value; } }
        private string patt;
        public string patt_ { get { return patt; } set { patt = value; } }
        private bool noservers = false;
        public bool noservers_ { get { return noservers; } set { noservers = value; } }
        private bool recurse = true;
        public bool recurse_ { get { return recurse; } set { recurse = value; } }
        public MainWindow()
        {
            InitializeComponent();
            Listofservers.SelectionMode = SelectionMode.Extended;
            ListofProjects.SelectionMode = SelectionMode.Extended;
            AnalyzeButton.IsEnabled = false;
            ProjectsButton.IsEnabled = false;
            ListofProjects.Items.Clear();
            Listofservers.Items.Clear();
        }
        /// <summary>
        /// //////////////////////////////////////////////////////
        /// Reads the names and port numbers of the servers that 
        /// can serve the client
        private void ReadServersfromXML()
        {
            XDocument xdoc = XDocument.Load("./../../ListofServers.xml");
            var listname = from lists in xdoc.Element("ListofServers").Descendants()
                           where lists.Name.ToString() == "Server"
                           select lists.Attribute("name").Value.ToString() + " " + lists.Value.ToString();
            foreach (string str in listname.ToList())
                listofServer_.Add(str);
        }
        /// <summary>
        /// /////////////////////////////////////////////////////
        /// Click event for getting projects list from xml file
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListofProjects.Items.Clear();           
            OutputDisplay.Clear();
            Listofservers.Items.Clear();
            listofServer_.Clear();
            ReadServersfromXML();
            
            foreach (string str in listofServer_)
                Listofservers.Items.Add(str.Substring(0, str.LastIndexOf(" ")));
            ProjectsButton.IsEnabled = true;

        }
        /// <summary>
        /// ///////////////////////////////////////////////////////////
        /// click event function for getting list of projects from the 
        /// selected servers. 
        private void ProjectsButton_Click(object sender, RoutedEventArgs e)
        {

            if (Listofservers.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a server whose projects you want to see");
                return;
            }
            noservers_ = false;
            portno_ = portnumb.Text;
            patt_ = patterns.Text;
            recurse_ = Recurse.IsChecked.Value;
            List<string> arr = new List<string>();
            List<string> a = new List<string>();
            ListofProjects.Items.Clear();
            connectedservers_.Clear();
            foreach (string server in Listofservers.SelectedItems)
            {
                foreach (string str in listofServer_)
                    if (str.Contains(server))
                        arr.Add(str.Substring(str.LastIndexOf(" ") + 1));
            }
            Thread th1 = new Thread(new ParameterizedThreadStart(o => ClientSenderMain((List<string>)o)));
            Thread th2 = new Thread(new ThreadStart(() => { ClientRecieverMain(ref a); }));
            th1.IsBackground = true;
            th2.IsBackground = true;
            th1.Start(arr);
            th2.Start();
            th1.Join();
            th2.Join();
            foreach (string str in a)
            {
                ListofProjects.Items.Add(str);
            }
            AnalyzeButton.IsEnabled = true;
            ServerButton.IsEnabled = false;         
        }
        /// <summary>
        /// ////////////////////////////////////////////////////////////////
        /// Reciever thread for getting list of projects
        /// <param name="a"></param>
        public void ClientRecieverMain(ref List<string> a)
        {
            Console.Write("\n  Communication Server Starting up");
            Console.Write("\n ==================================\n");
            List<string> projects = new List<string>();
            Thread.Sleep(2000);
            CommunicationPrototype.Client cl = new CommunicationPrototype.Client();
                try
                {
                    CommService service = new CommService();
                    BasicHttpBinding binding0 = new BasicHttpBinding();
                    Uri address0 = new Uri("http://localhost:" + portno_ + "/ICommService/BasicHttp");
                    using (service.host_ = new ServiceHost(typeof(CommService), address0))
                    {
                        service.host_.AddServiceEndpoint(typeof(ICommService), binding0, address0);
                        service.host_.Open();
                        Message msg = new Message();
                        msg.text = " ";                      
                        while (connectedservers_.Count == 0) 
                        {
                            if (noservers_)
                                return;
                        }
                        Thread.Sleep(2000);
                        int count = connectedservers_.Count;
                        while(count!=0)
                        {
                            count--;
                            msg = service.GetMessage();
                            string data = msg.text;
                            while (data != "")
                            {
                                a.Add(data.Substring(data.IndexOf(":") + 1, data.IndexOf(" ") - data.IndexOf(":") - 1));
                                data = data.Substring(data.IndexOf(" ") + 1);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Write("\n  {0}\n\n", ex.Message);
                }
        }
        /// <summary>
        /// ///////////////////////////////////////////////////////////////////////
        /// Sender Thread for sending the request for getting list of projects.
        
        public void ClientSenderMain(object arr)
        {
            List<string> arr1 = (List<string>)arr;
            CommunicationPrototype.Client client = new CommunicationPrototype.Client();
            foreach (string str in arr1)
            {
                try
                {
                    string url = "http://localhost:" + str + "/ICommService/BasicHttp";
                    Console.Write("\n  connecting to \"{0}\"\n", url);
                    client.CreateBasicHttpChannel(url);
                    CommunicationPrototype.Message msg = new CommunicationPrototype.Message();
                    msg.text = "GetProjects:"+portno_+":" + patt_;
                    client.channel_.PostMessage(msg);
                    ((ICommunicationObject)client.channel_).Close();
                    connectedservers_.Add(str);
                }
                catch (Exception ex)
                {
                    Console.Write("{0}", ex.Message);
                }
                Console.Write("\n\n");
            }
            if (connectedservers_.Count == 0)
                noservers_ = true;
        }
        /// <summary>
        /// ////////////////////////////////////////////////////////////////
        /// Function to read the XML recieved from the server in 
        /// order to display it in the text box.
        void DisplayOutput()        {
            string str4 = "";
            string dis="";
            if (Relation.IsChecked == true || Package.IsChecked == true)            {
                if (Package.IsChecked==true)
                    str4 = "Displaying Package Dependency\n=======================================\n\n";
                else
                    str4 = "Displaying Type Dependency\n=======================================\n\n";
                dis = "RelationTable";
            }
            else            {
                dis = "TypeTable";
                str4 = "Displaying Types\n==============================\n\n";
            } 
            foreach (string str in xmlfiles_)            {
                XDocument xdoc = XDocument.Load(str);
                var xlist = from xnod in xdoc.Element("Output").Element(dis).DescendantNodes()
                                    where xnod.Parent.Name == dis
                                    select xnod;
                List<XNode> xstr1 = xlist.ToList();
                string str2 = "";
                foreach (XElement str1 in xstr1)                {                    
                    var xlist1 = from xnod in str1.Descendants()
                                where xnod.Parent.Name == str1.Name
                                select xnod;
                    int count = 0;
                    List<XElement> xstr = xlist1.ToList();
                    if (Package.IsChecked == true && xstr.ElementAt(0).Value == xstr.ElementAt(xstr.Count / 2).Value)
                        continue;
                    else
                    {
                        str2 += str1.Name + "\n";
                        foreach (XElement str3 in xlist1.ToList())
                        {
                            count++;
                            str2 += str3.Value + "   ";
                            if (count == xlist1.ToList().Count / 2)
                            {
                                count = 0;
                                str2 += "\n";
                            }
                        }
                        str2 += "\n";
                    }
                }
                str4 += str2+"\n";
            }
            OutputDisplay.Text = str4;
        }
        /// <summary>
        /// ////////////////////////////////////////////////////////
        /// Creates XML file from the xml string recieved
        void CreateXML()
        {
            XmlDocument xdoc = new XmlDocument();
            int count = 1;
            string file="";
            xmlfiles_.Clear();
            foreach (string str in typetables_)
            {
                if (str != null)
                {
                    file = count.ToString() + ".Xml";
                    xdoc.LoadXml(str);
                    xdoc.Save(file);
                    count++;
                    xmlfiles_.Add(file);
                }
            }
        }
        /// <summary>
        /// ////////////////////////////////////////////////////////////
        /// Click event for sending analysis request to the server
        /// Works on sender and reciever threads
        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ListofProjects.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select atleast 1 project that you need to analayze.");
                return;
            }
            noservers_ = false;
            ProjectsButton.IsEnabled = false;
            List<string> arr = new List<string>();
            List<string> a = new List<string>();
            //a = ListofProjects.SelectedItems;
            connectedservers1_.Clear();
            typetables_.Clear();
            foreach (string proj in ListofProjects.SelectedItems)
                arr.Add(proj);
            Thread th1 = new Thread(new ParameterizedThreadStart(o => ClientSenderMain1((List<string>)o)));
            Thread th2 = new Thread(new ThreadStart(() => { ClientRecieverMain1(ref a); }));
            th1.IsBackground = true;
            th2.IsBackground = true;
            th1.Start(arr);
            th2.Start();
            th1.Join();
            th2.Join();
            if (typetables_.Count != 0)
                CreateXML();
            DisplayOutput();
            ProjectsButton.IsEnabled = true;
            Listofservers.IsEnabled = true;
            AnalyzeButton.IsEnabled = false;
        }
        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////
        /// Reciever Thread function for getting the analysis function from the server

        public void ClientRecieverMain1(ref List<string> a)
        {
            Console.Write("\n  Communication Server Starting up");
            Console.Write("\n ==================================\n");
            List<string> projects = new List<string>();
            CommunicationPrototype.Client cl = new CommunicationPrototype.Client();
                try
                {
                    CommService service = new CommService();
                    BasicHttpBinding binding0 = new BasicHttpBinding();
                    Uri address0 = new Uri("http://localhost:" + portno_ + "/ICommService/BasicHttp");
                    using (service.host_ = new ServiceHost(typeof(CommService), address0))
                    {
                        service.host_.AddServiceEndpoint(typeof(ICommService), binding0, address0);
                        service.host_.Open();
                        Message msg = new Message();
                        msg.text = " ";
                        while (connectedservers1_.Count == 0)
                        {
                            if (noservers_)
                                return;
                        }
                        Thread.Sleep(1000);
                        int count = connectedservers1_.Count;
                        while (count != 0)
                        {
                            count--;
                            msg = service.GetMessage();
                            typetables_.Add(msg.text);
                            a.Add(msg.text);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Write("\n  {0}\n\n", ex.Message);
                }
        }
        /// <summary>
        /// /////////////////////////////////////////////////////////////
        /// Sender thread function for sending analysis request to the server
        
        public void ClientSenderMain1(object arr)
        {
            List<string> arr1 = (List<string>)arr;
            CommunicationPrototype.Client client = new CommunicationPrototype.Client();
            foreach (string serv in connectedservers_)
            {
                try
                {
                    string url = "http://localhost:" + serv.Substring(serv.IndexOf(" ")+1) + "/ICommService/BasicHttp";
                    Console.Write("\n  connecting to \"{0}\"\n", url);
                    client.CreateBasicHttpChannel(url);
                    CommunicationPrototype.Message msg = new CommunicationPrototype.Message();
                    string str1="";
                    foreach (string str in arr1)
                        str1 += str+" ";  
                    msg.text = portno_+":"+str1;
                    if (recurse_ == true)
                        msg.text += ":/S";
                    client.channel_.PostMessage(msg);
                    connectedservers1_.Add(serv);
                }
                catch (Exception ex)
                {
                    Console.Write("{0}",ex.Message);
                }
            }
            if (connectedservers_.Count == 0)
                noservers_ = true;
        }
    }
}
