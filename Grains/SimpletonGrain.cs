using System;
using System.Threading.Tasks;
using GrainInterfaces;
using Orleans;
using Orleans.Placement;

namespace Grains
{
  [ActivationCountBasedPlacement]
  //[StatelessWorker]
  public class SimpletonGrain : Grain, ISimpleton
  {
    private static Random _random = new Random();
    
    public async Task<string> Greeting(string words)
    {
      var key = this.GetPrimaryKeyLong();
      var grain = GrainFactory.GetGrain<ITrack>(key);


      var p1 = _random.Next(1, 9999) / 1000.0;
      var p2 = _random.Next(1, 9999) / 1000.0;
      var p3 = _random.Next(1, 9999) / 1000.0;
      
      await grain.Record(DateTime.Now, p1, p2, p3);
      
      return "Hmm?";
    }
  }
}
