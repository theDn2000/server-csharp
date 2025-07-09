using SpacetimeDB;

namespace StdModule.Accounts
{
    public partial class AccountReducers
    {
        [Reducer]
        public static void Login(ReducerContext ctx, string username, string password_hash) // When the login is called, a new session is created
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password_hash))
            {
                throw new Exception("Username and password cannot be empty");
            }

            // Search for the account by username
            var acc = ctx.Db.account.username.Find(username);
            if (acc == null)
            {
                throw new Exception("Account not found");
            }

            // Check if the password matches
            if (acc.Value.password_hash != password_hash)
            {
                throw new Exception("Invalid password");
            }

            // If all is good, create a new session
            if (ctx.Db.session.identity.Find(ctx.Sender) != null)
            {
                throw new Exception("Session already exists");
            }

            var account = acc.Value;

            ctx.Db.session.Insert(new Session
            {
                identity = ctx.Sender,
                account_id = account.account_id,
                character_id = 0, // No character selected at login
                last_active = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                current_zone = "default_zone" // Default zone, can be changed later
            });

            Log.Info($"User {account.username} logged in");
        }
        /*
        [Reducer]
        public static void Login(ReducerContext ctx) // [CHECK] We have to handle the registration and make it compatible with the login reducer
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

            var account = acc.Value; // [CHECK] it should not be necessary to unpack here, but the code does not compile without it

            ctx.Db.session.Insert(new Session
            {
                identity = ctx.Sender,
                account_id = account.account_id,
                character_id = 0, // No character selected at login
                last_active = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                current_zone = "default_zone" // Default zone, can be changed later
            });

            Log.Info($"User {account.username} logged in");
        }
        */
        [Reducer]
        public static void Logout(ReducerContext ctx)
        {
            var ses = ctx.Db.session.identity.Find(ctx.Sender);
            if (ses == null)
            {
                throw new Exception("Session not found");
            }

            var session = ses.Value; // [CHECK] it should not be necessary to unpack here, but the code does not compile without it

            var username = ctx.Db.account.account_id.Find(session.account_id)?.username;
            if (username == null)
            {
                throw new Exception("Account not found");
            }

            ctx.Db.session.identity.Delete(session.identity);

            Log.Info($"User {username} logged out");
        }

        [Reducer]
        public static void Register(ReducerContext ctx, string username, string password_hash)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password_hash))
            {
                throw new Exception("Username and password cannot be empty");
            }

            if (ctx.Db.account.username.Find(username) != null)
            {
                throw new Exception("Account already exists");
            }

            ctx.Db.account.Insert(new Account
            {
                username = username, // The name cdoes not need to be unique, as the identity is unique
                password_hash = password_hash, // Store the hashed password
                created_at = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            });

            Log.Info($"User {username} registered");
        }

        [Reducer]
        public static void DeleteAccount(ReducerContext ctx) // You need to be logged in to delete your account, to prevent abuse
        {
            var ses = ctx.Db.session.identity.Find(ctx.Sender);
            if (ses == null)
            {
                throw new Exception("Session not found");
            }

            var session = ses.Value; // [CHECK] it should not be necessary to unpack here, but the code does not compile without it

            var account = ctx.Db.account.account_id.Find(session.account_id);
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            // 1. Delete all the characters associated with the account
            foreach (var character in ctx.Db.character.Where(c => c.account_id == account.id))
            {
                // 2. Delete the Entity associated with the Character, if it exists
                if (character.entity_id is uint entityId)
                {
                    var entity = ctx.Db.entity.FirstOrDefault(e => e.id == entityId);
                    if (entity is not null)
                    ctx.Db.entity.Delete(entity);
                }
                // 3. Delete the Character itself
                ctx.Db.character.Delete(character);
            }

            // 4. Delete all sessions associated with the account
            foreach (var s in ctx.Db.session.Where(s => s.account_id == account.id))
            {
                ctx.Db.session.Delete(s);
            }

            // 5. Finally, delete the account itself
            ctx.Db.account.Delete(account.Value);

            Log.Info($"User {account.Value.username} deleted its account");
        }
    }
}
