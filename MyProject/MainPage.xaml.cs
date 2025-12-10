namespace MyProject
{
    public partial class MainPage : ContentPage
    {
        private MovieViewModel viewModel;

        public MainPage()
        {
            InitializeComponent();
            viewModel = new MovieViewModel();
            BindingContext = viewModel;
        }


    }
}
