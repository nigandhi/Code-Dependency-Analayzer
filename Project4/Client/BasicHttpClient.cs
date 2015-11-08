///////////////////////////////////////////////////////////////////////
// BasicHttpClient.cs - Consumer of ICommService contract            //
// Language:    C#, 2014, .Net Framework 4.0                         //
// Platform:    Dell Inspiron 3521                                   //
// Author:      Nirav Gandhi, MS in CE, Syracuse University          //
//              (315) 395-4842, nigandhi@syr.edu                     //   
// Source:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading;
using System.Runtime.InteropServices;


namespace CommunicationPrototype
{
  public class Client
  {
    [DllImport("USER32.DLL", SetLastError = true)]
    public static extern void SetWindowPos(
      IntPtr hwnd, IntPtr order,
      int xpos, int ypos, int width, int height,
      uint flags
    );
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();

    private ICommService channel;

    public ICommService channel_ { get { return channel; } set { channel = value; } }
    public Client()
    {
      int ht = Console.LargestWindowHeight;
      int wd = Console.LargestWindowWidth;
      SetWindowPos(GetConsoleWindow(), (IntPtr)0, 550, 50, 400, 600, 0);
 //     Console.Title = "BasicHttpClient";
    }
    public void CreateBasicHttpChannel(string url)
    {
      EndpointAddress address = new EndpointAddress(url);
      BasicHttpBinding binding = new BasicHttpBinding();
      channel = ChannelFactory<ICommService>.CreateChannel(binding, address);
    }
    
    public void ClientRecieveMain(List<string> arr)
    {
        Console.Write("\n  BasicHttpClient Starting to Post Messages to Service");
        Console.Write("\n ======================================================\n");

        CommunicationPrototype.Client client = new CommunicationPrototype.Client();

        // We're parameterizing the channel creation process so 
        // clients can connect to any ICommService server.
        foreach (string str in arr)
        {
            try
            {
                string url = "http://localhost:" + str + "/ICommService/BasicHttp";
                Console.Write("\n  connecting to \"{0}\"\n", url);
                client.CreateBasicHttpChannel(url);
                CommunicationPrototype.Message msg = new CommunicationPrototype.Message();
                msg.text = "GetProjects:8080";
                client.channel.PostMessage(msg);
                
                /////////////////////////////////////////////////////////////
                // This message would shut down the communication service
                // msg.text = "quit";
                // Console.Write("\n  sending message: {0}", msg.text);
                // client.channel.PostMessage(msg);

                //     ((ICommunicationObject)client.channel).Close();
            }
            catch (Exception ex)
            {
                Console.Write("{0}",ex.Message);
            }
            Console.Write("\n\n");
        }
      Console.Write("\n\n");
    }

  }
}
