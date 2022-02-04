using Autofac;
using Caliburn.Micro.Autofac;
using MessengerAppClient.Content.ViewModels;
using MessengerAppClient.Login.ViewModels;
using MessengerAppClient.Shell.Models;
using MessengerAppClient.Shell.ViewModels;
using System.Windows;

namespace MessengerAppClient
{
    public class Bootstrapper : AutofacBootstrapper<ShellViewModel>
    {
        public Bootstrapper()
        {
            Initialize();
        }

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            // An IoC container is created and used to facilitate the dependency injection

            // ViewModel which is loaded by the bootstrapper
            builder.RegisterType<ShellViewModel>().SingleInstance();
            builder.RegisterType<SocketHandler>().SingleInstance();

            // ViewModels for the Login
            builder.RegisterType<LoginConductorViewModel>().SingleInstance();
            builder.RegisterType<LoginViewModel>().SingleInstance();
            builder.RegisterType<SignupViewModel>().SingleInstance();

            // ViewModels for the Messaging
            builder.RegisterType<ContentConductorViewModel>().SingleInstance();
            builder.RegisterType<HomeViewModel>().SingleInstance();
            builder.RegisterType<SettingsViewModel>().SingleInstance();
            builder.RegisterType<SideBarViewModel>().SingleInstance();
        }

        protected override void ConfigureBootstrapper()
        {
            base.ConfigureBootstrapper();
            EnforceNamespaceConvention = false;
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            // Note:
            //     DisplayRootViewFor<T> was changed in CaliburnMicro>=v4.0 so v3.2.0 is used instead
            //     See caliburnmicro.com/documentation/bootstrapper
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
