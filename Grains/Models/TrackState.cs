using System;

namespace Grains.Models
{
    public class TrackEvent
    {
        public DateTime Timestamp { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Altitude { get; set; }
    }
    
    public class TrackState
    {
        public string Id { get; set; }
        public DateTime Timestamp { get; private set; }
        public double Longitude { get; private set; }
        public double Latitude { get; private set; }
        public double Altitude { get; private set; }
        
        public long Changes { get; set; }

        public void Apply(TrackEvent @event)
        {
            Timestamp = @event.Timestamp;
            Longitude = @event.Longitude;
            Latitude = @event.Latitude;
            Altitude = @event.Altitude;

            Changes++;
        }
    }
}