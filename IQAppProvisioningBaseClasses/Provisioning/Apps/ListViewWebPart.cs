namespace IQAppProvisioningBaseClasses.Provisioning
{
    public class ListViewWebPart
    {
        public ListViewWebPart()
        {
        }

        public ListViewWebPart(string title, string zoneId, int order, string listName, bool isCalendar)
        {
            Title = title;
            ZoneId = zoneId;
            Order = order;
            ListName = listName;
            IsCalendar = isCalendar;
        }

        public string Title { get; set; }
        public string ZoneId { get; set; }
        public int Order { get; set; }
        public string ListName { get; set; }
        public bool IsCalendar { get; set; }
    }
}