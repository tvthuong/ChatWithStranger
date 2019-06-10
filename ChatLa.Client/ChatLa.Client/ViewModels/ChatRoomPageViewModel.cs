using Microsoft.AspNet.SignalR.Client;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using ChatLa.Client.Models;
using System.Windows.Input;
using Prism.Commands;
using System.Collections.ObjectModel;
using ChatLa.Client.Helpers;
using Common_Library;
using System.Linq;
using System.Threading;

namespace ChatLa.Client.ViewModels
{
    public class ChatRoomPageViewModel : ViewModelBase
    {
        private IHubProxy hubproxy;
        private HubConnection connection;
        private string _txtMessage;
        private Common_Library.Type _type;
        private Member _selected;
        private ObservableCollection<Member> _members;
        private bool _isAdmin;

        public bool IsAdmin
        {
            get => _isAdmin;
            set
            {
                SetProperty(ref _isAdmin, value);
                ((DelegateCommand)cmdChangeRoomType).RaiseCanExecuteChanged();
            }
        }
        public Member Selected
        {
            get => _selected;
            set
            {
                SetProperty(ref _selected, value);
                ((DelegateCommand)cmdSend).RaiseCanExecuteChanged();
            }
        }
        public ObservableCollection<Member> Members
        {
            get => _members;
            set
            {
                SetProperty(ref _members, value);
                ((DelegateCommand)cmdSend).RaiseCanExecuteChanged();
            }
        }
        private List<IDisposable> EventList { get; set; }
        public ObservableCollection<ChatMessage> ChatMessages { get; set; }
        public ICommand cmdViewMember { get; set; }
        public bool IsHasMembers => Type == Common_Library.Type.Public;
        protected Common_Library.Type Type
        {
            get => _type;
            set
            {
                SetProperty(ref _type, value);
                ((DelegateCommand)cmdSend).RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(IsHasMembers));
                ((DelegateCommand)cmdChangeRoomType).RaiseCanExecuteChanged();
            }
        }
        public string txtMessage
        {
            get => _txtMessage;
            set
            {
                SetProperty(ref _txtMessage, value);
                ((DelegateCommand)cmdSend).RaiseCanExecuteChanged();
            }
        }
        public ICommand cmdChangeRoomType { get; set; }
        public ICommand cmdSend { get; set; }
        public override bool IsBusy
        {
            get => base.IsBusy;
            set
            {
                base.IsBusy = value;
                ((DelegateCommand)cmdSend).RaiseCanExecuteChanged();
                ((DelegateCommand)cmdViewMember).RaiseCanExecuteChanged();
            }
        }
        public ChatRoomPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            cmdSend = new DelegateCommand(Send, canExecute);
            Members = new ObservableCollection<Member>() { };
            txtMessage = "";
            Title = new TranslateExtension() { Text = "ChatRoomPageTitle" }.ProvideValue(null).ToString();
            ChatMessages = new ObservableCollection<ChatMessage>();
            cmdChangeRoomType = new DelegateCommand(ChangeRoomType, CanChangeRoomType);
            cmdViewMember = new DelegateCommand(ViewMember, canView);
            EventList = new List<IDisposable>();
        }

        private bool CanChangeRoomType()
        {
            return Type == Common_Library.Type.Private && IsAdmin;
        }

        private async void ChangeRoomType()
        {
            IsBusy = true;
            Type = await hubproxy.Invoke<Common_Library.Type>("ChangeRoomType");
            IsBusy = false;
        }

        private bool canView()
        {
            return IsNotBusy;
        }

        private async void ViewMember()
        {
            IsBusy = true;
            await NavigationService.NavigateAsync("MemberList");
            IsBusy = false;
        }

        private void MemberExit(string chatname)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                string memberexitmessage = new TranslateExtension() { Text = "MemberExitMessage" }.ProvideValue(null).ToString();
                ChatMessage chatmessage = new ChatMessage() { Message = chatname + " " + memberexitmessage, Sender = Sender.System };
                ChatMessages.Add(chatmessage);
                DependencyService.Get<IToastMessage>().LongTime(chatname + " " + memberexitmessage);
            });
        }

        private void NewMessage(User user, string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ChatMessage chatmessage = new ChatMessage() { Message = message };
                if (user.ConnectionId == connection.ConnectionId)
                    chatmessage.Sender = Sender.Me;
                else
                    chatmessage.ChatName = user.ChatName;
                ChatMessages.Add(chatmessage);
            });
        }

        private bool canExecute()
        {
            return IsNotBusy && !string.IsNullOrEmpty(txtMessage) && ((Selected != null && IsHasMembers) || (!IsHasMembers && Selected == null)) && Members.Count > 0 && Members != null;
        }

        private void Send()
        {
            IsBusy = true;
            string temp = txtMessage;
            txtMessage = "";
            if (!IsHasMembers)
                hubproxy.Invoke("Send", temp);
            else
                if (string.IsNullOrEmpty(Selected.ConnectionId))
                hubproxy.Invoke("Send", temp);
            else
                hubproxy.Invoke("PrivateSend", temp, Selected.ConnectionId);
            IsBusy = false;
        }
        private void NewPrivateMessage(string message, User sender, User receiver)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ChatMessage chatmessage = new ChatMessage() { Message = message };
                if (sender.ConnectionId == connection.ConnectionId)
                {
                    chatmessage.Sender = Sender.Me;
                    chatmessage.ChatName = string.Format("{0} {1}", new TranslateExtension() { Text = "PrivateMessageTo" }.ProvideValue(null).ToString(), receiver.ChatName);
                }
                else if (receiver.ConnectionId == connection.ConnectionId)
                    chatmessage.ChatName = string.Format("{0} {1}", new TranslateExtension() { Text = "PrivateMessageFrom" }.ProvideValue(null).ToString(), sender.ChatName);
                ChatMessages.Add(chatmessage);
            });
        }
        private void NewMember(string chatname)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                string newmembermessage = new TranslateExtension() { Text = "NewMemberMessage" }.ProvideValue(null).ToString();
                ChatMessage chatmessage = new ChatMessage() { Message = chatname + " " + newmembermessage, Sender = Sender.System };
                ChatMessages.Add(chatmessage);
                DependencyService.Get<IToastMessage>().LongTime(chatname + " " + newmembermessage);
            });
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            parameters.Add("EventList", EventList);
            parameters.Add("ChatHub", hubproxy);
            parameters.Add("Connection", connection);
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
        private void InviteFriendResponse(string ConnectionId, string UserName, bool result, Common_Library.Type roomtype)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (result && connection.ConnectionId == ConnectionId)
                {
                    Type = roomtype;
                    ChatMessages.Clear();
                    GetMemberList();
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
        private void RoomIsFull()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DependencyService.Get<IToastMessage>().LongTime(new TranslateExtension() { Text = "RoomIsFullMessage" }.ProvideValue(null).ToString());
            });
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            if (parameters.ContainsKey("EventList"))
            {
                List<IDisposable> list = (List<IDisposable>)parameters["EventList"];
                foreach (IDisposable disposable in list)
                    disposable.Dispose();
            }
            if (parameters.ContainsKey("Type"))
                Type = (Common_Library.Type)parameters["Type"];
            if (parameters.ContainsKey("Connection") && parameters.ContainsKey("ChatHub"))
            {
                connection = (HubConnection)parameters["Connection"];
                hubproxy = (IHubProxy)parameters["ChatHub"];
                if (EventList.Count == 0)
                {
                    EventList.Add(hubproxy.On<string, string>("InviteFriendRequest", InviteFriendRequest));
                    EventList.Add(hubproxy.On("RoomIsFull", RoomIsFull));
                    EventList.Add(hubproxy.On<string, string, bool, Common_Library.Type>("InviteFriendResponse", InviteFriendResponse));
                    EventList.Add(hubproxy.On<string, User, User>("NewPrivateMessage", NewPrivateMessage));
                    EventList.Add(hubproxy.On<ObservableCollection<Common_Library.Member>>("GetMemberList", GetMemberList));
                    EventList.Add(hubproxy.On("NewMember", GetMemberList));
                    EventList.Add(hubproxy.On("MemberExit", GetMemberList));
                    EventList.Add(hubproxy.On<User, bool>("Response", Response));
                    EventList.Add(hubproxy.On<User, bool>("AddFriendResponse", AddFriendResponse));
                    EventList.Add(hubproxy.On<User>("Privatechatrequest", Privatechatrequest));
                    EventList.Add(hubproxy.On<User>("AddFriendrequest", AddFriendrequest));
                    EventList.Add(hubproxy.On<string>("NewMember", NewMember));
                    EventList.Add(hubproxy.On<string>("MemberExit", MemberExit));
                    EventList.Add(hubproxy.On("MustLeaveRoom", MustLeaveRoom));
                    EventList.Add(hubproxy.On<Common_Library.Type>("RoomTypeChanged", RoomTypeChanged));
                    EventList.Add(hubproxy.On<User, string>("NewMessage", NewMessage));
                    EventList.Add(hubproxy.On<string, string, string>("Chatrequest", Chatrequest));
                    EventList.Add(hubproxy.On<string, string, bool>("ChatWithFriendResponse", ChatWithFriendResponse));
                }
            }
            GetMemberList();
            IsBusy = false;
        }
        private void ChatWithFriendResponse(string ConnectionId, string UserName, bool result)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (result)
                {
                    Type = Common_Library.Type.Private;
                    ChatMessages.Clear();
                    GetMemberList();
                }
            });
        }
        private void RoomTypeChanged(Common_Library.Type type)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Type = type;
                string roomtypechangedmessage = new TranslateExtension() { Text = "RoomTypeChangedMessage" }.ProvideValue(null).ToString();
                ChatMessage chatmessage = new ChatMessage() { Message = roomtypechangedmessage, Sender = Sender.System };
                ChatMessages.Add(chatmessage);
                DependencyService.Get<IToastMessage>().LongTime(roomtypechangedmessage);
            });
        }

        private void MustLeaveRoom()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                IsBusy = true;
                DependencyService.Get<IToastMessage>().LongTime(new TranslateExtension() { Text = "MustLeaveRoomMessage" }.ProvideValue(null).ToString());
                var mainPage = App.Current.MainPage as MyTabbedPage;
                if (mainPage != null)
                    mainPage.IsHidden = false;
                await NavigationService.GoBackToRootAsync();
            });
        }

        private void AddFriendResponse(User user, bool result)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                IsBusy = true;
                if (!result && connection.ConnectionId != user.ConnectionId)
                    DependencyService.Get<IToastMessage>().LongTime(string.Format("({0} - {1}) {2}", user.ChatName, user.Account.UserName, new TranslateExtension() { Text = "DenyMessage" }.ProvideValue(null).ToString()));
                IsBusy = false;
            });
        }

        private void AddFriendrequest(User user)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                IsBusy = true;
                TranslateExtension translate = new TranslateExtension();
                translate.Text = "AddFriendAlertTitle";
                string title = translate.ProvideValue(null).ToString();
                translate.Text = "AddFriendAlertMessage";
                string message = string.Format("({0} - {1}) {2}", user.ChatName, user.Account.UserName, translate.ProvideValue(null).ToString());
                translate.Text = "AlertAccept";
                string accept = translate.ProvideValue(null).ToString();
                translate.Text = "AlertCancel";
                string cancel = translate.ProvideValue(null).ToString();
                bool result = await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
                await hubproxy.Invoke("AddFriendResponse", user.ConnectionId, result);
                IsBusy = false;
            });
        }

        private async void GetMemberList()
        {
            IsBusy = true;
            Device.BeginInvokeOnMainThread(() =>
            {
                Members.Clear();
            });
            try
            {
                await hubproxy.Invoke("GetMemberList");
                IsAdmin = await hubproxy.Invoke<bool>("IsAdmin");
            }
            catch (Exception) { IsBusy = false; }
        }
        private void GetMemberList(ObservableCollection<Member> list)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ObservableCollection<Member> members = new ObservableCollection<Member>(list);
                if (members.Count > 0)
                    members.Insert(0, new Member() { ConnectionId = "", ChatName = new TranslateExtension() { Text = "PkSendToAll" }.ProvideValue(null).ToString() });
                Members = members;
                IsBusy = false;
            });
        }
        private void Response(User user, bool result)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                IsBusy = true;
                if (result)
                {
                    ChatMessages.Clear();
                    Type = Common_Library.Type.Private;
                    GetMemberList();
                }
                else if (connection.ConnectionId != user.ConnectionId)
                    DependencyService.Get<IToastMessage>().LongTime(string.Format("{0} {1}", user.ChatName, new TranslateExtension() { Text = "DenyMessage" }.ProvideValue(null).ToString()));
                IsBusy = false;
            });
        }

        private void Privatechatrequest(User user)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                IsBusy = true;
                TranslateExtension translate = new TranslateExtension();
                translate.Text = "AlertTitle";
                string title = translate.ProvideValue(null).ToString();
                translate.Text = "AlertMessage";
                string message = string.Format("{0} {1}", user.ChatName, translate.ProvideValue(null).ToString());
                translate.Text = "AlertAccept";
                string accept = translate.ProvideValue(null).ToString();
                translate.Text = "AlertCancel";
                string cancel = translate.ProvideValue(null).ToString();
                bool result = await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
                await hubproxy.Invoke("Response", user.ConnectionId, result);
                IsBusy = false;
            });
        }
    }
}
