using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KdajBi.Web.ViewModels
{
    public class vmLocationsCombo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }

        public vmLocationsCombo(long p_Id, string p_Name, bool p_Selected)
        { Id = p_Id; Name = p_Name; Selected = p_Selected; }
    }
}
