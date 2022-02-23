using System.Xml;

namespace FontoXPathCSharp.Expressions;

public struct Name
{
    public readonly string LocalName;
    public readonly string? NamespaceUri;
    public readonly string? Prefix;

    public Name(string localName, string? namespaceUri, string? prefix)
    {
        LocalName = localName;
        NamespaceUri = namespaceUri;
        Prefix = prefix;
    }
}


public class NameTest : AbstractTestExpression
{
    private readonly Name _name;

    public NameTest(Name name)
    {
        _name = name;
    }

    protected internal override bool EvaluateToBoolean(XmlNode node, Value contextItem)
    {
        if (_name.Prefix == null && _name.NamespaceUri != "" && _name.LocalName == "*")
        {
            return true;
        }
        if (_name.Prefix == "*")
        {
            if (_name.LocalName == "*")
            {
                return true;
            }
            return _name.LocalName == node.LocalName;
        }
        if (_name.LocalName != "*")
        {
            if (_name.LocalName != node.LocalName)
            {
                return false;
            }
        }

        // TODO: there is a lot more to add here
        return false;
    }
}