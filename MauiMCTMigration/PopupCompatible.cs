
using Microsoft.Maui.Controls.Shapes;
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

	ContentPage? page;
	Grid? outsideOfPopupContainer;
	BoxView? scrim;
	Grid? popupContainer;
	TaskCompletionSource<object?> tcs = new();

	/// <summary>
	/// Asynchronously closes the current page and sets the result of the operation.
	/// <param name="result">An optional result object to be set upon closing the page. Can be <see langword="null"/>.</param>
	/// <returns></returns>
	/// </summary>
	public async Task CloseAsync(object? result = null)
	{
		if (page is not null && page.Content is Grid pageGrid && pageGrid is not null && outsideOfPopupContainer is not null)
		{
			await pageGrid.Dispatcher.DispatchAsync(async () =>
			{
				if (popupContainer is not null)
				{
					await popupContainer.ScaleTo(0, 100, Easing.CubicIn);
				}
				pageGrid.Remove(outsideOfPopupContainer);
				outsideOfPopupContainer = null;
			});
			tcs.SetResult(result);
		}
	}

	/// <summary>
	/// Displays a popup asynchronously on the specified <see cref="ContentPage"/>.
	/// </summary>
	/// <remarks>The popup is centered on the page by default.
	/// If an anchor is specified, the popup will be positioned relative to the anchor.
	/// The popup can be dismissed by tapping outside of it if <see cref="CanBeDismissedByTappingOutsideOfPopup"/> is set to <see langword="true"/>.</remarks>
	/// <param name="page">The <see cref="ContentPage"/> on which the popup will be displayed. Must not be null.</param>
	/// <returns>A task that represents the asynchronous operation.
	/// The task result contains an optional object that may be returned when the popup is closed.</returns>
	public async Task<object?> ShowPopupAsync(ContentPage page)
	{
		if (page.Content is not Grid pageGrid)
		{
			pageGrid = new Grid();
			if (page.Content is not null)
			{
				pageGrid.Add(page.Content);
			}
			page.Content = pageGrid;
		}
		tcs = new();
		this.page = page;
		outsideOfPopupContainer = new Grid();
		outsideOfPopupContainer.Add(scrim = new BoxView() { BackgroundColor = Colors.Black, Opacity = 0.2 });
		pageGrid.Add(outsideOfPopupContainer);
		if (CanBeDismissedByTappingOutsideOfPopup)
		{
			var outsideOfPopup = new Button()
			{
				BackgroundColor = Colors.Transparent,
				CornerRadius = 0,
				Command = new Command(async () => await CloseAsync())
			};
			outsideOfPopupContainer.Add(outsideOfPopup);
		}
		popupContainer = new Grid()
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
		};
		popupContainer.Add(new Border()
		{
			BackgroundColor = Color,
			Stroke = Colors.LightGray,
			StrokeThickness = 1,
			StrokeShape = new RoundRectangle()
			{
				CornerRadius = 10
			}
		});
		popupContainer.Add(new ContentView() { Padding = 20, Content = this });
		popupContainer.Scale = 0;
		Task<bool> scaleToTask = popupContainer.ScaleTo(1, 100, Easing.CubicOut);
		outsideOfPopupContainer.Add(popupContainer);
		if (Anchor is VisualElement anchor && anchor is not null)
		{
			var anchorPoint = GetAbsolutePosition(anchor);
			popupContainer.HorizontalOptions = LayoutOptions.Start;
			popupContainer.VerticalOptions = LayoutOptions.Start;
			popupContainer.TranslationX = anchorPoint.X + anchor.Width / 2 - popupContainer.Width / 2;
			bool anchorAtBottom = anchorPoint.Y + anchor.Height + 10 + popupContainer.Height < page.Height;
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
					anchorPoint = GetAbsolutePosition(anchor);
					popupContainer.TranslationX = anchorPoint.X + anchor.Width / 2 - popupContainer.Width / 2;
				}
			};
			popupContainer.PropertyChanged += (s, e) =>
			{
				switch (e.PropertyName)
				{
					case nameof(PopupCompatible.Width):
						anchorPoint = GetAbsolutePosition(anchor);
						popupContainer.TranslationX = anchorPoint.X + anchor.Width / 2 - popupContainer.Width / 2;
						break;
					case nameof(PopupCompatible.Height):
						anchorPoint = GetAbsolutePosition(anchor);
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
		await scaleToTask;
		return await tcs.Task;
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
