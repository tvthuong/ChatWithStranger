using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNet.SignalR.Client;
using Prism.Navigation;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Prism.Commands;
using Common_Library;
using Xamarin.Forms;
using ChatLa.Client.Helpers;

namespace ChatLa.Client.ViewModels
{
    class MemberListPageViewModel : ViewModelBase
    {
        private IHubProxy hubproxy;
        private HubConnection connection;
        private ObservableCollection<Common_Library.Member> _members;
        private bool _isRequest;
        private string _txtNotify;
        private bool _canInviteFriend;

        public ICommand cmdAddFriendrequest { get; set; }
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
        public bool CanInviteFriend
        {
            get => _canInviteFriend;
            set
            {
                SetProperty(ref _canInviteFriend, value);
                ((DelegateCommand)cmdInviteFriend).RaiseCanExecuteChanged();
            }
        }
        public bool IsEnabled => IsNotBusy && !IsRequest ? true : false;
        public bool IsDisabled => !IsEnabled;
        public ICommand cmdInviteFriend { get; set; }
        public ICommand cmdRemoveMember { get; set; }
        public ICommand cmdPrivatechatrequest { get; set; }
        public ICommand cmdRefresh { get; set; }
        private List<IDisposable> EventList { get; set; }
        public ObservableCollection<Common_Library.Member> Members
        {
            get => _members;
            set => SetProperty(ref _members, value);
        }
        public override bool IsBusy
        {
            get => base.IsBusy;
            set
            {
                base.IsBusy = value;
                ((DelegateCommand)cmdRefresh).RaiseCanExecuteChanged();
                ((DelegateCommand)cmdInviteFriend).RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(IsEnabled));
                RaisePropertyChanged(nameof(IsDisabled));
            }
        }
        public MemberListPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            Title = new TranslateExtension() { Text = "ToolbarItemMember" }.ProvideValue(null).ToString();
            txtNotify = "";
            Members = new ObservableCollection<Common_Library.Member>();
            EventList = new List<IDisposable>();
            cmdInviteFriend = new DelegateCommand(InviteFriend, CanExecuteInviteFriend);
            cmdRefresh = new DelegateCommand(GetMemberList, canExecute);
            cmdRemoveMember = new DelegateCommand<Common_Library.Member>(RemoveMember);
            cmdPrivatechatrequest = new DelegateCommand<Common_Library.Member>(Privatechatrequest);
            cmdAddFriendrequest = new DelegateCommand<Common_Library.Member>(AddFriendrequest);
        }

        private bool CanExecuteInviteFriend()
        {
            return CanInviteFriend && IsNotBusy;
        }

        private async void InviteFriend()
        {
            IsBusy = true;
            await NavigationService.NavigateAsync("InviteFriend");
            IsBusy = false;
        }
        private async void AllowInviteFriendChanged(string connectionid, bool allowinvitefriend)
        {
            IsBusy = true;
            await hubproxy.Invoke("AllowInviteFriendChanged", connectionid, allowinvitefriend);
            IsBusy = false;
        }

        private async void RemoveMember(Member member)
        {
            IsBusy = true;
            TranslateExtension translate = new TranslateExtension();
            translate.Text = "RemoveMemberTitle";
            string title = translate.ProvideValue(null).ToString();
            translate.Text = "RemoveMemberMessage";
            string message = translate.ProvideValue(null).ToString();
            translate.Text = "AlertAccept";
            string accept = translate.ProvideValue(null).ToString();
            translate.Text = "AlertCancel";
            string cancel = translate.ProvideValue(null).ToString();
            bool result = await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
            if (result)
                await hubproxy.Invoke("LeaveRoom", member.ConnectionId);
            else
                IsBusy = false;
        }

        private async void AddFriendrequest(Member member)
        {
            IsRequest = true;
            txtNotify = new TranslateExtension() { Text = "txtNotifyRequestText" }.ProvideValue(null).ToString();
            await hubproxy.Invoke("AddFriendrequest", member.ConnectionId);
        }

        private async void Privatechatrequest(Common_Library.Member member)
        {
            IsRequest = true;
            txtNotify = new TranslateExtension() { Text = "txtNotifyRequestText" }.ProvideValue(null).ToString();
            await hubproxy.Invoke("Privatechatrequest", member.ConnectionId);
        }

        private bool canExecute()
        {
            return IsNotBusy;
        }

        private async void GetMemberList()
        {
            IsBusy = true;
            Device.BeginInvokeOnMainThread(() =>
            {
                Members.Clear();
                txtNotify = "";
            });
            try
            {
                await hubproxy.Invoke("GetMemberList");
            }
            catch (Exception) { IsBusy = false; }
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
                    EventList.Add(hubproxy.On<User, bool>("Response", Response));
                    EventList.Add(hubproxy.On<User, bool>("AddFriendResponse", AddFriendResponse));
                    EventList.Add(hubproxy.On<ObservableCollection<Common_Library.Member>>("GetMemberList", GetMemberList));
                    EventList.Add(hubproxy.On<bool>("AllowInviteFriendChanged", AllowInviteFriendChanged));
                }
            }
            else if (parameters.ContainsKey("EventList"))
            {
                List<IDisposable> list = (List<IDisposable>)parameters["EventList"];
                foreach (IDisposable disposable in list)
                    disposable.Dispose();
            }
            cmdRefresh.Execute(null);
        }

        private void AllowInviteFriendChanged(bool caninvitefriend)
        {
            CanInviteFriend = caninvitefriend;
        }

        private void AddFriendResponse(User user, bool result)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (result)
                    GetMemberList();
                else if (connection.ConnectionId != user.ConnectionId)
                    txtNotify = string.Format("({0} - {1}) {2}", user.ChatName, user.Account.UserName, new TranslateExtension() { Text = "DenyMessage" }.ProvideValue(null).ToString());
                IsRequest = false;
            });
        }

        private void Response(User user, bool result)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (result)
                {
                    txtNotify = "";
                    IsRequest = false;
                }
                else
                {
                    if (connection.ConnectionId != user.ConnectionId)
                    {
                        txtNotify = string.Format("{0} {1}", user.ChatName, new TranslateExtension() { Text = "DenyMessage" }.ProvideValue(null).ToString());
                        IsRequest = false;
                    }
                }
            });
        }

        private void GetMemberList(ObservableCollection<Member> list)
        {
            Device.BeginInvokeOnMainThread(async() =>
            {
                foreach (var e in list)
                    if(e.IsVisibleAllowInviteFriend)
                        e.Action = new Action<string, bool>(AllowInviteFriendChanged);
                Members = list;
                txtNotify = "";
                CanInviteFriend = await hubproxy.Invoke<bool>("AllowInviteFriend");
                IsBusy = false;
            });
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            parameters.Add("EventList", EventList);
            parameters.Add("ChatHub", hubproxy);
            parameters.Add("Connection", connection);
        }
    }
}
