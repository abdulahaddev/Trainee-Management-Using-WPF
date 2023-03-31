using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TMS.Models
{
    public class Trainee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Course { get; set; }
        public bool Status { get; set; }
        public string ImageTitle { get; set; }

        [JsonIgnore]
        public ImageSource ImageSrc { get; set; }
    }

}
