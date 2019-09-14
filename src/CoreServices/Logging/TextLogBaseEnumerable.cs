using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StandardDot.Abstract.CoreServices;
using StandardDot.Dto.CoreServices;

namespace StandardDot.CoreServices.Logging
{
	/// <summary>
	/// An Enumerable to get text logs
	/// </summary>
	public class TextLogBaseEnumerable : LogBaseEnumerableBase
	{
		/// <param name="source">The source that the enumerable should represent</param>
		public TextLogBaseEnumerable(IEnumerable<LogBase> source)
			: base(source)
		{ }

		/// <param name="source">The source that the enumerable should represent</param>
		public TextLogBaseEnumerable(ILogBaseEnumerable source)
			: base(source)
		{ }

		/// <param name="path">The directory logs should be stored in (should end in /)</param>
		/// <param name="serializationService">The serialization service to use</param>
		/// <param name="onlySerializeLogsOfTheCorrectType">Only serializes logs of the correct type, has a significant performance hit</param>
		public TextLogBaseEnumerable(string path, ISerializationService serializationService, bool onlySerializeLogsOfTheCorrectType = false)
			: base(new TextLogEnumerable<object>(path, serializationService, onlySerializeLogsOfTheCorrectType))
		{ }

		public override IEnumerator<LogBase> GetEnumerator()
		{
			return Source.GetEnumerator();
		}
	}
}