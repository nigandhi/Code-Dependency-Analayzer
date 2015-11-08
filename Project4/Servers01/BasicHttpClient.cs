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

        private ICommService channel_;
        public ICommService channel { get { return channel_; } set { channel_ = value; } }
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

    }
}
