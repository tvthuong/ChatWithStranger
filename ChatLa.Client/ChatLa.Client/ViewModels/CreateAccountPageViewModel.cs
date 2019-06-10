using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using ChatLa.Client.Helpers;
using Microsoft.AspNet.SignalR.Client;
using Prism.Commands;
using Prism.Navigation;
using Xamarin.Forms;

namespace ChatLa.Client.ViewModels
{
    class CreateAccountPageViewModel : ViewModelBase
    {
        private List<Tuple<HubConnection, IHubProxy>> List;
        private string _userName;
        private string _password;
        private string _confirmPassword;
        private List<IDisposable> EventList { get; set; }
        public string UserName
        {
            get => _userName;
            set
            {
                SetProperty(ref _userName, value);
                ((DelegateCommand)cmdCreateAccount).RaiseCanExecuteChanged();
            }
        }
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                ((DelegateCommand)cmdCreateAccount).RaiseCanExecuteChanged();
            }
        }
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                SetProperty(ref _confirmPassword, value);
                ((DelegateCommand)cmdCreateAccount).RaiseCanExecuteChanged();
            }
        }
        public override bool IsBusy
        {
            get => base.IsBusy;
            set
            {
                base.IsBusy = value;
                ((DelegateCommand)cmdCreateAccount).RaiseCanExecuteChanged();
            }
        }
        public ICommand cmdCreateAccount { get; set; }
        public CreateAccountPageViewModel(List<Tuple<HubConnection, IHubProxy>> list, INavigationService navigationService) : base(navigationService)
        {
            List = list;
            Title = new Helpers.TranslateExtension() { Text = "btnCreateAccountText" }.ProvideValue(null).ToString();
            cmdCreateAccount = new DelegateCommand(CreateAccount, canExecute);
            UserName = "";
            Password = "";
            ConfirmPassword = "";
            EventList = new List<IDisposable>();
        }
        private bool canExecute()
        {
            return IsNotBusy && Password == ConfirmPassword && Regex.IsMatch(Password, "^\\w{8,30}$") && Regex.IsMatch(UserName, "^\\w{8,30}$");
        }

        private async void CreateAccount()
        {
            IsBusy = true;
            try
            {
                await List[0].Item2.Invoke("CreateAccount", new Common_Library.AccountBase() { UserName = UserName, Password = Password });
            }catch(Exception)
            {
                DependencyService.Get<IToastMessage>().LongTime(new TranslateExtension() { Text = "ConnectionFailedMessage" }.ProvideValue(null).ToString());
                IsBusy = false;
            }
        }
        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            foreach (var e in EventList)
                e.Dispose();
            EventList.Clear();
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (EventList.Count <= 0)
            {
                EventList.Add(List[0].Item2.On("CreateAccountSuccessfully", CreateAccountSuccessfully));
                EventList.Add(List[0].Item2.On("CreateAccountFailed", CreateAccountFailed));
            }
            IsBusy = false;
        }

        private void CreateAccountFailed()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DependencyService.Get<IToastMessage>().LongTime(new TranslateExtension() { Text = "CreateAccountFailedMessage" }.ProvideValue(null).ToString());
                IsBusy = false;
            });
        }

        private void CreateAccountSuccessfully()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                foreach (var l in List)
                    await l.Item2.Invoke("LogInAutomatically", new Common_Library.AccountBase() { UserName = UserName, Password = Password });
                CurrentSettings.Settings.UserName = UserName;
                CurrentSettings.Settings.Password = Password;
                ((MyTabbedPage)App.Current.MainPage).IsHidden = false;
                await NavigationService.GoBackToRootAsync();
            });
        }
    }
}
