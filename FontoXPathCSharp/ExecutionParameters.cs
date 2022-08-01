using FontoXPathCSharp.DocumentWriter;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.NodesFactory;
using FontoXPathCSharp.Types;

namespace FontoXPathCSharp;

public class ExecutionParameters<TNode>
{
    public ExecutionParameters(
        bool debug,
        bool disableCache,
        IDomFacade<TNode> domFacade,
        object? currentContext,
        INodesFactory<TNode>? nodesFactory = null,
        IDocumentWriter<TNode>? documentWriter = null,
        Action<string>? logger = null,
        XmlSerializerFunc<TNode>? xmlSerializer = null)
    {
        Debug = debug;
        DisableCache = disableCache;
        DomFacade = domFacade;
        NodesFactory = nodesFactory;
        DocumentWriter = documentWriter;
        CurrentContext = currentContext;
        Logger = logger;
        XmlSerializer = xmlSerializer;
    }

    public bool Debug { get; }

    public bool DisableCache { get; }

    public IDomFacade<TNode> DomFacade { get; }

    public INodesFactory<TNode>? NodesFactory { get; }

    public IDocumentWriter<TNode>? DocumentWriter { get; }

    public object? CurrentContext { get; }

    public Action<string>? Logger { get; }

    public XmlSerializerFunc<TNode>? XmlSerializer { get; }
}