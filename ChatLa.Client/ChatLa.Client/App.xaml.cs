using ChatLa.Client.Helpers;
using ChatLa.Client.Views;
using Microsoft.AspNet.SignalR.Client;
using Prism.Ioc;
using Prism.Unity;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace ChatLa.Client
{
    public partial class App : PrismApplication
    {
        private const string Main = "Main?createTab=Navigation|FindRoom&createTab=Navigation|RoomList&createTab=Navigation|FriendList&createTab=Navigation|Settings";
        public App() : base(null) { }

        protected async override void OnInitialized()
        {
            InitializeComponent();
            await NavigationService.NavigateAsync(Main + (!CurrentSettings.Settings.IsHasChatName ? "&selectedTab=Settings" : ""));
        }
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MyNavigationPage>("Navigation");
            containerRegistry.RegisterForNavigation<FindRoomPage>("FindRoom");
            containerRegistry.RegisterForNavigation<ChatRoomPage>("ChatRoom");
            containerRegistry.RegisterForNavigation<MyTabbedPage>("Main");
            containerRegistry.RegisterForNavigation<RoomListPage>("RoomList");
            containerRegistry.RegisterForNavigation<SettingsPage>("Settings");
            containerRegistry.RegisterForNavigation<MemberListPage>("MemberList");
            containerRegistry.RegisterForNavigation<FriendListPage>("FriendList");
            containerRegistry.RegisterForNavigation<LogInPage>("LogIn");
            containerRegistry.RegisterForNavigation<CreateAccountPage>("CreateAccount");
            containerRegistry.RegisterForNavigation<InviteFriendPage>("InviteFriend");
            List<Tuple<HubConnection, IHubProxy>> list = new List<Tuple<HubConnection, IHubProxy>>();
            while(list.Count < 3)
            {
                HubConnection connection = new HubConnection(string.Format("{0}://{1}:{2}", Common_Library.ConnectionInformation.Protocol, Common_Library.ConnectionInformation.IPAddress, Common_Library.ConnectionInformation.PortNumber));
                IHubProxy hubproxy = connection.CreateHubProxy("ChatHub");
                list.Add(new Tuple<HubConnection, IHubProxy>(connection, hubproxy));
            }
            containerRegistry.RegisterInstance(typeof(List<Tuple<HubConnection, IHubProxy>>), list);
        }
    }
}
