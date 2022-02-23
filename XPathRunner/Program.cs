using FontoXPathCSharp;

static void PrintAst(IEnumerable<object> ast, int indent = 0)
{
    foreach (var a in ast)
        if (a is object[] objects)
            PrintAst(objects, indent + 1);
        else
            Console.WriteLine(new string('\t', indent) + a);
}

var parser = XPathParser.PathExpr();

var result = parser("self::p", 0).Unwrap();
Console.WriteLine("Result:");
PrintAst(result);