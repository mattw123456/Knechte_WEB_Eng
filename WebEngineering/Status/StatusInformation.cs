namespace WebEngineering.Status
{
    public class StatusInformation
    {
        public required List<string> authors { get; set; }
        public required List<string> supportedApis { get; set; }
    }
    public class StatusInformationHealth
    {
        public bool live { get; set; }
        public bool ready { get; set; }
        public object databases { get; set; }

    }
}
