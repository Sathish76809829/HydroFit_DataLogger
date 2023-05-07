using System.Threading;
using System.Threading.Tasks;

namespace RMS.DataParser.Services
{
    public interface IDataProcessService
    {
        void Dispose();
        Task DoWork(string topic, CancellationToken stoppingToken);
    }
}