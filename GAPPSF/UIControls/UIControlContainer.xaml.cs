﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace GAPPSF.UIControls
{
    /// <summary>
    /// Interaction logic for UIControlContainer.xaml
    /// </summary>
    public partial class UIControlContainer : UserControl, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler WindowStateButtonClick;

        public bool DisposeOnClear { get; set; }

        public UIControlContainer()
        {
            DisposeOnClear = true;
            InitializeComponent();

            Localization.TranslationManager.Instance.LanguageChanged += Instance_LanguageChanged;
        }

        void Instance_LanguageChanged(object sender, EventArgs e)
        {
            if(FeatureControl!=null)
            {
                Text = FeatureControl.ToString();
            }
        }

        public void Dispose()
        {
            Localization.TranslationManager.Instance.LanguageChanged -= Instance_LanguageChanged;
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                var handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }

        private UserControl _featureControl = null;
        public UserControl FeatureControl
        {
            get { return _featureControl; }
            set
            {
                ClearFreatureContainer();
                if (value!=null)
                {
                    containerGrid.Children.Add(value);
                }
                SetProperty(ref _featureControl, value);
                if (value != null)
                {
                    Text = value.ToString();
                }
                else
                {
                    Text = "";
                }
                IUIControl fc = value as IUIControl;
                if (fc != null)
                {
                    WindowWidth = fc.WindowWidth;
                    WindowHeight = fc.WindowHeight;
                    WindowLeft = fc.WindowLeft;
                    WindowTop = fc.WindowTop;
                }
            }
        }

        private string _text = "";
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        private int _windowWidth = 500;
        public int WindowWidth
        {
            get { return _windowWidth; }
            set 
            {
                if (this.UIControl != null)
                {
                    this.UIControl.WindowWidth = value;
                }
                SetProperty(ref _windowWidth, value); 
            }
        }

        private int _windowHeight = 500;
        public int WindowHeight
        {
            get { return _windowHeight; }
            set
            {
                if (this.UIControl != null)
                {
                    this.UIControl.WindowHeight = value;
                }
                SetProperty(ref _windowHeight, value);
            }
        }

        private int _windowLeft = 100;
        public int WindowLeft
        {
            get { return _windowLeft; }
            set
            {
                if (this.UIControl != null)
                {
                    this.UIControl.WindowLeft = value;
                }
                SetProperty(ref _windowLeft, value);
            }
        }

        private int _windowTop = 100;
        public int WindowTop
        {
            get { return _windowTop; }
            set
            {
                if (this.UIControl != null)
                {
                    this.UIControl.WindowTop = value;
                }
                SetProperty(ref _windowTop, value);
            }
        }

        private IUIControl UIControl
        {
            get
            {
                IUIControl result = null;
                if (containerGrid.Children.Count > 0)
                {
                    result = containerGrid.Children[0] as IUIControl;
                }
                return result;
            }
        }

        private void ClearFreatureContainer()
        {
            while (containerGrid.Children.Count>0)
            {
                if (DisposeOnClear && containerGrid.Children[0] is IDisposable)
                {
                    IDisposable a = containerGrid.Children[0] as IDisposable;
                    containerGrid.Children.RemoveAt(0);
                    a.Dispose();
                }
                else
                {
                    containerGrid.Children.RemoveAt(0);
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //Clear
            FeatureControl = null;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //Cache list
            FeatureControl = new CacheList();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            FeatureControl = new GeocacheViewer();
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            FeatureControl = new ApplicationDataInfo();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (WindowStateButtonClick!=null)
            {
                WindowStateButtonClick(this, EventArgs.Empty);
            }
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            FeatureControl = new GMap.GoogleMap();
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            FeatureControl = new OfflineImages.Control();
        }

        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            FeatureControl = new GeocacheFilter.Control();
        }

        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {
            FeatureControl = new IgnoreGeocaches.Control();
        }

        private void MenuItem_Click_8(object sender, RoutedEventArgs e)
        {
            FeatureControl = new DebugLogView.Control();
        }

        private void MenuItem_Click_9(object sender, RoutedEventArgs e)
        {
            FeatureControl = new ActionBuilder.Control();
        }

        private void MenuItem_Click_10(object sender, RoutedEventArgs e)
        {
            FeatureControl = new Maps.Control(new MapProviders.MapControlFactoryGoogle());
        }

        private void MenuItem_Click_11(object sender, RoutedEventArgs e)
        {
            FeatureControl = new Maps.Control(new MapProviders.MapControlFactoryOSMOnline());
        }

        private void MenuItem_Click_12(object sender, RoutedEventArgs e)
        {
            FeatureControl = new Maps.Control(new MapProviders.MapControlFactoryOSMOffline());
        }

        private void MenuItem_Click_13(object sender, RoutedEventArgs e)
        {
            FeatureControl = new GoogleEarth.Control();
        }

        private void MenuItem_Click_14(object sender, RoutedEventArgs e)
        {
            FeatureControl = new Attachement.Control();
        }

        private void MenuItem_Click_15(object sender, RoutedEventArgs e)
        {
            FeatureControl = new CAR.Control();
        }

        private void MenuItem_Click_16(object sender, RoutedEventArgs e)
        {
            FeatureControl = new FormulaSolver.Control();
        }

        private void MenuItem_Click_17(object sender, RoutedEventArgs e)
        {
            FeatureControl = new GCEditor.Control();
        }

        private void MenuItem_Click_18(object sender, RoutedEventArgs e)
        {
            FeatureControl = new WPEditor.Control();
        }

        private void MenuItem_Click_19(object sender, RoutedEventArgs e)
        {
            FeatureControl = new LogViewer.Control();
        }

        private void MenuItem_Click_20(object sender, RoutedEventArgs e)
        {
            FeatureControl = new NotesEditor.Control();
        }

        private void MenuItem_Click_21(object sender, RoutedEventArgs e)
        {
            FeatureControl = new GeocacheCollection.Control();
        }

        private void MenuItem_Click_22(object sender, RoutedEventArgs e)
        {
            FeatureControl = new LogImageViewer.Control();
        }


    }
}
