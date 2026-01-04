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

    private void StatsTimeRangePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (StatsTimeRangePicker.SelectedItem is string selected)
        {
            viewModel.SelectedTimeRange = selected;
        }
    }
}