namespace ASP121.Data.Entity
{
    public class Rate
    {
        public Guid     ProductID { get; set; }
        public Guid     UserID    { get; set; }
        public int      Rating    { get; set; }
        public DateTime Moment    { get; set; }

        // Navigation properties
        public Product Product { get; set; } = null!;
    }                             
}
