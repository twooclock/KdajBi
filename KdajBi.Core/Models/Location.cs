using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace KdajBi.Core.Models
{
    //could be salon, dental office, etc.
    public class Location : BaseModel
    {
        public long Id { get; set; }

        [ForeignKey("Company"), Required]
        public long CompanyId { get; set; }
        

        [MinLength(3), MaxLength(150), Required]
        public string Name { get; set; }

        [MinLength(9), MaxLength(15)]
        public string Tel { get; set; }

        [ForeignKey("Schedule"), Required]
        public long ScheduleId { get; set; }
        public virtual Schedule Schedule { get; set; }

        public ICollection<Workplace> Workplaces { get; set; }

        public Location() { Active = true; Schedule = new Schedule(); Workplaces = new List<Workplace>(); }

        [MinLength(3), MaxLength(25)]
        public string PublicBookingToken { get; set; }

        [ MaxLength(150)]
        public string Address { get; set; }

        [MaxLength(150)]
        public string Timetable { get; set; }
    }
}
