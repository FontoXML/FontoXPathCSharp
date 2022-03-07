using FontoXPathCSharp.Expressions;
using ValueType = FontoXPathCSharp.Value.ValueType;

namespace FontoXPathCSharp;

public class Ast
{
    public readonly string Name;
    public Dictionary<string, ValueType> TypeAttributes;
    public Dictionary<string, string> StringAttributes;
    public List<Ast?> Children;
    public string TextContent;

    public Ast(string name)
    {
        Name = name;
        TypeAttributes = new Dictionary<string, ValueType>();
        StringAttributes = new Dictionary<string, string>();
        Children = new List<Ast?>();
    }


    public Ast? GetFirstChild(string name)
    {
        return Children.Find(x => name == "*" || name == x.Name);
    }
    
    public Ast? GetFirstChild(string[] names)
    {
        return Children.Find(x => names.Contains("*") || names.Contains(x.Name));
    }

    public IEnumerable<Ast?> GetChildren(string name)
    {
        return Children.FindAll(x => name == "*" || name.Contains(x.Name));
    }

    public Ast FollowPath(IEnumerable<string> path)
    {
        return path.ToList().Aggregate(this, (ast, p) => ast.GetFirstChild(p));
    }

    public QName GetQName()
    {
        return new QName(TextContent, StringAttributes["URI"], StringAttributes["prefix"]);
    }

    public override string ToString()
    {
        return string.Format("<AST \"{0}\", {{{1}}}, {{{2}}}, \"{3}\", [{4}]>", Name,
            string.Join(", ", StringAttributes.Select(x => $"{x.Key}: \"{x.Value}\"")),
            string.Join(", ", TypeAttributes.Select(x => $"{x.Key}: \"{x.Value}\"")),
            TextContent, Children.Count == 0 ? "" : $"\n{string.Join("\n", Children)}\n");
    }
}