using SQuan.Helpers.Maui.Mvvm;

namespace MauiMCTMigration;

/// <summary>
/// Implements a popup with a similar API to CommunityToolkit.Maui.Views.Popup v11.
/// </summary>
public partial class PopupCompatible : ContentView
{
	/// <summary>
	/// Gets or sets the anchor element for the popup.
	/// </summary>
	[BindableProperty] public partial VisualElement? Anchor { get; set; }

	/// <summary>
	/// Gets or sets the background color of the popup.
	/// </summary>
	[BindableProperty] public partial Color Color { get; set; } = Colors.Transparent;

	/// <summary>
	/// Gets or sets whether the popup can be dismissed by tapping outside of it.
	/// </summary>
	[BindableProperty] public partial bool CanBeDismissedByTappingOutsideOfPopup { get; set; } = true;

	/// <summary>
	/// Internal property to hold the page that hosts the popup.
	/// </summary>
	public ContentPage? page { get; internal set; }

	/// <summary>
	/// Internal property that holds the container grid for the popup.
	/// </summary>
	public Grid? pageContainer { get; internal set; }

	/// <summary>
	/// Internal property that holds the TaskCompletionSource for the popup result.
	/// </summary>
	public TaskCompletionSource<object?> tcs { get; internal set; } = new();

	/// <summary>
	/// Asynchronously closes the current page and sets the result of the operation.
	/// <param name="result">An optional result object to be set upon closing the page. Can be <see langword="null"/>.</param>
	/// <returns></returns>
	/// </summary>
	public async Task CloseAsync(object? result = null)
	{
		if (page is not null && page.Content is Grid grid && grid is not null && pageContainer is not null)
		{
			await grid.Dispatcher.DispatchAsync(() =>
			{
				grid.Remove(pageContainer);
				pageContainer = null;
			});
			tcs.SetResult(result);
		}
	}
}
