using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ChatLa.Client.Helpers;
using ChatLa.Client.Models;
using Common_Library;
using Microsoft.AspNet.SignalR.Client;
using Prism.Commands;
using Prism.Navigation;
using Xamarin.Forms;

namespace ChatLa.Client.ViewModels
{
    public class FriendListPageViewModel : ViewModelBase
    {
        private const int GroupTitleHeightAndroid = 100;
        private const int ItemHeightAndroid = 170;
        private const int GroupTitleHeightUWP = 50;
        private const int ItemHeightUWP = 85;
        private const int GroupTitleHeightiOS = 65;
        private const int ItemHeightiOS = 115;
        private ObservableCollection<GroupObservableCollection> _friends;
        private IHubProxy hubproxy;
        private HubConnection connection;
        private bool _isRequest;
        private Common_Library.Type type;
        private bool _isLogIn;
        private string _txtNotify;
        private ObservableCollection<GroupObservableCollection> _suggestsAddFriend;
        private ObservableCollection<GroupObservableCollection> _addFriendRequests;
        private List<IDisposable> EventList { get; set; }
        public int GroupTitleHeight
        {
            get
            {
                if (Device.RuntimePlatform == Device.Android)
                    return GroupTitleHeightAndroid;
                if (Device.RuntimePlatform == Device.UWP)
                    return GroupTitleHeightUWP;
                return GroupTitleHeightiOS;
            }
        }
        public int ItemHeight
        {
            get
            {
                if (Device.RuntimePlatform == Device.Android)
                    return ItemHeightAndroid;
                if (Device.RuntimePlatform == Device.UWP)
                    return ItemHeightUWP;
                return ItemHeightiOS;
            }
        }
        public string txtNotify
        {
            get => _txtNotify;
            set => SetProperty(ref _txtNotify, value);
        }
        public ICommand cmdAcceptAddFriendrequest { get; set; }
        public ICommand cmdDenyAddFriendrequest { get; set; }
        public ICommand cmdAddFriendrequest { get; set; }
        public ICommand cmdChatrequest { get; set; }
        public ICommand cmdDeleteFriend { get; set; }
        public override bool IsBusy
        {
            get => base.IsBusy;
            set
            {
                base.IsBusy = value;
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
            }
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
        public bool IsEnabled => IsNotBusy && IsLogIn && !IsRequest;
        public bool IsDisabled => !IsEnabled;
        public int AddFriendRequestsHeightRequest
        {
            get
            {
                if (AddFriendRequests.Count == 0)
                    return GroupTitleHeight;
                return ItemHeight * AddFriendRequests[0].Count + GroupTitleHeight;
            }
        }
        public ObservableCollection<GroupObservableCollection> AddFriendRequests
        {
            get => _addFriendRequests;
            set
            {
                SetProperty(ref _addFriendRequests, value);
                RaisePropertyChanged(nameof(AddFriendRequestsHeightRequest));
            }
        }
        public int HeightRequest
        {
            get
            {
                if(SuggestsAddFriend.Count == 0)
                    return GroupTitleHeight;
                return ItemHeight * SuggestsAddFriend[0].Count + GroupTitleHeight;
            }
        }
        public ObservableCollection<GroupObservableCollection> SuggestsAddFriend
        {
            get => _suggestsAddFriend;
            set
            {
                SetProperty(ref _suggestsAddFriend, value);
                RaisePropertyChanged(nameof(HeightRequest));
            }
        }
        public ObservableCollection<GroupObservableCollection> Friends
        {
            get => _friends;
            set => SetProperty(ref _friends, value);
        }
        public ICommand cmdRefresh { get; set; }
        public FriendListPageViewModel(List<Tuple<HubConnection, IHubProxy>> list, INavigationService navigationService) : base(navigationService)
        {
            Title = new TranslateExtension() { Text = "FriendListPageTitle" }.ProvideValue(null).ToString();
            AddFriendRequests = new ObservableCollection<GroupObservableCollection>();
            SuggestsAddFriend = new ObservableCollection<GroupObservableCollection>();
            EventList = new List<IDisposable>();
            Friends = new ObservableCollection<GroupObservableCollection>();
            cmdAcceptAddFriendrequest = new DelegateCommand<Common_Library.AccountBase>(AcceptAddFriendrequest);
            cmdDenyAddFriendrequest = new DelegateCommand<Common_Library.AccountBase>(DenyAddFriendrequest);
            cmdAddFriendrequest = new DelegateCommand<Common_Library.AccountBase>(AddFriendrequest);
            cmdRefresh = new DelegateCommand(GetFriendList);
            cmdChatrequest = new DelegateCommand<Common_Library.Friend>(Chatrequest);
            cmdDeleteFriend = new DelegateCommand<Common_Library.Friend>(DeleteFriend);
            connection = list[2].Item1;
            hubproxy = list[2].Item2;
            hubproxy.On("Connected", LogIn);
            hubproxy.On("AccountNotExist", AccountNotExist);
            hubproxy.On<ObservableCollection<Common_Library.Friend>>("GetFriendList", GetFriendList);
            hubproxy.On("LogInComplete", LogInComplete);
            hubproxy.On("FriendIsOffline", FriendIsOffline);
            hubproxy.On("DeleteComplete", GetFriendList);
            hubproxy.On<ObservableCollection<Common_Library.AccountBase>>("GetSuggestsAddFriendList", GetSuggestsAddFriendList);
            hubproxy.On<ObservableCollection<Common_Library.AccountBase>>("GetAddFriendRequest", GetAddFriendRequest);
            OnNavigatedTo(null);
        }

        private void DenyAddFriendrequest(AccountBase account)
        {
            Answer(account.UserName, false);
        }

        private void AcceptAddFriendrequest(AccountBase account)
        {
            Answer(account.UserName, true);
        }

        private async void Answer(string username, bool result)
        {
            IsBusy = true;
            await hubproxy.Invoke("AddFriendFromSuggestResponse", username, result);
        }

        private void GetAddFriendRequest(ObservableCollection<AccountBase> list)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                AddFriendRequests.Clear();
                AddFriendRequests.Add(new GroupObservableCollection(list) { Heading = new TranslateExtension() { Text = "txtAddFriendRequest" }.ProvideValue(null).ToString() });
                RaisePropertyChanged(nameof(AddFriendRequestsHeightRequest));
                IsBusy = false;
            });
        }

        private async void AddFriendrequest(AccountBase account)
        {
            IsBusy = true;
            await hubproxy.Invoke("AddFriendRequestFromSuggest", account.UserName);
        }

        private void GetSuggestsAddFriendList(ObservableCollection<AccountBase> list)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                SuggestsAddFriend.Clear();
                SuggestsAddFriend.Add(new GroupObservableCollection(list) { Heading = new TranslateExtension() { Text = "txtSuggestAddFriend" }.ProvideValue(null).ToString() });
                RaisePropertyChanged(nameof(HeightRequest));
                IsBusy = false;
            });
        }

        private void DeleteFriend(Friend friend)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                IsBusy = true;
                TranslateExtension translate = new TranslateExtension();
                translate.Text = "DeleteFriendTitle";
                string title = translate.ProvideValue(null).ToString();
                translate.Text = "DeleteFriendMessage";
                string message = translate.ProvideValue(null).ToString();
                translate.Text = "AlertAccept";
                string accept = translate.ProvideValue(null).ToString();
                translate.Text = "AlertCancel";
                string cancel = translate.ProvideValue(null).ToString();
                bool result = await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
                if (result)
                    await hubproxy.Invoke("DeleteFriend", friend);
                IsBusy = false;
            });
        }

        private void ChatWithFriendResponse(string ConnectionId, string UserName, bool result)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (result)
                {
                    type = Common_Library.Type.Private;
                    var mainpage = (MyTabbedPage)App.Current.MainPage;
                    if (mainpage != null)
                    {
                        ((ViewModelBase)((MyNavigationPage)mainpage.CurrentPage).CurrentPage.BindingContext).GoBackToRootAsync();
                        mainpage.CurrentPage = mainpage.Children[2];
                    }
                    await NavigationService.NavigateAsync("ChatRoom");
                }
                else if (connection.ConnectionId != ConnectionId)
                {
                    var mainpage = (MyTabbedPage)App.Current.MainPage;
                    if (mainpage != null)
                        mainpage.IsHidden = false;
                    txtNotify = string.Format("{0} {1}", UserName, new TranslateExtension() { Text = "DenyMessage" }.ProvideValue(null).ToString());
                }
                IsRequest = false;
            });
        }

        private void Chatrequest(string sender, string ConnectionId, string senderchatname)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                IsBusy = true;
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
                IsBusy = false;
            });
        }

        private void FriendIsOffline()
        {
            var mainpage = (MyTabbedPage)App.Current.MainPage;
            if (mainpage != null)
                mainpage.IsHidden = false;
            txtNotify = new TranslateExtension() { Text = "FriendIsOfflineMessage" }.ProvideValue(null).ToString();
            IsRequest = false;
        }

        private async void Chatrequest(Common_Library.Friend friend)
        {
            var mainpage = (MyTabbedPage)App.Current.MainPage;
            if (mainpage != null)
                mainpage.IsHidden = true;
            IsRequest = true;
            txtNotify = new TranslateExtension() { Text = "txtNotifyRequestText" }.ProvideValue(null).ToString();
            await hubproxy.Invoke("Chatrequest", friend.UserName, CurrentSettings.Settings.ChatName);
        }
        private void LogInComplete()
        {
            GetFriendList();
            IsLogIn = true;
        }

        private void GetFriendList(ObservableCollection<Friend> list)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Friends.Clear();
                TranslateExtension translate = new TranslateExtension() { Text = "OnlineFriend" };
                Friends.Add(new GroupObservableCollection(list.Where(f => f.IsOnline)) { Heading = translate.ProvideValue(null).ToString() });
                translate.Text = "OfflineFriend";
                Friends.Add(new GroupObservableCollection(list.Where(f => !f.IsOnline)) { Heading = translate.ProvideValue(null).ToString() });
                IsBusy = false;
            });
        }

        private void AccountNotExist()
        {
            CurrentSettings.Settings.DeleteAccount();
            IsLogIn = true;
        }
        private async void LogIn()
        {
            if (CurrentSettings.Settings.IsHasAccount)
                await hubproxy.Invoke("LogInAutomatically", new Common_Library.AccountBase() { UserName = CurrentSettings.Settings.UserName, Password = CurrentSettings.Settings.Password });
            else
                IsLogIn = true;
        }
        private async void GetFriendList()
        {
            IsBusy = true;
            Device.BeginInvokeOnMainThread(() =>
            {
                txtNotify = "";
            });
            try
            {
                if (CurrentSettings.Settings.IsCompleteSettings)
                {
                    await hubproxy.Invoke("GetFriendList");
                    await hubproxy.Invoke("GetSuggestsAddFriendList");
                    await hubproxy.Invoke("GetAddFriendRequest");
                }
                else
                    IsBusy = false;
            }
            catch (Exception) { IsBusy = false; }
        }
        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            parameters.Add("ChatHub", hubproxy);
            parameters.Add("Connection", connection);
            parameters.Add("Type", type);
            foreach(var e in EventList)
                e.Dispose();
            EventList.Clear();
        }
        private void RoomIsFull()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DependencyService.Get<IToastMessage>().LongTime(new TranslateExtension() { Text = "RoomIsFullMessage" }.ProvideValue(null).ToString());
            });
        }
        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            IsBusy = true;
            if(EventList.Count == 0)
            {
                EventList.Add(hubproxy.On<string, string>("InviteFriendRequest", InviteFriendRequest));
                EventList.Add(hubproxy.On("RoomIsFull", RoomIsFull));
                EventList.Add(hubproxy.On<string, string, bool, Common_Library.Type>("InviteFriendResponse", InviteFriendResponse));
                EventList.Add(hubproxy.On<string, string,string>("Chatrequest", Chatrequest));
                EventList.Add(hubproxy.On<string, string, bool>("ChatWithFriendResponse", ChatWithFriendResponse));
            }
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
                GetFriendList();
            }
        }

        private void InviteFriendResponse(string ConnectionId, string UserName, bool result, Common_Library.Type roomtype)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (result)
                {
                    type = roomtype;
                    var mainpage = (MyTabbedPage)App.Current.MainPage;
                    if (mainpage != null)
                    {
                        ((ViewModelBase)((MyNavigationPage)mainpage.CurrentPage).CurrentPage.BindingContext).GoBackToRootAsync();
                        mainpage.CurrentPage = mainpage.Children[2];
                    }
                    await NavigationService.NavigateAsync("ChatRoom");
                }
            });
        }

        private void InviteFriendRequest(string sender, string ConnectionId)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                IsBusy = true;
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
                IsBusy = false;
            });
        }
    }
}
