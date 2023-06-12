using System.Data;

namespace ASP121.Data.Entity
{
    public class ProductGroup
    {
        public Guid      ID          { get; set; }
        public String    Title       { get; set; } = null!;
        public String?   Description { get; set; }
        public DateTime? DeleteDt    { get; set; }
    }
}
