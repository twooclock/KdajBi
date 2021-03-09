using KdajBi.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KdajBi.Core.Models
{

    public class SmsCampaign
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("UserID"), Required]
        public virtual AppUser AppUser { get; set; }

        [ForeignKey("CompanyId"), Required]
        public virtual Company Company { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Guid { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? Date { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(640), Required]
        public string MsgTxt { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? SendAfter { get; set; }


        public DateTime? ApprovedAt { get; set; }
        public DateTime? CanceledAt { get; set; }
        public DateTime? SentAt { get; set; }

        public ICollection<SmsMsg> Recipients { get; set; }

        public long RecipientsCount { get { return Recipients.Count(); } private set { } }

        public long SentOk { get; set; }
        public long SentError { get; set; }

        public SmsCampaign() {
            Company = new Company();
            AppUser = new AppUser();
            Recipients = new List<SmsMsg>();
        }
        public SmsCampaign(string p_Name, string p_MsgTxt) {
            Name = p_Name;
            MsgTxt = p_MsgTxt;
            Recipients = new List<SmsMsg>();
        }
    }

    public class SmsMsg 
    {
        public long Id { get; set; }

        [MaxLength(15),Required]
        public string Recipient { get; set; }

        [MaxLength(100)]
        public string ApiResponse { get; set; }



        public SmsMsg() { }
        public SmsMsg(string p_Recipient) {
            Recipient = p_Recipient;
        }

    }
}
