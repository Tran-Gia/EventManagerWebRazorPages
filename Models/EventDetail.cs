using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EventManagerWebRazorPage.Models
{
    [PrimaryKey(nameof(EventId))]
    public class EventDetail
    {
        public string EventId { get; set; } = string.Empty;
        public string? UserId { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? MainImage { get; set; }
        public bool IsHidden { get; set; } = false;
        [DisplayName("Event Time")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime EventTime { get; set; }
        [DisplayName("From")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime RegistrationStartDate { get; set; }
        [DisplayName("To")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime RegistrationEndDate { get; set; }
        public ICollection<EventItem> EventItem { get; set; } = [];
    }
    public class EventDetailValidator : AbstractValidator<EventDetail>
    {
        public EventDetailValidator()
        {
            RuleFor(x => x.Title).NotNull().MinimumLength(5).MaximumLength(200);
            RuleFor(x => x.Message).NotNull().MinimumLength(10).MaximumLength(2555);
            RuleSet("EditableRegistrationStartDate", () =>
            {
                RuleFor(x => x.RegistrationStartDate).NotNull()
                    .GreaterThanOrEqualTo(DateTime.Now).WithMessage("Registration Start Date cannot be set to the past");
            });
            RuleFor(x => x.RegistrationEndDate).NotNull()
                .GreaterThanOrEqualTo(x => x.RegistrationStartDate).WithMessage("Registration End Date must be greater than or equal to Registration Start Date");
            RuleFor(x => x.EventTime).NotNull()
                .GreaterThan(x => x.RegistrationEndDate).WithMessage("Event Time must be greater than Registration End Date");
            RuleFor(x => x.EventItem).NotNull().WithMessage("There must be at least 1 Event Item for participants to choose from");
            RuleForEach(x => x.EventItem).SetValidator(new EventItemValidator());
        }
    }
}
