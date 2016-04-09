using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleArgv {

	public class CommandLine {
		#region Fields

		private readonly string[] _preFixes;
		private readonly Dictionary<string, string> _paramNameLookup;
		private readonly Dictionary<string, string[]> _aliasLookup;
		private readonly Dictionary<string, Func<string[], object>> _validationFuncs;
		private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
		private readonly Dictionary<string, string[]> _rawParameters = new Dictionary<string, string[]>();

		#endregion Fields

		#region Properties

		public IReadOnlyDictionary<string, string[]> RawParameters => _rawParameters;

		#endregion Properties

		#region Constructors

		public CommandLine(string[] preFixes, StringComparer argumentNameCompararer = null) {
			_preFixes = preFixes.OrderByDescending(o => o.Length).ToArray();
			var stringComparer = argumentNameCompararer ?? StringComparer.OrdinalIgnoreCase;
			_validationFuncs = new Dictionary<string, Func<string[], object>>(stringComparer);
			_paramNameLookup = new Dictionary<string, string>(stringComparer);
			_aliasLookup = new Dictionary<string, string[]>(stringComparer);
		}

		#endregion Constructors

		#region Methods

		public void AddArgument(Func<string[], object> parser, params string[] aliases) {
			var primary = aliases[0];

			foreach (var alias in aliases) {
				if (_paramNameLookup.ContainsKey(alias))
					throw new ExistingAliasException("argument already exists", alias);
				_paramNameLookup.Add(alias, primary);
			}

			_aliasLookup.Add(primary, aliases);

			_validationFuncs.Add(primary, parser);
		}

		public T GetValue<T>(string parameterName, T defaultValue) {
			if (!_paramNameLookup.ContainsKey(parameterName))
				return defaultValue;
			var value = _parameters[_paramNameLookup[parameterName]];
			return value is T ? (T)value : defaultValue;
		}

		public object GetValue(string parameterName) => !_paramNameLookup.ContainsKey(parameterName) ? null : _parameters[_paramNameLookup[parameterName]];

		public void Parse(params string[] argv) {
			var currentArg = string.Empty;
			var currentRaw = string.Empty;
			var currentBuilder = new List<string>();
			string[] value;

			foreach (var s in argv) {
				if (_preFixes.Any(o => s.StartsWith(o))) {
					if (!_paramNameLookup.ContainsKey(s))
						throw new UnknownArgumentException("argument not recognized", s);

					var argname = _paramNameLookup[s];

					if (_parameters.ContainsKey(argname))
						throw new ArgumentAlreadyEncounteredException("argument encountered twice", string.Join(", ", _aliasLookup[argname]));

					value = currentBuilder.ToArray();
					_rawParameters.Add(currentRaw, value);
					ParseArgument(currentArg, value);
					currentArg = argname;
					currentRaw = s;
					currentBuilder.Clear();
					continue;
				}

				currentBuilder.Add(s);
			}

			value = currentBuilder.ToArray();
			_rawParameters.Add(currentRaw, value);
			ParseArgument(currentArg, value);
		}

		private void ParseArgument(string argName, string[] values) {
			try {
				_parameters.Add(argName, _validationFuncs[argName].Invoke(values));
			} catch (Exception e) {
				throw new ArgumentValidationException($"{e.Message}", string.Join(", ", _aliasLookup[argName]), e);
			}
		}

		#endregion Methods
	}
}
