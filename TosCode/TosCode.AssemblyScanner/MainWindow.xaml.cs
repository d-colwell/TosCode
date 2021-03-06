﻿using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using TosCode.AssemblyScanner.ViewModels;
using Tricentis.TCAPIObjects.Objects;
using System.Linq;
namespace TosCode.Connector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TosCode.Connector.Connector connector;
        public MainWindow()
        {
            InitializeComponent();
            this.Assemblies = new ObservableCollection<AssemblyViewModel>();
            DataContext = this;
        }

        public MainWindow(TCFolder folder)
            :base()
        {
            this.ToscaFolder = folder;
            this.IsInitialisedFromTosca = this.ToscaFolder != null;
            connector = new Connector(this.ToscaFolder);
            if (this.IsInitialisedFromTosca)
                this.saveToTosca.Visibility = Visibility.Collapsed;
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Library Files (.dll)|*.dll|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 0;
            openFileDialog.Multiselect = false;
            bool? userClickedOK = openFileDialog.ShowDialog();
            if (userClickedOK == true)
            {
                this.AssemblyPath = openFileDialog.FileName;
            }
            this.Assemblies.Clear();
            this.Assemblies.Add(GetAssembly());
            foreach (var assembly in Assemblies)
            {
                if(this.IsInitialisedFromTosca)
                {
                    var matches = connector.MatchMethodModules(assembly.Model());
                    foreach (var match in matches)
                    {
                        match.MethodModel.FriendlyName = match.XModule.Name;
                        match.MethodModel.IsSelected = true;
                    }
                }
            }
        }


        #region Dependency Properties
        public TCFolder ToscaFolder
        {
            get { return (TCFolder)GetValue(ToscaFolderProperty); }
            set { SetValue(ToscaFolderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ToscaFolder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToscaFolderProperty =
            DependencyProperty.Register("ToscaFolder", typeof(TCFolder), typeof(MainWindow), new PropertyMetadata(null));


        public bool IsInitialisedFromTosca
        {
            get { return (bool)GetValue(IsInitialisedFromToscaProperty); }
            set { SetValue(IsInitialisedFromToscaProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsInitialisedFromTosca.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsInitialisedFromToscaProperty =
            DependencyProperty.Register("IsInitialisedFromTosca", typeof(bool), typeof(MainWindow), new PropertyMetadata(null));


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
