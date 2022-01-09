using Autofac;
using Caliburn.Micro.Autofac;
using MessengerAppClient.Content.ViewModels;
using MessengerAppClient.Login.ViewModels;
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
            builder.RegisterType<ShellViewModel>().SingleInstance();

            builder.RegisterType<LoginConductorViewModel>().SingleInstance();
            builder.RegisterType<LoginViewModel>().SingleInstance();
            builder.RegisterType<SignupViewModel>().SingleInstance();

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
            // DisplayRootViewFor<T> was changed in CaliburnMicro>=v4.0
            // See caliburnmicro.com/documentation/bootstrapper
            // So v3.2.0 must be used instead
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
