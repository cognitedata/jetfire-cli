using System;
namespace Cognite.Jetfire.Api.Model
{
    public class NotificationRead
    {
        public string Destination { get; set; }
        public int Id { get; set; }
        public int ConfigId { get; set; }
    }

    public class NotificationCreate
    {
        public string Destination { get; set; }

        public NotificationCreate(string destination)
        {
            Destination = destination;
        }
    }
}
