using System.Threading.Tasks;

namespace GrainInterfaces
{
  public interface ISimpleton: Orleans.IGrainWithIntegerKey
  {
    Task<string> Greeting(string words);
  }
}