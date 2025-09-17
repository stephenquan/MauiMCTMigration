namespace MauiMCTMigration;

/// <summary>
/// Provides static and extension methods for displaying popups on a <see cref="ContentPage"/>.
/// </summary>
public static class PopupCompatibleExtensions
{
	/// <summary>
	/// Displays a popup asynchronously on the specified <see cref="ContentPage"/>.
	/// </summary>
	/// <remarks>The popup is centered on the page by default,
	/// but if an anchor is specified, it will be positioned relative to the anchor.
	/// If <see cref="PopupCompatible.CanBeDismissedByTappingOutsideOfPopup"/> is set to <see langword="true"/>,
	/// the popup can be dismissed by tapping outside of it.</remarks>
	/// <param name="page">The <see cref="ContentPage"/> on which the popup will be displayed.</param>
	/// <param name="popup">The <see cref="PopupCompatible"/> instance representing the popup to be shown.</param>
	/// <returns>A task that represents the asynchronous operation.
	/// The task result contains an optional object returned by the popup when it is closed.</returns>
	public static async Task<object?> ShowPopupAsync(this ContentPage page, PopupCompatible popup)
	{
		return await popup.ShowPopupAsync(page);
	}
}
