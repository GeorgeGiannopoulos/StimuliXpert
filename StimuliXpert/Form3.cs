using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StimuliXpert
{
    public partial class Form3 : Form
    {

        /*
         ********************************* Form Informations *********************************
         * [Variables]
         * 
         * [Mesthods]
         * 
         * [Forms]
         * Form1 = Main window
         * Form2 = presentation window (full screen, no borders)
         * Form3 = presents unique number on every screen to identify them
         * Form4 = attributes windows, create or edit atrributes
         * Form5 = set attributes to media files
         * Form6 = analysis windows (graphics)
         * Form7 = application settings window
         * Form8 = preview settings options (full screen, no borders)
         * Form9 = user select custom resolution
         * Form10 = convert txt to csvs
         * 
         ********************************* Main Form ***************************************
         */

        public Form3()
        {
            InitializeComponent();
        }

        public void load(string name)                                           // custom load method
        {
            label1.Text = name;                                                 // set name = monitor's number id
            label1.Visible = true;                                              // make visible the counter (list1.text)
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;    // Draw label to the center od the screen (mabey optional)
            label1.ForeColor = System.Drawing.Color.Black;                      // set the color of the label
            timer1.Interval = 3000;                                             // Identify Label's time
            timer1.Start();                                                     // start timer
        }

        private void timer1_Tick(object sender, EventArgs e)                    
        {
            timer1.Stop();                  // stop timer
            this.Close();                   // close Form3 object (Identify label)
        }

    }
}
