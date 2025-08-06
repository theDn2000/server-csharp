using SpacetimeDB;

namespace StdModule.World
{
    [Table(Name = "entity", Public = true)] // Persistent table to store entities, public so clients can access it
    public partial struct Entity
    {
        [PrimaryKey, AutoInc]
        public int entity_id; // Unique identifier for the entity, extensions can use this to reference the entity

        // Position and rotation in the world
        public float pos_x; // X position of the entity in the world
        public float pos_y; // Y position of the entity in the world
        public float pos_z; // Z position of the entity in the world
        public float rot_y; // Y rotation of the entity (yaw), used for orientation in the world

        // Health and status
        //public uint health; // Current health of the entity
        //public uint mana; // Current mana of the entity

        // Utils
        public string entity_type; // Type of the entity (e.g., "character", "npc", "item")
        public ulong created_at; // Timestamp when the entity was created
    }
}