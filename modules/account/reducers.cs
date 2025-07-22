using SpacetimeDB;
using StdModule.Globals;

namespace StdModule.Accounts
{
    public partial class AccountReducers
    {
        // Initial reducer to handle the connection
        [Reducer(ReducerKind.ClientConnected)]
        public static void Connect(ReducerContext ctx)
        {
            Log.Info($"{ctx.Sender} just connected.");
        }
        
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
                // Add a message to the userNotification table to inform the user that the account was not found
                ctx.Db.userNotification.Insert(new UserNotification // [CHECK] Maybe it is necessary to create a specific reducer for this
                {
                    identity = ctx.Sender,
                    message = "SERVER: Account not found",
                    timestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });

                // Log the error
                throw new Exception("Account not found");

            }

            // Check if the password matches
            if (acc.Value.password_hash != password_hash)
            {
                throw new Exception("Invalid password");
            }

            // If all is good, create a new session [CHECK] la cosa es que no se pueda crear una sesion si ya existe una con el mismo account_id, ya que eso significa que el usuario ya esta logueado
            if (ctx.Db.session.identity.Find(ctx.Sender) != null )
            {
                throw new Exception("Session already exists");
            }

            // If there is a session with the same account_id, it means the user is already logged in another device, therefore update the session
            var existingSession = ctx.Db.session.account_id.Find(acc.Value.account_id);
            Log.Info($"Existing session last_active: {existingSession}");
            if (existingSession != null) // Discard the dummie session with identity 0 
            {
                // LOG
                Log.Warn($"User {existingSession} is already logged in from another device, updating session");
                // Update the existing session with the new identity and reset the character_id
                ctx.Db.session.identity.Delete(existingSession.Value.identity); // [CHECK] Optimizable

                // Inserta la nueva sesión
                ctx.Db.session.Insert(new Session
                {
                    identity = ctx.Sender,
                    account_id = acc.Value.account_id,
                    character_id = 0,
                    last_active = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    current_zone = "default_zone"
                });

                Log.Info($"User {acc.Value.username} logged in from another device, session updated");
            }

            // If there is no session with the same account_id, create a new session
            else
            {
                ctx.Db.session.Insert(new Session
                {
                    identity = ctx.Sender,
                    account_id = acc.Value.account_id,
                    character_id = 0, // No character selected at login
                    last_active = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    current_zone = "default_zone" // Default zone, can be changed later


                });

                Log.Info($"User {acc.Value.username} logged in");
            } 
        }

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
            foreach (var character in ctx.Db.character.account_id.Filter(account.Value.account_id))
            {
                ctx.Db.character.Delete(character);
            }

            // 4. Delete the session associated with the account
            ctx.Db.session.identity.Delete(session.identity);

            // 5. Finally, delete the account itself
            ctx.Db.account.Delete(account.Value);

            Log.Info($"User {account.Value.username} deleted his/her account");
        }
    }
}
