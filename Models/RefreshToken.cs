
using System.ComponentModel.DataAnnotations.Schema;

namespace JwtDemo.Models
{
    public class RefreshToken
    {
        public int Id {get;set;}
        public string Token {get;set;} = string.Empty;
        public string UserId {get;set;} = string.Empty;
        public string JwtId {get;set;} = string.Empty;
        public bool IsUsed {get;set;} = false;
        public bool IsRevoked {get;set;} = false;
        [ForeignKey(nameof(UserId))]
        public User? User {get;set;} 
        public DateTime DateAdded {get ; set;}
        public DateTime ExpiryDate {get ; set;}
        
    }
}