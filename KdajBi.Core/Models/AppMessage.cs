using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace KdajBi.Core.Models
{

    public class AppMessage : BaseModel
    {
        public long Id { get; set; }
        public DateTime? MessageDate { get; set; }
        [MaxLength(50)]
        public string Source { get; set; }

        public long ToCompanyId { get; set; }

        [MaxLength(255)]
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool ForAdminOnly { get; set; }

    }

    public class UserAppMessage : BaseModel
    {
        public long Id { get; set; }
        [ForeignKey("AppMessage"), Required]
        public long AppMessageId { get; set; }
        public AppMessage AppMessage { get; set; }

        [ForeignKey("AppUser")]
        public long UserId { get; set; }
        public bool? Read { get; set; } 

        public DateTime? DateRead { get; set; }

    }
}
