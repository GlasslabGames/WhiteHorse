using UnityEngine;
using System.Collections;
using GlassLab.Core.Serialization;
using System.Collections.Generic;
using MiniJSON;

namespace GlassLab.Core.Conditional
{
  public class ConditionalTester : MonoBehaviour
  {
    public class TestClass
    {
      [Persist]
      int a;

      public TestClass(int a) { this.a = a; }
      public TestClass() { }
      public string ToString() { return a.ToString(); }
    }

    public Conditional Conditional;

    [Persist]
    public List<List<TestClass>> list = new List<List<TestClass>>();
    public List<Dictionary<string, TestClass>> listDic = new List<Dictionary<string, TestClass>>();
    public Dictionary<string, List<TestClass>> dicList= new Dictionary<string, List<TestClass>>();

    public bool IsSatisfied()
    {
      return Conditional.IsSatisfied;
    }

    void Awake()
    {
      list.Add(new List<TestClass>() { new TestClass(452345), new TestClass(452345) });
      list.Add(new List<TestClass>() { new TestClass(543), new TestClass(7777) });
      list.Add(new List<TestClass>() { new TestClass(222), new TestClass(4444) });
      listDic.Add(new Dictionary<string,TestClass> { {"a", new TestClass(5135)}, {"b", new TestClass(1)} });
      dicList["a"] = new List<TestClass>() { new TestClass(222), new TestClass(4444) };
      dicList["mrhm"] = new List<TestClass>() { new TestClass(222), new TestClass(4444) };
      dicList["e"] = new List<TestClass>() { new TestClass(222), new TestClass(4444) };

      Dictionary<string, object> data = SessionSerializer.Serialize(this);

      string testString = "Before: ";
      foreach (List<TestClass> l in list)
      {
        foreach (TestClass s in l)
        {
          testString += s + ", ";
        }
        testString += "\n";
      }
      foreach (Dictionary<string, TestClass> l in listDic)
      {
        foreach (KeyValuePair<string, TestClass> s in l)
        {
          testString += s.Key + " => " + s.Value + ", ";
        }
        testString += "\n";
      }
      foreach (KeyValuePair<string, List<TestClass>> s in dicList)
      {
        testString += s.Key + " => ";
        foreach (TestClass listValue in s.Value)
        {
          testString += listValue + ", ";
        }
        testString += "\n";
      }
      Debug.Log(testString);
      string tempData = Json.Serialize(data);
      data = (Dictionary<string,object>) Json.Deserialize(tempData);

      SessionDeserializer.Deserialize(this, data);

      testString = "After: ";
      foreach (List<TestClass> l in list)
      {
        foreach (TestClass s in l)
        {
          testString += s + ", ";
        }
        testString += "\n";
      }
      foreach (Dictionary<string, TestClass> l in listDic)
      {
        foreach (KeyValuePair<string, TestClass> s in l)
        {
          testString += s.Key + " => " + s.Value + ", ";
        }
        testString += "\n";
      }
      foreach (KeyValuePair<string, List<TestClass>> s in dicList)
      {
        testString += s.Key + " => ";
        foreach (TestClass listValue in s.Value)
        {
          testString += listValue + ", ";
        }
        testString += "\n";
      }
      Debug.Log(testString);
    }
  }
}