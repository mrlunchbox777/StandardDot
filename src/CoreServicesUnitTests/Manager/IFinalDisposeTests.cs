using Moq;
using StandardDot.CoreServices.Manager;
using Xunit;

namespace StandardDot.CoreServices.UnitTests.Manager
{
	public class IFinalDisposeTests
	{
		[Fact]
		public void InterfaceValidation()
		{
			Mock<IFinalDispose> mFinalDispose = new Mock<IFinalDispose>();
			mFinalDispose.Setup(x => x.Open()).Verifiable();
			mFinalDispose.Setup(x => x.Close()).Verifiable();

			using(IFinalDispose finalDispose = mFinalDispose.Object)
			{
				finalDispose.Open();
				finalDispose.Close();
			}

			mFinalDispose.Verify();
		}
	}
}