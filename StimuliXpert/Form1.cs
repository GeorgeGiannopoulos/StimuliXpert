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
using System.IO;

namespace StimuliXpert
{
    public partial class Form1 : Form
    {
        /*
         ********************************* Form Informations *********************************
         * [Variables]
         * listBox1 = media Items
         * comboBox1 = bakcground color of stimuli
         * comboBox2 = display position
         * comboBox3 = system's monitors for presentation window
         * 
         * checkBox1 = enable/disable keystroke log 
         * checkBox2 = enable/disable  random sequence
         *          
         * textBox1 = output name of stimuli's created file
         * 
         * groupBox1 = Stimuli files
         * groupBox2 = Record Options
         * groupBox3 = Stimuli Control
         * groupBox4 = Screen selection
         * 
         * numericUpDown1 = duration time for pictures
         * numericUpDown2 = time between media presensation
         * numericUpDown3 = start up counter value
         * 
         * stimuliState = state of stimuli = {"initialize","start","pause","continue","stop","ended"}
         * Media_files = contians Media_files paths after every change of listBox1 for backup purposes
         * attributes_names =  initialize attributes_names
         * frm2 = the form who control media's presensation          
         * screens = connected system's screens (=Screen.PrimaryScreen)
         * primary = system's primary display
         * main_sreen = main window's screen
         * presentation = presenstation window's screen
         * stimuli_output_name = name of emotiv output file
         * stimuli_output_path = path of emotiv output file
         * 
         * pcbx = emotiv pictureboxes
         * emotiv_battery_level = emotiv battery level
         * emotiv_status = emotiv status
         * emotiv_wireless_signal_status = emotiv wireless signal status
         * emotiv_Contacts_Quality = emotiv Contacts Quality
         * 
         * [Mesthods]
         * initializeform1() = initialize the state of Form1(mainform)
         * initializeform2() = initialize the state of Form2(presentation form)
         * initializeStimuli() = initialize stimuli options
         * initializeRecordOptions() = initialize record options (system properties)
         * comboBox1Load() = add and initialize bakcground color of stimuli
         * comboBox2Load() = add and initialize display position
         * comboBox3Load() = initialize screens
         * createpictureboxes() = create pictureboxes for emotiv status
         * initializepictureBoxes() = initialize pictureboxes
         * drawpictureBoxes() = draw pictureboxes
         * button_control() = enable and disable the presensation buttons
         * IsValidFilename() = check if string - windows invalid filename
         * checkColor() = check backgroung, counter and cross colors
         * output_path_show() = display output path if feature enable
         * Detect() = detect dilimiter in file
         * delimiter_return() = return delimiter from input = name
         * initialize_delimiter() = using detect method to detect delimiter
         * MakeUnique() = make unique name for txt or csv after conversion
         * garbage_collector() = free memory
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

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            createpictureboxes();
            initializepictureBoxes();

            listBox1.DisplayMember = "safefilename";        // Display Member & Value Member
            listBox1.ValueMember = "filename";              // of listBox1

            comboBox1Load();                                // add and initialize bakcground color of stimuli
            comboBox2Load();                                // add and initialize display position

            primary = Screen.PrimaryScreen;                 // system's primary display
            main_screen = primary;                          // main form screen = primary
            presentation_screen = primary;                  // presentation form screen = primary

            initializeform1();                              // initialize the state of Form1(mainform)
            initializeStimuli();                            // initialize stimuli options

            groupBox4.Enabled = false;                      // initialize screen selection groupbox state
            groupBox4.Visible = false;                      // screen selection groupbox = not visible
            differentscreenstoolStripMenuItem.Checked = false;  // initialize screen selection features
            if (screens.Length == 1)                            // system's monitor = 1 then...
                differentscreenstoolStripMenuItem.Enabled = false;

            groupBox6.Visible = false;                      // attributes groupbox = not visible
            attributesToolStripMenuItem.Checked = false;    // initialize screen selection features
            attributes_button.Visible = false;              // attributes button is hidden when attribute menu is hidden
            clear_attribs_button.Visible = false;           // clear attributes button is hidden when attribute menu is hidden

            stimuli_output_path = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath + "\\Resources\\Output\\";
            output_path_show();                            // display output path if feature enable

            emotiv.BatteryChanged += new EventHandler<FloatEventArgs>(emotiv_BatteryChanged);
            emotiv.Connected_Changed += new EventHandler<BoolEventArgs>(emotiv_Connected_Changed);
            emotiv.Wireless_Signal_Changed += new EventHandler<IntEventArgs>(emotiv_Wireless_Signal_Changed);
            emotiv.Contacts_quality_Changed += new EventHandler<TextEventArgs>(emotiv_Contacts_Quality_Changed);
            emotiv.check();
            timer1.Interval = 1000;
            timer1.Start();
        }

        //********************************* Declare Variables *********************************
        static public string stimuliState;                                      // stimuliState = {"initialize","start","pause","continue","stop","ended"}
        public string stimuli_output_name;                                      // name of emotiv output file
        string stimuli_output_path;                                             // path of emotiv output file
        static public string stimuli_start_time;                                // start date and time of emotiv stimuli
        string attributes_names = "";                                           // initialize attributes_names
        List<String> Media_files = new List<String>();                          // contians Media_files paths after every change of listBox1 for backup purposes
        Form2 frm2 = new Form2();                                               // the form who control media's presensation
        Screen[] screens = Screen.AllScreens;                                   // screens = connected system's screens (primary display = Screen.PrimaryScreen)
        Screen primary, main_screen, presentation_screen;                       // primary = system's primary display, main_sreen = main window's screen,presentation = presenstation window's screen
        PictureBox[] pcbx;                                                      // emotiv pictureboxes

        float emotiv_battery_level;                                             // emotiv battery level
        bool emotiv_status;                                                     // emotiv status
        int emotiv_wireless_signal_status;                                      // emotiv wireless signal status
        string[] emotiv_Contacts_Quality = new string[14];                      // emotiv Contacts Quality

        mySuperEmotiv emotiv = new mySuperEmotiv(0);                            // create emotiv object

        class newitem                                                           // new item for listBox1 who represent media files
        {
            public string filename { get; set; }        // the path of the media file
            public string safefilename { get; set; }    // the name of the media file with the file extention (without the path)

            public override string ToString()
            {
                return filename;
            }
        }

        private void initializeform1()                                          // initialize the state of Form1(mainform)
        {
            Location = new System.Drawing.Point(main_screen.Bounds.Location.X + 120, main_screen.Bounds.Location.Y + 120); // Start location of the form
            StartPosition = FormStartPosition.Manual;           // enable manual option for form position

        }

        private void initializeform2()                                          // initialize the state of Form2(presentaion form = "child form")
        {
            frm2.Location = new System.Drawing.Point(presentation_screen.Bounds.Location.X, presentation_screen.Bounds.Location.Y); // location = (0,0) of presentation screen
            frm2.StartPosition = FormStartPosition.Manual;                      // enable manual option for form position

            frm2.ClientSize = new System.Drawing.Size(presentation_screen.Bounds.Width, presentation_screen.Bounds.Height);
            frm2.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;   // no border
            frm2.ControlBox = false;                                            // no controlBox (minimize,maximize,close)
            frm2.BackColor = (Color)comboBox1.SelectedItem;                     // backcolor = user selection
        }

        private void initializeStimuli()                                        // initialize stimuli options
        {
            Media_files.Clear();
            listBox1.Items.Clear();         // clear media from listBox1 from previous experiment
            listBox2.Items.Clear();         // clear attributes from listBox2 from previous experiment

            initializeRecordOptions();

            comboBox1.SelectedIndex = 0;    // initiale value of bakcground color of stimuli
            comboBox2.SelectedIndex = 0;    // initiale value of display position

            textBox1.Text = "Stimuli";      // default stimuli file name
            stimuli_output_name = textBox1.Text;// default output name

            stimuliState = "initialize";    // initialize stimuli state
            button_control(stimuliState);   // initialize stimuli button's state 
        }

        private void initializeRecordOptions()                                  // initialize record options (system properties)
        {
            numericUpDown1.Value = Properties.Settings.Default.Display_Time;        // initiale value of duration time for pictures
            numericUpDown2.Value = Properties.Settings.Default.Between_Time;        // initiale value of time between media presensation
            numericUpDown3.Value = Properties.Settings.Default.Counter_Time;        // initiale value of start up counter
            numericUpDown4.Value = Properties.Settings.Default.Number_of_elements;  // initiale value of emelemends between evaluation
            numericUpDown5.Value = Properties.Settings.Default.Evaluation_Time;     // initiale value of evaluation time
            numericUpDown6.Value = Properties.Settings.Default.Fixation_Cross_Time; // initiale value of cross's time
            numericUpDown6.Enabled = Properties.Settings.Default.Fixation_Cross;    // when fixation cross feature is disable then numericUpDown6 is disable

            checkBox1.Checked = Properties.Settings.Default.Keystroke_log;      // initialize keystroke to disable
            checkBox2.Checked = Properties.Settings.Default.Random_Sequence;    // initialize random sequence to enable
            checkBox3.Checked = Properties.Settings.Default.Fixation_Cross;     // initialize fixation cross to disable
        }

        //********************************* Create emotiv Gui *********************************
        private void createpictureboxes()                                       // create pictureboxes for emotiv status
        {
            Point[] points = new Point[] { 
                new Point { X = 65, Y = 89 }, 
                new Point { X = 36, Y = 121 },
                new Point { X = 87, Y = 115 },
                new Point { X = 52, Y = 140 },
                new Point { X = 26, Y = 166 },
                new Point { X = 56, Y = 229 },
                new Point { X = 84, Y = 258 },
                new Point { X = 135, Y = 258 },
                new Point { X = 161, Y = 229 },
                new Point { X = 192, Y = 166 },
                new Point { X = 164, Y = 140 },
                new Point { X = 129, Y = 115 },
                new Point { X = 182, Y = 121 },
                new Point { X = 152, Y = 89 } };

            pcbx = new PictureBox[14];

            for (int i = 0; i < 14; i++)
            {
                PictureBox newBox = new PictureBox();
                newBox.Location = points[i];
                newBox.Size = new Size(20, 20);
                newBox.BackColor = Color.White;
                newBox.SizeMode = PictureBoxSizeMode.Zoom;
                newBox.Visible = false;

                pcbx[i] = newBox;
                this.Controls.Add(pcbx[i]);
            }

        }

        private void initializepictureBoxes()                                   // initialize pictureboxes
        {
            for (int i = 0; i < 14; i++)
            {
                pcbx[i].Visible = false;
            }
        }

        //********************************* File Menu *********************************
        private void newToolStripMenuItem_Click(object sender, EventArgs e)     // initialize the experiment options
        {
            initializeStimuli();
        }

        private void savetoolStripSeparator1_Click(object sender, EventArgs e)  // save current experiment options
        {
            string[] safefile = new string[35 + listBox1.Items.Count + listBox2.Items.Count];  
            int i = 0;
            safefile[i++] = "[Experiment options]";                                     //[0]
            safefile[i++] = ""; // New line                                             //[1]
            safefile[i++] = "[Time options]";                                           //[2]
            safefile[i++] = "duration time = " + numericUpDown1.Value.ToString();       //[3]
            safefile[i++] = "between time = " + numericUpDown2.Value.ToString();        //[4]
            safefile[i++] = "counter time = " + numericUpDown3.Value.ToString();        //[5]
            safefile[i++] = ""; // New line                                             //[6]
            safefile[i++] = "[Evaluation options]";                                     //[7]
            safefile[i++] = "# before evaluation = " + numericUpDown4.Value.ToString(); //[8]
            safefile[i++] = "evaluation time = " + numericUpDown5.Value.ToString();     //[9]
            safefile[i++] = ""; // New line                                             //[10]
            safefile[i++] = "[Display options]";                                        //[11]
            safefile[i++] = "background color = " + comboBox1.SelectedIndex.ToString(); //[12]
            safefile[i++] = "display mode = " + comboBox2.SelectedIndex.ToString();     //[13]
            safefile[i++] = ""; // New line                                             //[14]
            safefile[i++] = "[Key Options]";                                            //[15]
            safefile[i++] = "Fixation Cross = " + checkBox3.Checked.ToString();         //[16]
            safefile[i++] = "Fixation Cross time = " + numericUpDown6.Value.ToString(); //[17]
            safefile[i++] = "KeyStroke log = " + checkBox1.Checked.ToString();          //[18]
            safefile[i++] = "Random Sequence = " + checkBox2.Checked.ToString();        //[19]
            safefile[i++] = ""; // New line                                             //[20]
            safefile[i++] = "[Imported files]";                                         //[21]
            safefile[i++] = "number of items = " + listBox1.Items.Count.ToString();     //[22]
            safefile[i++] = ""; // New line                                             //[23]
            safefile[i++] = "[media start]";                                            //[24]
            for (int index = 0; index < listBox1.Items.Count; index++)  // add media files paths in options txt file
            {
                safefile[i++] = "filepath = " + listBox1.Items[index].ToString();
            }
            safefile[i++] = "[media stop]";                                             //[25+N]
            safefile[i++] = ""; // New line                                             //[26+N]
            safefile[i++] = "[Attributes]";                                             //[27+N]
            safefile[i++] = "number of attributes = " + listBox2.Items.Count.ToString();//[28+N]
            safefile[i++] = ""; // New line                                             //[29+N]
            safefile[i++] = "[Attributes start]";                                       //[30+N]
            for (int j = 0; j < listBox2.Items.Count; j++)  // add media files paths in options txt file
            {
                safefile[i++] = "attribute = " + listBox2.Items[j].ToString();
            }
            safefile[i++] = "[Attributes stop]";                                        //[31+N+k]

            safefile[i++] = ""; // New line                                             //[32+N+k]
            safefile[i++] = "[Output Path]";                                            //[33+N+k]
            safefile[i++] = "output path = " + stimuli_output_path;                     //[34+N+k]


            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();   // The user selected a folder and pressed the OK button.
            folderBrowserDialog1.SelectedPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Resources\\Settings";
            folderBrowserDialog1.Description = "Select the directory that you want to use.\nDefault directory is: \\Resources\\Settings (in .exe folder).";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                path = new Uri(path).LocalPath;
                string date = string.Format("{0:dd-MM-yyy_hh-mm-ss}", DateTime.Now);
                System.IO.File.WriteAllLines(path + "\\Stimuli_Options_" + date + ".txt", safefile);
                MessageBox.Show("Options saved!");
            }

        }

        private void loadtoolStripMenuItem1_Click(object sender, EventArgs e)   // load previous experiment options
        {
            initializeStimuli();

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Resources\\Settings";

            openFileDialog1.Filter = "Text files (*.txt)|*.txt";
            openFileDialog1.Title = "Browse settings file to load";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog1.FileName;

                string[] loadfile = System.IO.File.ReadAllLines(path);

                numericUpDown1.Value = Decimal.Parse(loadfile[3].Substring(loadfile[3].LastIndexOf('=') + 2));  // load dutation time from saved file
                numericUpDown2.Value = Decimal.Parse(loadfile[4].Substring(loadfile[4].LastIndexOf('=') + 2));  // load between time from saved file
                numericUpDown3.Value = Decimal.Parse(loadfile[5].Substring(loadfile[5].LastIndexOf('=') + 2));  // load start up counter value from saved file

                numericUpDown4.Value = Decimal.Parse(loadfile[8].Substring(loadfile[8].LastIndexOf('=') + 2));  // load # of elements before evaluation from saved file
                numericUpDown5.Value = Decimal.Parse(loadfile[9].Substring(loadfile[9].LastIndexOf('=') + 2));  // load evaluation time  from saved file

                comboBox1.SelectedIndex = int.Parse(loadfile[12].Substring(loadfile[12].LastIndexOf('=') + 2)); // load background color of stimuli from saved file
                comboBox2.SelectedIndex = int.Parse(loadfile[13].Substring(loadfile[13].LastIndexOf('=') + 2)); // load display position from saved file

                checkBox3.Checked = Boolean.Parse(loadfile[16].Substring(loadfile[16].LastIndexOf('=') + 2));   // load Fixation Cross value from saved file
                numericUpDown6.Value = Decimal.Parse(loadfile[17].Substring(loadfile[17].LastIndexOf('=') + 2));// load Fixation Cross time from saved file
                checkBox1.Checked = Boolean.Parse(loadfile[18].Substring(loadfile[18].LastIndexOf('=') + 2));   // load keystroke log value from saved file
                checkBox2.Checked = Boolean.Parse(loadfile[19].Substring(loadfile[19].LastIndexOf('=') + 2));   // load random sequence value from saved file

                int N = int.Parse(loadfile[22].Substring(loadfile[22].LastIndexOf('=') + 2));
                if (N != 0)                                                 // load media files from saved files
                {
                    for (int i = 0; i < N; i++) 
                    {
                        string temp = loadfile[i + 25].Substring(loadfile[i + 25].LastIndexOf('=') + 2);
                        if (temp.IndexOf("#@#") == -1)
                        {
                            listBox1.Items.Add(new newitem { safefilename = System.IO.Path.GetFileName(temp), filename = temp });
                            Media_files.Add(temp);
                        }
                        else
                        {
                            string[] tokens = temp.Split(new string[] { "#@#" }, StringSplitOptions.None);
                            listBox1.Items.Add(new newitem { safefilename = System.IO.Path.GetFileName(tokens[0]) + " - " + tokens[1], filename = temp });
                            Media_files.Add(temp);
                        }
                    }
                }
                
                int K = int.Parse(loadfile[28 + N].Substring(loadfile[28 + N].LastIndexOf('=') + 2));
                if (K != 0)                                                 // load media files from saved files
                {
                    for (int i = 0; i < K; i++)
                    {
                        listBox2.Items.Add(loadfile[i + 31 + N].Substring(loadfile[i + 31 + N].LastIndexOf('=') + 2));
                    }
                }

                string path_temp_default = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath + "\\Resources\\Output\\";
                string path_temp_loaded = loadfile[34 + N + K].Substring(loadfile[34 + N + K].LastIndexOf('=') + 2);
                if (Directory.Exists(path_temp_loaded))
                    stimuli_output_path = loadfile[34 + N + K].Substring(loadfile[34 + N + K].LastIndexOf('=') + 2);
                else 
                    stimuli_output_path = path_temp_default;
                output_path_show();

                MessageBox.Show("Options loaded!" + N.ToString() + "files imported and " + K.ToString() + " attributes imported.");

            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)    // Exit from Application
        {
            frm2.Dispose();     // Dispose Form2 window. Form2.Hide = enabled while the main Form is running
            Application.Exit(); // Close all forms.
        }

        //********************************* Tools Menu *********************************
        private void differentscreenstoolStripMenuItem_Click(object sender, EventArgs e)            // enable/disable screen selection features
        {
            if (differentscreenstoolStripMenuItem.Checked == true)              // if feature enabled
                differentscreenstoolStripMenuItem.Checked = false;
            else                                                                // if feature disabled
                differentscreenstoolStripMenuItem.Checked = true;
        }

        private void differentscreenstoolStripMenuItem_CheckedChanged(object sender, EventArgs e)   //enable/disable screen selection features
        {
            if (differentscreenstoolStripMenuItem.Checked == true && screens.Length > 1)                // if feature enabled and screens > 1
            {
                groupBox4.Visible = true;
                groupBox4.Enabled = true;

                label8.Text = main_screen.DeviceName.Substring(4, 8);
                comboBox3Load();
            }
            else                                                                // if feature disabled
            {
                groupBox4.Visible = false;
                groupBox4.Enabled = false;
                main_screen = Screen.FromControl(this);
                presentation_screen = main_screen;
            }
            draw_menu();
        }

        private void attributesToolStripMenuItem_Click(object sender, EventArgs e)                  //enable/disable attributes features
        {
            if (attributesToolStripMenuItem.Checked == true)                    // if feature enabled
                attributesToolStripMenuItem.Checked = false;
            else                                                                // if feature disabled
                attributesToolStripMenuItem.Checked = true;
        }

        private void attributesToolStripMenuItem_CheckedChanged(object sender, EventArgs e)         //enable/disable attributes features
        {
            if (attributesToolStripMenuItem.Checked == true)                // if feature enabled and screens > 1
            {
                groupBox6.Visible = true;
                groupBox6.Enabled = true;
                attributes_button.Visible = true;
                clear_attribs_button.Visible = true;

            }
            else                                                                // if feature disabled
            {
                groupBox6.Visible = false;
                groupBox6.Enabled = false;
                attributes_button.Visible = false;
                clear_attribs_button.Visible = false;
            }
            draw_menu();
        }

        private void ConvertdelimitedfilestoolStripMenuItem1_Click(object sender, EventArgs e)      // convert delimited files to txt or csv
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();  // open dialog Box for user to select the file to load
            openFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Resources\\Output";
            openFileDialog1.Filter = "Delimited files (*.txt,*.csv)|*.txt;*.csv";

            openFileDialog1.Title = "Browse EEG file to load";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string input_path = openFileDialog1.FileName;
                string output_path = Path.GetDirectoryName(input_path) + "\\" + Path.GetFileNameWithoutExtension(input_path);
                output_path = new Uri(output_path).LocalPath;

                Form10 frm10 = new Form10();
                frm10.ShowDialog();

                if (frm10.DialogResult == DialogResult.OK)
                {
                    string delimiter = delimiter_return(frm10.return_delimeter);
                    string extension = frm10.return_extension;
                    bool clear = frm10.return_clear;
                    bool add_header = frm10.rerurn_header;

                    string output_path_Unique = MakeUnique(output_path + extension);
                    string delimiter_original = initialize_delimiter(input_path);

                    TextWriter file = new StreamWriter(output_path_Unique, true);

                    using (StreamReader reader = new StreamReader(input_path))
                    {
                        string header = reader.ReadLine();
                        string header_temp = "File_Name" + delimiter_original + "Valence" + delimiter_original + "Arousal" + delimiter_original + "time_stamp" + delimiter_original +
                                "AF3" + delimiter_original + "F7" + delimiter_original + "F3" + delimiter_original + "FC5" + delimiter_original + "T7" + delimiter_original + "P7" + delimiter_original + "O1" + delimiter_original +
                                "O2" + delimiter_original + "P8" + delimiter_original + "T8" + delimiter_original + "FC6" + delimiter_original + "F4" + delimiter_original + "F8" + delimiter_original + "AF4";

                        if (add_header && (header != header_temp))
                        {
                            file.WriteLine("File_Name" + delimiter + "Valence" + delimiter + "Arousal" + delimiter + "time_stamp" + delimiter +
                                "AF3" + delimiter + "F7" + delimiter + "F3" + delimiter + "FC5" + delimiter + "T7" + delimiter + "P7" + delimiter + "O1" + delimiter +
                                "O2" + delimiter + "P8" + delimiter + "T8" + delimiter + "FC6" + delimiter + "F4" + delimiter + "F8" + delimiter + "AF4");
                        }
                    }

                    if (!clear)
                    {
                        using (StreamReader reader = new StreamReader(input_path))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                StringBuilder b = new StringBuilder(line);
                                b.Replace(delimiter_original, delimiter);
                                file.WriteLine(b);
                            }
                        }
                    }
                    else
                    {
                        using (StreamReader reader = new StreamReader(input_path))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                string file_name = line.Split(new string[] { delimiter_original }, StringSplitOptions.None)[0];
                                if (file_name != "ST_counter" && file_name != "ST_between" && file_name != "ST_media" && file_name != "ST_evaluation" && file_name != "ST_pause" && file_name != "ST_fixation_cross" && file_name != "ST_ended" && file_name != "Arousal.png" && file_name != "Valence.png" && file_name != "fixation_cross.png")
                                {
                                    StringBuilder b = new StringBuilder(line);
                                    b.Replace(delimiter_original, delimiter);
                                    file.WriteLine(b);
                                }
                            }
                        }
                    }

                    file.Close();
                    MessageBox.Show("File converted!");
                }
                frm10.Dispose();
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)// Open settings Form
        {
            Form7 frm7 = new Form7();

            frm7.Location = new System.Drawing.Point(main_screen.Bounds.Location.X, main_screen.Bounds.Location.Y); // location = (0,0) of presentation screen
            frm7.StartPosition = FormStartPosition.CenterScreen;                      // enable manual option for form position
            frm7.ShowDialog();
            if (frm7.DialogResult == DialogResult.OK)
            {
                checkColor();                                                       // check backgroung, counter and cross colors
                output_path_show();                                                 // display output path if feature enable
                if(frm7.changed)
                    initializeRecordOptions();                                          // initialize record options (system properties)
            }
            frm7.Dispose();
        }

        //********************************* Help Menu *********************************
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)   // Open new windows with information about the program
        {
            AboutBox1 aboutbox = new AboutBox1();   // new windows with information about the program. This informations are stored and can be changed in solution's properties
            aboutbox.ShowDialog();                  // show this window. The user can return in main form when he closes aboutBox window
        }

        //********************************* Stimuli Menu *********************************
        private void addtoolStripMenuItem_Click(object sender, EventArgs e)     // The user can add the files for stimuli experiment
        {
            add_button_Click(add_button, EventArgs.Empty);
        }

        private void deletetoolStripMenuItem_Click(object sender, EventArgs e)  // Delete the selected files from listBox1
        {
            delete_button_Click(delete_button, EventArgs.Empty);
        }

        private void cleartoolStripMenuItem_Click(object sender, EventArgs e)   // Clear all files from listBox1
        {
            clear_button_Click(clear_button, EventArgs.Empty);
        }

        private void uptoolStripMenuItem_Click(object sender, EventArgs e)      // Move the selected items up
        {
            up_button_Click(up_button, EventArgs.Empty);
        }

        private void downtoolStripMenuItem_Click(object sender, EventArgs e)    // Move the selected items down
        {
            down_button_Click(down_button, EventArgs.Empty);
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)   // Start the stimuli experiment
        {
            start_button_Click(start_button, EventArgs.Empty);
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)   // Pause the stimuli experiment
        {
            Pause_button_Click(Pause_button, EventArgs.Empty);
        }

        private void continueToolStripMenuItem_Click(object sender, EventArgs e)// Continue the stimuli experiment
        {
            Continue_button_Click(Continue_button, EventArgs.Empty);
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)    // Stop the stimuli experiment
        {
            Stop_button_Click(Stop_button, EventArgs.Empty);
        }

        //********************************* Stimuli Files Group *********************************
        private void listBox1_DragEnter(object sender, DragEventArgs e)         // Enable Drag and Drop Effect on listBox1
        {
            e.Effect = DragDropEffects.All;
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)          // Do the Drag and Drop Effect on listBox1
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (string file in files)
            {
                listBox1.Items.Add(new newitem { safefilename = System.IO.Path.GetFileName(file), filename = file });
                Media_files.Add(file);
            }
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)        // Deselect Items in Mouse Down event (when the mouse click don't focus an item)
        {
            if (e.Y > listBox1.ItemHeight * listBox1.Items.Count)
                listBox1.SelectedItems.Clear();
        }

        private void add_button_Click(object sender, EventArgs e)               // The user can add the files for stimuli experiment
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();          // Opens a file Dialog for the user to pick files
            openFileDialog1.InitialDirectory = System.Environment.CurrentDirectory; 

            // This filter allow the user to select only the suported files
            openFileDialog1.Filter = "Image file (*.BMP,*.DIB,*.RLE,*.JPG,*.JPEG,*.JPE,*.JFI,;*.GIF,*.TIF,*.TIFF,*.PNG)|*.BMP;*.DIB;*.RLE;*.JPG;*.JPEG;*.JPE;*.JFIF;*.GIF;*.TIF;*.TIFF;*.PNG|" +
                                     "Videos Files (*.DAT, *.WMV, *.3G2, *.3GP, *.3GP2, *.3GPP, *.AMV, *.ASF, *.AVI, *.BIN, *.CUE, *.DIVX, *.DV, *.FLV, *.GXF, *.ISO, *.M1V, *.M2V, *.M2T, *.M2TS, *.M4V,)|" +
                                     "*.WMV; *.MKV; *.MOV; *.MP2; *.MP2V; *.MP4; *.MP4V; *.MPA; *.MPE; *.MPEG; *.MPEG1; *.MPEG2; *.MPEG4; *.MPG; *.MPV2; *.MTS; *.NSV; *.NUV; *.OGG; *.OGM; *.OGV; *.OGX; *.PS; *.REC; *.RM; *.RMVB; *.TOD; *.TS; *.TTS; *.VOB; *.VRO; *.WEBM|" +
                                     "Audio Files (*.MP3, *.WMA, *.MP3, *.WMA)| *.MP3; *.WMA, *.MP3, *.WMA|" +
                                     "All files|*.*";
            openFileDialog1.Multiselect = true;                         // The user can select multiple files at once
            openFileDialog1.Title = "Browse multimedia files";          // The name of the Dialog window

            if (openFileDialog1.ShowDialog() == DialogResult.OK)        // Add the selected files in listBox1
            {
                string[] files = openFileDialog1.FileNames;
                foreach (string file in files)
                {
                    listBox1.Items.Add(new newitem { safefilename = System.IO.Path.GetFileName(file), filename = file });
                    Media_files.Add(file);
                }
            }
        }

        private void delete_button_Click(object sender, EventArgs e)            // Delete the selected files from listBox1
        {
            while (listBox1.SelectedItems.Count != 0)
            {
                Media_files.RemoveAt(listBox1.SelectedIndices[0]);
                listBox1.Items.Remove(listBox1.SelectedItems[0]);
            }
        }

        private void clear_button_Click(object sender, EventArgs e)             // Clear all files from listBox1
        {
            listBox1.Items.Clear();
            Media_files.Clear();
        }

        private void up_button_Click(object sender, EventArgs e)                // Move the selected items up
        {
            if (listBox1.SelectedItems.Count != 0)
            {
                listBox1.SetSelected(0, false);

                int count = listBox1.SelectedItems.Count;
                int[] matrix = new int[listBox1.SelectedItems.Count];
                for (int i = 0; i < listBox1.SelectedItems.Count; i++)
                    matrix[i] = listBox1.SelectedIndices[i];

                for (int i = 0; i < count; i++)
                {
                    int Sel_ind = matrix[i];
                    object temp = listBox1.Items[Sel_ind];
                    listBox1.Items[Sel_ind] = listBox1.Items[Sel_ind - 1];
                    listBox1.Items[Sel_ind - 1] = temp;
                    listBox1.SetSelected(Sel_ind, false);
                    listBox1.SetSelected(Sel_ind - 1, true);

                    listBox1.SetSelected(0, false);

                    string temp1;
                    temp1 = Media_files[Sel_ind];
                    Media_files[Sel_ind] = Media_files[Sel_ind - 1];
                    Media_files[Sel_ind - 1] = temp1;
                }
            }
        }

        private void down_button_Click(object sender, EventArgs e)              // Move the selected items down
        {
            if (listBox1.SelectedItems.Count != 0)
            {
                listBox1.SetSelected(listBox1.Items.Count - 1, false);

                int count = listBox1.SelectedItems.Count;
                int[] matrix = new int[listBox1.SelectedItems.Count];
                for (int i = 0; i < listBox1.SelectedItems.Count; i++)
                    matrix[i] = listBox1.SelectedIndices[i];

                for (int i = count - 1; i >= 0; i--)
                {
                    int Sel_ind = matrix[i];
                    object temp = listBox1.Items[Sel_ind];
                    listBox1.Items[Sel_ind] = listBox1.Items[Sel_ind + 1];
                    listBox1.Items[Sel_ind + 1] = temp;
                    listBox1.SetSelected(Sel_ind, false);
                    listBox1.SetSelected(Sel_ind + 1, true);

                    listBox1.SetSelected(listBox1.Items.Count - 1, false);

                    string temp1;
                    temp1 = Media_files[Sel_ind];
                    Media_files[Sel_ind] = Media_files[Sel_ind + 1];
                    Media_files[Sel_ind + 1] = temp1;
                }
            }
        }

        private void attributes_button_Click(object sender, EventArgs e)        // Set attributes to every file
        {
            if (listBox1.Items.Count == 0)
            {
                MessageBox.Show("Zero Imported Files!");
            }
            else if (listBox2.Items.Count == 0)
            {
                MessageBox.Show("No Attributes!");
            }
            else
            {
                string[] files_names = new string[listBox1.Items.Count];
                for (int i = 0; i < listBox1.Items.Count; i++)
                    files_names[i] = System.IO.Path.GetFileName(listBox1.Items[i].ToString());

                string[] attributes = new string[listBox2.Items.Count];
                for (int i = 0; i < listBox2.Items.Count; i++)
                    attributes[i] = listBox2.Items[i].ToString();

                Form5 frm5 = new Form5(files_names, attributes);
                frm5.ShowDialog();
                if (frm5.DialogResult == DialogResult.OK)
                {
                    listBox1.Items.Clear();
                    string[] atr = frm5.returned_files_names;
                    attributes_names = frm5.returned_attr_names;

                    for (int i = 0; i < Media_files.Count; i++)
                    {
                        listBox1.Items.Add(new newitem { safefilename = System.IO.Path.GetFileName(Media_files[i]) + " - " + atr[i], filename = Media_files[i] + "#@#" + atr[i] });
                    }
                }
                frm5.Dispose();
            }
        }

        private void clear_attribs_button_Click(object sender, EventArgs e)     // Clear every given attribute
        {
            listBox1.Items.Clear();
            for (int i = 0; i < Media_files.Count; i++)
            {
                listBox1.Items.Add(new newitem { safefilename = System.IO.Path.GetFileName(Media_files[i]), filename = Media_files[i] });
            }

        }

        //********************************* Stimuli Group *********************************
        private void start_button_Click(object sender, EventArgs e)             // Start the stimuli experiment
        {
            if (listBox1.Items.Count != 0)
            {
                stimuli_start_time = string.Format("{0:dd-MM-yyy_hh-mm-ss}", DateTime.Now); 
                stimuli_output_name = textBox1.Text;

                stimuliState = "start";
                button_control(stimuliState);

                string[] filespaths = new string[listBox1.Items.Count];
                for (int i = 0; i < listBox1.Items.Count; i++)
                    filespaths[i] = listBox1.Items[i].ToString();

                Random rnd = new Random();
                string[] Random_filespaths = filespaths;
                if (checkBox2.Checked == true)
                    Random_filespaths = filespaths.OrderBy(x => rnd.Next()).ToArray();
       
                // Form2 initialize-show-start
                initializeform2();
                frm2.Presentation_Stage_Changed += new EventHandler<TextEventArgs>(frm2_Presentation_Stage_Changed);
                frm2.set_settings(Random_filespaths, (int)numericUpDown1.Value, (int)numericUpDown2.Value, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value, (int)numericUpDown6.Value, (System.Windows.Media.Stretch)comboBox2.SelectedItem, checkBox1.Checked, checkBox3.Checked, stimuli_output_path, stimuli_output_name, attributes_names); 
                frm2.showCounter();
                frm2.Show();

                emotiv.SetOutputFilename(stimuli_output_path, stimuli_output_name, stimuli_start_time);
                emotiv.Start();
            }
            else
                MessageBox.Show("Zero Imported Files!");
        }

        private void Pause_button_Click(object sender, EventArgs e)             // Pause the stimuli experiment
        {
            if (frm2.Visible) 
            {
                stimuliState = "pause";
                button_control(stimuliState);

                frm2.pauseMedia();
                Application.OpenForms[frm2.Name].Focus(); //or frm2.Focus();// Give Focus on Form2, for keystoke log to work 
            }     
        }

        private void Continue_button_Click(object sender, EventArgs e)          // Continue the stimuli experiment
        {
            if (frm2.Visible) 
            {
                stimuliState = "continue";
                button_control(stimuliState);

                frm2.continueMedia();
                Application.OpenForms[frm2.Name].Focus(); //or frm2.Focus();// Give Focus on Form2, for keystoke log to work 
            }
        }

        private void Stop_button_Click(object sender, EventArgs e)              // Stop the stimuli experiment
        {
            if (frm2.Visible)
            {
                frm2.stopMedia();
                emotiv.Stop();
            }

            stimuliState = "stop";
            button_control(stimuliState);
        }

        //********************************* Record Group *********************************
        private void textBox1_Enter(object sender, EventArgs e)                 // Changes the color of record's name
        {
            textBox1.ForeColor = Color.Black;
            if (textBox1.Text == "Stimuli")
            {
                textBox1.Text = "";
                textBox1.ForeColor = Color.Gray;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)                 // Changes the color of record's name
        {
            textBox1.ForeColor = Color.Black;
            if (textBox1.Text == "" || textBox1.Text == "Stimuli")
            {
                textBox1.ForeColor = Color.Gray;
                textBox1.Text = "Stimuli";
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)           // Changes the color of record's name
        {
            if (!IsValidFilename(textBox1.Text))                                //invalid characters: \  /  :  *  ?  "  <  >  |
            {
                MessageBox.Show("Invaled Character:  " + @"\"  + " " +
                                                          "/"  + " " +
                                                          ":"  + " " +
                                                          "*"  + " " +
                                                          "?"  + " " +
                                                          "\"" + " " +
                                                          "<"  + " " +
                                                          ">"  + " " +
                                                          "|");
                textBox1.Text = textBox1.Text.Remove(textBox1.Text.Length - 1); // remove the invalid character
                textBox1.Select(textBox1.Text.Length, 0);                       // move cursor on last character after invalid character removed
                textBox1.ForeColor = Color.Gray;
            }
            if (textBox1.Text != "Stimuli")
                textBox1.ForeColor = Color.Black;
        }

        private void comboBox1Load()                                            // add and initialize bakcground color of stimuli 
        {
            comboBox1.Items.Add(System.Drawing.Color.Black);
            comboBox1.Items.Add(System.Drawing.Color.Gray);
            comboBox1.Items.Add(System.Drawing.Color.White);
            comboBox1.Items.Add(System.Drawing.Color.Red);
            comboBox1.Items.Add(System.Drawing.Color.Blue);
            comboBox1.Items.Add(System.Drawing.Color.Green);
            comboBox1.SelectedIndex = 0;                                    // Default value = Black
        }

        private void comboBox2Load()                                            // add and initialize display position
        {
            comboBox2.Items.Add(System.Windows.Media.Stretch.Uniform);      // fills up as much of the space while preserving the aspect ratio and the image content.
            comboBox2.Items.Add(System.Windows.Media.Stretch.None);         // displays the native resolution of the content in its original size.
            comboBox2.Items.Add(System.Windows.Media.Stretch.UniformToFill);// fills up the entire space while preserving the aspect ratio. This can result in some of the image being cropped.
            comboBox2.Items.Add(System.Windows.Media.Stretch.Fill);         // fills up the entire space, but does not preserve the aspect ratio.
            comboBox2.SelectedIndex = 0;                                    // Default value = Uniform
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)       // fixation cross feature enable or disable
        {
            if (checkBox3.Checked)
                numericUpDown6.Enabled = true;
            else
                numericUpDown6.Enabled = false;

            checkColor();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) // when comboBox1 selectedIndex changed, call checkColor()
        {
            checkColor();
        }

        private void select_folder_button_Click(object sender, EventArgs e)     // User selects path for csv file and emotiv output file
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();   // The user selected a folder and pressed the OK button.
            folderBrowserDialog1.SelectedPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Resources\\Output";
            folderBrowserDialog1.Description = "Select the directory that you want to use.\nDefault directory is: \\Resources\\Output (in .exe folder).";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                stimuli_output_path = path + "\\";
            }
            output_path_show();
        }

        //********************************* Screen Selection Group *********************************
        private void comboBox3Load()                                            // initialize screens
        {
            comboBox3.Items.Clear();                                            // clear list of screen
            int i;
            for (i = 0; i < screens.Length; i++)                                // add current screens on list
                comboBox3.Items.Add(screens[i].DeviceName.Substring(4, 8));
            i = 0;
            while (screens[i].DeviceName.Substring(4, 8) == main_screen.DeviceName.Substring(4, 8)) // select presentation screen (!=main screen)
                i++;
            comboBox3.SelectedIndex = i;
        }

        private void comboBox3_SelectedValueChanged(object sender, EventArgs e) // user selected same screen for presentation
        {
            presentation_screen = screens[comboBox3.SelectedIndex];
            if (presentation_screen.DeviceName.Substring(4, 8) == main_screen.DeviceName.Substring(4, 8))
                MessageBox.Show("You selected the same screen!");
        }

        //********************************* Attributes Group *********************************
        private void add_attr_button_Click(object sender, EventArgs e)          // add attribute to listBox2
        {
            Form4 frm4 = new Form4(null);
            frm4.ShowDialog();
            if (frm4.DialogResult == DialogResult.OK)
            {
                listBox2.Items.Add(frm4.returned_attribute);
            }
            frm4.Dispose();
        }

        private void delete_attr_button_Click(object sender, EventArgs e)       // delete attribute from listBox2
        {
            while (listBox2.SelectedItems.Count != 0)
            {
                listBox2.Items.Remove(listBox2.SelectedItems[0]);
            }
        }

        private void clear_attr_button_Click(object sender, EventArgs e)        // clear all attributes from listBox2
        {
            listBox2.Items.Clear();
        }

        private void up_attr_button_Click(object sender, EventArgs e)           // Move the selected attributes up
        {
            if (listBox2.SelectedItems.Count != 0)
            {
                listBox2.SetSelected(0, false);

                int count = listBox2.SelectedItems.Count;
                int[] matrix = new int[listBox2.SelectedItems.Count];
                for (int i = 0; i < listBox2.SelectedItems.Count; i++)
                    matrix[i] = listBox2.SelectedIndices[i];

                for (int i = 0; i < count; i++)
                {
                    int Sel_ind = matrix[i];
                    object temp = listBox2.Items[Sel_ind];
                    listBox2.Items[Sel_ind] = listBox2.Items[Sel_ind - 1];
                    listBox2.Items[Sel_ind - 1] = temp;
                    listBox2.SetSelected(Sel_ind, false);
                    listBox2.SetSelected(Sel_ind - 1, true);

                    listBox2.SetSelected(0, false);
                }
            }
        }

        private void down_attr_button_Click(object sender, EventArgs e)         // Move the selected attributes down
        {
            if (listBox2.SelectedItems.Count != 0)
            {
                listBox2.SetSelected(listBox2.Items.Count - 1, false);

                int count = listBox2.SelectedItems.Count;
                int[] matrix = new int[listBox2.SelectedItems.Count];
                for (int i = 0; i < listBox2.SelectedItems.Count; i++)
                    matrix[i] = listBox2.SelectedIndices[i];

                for (int i = count - 1; i >= 0; i--)
                {
                    int Sel_ind = matrix[i];
                    object temp = listBox2.Items[Sel_ind];
                    listBox2.Items[Sel_ind] = listBox2.Items[Sel_ind + 1];
                    listBox2.Items[Sel_ind + 1] = temp;
                    listBox2.SetSelected(Sel_ind, false);
                    listBox2.SetSelected(Sel_ind + 1, true);

                    listBox2.SetSelected(listBox2.Items.Count - 1, false);
                }
            }
        }

        private void listBox2_MouseDown(object sender, MouseEventArgs e)        // Mouse Down event
        {
            if (e.Y > listBox2.ItemHeight * listBox2.Items.Count)
                listBox2.SelectedItems.Clear();
        }

        private void listBox2_MouseDoubleClick(object sender, MouseEventArgs e) // Mouse Double click event
        {
            int index = this.listBox2.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                Form4 frm4 = new Form4(listBox2.Items[index].ToString());
                frm4.ShowDialog();
                if (frm4.DialogResult == DialogResult.OK)
                {
                    // remove previous item...
                    listBox2.Items.RemoveAt(index);
                    // insert modified item...
                    listBox2.Items.Insert(index, frm4.returned_attribute);
                }
                frm4.Dispose();
            }
        }

        //********************************* Buttons *********************************
        private void exit_button_Click(object sender, EventArgs e)              // Exit from Application
        {
            frm2.Dispose();             // Dispose Form2 window. Form2.Hide = enabled while the main Form is running
            Application.Exit();         // Close all forms.
        }

        private void identify_button_Click(object sender, EventArgs e)          // identify screens using numbers
        {
            Form3[] identify_form = new Form3[screens.Length];                  // new form matrix 

            for (int i = 0; i < screens.Length; i++)                            // for screens.Length show number on each screen
            {
                //Form3 frm3 = new Form3();
                identify_form[i] = new Form3();
                identify_form[i].Location = new System.Drawing.Point(screens[i].Bounds.Location.X, screens[i].Bounds.Location.Y);
                identify_form[i].StartPosition = FormStartPosition.Manual;
                identify_form[i].ClientSize = new System.Drawing.Size(screens[i].Bounds.Width, screens[i].Bounds.Height);
                identify_form[i].FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                identify_form[i].ControlBox = false;

                int j = i + 1;
                identify_form[i].load(j.ToString());
                identify_form[i].Show();
            }
        }

        private void button_control(string state)                               // enable and disable the presensation buttons
        {
            /* state = initialize || stop || ended  -> then only Start button can be clicked
             * state = start                        -> then only Pause and Stop buttons can be clicked
             * state = pause                        -> then only Continue and Stop buttons can be clicked
             * state = continue                     -> then only Pause and Stop buttons can be clicked
             */

            if (state == "initialize" || state == "stop" || state == "ended" ) 
            {
                start_button.Enabled = true;
                Pause_button.Enabled = false;
                Continue_button.Enabled = false;
                Stop_button.Enabled = false;

                startToolStripMenuItem.Enabled = true;
                pauseToolStripMenuItem.Enabled = false;
                continueToolStripMenuItem.Enabled = false;
                stopToolStripMenuItem.Enabled = false;

                identify_button.Enabled = true;
            }
            else if(state == "start")
            {
                start_button.Enabled = false;
                Pause_button.Enabled = true;
                Continue_button.Enabled = false;
                Stop_button.Enabled = true;

                startToolStripMenuItem.Enabled = false;
                pauseToolStripMenuItem.Enabled = true;
                continueToolStripMenuItem.Enabled = false;
                stopToolStripMenuItem.Enabled = true;

                identify_button.Enabled = false;
            }
            else if(state == "pause")
            {
                start_button.Enabled = false;
                Pause_button.Enabled = false;
                Continue_button.Enabled = true;
                Stop_button.Enabled = true;

                startToolStripMenuItem.Enabled = false;
                pauseToolStripMenuItem.Enabled = false;
                continueToolStripMenuItem.Enabled = true;
                stopToolStripMenuItem.Enabled = true;

                identify_button.Enabled = false;
            }
            else if (state == "continue")
            {
                start_button.Enabled = false;
                Pause_button.Enabled = true;
                Continue_button.Enabled = false;
                Stop_button.Enabled = true;

                startToolStripMenuItem.Enabled = false;
                pauseToolStripMenuItem.Enabled = true;
                continueToolStripMenuItem.Enabled = false;
                stopToolStripMenuItem.Enabled = true;

                identify_button.Enabled = false;
            }
        }

        //********************************* Other Methods *********************************
        private bool IsValidFilename(string testName)                           // check if string - windows invalid filename
        {
            string strTheseAreInvalidFileNameChars = new string(System.IO.Path.GetInvalidFileNameChars());
            Regex regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

            if (regInvalidFileName.IsMatch(testName)) { return false; };

            return true;
        }

        private void emotiv_status_update()                                     // update GUI emotiv status
        {
            pictureBox16.Image.Dispose();
            pictureBox17.Image.Dispose();
            string emotiv_images = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath + "\\Resources\\Images\\Emotiv_Contact_Status\\";
            if (emotiv_status)
            {
                //Battery
                progressBar1.Value = (int)emotiv_battery_level;
                label12.Text = emotiv_battery_level.ToString() + "%";
                
                //wireless signal
                if (emotiv_wireless_signal_status == 0)
                {
                    pictureBox16.Image = Image.FromFile(emotiv_images + "no_signal.jpg");
                }
                else if (emotiv_wireless_signal_status == 1)
                {
                    pictureBox16.Image = Image.FromFile(emotiv_images + "bad_signal.jpg");
                }
                else if (emotiv_wireless_signal_status == 2)
                {
                    pictureBox16.Image = Image.FromFile(emotiv_images + "good_signal.jpg");
                }
                
                //Connected
                pictureBox17.Image = Image.FromFile(emotiv_images + "connected.jpg");
                
                //Sensors quality
                drawpictureBoxes(emotiv_Contacts_Quality, emotiv_images);
                
            }
            else
            {
                progressBar1.Value = 0;
                label12.Text = "0%";
                pictureBox16.Image = Image.FromFile(emotiv_images + "no_signal.jpg");
                pictureBox17.Image = Image.FromFile(emotiv_images + "disconnected.jpg");
                initializepictureBoxes();
            }

        }

        private void drawpictureBoxes(string[] ecq, string path)                // draw pictureboxes
        {
            Bitmap bmp;
            Graphics graphics;
            Brush brush;

            for (int i = 0; i < 14; i++)
            {
                bmp = new Bitmap(pcbx[i].Width - 1, pcbx[i].Width - 1);
                graphics = Graphics.FromImage(bmp);
                
                pcbx[i].Visible = true;
                pcbx[i].BringToFront();
                if (ecq[i] == "1")
                {
                    brush = new SolidBrush(Color.Red);
                }
                else if (ecq[i] == "2")
                {
                    brush = new SolidBrush(Color.Orange);
                }
                else if (ecq[i] == "3")
                {
                    brush = new SolidBrush(Color.Yellow);
                }
                else if (ecq[i] == "4")
                {
                    brush = new SolidBrush(Color.Green);
                }
                else
                {
                    brush = new SolidBrush(Color.Black);
                }
                graphics.FillEllipse(brush, new Rectangle(0, 0, pcbx[i].Width - 1, pcbx[i].Width - 1));
                pcbx[i].Image = bmp;
            }
        }

        private void draw_menu()                                                // Draw menu (features from tool menu)
        {
            
            if (differentscreenstoolStripMenuItem.Checked)
            {
                groupBox4.Location = new Point(871, 27);
                groupBox6.Location = new Point(871, 99);
            }
            else
            {
                groupBox6.Location = new Point(871, 27);
            }
            

        }

        private void checkColor()                                               // check backgroung, counter and cross colors
        {
            label19.Text = "";
            label19.Visible = false;
            label19.Location = new Point(12, 366);
            if (Properties.Settings.Default.Output_Path_Visible)
                label19.Location = new Point(12, 389);

            if (Properties.Settings.Default.Warnings_Counter_Cross) 
            {
                if ((Color)comboBox1.SelectedItem == Properties.Settings.Default.Counter_Color)
                {
                    label19.Text = "Warning! Background color is the same with counter color";
                    label19.Visible = true;
                }
                if ((Color)comboBox1.SelectedItem != Properties.Settings.Default.Counter_Color && (Color)comboBox1.SelectedItem == Properties.Settings.Default.Cross_Color && checkBox3.Checked)
                {
                    label19.Text = "Warning! Background color is the same with cross color";
                    label19.Visible = true;
                }
                if ((Color)comboBox1.SelectedItem == Properties.Settings.Default.Counter_Color && (Color)comboBox1.SelectedItem == Properties.Settings.Default.Cross_Color && checkBox3.Checked)
                {
                    label19.Text = "Warning! Background color is the same with counter color and cross color";
                    label19.Visible = true;
                }
            }
        }

        private void output_path_show()                                         // display output path if feature enable
        {
            label20.Text = "";
            label20.Visible = false;
            if (Properties.Settings.Default.Output_Path_Visible)
            {
                label20.Text = stimuli_output_path.ToString();
                label20.Visible = true;
            }
        }

        private char Detect(TextReader reader, int rowCount, IList<char> separators) // detect dilimiter in file
        {
            IList<int> separatorsCount = new int[separators.Count];
            int character;
            int row = 0;
            bool quoted = false;
            bool firstChar = true;

            while (row < rowCount)
            {
                character = reader.Read();
                switch (character)
                {
                    case '"':
                        if (quoted)
                        {
                            if (reader.Peek() != '"') // Value is quoted and current character is " and next character is not ".
                                quoted = false;
                            else
                                reader.Read(); // Value is quoted and current and next characters are "" - read (skip) peeked qoute.
                        }
                        else
                        {
                            if (firstChar) // Set value as quoted only if this quote is the first char in the value.
                                quoted = true;
                        }
                        break;
                    case '\n':
                        if (!quoted)
                        {
                            ++row;
                            firstChar = true;
                            continue;
                        }
                        break;
                    case -1:
                        row = rowCount;
                        break;
                    default:
                        if (!quoted)
                        {
                            int index = separators.IndexOf((char)character);
                            if (index != -1)
                            {
                                ++separatorsCount[index];
                                firstChar = true;
                                continue;
                            }
                        }
                        break;
                }

                if (firstChar)
                    firstChar = false;
            }
            int maxCount = separatorsCount.Max();

            return maxCount == 0 ? '\0' : separators[separatorsCount.IndexOf(maxCount)];
        }

        private string delimiter_return(string delimiter_name)                  // return delimiter from input = name
        {
            string delimiter = " ";

            if (delimiter_name == "comma")
                delimiter = ",";
            else if (delimiter_name == "semicolon")
                delimiter = ";";
            else if (delimiter_name == "colon")
                delimiter = ":";
            else if (delimiter_name == "bar")
                delimiter = "|";
            else if (delimiter_name == "tab")
                delimiter = "\t";

            return delimiter;
        }

        private string initialize_delimiter(string path)                        // using detect method to detect delimiter
        {
            IList<char> separators = new List<char>();
            separators.Clear();
            separators.Add(' ');
            separators.Add(',');
            separators.Add('|');
            separators.Add(';');
            separators.Add(':');
            separators.Add('\t');

            StreamReader reader_temp = new StreamReader(path);
            string delimiter = Detect(reader_temp, 1, separators).ToString();
            return delimiter;
        }

        public string MakeUnique(string path)                                   // make unique name for txt or csv after conversion
        {
            string dir = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string fileExt = Path.GetExtension(path);

            for (int i = 1; ; ++i)
            {
                if (!File.Exists(path))
                    return path;

                path = Path.Combine(dir, fileName + "(" + i + ")" + fileExt);
            }
        }

        //********************************* Form Events *********************************
        private void Form1_LocationChanged(object sender, EventArgs e)          // Location of main screen changed
        {
            Screen current_screen = Screen.FromControl(this);                   // main form current screen
            if (current_screen != main_screen)
            {
                main_screen = current_screen;
                label8.Text = main_screen.DeviceName.Substring(4,8);

                if (differentscreenstoolStripMenuItem.Checked == true)
                    comboBox3Load();
                else
                    presentation_screen = current_screen;
            }
        }

        //********************************* Analysis Form Events *********************************
        private void analysis_button_Click(object sender, EventArgs e)          // This button opens analysis windows(Form6) and hide main windows(Form1)
        {
            Form6 frm6 = new Form6();
            frm6.Show();
            this.Hide();
            frm6.FormClosing += frm6_Closing; 
        }

        private void frm6_Closing(object sender, FormClosingEventArgs e)        // Closing event of analysis From (Form6)
        {
            this.Show();
        }

        //********************************* Emotiv Class Events *********************************
        private void emotiv_BatteryChanged(object sender, FloatEventArgs e)     // emotiv battery custom event
        {
            float a = e.Number * 100;
            if (emotiv_status)
            {
                emotiv_battery_level = Math.Abs(a);
            }
            else 
            {
                emotiv_battery_level = 0;
            }
        }

        private void emotiv_Wireless_Signal_Changed(object sender, IntEventArgs e) // emotiv wireless signal custom event
        {
            if (emotiv_status)
            {
                emotiv_wireless_signal_status = e.Number;
            }
            else
            {
                emotiv_wireless_signal_status = 0;
            }
        }

        private void emotiv_Connected_Changed(object sender, BoolEventArgs e)   // emotiv connected custom event
        {
            emotiv_status = e.State;
        }

        private void emotiv_Contacts_Quality_Changed(object sender, TextEventArgs e) // emotiv contacts quality custom event
        {
            if (emotiv_status)
            {
                emotiv_Contacts_Quality = e.Text.Split(new string[] { "," }, StringSplitOptions.None);// split columns for every row
            }
            else
            {
                for (int i=0;i<14;i++)
                {
                    emotiv_Contacts_Quality[i] = "0";
                }
            }
            
        }

        //********************************* Form2 Events *********************************
        private void frm2_Presentation_Stage_Changed(object sender, TextEventArgs e) // presenstation ended custom event
        {
            if (e.Text == "ended" || e.Text == "stop")
            {
                button_control(e.Text);
                emotiv.Stop();
                frm2.Presentation_Stage_Changed -= frm2_Presentation_Stage_Changed;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)                    // emotiv events check
        {
            emotiv.check();
            emotiv_status_update();

            // garbage collector every second
            if(Properties.Settings.Default.Collector)
                garbage_collector();
        }

        //********************************* garbage collector *********************************
        private void garbage_collector()                                        // free memory 
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }

        
    }

}
