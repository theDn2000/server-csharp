using SpacetimeDB;

namespace StdModule.Accounts
{
    [Table(Name = "account")] // Persistent table to store accounts
    public partial struct Account
    {
        [PrimaryKey]
        public string username;
        public string password_hash; // Hashed password for security
        [Unique, AutoInc]
        public uint account_id;
        
        public ulong created_at; // Unix timestamp of account creation

        public uint number_of_characters; // Number of characters created by this account

        // Additional fields can be added as needed
    }
}

