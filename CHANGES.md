# Differences with FontoXPath

Below is a list of differences between this implementation of FontoXPath and the [original](https://github.com/FontoXML/fontoxpath). The changes vary from fundamental differences in how abstractions were done, features that were removed, style/organizational changes neccessitated by C# and small ergonomics changes that were convenient to add.

The main goal of this list is to give an outline of the differences, so the codebase is easier to navigate for those familiar with the original code base and to help in deciding which changes are potential candidates for being ported back over.


## XPath/XQuery Compliance

As it stands, the compiler is effectively XPath 1.0 compliant (ignoring potential bugs). 
More functionality is being added over time and constructs from later XPath versions are available.

## Value System

- Value is AbstractValue (We have also had to be more precise in the codebase at large)
- AtomicValue is a subtype of AbstractValue 
- AbstractValues on their own are fairly useless and need to be cast to subtypes to be of more use.
- DateTime and such do not exist yet.


## Node Pointers

- Where node pointer stuff occurs, we look at if it makes sense to implement it as just nodes.
- There are no nodepointer data types and such though.


## Renames/relocates

- Expression is AbstractExpression
- Functions cannot have the same name as the classes/files they are contained in, so that convention causes some stuff to be named differently.
- Atomize -> AtomizeSequence for example, since the file is called Atomize
- CastToType and TryCastToType are both in TypeCasting.
- Fewer files containing a single function since this clashes badly with C# naming conventions

### Standalone functions to member functions/extension methods:

- IsSubtypeOfAny (utility function) was added for SubTypeOf or chains.
- IsSubTypeOf and associated functions were turned into extension methods, so you can call them directly on a type.
- CastToType is a member function of AbstractValue
- Certain utility functions on ISequences have been added to the ISequence file rather than keeping them standalone.
- Ast handling is quite different (AstHelper is integrated into it)


## Assorted Changes:

- Serious changes in XDM conversion
- ReturnType interface/enum not present anymore, as that cannot be done in C# the ideal solution is not reached yet there, just something workable.
- NameTest uses a QName, no separate parameters

- Specificitiy was reworked to be more robust/consistent.
- Ast node names are an enum (a long one)
- There are a few other places where string/char comparisons were turned into enums.

- We often opted to use delegates to keep things readable and that they can be templated.
- Creating ISequences from iterators that contains AbstractValue themselves (but do inherit it) get passed through an adapter function that does type coercion to AbstractValue.


### Qt3 Testing:

- Tests are loaded through Qt3TestDataProvider.
- Multiple CSV files with debug info are generated in the assets folder using the logging fixture.

