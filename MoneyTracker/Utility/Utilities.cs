using System.Drawing.Printing;
using System.Net.NetworkInformation;

namespace MoneyTracker.Utility
{
    public static class Utilities
    {
        public static List<int> getPaginationList(int currentPage, int pageCount)
        {
            List<int> pages = new List<int>();

            int page;
            if (currentPage == pageCount)
            {
                page = currentPage - 4;
            }
            else if(currentPage == pageCount - 1)
            {
                page = currentPage - 3;
            }
            else
            {
                page = currentPage - 2;
            }

            if (page < 1) page = 1;
            
            int counter = 0;
            do
            {
                pages.Add(page);
                page++;
                counter++;
            } while (counter < 5 && page <= pageCount );

            return pages;
        }
    }
}
