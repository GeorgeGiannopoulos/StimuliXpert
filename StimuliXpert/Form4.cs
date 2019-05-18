using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace StimuliXpert
{
    public partial class Form4 : Form
    {
        /*
         ********************************* Form Informations *********************************
         * [Variables]
         * attribute = null or attribute to edit
         * returned_attribute = attribute sent to Form1
         * textBox1 = attribute name
         * textBox2 = attribute values 
         * 
         * [Mesthods]
         * initializeForm4() = initialize Form4 parameters
         * IsValidFilename() = // check if string - windows invalid filename
         * attribute_slip() = attribute (loaded variable) != null -> slip attribute name and attribute values (for edit purposes)
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
         ********************************* Attribute Set Form ***************************************
         */

        string attribute;                                                       // null or attribute to edit

        public string returned_attribute { get; set; }
        bool valid = true;

        public Form4(string attrib)
        {
            InitializeComponent();

            attribute = attrib;             // loaded viriable (edit attribute or set new)
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            ok_button.DialogResult = System.Windows.Forms.DialogResult.OK;
            cancel_button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.AcceptButton = ok_button;
            this.CancelButton = cancel_button;

            initializeForm4();
        }

        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!valid)
                e.Cancel = true;
        }

        private void initializeForm4()                                          // initialize Form4 parameters
        {
            if (attribute == null) 
            {
                textBox1.Text = "Attribute Name";
                textBox1.ForeColor = Color.Gray;

                textBox2.Text = "Attr_1,Attr_2,Attr_3,...";
                textBox2.ForeColor = Color.Gray;
            }
            else
            {
                attribute_slip(attribute);
            }
        }

        private void textBox1_Enter(object sender, EventArgs e)                 // Changes the color of attribute's name
        {
            textBox1.ForeColor = Color.Black;
            if (textBox1.Text == "Attribute Name")
            {
                textBox1.Text = "";
                textBox1.ForeColor = Color.Gray;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)                 // Changes the color of attribute's name
        {
            textBox1.ForeColor = Color.Black;
            if (textBox1.Text == "" || textBox1.Text == "Attribute Name")
            {
                textBox1.ForeColor = Color.Gray;
                textBox1.Text = "Attribute Name";
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)           // Changes the color of attribute's name
        {
            if (!IsValidFilename(textBox1.Text))                                //invalid characters: \  /  :  *  ?  "  <  >  |
            {
                MessageBox.Show("Invaled Character:  " + @"\" + " " +
                                                          "/" + " " +
                                                          ":" + " " +
                                                          "*" + " " +
                                                          "?" + " " +
                                                          "\"" + " " +
                                                          "<" + " " +
                                                          ">" + " " +
                                                          "|");
                textBox1.Text = textBox1.Text.Remove(textBox1.Text.Length - 1); // remove the invalid character
                textBox1.Select(textBox1.Text.Length, 0);                       // move cursor on last character after invalid character removed
                textBox1.ForeColor = Color.Gray;
            }
            if (textBox1.Text != "Attribute Name")
                textBox1.ForeColor = Color.Black;
        }

        private void textBox2_Enter(object sender, EventArgs e)                 // Changes the color of attribute's values
        {
            textBox2.ForeColor = Color.Black;
            if (textBox2.Text == "Attr_1,Attr_2,Attr_3,...")
            {
                textBox2.Text = "";
                textBox2.ForeColor = Color.Gray;
            }
        }

        private void textBox2_Leave(object sender, EventArgs e)                 // Changes the color of attribute's values
        {
            textBox2.ForeColor = Color.Black;
            if (textBox2.Text.EndsWith(","))
                textBox2.Text = textBox2.Text.Remove(textBox2.Text.Length - 1); // remove the invalid character
            if (textBox2.Text == "" || textBox2.Text == "Attr_1,Attr_2,Attr_3,...")
            {
                textBox2.ForeColor = Color.Gray;
                textBox2.Text = "Attr_1,Attr_2,Attr_3,...";
            }
            
        }

        private void textBox2_TextChanged(object sender, EventArgs e)           // Changes the color of attribute's values
        {
            if (!IsValidFilename(textBox2.Text))                                //invalid characters: \  /  :  *  ?  "  <  >  |
            {
                MessageBox.Show("Invaled Character:  " + @"\" + " " +
                                                          "/" + " " +
                                                          ":" + " " +
                                                          "*" + " " +
                                                          "?" + " " +
                                                          "\"" + " " +
                                                          "<" + " " +
                                                          ">" + " " +
                                                          "|");
                textBox2.Text = textBox2.Text.Remove(textBox2.Text.Length - 1); // remove the invalid character
                textBox2.Select(textBox2.Text.Length, 0);                       // move cursor on last character after invalid character removed
                textBox2.ForeColor = Color.Gray;
            }
            if (textBox2.Text != "Attr_1,Attr_2,Attr_3,...")
                textBox2.ForeColor = Color.Black;
        }

        private void ok_button_Click(object sender, EventArgs e)                // check if textBoxes has values and hide Form
        {
            if (textBox1.Text != "Attribute Name" && textBox2.Text != "Attr_1,Attr_2,Attr_3,...")
            {
                valid = true;
                this.returned_attribute = textBox1.Text + " {" + textBox2.Text + "}";
                this.Close();
            }
            else if (textBox1.Text == "Attribute Name" && textBox2.Text != "Attr_1,Attr_2,Attr_3,...")
            {
                valid = false;
                MessageBox.Show("Attribute must have a name!");
            }

            else if (textBox1.Text != "Attribute Name" && textBox2.Text == "Attr_1,Attr_2,Attr_3,...")
            {
                valid = false;
                MessageBox.Show("Attribute must have values!");
            }
            else
            {
                valid = false;
                MessageBox.Show("Attribute must have a name and values!");
            }
        }

        private void cancel_button_Click(object sender, EventArgs e)            // Cancel event and close Form
        {
            valid = true;
            this.Close();
        }

        private void attribute_slip(string attrib)                              // slip attribute name and attribute values (for edit purposes)
        {
            textBox1.Text = attrib.Substring(0, attrib.IndexOf('{') - 1);       // get text before '{'

            int pFrom = attrib.IndexOf(" {") + " {".Length;                     // index of " {"
            int pTo = attrib.LastIndexOf("}");                                  // index of "}"
            textBox2.Text = attrib.Substring(pFrom, pTo - pFrom);               // get text from " {" to "}" without "{" and "}"
        }

        private bool IsValidFilename(string testName)                           // check if string - windows invalid filename
        {
            string strTheseAreInvalidFileNameChars = new string(System.IO.Path.GetInvalidFileNameChars());
            Regex regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

            if (regInvalidFileName.IsMatch(testName)) { return false; };

            return true;
        }

        
    }
}
