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

        [ForeignKey("AppUser"), Required]
        public int UserId { get; set; }
        public virtual AppUser AppUser { get; set; }

        [ForeignKey("Company"), Required]
        public long CompanyId { get; set; }
        public virtual Company Company { get; set; }

        [ForeignKey("Location")]
        public long? LocationId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Guid { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? Date { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(640), Required]
        public string MsgTxt { get; set; }

        public int MsgSegments { get; set; }

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

        [ForeignKey("SmsCampaign"), Required]
        public long SmsCampaignId { get; set; }

        [MaxLength(15),Required]
        public string Recipient { get; set; }

        public long? ClientId { get; set; }
        public virtual Client Client { get; set; }

        [MaxLength(100)]
        public string ApiResponse { get; set; }



        public SmsMsg() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p_Recipient">Recipient mobile number</param>
        /// <param name="p_ClientId">Recipient clientid</param>
        public SmsMsg(string p_Recipient, long p_ClientId)
        {
            Recipient = p_Recipient; ClientId = p_ClientId;
        }

    }
}
