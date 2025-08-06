// Reducers related to character management
using SpacetimeDB;



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
    }
}

        
