using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using YounesCo_Backend.Data;

namespace YounesCo_Backend.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [StringLength(450, ErrorMessage = "Maximum length for customer id is {1}")]
        public string CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public AppUser Customer { get; set; }

        [StringLength(256, ErrorMessage = "Maximum length for title is {1}")]
        public string Status { get; set; } = "Sent";

        [Required]
        [StringLength(256, ErrorMessage = "Maximum length for title is {1}")]
        public string Payment { get; set; }

        [Range(0, 1000000000)]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        public bool Read { get; set; } = false;

        public bool Deleted { get; set; } = false;

        public DateTime Date { get; set; } = DateTime.Now;

        
    }
}
