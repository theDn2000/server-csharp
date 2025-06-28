// Extends entity_table.cs to include character-specific fields
using SpacetimeDB;

namespace StdModule.World
{
    [Table(Name = "entity_character", Public = true)] // Persistent table to store character entities, public so clients can access it
    public partial struct EntityCharacter
    {
        [PrimaryKey, AutoInc]
        public int entity_id; // Unique identifier for the entity, same as in entity_table

        [SpacetimeDB.Index.BTree]
        public uint character_id; // Foreign key to the character this entity represents

        // Display and identification
        public string name;
        public uint level; // Level of the character entity
        public string class_name; // Class of the character entity (e.g., Warrior, Mage)
        public uint race; // Race of the character entity (e.g., Human, Orc)
    }
}