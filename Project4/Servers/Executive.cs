///////////////////////////////////////////////////////////////////////
// Executive.cs - Executive file for the project                     //
// ver 1.0                                                           //
// Language:    C#, 2014, .Net Framework 4.0                         //
// Application: CSE681, Project #4, Fall 2014                        //
// Author:      Nirav Gandhi, MS in CE, Syracuse University          //
//              (315) 395-4842, nigandhi@syr.edu                     //   
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package is the main package necessary for the communicating 
 * with different packages. It is specific to Project#2 requirements. 
 * 
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
 *   Semi.cs, Toker.cs, Display.cs, CommandLineProcessing.cs
 *   FileMgr.cs, XML.cs, Analyzer.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 26 Nov 2014
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
    class Servers
    {
        static void Main()
        {
            Console.Write("\n Servers00 starting up....!!");
//            Console.Write("\n  Communication Server Starting up");
            Console.Write("\n ==================================\n");

            try
            {
                Client cl = new Client();
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
    }
}
