using SpacetimeDB;

namespace StdModule.Accounts
{
    [Table(Name = "account", Public = true)] // Persistent table to store accounts, public so clients can access it
    public partial struct Account
    {
        [PrimaryKey]
        public Identity identity; // SpacetimeDB unique authentication identity
        [Unique, AutoInc]
        public uint account_id;
        public string username;
        public ulong created_at; // Unix timestamp of account creation

        // Additional fields can be added as needed
    }
}

