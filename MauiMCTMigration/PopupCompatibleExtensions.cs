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
		if (page.Content is not Grid grid)
		{
			grid = new Grid();
			if (page.Content is not null)
			{
				grid.Add(page.Content);
			}
			page.Content = grid;
		}
		popup.page = page;
		popup.pageContainer = new Grid();
		popup.tcs = new();
		popup.pageContainer.Add(new BoxView() { BackgroundColor = Colors.Black, Opacity = 0.2 });
		if (popup.CanBeDismissedByTappingOutsideOfPopup)
		{
			var dismissButton = new Button()
			{
				BackgroundColor = Colors.Transparent,
				CornerRadius = 0,
				Command = new Command(async () => await popup.CloseAsync())
			};
			popup.pageContainer.Add(dismissButton);
		}
		var popupContainer = new Grid()
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			BackgroundColor = popup.Color,
			Padding = 10,
		};
		popupContainer.Add(popup);
		popup.pageContainer.Add(popupContainer);
		grid.Add(popup.pageContainer);
		if (popup.Anchor is VisualElement anchor && anchor is not null)
		{
			var anchorPoint = PopupCompatibleExtensions.GetAbsolutePosition(anchor);
			popupContainer.HorizontalOptions = LayoutOptions.Start;
			popupContainer.VerticalOptions = LayoutOptions.Start;
			bool anchorAtBottom = anchorPoint.Y + anchor.Height + 10 + popup.Height < page.Height;
			popupContainer.TranslationX = anchorPoint.X + anchor.Width / 2;
			if (anchorAtBottom)
			{
				popupContainer.TranslationY = anchorPoint.Y + anchor.Height + 10;
			}
			else
			{
				popupContainer.TranslationY = anchorPoint.Y - 10 - popupContainer.Height;
			}
			anchor.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(VisualElement.Width))
				{
					anchorPoint = PopupCompatibleExtensions.GetAbsolutePosition(anchor);
					popupContainer.TranslationX = anchorPoint.X + anchor.Width / 2 - popup.Width / 2;
				}
			};
			popupContainer.PropertyChanged += (s, e) =>
			{
				switch (e.PropertyName)
				{
					case nameof(PopupCompatible.Width):
						anchorPoint = PopupCompatibleExtensions.GetAbsolutePosition(anchor);
						popupContainer.TranslationX = anchorPoint.X + anchor.Width / 2 - popupContainer.Width / 2;
						break;
					case nameof(PopupCompatible.Height):
						anchorPoint = PopupCompatibleExtensions.GetAbsolutePosition(anchor);
						anchorAtBottom = anchorPoint.Y + anchor.Height + 10 + popupContainer.Height < page.Height;
						if (anchorAtBottom)
						{
							popupContainer.TranslationY = anchorPoint.Y + anchor.Height + 10;
						}
						else
						{
							popupContainer.TranslationY = anchorPoint.Y - 10 - popupContainer.Height;
						}
						break;
				}
			};
		}
		return await popup.tcs.Task;
	}

	/// <summary>
	/// Calculates the absolute position of a <see cref="VisualElement"/> relative to its root element.
	/// </summary>
	/// <param name="visualElement">The <see cref="VisualElement"/> for which to calculate the absolute position.
	/// Cannot be <see langword="null"/>.</param>
	/// <returns>A <see cref="Point"/> representing the absolute position of the specified <paramref name="visualElement"/>.
	/// The coordinates are relative to the root element.</returns>
	public static Point GetAbsolutePosition(VisualElement visualElement)
	{
		var pos = new Point();
		while (visualElement is not null)
		{
			pos.X += visualElement.X;
			pos.Y += visualElement.Y;
			if (visualElement is ScrollView scrollView)
			{
				pos.X -= scrollView.ScrollX;
				pos.Y -= scrollView.ScrollY;
			}
			if (visualElement.Parent is VisualElement parent)
			{
				visualElement = parent;
				continue;
			}
			break;
		}
		return pos;
	}
}
