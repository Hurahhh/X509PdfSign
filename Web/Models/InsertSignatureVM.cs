using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class InsertSignatureVM
    {
        [Required]
        public string UniqueId { get; set; }

        [Required]
        public string CommaSeparatedCertChainBase64 { get; set; }

        [Required]
        public string PdfBase64 { get; set; }

        [Required]
        public string SignatureBases64 { get; set; }
    }
}