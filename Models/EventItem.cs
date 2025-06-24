using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EventManagerWebRazorPage.Models
{
    [PrimaryKey(nameof(ItemId))]
    public class EventItem
    {
        public string ItemId { get; set; } = string.Empty;
        public string? ItemName { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }
        public int Amount { get; set; }
        public string EventId { get; set; } = string.Empty;
        public EventDetail? EventDetail { get; set; }
        public ICollection<EventItemOption>? EventItemOption { get; set; }
    }
    public class EventItemValidator : AbstractValidator<EventItem>
    {
        public EventItemValidator()
        {
            RuleFor(x => x.ItemName).NotNull().MaximumLength(50);
            RuleFor(x=> x.Description).MaximumLength(200);
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(1);
        }
    }
}
