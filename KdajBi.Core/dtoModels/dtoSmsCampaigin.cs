using KdajBi.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KdajBi.Core.dtoModels
{
    public class dtoSmsCampaigin
    {
        public int CampaignType { get; set; } //0-individual mobiles, 1-individual locations, 2-sex
        public string MsgTxt { get; set; }
        public ICollection<string> Recipients { get; set; } //could be mobiles, locationid or sex

        public DateTime SendAfter { get; set; }


        public dtoSmsCampaigin()
        {
            Recipients = new List<string>();
        }
    }
}
