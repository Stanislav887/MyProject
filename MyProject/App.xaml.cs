namespace MyProject
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Apply saved theme
            string savedTheme = Preferences.Default.Get("AppTheme", "System Default");
            switch (savedTheme)
            {
                case "Light":
                    UserAppTheme = AppTheme.Light;
                    break;
                case "Dark":
                    UserAppTheme = AppTheme.Dark;
                    break;
                default:
                    UserAppTheme = AppTheme.Unspecified;
                    break;
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}