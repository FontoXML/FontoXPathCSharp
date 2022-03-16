using System.Xml;

namespace FontoXPathCSharp;

public class ExecutionParameters
{
    public XmlNode DomFacade;

    public ExecutionParameters(XmlNode domFacade)
    {
        DomFacade = domFacade;
    }
}