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
    public partial class Form9 : Form
    {

        /*
         ********************************* Form Informations *********************************
         * [Variables]
         * textBox1 = width
         * textBox2 = heigth
         * C_Width = selected width
         * C_Height = selected heigth
         * valid = if user set valid resolution values
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

        public Form9()
        {
            InitializeComponent();
        }

        private void Form9_Load(object sender, EventArgs e)
        {
            ok_button.DialogResult = System.Windows.Forms.DialogResult.OK;
            cancel_button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.AcceptButton = ok_button;
            this.CancelButton = cancel_button;
        }

        //********************************* Declare Variables *********************************
        public int C_Width { get; set; }                                        // selected width
        public int C_Height { get; set; }                                       // selected heigth
        bool valid = true;                                                      // check dialog ok status

        //*********************************  Buttons *********************************
        private void ok_button_Click(object sender, EventArgs e)                // DialogResault.OK
        {
            int w,h;
            if (int.TryParse(textBox1.Text, out w) && int.TryParse(textBox2.Text, out h))   // if w and h are integer
            {
                if ((w > 0 && w <= 7680) && (h > 0 && h <= 4320))                           // if w and h are valid resolution numbers
                {
                    valid = true;
                    this.C_Width = w;
                    this.C_Height = h;
                    this.Close();
                }
                else                                                                        // invalid resolution numbers
                {
                    valid = false;
                    MessageBox.Show("Maximum Resolution 8K (=7680x4320)");
                }
            }
            if (!int.TryParse(textBox1.Text, out w))                                        // wrong w value - not integer
            {
                valid = false;
                MessageBox.Show("Width is invalid");
                textBox1.Clear();
            }
            if (!int.TryParse(textBox2.Text, out h))                                        // wrong h value - not integer
            {
                valid = false;
                MessageBox.Show("Height is invalid");
                textBox2.Clear();
            }
        }

        private void cancel_button_Click(object sender, EventArgs e)            // DialogResault.Cancel
        {
            valid = true;
            this.Close();
        }

        //********************************* Form events  *********************************
        private void Form9_FormClosing(object sender, FormClosingEventArgs e)   // Form closing event  
        {
            if (!valid)
                e.Cancel = true;
        }

    }
}
