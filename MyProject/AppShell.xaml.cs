using Microsoft.Maui.Controls;
namespace MyProject
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register the route for navigation
            Routing.RegisterRoute(nameof(MovieDetailPage), typeof(MovieDetailPage));

            Routing.RegisterRoute(nameof(HistoryPage), typeof(HistoryPage));

            // Register the StatisticsPage route
            Routing.RegisterRoute(nameof(StatisticsPage), typeof(StatisticsPage));

            // Check if user name is saved
            string userName = Preferences.Default.Get<string>("UserName", "");
            if (string.IsNullOrEmpty(userName))
            {
                // Show page to enter name
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await this.Navigation.PushModalAsync(new UserNamePage());
                });
            }
        }

    }
}
