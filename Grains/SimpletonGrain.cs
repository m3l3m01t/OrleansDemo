using System.Threading.Tasks;
using GrainInterfaces;
using Orleans.Concurrency;

namespace Grains
{
  [StatelessWorker]
  public class SimpletonGrain : Orleans.Grain, ISimpleton
  {
    public Task<string> Greeting(string words)
    {
      return Task.FromResult("Hmm?");
    }
  }
}