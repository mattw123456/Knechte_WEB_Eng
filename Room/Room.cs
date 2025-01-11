namespace WebEngineering.Room
{
    public class Room
    {
        public Guid? id { get; set; }
        public string name { get; set; }
        public Guid storey_id { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}