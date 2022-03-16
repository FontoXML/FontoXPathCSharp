using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Types;

public class Options
{
    bool annotateAst;
    AbstractValue? currentContext;
    bool debug;
    string defaultFunctionNamespaceURI;
    bool disableCache;
}