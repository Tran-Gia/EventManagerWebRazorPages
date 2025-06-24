using EventManagerWebRazorPage.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EventManagerWebRazorPage.ViewModels
{
    public class EventDetailViewModel
    {
        public EventDetail EventDetail { get; set; } = new EventDetail();

        [DataType(DataType.Upload)]
        [BindProperty]
        public IFormFile? Image { get; set; }
        public List<EventItemImagesViewModel>? EventItemImagesViewModels { get; set; } = [];
    }
    public class EventItemImagesViewModel
    {
        public int EventItemIndex { get; set; }
        public IFormFile? EventItemImage { get; set; }
    }
    public class EventDetailViewModelValidator : AbstractValidator<EventDetailViewModel>
    {
        public EventDetailViewModelValidator()
        {
            RuleFor(x => x.EventDetail).SetValidator(new EventDetailValidator());
            RuleFor(x => x.Image).SetValidator(new ImageValidator()!).When(x => x.Image != null);
            RuleForEach(x => x.EventItemImagesViewModels).ChildRules(eventItem =>
            {
                eventItem.RuleFor(x => x.EventItemImage).SetValidator(new ImageValidator()!).When(x => x.EventItemImage != null);   
            }).When(x => x.EventItemImagesViewModels != null && x.EventItemImagesViewModels.Count > 0);
            RuleForEach(x => x.EventDetail.EventItem).SetValidator(new EventItemValidator()).OverridePropertyName("eventItems");
        } 
    }
    public class ImageValidator : AbstractValidator<IFormFile>
    {
        public ImageValidator()
        {
            RuleFor(x => x.Length).NotNull().GreaterThan(0).WithMessage("File size is zero");
            RuleFor(x => x.ContentType).NotNull().Must(x => x.Equals("image/jpeg") || x.Equals("image/jpg") || x.Equals("image/png")).WithMessage("Invalid File type");
        }
    }
}
