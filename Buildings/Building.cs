namespace WebEngineering.Buildings
{
    public class Building
    {
        // Optionales Guid-Feld, da es von der DB generiert wird
        public Guid? id { get; set; }

        // Die übrigen Felder sind erforderlich
        public required string name { get; set; }
        public required string streetname { get; set; }
        public required string housenumber { get; set; }
        public required string country_code { get; set; }
        public required string postalcode { get; set; }
        public required string city { get; set; }

        // Optionales DateTime-Feld, da es nicht immer gesetzt wird
        public DateTime? deleted_at { get; set; }
    }
}

