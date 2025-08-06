// Reducers related to character management
using SpacetimeDB;
// Import necessary namespaces
using StdModule.Accounts;

namespace StdModule.Characters
{
    public partial class CharacterReducers
    {
        // Reducer to create a new character
        [Reducer]
        public static void CreateCharacter(ReducerContext ctx, string name, string race)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Character name cannot be empty");
            }

            // Check if character with this name already exists for the account
            var existingCharacter = ctx.Db.character.name.Find(name);
            if (existingCharacter != null)
            {
                throw new Exception($"Character with name '{name}' already exists");
            }

            // Check if the account has reached the maximum number of characters
            var session = ctx.Db.session.identity.Find(ctx.Sender);
            if (session == null)
            {
                throw new Exception("Session not found");
            }

            // Obtain the account information from the session
            var account = ctx.Db.account.account_id.Find(session.Value.account_id);
            if (account == null)
            {
                throw new Exception("Account not found");
            }
            if (account.Value.number_of_characters >= 5) // [CHECK] Assuming a max of 5 characters per account
            {
                throw new Exception("Maximum number of characters reached for this account (5)");
            }

            // Create a new character
            var character = new Character
            {
                account_id = account.Value.account_id, // Use the account ID from the account table
                entity_id = 0, // Initially no entity is created, can be updated later
                username = account.Value.username, // Use the username from the account table

                name = name,
                created_at = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),

                class_name = "Unemployed", // Default class, can be changed later
                race = race, // Race provided by the user
                level = 1, // Starting level
                experience = 0, // Starting experience

                strength = 0, // Default stats
                intelligence = 0,
                agility = 0,
                endurance = 0,
            };

            ctx.Db.character.Insert(character);
            Log.Info($"Character {character.name}, level {character.level} {character.race} {character.class_name} from {character.username} created successfully");
        }



        // Reducer to delete a character
        [Reducer]
        public static void DeleteCharacter(ReducerContext ctx, uint character_id)
        {
            // Validate the character ID
            if (character_id == 0)
            {
                throw new Exception("Character ID cannot be 0");
            }

            // Obtain the session information
            var session = ctx.Db.session.identity.Find(ctx.Sender);
            if (session == null)
            {
                throw new Exception("Session not found");
            }

            // Get the account information from the session
            var account = ctx.Db.account.account_id.Find(session.Value.account_id);
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            // Get the character information
            var character = ctx.Db.character.character_id.Find(character_id);
            if (character == null)
            {
                throw new Exception($"Character with ID '{character_id}' not found");
            }

            // Check if the character belongs to the account
            if (character.Value.account_id != account.Value.account_id)
            {
                throw new Exception("Character does not belong to this account");
            }

            // Delete the character
            ctx.Db.character.Delete(character.Value);
            Log.Info($"Character {character.Value.name} from {account.Value.username} deleted successfully");
        }



        // Reducer to select a character (assign it to the session)
        [Reducer]
        public static void SelectCharacter(ReducerContext ctx, uint character_id)
        {
            // Validate the character ID
            if (character_id == 0)
            {
                throw new Exception("Character ID cannot be 0");
            }

            // Obtain the session information
            var session = ctx.Db.session.identity.Find(ctx.Sender);
            if (session == null)
            {
                throw new Exception("Session not found");
            }

            // Obtain the account and character information
            var account = ctx.Db.account.account_id.Find(session.Value.account_id);
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            // Get the character information
            var character = ctx.Db.character.character_id.Find(character_id);
            if (character == null)
            {
                throw new Exception($"Character with ID '{character_id}' not found");
            }

            // Check if the character belongs to the account
            if (character.Value.account_id != account.Value.account_id)
            {
                throw new Exception("Character does not belong to this account");
            }

            // Update the session in the database

            // Unpack the session
            var updated_session = session.Value; 
            // Assign the selected character ID to the session
            updated_session.character_id = character_id;
            // Push the changes to the database
            ctx.Db.session.identity.Update(updated_session); 


            // Log the selection
            Log.Info($"Character {character.Value.name} selected for session {ctx.Sender}");
        }
    }
}