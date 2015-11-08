///////////////////////////////////////////////////////////////////////
// CommService.svc.cs - Handles communication with the clients       //
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
 * Contains send and recieve thread functions and a repository type class 
 * that maintains the information recieved from the client and the
 * information that is required to be sent to the client
 * 
 * Required Files:
 * ===============
 * BasicHttClient.cs, BlockingQueue.cs, CommService.svc.cs, ICommService.cs
 * Analyzer.cs, FileMge.cs, Display.cs
 * 
 * Maintenance History:
 * ====================
 * ver 3.0 : 24 Nov 14
 * -Created Sender and Reciever Threads for communicating with the client
 * ver 2.2 : 01 Nov 11
 * - Removed unintended local declaration of ServiceHost in Receiver's 
 *   CreateReceiveChannel function
 * ver 2.1 : 10 Oct 11
 * - removed [OperationContract] from GetMessage() so only local client
 *   can dequeue messages
 * - added send thread to keep clients from blocking on slow sends
 * - added retries when creating Communication channel proxy
 * - added comments to clarify what code is doing
 * ver 2.0 : 06 Nov 08
 * - added close functions that close the service and receive channel
 * ver 1.0 : 14 Jul 07
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using SWTools;
using System.Runtime.InteropServices;
using CodeAnalysis;
using System.IO;
using System.Xml.Linq;
namespace CommunicationPrototype
{
  // PerSession activation creates an instance of the service for each
  // client.  That instance lives for a pre-determined lease time.  
  // - If the creating client calls back within the lease time, then
  //   the lease is renewed and the object stays alive.  Otherwise it
  //   is invalidated for garbage collection.
  // - This behavior is a reasonable compromise between the resources
  //   spent to create new objects and the memory allocated to persistant
  //   objects.
  // 

  [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]

  public class CommService : ICommService
  {
    [DllImport("USER32.DLL", SetLastError = true) ]
    public static extern void SetWindowPos(
      IntPtr hwnd, IntPtr order, 
      int xpos, int ypos, int width, int height, 
      uint flags
    );
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();

    // We want the queue to be shared by all clients and the server,
    // so make it static.

    static BlockingQueue<Message> BlockingQ = null;
    private ServiceHost host_ = null;
    public ServiceHost host { get { return host_; } set { host_ = value; } }
    private bool connected = false;
    private List<string> projects = new List<string>();
    private string xmldata;
    public string xmldata_ { get { return xmldata; } set { xmldata = value; } }
    public List<string> projects_ { get { return projects; } set { projects = value; } }
    public bool connected_ { get { return connected; } set { connected = value; } }

    private List<string> analyzeproj = new List<string>();
    public List<string> analyzeproj_ { get { return analyzeproj; } set { analyzeproj = value; } }
      /// <summary>
      /// ////////////////////////////////////////////////////////////
      /// class to keep track of the details recieved from each client
      /// and the details that need to be sent to the client
    public class clientconnect
    {
        public string portno { get; set; }
        public bool connected { get; set; }

        private List<string> patt = new List<string>();
        public List<string> patt_ { get { return patt; } set { patt = value; } }

        private List<string> analproj = new List<string>();
        public List<string> analproj_ { get { return analproj; } set { analproj = value; } }
        private string data;
        public string data_ { get { return data; } set { data = value; } }
        private bool recurse = false;
        public bool recurse_ { get { return recurse; } set { recurse = value; } }
    }

    private List<clientconnect> connectedclients = new List<clientconnect>();
    public List<clientconnect> connectedclients_ { get { return connectedclients; } set { connectedclients = value; } }

    public CommService()
    {
      if(BlockingQ == null)
        BlockingQ = new BlockingQueue<Message>();
      SetWindowPos(GetConsoleWindow(), (IntPtr)0, 100, 100, 400, 600, 0);
      Console.Title = "CommService";
    }

    public void PostMessage(Message msg)
    {
     // IdentifyClient();
      BlockingQ.enQ(msg);
    }

    public void IdentifyClient()
    {
      OperationContext context = OperationContext.Current;
      MessageProperties messageProperties = context.IncomingMessageProperties;
      RemoteEndpointMessageProperty endpointProperty =
          messageProperties[RemoteEndpointMessageProperty.Name]
          as RemoteEndpointMessageProperty;

      Console.Write(
        "\n  IP address is {0} and port is {1}", 
        endpointProperty.Address, 
        endpointProperty.Port
      );
    }

    // Since this is not a service operation only server can call

    public Message GetMessage()
    {
      return BlockingQ.deQ();
    }
      /// <summary>
      /// /////////////////////////////////////////////////////////////
      /// Server Reciever thread
    public virtual void ThreadProc()    {
        Message msg = new Message();
        while (msg.text!="quit")        {
            msg = this.GetMessage();
            string data = msg.text;
            if (msg.text.Contains("GetProjects"))            {
                clientconnect cc = new clientconnect();
                cc.connected = true;
                data = data.Substring(data.IndexOf(":") + 1);
                cc.portno = data.Substring(0, data.IndexOf(":"));
                if (data.Length > data.IndexOf(":")+3)                {
                    string patt = data.Substring(data.IndexOf(":") + 1);
                    cc.patt_.Add(patt);
                }
                string data1="";
                foreach (string str in FieMgrMain("./../../TestProjects"))
                    data1 += str + " ";
                Thread.Sleep(500);
                connectedclients_.Add(cc);
                ServerSender(cc.portno,data1);
            }
            else            {
                foreach (clientconnect cc in connectedclients_)                {
                    if (cc.portno == data.Substring(0, data.IndexOf(":")))                    {
                        data=data.Substring(data.IndexOf(":")+1);
                        if (data.IndexOf(":") != -1)
                        {
                            cc.recurse_ = true;
                            data = data.Substring(0, data.IndexOf(":"));
                        }
                        cc.analproj_.Clear();
                        while (data.IndexOf(" ") != -1)
                        {
                            cc.analproj_.Add(data.Substring(0, data.IndexOf(" ")));
                            data=data.Substring(data.IndexOf(" ")+1);
                        }
                        analyzercall(cc);
                        ServerSender(cc.portno,cc.data_);
                        connectedclients_.Remove(cc);
                        break;
                    }
                }
            }
            //if (msg.text == "quit")
            //    msg.text = "";
        }
        msg.text = xmldata_;       
    }
      /// <summary>
      /// //////////////////////////////////////////////
      ///getting the list of projects from the directory
    public List<string> FieMgrMain(string args)
    {
        FileMgr fm = new FileMgr();
        List<string> dirs = new List<string>();
        foreach (string str in fm.getDirectories(args))
            dirs.Add("8089:"+str.Substring(str.LastIndexOf("\\")+1));
        return dirs;
    }
      /// <summary>
      /// //////////////////////////////////////////////////////////////
      /// analyzer call or the executive for analyzer project
      public void analyzercall(clientconnect cc)
      {
            if (analyzeproj_.Count == 0)
            {
            //    Console.Write("Please enter Command Line Arguments.\n\n");
                //return null;
            }
            Console.Write("\n\nCurrent path:\n {0}", Directory.GetCurrentDirectory());
            CommandLine.CommandLineProcessing clp = new CommandLine.CommandLineProcessing();
            string path = Directory.GetCurrentDirectory();
            path += "/Servers/TestProjects";           
            Console.Write("{0}",path);
            if(CodeAnalysis.Repository.getInstance()!=null)
                CodeAnalysis.Repository.clearInstance();  
            CodeAnalysis.FileMgr fm = new CodeAnalysis.FileMgr();
            string str2 = "";
            foreach (string str1 in cc.patt_)
                str2 += str1;
            string[] commandlinestr = {path,"/X",str2};
            clp.ProcessCommandLine(commandlinestr);
            fm.setrecurseflag(cc.recurse_);
            fm.findFiles(path);
            List<string> files = new List<string>();
            foreach (string str in fm.getFiles())
            {
                foreach (string str1 in cc.analproj_)
                    if (str.Contains(str1))
                        files.Add(str);
            }            
            CodeAnalysis.Analyzer.doAnalysis(files.ToArray(), true);
            CodeAnalysis.Display dis = new CodeAnalysis.Display();
            dis.Displayfiles(files);     
            try
            {
                CodeAnalysis.XML xml = new CodeAnalysis.XML();
                dis.DisplayData(clp);
                if (clp.getxmlflag())
                    xml.XMLWrite(clp.getrelationshipflag());
                var xmldoc = XDocument.Load("Output.xml");
                cc.data_ = xmldoc.ToString();
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}\n", ex.Message);
            }
      }
      /// <summary>
      /// /////////////////////////////////////////////////
      /// Snder Thread function
    public void ServerSender(string port, string data)
    {
        Console.Write("\n  BasicHttpClient Starting to Post Messages to Service");
        Console.Write("\n ======================================================\n");
        Client client = new Client();
        try
        {
            string url = "http://localhost:"+port+"/ICommService/BasicHttp";
            Console.Write("\n  connecting to \"{0}\"\n", url);
            client.CreateBasicHttpChannel(url);
            Message msg = new Message();
            msg.text = data;
            client.channel.PostMessage(msg); 
        }
        catch (Exception ex)
        {
            Console.Write("\n\n  {0}", ex.Message);
        }
        Console.Write("\n\n");
    }
#if(TEST_COMMSERVICE_SVC)
    public static void Main( )
    {
      Console.Write("\n  Communication Server Starting up");
      Console.Write("\n ==================================\n");
        
      try
      {
        Client cl=new Client();
        CommService service = new CommService();
        BasicHttpBinding binding0 = new BasicHttpBinding();
        Uri address0 = new Uri("http://localhost:8089/ICommService/BasicHttp");

        using (service.host = new ServiceHost(typeof(CommService), address0))
        {
          service.host.AddServiceEndpoint(typeof(ICommService), binding0, address0);
          service.host.Open();

          Console.Write("\n  CommService is ready.");
          Console.Write("\n    Maximum BasicHttp message size = {0}", binding0.MaxReceivedMessageSize);
          Console.WriteLine();

            
          Thread child = new Thread(new ThreadStart(service.ThreadProc));
          //Thread ch1 = new Thread(new ThreadStart(service.ServerSender));
          child.Start();
          //ch1.Start();
          child.Join();
          //ch1.Join();
            
          Console.Write("\n\n  Press <ENTER> to terminate service.\n\n");
          Console.ReadLine();
        }
         
      }
      catch (Exception ex)
      {
        Console.Write("\n  {0}\n\n", ex.Message);
      }
        
    }
#endif


  }
}
