///////////////////////////////////////////////////////////////////////
// FileMgr.cs - Executive file for the project                       //
// ver 1.0                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Application: CSE681, Project #2, Fall 2014                        //
// Author:      Nirav Gandhi, MS in CE, Syracuse University          //
//              (315) 395-4842, nigandhi@syr.edu                     //   
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package searches for file present in the current directory. 
 * If needed this package also searches for files with particular 
 * pattern recursively through the sub directories. 
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
    public class FileMgr
    {
        private List<string> files=new List<string>();
        private List<string> patterns = new List<string>();
        private bool recurse = false;
        public List<string> getDirectories(string path)
        {
            List<string> dirs = new List<string>();
            dirs=Directory.GetDirectories(path).ToList();
            return dirs;
        }
        ///////////////////////////////////////////////////////
        /// searches for files at the path provided
        public void findFiles(string path )
        {
            if (patterns.Count == 0)
                addPattern("*.*");
            try
            {
                foreach (string pattern in patterns)
                {
                    string[] newfiles = Directory.GetFiles(path, pattern);
                    for (int i = 0; i < newfiles.Length; ++i)
                    {
                        newfiles[i] = Path.GetFullPath(newfiles[i]);
                    }
                    files.AddRange(newfiles);
                }
                if (recurse)
                {
                    string[] dirs = Directory.GetDirectories(path);
                    foreach (string dir in dirs)
                        findFiles(dir);
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}\n", ex.Message);
            }
        }
        public void addPattern(string pattern)
        {
            patterns.Add(pattern);
        }
        public List<string> getFiles()
        {
            return files;
        }
        public void setrecurseflag(bool flag)
        {
            recurse = flag;
        }
        public void setPattern(List<string> patterns_)
        {
            patterns=patterns_;
        }
#if(TEST_FILEMGR)
        static void Main(string[] args)
        {
            Console.Write("\n Testing FileMgr Class");
            Console.Write("\n ======================");

            FileMgr fm = new FileMgr();
            fm.setrecurseflag(true);
            fm.addPattern("*.*");
            fm.findFiles("../../../testcode");
            List<string> files=fm.getFiles();
            foreach (string file in files)
                Console.Write("\n {0}", file);
            Console.Write("\n\n");
        }
#endif
    }
}
