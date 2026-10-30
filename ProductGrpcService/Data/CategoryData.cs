using CategoryGrpcService;

namespace ProductGrpcService.Data
{
    public  class CategoryData
    {
        public static List<CategoryResponse> GetMockCategoryList()
        {
            return new List<CategoryResponse>
            {

                new CategoryResponse
                {
                    Id = 1,
                    Name = "Accessories",
                    Note = "PC Hardware & Peripherals"
                },
                new CategoryResponse
                {
                    Id = 2,
                    Name = "Displays",
                    Note = "Monitors & Screens"
                },
                new CategoryResponse
                {
                    Id = 3,
                    Name = "Audio",
                    Note = "Headsets & Speakers"
                },
                new CategoryResponse
                {
                    Id = 4,
                    Name = "Cables & Hubs",
                    Note = "Connectivity"
                }
            };
        }
    }
}
