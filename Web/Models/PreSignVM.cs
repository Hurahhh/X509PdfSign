using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class PreSignVM
    {
        [Required]
        public string CommaSeparatedCertChainBase64 { get; set; }
    }
}