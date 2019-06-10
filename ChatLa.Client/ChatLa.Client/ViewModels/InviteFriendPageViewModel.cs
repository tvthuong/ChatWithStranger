using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using ChatLa.Client.Helpers;
using Common_Library;
using Microsoft.AspNet.SignalR.Client;
using Prism.Commands;
using Prism.Navigation;
using Xamarin.Forms;

namespace ChatLa.Client.ViewModels
{
    class InviteFriendPageViewModel : ViewModelBase
    {
        private IHubProxy hubproxy;
        private HubConnection connection;
        private bool _isRequest;
        private string _txtNotify;
        private ObservableCollection<AccountBase> _onlineFriend;

        public ObservableCollection<Common_Library.AccountBase> OnlineFriend
        {
            get => _onlineFriend;
            set => SetProperty(ref _onlineFriend, value);
        }
        public string txtNotify
        {
            get => _txtNotify;
            set => SetProperty(ref _txtNotify, value);
        }
        public bool IsRequest
        {
            get => _isRequest;
            set
            {
                SetProperty(ref _isRequest, value);
                RaisePropertyChanged(nameof(IsEnabled));
                RaisePropertyChanged(nameof(IsDisabled));
            }
        }
        public bool IsEnabled => IsNotBusy && !IsRequest ? true : false;
        public bool IsDisabled => !IsEnabled;
        private List<IDisposable> EventList { get; set; }
        public override bool IsBusy
        {
            get => base.IsBusy;
            set
            {
                base.IsBusy = value;
                ((DelegateCommand)cmdRefresh).RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(IsEnabled));
                RaisePropertyChanged(nameof(IsDisabled));
            }
        }
        public ICommand cmdRefresh { get; set; }
        public ICommand cmdInviteFriend { get; set; }

        public InviteFriendPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            Title = new Helpers.TranslateExtension() { Text = "ToolbarItemMember2" }.ProvideValue(null).ToString();
            txtNotify = "";
            EventList = new List<IDisposable>();
            OnlineFriend = new ObservableCollection<AccountBase>();
            cmdInviteFriend = new DelegateCommand<Common_Library.AccountBase>(InviteFriend);
            cmdRefresh = new DelegateCommand(GetOnlineFriend, canExecute);
        }

        private async void InviteFriend(AccountBase account)
        {
            IsRequest = true;
            txtNotify = new TranslateExtension() { Text = "txtNotifyRequestText" }.ProvideValue(null).ToString();
            await hubproxy.Invoke("InviteFriendRequest", account.UserName);
        }

        private bool canExecute()
        {
            return IsNotBusy;
        }

        private void GetOnlineFriend()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                IsBusy = true;
                Device.BeginInvokeOnMainThread(() =>
                {
                    OnlineFriend.Clear();
                    txtNotify = "";
                });
                try
                {
                    if (!await hubproxy.Invoke<bool>("AllowInviteFriend"))
                    {
                        await NavigationService.GoBackAsync();
                        return;
                    }
                    await hubproxy.Invoke("GetOnlineFriend");
                }
                catch (Exception) { IsBusy = false; }
            });
        }
        private void FriendIsOffline()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DependencyService.Get<IToastMessage>().LongTime(new TranslateExtension() { Text = "FriendIsOfflineMessage" }.ProvideValue(null).ToString());
                GetOnlineFriend();
                IsRequest = false;
            });
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("Connection") && parameters.ContainsKey("ChatHub"))
            {
                connection = (HubConnection)parameters["Connection"];
                hubproxy = (IHubProxy)parameters["ChatHub"];
                if (EventList.Count == 0)
                {
                    EventList.Add(hubproxy.On("FriendIsOffline", FriendIsOffline));
                    EventList.Add(hubproxy.On("Response", GetOnlineFriend));
                    EventList.Add(hubproxy.On("ChatWithFriendResponse", GetOnlineFriend));
                    EventList.Add(hubproxy.On("NewMember", GetOnlineFriend));
                    EventList.Add(hubproxy.On("MemberExit", GetOnlineFriend));
                    EventList.Add(hubproxy.On<string, string, bool, Common_Library.Type>("InviteFriendResponse", InviteFriendResponse));
                    EventList.Add(hubproxy.On<ObservableCollection<Common_Library.AccountBase>>("GetOnlineFriend", GetOnlineFriend));
                    EventList.Add(hubproxy.On<bool>("AllowInviteFriendChanged", AllowInviteFriendChanged));
                }
            }
            cmdRefresh.Execute(null);
        }

        private void InviteFriendResponse(string ConnectionId, string UserName, bool result, Common_Library.Type roomtype)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (result)
                {
                    if (connection.ConnectionId != ConnectionId)
                    {
                        txtNotify = "";
                        IsRequest = false;
                    }
                    else
                        GetOnlineFriend();
                }
                else if (connection.ConnectionId != ConnectionId)
                {
                    txtNotify = string.Format("{0} {1}", UserName, new TranslateExtension() { Text = "DenyMessage" }.ProvideValue(null).ToString());
                    IsRequest = false;
                }
            });
        }

        private void GetOnlineFriend(ObservableCollection<AccountBase> list)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                OnlineFriend = list;
                IsBusy = false;
            });
        }

        private void AllowInviteFriendChanged(bool caninvitefriend)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (!caninvitefriend)
                {
                    IsBusy = true;
                    await NavigationService.GoBackAsync();
                }
            });
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            parameters.Add("EventList", EventList);
        }
    }
}
