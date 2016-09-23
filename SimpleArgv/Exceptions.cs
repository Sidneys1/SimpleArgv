using System;

namespace SimpleArgv {
	/// <summary>
	/// Abstract base class for CommandLine exceptions related to Arguments
	/// </summary>
	public abstract class CommandLineArgumentException : Exception {
		public string ParameterName { get; }
		protected CommandLineArgumentException(string message, string parameterName, Exception innerException = null) : base(message, innerException) { ParameterName = parameterName; }

	    public override string ToString() => $"Failed to parse parameter {ParameterName}: {Message}";
	}

	/// <summary>
	/// An unregistered argument has been encountered
	/// </summary>
	public class UnknownArgumentException : CommandLineArgumentException {
		internal UnknownArgumentException(string message, string parameterName) : base(message, parameterName) { }
	}
	
	/// <summary>
	/// Argument registration was attempted using an alias that is already registered
	/// </summary>
	public class ExistingArgumentAliasException : CommandLineArgumentException {
		internal ExistingArgumentAliasException(string message, string parameterName) : base(message, parameterName) {}
	}

	/// <summary>
	/// A registered argument (or its aliases) has been encountered twice
	/// </summary>
	public class ArgumentAlreadyEncounteredException : CommandLineArgumentException {
		internal ArgumentAlreadyEncounteredException(string message, string parameterName) : base(message, parameterName) { }
	}

	/// <summary>
	/// An execption has occurred during argument validation
	/// </summary>
	public class ArgumentValidationException : CommandLineArgumentException {
		internal ArgumentValidationException(string message, string parameterName, Exception innerException = null) : base(message, parameterName, innerException) { }
	}
}