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
        }
    }
}
