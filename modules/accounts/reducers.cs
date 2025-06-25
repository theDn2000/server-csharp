using SpacetimeDB;

namespace StdModule.Accounts
{
    public partial class AccountsReducers
    {
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
                last_active = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                current_zone = "default_zone" // Default zone, can be changed later
            });

            Log.Info($"User {account.username} logged in");
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

            ctx.Db.session.identity.Delete(session.identity);

            Log.Info($"User {session.identity} logged out");
        }
    }
}
