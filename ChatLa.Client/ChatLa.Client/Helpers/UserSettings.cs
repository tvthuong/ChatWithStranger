using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Prism.Mvvm;
namespace ChatLa.Client.Helpers
{
    public class UserSettings : BindableBase
    {
        public UserSettings() {}
        static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }
        public string ChatName
        {
            get => AppSettings.GetValueOrDefault(nameof(ChatName), "");
            set
            {
                if (!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value))
                {
                    if (!IsHasChatName)
                        ((MyTabbedPage)App.Current.MainPage).IsHidden = false;
                    AppSettings.AddOrUpdateValue(nameof(ChatName), value);
                }
                RaisePropertyChanged(nameof(ChatName));
                RaisePropertyChanged(nameof(IsHasChatName));
                RaisePropertyChanged(nameof(IsCompleteSettings));
            }
        }
        public string UserName
        {
            get => AppSettings.GetValueOrDefault(nameof(UserName), "");
            set
            {
                AppSettings.AddOrUpdateValue(nameof(UserName), value);
                RaisePropertyChanged(nameof(UserName));
                RaisePropertyChanged(nameof(IsHasAccount));
                RaisePropertyChanged(nameof(IsNotHasAccount));
                RaisePropertyChanged(nameof(IsCompleteSettings));
            }
        }
        public string Password
        {
            get => AppSettings.GetValueOrDefault(nameof(Password), "");
            set
            {
                AppSettings.AddOrUpdateValue(nameof(Password), value);
                RaisePropertyChanged(nameof(Password));
                RaisePropertyChanged(nameof(IsHasAccount));
                RaisePropertyChanged(nameof(IsNotHasAccount));
                RaisePropertyChanged(nameof(IsCompleteSettings));
            }
        }

        public void DeleteAccount()
        {
            AppSettings.Remove(nameof(UserName));
            AppSettings.Remove(nameof(Password));
            RaisePropertyChanged(nameof(UserName));
            RaisePropertyChanged(nameof(Password));
            RaisePropertyChanged(nameof(IsHasAccount));
            RaisePropertyChanged(nameof(IsNotHasAccount));
            RaisePropertyChanged(nameof(IsCompleteSettings));
        }
        public bool IsHasChatName => !string.IsNullOrEmpty(ChatName);
        public bool IsHasAccount => !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);
        public bool IsNotHasAccount => !IsHasAccount;
        public bool IsCompleteSettings => IsHasAccount && IsHasChatName;
        public static void ClearAllData()
        {
            AppSettings.Clear();
        }
    }
}
