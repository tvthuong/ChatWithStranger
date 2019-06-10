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
    class LogInPageViewModel : ViewModelBase
    {
        private string _userName;
        private string _password;
        private List<Tuple<HubConnection, IHubProxy>> List;
        private string _newPassword;
        private string _confirmPassword;
        private bool _isChangePassword;

        private List<IDisposable> EventList { get; set; }
        public bool IsLogIn => !IsChangePassword;
        public bool IsChangePassword
        {
            get => _isChangePassword;
            set
            {
                SetProperty(ref _isChangePassword, value);
                RaisePropertyChanged(nameof(IsLogIn));
            }
        }
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                SetProperty(ref _confirmPassword, value);
                ((DelegateCommand)cmdChangePassword).RaiseCanExecuteChanged();
            }
        }
        public string NewPassword
        {
            get => _newPassword;
            set
            {
                SetProperty(ref _newPassword, value);
                ((DelegateCommand)cmdChangePassword).RaiseCanExecuteChanged();
            }
        }
        public string UserName
        {
            get => _userName;
            set
            {
                SetProperty(ref _userName, value);
                ((DelegateCommand)cmdLogIn).RaiseCanExecuteChanged();
            }
        }
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                ((DelegateCommand)cmdLogIn).RaiseCanExecuteChanged();
                ((DelegateCommand)cmdChangePassword).RaiseCanExecuteChanged();
            }
        }
        public ICommand cmdChangePassword { get; set; }
        public ICommand cmdLogIn { get; set; }
        public ICommand cmdCreateAccount { get; set; }
        public override bool IsBusy
        {
            get => base.IsBusy;
            set
            {
                base.IsBusy = value;
                ((DelegateCommand)cmdLogIn).RaiseCanExecuteChanged();
                ((DelegateCommand)cmdCreateAccount).RaiseCanExecuteChanged();
                ((DelegateCommand)cmdChangePassword).RaiseCanExecuteChanged();
            }
        }
        public LogInPageViewModel(List<Tuple<HubConnection, IHubProxy>> list, INavigationService navigationService) : base(navigationService)
        {
            List = list;
            cmdLogIn = new DelegateCommand(LogIn, CanLogIn);
            cmdChangePassword = new DelegateCommand(ChangePassword, CanChangePassword);
            cmdCreateAccount = new DelegateCommand(CreateAccount, CanExecute);
            UserName = "";
            Password = "";
            NewPassword = "";
            ConfirmPassword = "";
            EventList = new List<IDisposable>();
        }

        private bool CanChangePassword()
        {
            return IsNotBusy && NewPassword != Password && NewPassword == ConfirmPassword && Regex.IsMatch(Password, "^\\w{8,30}$") && Regex.IsMatch(NewPassword, "^\\w{8,30}$");
        }

        private async void ChangePassword()
        {
            IsBusy = true;
            try
            {
                await List[0].Item2.Invoke("ChangePassword", new Common_Library.AccountBase() { UserName = CurrentSettings.Settings.UserName, Password = Password }, NewPassword);
            }
            catch (Exception)
            {
                DependencyService.Get<IToastMessage>().LongTime(new TranslateExtension() { Text = "ConnectionFailedMessage" }.ProvideValue(null).ToString());
                IsBusy = false;
            }
        }

        public override async void GoBackToRootAsync()
        {
            IsBusy = true;
            base.GoBackToRootAsync();
            await NavigationService.GoBackAsync();
        }
        private bool CanLogIn()
        {
            return CanExecute() && !string.IsNullOrEmpty(UserName) && !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password);
        }

        private async void CreateAccount()
        {
            IsBusy = true;
            await NavigationService.NavigateAsync("CreateAccount");
            IsBusy = false;
        }

        private bool CanExecute()
        {
            return IsNotBusy;
        }

        private async void LogIn()
        {
            IsBusy = true;
            try
            {
                await List[0].Item2.Invoke("LogIn", new Common_Library.AccountBase() { UserName = UserName, Password = Password });
            }
            catch (Exception)
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
            if(parameters.ContainsKey("ChangePassword"))
                IsChangePassword = bool.Parse(parameters["ChangePassword"].ToString());
            if(IsChangePassword)
                Title = new Helpers.TranslateExtension() { Text = "btnChangePasswordText" }.ProvideValue(null).ToString();
            else
                Title = new Helpers.TranslateExtension() { Text = "btnLogInText" }.ProvideValue(null).ToString();
            if (EventList.Count <= 0)
            {
                EventList.Add(List[0].Item2.On("ChangePasswordComplete", ChangePasswordComplete));
                EventList.Add(List[0].Item2.On("PasswordIsNotValid", PasswordIsNotValid));
                EventList.Add(List[0].Item2.On("LogInSuccessfully", LogInSuccessfully));
                EventList.Add(List[0].Item2.On("LogInFailed", LogInFailed));
            }
            IsBusy = false;
        }

        private void ChangePasswordComplete()
        {
            Device.BeginInvokeOnMainThread(async() =>
            {
                CurrentSettings.Settings.Password = NewPassword;
                await NavigationService.GoBackAsync();
            });
        }

        private void PasswordIsNotValid()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DependencyService.Get<IToastMessage>().LongTime(new TranslateExtension() { Text = "PasswordIsNotValidMessage" }.ProvideValue(null).ToString());
                IsBusy = false;
            });
        }

        private void LogInFailed()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DependencyService.Get<IToastMessage>().LongTime(new TranslateExtension() { Text = "LogInFailedMessage" }.ProvideValue(null).ToString());
                IsBusy = false;
            });
        }

        private void LogInSuccessfully()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                CurrentSettings.Settings.UserName = UserName;
                CurrentSettings.Settings.Password = Password;
                foreach (var l in List)
                    await l.Item2.Invoke("LogInAutomatically", new Common_Library.AccountBase() { UserName = UserName, Password = Password });
                await NavigationService.GoBackAsync();
            });
        }
    }
}
