using AbacusViewer.Models;

namespace AbacusViewer.Services
{
    public interface IEMSServiceProvider
    {
        EventHierarchy GetEventMarketSelections(long eventId, string baseUrl);
    }
}
