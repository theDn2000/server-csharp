using SpacetimeDB;

namespace StdModule.Globals
{
    [Table(Name = "userNotification", Public = true)] // Persistent table to store informatio about login attempts, public so clients can access it and chek if the login was successful
    public partial struct UserNotification
    {
        [PrimaryKey]
        public Identity identity; // SpacetimeDB unique authentication identity

        public string message; // Message to be displayed to the user
        public ulong timestamp; // Timestamp of the message
    }
}