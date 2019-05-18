using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Interop;

namespace StimuliXpert
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {

        /*
         ********************************* UserControl Informations *****************************************
         * [Variables]
         * mediaelement1 = media host. containe media element and control
         * 
         * [Mesthods]
         * setMediaSource() = set the source(path of file) which is about to play
         * mediaPlay() = play media file 
         * mediaStop() = stop media file
         * mediaPause() = pause media file
         * mediaClose() = close media file
         * 
         ********************************* Media Element UserControl ****************************************
         */

        public UserControl1()
        {
            InitializeComponent();

            mediaelement1.LoadedBehavior = MediaState.Manual;                               // Manual LoadedBehavior = User can load  the element
            mediaelement1.UnloadedBehavior = MediaState.Manual;                             // Manual UnloadedBehavior = User can unload the element
            mediaelement1.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;  // HorizontalAlignment = center of the screen
            mediaelement1.VerticalAlignment = System.Windows.VerticalAlignment.Center;      // VerticalAlignment = center of the screen
        }

        private void usercontrol1_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private bool mediaEnded;

        public bool New_emediaEnded
        {
            get { return mediaEnded; }
            set
            {
                if (mediaEnded != value)
                {
                    mediaEnded = value;
                    OnMedia_Ended_Changed(new BoolEventArgs(mediaEnded));
                }
            }
        }

        public event EventHandler<BoolEventArgs> Media_Ended_Changed;

        protected virtual void OnMedia_Ended_Changed(BoolEventArgs e)
        {
            EventHandler<BoolEventArgs> eh = Media_Ended_Changed;
            if (eh != null)
                eh(this, e);
        }

        public void setMediaSource(string filepath, System.Windows.Media.Stretch displaymode) // Set the source (path) of media element
        {
            mediaelement1.Source = new Uri(filepath);   // Set the element Source 
            mediaelement1.Stretch = displaymode;        // Element Box size
            New_emediaEnded = false;
        }

        public void mediaPlay()                         // Play loaded element
        {
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;  // hardware accelaration source
            HwndTarget hwndTarget = hwndSource.CompositionTarget;                       // hardware accelaration target
            hwndTarget.RenderMode = RenderMode.SoftwareOnly;                            // hardware accelaration disable
            mediaelement1.Play();                       // Play the element whose source given before
        }

        public void mediaStop()                         // Stop loaded element
        {
            if (mediaelement1.IsEnabled)                // if media IsEnabled (On screen)
                mediaelement1.Stop();                   // Stop the element whose source given before
        }

        public void mediaPause()                        // Pause loaded element
        {
            if (mediaelement1.IsEnabled)                // if media IsEnabled (On screen)
                mediaelement1.Pause();                  // Pause the element whose source given before
        }

        public void mediaClose()                        // Close loaded element
        {
            if (mediaelement1.IsEnabled)                // if media IsEnabled (On screen)
            {
                mediaelement1.Stop();                   // Stop the element whose source given before
                mediaelement1.Close();                  // Close the element whose source given before
                mediaelement1.Source = null;            // Set source to null (clear source)
            }
        }

        private void mediaelement1_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (mediaelement1.HasAudio)
            {
                New_emediaEnded = true;
            }
        }  

        
        
    }
}
