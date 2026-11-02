namespace ProductGrpcService.Data
{
    public static class CategoryData
    {
        public static List<CategoryResponse> GetMockCategoryList()
        {
            return new List<CategoryResponse>
            {
                new CategoryResponse
                {
                    Id = 1,
                    Name = "Electronics",
                    Description = "Electronic products"
                },
                new CategoryResponse
                {
                    Id = 2,
                    Name = "Accessories",
                    Description = "Computer accessories"
                },
                new CategoryResponse
                {
                    Id = 3,
                    Name = "Monitors",
                    Description = "Computer monitors and displays"
                }
            };
        }
    }
}