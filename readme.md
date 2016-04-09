Simple `argv` Parser
================

## Setup

Reference `SimpleArgv` in your project.
Create an instance of `SimpleArgv.CommandLine`, passing in the strings to be used to denote arguments:
```cs
using SimpleArgv;
// ...
var commandLine = new CommandLine(new[] { "--", "-" });
// E.g., --argument, -a
```

Next, register the validation function for the 'loose' arguments (any arguments that come before any argument names).
Validation functions are a `Func<string[], object>` that recieves an array of argv members that came between this argument and the next one, and returns an `object`: the parsed value.
For this example 'loose' validator, we return a string that is the input strings joined by `", "`.
(Loose arguments are identified by the argument name `string.Empty`):
```cs
commandLine.AddArguments(args => string.Join(", ", args), string.Empty);
```

Finally, register each argument you want your program to accept with `CommandLine.AddArgument(Func<string[], object>, params string[] aliases)`.
In this case, we'll set up two arguments: `--verbose, -v`, which accepts a number in the range of `1-3`, and `--path, -p`, which returns its input joined by `" "`.
```cs
commandLine.AddArgument(args =>
{
    if (args.Length > 1)
        throw new ArgumentException($"only one value accepted (got {args.Length})");
    if (args.Length == 0)
        return 1;
    int o;
    if (!int.TryParse(args[0], out o) || o < 1 || o > 3)
        throw new ArgumentException($"expected integer in range 1-3 (got '{args[0]}')");
    return o;
}, "--verbose", "-v");

commandLine.AddArgument(args => string.Join(" ", args), "--path", "-p");
```

`CommandLine.AddArgument` has the potential to throw the exception `ExistingArgumentAliasException`, which indicates that an alias provided has already been registered with/as another argument.

## Parsing

Once our arguments are registered, we call `CommandLine.Parse(params string[] argv)`.
We surround this with a `try...catch` looking for the base exception `CommandLineArgumentException`,
which in part encompasses the following types:

* `UnknownArgumentException`: Raised when an element of argv starting with the argument denotation strings (see CommandLine constructor) is encountered that hasn't been added with `CommandLine.AddArgument`
* `ArgumentAlreadyEncounteredException`: Raised when an argument (or one of its aliases) is encountered a second time
* `ArgumentValidationException`: Raised when an except is encountered within the `Func<string[], string>` passed in with `CommandLine.AddArgument()`. The original exception is passed as `InnerException`, and the original message is copied.

```cs
try {
    commandLine.Parse(argv);
} catch (CommandLineArgumentException e) {
    Console.Error.WriteLine($"{e.ArgumentName}: {e.Message}");
    Console.ReadLine();
    return;
}
```

## Retrieving Values

```cs
// Gets the value of the 'loose' arguments
Console.WriteLine($"Default:\t['{commandLine.GetValue(string.Empty)}']");

// Here we check the RawArguments to see whether the short or long name was used
// We then retrieve the value. Since --verbose and -v are aliased, it doesn't matter which is used as the argument name
Console.WriteLine($"Verbose:\t{commandLine.RawArguments.ContainsKey("-v")}/{commandLine.RawArguments.ContainsKey("--verbose")} (Level: {commandLine.GetValue("-v")})");

// Here we explicitly get the value of path, or the PWD if --path, -p wasn't specified
Console.WriteLine($"   Path:\t'{commandLine.GetValue("--path", Environment.CurrentDirectory)}'");
```