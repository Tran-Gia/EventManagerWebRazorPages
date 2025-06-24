using EventManagerWebRazorPage.Models;

namespace EventManagerWebRazorPage.ViewModels
{
    public class EventWithDetailedEventItemsViewModel
    {
        public EventDetail EventDetail { get; set; } = new EventDetail();
        public string? CreatedBy { get; set; } = string.Empty;
        public List<EventItemViewModel> EventItemViewModels { get; set; } = [];
    }
}
