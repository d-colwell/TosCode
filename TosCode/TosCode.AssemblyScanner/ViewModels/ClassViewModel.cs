using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace TosCode.AssemblyScanner.ViewModels
{
    public class ClassViewModel : ViewModelBase
    {
        private Type cls;
        private ObservableCollection<MethodViewModel> methods;
        public ClassViewModel() { }
        public ClassViewModel(Type cls)
        {
            this.cls = cls;
            this.Methods = new ObservableCollection<MethodViewModel>();
            var mthds = cls.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).Where(m =>
                m.GetParameters().Select(p => p.ParameterType).All(p => p.IsPrimitive || p == typeof(string) || p == typeof(double) || p == typeof(Decimal) || p == typeof(DateTime))
                && !m.IsSpecialName
            );
            foreach (var method in mthds)
            {
                this.Methods.Add(new MethodViewModel(method));
            }
        }

        public string Name => cls.Name;

        public ObservableCollection<MethodViewModel> Methods
        {
            get { return methods; }
            set
            {
                if (methods == value)
                    return;
                methods = value;
                base.NotifyPropertyChanged(nameof(Methods));
            }
        }

    }
}