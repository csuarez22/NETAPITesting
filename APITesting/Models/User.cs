using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata.Ecma335;
using APITesting.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;

namespace APITesting.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("username")]
        public required string Username { get; set; }
        [Column("email")]
        public required string Email { get; set; }
        [Column("password")]
        public required string EncodedPassword { get; set; }
        [Column("first_name")]
        public required string FirstName { get; set; }
        [Column("last_name")]
        public required string LastName { get; set; }
        [Column("date_of_birth")]
        public DateOnly DateOfBirth { get; set; }
        
        //user doesn't always need to specify address, so we make it nullable
        [Column("address")]
        public string? Address { get; set; }
    }
}