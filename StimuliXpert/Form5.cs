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
    public partial class Form5 : Form
    {
        /*
         ********************************* Form Informations *********************************
         * [Variables]
         * files_names = files names from listBox1 in Form1(main form)
         * attributes_names = matrix with attributes names from listBox2 in Form1(main form)
         * attributes_values = matrix with attributes values from listBox2 in Form1(main form)
         * attribute_ok = {"Error","OK"} for check reason in main form 
         * listView1 = list view for display to user the attributes for every media file (not editable)
         * cmb = # comboBoxes with attributes values for every attribute name
         * fn = comboBox with files names 
         * returned_attr_names = attributes name sent to Form1 - (attr1,attr2,...)
         * returned_files_names = files names with attribute values next to them - (filename,at_val1,at_val2,...)
         * valid = check if dialog result is OK
         * 
         * [Mesthods]
         * attribute_slip() = slip attribute name and attribute values 
         * createCombo() = create comboBoxes. 1 for files name and 1 for every attribute name 
         * createListView() = create the listview who contains the attributes for every media file 
         * editListView() = set or get attribute values for every mefia file
         * DrawForm() = Draw the listView (size of control and size of every column), draw the size of every comboBox and if ComboBoxes width > ~800 change line 
         * Get_Attributes() = return matrix[files_names.Length + 1] with attribute names (matrix[0]) and attributes values in every line (matrix[1...])
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
         ********************************* Attribute Set to media files Form **************************
         */

        public Form5(string[] names, string[] attrib)
        {
            attributes_names = new string[attrib.Length];                                   // see Form Informations
            attributes_values = new string[attrib.Length];                                  // see Form Informations

            files_names = names;
            attribute_slip(attrib);                                                         // slip attribute name and attribute values 

            createListView();                                                               // create the listview who contains the attributes for every media file 

            InitializeComponent();

        }

        private void Form5_Load(object sender, EventArgs e)
        {
            ok_button.DialogResult = System.Windows.Forms.DialogResult.OK;
            cancel_button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.AcceptButton = ok_button;
            this.CancelButton = cancel_button;

            createCombo();                                                                  // create comboBoxes. 1 for files name and 1 for every attribute name
            editListView();                                                                 // set or get attribute values for every mefia file
            DrawForm();                                                                     // Draw the listView (size of control and size of every column), draw the size of every comboBox and if ComboBoxes width > ~800 change line 
        }

        //********************************* Declare Variables *********************************
        string[] files_names, attributes_names, attributes_values;                          // see Form Informations
        ListView listView1 = new ListView();                                                // Initialize the ListView control and add columns to it.
        ComboBox[] cmb;                                                                     // # comboBoxes with attributes values for every attribute name 
        ComboBox fn;                                                                        // comboBox with files names 

        public string returned_attr_names { get; set; }                                     // attributes name sent to Form1 - (attr1,attr2,...)
        public string[] returned_files_names { get; set; }                                  // files names with attribute values next to them - (filename,at_val1,at_val2,...)
        bool valid = true;                                                                  // check if dialog result is OK

        //********************************* Form events *********************************
        private void Form5_FormClosing(object sender, FormClosingEventArgs e)               // From closing event
        {
            if (!valid)
                e.Cancel = true;
        }

        //********************************* Buttons *********************************
        private void set_button_Click(object sender, EventArgs e)                           // Set comboBoxes values to listView
        {
            if (fn.SelectedIndex >= 0)
            {
                int i;
                for (i = 0; i < cmb.Length; i++)
                {
                    if (cmb[i].SelectedIndex < 0)
                        break;
                }
                if (i == cmb.Length)
                {
                    for (i = 0; i < cmb.Length; i++)
                    {
                        listView1.Items[fn.SelectedIndex].SubItems[i + 1].Text = cmb[i].Text;
                    }
                }
                else
                    MessageBox.Show("All attributes must have values!");
            }
            else
            {
                MessageBox.Show("Select file first!");
                fn.Focus();
            }

        }

        private void ok_button_Click(object sender, EventArgs e)                            // Hide Form and set attribute_ok = "OK"
        {
            // Call FindItemWithText, sending output to MessageBox.
            ListViewItem item1 = listView1.FindItemWithText(" ");
            if (item1 == null)
            {
                this.returned_files_names = new string[files_names.Length];

                string row = ""; //attributes_names[0];
                for (int i = 0; i < cmb.Length; i++)
                {
                    row = row + attributes_names[i] + ",";
                }
                row = row.Remove(row.Length - 1); // remove the invalid character
                this.returned_attr_names = row;

                for (int i = 0; i < files_names.Length; i++)
                {
                    row = "";// listView1.Items[i].SubItems[1].Text;
                    for (int j = 0; j < cmb.Length; j++)
                    {
                        row = row + listView1.Items[i].SubItems[j + 1].Text + ",";
                    }
                    row = row.Remove(row.Length - 1); // remove the invalid character
                    returned_files_names[i] = row;
                }

                valid = true;
                this.Hide();
            }
            else
            {
                valid = false;
                MessageBox.Show("The file: " + item1.SubItems[0].ToString() + " has null attributes!");
            }
        }

        private void cancel_button_Click(object sender, EventArgs e)                        // Cancel the event and close Form
        {
            valid = true;
            this.Close();
        }

        //********************************* Methods *********************************
        private void attribute_slip(string[] attrib)                                        // slip attribute name and attribute values
        {
            for (int i = 0; i < attrib.Length;i++ )
            {
                attributes_names[i] = attrib[i].Substring(0, attrib[i].IndexOf('{') - 1);   // get text before '{'

                int pFrom = attrib[i].IndexOf(" {") + " {".Length;                          // index of " {"
                int pTo = attrib[i].LastIndexOf("}");                                       // index of "}"
                attributes_values[i] = attrib[i].Substring(pFrom, pTo - pFrom);             // get text from " {" to "}" without "{" and "}"
            }
        }

        private void createCombo()                                                          // create comboBoxes. 1 for files name and 1 for every attribute name
        {
            fn = new ComboBox();
            foreach (string value in files_names)
                fn.Items.Add(System.IO.Path.GetFileName(value));
                
            this.Controls.Add(fn);

            fn.SelectedIndexChanged += new System.EventHandler(fn_SelectedIndexChanged);

            cmb = new ComboBox[attributes_names.Length];
            for (int i = 0; i < attributes_names.Length; i++)
            {
                ComboBox newBox = new ComboBox();
                newBox.Text = attributes_names[i];
                cmb[i] = newBox;

                string[] values = attributes_values[i].Split(',');
                foreach (string value in values)
                    cmb[i].Items.Add(value);
                
                this.Controls.Add(cmb[i]);
            }
        }

        private void fn_SelectedIndexChanged(object sender, System.EventArgs e)             // when fn(comboBox) selected index Changed cmb selected index = -1 or point to value
        {
            if (fn.SelectedIndex >= 0) 
            {
                for (int i = 0; i < cmb.Length; i++)
                {
                    int resultIndex = -1;
                    string temp_text = listView1.Items[fn.SelectedIndex].SubItems[i+1].Text;
                    resultIndex = cmb[i].FindStringExact(temp_text);
                    cmb[i].SelectedIndex = resultIndex;
                }
            }
        }

        private void createListView()                                                       // create the listview who contains the attributes for every media file
        {
            
            listView1.BorderStyle = BorderStyle.FixedSingle;
            listView1.CheckBoxes = false;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.View = View.Details;
            listView1.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listView1.LabelEdit = false;
            listView1.MultiSelect = false;
            listView1.Name = "listView1";
            listView1.Sorting = SortOrder.None;                     // Set the initial sorting type for the ListView.
            listView1.TabIndex = 0;

            listView1.Location = new Point(10, 10);                 // Set the location of the ListView control. 
            listView1.Size = new Size(650, 250);                    // Set the size of the ListView control.

            // Add columns and set their text. 
            listView1.Columns.Add("Filenames\\Attributes", -2, HorizontalAlignment.Center);

            for (int i = 0; i < attributes_names.Length; i++)
            {
                listView1.Columns.Add(attributes_names[i], -2, HorizontalAlignment.Center);
            }
            listView1.Columns.Add(null, 0, HorizontalAlignment.Left);
            this.Controls.Add(listView1);

        }

        private void editListView()                                                         // set or get attribute values for every mefia file
        {
            for (int i = 0; i < listView1.Columns.Count-1; i++)
            {
                listView1.Columns[i].Width = 15 + listView1.Columns[i].Width;
            }

            // Create ListView items to add to the control.
            for (int index = 0; index < files_names.Length; index++) 
            {
                string[] row = new string[cmb.Length + 1];
                row[0] = System.IO.Path.GetFileName(files_names[index]);
                for (int i = 0; i < cmb.Length; i++)
                {
                    row[i + 1] = " ";
                }
                ListViewItem itm = new ListViewItem(row);
                this.listView1.Items.Add(itm);
            }
        }

        private void DrawForm() // Draw the listView (size of control and size of every column), draw the size of every comboBox and if ComboBoxes width > ~800 change line
        {
            int width = 0;
            int count_width = 0;
            for (int i = 0; i < listView1.Columns.Count-1; i++)
            {
                width = width + listView1.Columns[i].Width;
            }
            if (width < 650)
            {
                for (int i = 0; i < listView1.Columns.Count - 1; i++)
                {
                    listView1.Columns[i].Width = 650 / (listView1.Columns.Count - 1); //(650 - listView1.Columns[0].Width) / (listView1.Columns.Count - 2);
                }
            }
            else if (width > 800)
            {
                for (int i = 0; i < listView1.Columns.Count - 1; i++)
                {
                    count_width = count_width + listView1.Columns[i].Width;
                    if (count_width > 800)
                        break;
                }
                listView1.Width = count_width+10;
            }

            if (count_width < 800)
                count_width = 800;


            int height = 25;
            int spacing = 3;
            fn.Location = new Point(10, 268);
            fn.Size = new Size(listView1.Columns[0].Width , height);

            int start_point = 10;
            int cmb_width = listView1.Columns[0].Width;
            int new_line = 0;
            int new_line_cmb_width = listView1.Columns[0].Width;
            for (int i = 0; i < cmb.Length; i++) 
            {
                if (cmb_width < count_width)
                {
                    cmb[i].Location = new Point(start_point + spacing + cmb_width, 268);
                    cmb[i].Size = new Size(listView1.Columns[i+1].Width - spacing, height);
                    cmb_width = cmb_width + listView1.Columns[i+1].Width;
                }
                else 
                {
                    cmb[i].Location = new Point(start_point + spacing + new_line_cmb_width, 295);
                    cmb[i].Size = new Size(listView1.Columns[i + 1].Width - spacing, height);
                    new_line_cmb_width = new_line_cmb_width + listView1.Columns[i + 1].Width;
                    new_line++;
                }
            }

            int Y = 268 + height;
            if (cmb_width >= count_width) 
            {
                Y = 295 + height;
            }

            cancel_button.Location = new Point(10 + listView1.Width - cancel_button.Width, Y);
            ok_button.Location = new Point(cancel_button.Location.X - 5 - ok_button.Width, Y);
            set_button.Location = new Point(ok_button.Location.X - 5 - set_button.Width, Y);
            
        }

        
    }
}
