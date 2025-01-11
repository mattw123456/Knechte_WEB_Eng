namespace WebEngineering.Status
{
    public interface IStatusService
    {
        public StatusInformation GebeStatusInformationenZurueck();
        public StatusInformationHealth GebeHealthInformationenZurueck();
    }

 
}
