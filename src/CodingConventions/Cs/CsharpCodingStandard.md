# C# Coding standard
## Motivation
Code conventions are important to programmers for a number of reasons:
* 80% of the lifetime cost of a piece of software goes to maintenance.
* Hardly any software is maintained for its whole life by the original author.
* They create a consistent look to the code, so that readers can focus on content, not layout.
* They enable readers to understand the code more quickly by making assumptions based on previous experience.
* They facilitate copying, changing, and maintaining the code.

This convention mostly follows [Microsoft C# coding guidelines](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions).

## Naming conventions
* No name prefixes should be used unless explicitly stated in this document (common exceptions are names of interfaces and private fields). Do not use Hungarian notation.
* Classes, structures, methods, properties, events, delegates, enumerations, non-private constants and fields should use `PascalCaseNaming`.
* Interface names should have `I` prefix (for example `IDisposable`).
* Private constants and fields should use `_camelCaseNaming` (with underscore prefix). Do not use screaming caps for constants or readonly variables.
* Local variables and method arguments should use `camelCaseNaming`.
* Avoid using abbreviations. Exceptions are commonly used ones, such as `Id`, `Xml`, `Ftp` etc. Note that only the first letter of the abbreviation is capital.
* Use predefined type names instead of system type names (for example use `int` instead of `Int32`). 
* Use noun or noun phrases to name classes and structures: `Employee`, `DocumentCollection` etc.
* Use noun phrases or adjectives to name interfaces: `IShape`, `IShapeCollection`, `ICancelable`.
* Name source files according to their classes. An exception: file names with partial classes reflect their source or purpose: `Task.generated.cs`, `MainForm.designer.cs` etc.
* Use `Async` suffix in the asynchronous method names:
```csharp
Task<string> DownloadFileAsync()
{
    // ...
}
```
* Use verb or verb phrases (in past) to name events: `Enabled`, `ButtonClicked`:
```csharp
public event EventHandler Enabled;
private void OnEnabled(object sender, EventArgs args);
```

## Layout conventions
* Use tabs (\0x09) for indentation. Tab size should be set to 4 spaces.
* Vertically align curly brackets.
```csharp
public class SomeClass
{
}
```

* Write only one statement per line.
* Write only one declaration per line.
* Always insert single space after a coma, semicolon, loop and conditional keywords (`do`, `while`, `for`, `if`):
```csharp
for (i = 0; i < 10; ++i)
{
    // ...
}
```

* Always surround binary operators with spaces:
```csharp
int n = (a < b) ? a + b - 10 : a;
```

* Do not insert spaces after `(` `[` or before `)` `]`:
```csharp
if (a > b)
{
    // ...
}
```

* Use the standard class layout order (see sample at the end of the document).

## Commenting conventions
* Use native C# XML comment syntax. Always comment all public assembly content.
* Place the comment on a separate line, not at the end of a line of code.
* Begin comment text with an uppercase letter.
* End comment text with a period.
* Insert one space between the comment delimiter `//` or `///` and the comment text, as shown in the following example. 
```csharp
// The following declaration creates a query. It does not run
// the query.
```

* Use standard comment pattern for constructors:
```csharp
/// <summary>
/// Initializes a new instance of the <see cref="SomeClass"/> class.
/// </summary>
```

## Language guidelines
* Do not use non-private fields.
* Use `var` instead of explicit type name where possible.
* In general, use `int` rather than unsigned types. The use of `int` is common throughout C#, and it is easier to interact with other libraries when you use `int`.
* Use curly brackets even for single-line condition/loop cases.
* Use single blank line to separate method implementations inside a class/structure.

## Good programming practices
* Do use [MVC](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93controller) architectural patterns to split application into essential parts.
* Do use [SOLID](https://en.wikipedia.org/wiki/SOLID_(object-oriented_design)) design principles as common practice.
* Consider using [Dispose pattern](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern) when implementing `IDisposable` interface.
* Use `EventHandler` delegate type for declaring events; always make event handler method `private`.
* Use `Enviroment.NewLine` instead of hard-coding the line breaks.
* Use `Path.Combine()` or `Path.DirectorySeparatorChar` for path construction.
* Do check argument values for publicly visible members (and throw `ArgumentException` if the value is not valid).
* Avoid using conditional compilation directives (`#if`, `#define` etc.). Use `ConditionalAttribute` instead if possible. In cases when usage of the directives is unavoidable extract the conditional code into separate classes/methods.
* Split method code into operator groups that implement a complete step. Separate the groups with blank lines. Consider moving local variables initialization into the very first group. Avoid single-line groups.
* Avoid commenting blocks of code: if you don't need the code than just remove it. If you think the code might be still usable in future, extract it into a method/class/conditional block. If this does not work for you at least leave a comment before the block with information on:
    * What this code actually does;
    * Why the code has been commented out;
    * The conditions when the code should be uncommented.

## Sample code
The below example demonstrates most of the rules described in this document. Please note usage of the regions:
```csharp
/// <summary>
/// Sample C# class.
/// </summary>
public class SampleCsharpClass : MonoBehaviour, IEnumerator, IDisposable
{
    #region data

    private const int _privateConstant = 31;

    private static long _staticField;

    private readonly string _readonlyField;

    private int _memberField1;
    private long _memberField2;

    #endregion

    #region interface

    /// <summary>
    /// Sample public event.
    /// </summary>
    public event EventHandler SampleEvent;

    /// <summary>
    /// Sample public property.
    /// </summary>
    public int SampleProperty => _memberField1;

    /// <summary>
    /// Initializes a new instance of the <see cref="SampleCsharpClass"/> class.
    /// </summary>
    public SampleCsharpClass(string s)
    {
        _readonlyField = s;
    }

    /// <summary>
    /// Sample public method.
    /// </summary>
    public void PublicMethod()
    {
        if (_memberField1 > 1)
        {
            for (int i = 0; i < _memberField1; ++i)
            {
                // Do something.
            }
        }
    }

    /// <summary>
    /// Sample protected method.
    /// </summary>
    protected void ProtectedMethod()
    {
        SampleEvent?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Implement resource cleanup logic to be shared between <see cref="Dispose()"/> method and the optional finalizer.
    /// </summary>
    /// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern"/>
    protected virtual void Dispose(bool disposing)
    {
    }

    #endregion

    #region MonoBehaviour

    private void Start()
    {
        // TODO
    }

    #endregion

    #region IEnumerator

    /// <inheritdoc/>
    public object Current => null;

    /// <inheritdoc/>
    public bool MoveNext() => false;

    /// <inheritdoc/>
    public void Reset() => throw new NotSupportedException();

    #endregion

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region implementation

    private void PrivateMethod()
    {
    }

    #endregion
}
```
