///////////////////////////////////////////////////////////////////////
// Display.cs - Display results to the console                       //
// ver 1.0                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Application: CSE681, Project #2, Fall 2014                        //
// Author:      Nirav Gandhi, MS in CE, Syracuse University          //
//              (315) 395-4842, nigandhi@syr.edu                     //   
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Displays result from the repository package to console. Seperate methods are 
 * written for displaying different parts of the output. Here, as this package 
 * takes data from the repository it is indirectly dependent on the parser files.
 *  
 */
/* Required Files:
 * IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
 * Semi.cs, Toker.cs, FileMgr.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 08 Oct 2014
 * - first release
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
    public class Display
    {
        Repository repo_=Repository.getInstance();
        ////////////////////////////////////////////////
        /// displays types present in the files
        void DisplayTypes()
        {
            Console.Write("Type Table\n==========\n\n");
            Console.Write("{0,22} {1,10} {2,20} {3,10} {4,10}", "Filename", "Type", "Name", "Start Line", "End Line");
            Console.Write("\n===============================================================================");
            try
            {
                foreach (Elem e in repo_.locations)
                {
                    if (e.type != "function" && e.type != "namespace")
                        Console.Write("\n{0,22} {1,10} {2,20} {3,10} {4,10}", e.filename, e.type, e.name, e.begin, e.end);
                }
            }

            catch (Exception ex)
            {
                Console.Write("\n\n  {0}\n", ex.Message);
            }
            Console.Write("\n");
            Console.Write("\n");
        }
        ////////////////////////////////////////////////
        /// displays functions present in the files
        void DisplayFunctions()
        {
            Console.Write("\n\nFunction Table\n==============\n\n");
            Console.Write("{0,22} {1,10} {2,20} {3,10} {4,10}", "Filename","Type", "Name", "Size", "Complexity");
            Console.Write("\n=================================================================================");
            try { 
                foreach (Elem e in repo_.locations)
                {
                    if (e.type == "function")
                        Console.Write("\n{0,22} {1,10} {2,20} {3,10} {4,10}", e.filename, e.type, e.name, e.end - e.begin, e.complexity);
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}\n", ex.Message);
            }
            Console.Write("\n");
            Console.Write("\n");
        }
        ////////////////////////////////////////////////
        /// displays relation between types present in the files
        void DisplayRelation()
        {
            Console.Write("\n\nRelation Table\n=============\n\n");
            Console.Write("  {0,12} {1,23} {2,17} {3,20} ", "Relation","Filename","Type", "Name");
            Console.Write("\n==============================================================================");
            try{
                if (repo_ == null)
                    return;
                foreach (Elem_relation e in repo_.relations)
                {
                    Console.Write("\n\n\"{0,12}\" {1,23} {2,17} {3,20} ", e.relation, e.file1, e.type1, e.name1);
                    if(e.relation=="Aggregation")
                        Console.Write("\n{0,12}","aggregated by");
                    else if (e.relation == "Inheritance")
                        Console.Write("\n {0,12}", "inherited by");
                    else if (e.relation == "Composition")
                        Console.Write("\n {0,12}", "composed by");
                    else if (e.relation == "Using")
                        Console.Write("\n {0,12}", "used by");
                    Console.Write("{0,25} {1,17} {2,20} ", e.file2, e.type2, e.name2); 
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}\n", ex.Message);
            }
            Console.Write("\n");
            Console.Write("\n");
        }
        ////////////////////////////////////////////////
        /// displays list of files that are 
        public void Displayfiles(List<string> files)
        {
            Console.Write("\n\nList of Files to be processed:\n=====================================\n\n");
            foreach (string file in files)
            {
                Console.Write(" {0} \n",file);
            }
            Console.Write("\n");
        }
        ////////////////////////////////////////////////
        /// displays list of files that are 
        public void DisplayCommandline(CommandLine.CommandLineProcessing cl_)
        {
            Console.Write("\n\nCommand Line Arguments:\n===================================\n");
            Console.Write("\nPath: {0}", cl_.getPath());
            Console.Write("\n\nPatterns: ");
            foreach (string patt in cl_.getPatterns())
                Console.Write("\n {0,15}", patt);
            Console.Write("\n\nOptions: ");
            foreach (string opt in cl_.getOptions())
                Console.Write("\n {0,13}", opt);
            Console.Write("\n\n");
        }
        ///////////////////////////////////////
        /// controls other display modules
        public void DisplayData(CommandLine.CommandLineProcessing cl)
        {
            DisplayCommandline(cl);
            if (cl.getrelationshipflag())
                DisplayRelation();
            else
            {
                DisplayTypes();
                DisplayFunctions();
            }
            if (cl.getxmlflag())
                Console.Write("\nXML file \"output.xml\" created.\n\n");
        }
#if(TEST_DISPLAY)
        static void Main(string[] args)
        {
            CommandLine.CommandLineProcessing clp = new CommandLine.CommandLineProcessing();
            clp.ProcessCommandLine(args);
            Display d = new Display();
            d.DisplayData(clp);

        }
#endif
    }
}
