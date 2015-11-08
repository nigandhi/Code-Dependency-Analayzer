///////////////////////////////////////////////////////////////////////
// CommandLineProcessing.cs - Processes the command line arguments   //
// ver 1.0                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Application: CSE681, Project #2, Fall 2014                        //
// Author:      Nirav Gandhi, MS in CE, Syracuse University          //
//              (315) 395-4842, nigandhi@syr.edu                     //   
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package seperates the command line into path, patterns and options.
 * Also, sets appropriate flags for according to the options present in
 * the command line.
 */
/* Required Files:
 *   uses the System.IO library for accesing the Path and Directory.
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
using System.IO;

namespace CommandLine
{
    public class CommandLineProcessing
    {
        private string path;
        private List<string> patterns = new List<string>();
        private List<string> options = new List<string>();
        private bool recursiveflag = false;
        private bool xmlflag = false;
        private bool relationshipflag = false;

        public string getPath()
        {
            return Path.GetFullPath(path);
        }
        public void setPath(string path_)
        {
            path = path_;
        }
        public List<string> getPatterns()
        {
            return patterns;
        }
        public void setPatterns(List<string> patterns_)
        {
            patterns = patterns_;
        }
        public List<string> getOptions()
        {
            return options;
        }
        public void setOptions(List<string> options_)
        {
            options = options_;
        }
        public bool getrecursiveflag()
        {
            return recursiveflag;
        }
        public bool getrelationshipflag()
        {
            return relationshipflag;
        }
        public bool getxmlflag()
        {
            return xmlflag;
        }
        /// <summary>
        /// //////////////////////////////////////////
        /// sets flags based on options
        public void setflags()
        {
            foreach (string opt in options)
            {
                if (opt == "/S" || opt == "/s")
                    recursiveflag = true;
                else if (opt[1] == 'R' || opt[1] == 'r')
                    relationshipflag = true;
                else if (opt[1] == 'X' || opt[1] == 'x')
                    xmlflag = true;
            }
        }

        ///////////////////////////////////////////////////
        /// Identifies commandline arguments
        public void ProcessCommandLine(string[] args_)
        {
            string path_ = args_[0];
            path = path_;
            for (int i = 1; i < args_.Length; i++)
            {
                if (args_[i].Contains("."))
                    patterns.Add(args_[i]);
                else if (args_[i].Contains("/"))
                    options.Add(args_[i]);
                else
                    Console.Write("\n Not a valid commandline argument \"{0}\" \n", args_[i]);
            }
            setflags();
        }
#if(TEST_COMMANDLINEPROCESSING)
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Write("Please enter Command Line Arguments.\n\n");
                return;
            }
            CommandLineProcessing clp = new CommandLineProcessing();
            clp.ProcessCommandLine(args);
            Console.Write("\n Path: \"{0}\"",clp.getPath());
            Console.Write("\n Patterns:");
            foreach (string patt in clp.patterns)
                Console.Write("\n \t   \"{0}\"", patt);
            Console.Write("\n Options:");
            foreach (string option in clp.options)
                Console.Write("\n \t   \"{0}\"", option);
            Console.Write("\n\n");

        }
    }
#endif
    }
}