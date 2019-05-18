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
    public partial class Form10 : Form
    {

        /*
         ********************************* Form Informations *********************************
         * [Variables]
         * return_delimeter = delimeter selected by the user
         * return_extension = extension selected by the user
         * return_clear = clear data selected by the user
         * rerurn_header = add header selected by the user
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

        public Form10()
        {
            InitializeComponent();
        }

        private void Form10_Load(object sender, EventArgs e)
        {
            ok_button.DialogResult = DialogResult.OK;
            cancel_button.DialogResult = DialogResult.Cancel;
            this.AcceptButton = ok_button;
            this.CancelButton = cancel_button;

            radioButton1.Checked = true;
            radioButton8.Checked = true;
            radioButton9.Checked = true;
            radioButton11.Checked = true;
        }

        //********************************* Declare Variables *********************************
        public string return_delimeter { get; set; }                            // returned delimiter
        public string return_extension { get; set; }                            // returned extension
        public bool return_clear { get; set; }                                  // returned clear
        public bool rerurn_header { get; set; }                                 // return header 

        //*********************************  Buttons *********************************
        private void ok_button_Click(object sender, EventArgs e)                // accept button
        {
            this.Close();
        }

        private void cancel_button_Click(object sender, EventArgs e)            // cancel button
        {
            this.Close();
        }

        //********************************* RadioButtons events  *********************************
        private void radioButton1_CheckedChanged(object sender, EventArgs e)    // user delimiter selection
        {
            if (radioButton1.Checked == true)
                return_delimeter = "space";
            else if (radioButton2.Checked == true)
                return_delimeter = "comma";
            else if (radioButton3.Checked == true)
                return_delimeter = "semicolon";
            else if (radioButton4.Checked == true)
                return_delimeter = "colon";
            else if (radioButton5.Checked == true)
                return_delimeter = "bar";
            else if (radioButton6.Checked == true)
                return_delimeter = "tab";
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)    // user extension selection
        {
            if (radioButton7.Checked)
                return_extension = ".txt";
            else
                return_extension = ".csv";
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)    // user clear data selection
        {
            if (radioButton9.Checked)
                return_clear = false;
            else
                return_clear = true;
        }

        private void radioButton11_CheckedChanged(object sender, EventArgs e)   // user add header selection
        {
            if (radioButton11.Checked)
                rerurn_header = false;
            else
                rerurn_header = true;
        }

    }
}
