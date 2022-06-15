using System.Collections.Generic;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;

namespace XPathTest;

public class QT3Tests
{
    public static void Bla(string[] args)
    {
        var qt3tests = new XmlDocument();
        qt3tests.Load("assets/QT3TS/catalog.xml");

        var nodes = qt3tests;
        Evaluate.EvaluateXPathToBoolean("child::*", qt3tests, null, new Dictionary<string, IExternalValue>(), new Options());

    }
}