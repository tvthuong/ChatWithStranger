using ChatLa.Client.Helpers;
using ChatLa.Client.Models;
using Microsoft.AspNet.SignalR.Client;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ChatLa.Client.ViewModels
{
    public class FindRoomPageViewModel : ViewModelBase
    {
        private IHubProxy hubproxy;
        private HubConnection connection;
        private string _txtNotify;
        private Common_Library.Type type;
        private bool _isLogIn;
        private List<IDisposable> EventList { get; set; }
        public RoomType Selected { get; set; }
        public ObservableCollection<RoomType> RoomTypes { get; set; }
        public string txtNotify
        {
            get => _txtNotify;
            set => SetProperty(ref _txtNotify, value);
        }
        public ICommand cmdFindRoom { get; set; }
        public override bool IsBusy
        {
            get => base.IsBusy;
            set
            {
                base.IsBusy = value;
                ((DelegateCommand)cmdFindRoom).RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(IsEnabled));
                RaisePropertyChanged(nameof(IsDisabled));
            }
        }
        public bool IsLogIn
        {
            get => _isLogIn;
            set
            {
                SetProperty(ref _isLogIn, value);
                RaisePropertyChanged(nameof(IsEnabled));
                RaisePropertyChanged(nameof(IsDisabled));
                ((DelegateCommand)cmdFindRoom).RaiseCanExecuteChanged();
            }
        }
        public bool IsEnabled => IsNotBusy && IsLogIn;
        public bool IsDisabled => !IsEnabled;
        public FindRoomPageViewModel(List<Tuple<HubConnection, IHubProxy>> list, INavigationService navigationService) : base(navigationService)
        {
            Title = new TranslateExtension() { Text = "FindRoomPageTitle" }.ProvideValue(null).ToString();
            txtNotify = "";
            EventList = new List<IDisposable>();
            RoomType select = new RoomType() { Type = Common_Library.Type.Public };
            RoomTypes = new ObservableCollection<RoomType>() { select, new RoomType() { Type = Common_Library.Type.Private } };
            Selected = select;
            cmdFindRoom = new DelegateCommand(FindRoom, canExecute);
            connection = list[0].Item1;
            hubproxy = list[0].Item2;
            hubproxy.On("Connected", LogIn);
            hubproxy.On<Common_Library.Type>("Ready", Ready);
            hubproxy.On("Wait", Wait);
            hubproxy.On("AccountNotExist", AccountNotExist);
            hubproxy.On("LogInComplete", LogInComplete);
            hubproxy.On("ChatNameIsExist", ChatNameIsExist);
            if (!CurrentSettings.Settings.IsHasChatName)
                OnNavigatedTo(null);
        }

        private void ChatNameIsExist()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var mainPage = App.Current.MainPage as MyTabbedPage;
                DependencyService.Get<IToastMessage>().LongTime(new TranslateExtension() { Text = "ChatNameExistMessage" }.ProvideValue(null).ToString());
                mainPage.IsHidden = false;
                IsBusy = false;
            });
        }

        private void LogInComplete()
        {
            IsLogIn = true;
        }
        private void AccountNotExist()
        {
            CurrentSettings.Settings.DeleteAccount();
            IsLogIn = true;
        }

        private async void LogIn()
        {
            if(CurrentSettings.Settings.IsHasAccount)
                await hubproxy.Invoke("LogInAutomatically", new Common_Library.AccountBase() { UserName = CurrentSettings.Settings.UserName, Password = CurrentSettings.Settings.Password });
            else
                IsLogIn = true;
        }
        private void InviteFriendRequest(string sender, string ConnectionId)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                TranslateExtension translate = new TranslateExtension();
                translate.Text = "AlertInviteTitle";
                string title = translate.ProvideValue(null).ToString();
                translate.Text = "AlertInviteMessage";
                string message = string.Format("{0} {1}", sender, translate.ProvideValue(null).ToString());
                translate.Text = "AlertAccept";
                string accept = translate.ProvideValue(null).ToString();
                translate.Text = "AlertCancel";
                string cancel = translate.ProvideValue(null).ToString();
                bool result = await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
                await hubproxy.Invoke("InviteFriendResponse", ConnectionId, result, CurrentSettings.Settings.ChatName);
            });
        }
        private void InviteFriendResponse(string ConnectionId, string UserName, bool result, Common_Library.Type roomtype)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (result)
                {
                    txtNotify = "";
                    type = roomtype;
                    await NavigationService.NavigateAsync("ChatRoom");
                    IsBusy = false;
                }
            });
        }
        private void RoomIsFull()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DependencyService.Get<IToastMessage>().LongTime(new TranslateExtension() { Text = "RoomIsFullMessage" }.ProvideValue(null).ToString());
            });
        }
        private void Wait()
        {
            txtNotify = new TranslateExtension() { Text = "txtNotifyText" }.ProvideValue(null).ToString();
            if (EventList.Count == 0)
            {
                EventList.Add(hubproxy.On<string, string>("InviteFriendRequest", InviteFriendRequest));
                EventList.Add(hubproxy.On("RoomIsFull", RoomIsFull));
                EventList.Add(hubproxy.On<string, string, bool, Common_Library.Type>("InviteFriendResponse", InviteFriendResponse));
                EventList.Add(hubproxy.On<string, string, string>("Chatrequest", Chatrequest));
                EventList.Add(hubproxy.On<string, string, bool>("ChatWithFriendResponse", ChatWithFriendResponse));
            }
        }

        private void Ready(Common_Library.Type type)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                txtNotify = "";
                this.type = type;
                await NavigationService.NavigateAsync("ChatRoom");
                IsBusy = false;
            });
        }
        private bool canExecute()
        {
            return IsEnabled;
        }

        private async void FindRoom()
        {
            IsBusy = true;
            var mainPage = App.Current.MainPage as MyTabbedPage;
            mainPage.IsHidden = true;
            try
            {
                await hubproxy.Invoke("FindRoom", CurrentSettings.Settings.ChatName, Selected.Type);
            }
            catch (Exception)
            {
                DependencyService.Get<IToastMessage>().LongTime(new TranslateExtension() { Text = "ConnectionFailedMessage" }.ProvideValue(null).ToString());
                mainPage.IsHidden = false;
                IsBusy = false;
            }
        }
        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            try
            {
                if (connection.State == ConnectionState.Disconnected)
                    await connection.Start();
            }
            catch (Exception) { }
            if (parameters != null && parameters.ContainsKey("EventList"))
            {
                List<IDisposable> list = (List<IDisposable>)parameters["EventList"];
                foreach (IDisposable disposable in list)
                    disposable.Dispose();
                await hubproxy.Invoke("LeaveRoom");
            }
            IsBusy = false;
        }
        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            parameters.Add("ChatHub", hubproxy);
            parameters.Add("Connection", connection);
            parameters.Add("Type", type);
            foreach (var e in EventList)
                e.Dispose();
            EventList.Clear();
        }
        private void ChatWithFriendResponse(string ConnectionId, string UserName, bool result)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (result)
                {
                    txtNotify = "";
                    type = Common_Library.Type.Private;
                    await NavigationService.NavigateAsync("ChatRoom");
                    IsBusy = false;
                }
            });
        }
        private void Chatrequest(string sender, string ConnectionId, string senderchatname)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                TranslateExtension translate = new TranslateExtension();
                translate.Text = "AlertTitle";
                string title = translate.ProvideValue(null).ToString();
                translate.Text = "AlertMessage";
                string message = string.Format("{0} {1}", sender, translate.ProvideValue(null).ToString());
                translate.Text = "AlertAccept";
                string accept = translate.ProvideValue(null).ToString();
                translate.Text = "AlertCancel";
                string cancel = translate.ProvideValue(null).ToString();
                bool result = await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
                await hubproxy.Invoke("ChatWithFriendResponse", ConnectionId, sender, result, senderchatname, CurrentSettings.Settings.ChatName);
            });
        }
    }
}
