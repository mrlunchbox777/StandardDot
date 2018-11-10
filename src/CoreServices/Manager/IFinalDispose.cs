using System;

namespace StandardDot.CoreServices.Manager
{
	public interface IFinalDispose : IDisposable
	{
		void Close();

		void Open();
	}
}