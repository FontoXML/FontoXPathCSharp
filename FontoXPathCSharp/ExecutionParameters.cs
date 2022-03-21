using System.Xml;

namespace FontoXPathCSharp;

public class ExecutionParameters
{
    public readonly XmlNode DomFacade;

    public ExecutionParameters(XmlNode domFacade)
    {
        DomFacade = domFacade;
    }
}