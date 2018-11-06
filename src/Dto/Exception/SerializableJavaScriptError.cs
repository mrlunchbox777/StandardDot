using System.Runtime.Serialization;

namespace StandardDot.Dto.Exception
{
	/// <summary>
	/// A class that can be used to serialize any exception. Validity of serialization is valued over complete data.
	/// </summary>
	[DataContract]
	public class SerializableJavaScriptError
	{
		public SerializableJavaScriptError() { }
		
		public SerializableJavaScriptError(string message, string name
			// Mozilla
			, string fileName = null, int? lineNumber = null, int? columnNumber = null, string stack = null
			// Microsoft
			, string description = null, int? number = null
			// methods
			, string errorSource = null, string toStringResult = null)
		{
			Message = message;
			Name = name;
			FileName = fileName;
			LineNumber = lineNumber;
			ColumnNumber = columnNumber;
			Stack = stack;
			Description = description;
			Number = number;
			ErrorSource = errorSource;
			ToStringResult = toStringResult;
		}

		[DataMember(Name = "message")]
		public string Message { get; set; }

		[DataMember(Name = "name")]
		public string Name { get; set; }

		// Mozilla
		[DataMember(Name = "fileName")]
		public string FileName { get; set; }

		[DataMember(Name = "lineNumber")]
		public int? LineNumber { get; set; }
		
		[DataMember(Name = "columnNumber")]
		public int? ColumnNumber { get; set; }
		
		[DataMember(Name = "stack")]
		public string Stack { get; set; }
		
		// Microsoft
		[DataMember(Name = "description")]
		public string Description { get; set; }
		
		[DataMember(Name = "number")]
		public int? Number { get; set; }

		// Methods
		[DataMember(Name = "errorSource")]
		public string ErrorSource { get; set; }

		[DataMember(Name = "toStringResult")]
		public string ToStringResult { get; set; }

		// Checks
		[DataMember(Name = "hasMozilla")]
		public bool HasMozilla
		{
			get => !string.IsNullOrWhiteSpace(FileName) || LineNumber != null || ColumnNumber != null || !string.IsNullOrWhiteSpace(Stack);
			set {}
		}
		
		[DataMember(Name = "hasMicrosoft")]
		public bool HasMicrosoft
		{
			get => !string.IsNullOrWhiteSpace(Description) || Number != null;
			set {}
		}
		
		[DataMember(Name = "hasMethodValues")]
		public bool HasMethodValues
		{
			get => !string.IsNullOrWhiteSpace(ErrorSource) || !string.IsNullOrWhiteSpace(ToStringResult);
			set {}
		}
        
    }
}