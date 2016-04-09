using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleArgv {

	/// <summary>
	/// Provides methods to manage command-line arguments
	/// </summary>
	public class CommandLine {
		#region Fields

		private readonly string[] _preFixes;
		private readonly Dictionary<string, string> _argNameLookup;
		private readonly Dictionary<string, string[]> _aliasLookup;
		private readonly Dictionary<string, Func<string[], object>> _validationFuncs;
		private readonly Dictionary<string, object> _arguments = new Dictionary<string, object>();
		private readonly Dictionary<string, string[]> _rawArguments = new Dictionary<string, string[]>();

		#endregion Fields

		#region Properties

		/// <summary>
		/// The raw command-line input
		/// </summary>
		public IReadOnlyDictionary<string, string[]> RawArguments => _rawArguments;

		#endregion Properties

		#region Constructors
		
		/// <summary>
		/// Create a new instance of the CommandLine class
		/// </summary>
		/// <param name="preFixes">Prefixes used to denote argument names. E.g. '--' and '-', or '/'</param>
		/// <param name="argumentNameCompararer">The string comparison operator used to match parameter names.</param>
		/// <exception cref="ArgumentException">Invalid arguments</exception>
		public CommandLine(string[] preFixes, StringComparer argumentNameCompararer = null) {
			if (preFixes.Length == 0)
				throw new ArgumentException("There must be at least one prefix", nameof(preFixes));

			_preFixes = preFixes.OrderByDescending(o => o.Length).ToArray();
			var stringComparer = argumentNameCompararer ?? StringComparer.OrdinalIgnoreCase;
			_validationFuncs = new Dictionary<string, Func<string[], object>>(stringComparer);
			_argNameLookup = new Dictionary<string, string>(stringComparer);
			_aliasLookup = new Dictionary<string, string[]>(stringComparer);
		}

		#endregion Constructors

		#region Methods

		/// <summary>
		/// Registers an argument validator and aliases
		/// </summary>
		/// <param name="validator">
		/// The function used to valiate the argument values (as an array of strings).
		/// Returns the parsed value as an object
		/// </param>
		/// <param name="aliases">The name and any aliases of the argument (inluding prefix)</param>
		/// <exception cref="ArgumentException">Invalid arguments</exception>
		public void AddArgument(Func<string[], object> validator, params string[] aliases) {
			if (aliases.Length == 0)
				throw new ArgumentException("There must be at least one alias", nameof(aliases));

			var primary = aliases[0];

			foreach (var alias in aliases) {
				if (_argNameLookup.ContainsKey(alias))
					throw new ExistingArgumentAliasException("argument already exists", alias);
				_argNameLookup.Add(alias, primary);
			}

			_aliasLookup.Add(primary, aliases);

			_validationFuncs.Add(primary, validator);
		}

		/// <summary>
		/// Retrieve the value of the given parameter. Aliases will be translated.
		/// </summary>
		/// <typeparam name="T">The type to return</typeparam>
		/// <param name="argumentName">The name of the argument to retreive the value of</param>
		/// <param name="defaultValue">The default value to return if the specified argument doesn't exist or its value is not of type T</param>
		/// <returns>The value of the given argument if it exists and is of type T. Otherwise, returns defaultValue</returns>
		public T GetValue<T>(string argumentName, T defaultValue) {
			if (!_argNameLookup.ContainsKey(argumentName))
				return defaultValue;
			var value = _arguments[_argNameLookup[argumentName]];
			return value is T ? (T)value : defaultValue;
		}

		/// <summary>
		/// Retrieve the value of the given parameter. Aliases will be translated.
		/// </summary>
		/// <param name="argumentName">The name of the argument to retreive the value of</param>
		/// <returns>The value object of the given argument if it exists. Otherwise, returns null</returns>
		public object GetValue(string argumentName) => !_argNameLookup.ContainsKey(argumentName) ? null : _arguments[_argNameLookup[argumentName]];

		/// <summary>
		/// Parses the given argv array according the the arguments registered
		/// </summary>
		/// <param name="argv">A command-line argv</param>
		public void Parse(params string[] argv) {
			var currentArg = string.Empty;
			var currentRaw = string.Empty;
			var currentBuilder = new List<string>();
			string[] value;

			foreach (var s in argv) {
				if (_preFixes.Any(o => s.StartsWith(o))) {
					if (!_argNameLookup.ContainsKey(s))
						throw new UnknownArgumentException("argument not recognized", s);

					var argname = _argNameLookup[s];

					if (_arguments.ContainsKey(argname))
						throw new ArgumentAlreadyEncounteredException("argument encountered twice", string.Join(", ", _aliasLookup[argname]));

					value = currentBuilder.ToArray();
					_rawArguments.Add(currentRaw, value);
					ParseArgument(currentArg, value);
					currentArg = argname;
					currentRaw = s;
					currentBuilder.Clear();
					continue;
				}

				currentBuilder.Add(s);
			}

			value = currentBuilder.ToArray();
			_rawArguments.Add(currentRaw, value);
			ParseArgument(currentArg, value);
		}

		private void ParseArgument(string argName, string[] values) {
			try {
				_arguments.Add(argName, _validationFuncs[argName].Invoke(values));
			} catch (Exception e) {
				throw new ArgumentValidationException(e.Message, string.Join(", ", _aliasLookup[argName]), e);
			}
		}

		#endregion Methods
	}
}
