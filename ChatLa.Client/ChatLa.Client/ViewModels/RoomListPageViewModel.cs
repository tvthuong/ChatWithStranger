using System;
using System.Collections.Generic;
using System.Text;
using Prism.Navigation;
using System.Collections.ObjectModel;
using Microsoft.AspNet.SignalR.Client;
using System.Windows.Input;
using Prism.Commands;
using Xamarin.Forms;
using ChatLa.Client.Helpers;
using ChatLa.Client.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ChatLa.Client.ViewModels
{
    class RoomListPageViewModel : ViewModelBase
    {
        private ObservableCollection<GroupObservableCollection> _rooms;
        private IHubProxy hubproxy;
        private HubConnection connection;
        private Common_Library.Type type;
        private bool _isLogIn;
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
        public bool IsEnabled => IsNotBusy && IsLogIn;
        public bool IsDisabled => !IsEnabled;
        public ObservableCollection<GroupObservableCollection> Rooms
        {
            get => _rooms;
            set => SetProperty(ref _rooms, value);
        }
        public ICommand cmdRefresh { get; set; }
        public RoomListPageViewModel(List<Tuple<HubConnection, IHubProxy>> list, INavigationService navigationService) : base(navigationService)
        {
            Title = new TranslateExtension() { Text = "RoomListPageTitle" }.ProvideValue(null).ToString();
            Rooms = new ObservableCollection<GroupObservableCollection>();
            cmdRefresh = new DelegateCommand(GetRoomList);
            connection = list[1].Item1;
            hubproxy = list[1].Item2;
            hubproxy.On<Common_Library.Type>("Ready", Ready);
            hubproxy.On("RoomNotExist", RoomNotExist);
            hubproxy.On("Connected", LogIn);
            hubproxy.On<ObservableCollection<Common_Library.Room>>("GetRoomList", GetRoomList);
            hubproxy.On("AccountNotExist", AccountNotExist);
            hubproxy.On("RoomIsFull", RoomIsFull);
            hubproxy.On("LogInComplete", LogInComplete);
            hubproxy.On("ChatNameIsExist", ChatNameIsExist);
            OnNavigatedTo(null);
        }

        private void ChatNameIsExist()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DependencyService.Get<IToastMessage>().LongTime(new TranslateExtension() { Text = "ChatNameExistMessage" }.ProvideValue(null).ToString());
                ((MyTabbedPage)App.Current.MainPage).IsHidden = false;
            });
            IsBusy = false;
        }

        private void LogInComplete()
        {
            GetRoomList();
            IsLogIn = true;
        }
        private void RoomIsFull()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DependencyService.Get<IToastMessage>().LongTime(new TranslateExtension() { Text = "RoomIsFullMessage" }.ProvideValue(null).ToString());
                ((MyTabbedPage)App.Current.MainPage).IsHidden = false;
            });
            IsBusy = false;
        }

        private void AccountNotExist()
        {
            CurrentSettings.Settings.DeleteAccount();
            GetRoomList();
            IsLogIn = true;
        }

        private async void LogIn()
        {
            if (CurrentSettings.Settings.IsHasAccount)
                await hubproxy.Invoke("LogInAutomatically", new Common_Library.AccountBase() { UserName = CurrentSettings.Settings.UserName, Password = CurrentSettings.Settings.Password });
            else
            {
                GetRoomList();
                IsLogIn = true;
            }
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            IsBusy = true;
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
                GetRoomList();
            }
        }
        public async void JoinRoom(string chatname, string roomid)
        {
            IsBusy = true;
            ((MyTabbedPage)App.Current.MainPage).IsHidden = true;
            try
            {
                await hubproxy.Invoke("SelectRoom", chatname, roomid);
            }
            catch (Exception)
            {
                IsBusy = false;
                ((MyTabbedPage)App.Current.MainPage).IsHidden = false;
            }
        }
        private void Ready(Common_Library.Type type)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                this.type = type;
                await NavigationService.NavigateAsync("ChatRoom");
                IsBusy = false;
            });
        }
        private void RoomNotExist()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DependencyService.Get<IToastMessage>().LongTime(new TranslateExtension() { Text = "RoomNotExistMessage" }.ProvideValue(null).ToString());
                ((MyTabbedPage)App.Current.MainPage).IsHidden = false;
            });
            IsBusy = false;
        }

        private void GetRoomList(ObservableCollection<Common_Library.Room> list)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                TranslateExtension translate = new TranslateExtension() { Text = "RoomTypePrivate" };
                Rooms.Add(new GroupObservableCollection(list.Where(r => r.Type == Common_Library.Type.Private)) { Heading = translate.ProvideValue(null).ToString() });
                translate.Text = "RoomTypePublic";
                Rooms.Add(new GroupObservableCollection(list.Where(r => r.Type == Common_Library.Type.Public)) { Heading = translate.ProvideValue(null).ToString() });
                IsBusy = false;
            });
        }

        private async void GetRoomList()
        {
            IsBusy = true;
            Device.BeginInvokeOnMainThread(() =>
            {
                Rooms.Clear();
            });
            try
            {
                await hubproxy.Invoke("GetRoomList");
            }
            catch (Exception) { IsBusy = false; }
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            base.OnNavigatedFrom(parameters);
            parameters.Add("ChatHub", hubproxy);
            parameters.Add("Connection", connection);
            parameters.Add("Type", type);
        }
    }
}
