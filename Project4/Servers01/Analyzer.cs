///////////////////////////////////////////////////////////////////////
// Analyzer.cs - Controls the execution of the parser                //
// ver 1.0                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Application: CSE681, Project #2, Fall 2014                        //
// Author:      Nirav Gandhi, MS in CE, Syracuse University          //
//              (315) 395-4842, nigandhi@syr.edu                     //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Used to parse each file from the list of files and control the execution 
 * of the parser by deciding what rules and actions that need to be appied 
 * to the file and decide the number of pass that need to be done on the set 
 * of files based on the flag for finding the relationship between types.  
 * 
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
 *   Semi.cs, Toker.cs, Display.cs, CommandLineProcessing.cs
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

namespace CodeAnalysis
{
    public class Analyzer
    {
        static public string[] getFiles(string path, List<string> patterns) { 
            FileMgr fm=new FileMgr();
            foreach (string pattern in patterns)
                fm.addPattern(pattern);
            fm.findFiles(path);
            return fm.getFiles().ToArray();
        }
        ///////////////////////////////////////////////////////
        /// performs analysis on each file and controls parser
        static public void doAnalysis(string[] files,bool relationship) { 
          BuildCodeAnalyzer builder;
          BuildCodeAnalyzer_relation builder_relation;
          Parser parser;
          Parser parser_;
          for(int i=0;i<2;i++) {  
              foreach (object file in files) {
                  CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
                  CSsemi.CSemiExp semi_relation = new CSsemi.CSemiExp();
                  semi.displayNewLines = false;
                  if (!semi.open(file as string)) {
                      Console.Write("\n  Can't open {0}\n\n", file);
                      return;
                  }
                  if (!semi_relation.open(file as string)) {
                      Console.Write("\n  Can't open {0}\n\n", file);
                      return;
                  }
                  if (i == 0)  {
                      builder = new BuildCodeAnalyzer(semi);
                      parser = builder.build();
                      Repository rep1 = Repository.getInstance();
                      rep1.setfilename(Path.GetFileName(file.ToString()));
                      try {
                         while (semi.getSemi())
                             parser.parse(semi);
                      }
                      catch (Exception ex)
                      {
                          Console.Write("\n\n  {0}\n", ex.Message);
                      }
                  }
                  else  {
                      builder_relation = new BuildCodeAnalyzer_relation(semi_relation);
                      parser_ = builder_relation.build_relation();
                      Repository rep1 = Repository.getInstance();
                      rep1.setfilename(Path.GetFileName(file.ToString()));
                      try {
                          while (semi_relation.getSemi())
                              parser_.parse(semi_relation);
                      }
                      catch (Exception ex) {
                          Console.Write("\n\n  {0}\n", ex.Message);
                      }
                  }
                  semi.close(); }
              if (!relationship)
                  return;
            }
        }
#if(TEST_ANALYZER)
        static void Main(string[] args)
        {
            string path = "C:\\Users\\Nirav Gandhi\\Desktop\\testcode";
            List<string> patterns = new List<string>();
            patterns.Add("*.*");
            string[] files = Analyzer.getFiles(path,patterns);
            doAnalysis(files,true);
        }
#endif
    }
}
