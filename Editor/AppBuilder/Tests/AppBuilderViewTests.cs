﻿using System;
using System.Windows.Input;
using ApprovalTests.Reporters;
using ApprovalTests.Wpf;
using DeltaEngine.Editor.Core;
using DeltaEngine.Editor.Messages;
using NUnit.Framework;
using WpfWindow = System.Windows.Window;

namespace DeltaEngine.Editor.AppBuilder.Tests
{
	[UseReporter(typeof(KDiffReporter))]
	public class AppBuilderViewTests
	{
		[Test, STAThread, Category("Slow")]
		public void VerifyViewWithMocking()
		{
			WpfApprovals.Verify(CreateTestWindow(CreateViewAndViewModelViaMockService()));
		}

		private static WpfWindow CreateTestWindow(AppBuilderView view)
		{
			return new WpfWindow
			{
				Content = view,
				Width = 1280,
				Height = 720,
				MinWidth = 800,
				MinHeight = 480
			};
		}

		private static AppBuilderView CreateViewAndViewModelViaMockService()
		{
			var service = new MockBuildService();
			var view = CreateViewWithInitialiedViewModel(service);
			service.ReceiveSomeTestMessages();
			return view;
		}

		private static AppBuilderView CreateViewWithInitialiedViewModel(Service service)
		{
			var view = new AppBuilderView();
			view.Init(service);
			return view;
		}

		[Test, STAThread, Category("Slow"), Category("WPF")]
		public void ShowViewWithMockServiceAndDummyApps()
		{
			var appBuilderView = CreateViewAndViewModelViaMockService();
			var window = CreateTestWindow(appBuilderView);
			var viewModel = appBuilderView.ViewModel;
			viewModel.AppListViewModel.AddApp("My favorite app".AsMockAppInfo(PlatformName.Windows));
			viewModel.AppListViewModel.AddApp("My mobile app".AsMockAppInfo(PlatformName.WindowsPhone7));
			window.ShowDialog();
		}

		[Test, STAThread, Category("Slow"), Category("WPF")]
		public void ShowViewWithMockServiceAndLoadedAppStorage()
		{
			var appBuilderView = CreateViewAndViewModelViaMockService();
			var window = CreateTestWindow(appBuilderView);
			appBuilderView.ViewModel.AppListViewModel.Load();
			window.ShowDialog();
		}

		[Test, STAThread, Category("Slow"), Category("WPF")]
		public void ShowViewWithMockServiceToVisualizeSwitchingBetweenBothLists()
		{
			var service = new MockBuildService();
			var appBuilderView = CreateViewWithInitialiedViewModel(service);
			var window = CreateTestWindow(appBuilderView);
			window.MouseDoubleClick += (sender, e) => FireAppBuildMessagesOnMouseDoubleClick(e, service);
			window.ShowDialog();
		}

		private static void FireAppBuildMessagesOnMouseDoubleClick(MouseButtonEventArgs e,
			MockBuildService service)
		{
			if (e.LeftButton != MouseButtonState.Released)
				service.ReceiveData(new AppBuildResult("MockProject", PlatformName.Windows)
				{
					PackageFileName = "MockProject.app",
				});
			else if (e.RightButton != MouseButtonState.Released)
				service.ReceiveData(new AppBuildFailed("Demo message for failed build of a project"));
		}
	}
}