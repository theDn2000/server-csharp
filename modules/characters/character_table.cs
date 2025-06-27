using SpacetimeDB;

namespace StdModule.Characters
{
    [Table(Name = "character", Public = true)] // Persistent table to store characters, public so clients can access it
    public partial struct Character
    {
        [PrimaryKey, AutoInc]
        public uint character_id; // Unique identifier for the character

        [SpacetimeDB.Index.BTree]
        public uint account_id; // Foreign key to the account this character belongs to
        [SpacetimeDB.Index.BTree]
        public uint entity_id; // Foreign key to the entity this character represents, could be 0 if the entity is not yet created

        [Unique]
        public string name; // Unique name for the character
        public ulong created_at; // Timestamp when the character was created

        public string class_name; // Class of the character (e.g., Warrior, Mage)
        public uint level; // Level of the character
        public uint experience; // Experience points of the character
    }
}