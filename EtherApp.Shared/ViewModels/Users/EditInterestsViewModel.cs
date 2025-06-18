using EtherApp.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace EtherApp.Shared.ViewModels.Users
{
    public class EditInterestsViewModel
    {
        public List<Interest> AllInterests { get; set; }

        [Required(ErrorMessage = "Please select at least one interest")]
        [MinLength(1, ErrorMessage = "You must select at least one interest")]
        public List<int> SelectedInterestIds { get; set; }
    }
}
