namespace ProductGrpcService.Model
{
    public class ProductModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int CategoryId { get; set; }
        public int InventoryId { get; set; }
    }
    public class ProductAddModel : ProductModel
    {

    }
    public class ProductViewModel : ProductModel
    {

    }
    public class ProductEditModel : ProductListModel
    {

    }
    public class ProductListModel : ProductViewModel
    {
        public int Id { get; set; }
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
    }

}