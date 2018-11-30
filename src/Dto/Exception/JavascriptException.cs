using System.Collections;
using System.Collections.Generic;

namespace StandardDot.Dto.Exception
{
	public class JavascriptException : System.Exception
	{
		public JavascriptException()
			: base()
		{}

		public JavascriptException(string message)
			: base(message)
		{}

		public JavascriptException(string message, System.Exception innerException)
			: base(message, innerException)
		{}

		public JavascriptException(SerializableJavaScriptError error, System.Exception innerException = null)
			: this("Javascript Error - " + error.Name + " - " + error.Message, innerException)
		{
			_error = error;
		}

		private SerializableJavaScriptError _error;

		public override string StackTrace => _error == null
			? base.StackTrace
			: _error.Stack;

		public override string Source => _error == null
			? base.Source
			: _error.ErrorSource;

		public override IDictionary Data => _error == null
			? base.Data
			: new Dictionary<string, string>
				{
					{"FileName", _error.FileName},
					{"LineNumber", _error.LineNumber?.ToString()},
					{"ColumnNumber", _error.ColumnNumber?.ToString()},
					{"Description", _error.Description},
					{"Number", _error.Number?.ToString()},
					{"ToStringResult", _error.ToStringResult},
					{"HasMozilla", _error.HasMozilla.ToString()},
					{"HasMicrosoft", _error.HasMicrosoft.ToString()},
					{"Notes", _error.Notes}
				};
	}
}