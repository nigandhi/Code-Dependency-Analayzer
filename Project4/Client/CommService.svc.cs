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
 * Maintenance History:
 * ====================
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
using Client;
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
    private ServiceHost host = null;
    public ServiceHost host_ { get { return host; } set { host = value; } }
    public CommService()
    {
      // Only one service, the first, should create the queue

      if(BlockingQ == null)
        BlockingQ = new BlockingQueue<Message>();

      SetWindowPos(GetConsoleWindow(), (IntPtr)0, 100, 100, 400, 600, 0);
      //Console.Title = "CommService";
    }

    public void PostMessage(Message msg)
    {
      IdentifyClient();
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

    // Method for server's child thread to run to process messages.
    // It's virtual so you can derive from this service and define
    // some other server functionality.

    protected virtual List<string> ThreadProc()
    {
        List<string> asd=new List<string>();
        //Client cl = new Client();
        
//        Console.Write(Ide);
       while (true)
       {
         Message msg = this.GetMessage();
         asd.Add(msg.ToString());
         //IdentifyClient();
         //  string cmdStr = "";
         //switch (msg.command)
         //{
         //  case Message.Command.DoThis:
         //    cmdStr = "DoThis";
         //    break;
         //  case Message.Command.DoThat:
         //    cmdStr = "DoThat";
         //    break;
         //  case Message.Command.DoAnother:
         //    cmdStr = "DoAnother";
         //    break;
         //  default:
         //    cmdStr = "unknown command";
         //    break;
         //}
         //Console.Write("\n  received: {0}\t{1}",cmdStr,msg.text);
         if (msg.text == "Project1")
           break;
       } 
       
        return asd;
    }

    //public List<string> ClientRecieverMain()
    //{
    //  Console.Write("\n  Communication Server Starting up");
    //  Console.Write("\n ==================================\n");
    //  List<string> projects = new List<string>();
    //  try
    //  {
    //    CommService service = new CommService();
        
    //    // - We're using WSHttpBinding and NetTcpBinding so digital certificates
    //    //   are required.
    //    // - Both these bindings support ordered delivery of messages by default.

    //    BasicHttpBinding binding0 = new BasicHttpBinding();
    //    Uri address0 = new Uri("http://localhost:8080/ICommService/BasicHttp");

    //    using (service.host = new ServiceHost(typeof(CommService), address0))
    //    {
    //      service.host.AddServiceEndpoint(typeof(ICommService), binding0, address0);
    //      service.host.Open();

    //      Console.Write("\n  CommService is ready.");
    //      Console.Write("\n    Maximum BasicHttp message size = {0}", binding0.MaxReceivedMessageSize);
    //      Console.WriteLine();
    //        //projects = service.ThreadProc();
    //       // new Thread(() => { projects = service.ThreadProc(); }).Start();
    //      //child.Start();
    //      //child.Join();

    //      Console.Write("\n\n  Press <ENTER> to terminate service.\n\n");
    //      Console.ReadLine();
    //      Message msg = this.GetMessage();
    //      projects.Add(msg.ToString());          
    //    }
    //  }
    //  catch (Exception ex)
    //  {
    //    Console.Write("\n  {0}\n\n", ex.Message);
    //  }

    //  return projects;
    //}
  }
}
