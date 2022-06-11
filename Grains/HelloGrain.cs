using GrainInterfaces;

using Orleans.Runtime;
using Microsoft.Extensions.Logging;

using System.Threading.Tasks;
using Grains.Models;

namespace Grains
{
  public class HelloGrain : Orleans.Grain, IHello
  {
    private readonly ILogger<HelloGrain> _logger;
    private readonly IPersistentState<GreetingState> _state;

    public HelloGrain(ILogger<HelloGrain> logger, [PersistentState("state", "postgre")] IPersistentState<Models.GreetingState> state)
    {
      _logger = logger;
      _state = state;
    }
    public async Task<string> SayHello(string greeting)
    {
      var id = IdentityString;

      var state = _state.State;
      state.Count += 1;
      state.LastReceived = greeting;

      await _state.WriteStateAsync();

      _logger.LogInformation($"\n SayHello message received: greeting = '{greeting}'");
      return $"\n Client said: '{greeting}' from '{id} - {state.Count}";
    }
  }
}
