﻿using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class InsertSignatureVM
    {
        [Required]
        public string SrcPdf { get; set; }

        [Required]
        public string SignatureBases64 { get; set; }

        [Required]
        public string UniqueId { get; set; }
    }
}