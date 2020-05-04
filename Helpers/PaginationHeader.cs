namespace YounesCo_Backend.Helpers
{
    public class PaginationHeader
    {
        public int CurrentPage { get; set; }

        public int PageSize { get; set; }
        public int NumberOfPages { get; set; }
        public int TotalItems { get; set; }

        public PaginationHeader(int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            this.CurrentPage = currentPage;
            this.PageSize = itemsPerPage;
            this.TotalItems = totalItems;
            this.NumberOfPages = totalPages;
        }
    }
}