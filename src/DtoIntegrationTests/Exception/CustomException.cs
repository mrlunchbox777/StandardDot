
using System.Collections;
using System.Reflection;

namespace StandardDot.Dto.IntegrationTests.Exception
{
	public class CustomException : System.Exception
	{
		public CustomException()
			: base()
		{ }

		public CustomException(int hResult, string stackTrace,
			string message, IDictionary data, CustomException innerException)
			: base(message, innerException)
		{
			HResult = hResult;
			StackTrace = stackTrace;
			Message = message;
			Data = data;
		}

		public override string Source { get; set; }

		public override string HelpLink { get; set; }

		public override string StackTrace { get; }

		public override string Message { get; }

		public override IDictionary Data { get; }
	}
}