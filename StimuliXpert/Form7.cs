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
    public partial class Form7 : Form
    {
        /*
         ********************************* Form Informations *********************************
         * [Variables]
         * Counter_Font = counter's font
         * Counter_Color = counter's color
         * Cross_Font = cross's font
         * Cross_Color = cross'scolor
         * warnings = warning feature enabled or disabled
         * cursor_log = cursor log feature enabled or disabled
         * norm_val = normalize value
         * delimiter = selected delimiter
         * extension = selected extension
         * header = header feature enabled or disabled
         * 
         * Display_time = value of duration time for pictures
         * Between_time = value of time between media presensation
         * Counter_time = value of start up counter
         * Number_of_elements = value of emelemends between evaluation
         * Evaluation_time = value of evaluation time
         * Fixation_cross_time = value of cross's time
         * fixation_cross = fixation cross to disable
         * Keystroke_log = keystroke to disable
         * Random_Sequence = random sequence to enable
         * 
         * listBox1 = preview's form (From8) background color (for counter)
         * listBox2 = preview's form (From8) background color (for cross)
         * 
         * [Mesthods]
         * InitializeListBox1() = initialize listBox1 with colors options
         * InitializeListBox2() = initialize listBox2 with colors options
         * InitializeRadioButtons() = initialize radioButtons selections
         * InitializeCheckBoxes() = initialize checkboxes selections
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

        public Form7()
        {
            InitializeComponent();
            InitializeListBox1();                                               // initialize listBox1 with colors options
            InitializeListBox2();                                               // initialize listBox2 with colors options
            InitializeRadioButtons();                                           // initialize radioButtons selections
            InitializeCheckBoxes();                                             // initialize checkboxes selections
        }

        private void Form7_Load(object sender, EventArgs e)
        {
            ok_button.DialogResult = DialogResult.OK;
            cancel_button.DialogResult = DialogResult.Cancel;
            this.AcceptButton = ok_button;
            this.CancelButton = cancel_button;

            Counter_Font = Properties.Settings.Default.Counter_Font;            // counter's font  (current value)
            Counter_Color = Properties.Settings.Default.Counter_Color;          // counter's color (current value)
            label1.Font = Properties.Settings.Default.Counter_Font;             // counter's font  (label value)
            label1.ForeColor = Properties.Settings.Default.Counter_Color;       // counter's color (label value)

            Cross_Font = Properties.Settings.Default.Cross_Font;                // cross's font  (current value)
            Cross_Color = Properties.Settings.Default.Cross_Color;              // cross's color  (current value)
            label4.Font = Properties.Settings.Default.Cross_Font;               // cross's font  (label value)
            label4.ForeColor = Properties.Settings.Default.Cross_Color;         // cross's color  (label value)

            warnings = Properties.Settings.Default.Warnings_Counter_Cross;      // initialize warnings selection 
            path_visible = Properties.Settings.Default.Output_Path_Visible;     // initialize path selection
            collector = Properties.Settings.Default.Collector;                  // initialize memory bust option

            cursor_log = Properties.Settings.Default.Cursor_log;                // cursor log feature enabled or disabled
            norm_val = Properties.Settings.Default.Normalize_Value;             // normalize value
            numericUpDown1.Value = (decimal)Properties.Settings.Default.Normalize_Value; // normalize value to numericUpDown
            delimiter = Properties.Settings.Default.Delimiter;                  // selected delimiter
            extension = Properties.Settings.Default.Extension;                  // selected extension
            header = Properties.Settings.Default.Header;                        // header feature enabled or disabled

            Display_time = Properties.Settings.Default.Display_Time;                // initiale value of duration time for pictures
            numericUpDown7.Value = Properties.Settings.Default.Display_Time;        // initiale value of duration time for pictures
            Between_time = Properties.Settings.Default.Between_Time;                // initiale value of time between media presensation
            numericUpDown2.Value = Properties.Settings.Default.Between_Time;        // initiale value of time between media presensation
            Counter_time = Properties.Settings.Default.Counter_Time;                // initiale value of start up counter
            numericUpDown3.Value = Properties.Settings.Default.Counter_Time;        // initiale value of start up counter
            Number_of_elements = Properties.Settings.Default.Number_of_elements;    // initiale value of emelemends between evaluation
            numericUpDown4.Value = Properties.Settings.Default.Number_of_elements;  // initiale value of emelemends between evaluation
            Evaluation_time = Properties.Settings.Default.Evaluation_Time;          // initiale value of evaluation time
            numericUpDown5.Value = Properties.Settings.Default.Evaluation_Time;     // initiale value of evaluation time
            Fixation_cross_time = Properties.Settings.Default.Fixation_Cross_Time;  // initiale value of cross's time
            numericUpDown6.Value = Properties.Settings.Default.Fixation_Cross_Time; // initiale value of cross's time
            fixation_cross = Properties.Settings.Default.Fixation_Cross;            // initialize fixation cross to disable
            Keystroke_log = Properties.Settings.Default.Keystroke_log;              // initialize keystroke to disable
            Random_Sequence = Properties.Settings.Default.Random_Sequence;          // initialize random sequence to enable

            initiale_Display_time = Display_time;               // initiale value of duration time for pictures
            initiale_Between_time = Between_time;               // initiale value of time between media presensation
            initiale_Counter_time = Counter_time;               // initiale value of start up counter
            initiale_Number_of_elements = Number_of_elements;   // initiale value of emelemends between evaluation
            initiale_Evaluation_time = Evaluation_time;         // initiale value of evaluation time
            initiale_Fixation_cross_time = Fixation_cross_time; // initiale value of cross's time
            initiale_fixation_cross = fixation_cross;           // initialize fixation cross to disable
            initiale_Keystroke_log = Keystroke_log;             // initialize keystroke to disable
            initiale_Random_Sequence = Random_Sequence;         // initialize random sequence to enable
            changed = false;
        }

        //********************************* Declare Variables *********************************
        Font Counter_Font, Cross_Font;                                          // counter's font, cross's font 
        Color Counter_Color, Cross_Color;                                       // counter's color, cross's color 
        bool warnings, path_visible;                                            // warning feature enabled or disabled
        bool cursor_log;                                                        // cursor log feature enabled or disabled
        decimal norm_val;                                                       // normalize value
        string delimiter, extension;                                            // selected delimiter, selected extension
        bool header;                                                            // header feature enabled or disabled
        decimal Display_time, Between_time, Counter_time, Number_of_elements, Evaluation_time, Fixation_cross_time; // see informations
        bool fixation_cross, Keystroke_log, Random_Sequence;                    // see informations
        bool collector;

        public bool changed;                                                    // check if below variables has changed
        decimal initiale_Display_time, initiale_Between_time, initiale_Counter_time, initiale_Number_of_elements, initiale_Evaluation_time, initiale_Fixation_cross_time; // see informations
        bool initiale_fixation_cross, initiale_Keystroke_log, initiale_Random_Sequence; // see informations

        private void InitializeListBox1()                                       // initialize listBox1 with colors options
        {
            listBox1.Items.Add(System.Drawing.Color.Black);
            listBox1.Items.Add(System.Drawing.Color.Gray);
            listBox1.Items.Add(System.Drawing.Color.White);
            listBox1.Items.Add(System.Drawing.Color.Red);
            listBox1.Items.Add(System.Drawing.Color.Blue);
            listBox1.Items.Add(System.Drawing.Color.Green);
            listBox1.SelectedIndex = 0;                                         // Default value = Black
        }

        private void InitializeListBox2()                                       // initialize listBox2 with colors options
        {
            listBox2.Items.Add(System.Drawing.Color.Black);
            listBox2.Items.Add(System.Drawing.Color.Gray);
            listBox2.Items.Add(System.Drawing.Color.White);
            listBox2.Items.Add(System.Drawing.Color.Red);
            listBox2.Items.Add(System.Drawing.Color.Blue);
            listBox2.Items.Add(System.Drawing.Color.Green);
            listBox2.SelectedIndex = 0;                                         // Default value = Black
        }

        private void InitializeRadioButtons()                                   // initialize radioButtons selections
        {
            if (Properties.Settings.Default.Warnings_Counter_Cross)
                radioButton1.Checked = true;
            else
                radioButton2.Checked = true;

            if (Properties.Settings.Default.Output_Path_Visible)
                radioButton3.Checked = true;
            else
                radioButton4.Checked = true;

            if (!Properties.Settings.Default.Collector)
                radioButton17.Checked = true;
            else
                radioButton18.Checked = true;

            if (Properties.Settings.Default.Cursor_log)
                radioButton5.Checked = true;
            else
                radioButton6.Checked = true;

            if (Properties.Settings.Default.Delimiter == "space")
                radioButton12.Checked = true;
            else if (Properties.Settings.Default.Delimiter == "comma")
                radioButton11.Checked = true;
            else if (Properties.Settings.Default.Delimiter == "semicolon")
                radioButton10.Checked = true;
            else if (Properties.Settings.Default.Delimiter == "colon")
                radioButton9.Checked = true;
            else if (Properties.Settings.Default.Delimiter == "bar")
                radioButton8.Checked = true;
            else if (Properties.Settings.Default.Delimiter == "tab")
                radioButton7.Checked = true;

            if (Properties.Settings.Default.Extension == ".txt")
                radioButton14.Checked = true;
            else if (Properties.Settings.Default.Extension == ".csv")
                radioButton13.Checked = true;

            if (Properties.Settings.Default.Header)
                radioButton16.Checked = true;
            else
                radioButton15.Checked = true;
        }

        private void InitializeCheckBoxes()                                     // initialize checkboxes selections
        {
            checkBox3.Checked = Properties.Settings.Default.Fixation_Cross;
            checkBox1.Checked = Properties.Settings.Default.Keystroke_log;
            checkBox2.Checked = Properties.Settings.Default.Random_Sequence;
        }

        //********************************* Buttons *********************************
        private void ok_button_Click(object sender, EventArgs e)                // save the editable values
        {
            Properties.Settings.Default.Counter_Font = Counter_Font;
            Properties.Settings.Default.Counter_Color = Counter_Color;
            Properties.Settings.Default.Cross_Font = Cross_Font;
            Properties.Settings.Default.Cross_Color = Cross_Color;

            Properties.Settings.Default.Warnings_Counter_Cross = warnings;
            Properties.Settings.Default.Output_Path_Visible = path_visible;
            Properties.Settings.Default.Collector = collector;

            Properties.Settings.Default.Cursor_log = cursor_log;
            Properties.Settings.Default.Normalize_Value = norm_val;
            Properties.Settings.Default.Delimiter = delimiter;
            Properties.Settings.Default.Extension = extension;
            Properties.Settings.Default.Header = header;

            if(initiale_Display_time != Display_time || initiale_Between_time != Between_time || initiale_Number_of_elements != Number_of_elements || initiale_Counter_time != Counter_time || initiale_Evaluation_time != Evaluation_time || initiale_Fixation_cross_time != Fixation_cross_time || initiale_fixation_cross != fixation_cross || initiale_Keystroke_log != Keystroke_log || initiale_Random_Sequence != Random_Sequence)
            {
                Properties.Settings.Default.Display_Time = Display_time;
                Properties.Settings.Default.Between_Time = Between_time;
                Properties.Settings.Default.Counter_Time = Counter_time;
                Properties.Settings.Default.Number_of_elements = Number_of_elements;
                Properties.Settings.Default.Evaluation_Time = Evaluation_time;
                Properties.Settings.Default.Fixation_Cross_Time = Fixation_cross_time;
                Properties.Settings.Default.Fixation_Cross = fixation_cross;
                Properties.Settings.Default.Keystroke_log = Keystroke_log;
                Properties.Settings.Default.Random_Sequence = Random_Sequence;
                changed = true;
            }
            
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void cancel_button_Click(object sender, EventArgs e)            // close form without saving the values
        {
            this.Close();
        }

        //********************************* Counter Font edit Tab *********************************
        private void edit_counter_button_Click(object sender, EventArgs e)      // edit counter font and color
        {
            FontDialog fontdlg1 = new FontDialog();
            fontdlg1.ShowColor = true;

            fontdlg1.Font = Counter_Font;
            fontdlg1.Color = Counter_Color;

            if (fontdlg1.ShowDialog() == DialogResult.OK)
            {
                Counter_Font = fontdlg1.Font;
                Counter_Color = fontdlg1.Color;

                label1.Font = fontdlg1.Font;
                label1.ForeColor = fontdlg1.Color;

            }
        }

        private void preview_counter_button_Click(object sender, EventArgs e)   // preview the editing result
        {
            Form8 frm8 = new Form8(label1.Text,(Color)listBox1.SelectedItem,label1.Font,label1.ForeColor);

            frm8.Location = new System.Drawing.Point(Screen.PrimaryScreen.Bounds.Location.X, Screen.PrimaryScreen.Bounds.Location.Y);
            frm8.StartPosition = FormStartPosition.Manual;
            frm8.ClientSize = new System.Drawing.Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            frm8.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            frm8.ControlBox = false;

            frm8.Show();
        }

        private void default_counter_button_Click(object sender, EventArgs e)   // return to default values
        {
            Counter_Font = Properties.Settings.Default.Default_Counter_Font;
            Counter_Color = Properties.Settings.Default.Default_Counter_Color;

            label1.Font = Properties.Settings.Default.Default_Counter_Font;
            label1.ForeColor = Properties.Settings.Default.Default_Counter_Color;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)  // change panel color
        {
            panel1.BackColor = (Color)listBox1.SelectedItem;

            label5.Text = "";
            if (panel1.BackColor == Counter_Color)
                label5.Text = "Warning! Preview Background Color is the same with Counter Color";
        }

        private void label1_ForeColorChanged(object sender, EventArgs e)        // Check preview background and counter color
        {
            label5.Text = "";
            if (panel1.BackColor == Counter_Color)
                label5.Text = "Warning! Preview Background Color is the same with Counter Color";
        }

        //********************************* Cross Font edit Tab *********************************
        private void edit_cross_button_Click(object sender, EventArgs e)        // edit cross font and color
        {
            FontDialog fontdlg1 = new FontDialog();
            fontdlg1.ShowColor = true;

            fontdlg1.Font = Cross_Font;
            fontdlg1.Color = Cross_Color;

            if (fontdlg1.ShowDialog() == DialogResult.OK)
            {
                Cross_Font = fontdlg1.Font;
                Cross_Color = fontdlg1.Color;

                label4.Font = fontdlg1.Font;
                label4.ForeColor = fontdlg1.Color;
            }
        }

        private void preview_cross_button_Click(object sender, EventArgs e)     // preview the editing result
        {
            Form8 frm8 = new Form8(label4.Text, (Color)listBox2.SelectedItem, label4.Font, label4.ForeColor);

            frm8.Location = new System.Drawing.Point(Screen.PrimaryScreen.Bounds.Location.X, Screen.PrimaryScreen.Bounds.Location.Y);
            frm8.StartPosition = FormStartPosition.Manual;
            frm8.ClientSize = new System.Drawing.Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            frm8.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            frm8.ControlBox = false;

            frm8.Show();
        }

        private void default_cross_button_Click(object sender, EventArgs e)     // return to default values
        {
            Cross_Font = Properties.Settings.Default.Default_Cross_Font;
            Cross_Color = Properties.Settings.Default.Default_Cross_Color;

            label4.Font = Properties.Settings.Default.Default_Cross_Font;
            label4.ForeColor = Properties.Settings.Default.Default_Cross_Color;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)  // change panel color
        {
            panel2.BackColor = (Color)listBox2.SelectedItem;

            label6.Text = "";
            if (panel2.BackColor == Cross_Color)
                label6.Text = "Warning! Preview Background Color is the same with Cross Color";
        }

        private void label4_ForeColorChanged(object sender, EventArgs e)        // Check preview background and cross color
        {
            label6.Text = "";
            if (panel2.BackColor == Cross_Color)
                label6.Text = "Warning! Preview Background Color is the same with Cross Color";
        }

        //********************************* Options Tab *********************************
        private void radioButton1_CheckedChanged(object sender, EventArgs e)    // radioButtons 1-2 checked change, feature enabled or disabled
        {
            if (radioButton1.Checked)
                warnings = true;
            else
                warnings = false;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)    // radioButtons 3-4 checked change, feature enabled or disabled
        {
            if (radioButton3.Checked)
                path_visible = true;
            else
                path_visible = false;
        }

        private void radioButton17_CheckedChanged(object sender, EventArgs e)   // radioButtons 17-18 checked change, feature enabled or disabled
        {
            if (radioButton17.Checked)
                collector = false;
            else
                collector = true;
        }

        private void default_warnings_button_Click(object sender, EventArgs e)  // return to default values
        {
            // warnings = Properties.Settings.Default.Default_Warnings_Counter_Cross;
            if (Properties.Settings.Default.Default_Warnings_Counter_Cross)
                radioButton1.Checked = true;
            else
                radioButton2.Checked = true;

            // path_visible = Properties.Settings.Default.Output_Path_Visible;
            if (Properties.Settings.Default.Default_Output_Path_Visible)
                radioButton3.Checked = true;
            else
                radioButton4.Checked = true;

            if (!Properties.Settings.Default.Default_Collector)
                radioButton17.Checked = true;
            else
                radioButton18.Checked = true;
        }

        //********************************* Output Tab *********************************
        private void radioButton5_CheckedChanged(object sender, EventArgs e)    // radioButtons 6-6 checked change, feature enabled or disabled
        {
            if (radioButton5.Checked)
                cursor_log = true;
            else
                cursor_log = false;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)    // change normalize value
        {
            norm_val = numericUpDown1.Value;
        }

        private void radioButton12_CheckedChanged(object sender, EventArgs e)   // change user selection for delimiter
        {
            if (radioButton12.Checked == true)
                delimiter = "space";
            else if (radioButton11.Checked == true)
                delimiter = "comma";
            else if (radioButton10.Checked == true)
                delimiter = "semicolon";
            else if (radioButton9.Checked == true)
                delimiter = "colon";
            else if (radioButton8.Checked == true)
                delimiter = "bar";
            else if (radioButton7.Checked == true)
                delimiter = "tab";
        }

        private void radioButton14_CheckedChanged(object sender, EventArgs e)   // change user selection for output file extension
        {
            if (radioButton14.Checked)
                extension = ".txt";
            else
                extension = ".csv";
        }

        private void radioButton15_CheckedChanged(object sender, EventArgs e)   // header feature enabled or disabled
        {
            if (!radioButton15.Checked)
                header = true;
            else
                header = false;
        }

        private void default_cursor_button_Click(object sender, EventArgs e)    // return to default values
        {
            if (Properties.Settings.Default.Default_Cursor_log)
                radioButton5.Checked = true;
            else
                radioButton6.Checked = true;

            numericUpDown1.Value = (decimal)Properties.Settings.Default.Default_Normalize_Value;

            if (Properties.Settings.Default.Default_Delimiter == "space")
                radioButton12.Checked = true;
            else if (Properties.Settings.Default.Default_Delimiter == "comma")
                radioButton11.Checked = true;
            else if (Properties.Settings.Default.Default_Delimiter == "semicolon")
                radioButton10.Checked = true;
            else if (Properties.Settings.Default.Default_Delimiter == "colon")
                radioButton9.Checked = true;
            else if (Properties.Settings.Default.Default_Delimiter == "bar")
                radioButton8.Checked = true;
            else if (Properties.Settings.Default.Default_Delimiter == "tab")
                radioButton7.Checked = true;

            if (Properties.Settings.Default.Default_Extension == ".txt")
                radioButton14.Checked = true;
            else
                radioButton13.Checked = true;

            if (Properties.Settings.Default.Default_header)
                radioButton16.Checked = true;
            else
                radioButton15.Checked = true;
        }

        //********************************* Record Options Tab *********************************
        private void numericUpDown7_ValueChanged(object sender, EventArgs e)    // initiale value of duration time for pictures
        {
            Display_time = numericUpDown7.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)    // initiale value of time between media presensation
        {
            Between_time = numericUpDown2.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)    // initiale value of start up counter
        {
            Counter_time = numericUpDown3.Value;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)    // initiale value of emelemends between evaluation
        {
            Number_of_elements = numericUpDown4.Value;
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)    // initiale value of evaluation time
        {
            Evaluation_time = numericUpDown5.Value;
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)    // initiale value of cross's time
        {
            Fixation_cross_time = numericUpDown6.Value;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)       // initialize fixation cross to disable     
        {
            fixation_cross = checkBox3.Checked;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)       // initialize keystroke to disable
        {
            Keystroke_log = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)       // initialize random sequence to enable
        {
            Random_Sequence = checkBox2.Checked;
        }

        private void default_record_button_Click(object sender, EventArgs e)    // return to default values
        {
            numericUpDown7.Value = Properties.Settings.Default.Default_Display_Time;
            numericUpDown2.Value = Properties.Settings.Default.Default_Between_Time;
            numericUpDown3.Value = Properties.Settings.Default.Default_Counter_Time;
            numericUpDown4.Value = Properties.Settings.Default.Default_Number_of_elements;
            numericUpDown5.Value = Properties.Settings.Default.Default_Evaluation_Time;
            numericUpDown6.Value = Properties.Settings.Default.Default_Fixation_Cross_Time;

            checkBox3.Checked = Properties.Settings.Default.Default_Fixation_Cross;
            checkBox1.Checked = Properties.Settings.Default.Default_Keystroke_log;
            checkBox2.Checked = Properties.Settings.Default.Default_Random_Sequence;
        }

        

        




    }
}
