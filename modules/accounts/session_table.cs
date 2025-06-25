using SpacetimeDB;

namespace StdModule.Accounts
{
    [Table(Name = "session", Public = true)] // Non-persistent table to store sessions, public so clients can access it
    public partial struct Session
    {
        [PrimaryKey]
        public Identity identity; // SpacetimeDB unique authentication identity
        public uint account_id; // Foreign key to the account table
        public ulong last_active; // Last time the session was active
        public string current_zone; // Current zone the player is in

    // Additional fields can be added as needed
    }
}