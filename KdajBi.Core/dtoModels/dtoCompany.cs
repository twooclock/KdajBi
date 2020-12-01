using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KdajBi.Core.dtoModels
{
    public class dtoCompany
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public dtoCompany() { }
        public dtoCompany(long p_id, string p_name)
        { Id = p_id; Name = p_name; }
    }
}
