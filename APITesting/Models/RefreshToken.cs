using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Pkcs;

namespace APITesting.Models
{
    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("token")]
        public required string Token { get; set; }
        [Column("username")]
        public required string Username { get; set; }
        [Column("expiry_date")]
        public DateTime ExpiryDate{ get; set; }
    }
}
