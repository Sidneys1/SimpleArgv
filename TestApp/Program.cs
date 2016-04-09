using System;
using SimpleArgv;

namespace TestApp {
	class Program {
		static void Main(string[] args) {
			var s = new CommandLine(new[] { "--", "-" });

			s.AddArgument(strings => string.Join("', '", strings), string.Empty);

			s.AddArgument(strings =>
			{
				if (strings.Length > 1)
					throw new ArgumentException($"only one value accepted (got {strings.Length})");
				if (strings.Length == 0)
					return 1;
				int o;
				if (!int.TryParse(strings[0], out o) || o < 1 || o > 3)
					throw new ArgumentException($"expected integer in range 1-3 (got '{strings[0]}')");
				return o;
			}, "--verbose", "-v");

			s.AddArgument(strings => string.Join(" ", strings), "--path", "-p");

			try {
				if (args.Length > 0)
					s.Parse(args);
				else
					s.Parse("Extra","stuff!", "-v", "3", "--path", "C:\\Program", "Files\\");
			} catch (CommandLineArgumentException e) {
				Console.Error.WriteLine($"{e.ParameterName}: {e.Message}");
				Console.ReadLine();
				return;
			}

			Console.WriteLine($"Default:\t['{s.GetValue(string.Empty)}']");

			Console.WriteLine($"Verbose:\t{s.RawParameters.ContainsKey("-v")}/{s.RawParameters.ContainsKey("--verbose")} (Level: {s.GetValue("-v")})");

			Console.WriteLine($"   Path:\t'{s.GetValue("--path")}'");

			Console.ReadLine();
		}
	}
}
