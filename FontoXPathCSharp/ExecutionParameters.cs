namespace FontoXPathCSharp;

using System.Xml;

public class ExecutionParameters
{
    public readonly XmlNode DomFacade;

    public ExecutionParameters(XmlNode domFacade)
    {
        DomFacade = domFacade;
    }
}
