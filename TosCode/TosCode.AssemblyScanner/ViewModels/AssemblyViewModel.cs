using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TosCode.AssemblyScanner.ViewModels
{
    public class AssemblyViewModel : ViewModelBase
    {
        private readonly Assembly assembly;
        private ObservableCollection<ClassViewModel> _classes;
        public AssemblyViewModel() { }
        public AssemblyViewModel(Assembly assembly)
        {
            this.assembly = assembly;
            this.Classes = new ObservableCollection<ClassViewModel>();
            var classes = assembly.GetTypes().Where( t=>
                t.IsPublic
                && t.GetConstructors().Any(c=>c.IsPublic && c.GetParameters().Count() == 0)
                && t.IsClass    
            );
            foreach (var cls in classes)
            {
                Classes.Add(new ClassViewModel(cls));
            }
        }

        public string Name => assembly.FullName;

        public ObservableCollection<ClassViewModel> Classes
        {
            get { return _classes; }
            set
            {
                if (_classes == value)
                    return;
                _classes = value;
                base.NotifyPropertyChanged(nameof(Classes));
            }
        }
    }
}
