using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Types;

public class Options
{
    private bool annotateAst;
    private AbstractValue? currentContext;
    private bool debug;
    private string defaultFunctionNamespaceURI;
    private bool disableCache;
}