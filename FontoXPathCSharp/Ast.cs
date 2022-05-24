using FontoXPathCSharp.Value;

namespace FontoXPathCSharp;

public class Ast
{
    public readonly string Name;
    public List<Ast> Children;
    public Dictionary<string, string> StringAttributes;
    public string TextContent;

    public Ast(string name)
    {
        Name = name;
        StringAttributes = new Dictionary<string, string>();
        Children = new List<Ast>();
        TextContent = "";
    }


    public Ast? GetFirstChild(string name)
    {
        return Children.Find(x => name == "*" || name == x.Name);
    }

    public Ast? GetFirstChild(string[] names)
    {
        return Children.Find(x => names.Contains("*") || names.Contains(x.Name));
    }

    public IEnumerable<Ast> GetChildren(string name)
    {
        return Children.FindAll(x => name == "*" || name.Contains(x.Name));
    }

    public Ast? FollowPath(IEnumerable<string> path)
    {
        var ast = this;

        foreach (var p in path)
        {
            ast = GetFirstChild(p);
            if (ast == null) break;
        }

        return ast;
    }

    public QName GetQName()
    {
        return new QName(TextContent, StringAttributes["URI"],
            StringAttributes.ContainsKey("prefix") ? StringAttributes["prefix"] : null);
    }

    public override string ToString()
    {
        return string.Format("<AST \"{0}\", {{{1}}}, \"{2}\", [{3}]>", Name,
            string.Join(", ", StringAttributes.Select(x => $"{x.Key}: \"{x.Value}\"")),
            TextContent, Children.Count == 0 ? "" : $"\n{string.Join("\n", Children)}\n");
    }
}