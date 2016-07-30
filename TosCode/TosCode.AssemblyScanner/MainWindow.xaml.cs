using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TosCode.AssemblyScanner.ViewModels;

namespace TosCode.AssemblyScanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Assemblies = new ObservableCollection<AssemblyViewModel>();
            DataContext = this;
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Library Files (.dll)|*.dll|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 0;
            openFileDialog.Multiselect = false;
            bool? userClickedOK = openFileDialog.ShowDialog();
            if(userClickedOK == true)
            {
                this.AssemblyPath = openFileDialog.FileName;
            }
            this.Assemblies.Clear();
            this.Assemblies.Add(GetAssembly());
        }


        #region Dependency Properties
        public string AssemblyPath
        {
            get { return (string)GetValue(AssemblyPathProperty); }
            set { SetValue(AssemblyPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AssemblyPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AssemblyPathProperty =
            DependencyProperty.Register("AssemblyPath", typeof(string), typeof(MainWindow), new PropertyMetadata(null));

        public ObservableCollection<AssemblyViewModel> Assemblies
        {
            get { return (ObservableCollection<AssemblyViewModel>)GetValue(AssemblyProperty); }
            set { SetValue(AssemblyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Assembly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AssemblyProperty =
            DependencyProperty.Register("Assemblies", typeof(ObservableCollection<AssemblyViewModel>), typeof(MainWindow), new PropertyMetadata(null));

        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            set { SetValue(ErrorMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register("ErrorMessage", typeof(string), typeof(MainWindow), new PropertyMetadata(null));

        public ClassViewModel SelectedClass
        {
            get { return (ClassViewModel)GetValue(SelectedClassProperty); }
            set { SetValue(SelectedClassProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedClass.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedClassProperty =
            DependencyProperty.Register("SelectedClass", typeof(ClassViewModel), typeof(MainWindow), new PropertyMetadata(null));

        public MethodViewModel SelectedMethod
        {
            get { return (MethodViewModel)GetValue(SelectedMethodProperty); }
            set { SetValue(SelectedMethodProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedMethod.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedMethodProperty =
            DependencyProperty.Register("SelectedMethod", typeof(MethodViewModel), typeof(MainWindow), new PropertyMetadata(null));



        #endregion

        private AssemblyViewModel GetAssembly()
        {
            if (string.IsNullOrEmpty(this.AssemblyPath))
                this.ErrorMessage = "No Assembly File Selected";
            if (!File.Exists(this.AssemblyPath))
                this.ErrorMessage = "Invalid Assembly file path";

            if (!string.IsNullOrEmpty(this.ErrorMessage))
                return null;

            Assembly assembly = System.Reflection.Assembly.LoadFrom(AssemblyPath);
            return new AssemblyViewModel(assembly);
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is ClassViewModel)
                SelectedClass = e.NewValue as ClassViewModel;
            if (e.NewValue is MethodViewModel)
                SelectedMethod = e.NewValue as MethodViewModel;
        }
    }
}
