using SpacetimeDB;

public static partial class Module
{
    [Table(Name = "config", Public = true)] // Name is config and it is public
    //[Description("Configuration table for the server")]

    public partial struct Config
    {
        [PrimaryKey]
        public uint id;
        public ulong world_size;
    }

    // Define a type (struct) for a vector2
    // This is used to store 2D coordinates in the database
    [SpacetimeDB.Type]
    public partial struct DbVector2(float x, float y)
    {
        public float x = x;
        public float y = y;
    }

    // The entity represents an object in the game world
    [Table(Name = "entity", Public = true)]
    public partial struct Entity
    {
        [PrimaryKey, AutoInc]
        public uint entity_id;
        public DbVector2 position; // Use defined structure DbVector2 for position
        public uint life; // Life of the entity
    }

    // Now we can create different types of entities from the entity struct
    [Table(Name = "character", Public = true)]
    public partial struct Character
    {
        [PrimaryKey]
        public uint entity_id; // Player's entity ID, which is also an entity
        [SpacetimeDB.Index.BTree]
        public uint player_id; // Unique player ID
        public DbVector2 direction;
    }

    [Table(Name = "npc", Public = true)] //NPC only has the same fields as entity (position and life)
    public partial struct Npc
    {
        [PrimaryKey]
        public uint entity_id; // NPC's entity ID, which is also an entity
    }

    [Table(Name = "player", Public = true)] // Player data, each player can control one character
    public partial struct Player
    {
        [PrimaryKey]
        public Identity identity; // SpacetimeDB unique authentication identity
        [Unique, AutoInc] // Unique player ID, which is auto-incremented
        public uint player_id; // Unique player ID
        public string name; // Player's name
    }

    // Reducer: Clients can call this to do things, is super fast and efficient
    // In this case, it just logs the sender of the request
    [Reducer(ReducerKind.ClientConnected)]
    public static void Connect(ReducerContext ctx)
    {
        Log.Info($"{ctx.Sender} just connected");
    }
}

