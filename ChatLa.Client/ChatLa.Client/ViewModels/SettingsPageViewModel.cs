using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatLa.Client.Helpers;
using Microsoft.AspNet.SignalR.Client;
using Prism.Commands;
using Prism.Navigation;
using Xamarin.Forms;
namespace ChatLa.Client.ViewModels
{
    class SettingsPageViewModel : ViewModelBase
    {
        private bool IsFirstRun { get; set; }
        private List<Tuple<HubConnection, IHubProxy>> List;
        public ICommand cmdChangePassword { get; set; }
        public ICommand cmdLogOut { get; set; }
        public override bool IsBusy
        {
            get => base.IsBusy;
            set
            {
                base.IsBusy = value;
                ((DelegateCommand)cmdLogIn).RaiseCanExecuteChanged();
                ((DelegateCommand)cmdLogOut).RaiseCanExecuteChanged();
                ((DelegateCommand)cmdChangePassword).RaiseCanExecuteChanged();
            }
        }
        public ICommand cmdLogIn { get; set; }
        public SettingsPageViewModel(List<Tuple<HubConnection, IHubProxy>>  list, INavigationService navigationService) : base(navigationService)
        {
            Title = new TranslateExtension() { Text = "SettingsPageTitle" }.ProvideValue(null).ToString();
            cmdLogIn = new DelegateCommand(LogIn,CanExecute);
            cmdLogOut = new DelegateCommand(LogOut, CanExecute);
            cmdChangePassword = new DelegateCommand(ChangePassword, CanExecute);
            List = list;
            if (CurrentSettings.Settings.IsHasChatName)
                OnNavigatedTo(null);
            else
                IsFirstRun = true;
        }

        private void LogOut()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                IsBusy = true;
                TranslateExtension translate = new TranslateExtension();
                translate.Text = "LogOutTitle";
                string title = translate.ProvideValue(null).ToString();
                translate.Text = "LogOutMessage";
                string message = translate.ProvideValue(null).ToString();
                translate.Text = "AlertAccept";
                string accept = translate.ProvideValue(null).ToString();
                translate.Text = "AlertCancel";
                string cancel = translate.ProvideValue(null).ToString();
                bool result = await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
                if (result)
                {
                    foreach (var e in List)
                        try
                        {
                            await e.Item2.Invoke("LogOut");
                        }
                        catch (Exception)
                        {
                            DependencyService.Get<IToastMessage>().LongTime(new TranslateExtension() { Text = "ConnectionFailedMessage" }.ProvideValue(null).ToString());
                            IsBusy = false;
                            return;
                        }
                    CurrentSettings.Settings.DeleteAccount();
                }
                IsBusy = false;
            });
        }

        private async void ChangePassword()
        {
            IsBusy = true;
            await NavigationService.NavigateAsync("LogIn?ChangePassword=true");
            IsBusy = false;
        }

        private async void LogIn()
        {
            IsBusy = true;
            await NavigationService.NavigateAsync("LogIn");
            IsBusy = false;
        }

        private bool CanExecute()
        {
            return IsNotBusy;
        }

        public async override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if(!CurrentSettings.Settings.IsHasChatName)
            {
                var mainPage = App.Current.MainPage as MyTabbedPage;
                if (mainPage != null)
                {
                    if (IsFirstRun)
                    {
                        await Task.Delay(500);
                        IsFirstRun = false;
                    }
                    mainPage.IsHidden = true;
                }
            }
            IsBusy = false;
        }
    }
}
