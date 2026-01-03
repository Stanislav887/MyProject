using Microsoft.Maui.Controls;

namespace MyProject;

public partial class StatisticsPage : ContentPage
{
    private MovieViewModel viewModel;
    public StatisticsPage()
	{
		InitializeComponent();
        viewModel = MovieViewModel.Shared;
        BindingContext = viewModel;
    }
}