using Prism.Navigation;
using Prism.Mvvm;

namespace ChatLa.Client.ViewModels
{
    public class ViewModelBase : BindableBase, INavigationAware
    {
        private string _title;
        private bool _isBusy = true;
        public virtual bool IsBusy
        {
            get => _isBusy;
            set
            {
                SetProperty(ref _isBusy, value);
                RaisePropertyChanged(nameof(IsNotBusy));
            }
        }
        public bool IsNotBusy => !IsBusy;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title , value);
        }
        protected INavigationService NavigationService { set; get; }
        public ViewModelBase(INavigationService navigationService)
        {
            NavigationService = navigationService;
            Title = "";
        }
        public virtual void GoBackToRootAsync() { }
        public virtual void OnNavigatedFrom(INavigationParameters parameters){ }
        public virtual void OnNavigatedTo(INavigationParameters parameters){ }
        public virtual void OnNavigatingTo(INavigationParameters parameters){ }
    }
}