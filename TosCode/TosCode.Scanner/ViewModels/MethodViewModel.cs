using System.Collections.ObjectModel;
using System.Reflection;
using TosCode.Connector.Models;

namespace TosCode.Scanner.ViewModels
{
    public class MethodViewModel : ViewModelBase
    {
        private MethodInfo method;
        public MethodViewModel() { }
        public MethodViewModel(MethodInfo method, ClassModel parent)
        {
            this.method = method;
            this.Parameters = new ObservableCollection<ParameterViewModel>();
            this.parent = parent;
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
        private ClassModel parent;

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

        public MethodModel Model()
        {
            return new MethodModel
            {
                FriendlyName = this.friendlyName,
                IsSelected = this.isSelected,
                MethodName = this.method.Name,
                DeclaringType = this.parent
            };
        }

    }
}