using System;
using System.ComponentModel.DataAnnotations;

namespace PsnHackathonBot.Models
{
    public class School
    {
        [Required]
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public ulong VerificationId { get; set; }
    }
}