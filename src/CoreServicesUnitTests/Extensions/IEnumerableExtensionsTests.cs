using StandardDot.CoreServices.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace StandardDot.CoreServices.Extensions.UnitTests
{
    public class IEnumerableExtensionsTests
    {
        [Fact]
        public void PaginateOldSchoolTest()
        {
            int regularPageSize = 10;
            List<int> list = GetCollection(regularPageSize);
            int pages = list.Count / 10;
            int lastPageSize = list.Count % 10;
            Assert.NotEqual(0, lastPageSize);

            int pageCount = 0;
            List<int> currentPage;
            bool gotAnIncompletePage = false;
            
            for (; pageCount <= pages; pageCount++)
            {
                currentPage = list.Paginate(regularPageSize).GetPage(pageCount).ToList();
                if (pageCount == pages)
                {
                    gotAnIncompletePage = true;
                    Assert.Equal(lastPageSize, currentPage.Count);
                }
                for (int i = 0; i < currentPage.Count; i++)
                {
                    Assert.Equal(list[i + (pageCount * regularPageSize)], currentPage[i]);
                }
            }
            Assert.True(gotAnIncompletePage);
            Assert.Equal(pages, pageCount - 1);
            Assert.Empty(list.Paginate(regularPageSize).GetPage(++pageCount));
        }

        private Random _random = new Random();

        private List<int> GetCollection(int pageSize, bool getIncompletePage = true)
        {
            List<int> result = new List<int>();

            int items = _random.Next(50, 100);
            while (items % pageSize == 0)
            {
                items = _random.Next(50, 100);
            }
            for (int i = 0; i < items; i++)
            {
                result.Add(_random.Next(0,100));
            }
            return result;
        }
    }
}