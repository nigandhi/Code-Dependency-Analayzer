///////////////////////////////////////////////////////////////////////
// RulesAndActions.cs - Parser rules specific to an application      //
// ver 3.0                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Platform:    Dell Precision T7400, Win7, SP1                      //
// Application: Demonstration for CSE681, Project #2, Fall 2011      //
// Author:      Nirav Gandhi, MS in CE, Syracuse University          //
//              (315) 395-4842, nigandhi@syr.edu                     //   
// Source:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * RulesAndActions package contains all of the Application specific
 * code required for most analysis tools.
 *
 * It defines the following Four rules which each have a
 * grammar construct detector and also a collection of IActions:
 *   - DetectNameSpace rule
 *   - DetectClass rule
 *   - DetectFunction rule
 *   - DetectScopeChange
 *   - DetectStruct
 *   - DetectNamespace
 *   - DetectArray
 *   - DetectDelegate
 *   - DetectInheritence
 *   - DetectAggregation
 *   - DetectComposition
 *   - DetectUsing
 *   - DetectClassRelation
 *   
 *   Three actions - some are specific to a parent rule:
 *   - Print
 *   - PrintFunction
 *   - PrintScope
 * 
 * The package also defines a Repository class for passing data between
 * actions and uses the services of a ScopeStack, defined in a package
 * of that name.
 *
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *  
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
 *   Semi.cs, Toker.cs
 *   
 * Build command:
 *   csc /D:TEST_PARSER Parser.cs IRuleAndAction.cs RulesAndActions.cs \
 *                      ScopeStack.cs Semi.cs Toker.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 3.0 : 8 Oct 2014
 * - Added new rules and actions for relationship detection
 * - Added new rules for braceless scope, array, stuct and enum
 * ver 2.2 : 24 Sep 2011
 * - modified Semi package to extract compile directives (statements with #)
 *   as semiExpressions
 * - strengthened and simplified DetectFunction
 * - the previous changes fixed a bug, reported by Yu-Chi Jen, resulting in
 * - failure to properly handle a couple of special cases in DetectFunction
 * - fixed bug in PopStack, reported by Weimin Huang, that resulted in
 *   overloaded functions all being reported as ending on the same line
 * - fixed bug in isSpecialToken, in the DetectFunction class, found and
 *   solved by Zuowei Yuan, by adding "using" to the special tokens list.
 * - There is a remaining bug in Toker caused by using the @ just before
 *   quotes to allow using \ as characters so they are not interpreted as
 *   escape sequences.  You will have to avoid using this construct, e.g.,
 *   use "\\xyz" instead of @"\xyz".  Too many changes and subsequent testing
 *   are required to fix this immediately.
 * ver 2.1 : 13 Sep 2011
 * - made BuildCodeAnalyzer a public class
 * ver 2.0 : 05 Sep 2011
 * - removed old stack and added scope stack
 * - added Repository class that allows actions to save and 
 *   retrieve application specific data
 * - added rules and actions specific to Project #2, Fall 2010
 * ver 1.1 : 05 Sep 11
 * - added Repository and references to ScopeStack
 * - revised actions
 * - thought about added folding rules
 * ver 1.0 : 28 Aug 2011
 * - first release
 *
 * Planned Modifications (not needed for Project #2):
 * --------------------------------------------------
 * - add folding rules:
 *   - CSemiExp returns for(int i=0; i<len; ++i) { as three semi-expressions, e.g.:
 *       for(int i=0;
 *       i<len;
 *       ++i) {
 *     The first folding rule folds these three semi-expression into one,
 *     passed to parser. 
 *   - CToker returns operator[]( as four distinct tokens, e.g.: operator, [, ], (.
 *     The second folding rule coalesces the first three into one token so we get:
 *     operator[], ( 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CodeAnalysis
{
  public class Elem  // holds scope information
  {
    public string type { get; set; }
    public string name { get; set; }
    public int begin { get; set; }
    public int end { get; set; }
    public int complexity { get; set; }
    public string filename { get; set; }
    public override string ToString()
    {
        string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
      StringBuilder temp = new StringBuilder();
      temp.Append("{");
      temp.Append(String.Format("{0,-10}", type)).Append(" : ");
      temp.Append(String.Format("{0,-10}", name)).Append(" : ");
      temp.Append(String.Format("{0,-5}", begin.ToString()));  // line of scope start
      temp.Append(String.Format("{0,-5}", end.ToString()));    // line of scope end
      temp.Append("}");
      return temp.ToString();
    }
  }
  public class Elem_relation
  {
      public string file1 { get; set; }
      public string type1 { get; set; }
      public string name1 { get; set; }
      public string file2 { get; set; }
      public string type2 { get; set; }
      public string name2 { get; set; }
      public string relation { get; set; }
  
  }

  public class Repository
  {
    ScopeStack<Elem> stack_ = new ScopeStack<Elem>();
    List<Elem> locations_ = new List<Elem>();
    List<Elem_relation> relation_=new List<Elem_relation>();
    List<string> class_relation_=new List<string>();
    List<string> composed_relation_ = new List<string>();
    string filename_;
      public string filename
      {
          get{return filename_;}
      }
      public void setfilename(string name)
      { filename_ = name; }
    static Repository instance;
    private int complex { get; set; }
    private bool complexflag { get; set; }
    
    public Repository()
    {
      instance = this;
    }

    public List<string> class_relation { get { return class_relation_; } }
    public bool getcomplexflag() { return complexflag; }
    public void setcomplexflag(bool complexflag_) { complexflag = complexflag_; }
    public int getcomplex() { return complex; }
    public void incrementcomplex() { complex++; }
    public void resetcomplex() { complex = 1; }
    public static Repository getInstance()
    {
      return instance;
    }
    // provides all actions access to current semiExp
    public static void clearInstance()  
    {
          if (instance != null)
          {
              instance.class_relation.Clear();
              instance.composed_relations.Clear();
              instance.locations.Clear();
              instance.stack.clear();
              instance.relation_.Clear();
          }
      }
    public CSsemi.CSemiExp semi
    {
      get;
      set;
    }

    // semi gets line count from toker who counts lines
    // while reading from its source

    public int lineCount  // saved by newline rule's action
    {
      get { return semi.lineCount; }
    }
    public int prevLineCount  // not used in this demo
    {
      get;
      set;
    }
    public ScopeStack<Elem> stack  // pushed and popped by scope rule's action
    {
      get { return stack_; } 
    }
    public List<Elem> locations
    {
      get { return locations_; }
    }
    public List<Elem_relation> relations
    {
        get { return relation_; }
    }
      public List<string> composed_relations
      {
        get{ return composed_relation_;}
      }

  }
  /////////////////////////////////////////////////////////
  // pushes types relations info on stack when entering new scope
  public class PushStack_relation : AAction
  {

    Repository repo_;
    public PushStack_relation(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(CSsemi.CSemiExp semi)
    {
        Elem_relation elem = new Elem_relation();
        if (semi[0] == "class"|| semi[0]=="function")
        {
            repo_.class_relation.Add(semi[1]);
            return;
        }
        
        foreach(Elem baseclass in repo_.locations)
        {
            if (baseclass.name == semi[1])
            {
                elem.relation = semi[0];
                elem.type1 = baseclass.type;
                elem.name1 = baseclass.name;
                elem.file1 = baseclass.filename;
                elem.file2 = repo_.filename;
                if (semi.count > 2)
                {
                    elem.type2 = semi[2];
                    elem.name2 = semi[3];
                    repo_.class_relation.Add(semi[3]);
                }
                else
                {
                    elem.name2 = repo_.class_relation[repo_.class_relation.Count-1];
                    if (repo_.class_relation[repo_.class_relation.Count - 1] == "Main")
                        elem.type2 = "function";
                    else
                        elem.type2 = "class";
                }
                repo_.relations.Add(elem);
                return;
            }
        }
     }
  }
  /////////////////////////////////////////////////////////
  // pushes scope info on stack when entering new scope
  public class PushStack : AAction
  {
    Repository repo_;

    public PushStack(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(CSsemi.CSemiExp semi)
    {
      Elem elem = new Elem();
      elem.filename = repo_.filename;  
      elem.type = semi[0];  // expects type
      elem.name = semi[1];
      elem.begin = repo_.semi.lineCount - 1;
      elem.end = 0;
      if (elem.type == "array")
      {
          elem.end = elem.begin;
      }
      if (elem.type == "struct" || elem.type == "enum")
          repo_.composed_relations.Add(elem.name);
      if (repo_.getcomplexflag())
          repo_.incrementcomplex();
      if (elem.type == "braceless")
          return;
        repo_.stack.push(elem);
      if (elem.type == "function")
      {
          repo_.resetcomplex();
          repo_.setcomplexflag(true);
      }
        if (elem.type == "control" || elem.name == "anonymous")
      {
          return;
      }
      repo_.locations.Add(elem);
      if (AAction.displaySemi)
      {
        Console.Write("\n  line# {0,-5}", repo_.semi.lineCount - 1);
        Console.Write("entering ");
        string indent = new string(' ', 2 * repo_.stack.count);
        Console.Write("{0}", indent);
        this.display(semi); // defined in abstract action
      }
      if(AAction.displayStack)
        repo_.stack.display();
    }
  }
  /////////////////////////////////////////////////////////
  // pops scope info from stack when leaving scope
  public class PopStack : AAction
  {
    Repository repo_;

    public PopStack(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(CSsemi.CSemiExp semi)
    {
      Elem elem;
      try
      {
        elem = repo_.stack.pop();
        for (int i = 0; i < repo_.locations.Count; ++i )
        {
          Elem temp = repo_.locations[i];
          if (elem.type == temp.type)
          {
            if (elem.name == temp.name && elem.begin==temp.begin)
            {
              if ((repo_.locations[i]).end == 0)
              {
                (repo_.locations[i]).end = repo_.semi.lineCount;
                break;
              }
            }
          }
        }
      }
      catch
      {
        Console.Write("popped empty stack on semiExp: ");
        semi.display();
        return;
      }
      CSsemi.CSemiExp local = new CSsemi.CSemiExp();
      local.Add(elem.type).Add(elem.name);
      if (elem.type == "function")
      {
          elem.complexity = repo_.getcomplex();
          repo_.setcomplexflag(false);
          repo_.resetcomplex();
      }
      if (local[0] == "control")
          return;
      if (AAction.displaySemi)
      {
        Console.Write("\n  line# {0,-5}", repo_.semi.lineCount);
        Console.Write("leaving  ");
        string indent = new string(' ', 2 * (repo_.stack.count + 1));
        Console.Write("{0}", indent);
        this.display(local); // defined in abstract action
      }
    }
  }

  ///////////////////////////////////////////////////////////
  // action to print function signatures - not used in demo

  public class PrintFunction : AAction
  {
    Repository repo_;

    public PrintFunction(Repository repo)
    {
      repo_ = repo;
    }
    public override void display(CSsemi.CSemiExp semi)
    {
      Console.Write("\n    line# {0}", repo_.semi.lineCount - 1);
      Console.Write("\n    ");
      for (int i = 0; i < semi.count; ++i)
        if (semi[i] != "\n" && !semi.isComment(semi[i]))
          Console.Write("{0} ", semi[i]);
    }
    public override void doAction(CSsemi.CSemiExp semi)
    {
      this.display(semi);
    }
  }
  /////////////////////////////////////////////////////////
  // concrete printing action, useful for debugging

  public class Print : AAction
  {
    Repository repo_;

    public Print(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(CSsemi.CSemiExp semi)
    {
      Console.Write("\n  line# {0}", repo_.semi.lineCount - 1);
      this.display(semi);
    }
  }
  /////////////////////////////////////////////////////////
  // rule to detect namespace declarations

  public class DetectNamespace : ARule
  {
    public override bool test(CSsemi.CSemiExp semi)
    {
      int index = semi.Contains("namespace");
      if (index != -1)
      {
        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
        // create local semiExp with tokens for type and name
        local.displayNewLines = false;
        local.Add(semi[index]).Add(semi[index + 1]);
        doActions(local);
        return true;
      }
      return false;
    }
  }
  /////////////////////////////////////////////////////////
  // rule to detect struct declarations

  public class DetectStruct : ARule
  {
      public override bool test(CSsemi.CSemiExp semi)
      {
          int index = semi.Contains("struct");
          if (index != -1)
          {
              CSsemi.CSemiExp local = new CSsemi.CSemiExp();
              // create local semiExp with tokens for type and name
              local.displayNewLines = false;
              local.Add(semi[index]).Add(semi[index + 1]);
              doActions(local);
              return true;
          }
          return false;
      }
  }

  /////////////////////////////////////////////////////////
  // rule to detect enum declarations

  public class DetectEnum : ARule
  {
      public override bool test(CSsemi.CSemiExp semi)
      {
          int index = semi.Contains("enum");
          if (index != -1)
          {
              CSsemi.CSemiExp local = new CSsemi.CSemiExp();
              // create local semiExp with tokens for type and name
              local.displayNewLines = false;
              local.Add(semi[index]).Add(semi[index + 1]);
              doActions(local);
              return true;
          }
          return false;
      }
  }
  /////////////////////////////////////////////////////////
  // rule to detect delegate declarations

  public class DetectDelegate : ARule
  {
      public override bool test(CSsemi.CSemiExp semi)
      {
          int index = semi.Contains("delegate");
          if (index != -1)
          {
              CSsemi.CSemiExp local = new CSsemi.CSemiExp();
              // create local semiExp with tokens for type and name
              local.displayNewLines = false;
              local.Add(semi[index]).Add(semi[index + 2]);
              doActions(local);
              return true;
          }
          return false;
      }
  }

  /////////////////////////////////////////////////////////
  // rule to dectect class definitions

  public class DetectClass : ARule
  {
    public override bool test(CSsemi.CSemiExp semi)
    {
      int indexCL = semi.Contains("class");
      int indexIF = semi.Contains("interface");
      int indexST = semi.Contains("struct");

      int index = Math.Max(indexCL, indexIF);
      index = Math.Max(index, indexST);
      if (index != -1)
      {
        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
        // local semiExp with tokens for type and name
        local.displayNewLines = false;
        local.Add(semi[index]).Add(semi[index + 1]);
        doActions(local);
        return true;
      }
      return false;
    }
  }
  /////////////////////////////////////////////////////////
  // rule to dectect function definitions
  public class DetectFunction : ARule
  {
    public static bool isSpecialToken(string token)
    {
      string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
      foreach (string stoken in SpecialToken)
        if (stoken == token)
          return true;
      return false;
    }
    public override bool test(CSsemi.CSemiExp semi)
    {
      if (semi[semi.count - 1] != "{")
        return false;

      int index = semi.FindFirst("(");
      if (index > 0 && !isSpecialToken(semi[index - 1]))
      {
        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
        local.Add("function").Add(semi[index - 1]);
        doActions(local);
        return true;
      }
      return false;
    }
  }
  /////////////////////////////////////////////////////////
  // rule to dectect array definitions
  public class DetectArray : ARule
  {
      public static bool isSpecialToken(string token)
      {
          string[] SpecialToken = { "if", "else", "for", "foreach", "while", "try", "catch", "using" };
          foreach (string stoken in SpecialToken)
              if (stoken == token)
                  return true;
          return false;
      }
      public override bool test(CSsemi.CSemiExp semi)
      {
          if (semi.Contains("{")>0)
              if (semi.Contains("[]")>0)
                  //if (semi.Contains("=") > 0)
              {
                  CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                  local.Add("array").Add("array");
                  doActions(local);
                  return true;
              }
         return false;
      }
  }
  /////////////////////////////////////////////////////////
  // rule to dectect bracelessscope definitions
  public class DetectBraceLess : ARule
  {
      public static bool isSpecialToken(string token)
      {
          string[] SpecialToken = { "if","else", "for", "foreach", "while","try", "catch", "using" };
          foreach (string stoken in SpecialToken)
              if (stoken == token)
                  return true;
          return false;
      }
      public override bool test(CSsemi.CSemiExp semi)
      {
          int index = semi.FindFirst("(");
          if (index>0 && isSpecialToken(semi[index-1]))
          {
              CSsemi.CSemiExp local = new CSsemi.CSemiExp();
              local.Add("braceless").Add("braceless");
              doActions(local);
              semi.remove(index);
              test(semi);
              return true;
          }
          return false;
      }
  }
  /////////////////////////////////////////////////////////
  // detect entering anonymous scope
  // - expects namespace, class, and function scopes
  //   already handled, so put this rule after those
  public class DetectAnonymousScope : ARule
  {
    public override bool test(CSsemi.CSemiExp semi)
    {
      int index = semi.Contains("{");
      if (index != -1)
      {
        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
        // create local semiExp with tokens for type and name
        local.displayNewLines = false;
        local.Add("control").Add("anonymous");
        doActions(local);
        return true;
      }
      return false;
    }
  }
  /////////////////////////////////////////////////////////
  // detect leaving scope

  public class DetectLeavingScope : ARule
  {
      public override bool test(CSsemi.CSemiExp semi)
      {
          int index = semi.Contains("}");
          if (index != -1)
          {
              doActions(semi);
              return true;
          }
          return false;
      }
  }
/// <summary>
/// //////////////////////////////////////////////////////////
/// detect inheritance relationship
/// </summary>
  public class DetectInheritance : ARule
  {
      public override bool test(CSsemi.CSemiExp semi)
      {
          int index = semi.Contains(":");
          if (index != -1)
          {
              CSsemi.CSemiExp local = new CSsemi.CSemiExp();
              local.displayNewLines = false;
              local.Add("Inheritance").Add(semi[index + 1]).Add(semi[index - 2]).Add(semi[index - 1]);
              doActions(local);
              if ((index = semi.Contains(",")) != -1)
              {
                  local.insert(1, semi[index + 1]);
                  local.remove(2);
                  doActions(local);
              }
              return true;
          }
          return false;
      }
  }
  /////////////////////////////////////////////////////////
  // rule to dectect class definitions for detecting relations

  public class DetectClassRelation : ARule
  {
      public override bool test(CSsemi.CSemiExp semi)
      {
          int indexCL = semi.Contains("class");
          int indexMa = semi.Contains("Main");

          int index = Math.Max(indexCL, indexMa);
          index = Math.Max(index, indexMa);

          if (index != -1)
          {
              CSsemi.CSemiExp local = new CSsemi.CSemiExp();
              // local semiExp with tokens for type and name
              local.displayNewLines = false;
              if(semi[index]=="Main")
                  local.Add("function").Add(semi[index]);
              else
                  local.Add(semi[index]).Add(semi[index + 1]);
              doActions(local);
              return true;
          }
          return false;
      }
  }

  /// //////////////////////////////////////////////////////////
  /// detect aggregation relationship
  /// </summary>
  public class DetectAggregation : ARule
  {
      public override bool test(CSsemi.CSemiExp semi)
      {
          int index = semi.Contains("new");
          if (index != -1)
          {
              CSsemi.CSemiExp local = new CSsemi.CSemiExp();
              local.displayNewLines = false;
              if (semi.Contains(".") != -1)
                  index = semi.FindFirst(".");
              local.Add("Aggregation").Add(semi[index + 1]);
              doActions(local);
              return true;
          }
          return false;
      }
  }

  /// //////////////////////////////////////////////////////////
  /// detect aggregation relationship
  /// </summary>
  public class DetectComposition : ARule
  {
    Repository repo_=Repository.getInstance();
      public override bool test(CSsemi.CSemiExp semi)
      {
          foreach(string structlist in repo_.composed_relations)
          {
              int index = semi.Contains(structlist);
              if (index != -1 && semi.Contains("struct")==-1 && semi.Contains("enum")==-1)
              {
                  CSsemi.CSemiExp local = new CSsemi.CSemiExp();
                  local.displayNewLines = false;
                  local.Add("Composition").Add(semi[index]);
                  doActions(local);
                  return true;
              }
          }
          return false;
      }
  }
 
 /// //////////////////////////////////////////////////////////
 /// detect Using relationship
  public class DetectUsing : ARule
  {
      public static bool isSpecialToken(string token)
      {
          string[] SpecialToken = { "if", "for", "foreach", "while","Main", "catch", "using" };
          foreach (string stoken in SpecialToken)
              if (stoken == token)
                  return true;
          return false;
      }
      public override bool test(CSsemi.CSemiExp semi)
      {
          if (semi[semi.count - 1] != "{" )
              return false;

          int index = semi.FindFirst("(");
          if (semi[index + 1] == ")")
              return false;
          if (index > 0 && !isSpecialToken(semi[index - 1]))
          {
              CSsemi.CSemiExp local = new CSsemi.CSemiExp();
              if (semi.Contains(".") != -1)
                  index = semi.FindFirst(".");
              local.Add("Using").Add(semi[index + 1]);
              doActions(local);
              return true;
          }
          return false;
      }
  }
  public class BuildCodeAnalyzer
  {
    static Repository repo = new Repository();

    public BuildCodeAnalyzer(CSsemi.CSemiExp semi)
    {
      repo.semi = semi;
    }
    public virtual Parser build()    {
      Parser parser = new Parser();
      AAction.displaySemi = false;
      AAction.displayStack = false;  // this is default so redundant
      PushStack push = new PushStack(repo);
      // capture namespace info
      DetectNamespace detectNS = new DetectNamespace();
      detectNS.add(push);
      parser.add(detectNS);
      // capture struct info
      DetectStruct detectSt = new DetectStruct();
      detectSt.add(push);
      parser.add(detectSt);
      // capture enum info
      DetectEnum detectEn = new DetectEnum();
      detectEn.add(push);
      parser.add(detectEn);
      // capture delegate info
      DetectDelegate detectDl = new DetectDelegate();
      detectDl.add(push);
      parser.add(detectDl);
      // capture class info
      DetectClass detectCl = new DetectClass();
      detectCl.add(push);
      parser.add(detectCl);
      // capture function info
      DetectFunction detectFN = new DetectFunction();
      detectFN.add(push);
      parser.add(detectFN);
      // handle entering anonymous scopes, e.g., if, while, etc.
      DetectAnonymousScope anon = new DetectAnonymousScope();
      anon.add(push);
      parser.add(anon);
      // capture array info
      DetectArray detectArr = new DetectArray();
      detectArr.add(push);
      parser.add(detectArr);
      // handle leaving scopes
      DetectLeavingScope leave = new DetectLeavingScope();
      PopStack pop = new PopStack(repo);
      leave.add(pop);
      parser.add(leave);
      // capture bracelessscope info
      DetectBraceLess detectBL = new DetectBraceLess();
      detectBL.add(push);
      parser.add(detectBL);
      // parser configured
      return parser;
    }
  }
  public class BuildCodeAnalyzer_relation
  {
      Repository repo = Repository.getInstance();

      public BuildCodeAnalyzer_relation(CSsemi.CSemiExp semi)
      {
          repo.semi = semi;
      }
    public virtual Parser build_relation()
    {
        Parser parser_rel = new Parser();

        // decide what to show
        AAction.displaySemi = false;
        AAction.displayStack = false;  // this is default so redundant

        //// action used for namespaces, classes, and functions
        PushStack_relation push_rel = new PushStack_relation(repo);

        // capture inheritence info
        DetectInheritance detectIn = new DetectInheritance();
        detectIn.add(push_rel);
        parser_rel.add(detectIn);

        // capture inheritence info
        DetectAggregation detectAg = new DetectAggregation();
        detectAg.add(push_rel);
        parser_rel.add(detectAg);

        // capture using info
        DetectUsing detectUs = new DetectUsing();
        detectUs.add(push_rel);
        parser_rel.add(detectUs);

        // capture using info
        DetectComposition detectCm = new DetectComposition();
        detectCm.add(push_rel);
        parser_rel.add(detectCm);

        // capture inheritence info
        DetectClassRelation detectCl = new DetectClassRelation();
        detectCl.add(push_rel);
        parser_rel.add(detectCl);


        // parser configured
        return parser_rel;

    }
  }
}
