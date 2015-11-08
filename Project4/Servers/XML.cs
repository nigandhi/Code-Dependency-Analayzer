///////////////////////////////////////////////////////////////////////
// XML.cs -     Prints the output to the XML file                    //
// ver 1.0                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Application: CSE681, Project #2, Fall 2014                        //
// Author:      Nirav Gandhi, MS in CE, Syracuse University          //
//              (315) 395-4842, nigandhi@syr.edu                     //   
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package takes data from the repository and prints it to the XML file.
 * 
 * Defines the following methods for printing it to the XML file
 * - XMLWrite: writes the data from repository to XML. 
 * 
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
 *   Semi.cs, Toker.cs, Display.cs, CommandLineProcessing.cs
 *   FileMgr.cs, XML.cs, Analyzer.cs
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
using System.Xml;
using System.Xml.Linq;

namespace CodeAnalysis
{
    public class XML
    {
        Repository repo;
        XDocument xdoc;
        public XDocument xdoc_ { get { return xdoc; } }
        ///////////////////////////////////////////////////
        /// used to display functions in XML format
        public void XMLFunction(XElement tt)
        {
            try
            {
                XElement tt2 = new XElement("FunctionTable");
                if (repo == null)
                    return;
                foreach (Elem elem in repo.locations)
                {
                    if (elem.type != "function" || elem.type == "namespace")
                        continue;
                    XElement t = new XElement(elem.type);
                    XElement f = new XElement("FileName");
                    f.Value = elem.filename;
                    t.Add(f);
                    XElement n = new XElement("Name");
                    n.Value = elem.name;
                    t.Add(n);
                    XElement s = new XElement("Size");
                    s.Value = (elem.end-elem.begin).ToString();
                    t.Add(s);
                    XElement c = new XElement("Complexity");
                    c.Value = elem.complexity.ToString();
                    t.Add(c);
                    tt2.Add(t);
                }
                tt.Add(tt2);
                tt.Save("output.xml");
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}\n", ex.Message);
            }
        }
        ///////////////////////////////////////////////////
        /// used to display types in XML format
        public XElement XMLType(XElement tt)
        {
            try
            {
                repo = Repository.getInstance();
                xdoc = new XDocument();
//                XElement tt = new XElement("Output");
                if (repo == null)
                    return tt;
                XElement tt1 = new XElement("TypeTable");
                foreach (Elem elem in repo.locations)
                {
                    if (elem.type == "function" || elem.type == "namespace")
                        continue;
                    XElement t = new XElement(elem.type);
                    XElement f = new XElement("FileName");
                    f.Value = elem.filename;
                    t.Add(f);
                    XElement n = new XElement("Name");
                    n.Value = elem.name;
                    t.Add(n);
                    XElement s = new XElement("StartLine");
                    s.Value = elem.begin.ToString();
                    t.Add(s);
                    XElement c = new XElement("EndLine");
                    c.Value = elem.end.ToString();
                    t.Add(c);
                    tt1.Add(t);
                }
                tt.Add(tt1);
                tt.Save("output.xml");
                return tt;
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}\n", ex.Message);
                return null;
            }

        }
        ///////////////////////////////////////////////////
        /// used to display relations in XML format
        public void XMLRelation(XElement tt)
        {
            try
            {
                repo = Repository.getInstance();
                xdoc = new XDocument();
                XElement tt3 = new XElement("RelationTable");
                if (repo == null)
                    return;
                foreach (Elem_relation elem in repo.relations)
                {
                    XElement t = new XElement(elem.relation);
                    XElement f1 = new XElement("File1");
                    f1.Value = elem.file1;
                    t.Add(f1);
                    XElement t1 = new XElement("Type1");
                    t1.Value = elem.type1;
                    t.Add(t1);
                    XElement n1 = new XElement("Name1");
                    n1.Value = elem.name1;
                    t.Add(n1);
                    XElement f2 = new XElement("File2");
                    f2.Value = elem.file2;
                    t.Add(f2);
                    XElement t2 = new XElement("Type2");
                    t2.Value = elem.type2;
                    t.Add(t2);
                    XElement n2 = new XElement("Name2");
                    n2.Value = elem.name2;
                    t.Add(n2);
                    tt3.Add(t);
                }
                tt.Add(tt3);
                tt.Save("output.xml");
                return;
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}\n", ex.Message);
                return;
            }       
        }
        ///////////////////////////////////////////////////
        /// used to control the flow of execution
        public void XMLWrite(bool flag)
        {
            XElement xe = new XElement("Output");
//            if (flag)
                XMLRelation(xe);
//            else
            {
                XMLType(xe);
                XMLFunction(xe);
            }
        }
#if(TEST_XML)
        static void Main(string[] args)
        {
            XML x = new XML();
            x.XMLWrite(true);
        }
#endif
    }
}
