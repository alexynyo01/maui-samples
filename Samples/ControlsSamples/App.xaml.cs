﻿using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Telerik.Maui.Controls;
using Telerik.AppUtils.Services;
using QSF.Common;
using QSF.Pages;
using QSF.Services;
using Application = Microsoft.Maui.Controls.Application;
using System.Threading.Tasks;
using QSF.ViewModels;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;

namespace QSF;

public partial class App : Application
{
    public App(ITestingService testingService)
    {
#if ANDROID
        // Setting the AccentColor from the Maui world here is needed
        // since the moment we try to access it from native Android is too early.
        Application.AccentColor = Color.FromArgb("#2B0B98");
#endif

        this.UserAppTheme = Microsoft.Maui.ApplicationModel.AppTheme.Light;

        this.InitializeComponent();
        this.InitializeDependencies();

        testingService.OnCommand += (service, command) =>
        {
            if (command.Command.StartsWith("NAVIGATE:"))
            {
                var tcs = new TaskCompletionSource<string>();
                command.Result = tcs.Task;
                Dispatcher.Dispatch(async () =>
                {
                    try
                    {
                        var location = command.Command.Substring("NAVIGATE:".Length).Trim();
                        await DependencyService
                            .Get<INavigationService>()
                            .NavigateCommand(location);
                        
                        tcs.SetResult("OK");
                    }
                    catch(Exception e)
                    {
                        tcs.SetException(e);
                    }
                });
            }
            else if (command.Command.StartsWith("GET EXAMPLES:"))
            {
                var examples = ((Application.Current.MainPage as NavigationPage).RootPage.BindingContext as HomeViewModel)?.Examples;
                command.Result =
                    Task.FromResult<string>("OK: " +
                        JsonSerializer.Serialize(
                            examples
                                .Select(e => new Dictionary<string, string>()
                                {
                                    { "Control", e.ControlName },
                                    { "Example", e.Name }
                                })
                                .ToList()));
            }
        };

        if (testingService.IsAppUnderTest && DeviceInfo.Idiom == DeviceIdiom.Desktop)
        {
            MainPage = new NavigationPage(new UITestsHomePage(testingService));
        }
        else
        {
            if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
            {
                this.MainPage = new NavigationPage(new MainPageDesktop(testingService));
            }
            else
            {
                this.MainPage = new NavigationPage(new MainPageMobile(testingService));

#if __ANDROID__ || __IOS__
                // TODO: When https://github.com/dotnet/maui/issues/5835 is really fixed, remove the following lines and the respective methods.
                // Currently, setting MaxLines of a Label to more than one for Android or iOS and LineBreakMode to TailTruncation
                // results to a single-line Label with truncation. The MaxLines is ignored.
                LabelHandler.Mapper.AppendToMapping(nameof(Label.LineBreakMode), UpdateMaxLines);
                LabelHandler.Mapper.AppendToMapping(nameof(Label.MaxLines), UpdateMaxLines);
#endif
            }
        }
        
#if __ANDROID__ || WINDOWS
        Microsoft.Maui.Handlers.ViewHandler.ViewMapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Microsoft.Maui.Handlers.ContentViewHandler.Mapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Microsoft.Maui.Handlers.ImageButtonHandler.Mapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Microsoft.Maui.Handlers.LabelHandler.Mapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Microsoft.Maui.Handlers.LayoutHandler.Mapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Microsoft.Maui.Handlers.RadioButtonHandler.Mapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Microsoft.Maui.Handlers.ScrollViewHandler.Mapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Microsoft.Maui.Handlers.SearchBarHandler.Mapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Microsoft.Maui.Handlers.SliderHandler.Mapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Microsoft.Maui.Handlers.SwitchHandler.Mapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Telerik.Maui.Handlers.RadEntryHandler.EntryViewMapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Telerik.Maui.Handlers.RadButtonHandler.RadButtonMapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Telerik.Maui.Handlers.RadBorderHandler.BorderMapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Telerik.Maui.Handlers.RadItemsControlHandler.ItemsControlMapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
        Telerik.Maui.Handlers.RadCheckBoxHandler.RadCheckBoxMapper.AppendToMapping(nameof(IView.AutomationId), (h, v) => SetAutomationId(v));
#endif
    }

    private static void SetAutomationId(IView v)
    {
        var automationId = v.AutomationId;
        if (!string.IsNullOrEmpty(automationId))
        {
            BindableObject element = v as BindableObject;
            if (element != null)
            {
                SemanticProperties.SetDescription(element, automationId);
            }
        }
    }


    public static void DisplayAlert(string text)
    {
        var popup = new RadPopup();
        popup.IsModal = true;
        popup.Placement = PlacementMode.Center;
        popup.OutsideBackgroundColor = Color.FromArgb("#6F000000");

        var border = new RadBorder();
        border.CornerRadius = new Thickness(8);
        border.BackgroundColor = Color.FromArgb("#F1F1F1");

        var grid = new Grid();
        grid.Padding = new Thickness(10);
        grid.WidthRequest = 200;
        grid.HeightRequest = 150;
        grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });
        grid.RowDefinitions.Add(new RowDefinition() { Height = 30 });

        var label = new Microsoft.Maui.Controls.Label();
        label.Text = text;
        label.VerticalOptions = LayoutOptions.Start;
        label.HorizontalOptions = LayoutOptions.Start;
        label.TextColor = Colors.Black;
        label.LineBreakMode = LineBreakMode.WordWrap;
        Grid.SetRow(label, 0);
        grid.Children.Add(label);

        var okButton = new RadButton();
        okButton.Padding = new Thickness(2);
        if (DeviceInfo.Platform != DevicePlatform.WinUI)
        {
            okButton.WidthRequest = 100;
        }
        okButton.HorizontalOptions = LayoutOptions.Center;
        okButton.VerticalOptions = LayoutOptions.End;
        okButton.BackgroundColor = Color.FromArgb("#674bb2");
        okButton.TextColor = Colors.White;
        okButton.Text = "OK";
        okButton.Clicked += (sender, args) =>
        {
            popup.IsOpen = false;
        };

        Grid.SetRow(okButton, 1);
        grid.Children.Add(okButton);
        border.Content = grid;
        popup.Content = border;
        popup.IsOpen = true;
    }

#if __ANDROID__
    private static void UpdateMaxLines(ILabelHandler handler, ILabel label)
    {
        var textView = handler.PlatformView;
        if (label is Label controlsLabel && controlsLabel.MaxLines > -1 && textView.Ellipsize == Android.Text.TextUtils.TruncateAt.End)
        {
            textView.SetMaxLines(controlsLabel.MaxLines);
        }
    }
#elif __IOS__
    private static void UpdateMaxLines(ILabelHandler handler, ILabel label)
    {
        var labelView = handler.PlatformView;
        if (label is Label controlsLabel && labelView.LineBreakMode == UIKit.UILineBreakMode.TailTruncation)
        {
            labelView.Lines = controlsLabel.MaxLines;
        }
    }
#endif

#if WINDOWS || MACCATALYST
    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
        if (window != null)
        {
#if WINDOWS
            window.Title = "Telerik UI for .NET MAUI Controls Samples";
#endif

#if NET7_0_OR_GREATER
            window.MinimumWidth = 1024;
            window.MinimumHeight = 768;
#endif
        }

        return window;
    }
#endif

    private void InitializeDependencies()
    {
        DependencyService.Register<IConfigurationService, ConfigurationService>();
        DependencyService.Register<IResourceService, AssemblyResourceService>();
        DependencyService.Register<IFileViewerService, FileViewerService>();
        DependencyService.Register<INavigationService, NavigationService>();
        DependencyService.Register<IExampleService, ExampleService>();
        DependencyService.Register<IConfigurationAreaService, ConfigurationAreaService>();
        DependencyService.Register<IControlsService, ControlsService>();
        DependencyService.Register<ISearchService, SearchService>();
        DependencyService.Register<IToastMessageService, ToastMessageService>();
        DependencyService.Register<ISerializationService, SerializationService>();
        DependencyService.Register<IFilePickerService, FilePickerService>();
        DependencyService.Register<IMediaPickerService, MediaPickerService>();
    }

#if IOS
    protected override void OnStart()
    {
        base.OnStart();

        UIKit.UINavigationController vc = (UIKit.UINavigationController)Microsoft.Maui.ApplicationModel.Platform.GetCurrentUIViewController();
        vc.InteractivePopGestureRecognizer.Delegate = new BackSwipeWithoutNavigationBar(vc);
        vc.InteractivePopGestureRecognizer.Enabled = true;
    }

    public class BackSwipeWithoutNavigationBar : UIKit.UIGestureRecognizerDelegate
    {
        private UIKit.UINavigationController vc;

        public BackSwipeWithoutNavigationBar(UIKit.UINavigationController vc)
        {
            this.vc = vc;
        }

        public override bool ShouldBegin(UIKit.UIGestureRecognizer recognizer)
        {
            return this.vc.ViewControllers.Length > 1;
        }
    }
#endif
}
