namespace ShopApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            ApplicationConfiguration.Initialize();

            while (true)
            {
                var login = new FormLogin();
                if (login.ShowDialog() != DialogResult.OK) break;

                var main = new FormProducts(login.AuthenticatedUser);
                if (main.ShowDialog() != DialogResult.Retry) break;
            }
        }
    }
}
