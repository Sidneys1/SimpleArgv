using System;
using SimpleArgv;

namespace TestApp {
	class Program {
		static void Main(string[] argv) {
			var commandLine = new CommandLine(new[] { "--", "-" });

			commandLine.AddArgument(args => string.Join("', '", args), string.Empty);

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

			try {
				if (argv.Length > 0)
					commandLine.Parse(argv);
				else
					commandLine.Parse("Extra","stuff!", "-v", "3", "--path", "C:\\Program", "Files\\");
			} catch (CommandLineArgumentException e) {
				Console.Error.WriteLine($"{e.ParameterName}: {e.Message}");
				Console.ReadLine();
				return;
			}

			Console.WriteLine($"Default:\t['{commandLine.GetValue(string.Empty)}']");

			Console.WriteLine($"Verbose:\t{commandLine.RawArguments.ContainsKey("-v")}/{commandLine.RawArguments.ContainsKey("--verbose")} (Level: {commandLine.GetValue("-v")})");

			Console.WriteLine($"   Path:\t'{commandLine.GetValue("--path", Environment.CurrentDirectory)}'");
			Console.ReadLine();
		}
	}
}
