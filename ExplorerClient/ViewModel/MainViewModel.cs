using System.Diagnostics;
using System.Security.AccessControl;
using System.Windows.Input;
using ExplorerClient.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace ExplorerClient.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        #region members
        
        #endregion#


        #region Properties
        private string _stringMessage;

        public string StringMessage
        {
            get { return _stringMessage; }
            set
            {
                _stringMessage = value;
                RaisePropertyChanged();
            }
        }

        #endregion#

        #region commands

        private ICommand _sayHello;
        public ICommand SayHello
        {
            get
            {
                return _sayHello ?? (_sayHello = new RelayCommand(() =>
                {
                    StringMessage = Client.SayHello(StringMessage);
                }));
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            Client.Connect("localhost", 3000);
           
        }


    }
}