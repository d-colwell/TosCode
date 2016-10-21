using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using TosCode.Connector.Models;
using TosCode.Helpers;

namespace TosCode.Scanner.ViewModels
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
                m.GetParameters().All(IsParameterAllowed)
                && !m.IsSpecialName
            );
            foreach (var method in mthds)
            {
                this.Methods.Add(new MethodViewModel(method, this.Model()));
            }
        }

        public string Name => cls.Name;
        public string FullName => cls.FullName;
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

        public ClassModel Model()
        {
            return new ClassModel
            {
                Methods = this.Methods.Select(x => x.Model()),
                ClassName = this.cls.Name
            };
        }

        public bool IsParameterAllowed(ParameterInfo param)
        {
            var paramType = param.ParameterType;
            if (paramType.IsSimple())
                return true;
            if (paramType.IsInterface)
                return false;
            if (paramType.IsClass && !paramType.GetConstructors().Any(x => x.GetParameters().Count() == 0))
                return false;
            return true;
        }

    }
}