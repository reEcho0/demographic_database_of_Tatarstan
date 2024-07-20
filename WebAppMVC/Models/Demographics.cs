using System.ComponentModel.DataAnnotations;

namespace WebAppMVC.Models
{
    public class Demographics
    {
        [Key]
        public int Year { get; set; }
        public int Born { get; set; }
        public int Died { get; set; }
        public int Arrival { get; set; }
        public int Departure { get; set; }

    }

    
}
