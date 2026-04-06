namespace APITesting.Models.DTOs
{
    public class UserDTO
    {
        //we use this DTO for everything, so we'll check for null values in the service layer to determine which fields to create/update
        //these cannot be updated
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }

        //these can be updated
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Address { get; set; }
    }
}