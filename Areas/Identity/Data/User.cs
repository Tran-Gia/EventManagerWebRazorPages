using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EventManagerWebRazorPage.Areas.Identity.Data
{
    public class User : IdentityUser
    {
        [Required]
        public string UserInternalId { get; set; } = string.Empty;
        [DefaultValue(70)]
        [Range(0,100,ErrorMessage = "Please enter a value between {0} and {1}")]
        public int CreditScore { get; set; }
        [DefaultValue(0)]
        public int ConsecutiveCheckIn { get; set; }
    }
}
