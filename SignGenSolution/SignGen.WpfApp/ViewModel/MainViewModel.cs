using Microsoft.Win32;
using SignGen.Logic;
using SignGen.WpfApp.Command;
using SignGen.WpfApp.Other;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace SignGen.WpfApp.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Private Fields
        // Private Felder zur Kapselung und als Hilfe für INotifyPropertyChanged

        private string _defaultEncoding = SignGenFileInfoProvider.GetDefaultEncoding();
        private string _defaultImageFileType = ".jpg";
        private string _inputConfigPath;
        private string _companyLogoPath;
        private string _accountImageDirectory;
        private string _targetDirectory;
        private bool _overwriteExisting;
        private ICommand _generateCommand;
        private ICommand _removeItemCommand;
        private ICommand _addItemCommand;
        private ICommand _openFileCommand;

        #endregion

        #region Display

        public override string Title => "SignGen";

        /// <summary>
        /// Auflistung aller verfügbaren Encodings, um Auswahl über ComboBox zu ermöglichen
        /// </summary>
        public virtual ObservableCollection<BindingWrapper<string>> AvailableEncodings { get; set; }
            = new ObservableCollection<BindingWrapper<string>>(SignGenFileInfoProvider.GetEncodings().Select(s => new BindingWrapper<string>(s)));

        #endregion

        #region Parameter
        // Zur Generierung benötigte Parameter für Binding in View

        public virtual string InputConfigPath
        {
            get => _inputConfigPath;
            set => Update(value, ref _inputConfigPath);
        }
        public virtual string CompanyLogoPath
        {
            get => _companyLogoPath;
            set => Update(value, ref _companyLogoPath);
        }
        public virtual string AccountImageDirectory
        {
            get => _accountImageDirectory;
            set => Update(value, ref _accountImageDirectory);
        }
        public virtual string TargetDirectory
        {
            get => _targetDirectory;
            set => Update(value, ref _targetDirectory);
        }
        public virtual string DefaultEncoding
        {
            get => _defaultEncoding;
            set => Update(value, ref _defaultEncoding);
        }
        public virtual string DefaultImageFileType
        {
            get => _defaultImageFileType;
            set => Update(value, ref _defaultImageFileType);
        }
        public virtual bool OverwriteExisting
        {
            get => _overwriteExisting;
            set => Update(value, ref _overwriteExisting);
        }
        public virtual ObservableCollection<BindingWrapper<string>> TemplatePathes { get; set; } = new ObservableCollection<BindingWrapper<string>>();

        #endregion

        #region Interaction (Commands)
        // Benötigte Commands für Interaktion mit User

        public virtual ICommand GenerateCommand => _generateCommand
            ?? (_generateCommand = new SimpleActionCommand(Generate));

        public virtual ICommand RemoveItemCommand => _removeItemCommand
            ?? (_removeItemCommand = new SimpleActionCommand(RemoveItem));

        public virtual ICommand AddItemCommand => _addItemCommand
            ?? (_addItemCommand = new SimpleActionCommand(AddItem));

        public virtual ICommand OpenFileCommand => _openFileCommand
            ?? (_openFileCommand = new SimpleActionCommand(OpenFile));

        #endregion

        #region Methods
        // Methoden für Commands

        protected virtual void RemoveItem(object parameter)
        {
            if (parameter is BindingWrapper<string>)
            {
                var param = parameter as BindingWrapper<string>;
                if (TemplatePathes.Contains(param))
                {
                    TemplatePathes.Remove(param);
                }
            }
        }

        protected virtual void AddItem()
        {
            TemplatePathes.Add(new BindingWrapper<string>("<Neuer Eintrag>"));
        }

        protected virtual void OpenFile(object parameter)
        {
            PropertyInfo pi = null;
            object target = null;
            if (parameter is string)
            {
                var param = parameter as string;
                pi = this.GetType().GetProperty(param);
                target = this;
            }
            else if (parameter is BindingWrapper<string>)
            {
                pi = parameter.GetType().GetProperty(nameof(BindingWrapper<string>.Item));
                target = parameter;
            }

            if (target != null && pi != null && pi.PropertyType == typeof(string))
            {
                var dlg = new OpenFileDialog();
                if (dlg.ShowDialog() == true)
                {
                    pi.SetValue(target, dlg.FileName);
                }
            }
        }

        protected virtual void Generate()
        {
            SignGenLauncher launcher = null;
            string message = string.Empty;
            string caption = string.Empty;
            MessageBoxImage img = MessageBoxImage.Information;
            try
            {
                launcher = new SignGenLauncher(InputConfigPath, TemplatePathes.Select(w => w.Item).ToList(), CompanyLogoPath, TargetDirectory, AccountImageDirectory, DefaultEncoding, DefaultImageFileType, OverwriteExisting);
            }
            catch (Exception e)
            {
                caption = "Fehler";
                img = MessageBoxImage.Warning;
                message += "Die angegebenen Parameter stimmen nicht mit der Erwartung überein. Bitte prüfen Sie Ihre Konfiguration.\nFolgender Fehler ist aufgetreten: " + e.Message;
                launcher = null;
            }

            if (launcher != null)
            {
                var result = launcher.Run();
                if (result.Succeeded)
                {
                    caption = "Erfolg";
                    message += "Ihre Signaturen wurden erfolgreich generiert.";
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        message += "\nFolgende Warnungen sind dabei zu beachten: " + result.Message;
                    }
                }
                else
                {
                    caption = "Fehler";
                    img = MessageBoxImage.Warning;
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        message += result.Message;
                    }
                    message += "Beim Generieren der Signaturen ist ein Fehler aufgetreten.";
                }
            }

            MessageBox.Show(message, caption, MessageBoxButton.OK, img);
        }

        #endregion
    }
}
