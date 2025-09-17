using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;

namespace MauiMCTMigration;

#pragma warning disable CS1591 // Suppress missing XML comment warning

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	async void OnCounterClicked(object? sender, EventArgs e)
	{
		count++;

		if (count == 1)
		{
			CounterBtn.Text = $"Clicked {count} time";
		}
		else
		{
			CounterBtn.Text = $"Clicked {count} times";
		}

		SemanticScreenReader.Announce(CounterBtn.Text);

		await Task.Delay(1);

		#region Demo of old PopupCompatible
		var oldPopup = new PopupCompatible()
		{
			Anchor = CounterBtn,
			Color = Colors.SkyBlue,
			CanBeDismissedByTappingOutsideOfPopup = true,
		};
		oldPopup.Content = new Button
		{
			Text = "Close from PopupCompatible",
			Command = new Command(async () => await oldPopup.CloseAsync("close"))
		};
		object? result = await this.ShowPopupAsync(oldPopup);
		System.Diagnostics.Debug.WriteLine($"Popup result: {result}");
		#endregion

		#region Demo of new CommunityToolkit.Maui Popup
		var newPopup = new Popup<string>()
		{
			BackgroundColor = Colors.SkyBlue,
			CanBeDismissedByTappingOutsideOfPopup = true,
		};
		newPopup.Content = new Button
		{
			Text = "Close from CommunityToolkit.Maui v 12",
			Command = new Command(async () => await newPopup.CloseAsync("close"))
		};
		var r = await this.ShowPopupAsync<string>(newPopup);
		System.Diagnostics.Debug.WriteLine($"Popup result: {r.Result} WasDismissedByTappingOutsideOfPopup={r.WasDismissedByTappingOutsideOfPopup}");
		#endregion
	}
}
