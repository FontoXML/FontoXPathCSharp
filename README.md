# FontoXPathCSharp

An experimental C# port of the FontoXPath library. This is experimental code that is work in progress. This is not fit for production yet.

## Differences from FontoXPath

- [Ast](FontoXPathCSharp/Ast.cs) definition consists of typed classes instead of untyped arrays
- [Value](FontoXPathCSharp/Value/AbstractValue.cs) definition now uses a class hierarchy instead of `any`
- Number types are separated (int and decimal)
- Project split up into different modules

## Future Improvements

- Unify [FunctionProperties](FontoXPathCSharp/FunctionProperties.cs) and [FunctionValue](FontoXPathCSharp/Value/FunctionValue.cs)
- Clean up [XdmReturnValue](FontoXPathCSharp/EvaluationUtils/XdmReturnValue.cs)
- Get numeric value from any numeric [AbstractValue](FontoXPathCSharp/Value/ArrayValue.cs)
- Proper sorting of nodes in [PathExpression](FontoXPathCSharp/Expressions/PathExpression.cs)
