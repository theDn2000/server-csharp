using SpacetimeDB;

public static partial class Module
{
    [Table(Name = "account", Public = true)] // Persistent table to store accounts, public so clients can access it
    public partial struct Account
    {
        [PrimaryKey]
        public Identity identity; // SpacetimeDB unique authentication identity
        [Unique, AutoInc]
        public uint account_id;
        public string username;
        public string email;

        // Additional fields can be added as needed
    }

    [Table(Name = "session", Public = true)] // Non-persistent table to store sessions, public so clients can access it
    public partial struct Session
    {
        [PrimaryKey]
        public Identity identity; // SpacetimeDB unique authentication identity
        public uint account_id; // Foreign key to the account table
        public DateTimeOffset last_active; // Last time the session was active
        public string current_zone; // Current zone the player is in

        // Additional fields can be added as needed
    }



    [Reducer]
    public static void Login(ReducerContext ctx)
    {
        var acc = ctx.Db.account.identity.Find(ctx.Sender);
        if (acc == null)
        {
            throw new Exception("Account not found");
        }

        if (ctx.Db.session.identity.Find(ctx.Sender) != null)
        {
            throw new Exception("Session already exists");
        }

        ctx.Db.session.Insert(new Session
        {
            identity = ctx.Sender,
            account_id = acc.account_id,
            last_active = Time.Now,
            current_zone = "default_zone" // Default zone, can be changed later
        });

        Log.Info($"User {acc.username} logged in");
    }

    [Reducer]
    public static void Logout(ReducerContext ctx)
    {
        var session = ctx.Db.session.identity.Find(ctx.Sender);
        if (session == null)
        {
            throw new Exception("Session not found");
        }
        
        ctx.Db.session.identity.Delete(session.identity);

        Log.Info($"User {session.identity} logged out");
    }

































    /*

    [Table(Name = "spawn_npc_timer", Scheduled = nameof(SpawnNpc), ScheduledAt = nameof(scheduled_at))] // Table to schedule an event. This is useful to call a Reducer with a "periodical timer" 
    public partial struct SpawnNpcTimer
    {
        [PrimaryKey, AutoInc]
        public ulong scheduled_id;
        public ScheduleAt scheduled_at; // Variable de tipo ScheduledAt
    }


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
    public partial struct DbVector2
    {
        public float x;
        public float y;

        public DbVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        // Calcula la magnitud (longitud) del vector
        public float Magnitude()
        {
            return MathF.Sqrt(x * x + y * y);
        }

        // Devuelve el vector normalizado (misma dirección, longitud 1)
        public DbVector2 Normalized()
        {
            float mag = Magnitude();
            if (mag == 0)
                return new DbVector2(0, 0);
            return new DbVector2(x / mag, y / mag);
        }
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
        public float speed;
    }

    [Table(Name = "npc", Public = true)] //NPC only has the same fields as entity (position and life)
    public partial struct Npc
    {
        [PrimaryKey]
        public uint entity_id; // NPC's entity ID, which is also an entity
    }

    [Table(Name = "player", Public = true)] // Player data, each player can control one character
    [Table(Name = "logged_out_player")] // Table which contains the logged out players, notice that it is not public, so it can only be accessed by the database owner, which is the database creator
    public partial struct Player
    {
        [PrimaryKey]
        public Identity identity; // SpacetimeDB unique authentication identity
        [Unique, AutoInc] // Unique player ID, which is auto-incremented
        public uint player_id; // Unique player ID
        public string name; // Player's name
    }



    // Reducer: Clients can call this to do things, is super fast and efficient
    [Reducer(ReducerKind.Init)] // This is called when the server starts, before any client connects
    public static void Init(ReducerContext ctx)
    {
        Log.Info("Initializing...");
        ctx.Db.config.Insert(new Config { world_size = 1000 }); // Inserts a Config struct (defined with world_size 1000) into the config table. THIS IS A METHOD TO INSERT ELEMENTS TO A TABLE
        ctx.Db.spawn_npc_timer.Insert(new SpawnNpcTimer { scheduled_at = new ScheduleAt.Interval(TimeSpan.FromMilliseconds(500)) }); // Insert a new timer to the spawn_npc_timer TABLE
    }



    // NPC Variables
    const uint NPC_MASS_MIN = 2;
    const uint NPC_MASS_MAX = 4;
    const uint TARGET_NPC_COUNT = 600;

    public static float MassToRadius(uint mass) => MathF.Sqrt(mass);

    [Reducer] // This reducer spawns npcs periodically, so it must be called periodically
    public static void SpawnNpc(ReducerContext ctx, SpawnNpcTimer _timer)
    {
        if (ctx.Db.player.Count == 0) // There are no players yet
        {
            return; // Don't spawn NPCs
        }
        var world_size = (ctx.Db.config.id.Find(0) ?? throw new Exception("Config not found")).world_size;
        var rng = ctx.Rng;
        var npc_count = ctx.Db.npc.Count;
        while (npc_count < TARGET_NPC_COUNT)
        {
            var npc_mass = rng.Range(NPC_MASS_MIN, NPC_MASS_MAX);
            var npc_radius = MassToRadius(npc_mass);
            var x = rng.Range(npc_radius, world_size - npc_radius);
            var y = rng.Range(npc_radius, world_size - npc_radius);
            var entity = ctx.Db.entity.Insert(new Entity()
            {
                position = new DbVector2(x, y),
                life = 15,
            });

            ctx.Db.npc.Insert(new Npc
            {
                entity_id = entity.entity_id,
            });
            npc_count++;
            Log.Info($"Spawned NPC! {entity.entity_id}");
        }
    }
    // Functions that give a random range either a float or an int
    public static float Range(this Random rng, float min, float max) => rng.NextSingle() * (max - min) + min;
    public static uint Range(this Random rng, uint min, uint max) => (uint)rng.NextInt64(min, max);



    // This reducer logs the sender of the request
    [Reducer(ReducerKind.ClientConnected)]
    public static void Connect(ReducerContext ctx)
    {
        var player = ctx.Db.logged_out_player.identity.Find(ctx.Sender);
        if (player != null) // The player was offline
        {
            ctx.Db.player.Insert(player.Value);
            ctx.Db.logged_out_player.identity.Delete(player.Value.identity); // Check (when a player connects) if he was disconnected or is new (because it was not registered in the logged out player table)
        }
        else // The player is new
        {
            ctx.Db.player.Insert(new Player
            {
                identity = ctx.Sender,
                name = "",
            });
        }
        Log.Info($"{ctx.Sender} just connected");
    }



    [Reducer(ReducerKind.ClientDisconnected)]
    public static void Disconnect(ReducerContext ctx)
    {
        var player = ctx.Db.player.identity.Find(ctx.Sender) ?? throw new Exception("Player not found"); // The player disconnected but does not exist => ERROR
                                                                                                         // Remove all characters form arena
        foreach (var character in ctx.Db.character.player_id.Filter(player.player_id)) // Check all the characters controlled by a single player (in a future, each player should only control 1 character)
        {
            var entity = ctx.Db.entity.entity_id.Find(character.entity_id) ?? throw new Exception("Could not find entity ingame related to any of the characters of the player");
            ctx.Db.entity.entity_id.Delete(entity.entity_id);
            ctx.Db.character.entity_id.Delete(entity.entity_id); // No puedo NO borrar el character porque si borro la entidad se quedaria huerfano por la clave primaria, que es entity_id.
        }
        ctx.Db.logged_out_player.Insert(player); // The player is now offline
        ctx.Db.player.identity.Delete(player.identity); // The player is deleted from the "online" players table
    }



    [Reducer]
    public static void EnterGame(ReducerContext ctx, string name)
    {
        Log.Info($"Creating player with name {name}");
        var player = ctx.Db.player.identity.Find(ctx.Sender) ?? throw new Exception("Player not found"); // The player wants to join a game but it is not connected => ERROR
        player.name = name;
        ctx.Db.player.identity.Update(player);
        SpawnPlayerInitialCharacter(ctx, player.player_id);
    }

    public static Entity SpawnPlayerInitialCharacter(ReducerContext ctx, uint player_id)
    {
        var rng = ctx.Rng;
        var world_size = (ctx.Db.config.id.Find(0) ?? throw new Exception("Config not found")).world_size;
        var player_start_radius = MassToRadius(15);
        var x = rng.Range(player_start_radius, world_size - player_start_radius);
        var y = rng.Range(player_start_radius, world_size - player_start_radius);
        return SpawnCharacterAt(
            ctx,
            player_id,
            15,
            new DbVector2(x, y),
            ctx.Timestamp
        );
    }

    [Reducer]
    public static void UpdatePlayerInput(ReducerContext ctx, DbVector2 direction)
    {
        var player = ctx.Db.player.identity.Find(ctx.Sender) ?? throw new Exception("Player not found");				
        foreach (var c in ctx.Db.character.player_id.Filter(player.player_id))
        {
            var character = c;
            character.direction = direction.Normalized();
            character.speed = Math.Clamp(direction.Magnitude(), 0f, 1f);
            ctx.Db.character.entity_id.Update(character);
        }
        
    }

    public static Entity SpawnCharacterAt(ReducerContext ctx, uint player_id, uint life, DbVector2 position, DateTimeOffset timestamp)
    {
        var entity = ctx.Db.entity.Insert(new Entity
        {
            position = position,
            life = life,
        });

        ctx.Db.character.Insert(new Character
        {
            entity_id = entity.entity_id,
            player_id = player_id,
            direction = new DbVector2(0, 1),
            speed = 0f,
        });
        return entity;
    }
    */
}

