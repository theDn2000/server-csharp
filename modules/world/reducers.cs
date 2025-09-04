// Reducers related to character management
using SpacetimeDB;

// Import necessary namespaces
using StdModule.Characters;



namespace StdModule.World
{
    public partial class WorldReducers
    {
        // Reducer to create a new entity
        [Reducer]
        public static void CreateEntity(ReducerContext ctx, string entity_type)
        {
            if (string.IsNullOrEmpty(entity_type))
            {
                throw new Exception("Entity type cannot be empty");
            }

            // Check that the session is valid
            var session = ctx.Db.session.identity.Find(ctx.Sender);
            if (session == null)
            {
                throw new Exception("Session does not exist");
            }

            // Create a new entity
            var entity = new Entity
            {
                pos_x = 0.0f, // Default position
                pos_y = 0.0f,
                pos_z = 0.0f,
                rot_y = 0.0f, // Default rotation

                //health = 100, // Default health
                //mana = 100, // Default mana

                entity_type = entity_type,
                created_at = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };

            ctx.Db.entity.Insert(entity);
            Log.Info($"Entity of type {entity.entity_type} created successfully with ID {entity.entity_id}");
        }

        // Create an entity_character
        [Reducer]
        public static void CreateEntityCharacter(ReducerContext ctx, Character character)
        {
            // Validaciones básicas
            if (character.character_id == 0)
                throw new Exception("Invalid character_id");
            if (string.IsNullOrEmpty(character.name))
                throw new Exception("Name cannot be empty");
            if (string.IsNullOrEmpty(character.class_name))
                throw new Exception("Class name cannot be empty");

            // Crear entidad base
            var entity = new Entity
            {
                pos_x = character.last_pos_x,
                pos_y = character.last_pos_y,
                pos_z = character.last_pos_z,
                rot_y = 0.0f,
                entity_type = "character",
                created_at = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };

            ctx.Db.entity.Insert(entity);

            Log.Info($"Entity created successfully: entity_id={entity.entity_id}");

            // Crear la extensión entity_character usando el mismo entity_id
            var entityCharacter = new EntityCharacter
            {
                entity_id = entity.entity_id,
                character_id = character.character_id,
                name = character.name,
                level = character.level,
                class_name = character.class_name,
                race = character.race
            };
            ctx.Db.entity_character.Insert(entityCharacter);

            Log.Info($"EntityCharacter created: entity_id={entityCharacter.entity_id}, character_id={character.character_id}, name={character.name}");
        }
    }
}

        
