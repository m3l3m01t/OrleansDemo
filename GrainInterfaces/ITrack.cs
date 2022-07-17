using System.Threading.Tasks;

namespace GrainInterfaces
{
    public interface ITrack : Orleans.IGrainWithIntegerKey
    {
        Task Record(System.DateTime timeStamp, double longitude, double latitude, double altitude);
    }
}