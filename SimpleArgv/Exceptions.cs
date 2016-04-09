using System;

namespace SimpleArgv {
	public class CommandLineArgumentException : Exception {
		public string ParameterName { get; }
		public CommandLineArgumentException(string message, string parameterName, Exception innerException = null) : base(message, innerException) { ParameterName = parameterName; }
	}

	public class UnknownArgumentException : CommandLineArgumentException {
		public UnknownArgumentException(string message, string parameterName) : base(message, parameterName) { }
	}
	
	public class ExistingAliasException : CommandLineArgumentException {
		public ExistingAliasException(string message, string parameterName) : base(message, parameterName) {}
	}

	public class ArgumentAlreadyEncounteredException : CommandLineArgumentException {
		public ArgumentAlreadyEncounteredException(string message, string parameterName) : base(message, parameterName) { }
	}

	public class ArgumentValidationException : CommandLineArgumentException {
		public ArgumentValidationException(string message, string parameterName, Exception innerException = null) : base(message, parameterName, innerException) { }
	}
}