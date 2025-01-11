namespace WebEngineering.Storey
{
    public class Storey
    {
        public Guid? id { get; set; }
        public string name { get; set; }
        public Guid building_id { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}
