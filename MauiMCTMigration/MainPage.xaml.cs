using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using SQuan.Helpers.Maui.Mvvm;

namespace MauiMCTMigration;

public partial class PopupCompatible : ContentView
{
	[BindableProperty] public partial VisualElement? Anchor { get; set; }
	[BindableProperty] public partial Color Color { get; set; } = Colors.Transparent;
	[BindableProperty] public partial bool CanBeDismissedByTappingOutsideOfPopup { get; set; } = true;
	public ContentPage? page { get; internal set; }
	public Grid? pageContainer { get; internal set; }
	public TaskCompletionSource<object?> tcs { get; internal set; } = new();
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

public static class PopupCompatibleExtensions
{
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
