﻿using Lumina.Services;
using Lumina.ViewModels.Pages;
using Lumina.ViewModels.Windows;
using Lumina.Views.Pages;
using Lumina.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharpHook;
using SharpHook.Native;
using SharpHook.Reactive;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Windows.Threading;
using Wpf.Ui;
using Wpf.Ui.Appearance;

namespace Lumina
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)); })
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<ApplicationHostService>();

                // Page resolver service
                services.AddSingleton<IPageService, PageService>();

                // Theme manipulation

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();

                // Main window with navigation
                services.AddSingleton<INavigationWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();

                services.AddSingleton<DashboardPage>();
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<FeaturesPage>();
                services.AddSingleton<FeaturesViewModel>();
                services.AddSingleton<
                    SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<FullAutoPage>();
                services.AddSingleton<FullAutoViewModel>();

                services.AddSingleton<LuminaPage>();
                services.AddSingleton<LuminaViewModel>();
            }).Build();

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>

        async void OnStartup(object sender, StartupEventArgs e)
        {

            

            WebClient client = new WebClient();
            Stream stream = client.OpenRead("https://raw.githubusercontent.com/yurkyu/Lumina/master/Info/Version");
            StreamReader reader = new StreamReader(stream);
            String content = reader.ReadLine();

            var Version = "v0.9.61";

            /* if (Version.Equals(content, StringComparison.OrdinalIgnoreCase))
            {

            }
            else
            {
                Console.WriteLine("Getting Auto Updater Status...");
                WebClient client1 = new WebClient();
                Stream stream1 = client1.OpenRead("https://raw.githubusercontent.com/miuku-dll/Lumina/master/Info/AutoUpdateStatus");
                StreamReader reader1 = new StreamReader(stream1);
                String content1 = reader1.ReadLine();


                if (content1 == "on")
                {
                    MessageBox.Show("New version detected. Press ok to install");
                    Thread.Sleep(1000);

                    System.Diagnostics.Process.Start("./Lumina_Updater"); ;
                }
                else if (content1 == "off")
                {
                    MessageBox.Show("New version detected. Auto Update is turned off, please install new version from our Discord...");
                    Thread.Sleep(1000);
                    Process.Start(new ProcessStartInfo("https://discordapp.com/channels/1154133794417295532/1224072077405978646") { UseShellExecute = true });
                }
                Environment.Exit(0);
            } */


            try
            {
                Webhooks.WebhookLaunch();
                DashboardViewModel viewModel = new DashboardViewModel();
            }
            catch
            { /* If no webhook is set catches it */ }

            if (Settings.Default.FirstLaunch == true)
            {

                Process.Start(new ProcessStartInfo("https://discord.gg/4xuYgTzp5H") { UseShellExecute = true });

                Settings.Default.FirstLaunch = false;
                Util.SaveConfig();
            }
            await Task.Run(() => Killswitch());
            _host.Start();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            _host.Dispose();

            Environment.Exit(0);
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }

        public async Task Killswitch()
        {

            var hook = new SimpleReactiveGlobalHook(TaskPoolScheduler.Default);

            hook.KeyReleased.Subscribe(e => OnKeyReleased(e, hook));

            hook.KeyReleased.Subscribe(e => OnKeyReleased(e, hook));
            hook.RunAsync().Subscribe();

            static void OnKeyReleased(KeyboardHookEventArgs e, IReactiveGlobalHook hook)
            {
                if (e.Data.KeyCode == KeyCode.VcDelete)
                {
                    Environment.Exit(0);
                    hook.Dispose();
                }
            }
        }
    }
}
