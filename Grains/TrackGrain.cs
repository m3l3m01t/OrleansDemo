using System;
using System.Threading.Tasks;
using System.Xml;
using GrainInterfaces;
using Grains.Models;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.EventSourcing;
using Orleans.Providers;

namespace Grains
{
    //[StorageProvider(ProviderName = "postgre")]
    [LogConsistencyProvider()]
    public class TrackGrain : JournaledGrain<TrackState, TrackEvent>, ITrack
    {
        private readonly ILogger<TrackGrain> _logger;

        public TrackGrain(ILogger<TrackGrain> logger)
        {
            _logger = logger;
        }
        
        public Task Record(DateTime timeStamp, double longitude, double latitude, double altitude)
        {
            
            RaiseEvent(new TrackEvent
            {
                Timestamp = timeStamp, 
                Longitude = longitude,
                Latitude = latitude,
                Altitude = altitude
            });

            return ConfirmEvents();
        }

        protected override void OnTentativeStateChanged()
        {
            base.OnTentativeStateChanged();
        }

        protected override void OnStateChanged()
        {
            base.OnStateChanged();
            
            _logger.LogInformation("StateChanged {0}", State);
            _logger.LogInformation("StateChanged {0} - {1}:( {2}, {3}, {4})",
                State.Id, State.Timestamp, State.Longitude, State.Latitude, State.Altitude);
        }

        public override Task OnActivateAsync()
        {
            return base.OnActivateAsync();
        }

        protected override void TransitionState(TrackState state, TrackEvent @event)
        {
            _logger.LogInformation("TransitionState {0} - {1}:( {2}, {3}, {4}); Changes: {5}",
                state.Id, state.Timestamp, state.Longitude, state.Latitude, state.Altitude, state.Changes);

            state.Id = IdentityString;
            
            base.TransitionState(state, @event);
        }

        public override Task OnDeactivateAsync()
        {
            return base.OnDeactivateAsync();
        }
    }
}
