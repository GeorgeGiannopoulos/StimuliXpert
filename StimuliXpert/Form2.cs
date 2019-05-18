using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using WMPLib;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace StimuliXpert
{
    public partial class Form2 : Form
    {

        /*
         ********************************* Form Informations *********************************************
         * [Variables]
         * elementHost1 = media host. containe media element and control
         * uc = UserControl object that handels media control (source-play-pause-stop-close)
         * label1 = start up counter text
         * 
         * duration_time = duration time for pictures
         * between_time = time between media presensation
         * counter_show = start up counter value
         * time = duration time for pictures = 0  | duration time for video or sound = natural duration of file
         * cross_time = duration of fixation cross
         * counter = represent the number_id of the current file which the form present on screen
         * stage = state of media presentation = {"counter","media","between"} : stage is private (only shown in Form2) & mediaState is static public (Form1 can see it and Form2 too)
         * 
         * keystroke_state = keystroke_state = checkBox1.Checked
         * keystroke_state = checkBox1.Checked
         * 
         * start_time = stimuli start time (string)
         * current_media_file = path of current presented media (static public)
         * name_of_current_media_file = name of current presented media (static public)
         * save_file_name = the name of the output file
         * 
         * attributes_names = attributes names (attributes_names_1,attributes_names_2,...)
         * attributes_values = attributes values (attributes_values[i] = attribute_value_1[i],attribute_value_2[i],...)
         * default_attributes_values = attributes values for stages = {"counter","between","evaluation"}
         * current_attributes_values = attributes values for current file or stage
         * attribute_log = attribute_log = enable or disable
         * 
         * m_MouseHookManager = Mouse event listener
         * mouse_x = cursor X position
         * mouse_y = cursor Y position
         * isDrawing = true = log, false = no log
         * cursor_log = cursor log - application setting
         * normalize_value =  max normalize value for converting pixel to normalize point
         * new_center,old_center,screens_center = old, new and active screen centers
         * Total_width, Total_height = total pixels of active screen
         * mostLeft, mostTop, mostRight, mostBottom = active screen edges
         * 
         * delimiter = for csv file
         * List<String> csv = list of the keystroks. first row = {key , time_of_keyDown , time_pasted_from_start , name_of_the_current_file}
         *  
         * timer1 = used for counter : Interval = 1 second (1000 millisecond) 
         * timer2 = used for duration time : Interval = duration_time for pictures and natural duration of file for video or sound
         * timer3 = between_time
         * timer4 = used for evaluation time
         * timer5 - used for cross's time
         * timer2.Interval and timer3.Interval = 1 mellisecond when user's given value is 0
         * 
         * [Mesthods]
         * showCounter() = present label1(counter) on screen
         * showMedia() = present first media file (by calling uc control)
         * pauseMedia() = pause media file (by calling uc control)
         * continueMedia() = contrinue media file (by calling uc control)
         * stopMedia() = stop media file (by calling uc control)
         *          
         * Form2_FormClosing() = stop form2 closing - hide form2 - mediaState = "ended"
         * 
         * ProcessCmdKey() = override method for handling keystrokes
         * 
         * csvwrite() = write the keystroke list in csv file
         * delimiter_return() = return delimiter from input = name
         * cursor_position_normalize() = normalize x,y position od the cursor
         * calculate_centers() = calculate old, new and active screen centers
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
         ********************************* Stimuli Form ****************************************************
         */

        public Form2() 
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            elementHost1.Dock = DockStyle.Fill;                                 // Emelent Host Fill the Presensation Window
            elementHost1.Child = uc;                                            // UserControl Class = child of emelentHost of this Form 
            elementHost1.HostContainer.MouseDown += new System.Windows.Input.MouseButtonEventHandler(HostContainer_MouseDown);
            elementHost1.HostContainer.MouseUp += new System.Windows.Input.MouseButtonEventHandler(HostContainer_MouseUp);
            this.Controls.Add(elementHost1);                                    // Add Emelent Host to Form2 Control

            uc.Media_Ended_Changed += new EventHandler<BoolEventArgs>(media_ended_check);
        }

        //********************************* Declare Variables **********************************************
        UserControl1 uc = new UserControl1();                                   // UserControl object that handels media control (source-play-pause-stop-close)

        static public string mouse_x;                                           // cursor X position
        static public string mouse_y;                                           // cursor Y position
        private bool isDrawing = false;                                         // true = log, false = no log
        bool cursor_log;                                                        // cursor log - application setting
        double normalize_value;                                                 // max normalize value for converting pixel to normalize point
        Screen[] scr = Screen.AllScreens;                                       // load screens as parameters
        Point new_center,old_center,screens_center;                             // old, new and active screen centers
        int Total_width, Total_height;                                          // total pixels of active screen
        int mostLeft, mostTop, mostRight, mostBottom;                           // active screen edges

        string[] filespaths;                                                    // filespaths = Form1.listBox1 items in string[]
        string stage, delimiter;                                                // stage = {"counter","media","between","evaluation","fixation_cross"} | delimiter = for csv file
        string save_file_name;                                                  // save_file_name = the name of the output file
        string stimuli_output_path;                                             // path of emotiv output file
        string current_media_file;                                              // current_media_file = name of current presented media
        static public string name_of_current_media_file;                        // name of presented media file (for mySuperEmotiv purpose)
        System.DateTime start_time;                                             // stimuli start time. Initialize after Start Button clicked. Start after counter down elapsed
        bool keystroke_state;                                                   // keystroke_state = checkBox1.Checked
        bool evaluation_ended;                                                  // evaluation_ended = evaluation state (true, false)
        bool fixation_cross;                                                    // fixation_cross = checkBox2.Checked
        System.Windows.Media.Stretch displaymode;                               // displaymode = display position
        int duration_time, between_time, time, cross_time;                      // see Form Informations 
        int counter, evaluation_counter,evaluation_time, counter_show;          // see Form Informations 
        List<String> csv = new List<String>();                                  // lost with keystroke information

        string attributes_names;                                                // attributes names (attributes_names_1,attributes_names_2,...)
        string[] attributes_values;                                             // attributes values (attributes_values[i] = attribute_value_1[i],attribute_value_2[i],...)
        string default_attributes_values;                                       // attributes values for stages = {"counter","between","evaluation"}
        string current_attributes_values;                                       // attributes values for current file or stage
        bool attribute_log;                                                     // attribute_log = enable or disable

        private string presentation_stage;                                      // event variable

        public event EventHandler<TextEventArgs> Presentation_Stage_Changed;    // costum event

        public string New_presentation_stage                                    // new variable type
        {
            get { return presentation_stage; }
            set
            {
                if (presentation_stage != value)
                {
                    presentation_stage = value;
                    OnNewTextChanged(new TextEventArgs(presentation_stage));
                }
            }
        }

        //********************************* Media settigns *************************************************
        public void set_settings(string[] files, int dt, int bt, int ct, int ec, int et, int fct, System.Windows.Media.Stretch dmode, bool key_state, bool cross_state, string out_path, string name, string attr)// Initialize the presentation settings
        {
            New_presentation_stage = "start";
            System.Windows.Forms.Cursor.Position = screens_center;              // Move cursor to the center of presentation screen
            System.Windows.Forms.Cursor.Hide();                                 // Hide cursor when presentation form is active
            mouse_x = "0";                                                      // cursor X position
            mouse_y = "0";                                                      // cursor Y position
            isDrawing = false;                                                  // true = log, false = no log
            timer6.Start();

            label1.Font = Properties.Settings.Default.Counter_Font;             // counter Font
            label1.ForeColor = Properties.Settings.Default.Counter_Color;       // counter color

            label2.Font = Properties.Settings.Default.Cross_Font;               // fixation cross font
            label2.ForeColor = Properties.Settings.Default.Cross_Color;         // fixation cross color
            label2.Text = "+";                                                  // fixation symbol
            label2.Visible = false;                                             // cross label
            
            filespaths = files;                                                 // filespaths = Form1.listBox1 items in string[]
            counter_show = ct;                                                  // start up counter value
            if (dt == 0)                                                        // if user change duration time to 0, then... 
                duration_time = 1;                                              // duration_time = 1 millisecond (numericUpDown.Value must be != 0)
            else
                duration_time = dt * 1000;                                      // (*1000 = seconds)
            if (bt == 0)                                                        // if user change between time to 0, then... 
                between_time = 1;                                               // between_time = 1 millisecond (numericUpDown.Value must be != 0)
            else
                between_time = bt * 1000;                                       // (*1000 = seconds)
            if (et == 0)                                                        // if user change evaluation time to 0, then... 
                evaluation_time = 1;                                            // evaluation_time = 1 millisecond (numericUpDown.Value must be != 0)
            else
                evaluation_time = et * 1000;                                    // (*1000 = seconds)
            if (fct == 0)                                                       // if user change cross time to 0, then... 
                cross_time = 1;                                                 // cross_time = 1 millisecond (numericUpDown.Value must be != 0)
            else
                cross_time = fct * 1000;                                        // (*1000 = seconds)

            displaymode = dmode;                                                // Initialize displaymode
            save_file_name = name;                                              // Initialize save_file_name
            stimuli_output_path = out_path;                                     // Initializepath of emotiv output file
            attributes_names = attr;                                            // Initialize attributes_names
            attributes_values = new string[filespaths.Length];                  // new string[attributes_names.Count(f => f == ',') + 1];// Initialize attributes_values(counts "," and length = # of "," + 1)
            attribute_log = false;                                              // Initialize attribute_log
            
            evaluation_counter = ec;                                            // Initialize evaluation_counter
            keystroke_state = key_state;                                        // Initialize keystroke_state
            fixation_cross = cross_state;                                       // Initialize fixation_cross
            delimiter = delimiter_return(Properties.Settings.Default.Delimiter);
            csv.Clear();                                                        // Initialize keystroke log

            calculate_centers();                                                // calculate old, new and active screen centers
            cursor_log = Properties.Settings.Default.Cursor_log;                // cursor_log value
            normalize_value = (double)Properties.Settings.Default.Normalize_Value; // max normalize value for converting pixel to normalize point
        }

        private void calculate_centers()                                        // calculate old, new and active screen centers
        {
            mostLeft = Screen.AllScreens.Min(s => s.Bounds.Left);
            mostTop = Screen.AllScreens.Min(s => s.Bounds.Top);
            mostRight = Screen.AllScreens.Max(s => s.Bounds.Right);
            mostBottom = Screen.AllScreens.Max(s => s.Bounds.Bottom);

            Total_width = Math.Abs(mostRight - mostLeft);
            Total_height = Math.Abs(mostBottom - mostTop);

            old_center = new Point(0, 0);
            new_center = new Point(mostLeft, mostBottom);

            screens_center = new Point(mostLeft + Total_width / 2, mostTop + Total_height / 2);
        }

        //********************************* Media Control **************************************************
        public void showCounter()                                               // present label1(counter) on screen the minimum time to start the presentation is 1 second
        {
            stage = "counter";                                                  // set the stage of presensation to counter
            name_of_current_media_file = "ST_" + stage;                         // current_media_file = "counter"
            
            label1.Visible = true;                                              // make visible the counter (list1.text)
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;    // Draw label to the center od the screen (mabey optional)

            if (counter_show == 0)                                              // if counter = 0 seconds then start after 1 second and show "timuli Start..."
                label1.Text = "Stimuli Starts...";
            else                                                                // else write "Stimuli Starts at... " and # of seconds
                label1.Text = "Stimuli Starts at... " + counter_show.ToString();

            timer2.Interval = duration_time;                                    // initialize timer2.Interval
            timer3.Interval = between_time;                                     // initialize timer3.Interval
            timer4.Interval = evaluation_time;                                  // initialize timer4.Interval
            timer5.Interval = cross_time;                                       // initialize timer5.Interval
            timer6.Interval = 7;
            timer1.Interval = 1000;                                             // initialize timer1.Interval, always = 1 second (1000 milliseconds)
            timer1.Start();                                                     // start timer1 

            if (filespaths[0].IndexOf("#@#") != -1)                             // if files_names has attributes split them
            {
                attribute_log = true;                                           // enable attribute_log in csv
                attribute_slip(filespaths);
            }
                
            if (keystroke_state)
            {
                if (attribute_log)                                              // two types of csv
                {
                    csv.Add("Key" + delimiter + attributes_names + delimiter + "Relevant_Time" + delimiter + "Filename");
                    default_attributes_values = "";
                    
                    for (int i = 0; i < attributes_names.Count(f => f == ',')+1; i++)
                    {
                        default_attributes_values = default_attributes_values + "Nan" + delimiter;
                    }
                    default_attributes_values = default_attributes_values.Remove(default_attributes_values.Length - 1); // remove the invalid character
                    current_attributes_values = default_attributes_values;
                }
                else
                    csv.Add("Key" + delimiter + "Relevant_Time" + delimiter + "Filename");
            }
                
        }

        private void attribute_slip(string[] files)                             // split files names from attributes values with "#@#" string
        {
            for (int i = 0; i < files.Length; i++)                              // for every file name get path and attribute. Split string = "#@#".
            {
                string[] tokens = files[i].Split(new string[] { "#@#" }, StringSplitOptions.None);
                filespaths[i] = tokens[0];                                      // string before "#@#".
                attributes_values[i] = tokens[1];                               // string after "#@#".
            }

        }

        public void showMedia()                                                 // present first media file (by calling uc control)
        {
            stage = "media";                                                    // set the stage of presensation to media

            counter = -1;                                                       // first file which the form present on screen
            timer3.Interval = 1;
            timer3.Start();
        }

        public void pauseMedia()                                                // pause media file (by calling uc control)
        {
            name_of_current_media_file = "ST_pause";                            // current_media_file = "pause"
            current_attributes_values = default_attributes_values;              // current_attributes_values = "NaN,NaN,..."

            if (stage=="counter")                                               // pause the counter
                timer1.Enabled = false;
            else if (stage == "media")                                          // pause the media
            {
                timer2.Enabled = false;
                uc.mediaPause();
            }
            else if (stage == "between")                                        // pause the between stage
                timer3.Enabled = false;
            else if (stage == "evaluation")                                     // pause the evaluation stage
                timer4.Enabled = false;
            else if (stage == "fixation_cross")                                 // pause the fixation_cross stage
                timer5.Enabled = false;
        }

        public void continueMedia()                                             // contrinue media file (by calling uc control)
        {
            if (stage == "counter")                                             // contrinue the counter
                timer1.Enabled = true;
            else if (stage == "media")                                          // contrinue the media
            {
                timer2.Enabled = true;
                uc.mediaPlay();
            }
            else if (stage == "between")                                        // contrinue the between stage
                timer3.Enabled = true;
            else if (stage == "evaluation")                                     // contrinue the evaluation stage
                timer4.Enabled = true;
            else if (stage == "fixation_cross")                                 // contrinue the fixation_cross stage
                timer5.Enabled = true;
        }

        public void stopMedia()                                                 // stop media file (by calling uc control)
        {
            uc.mediaClose();                                                    // Clear the source of the path
            timer1.Stop();//Enabled = false;                                    // stop the timer1
            timer2.Stop();//Enabled = false;                                    // stop the timer2
            timer3.Stop();//Enabled = false;                                    // stop the timer3
            timer4.Stop();//Enabled = false;                                    // stop the timer4
            timer5.Stop();//Enabled = false;                                    // stop the timer5

            timer2.Interval = duration_time;                                    // Initialize timer2.Interval
            timer3.Interval = between_time;                                     // Initialize timer3.Interval
            timer4.Interval = evaluation_time;                                  // Initialize timer4.Interval
            timer5.Interval = cross_time;                                       // Initialize timer5.Interval
            counter = 0;                                                        // Initialize file counter
            this.Close();                                                       // call Form2_closing event
        }

        //********************************* Form2 Events ***************************************************
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)   // stop form2 closing - hide form2 - mediaState = "ended"
        {
            System.Windows.Forms.Cursor.Show();                                 // Show cursor when presentation form is active
            uc.mediaClose();                                                    // Clear the source of the path
            name_of_current_media_file = "ST_ended";                            // current_media_file = "ended"
            this.Hide();                                                        // hide form2
            e.Cancel = true;                                                    // this cancels the close event    
            Form1.stimuliState = "ended";                                      // say to Form1 the the media presentation ended
            New_presentation_stage = "ended";
            current_attributes_values = default_attributes_values;              // current_attributes_values = "NaN,NaN,..."
            timer6.Stop();

            if (keystroke_state)                                                // open file only when keystroke_state = enable
                csvwrite();

        }

        //********************************* timers *********************************************************
        private void timer1_Tick(object sender, EventArgs e)                    // used for counter
        {
            timer1.Stop();
            if (counter_show == 0)
            {
                label1.Visible = false;
                showMedia();
            }
            else if (counter_show == 1)
            {
                counter_show--;
                label1.Text = "Stimuli Starts...";
                timer1.Start();
            }
            else
            {
                counter_show--;
                label1.Text = "Stimuli Starts at... " + counter_show.ToString();
                timer1.Start();
            } 
        }

        private void timer2_Tick(object sender, EventArgs e)                    // used for duration time
        {
            timer2.Stop();
            uc.mediaClose();
           
            if (keystroke_state)
            {
                if (csv[csv.Count - 1].Substring(csv[csv.Count - 1].LastIndexOf(delimiter) + 1) != System.IO.Path.GetFileName(Regex.Replace(filespaths[counter], @"\s+", "_")))
                {
                    if (attribute_log)
                    {
                        csv.Add("No_Key" + delimiter + current_attributes_values + delimiter + "0000" + delimiter + name_of_current_media_file);
                    }
                    else
                        csv.Add("No_Key" + delimiter + "0000" + delimiter + name_of_current_media_file);
                }
            }

            name_of_current_media_file = "ST_between";                 // current_media_file = "ST"

            if(evaluation_counter==0)
            {
                if (counter == filespaths.Length - 1)
                    this.Close();
                else
                {
                    stage = "between";
                    name_of_current_media_file = "ST_" + stage;                 // current_media_file = "between"
                    current_attributes_values = default_attributes_values;      // current_attributes_values = "NaN,NaN,..."
                    timer3.Interval = between_time;                             // initialize timer3.Interval
                    timer3.Start();
                }
            }
            else
            {
                if (((counter + 1) % evaluation_counter) != 0 && counter == filespaths.Length - 1)
                {
                    //this.Close();
                    stage = "evaluation";

                    string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                    path = new Uri(path).LocalPath;

                    string evaluation_file = path + "\\Resources\\Images\\Evaluation_Images\\Valence.png";
                    current_media_file = evaluation_file;
                    string temp = Regex.Replace(current_media_file, @"\s+", "_");
                    //name_of_current_media_file = System.IO.Path.GetFileName(temp);
                    current_attributes_values = default_attributes_values;

                    uc.setMediaSource(current_media_file, System.Windows.Media.Stretch.Uniform);
                    name_of_current_media_file = System.IO.Path.GetFileName(temp);
                    uc.mediaPlay();
                    start_time = DateTime.Now;
                    evaluation_ended = false;
                    timer4.Start();
                }
                if (((counter + 1) % evaluation_counter) != 0 && counter < filespaths.Length - 1)
                {
                    stage = "between";
                    timer3.Interval = between_time;
                    timer3.Start();
                }
                else if (((counter + 1) % evaluation_counter) == 0)// && counter == filespaths.Length)
                {
                    stage = "evaluation";

                    string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                    path = new Uri(path).LocalPath;

                    string evaluation_file = path + "\\Resources\\Images\\Evaluation_Images\\Valence.png";
                    current_media_file = evaluation_file;
                    string temp = Regex.Replace(current_media_file, @"\s+", "_");
                    //name_of_current_media_file = System.IO.Path.GetFileName(temp);
                    current_attributes_values = default_attributes_values;

                    uc.setMediaSource(current_media_file, System.Windows.Media.Stretch.Uniform);
                    name_of_current_media_file = System.IO.Path.GetFileName(temp);
                    uc.mediaPlay();
                    start_time = DateTime.Now;
                    evaluation_ended = false;
                    timer4.Start();
                }
            }   
        }

        private void timer2_clone()                                             // used for video
        {
            uc.mediaClose();

            if (keystroke_state)
            {
                if (csv[csv.Count - 1].Substring(csv[csv.Count - 1].LastIndexOf(delimiter) + 1) != System.IO.Path.GetFileName(Regex.Replace(filespaths[counter], @"\s+", "_")))
                {
                    if (attribute_log)
                    {
                        csv.Add("No_Key" + delimiter + current_attributes_values + delimiter + "0000" + delimiter + name_of_current_media_file);
                    }
                    else
                        csv.Add("No_Key" + delimiter + "0000" + delimiter + name_of_current_media_file);
                }
            }

            name_of_current_media_file = "ST_between";                 // current_media_file = "ST"

            if (evaluation_counter == 0)
            {
                if (counter == filespaths.Length - 1)
                    this.Close();
                else
                {
                    stage = "between";
                    name_of_current_media_file = "ST_" + stage;                 // current_media_file = "between"
                    current_attributes_values = default_attributes_values;      // current_attributes_values = "NaN,NaN,..."
                    timer3.Interval = between_time;                             // initialize timer3.Interval
                    timer3.Start();
                }
            }
            else
            {
                if (((counter + 1) % evaluation_counter) != 0 && counter == filespaths.Length - 1)
                {
                    //this.Close();
                    stage = "evaluation";

                    string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                    path = new Uri(path).LocalPath;

                    string evaluation_file = path + "\\Resources\\Images\\Evaluation_Images\\Valence.png";
                    current_media_file = evaluation_file;
                    string temp = Regex.Replace(current_media_file, @"\s+", "_");
                    //name_of_current_media_file = System.IO.Path.GetFileName(temp);
                    current_attributes_values = default_attributes_values;

                    uc.setMediaSource(current_media_file, System.Windows.Media.Stretch.Uniform);
                    name_of_current_media_file = System.IO.Path.GetFileName(temp);
                    uc.mediaPlay();
                    start_time = DateTime.Now;
                    evaluation_ended = false;
                    timer4.Start();
                }
                if (((counter + 1) % evaluation_counter) != 0 && counter < filespaths.Length - 1)
                {
                    stage = "between";
                    timer3.Interval = between_time;
                    timer3.Start();
                }
                else if (((counter + 1) % evaluation_counter) == 0)// && counter == filespaths.Length)
                {
                    stage = "evaluation";

                    string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                    path = new Uri(path).LocalPath;

                    string evaluation_file = path + "\\Resources\\Images\\Evaluation_Images\\Valence.png";
                    current_media_file = evaluation_file;
                    string temp = Regex.Replace(current_media_file, @"\s+", "_");
                    //name_of_current_media_file = System.IO.Path.GetFileName(temp);
                    current_attributes_values = default_attributes_values;

                    uc.setMediaSource(current_media_file, System.Windows.Media.Stretch.Uniform);
                    name_of_current_media_file = System.IO.Path.GetFileName(temp);
                    uc.mediaPlay();
                    start_time = DateTime.Now;
                    evaluation_ended = false;
                    timer4.Start();
                }
            }
        }

        private void media_ended_check(object sender, BoolEventArgs e)          // event that tick when video endes
        {
            if(e.State)
                timer2_clone();
        }

        private void timer3_Tick(object sender, EventArgs e)                    // used for between time
        {
            timer3.Stop();
            if (counter == filespaths.Length-1)
                this.Close();
            else if (!fixation_cross)
            {
                if (cursor_log)
                    System.Windows.Forms.Cursor.Position = screens_center;              // Move cursor to the center of presentation screen
                stage = "media";
                counter++;
                current_media_file = filespaths[counter];
                string temp = Regex.Replace(current_media_file, @"\s+", "_");
                //name_of_current_media_file = System.IO.Path.GetFileName(temp);
                if(attribute_log)
                    current_attributes_values = attributes_values[counter];

                var player = new WindowsMediaPlayer();
                var clip = player.newMedia(current_media_file);
                time = (int)TimeSpan.FromSeconds(clip.duration).TotalMilliseconds;

                uc.setMediaSource(current_media_file, displaymode);
                name_of_current_media_file = System.IO.Path.GetFileName(temp);
                uc.mediaPlay();                                                     
                
                start_time = DateTime.Now;

                if (time == 0)
                {
                    timer2.Interval = duration_time;
                    timer2.Start();
                }
                //else
                    //timer2.Interval = time;

            }
            else if (fixation_cross) 
            {
                stage = "fixation_cross";

                label2.Visible = true;                                              // make visible the counter (list1.text)
                label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;    // Draw label to the center od the screen (mabey optional)

                name_of_current_media_file = "ST_" + stage;                 // current_media_file = "between"
                current_attributes_values = default_attributes_values;      // current_attributes_values = "NaN,NaN,..."

                start_time = DateTime.Now;
                timer5.Start();
            }
        }

        private void timer4_Tick(object sender, EventArgs e)                    // used for evaluation time
        {
            timer4.Stop();
            label2.Visible = false; 
            uc.mediaClose();
            name_of_current_media_file = "ST_between";                     // current_media_file = "ST"

            if ((counter == filespaths.Length - 1) && evaluation_ended == true)
                this.Close();
            else if (evaluation_ended == false) 
            {
                stage = "evaluation";
                string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                path = new Uri(path).LocalPath;

                string evaluation_file = path + "\\Resources\\Images\\Evaluation_Images\\Arousal.png";
                current_media_file = evaluation_file;
                string temp = Regex.Replace(current_media_file, @"\s+", "_");
                //name_of_current_media_file = System.IO.Path.GetFileName(temp);
                current_attributes_values = default_attributes_values;

                uc.setMediaSource(current_media_file, System.Windows.Media.Stretch.Uniform);
                name_of_current_media_file = System.IO.Path.GetFileName(temp);
                uc.mediaPlay();
                start_time = DateTime.Now;
                evaluation_ended = true;
                timer4.Start();
            }
            else if ((counter < filespaths.Length - 1) && evaluation_ended == true)
            {
                stage = "between";
                name_of_current_media_file = "ST_" + stage;                     // current_media_file = "between"
                current_attributes_values = default_attributes_values;
                timer3.Interval = between_time;
                timer3.Start();
            }
        }

        private void timer5_Tick(object sender, EventArgs e)                    // used for cross's time
        {
            timer5.Stop();
            
            label2.Visible = false; 

            stage = "media";
            counter++;
            current_media_file = filespaths[counter];
            string temp = Regex.Replace(current_media_file, @"\s+", "_");
            //name_of_current_media_file = System.IO.Path.GetFileName(temp);
            if (attribute_log)
                current_attributes_values = attributes_values[counter];

            var player = new WindowsMediaPlayer();
            var clip = player.newMedia(current_media_file);
            time = (int)TimeSpan.FromSeconds(clip.duration).TotalMilliseconds;

            uc.setMediaSource(current_media_file, displaymode);
            name_of_current_media_file = System.IO.Path.GetFileName(temp);
            uc.mediaPlay();
            start_time = DateTime.Now;

            if (time == 0)
            {
                timer2.Interval = duration_time;
                timer2.Start();
            }

            /*
            if (time == 0)
                timer2.Interval = duration_time;
            else
                timer2.Interval = time;

            timer2.Start();
            */
        }

        private void timer6_Tick(object sender, EventArgs e)                    // used for cersor log
        {
            timer6.Stop();
            if (isDrawing)
            {
                string[] XY = cursor_position_normalize(new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
                mouse_x = XY[0];
                mouse_y = XY[1];
            }

            timer6.Start();
        }

        //********************************* KeyStroke Hanlde ***********************************************
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)    // override method for handling keystrokes
        {
            System.DateTime timenow = DateTime.Now;
            string key = keyData.ToString();
            if (keyData == Keys.Escape)
            {
                stopMedia();
                return true;
            }
            if(keyData == Keys.Space)
            {
                if(Form1.stimuliState == "start" || Form1.stimuliState == "continue")
                {
                    Form1.stimuliState = "pause";
                    New_presentation_stage = "pause";
                    pauseMedia();
                }
                else if (Form1.stimuliState == "pause")
                {
                    Form1.stimuliState = "continue";
                    New_presentation_stage = "continue";
                    continueMedia();
                }
                return true;
            }
            /*
            if (keyData == Keys.Right && (Form1.stimuliState == "start" || Form1.stimuliState == "continue"))
            {
                if(timer2.Enabled || timer3.Enabled)
                {
                    timer2.Stop();
                    timer3.Stop();
                    timer3.Interval = 1;
                    timer3.Start();       
                }
                return true;
                
            }
            */
             
            if (keyData == Keys.NumPad0 || keyData == Keys.D0)
                key = "0";
            else if (keyData == Keys.NumPad1 || keyData == Keys.D1)
                key = "1";
            else if (keyData == Keys.NumPad2 || keyData == Keys.D2)
                key = "2";
            else if (keyData == Keys.NumPad3 || keyData == Keys.D3)
                key = "3";
            else if (keyData == Keys.NumPad4 || keyData == Keys.D4)
                key = "4";
            else if (keyData == Keys.NumPad5 || keyData == Keys.D5)
                key = "5";
            else if (keyData == Keys.NumPad6 || keyData == Keys.D6)
                key = "6";
            else if (keyData == Keys.NumPad7 || keyData == Keys.D7)
                key = "7";
            else if (keyData == Keys.NumPad8 || keyData == Keys.D8)
                key = "8";
            else if (keyData == Keys.NumPad9 || keyData == Keys.D9)
                key = "9";
                
            if (keystroke_state && Form1.stimuliState != "ended" && stage != "counter") 
            {
                //System.DateTime timenow = DateTime.Now;
                System.TimeSpan diff = timenow.Subtract(this.start_time);
                //string time_now = timenow.ToString("hh:mm:ss");
                string difference = diff.TotalMilliseconds.ToString().Split(',')[0];
                string file = "start";
                if (stage == "media")
                    file = System.IO.Path.GetFileName(current_media_file).ToString();
                else if (stage == "between")
                    file = "between";
                else if (stage == "evaluation")
                    file = System.IO.Path.GetFileName(current_media_file).ToString();
                else if (stage == "fixation_cross")
                    file = "fixation_cross";
                file = Regex.Replace(file, @"\s+", "_");

                string line;
                if (attribute_log)
                {
                    line = key + delimiter + current_attributes_values + delimiter + difference + delimiter + file;
                }
                else
                    line = key + delimiter + difference + delimiter + file;

                csv.Add(line);
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void csvwrite()                                                 // write the keystroke list in csv file
        {
            string date = Form1.stimuli_start_time;
            string filePath = stimuli_output_path + save_file_name + "_" + date + "_" + "KeyLog" + ".csv";

            int length = csv.Count;
            if (length > 1)
            {
                using (System.IO.TextWriter writer = File.CreateText(filePath))
                {
                    for (int index = 0; index < length; index++)
                        writer.WriteLine(string.Join(delimiter, csv[index]));
                }
            }
            else
            {
                using (System.IO.TextWriter writer = File.CreateText(filePath))
                {
                    csv.Add("No key" + delimiter + "No key" + delimiter + "No key");
                    writer.WriteLine(string.Join(delimiter, csv[0]));   // Title
                    writer.WriteLine(string.Join(delimiter, csv[1]));   // No Key,No Key,No Key
                }
            }
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

        //********************************* Mouse position **********************************************
        private string[] cursor_position_normalize(Point pn)                    // normalize x,y position of the cursor
        {
            if (pn.X > 0 && pn.X < Screen.PrimaryScreen.Bounds.Right)           // primary screen has one pixel less in calculations
                pn.X++;
            if (pn.Y > 0 && pn.Y < Screen.PrimaryScreen.Bounds.Bottom)          // primary screen has one pixel less in calculations
                pn.Y++;

            string[] new_pn = new string[2];

            double x_new = pn.X - new_center.X;
            double y_new = new_center.Y - pn.Y;

            double a = normalize_value / (float)Total_width;
            double b = normalize_value / (float)Total_height;

            new_pn[0] = (a * x_new).ToString(System.Globalization.CultureInfo.InvariantCulture);
            new_pn[1] = (b * y_new).ToString(System.Globalization.CultureInfo.InvariantCulture);

            return new_pn;
        }

        //********************************* Mouse Events **********************************************
        private void HostContainer_MouseDown(object sender, MouseButtonEventArgs e)         // Mouse Down event
        {
            if (cursor_log)
            {
                isDrawing = true;
            }
        }

        private void HostContainer_MouseUp(object sender, MouseButtonEventArgs e)           // Mouse Up event
        {
            isDrawing = false;
            mouse_x = "0";
            mouse_y = "0";
        }

        private void Form2_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)  // Mouse Down event
        {
            if (cursor_log)
            {
                isDrawing = true;
            }
        }

        private void Form2_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)    // Mouse Down event
        {
            isDrawing = false;
            mouse_x = "0";
            mouse_y = "0";
        }

        private void label1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) // Mouse Down event
        {
            if (cursor_log)
            {
                isDrawing = true;
            }
        }

        private void label1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)   // Mouse Down event
        {
            isDrawing = false;
            mouse_x = "0";
            mouse_y = "0";
        }

        private void label2_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) // Mouse Down event
        {
            if (cursor_log)
            {
                isDrawing = true;
            }
        }

        private void label2_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)   // Mouse Down event
        {
            isDrawing = false;
            mouse_x = "0";
            mouse_y = "0";
        }

        //********************************* Customs Events **********************************************
        protected virtual void OnNewTextChanged(TextEventArgs e)                // costum event
        {
            EventHandler<TextEventArgs> eh = Presentation_Stage_Changed;

            if (eh != null)
                eh(this, e);
        }

    }

    
}
