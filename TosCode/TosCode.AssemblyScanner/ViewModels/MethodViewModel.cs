using System.Collections.ObjectModel;
using System.Reflection;

namespace TosCode.AssemblyScanner.ViewModels
{
    public class MethodViewModel : ViewModelBase
    {
        private MethodInfo method;
        public MethodViewModel() { }
        public MethodViewModel(MethodInfo method)
        {
            this.method = method;
            this.Parameters = new ObservableCollection<ParameterViewModel>();
            foreach (var parameter in method.GetParameters())
            {
                Parameters.Add(new ParameterViewModel(parameter));
            }
        }

        public string Name => method.Name;

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected == value)
                    return;
                isSelected = value;
                base.NotifyPropertyChanged(nameof(IsSelected));
            }
        }

        private string friendlyName;

        public string FriendlyName
        {
            get { return friendlyName; }
            set
            {
                if (friendlyName == value)
                    return;
                friendlyName = value;
                base.NotifyPropertyChanged(nameof(FriendlyName));
            }
        }

        private ObservableCollection<ParameterViewModel>  parameters;

        public ObservableCollection<ParameterViewModel> Parameters
        {
            get { return parameters; }
            set
            {
                if (parameters == value)
                    return;
                parameters = value;
                base.NotifyPropertyChanged(nameof(Parameters));
            }
        }


    }
}