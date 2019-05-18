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
    public partial class Form8 : Form
    {
        /*
         ********************************* Form Informations *********************************
         * [Variables]
         * label1 = text to display
         * mouseCoords = mouse coordinates for mouse event handling
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
         * Form10 = convert txt to csv
         * 
         ********************************* Main Form ***************************************
         */

        public Form8(string txt, Color clr,Font fnt,Color fntclr)
        {
            InitializeComponent();

            mouseCoords = MousePosition;    // initialize private data member.
            label1.Text = txt;              // label text
            label1.Font = fnt;              // label font
            label1.ForeColor = fntclr;      // label color
            label1.BackColor = clr;         // label background
        }

        //********************************* Declare Variables *********************************
        Point mouseCoords;                                                      // mouse coordinates

        private void label1_MouseMove(object sender, MouseEventArgs e)          // when user moves the mouse, the form closes
        {
            if (!mouseCoords.IsEmpty)
            {
                //if coordinates have been changed from whenZ8 the object was first
                //initialized, close the SS.
                Point temp;
                temp = new Point(e.X, e.Y);
                if (mouseCoords != temp)
                {
                    this.Close();
                }
                mouseCoords = new Point(e.X, e.Y);
            }
        }

    }
}
