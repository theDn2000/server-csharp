using SpacetimeDB;

namespace StdModule.Accounts
{
    [Table(Name = "session", Public = true)] // Non-persistent table to store sessions, public so clients can access it
    public partial struct Session
    {
        [PrimaryKey]
        public Identity identity; // SpacetimeDB unique authentication identity
        public uint account_id; // Foreign key to the account table

        [SpacetimeDB.Index.BTree]
        public uint character_id; // Foreign key to the character table, could be 0 if no character is selected

        public ulong last_active; // Last time the session was active (updated at disconnect)
        public string current_zone; // Current zone the player is in

    // Additional fields can be added as needed
    }
}