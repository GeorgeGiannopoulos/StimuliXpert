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
using System.Globalization;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.WindowsForms;
using MathNet.Numerics.Statistics;

namespace StimuliXpert
{
    public partial class Form6 : Form
    {
        /*
         ********************************* Form Informations *********************************
         * [Variables]
         *  path = emotiv output file's path
         *  point_counter = counts lines with valid information
         *  initialize = stage event. Indicates if Graph_And_Initialize() ended or not
         *  OxyPlot.WindowsForms.PlotView plot1 = oxyplot plot
         *  fs = emotiv sampling frequency (= 128)
         *  channels = matrix with emotiv values from loaded file
         *  channel_name = channel name for the plot
         *  Band_name = Band name for the plot
         *  cursor_X_position = matrix with cursor's x position
         *  cursor_Y_position = matrix with cursor's y position
         *  b = filter b parametrs
         *  a = filter a parametrs
         *  
         * comboBox1 = user selection {time,delta,theta,alpha,beta,gamma} - tabcontrol1 page1
         * comboBox2 = user selection {CH1,CH2,CH3,...} - tabcontrol1 page2
         * 
         * checkBox0 = draw all channel - tabcontrol1 page1
         * checkBox 1-14 = draw selected channels - tabcontrol1 page1
         * 
         * [Mesthods]
         * initializeForm6() = Initialize the form. Load the file
         * initializeRadio() = Initialize the Radio choice. Load the file
         * Initialize_comboBox3() = Initialize comboBox3 with files names
         * DrawPlot() = Draw the plot in panel2 of stlipContainer1 and panel1 of splitContainer2
         * Graph_And_Initialize() = Graph the time series of loaded file and initialize channels matrix
         * Graph_Time_All_Channels() = Graph the time series all or selected channels
         * Graph_Band_All_Channels() = Graph the band series all or selected channels
         * Graph_per_Channel() = Graph the time and bands series for selected channel
         * Graph_correlation_coefficient = Graph the coefficients with barseries
         * Enable_Cor_Coeff() = Enabled when tabPage3 selected
         * Disable_Cor_Coeff() = Enabled when tabPage1-2 selected
         * CreatePicture() = Create PictureBoxes
         * calculate_location() = calculate PictureBoxes location
         * filter() = filter time series to extract bands
         * filter_double() = filter time series to extract bands
         * correlation_coefficients() = calculate the correlation coefficients betwenn channels and mouse position
         * copy_to_clipboard() = copy plot to clipboard
         * closer_to() = return double closer to input
         * linear_relationship() = linear relationship status
         * Detect() = detect dilimiter in file
         * initialize_delimiter() = using detect method to detect delimiter
         * GetColour() =  colormap - double to rgb color
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

        string path;                            // emotiv output file's path
        string delimiter;
        int point_counter;                      // counts lines with valid information
        int column_emotiv_data = 4;
        bool initialize;                        // stage event. Indicates if Graph_And_Initialize() ended or not
        OxyPlot.WindowsForms.PlotView plot1;    // oxyplot plot
        double fs = 128;                        // emotiv sampling frequency
        double[][] channels = new double[14][]; // matrix with emotiv values from loaded file
        double[] cursor_X_position;             // matrix with cursor's x position
        double[] cursor_Y_position;             // matrix with cursor's y position
        double[,] corr_coeff = new double[28, 5];// correlation coefficients
        string[] channel_name = new string[] { "CH01", "CH02", "CH03", "CH04", "CH05", "CH06", "CH07", "CH08", "CH09", "CH10", "CH11", "CH12", "CH13", "CH14" }; // channel name for the plot
        string[] channel_real_name = new string[] { "AF3 ", "F7  ", "F3  ", "FC5 ", "T7  ", "P7  ", "O1  ", "O2  ", "P8  ", "T8  ", "FC6 ", "F4  ", "F8  ", "AF4 " }; // channel name for the plot
        string[] Band_name = new string[] { "Delta", "Theta", "Alpha", "Beta ", "Gamma" };   // Band name for the plot
        List<String> filesnames = new List<String>();
        Label[] lb = new Label[140];
        PictureBox[] pcbx = new PictureBox[140];
        int[] x, y, px, py;
        int xdis, ydis;
        System.Drawing.Size lbsize, pcbxsize;
        
        double[][] b = { new double[] {2.44199080316396e-07,0,-1.22099540158198e-06,0,2.44199080316396e-06,0,-2.44199080316396e-06,0,1.22099540158198e-06,0,-2.44199080316396e-07},     // initializers for delta band parameters
                            new double[] {1.72043196417412e-06,0,-8.60215982087062e-06,0,1.72043196417412e-05,0,-1.72043196417412e-05,0,8.60215982087062e-06,0,-1.72043196417412e-06},  // initializers for theta band parameters 
                            new double[] {6.74126400212344e-06,0,-3.37063200106172e-05,0,6.74126400212344e-05,0,-6.74126400212344e-05,0,3.37063200106172e-05,0,-6.74126400212344e-06},  // initializers for alpha band parameters 
                            new double[] {0.00421373028451020,0,-0.0210686514225510,0,0.0421373028451020,0,-0.0421373028451020,0,0.0210686514225510,0,-0.00421373028451020},            // initializers for beta band parameters 
                            new double[] {0.00421373028451073,0,-0.0210686514225536,0,0.0421373028451073,0,-0.0421373028451073,0,0.0210686514225536,0,-0.00421373028451073},            // initializers for gamma band parameters 
            };

        double[][] a = {new double[] {1,-9.64733442586432,41.9201999305100,-108.041857056863,182.905556916308,-212.521209040840,171.637845207138,-95.1405353426132,34.6407109512082,-7.48108320921125,0.727706070244410},       // initializers for delta band parameters 
                            new double[] {1,-9.20353049380731,38.4249249122489,-95.8189501441956,158.029157445770,-180.102032086815,143.642361508956,-79.1672333273362,28.8577963462825,-6.28308375447290,0.620590673810655},   // initializers for theta band parameters
                            new double[] {1,-8.29906128186048,31.9426856013720,-74.9080114968635,118.365019810865,-131.587164864703,104.206457875423,-58.0596160973316,21.7972548049492,-4.98617318236932,0.529053288125090},   // initializers for alpha band parameters 
                            new double[] {1,-3.94571422905478,8.65952623054397,-12.9158793738254,14.4604730704442,-12.4231158351232,8.31287342188797,-4.24828658614559,1.61788330208449,-0.414586104221134,0.0606076745185185}, // initializers for beta band parameters 
                            new double[] {1,2.88100664398518,5.69906201023627,7.66623078348309,8.36258568733237,6.99510962151936,4.83356378150274,2.54513088148241,1.08007249718361,0.302714604106325,0.0606076745185181},      // initializers for gamma band parameters  
            };

        public Form6()
        {
            InitializeComponent();
        }

        private void Form6_Load(object sender, EventArgs e)
        {
            DrawPlot();         // Draw the plot
            initializeForm6();
            CreatePicture();
        }

        //********************************* Declare Variables *********************************
        private void initializeForm6()                                          // Initialize the form. Load the file
        {
            initialize = false;                     // Initialize  stage = false
            path = "";
            tabControl1.SelectedIndex = 0;
            radioButton1.Checked = true;
            radioButton2.Checked = false;
            comboBox3.Enabled = false;
            comboBox3.SelectedIndex = -1;

            comboBox1.SelectedIndex = -1;           // Initialize comboBox1 selection
            comboBox2.SelectedIndex = -1;           // Initialize comboBox2 selection
            
            OpenFileDialog openFileDialog1 = new OpenFileDialog();  // open dialog Box for user to select the file to load
            openFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Resources\\Output";

            openFileDialog1.Filter = "Text files (*.txt)|*.txt|" +
                                     "CSV files (*.csv)|*.csv";
            openFileDialog1.Title = "Browse EEG file to load";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                checkBox0.Checked = true;           // check all channels to draw
                path = openFileDialog1.FileName;
                delimiter = initialize_delimiter(path);
                Initialize_comboBox3();
                comboBox1.SelectedIndex = 0;        // time selection
                Graph_And_Initialize();             // load and draw emotiv values from the file
                initialize = true;                  // initialize stage completed (true)
                tabControl1.Enabled = true;
            }
            else 
            {
                MessageBox.Show("Click File->New and choose a new file!");
                tabControl1.Enabled = false;
            }
        }

        private void initializeRadio()                                          // Initialize the Radio choice. Load the file
        {
            initialize = false;                     // Initialize  stage = false
            tabControl1.SelectedIndex = 0;
            comboBox1.SelectedIndex = -1;           // Initialize comboBox1 selection
            comboBox2.SelectedIndex = -1;           // Initialize comboBox2 selection
            checkBox1.Checked = false;              // uncheck channels 1 - 14 (initial position)
            checkBox2.Checked = false;              // uncheck channels 1 - 14 (initial position)
            checkBox3.Checked = false;              // uncheck channels 1 - 14 (initial position)
            checkBox4.Checked = false;              // uncheck channels 1 - 14 (initial position)
            checkBox5.Checked = false;              // uncheck channels 1 - 14 (initial position)
            checkBox6.Checked = false;              // uncheck channels 1 - 14 (initial position)
            checkBox7.Checked = false;              // uncheck channels 1 - 14 (initial position)
            checkBox8.Checked = false;              // uncheck channels 1 - 14 (initial position)
            checkBox9.Checked = false;              // uncheck channels 1 - 14 (initial position)
            checkBox10.Checked = false;             // uncheck channels 1 - 14 (initial position)
            checkBox11.Checked = false;             // uncheck channels 1 - 14 (initial position)
            checkBox12.Checked = false;             // uncheck channels 1 - 14 (initial position)
            checkBox13.Checked = false;             // uncheck channels 1 - 14 (initial position)
            checkBox14.Checked = false;             // uncheck channels 1 - 14 (initial position)
            checkBox0.Checked = true;               // check all channels to draw
            comboBox1.SelectedIndex = 0;            // time selection
            Graph_And_Initialize();                 // load and draw emotiv values from the file
            initialize = true;                      // initialize stage completed (true)
        }

        private void Initialize_comboBox3()                                     // Initialize comboBox3 with files names
        {
            filesnames.Clear();
            filesnames.Add("media_file_name");
            comboBox3.Items.Clear();

            using (StreamReader reader = new StreamReader(path))
            {
                string headerLine = reader.ReadLine();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] tokens = line.Split(new string[] { delimiter }, StringSplitOptions.None);// split columns for every row
                    string file_name = tokens[0];                                               // first column of file

                    if (file_name != filesnames[filesnames.Count - 1] && file_name != "ST_counter" && file_name != "ST_between" && file_name != "ST_media" && file_name != "ST_pause" && file_name != "ST_evaluation" && file_name != "ST_fixation_cross" && file_name != "ST_ended" && file_name != "Arousal.png" && file_name != "Valence.png" && file_name != "fixation_cross.png")
                    {
                        filesnames.Add(file_name);
                        comboBox3.Items.Add(file_name);
                    }
                }
            }
        }

        private void DrawPlot()                                                 // Draw the plot in panel2 of stlipContainer1 and panel1 of splitContainer2
        {
            plot1 = new OxyPlot.WindowsForms.PlotView();            // create new plot
            plot1.Dock = DockStyle.Fill;                            // fill panel1 form splitContainer2
            plot1.Location = new System.Drawing.Point(0, 0);        
            plot1.Name = "plot1";
            plot1.PanCursor = System.Windows.Forms.Cursors.Hand;    // cursor style when right mouse button click
            plot1.Size = new System.Drawing.Size(500, 350);

            plot1.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;   // zoom option when mouse middle button on horizontal axis
            plot1.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;  // zoom option when mouse middle button on plot
            plot1.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;     // zoom option when mouse middle button on vertical axis

            this.Controls.Add(plot1);                               // add plot to Form6 control
            splitContainer2.Panel1.Controls.Add(plot1);             // add plot1 to panel1 from splitcontainer2 
        }

        //********************************* File Menu *********************************
        private void newToolStripMenuItem_Click(object sender, EventArgs e)     // Initialize analysis form. Load new file.
        {
            initializeForm6();              // Initialize analysis
        }

        private void ExportplottopngtoolStripMenuItem1_Click(object sender, EventArgs e)// Export Current Plot Model to ClipBoard
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.FileName = "capture.png";
            saveFileDialog1.Filter = "PNG files (*.png)|*.png";
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string save_path = saveFileDialog1.FileName;
                using (var stream = File.Create(save_path))
                {
                    PngExporter pngExporter1 = new PngExporter() { Width = 1920, Height = 1080 };
                    pngExporter1.Export(plot1.Model, stream);
                }
            }
        }

        private void ExportplottopngcustomresolutiontoolStripMenuItem1_Click(object sender, EventArgs e)// Export Current Plot Model to ClipBoard
        {
            Form9 frm9 = new Form9();
            frm9.ShowDialog();
            if (frm9.DialogResult == DialogResult.OK)
            {
                int w = frm9.C_Width;            //values preserved after close
                int h = frm9.C_Height;

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.FileName = "capture.png";
                saveFileDialog1.Filter = "PNG files (*.png)|*.png";
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string save_path = saveFileDialog1.FileName;
                    using (var stream = File.Create(save_path))
                    {
                        PngExporter pngExporter1 = new PngExporter() { Width = w, Height = h };
                        pngExporter1.Export(plot1.Model, stream);
                    }
                }
            }
            frm9.Dispose();
        }

        private void SaveCorrelationCoefficientstocsvtoolStripMenuItem1_Click(object sender, EventArgs e)// Save correlation coefficients to csv
        {
            tabControl1.SelectedIndex = 2;
            string[] safefile = new string[30];
            string delimeter = ";";
            safefile[0] = "Channel" + delimeter + "Delta" + delimeter + "Theta" + delimeter + "Alpha" + delimeter + "Beta" + delimeter + "Gamma";
            for (int i = 1; i < 29; i++)
            {
                if (i < 15)
                    safefile[i] = "Valence_" + channel_real_name[i - 1] + delimeter + corr_coeff[i - 1, 0] + delimeter + corr_coeff[i - 1, 1] + delimeter + corr_coeff[i - 1, 2] + delimeter + corr_coeff[i - 1, 3] + delimeter + corr_coeff[i - 1, 4];
                else
                    safefile[i] = "Arousal_" + channel_real_name[i - 15] + delimeter + corr_coeff[i - 1, 0] + delimeter + corr_coeff[i - 1, 1] + delimeter + corr_coeff[i - 1, 2] + delimeter + corr_coeff[i - 1, 3] + delimeter + corr_coeff[i - 1, 4];
            }

            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();   // The user selected a folder and pressed the OK button.
            folderBrowserDialog1.SelectedPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Resources\\Output";
            folderBrowserDialog1.Description = "Select the directory that you want to use.\nDefault directory is: \\Resources\\Output (in .exe folder).";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                path = new Uri(path).LocalPath;
                string date = string.Format("{0:dd-MM-yyy_hh-mm-ss}", DateTime.Now);
                System.IO.File.WriteAllLines(path + "\\Correlation_Coefficients_" + date + ".csv", safefile);
                MessageBox.Show("Correlation Coefficients saved!");
            }
        }

        private void SaveCorrelationCoefficientsresulttotxttoolStripMenuItem1_Click(object sender, EventArgs e) // Save all results to txt file
        {
            tabControl1.SelectedIndex = 2;
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();   // The user selected a folder and pressed the OK button.
            folderBrowserDialog1.SelectedPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\Resources\\Output";
            folderBrowserDialog1.Description = "Select the directory that you want to use.\nDefault directory is: \\Resources\\Output (in .exe folder).";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string txtfile="";
                string path = folderBrowserDialog1.SelectedPath;
                path = new Uri(path).LocalPath;
                string date = string.Format("{0:dd-MM-yyy_hh-mm-ss}", DateTime.Now);

                string[] corr_coeff_to_string = new string[140];
                int counter = 0;
                for (int i = 0; i < 14; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        corr_coeff[i, j] = correlation_coefficients(b[j], a[j], channels[i], cursor_X_position);
                        corr_coeff[i + 14, j] = correlation_coefficients(b[j], a[j], channels[i], cursor_Y_position);

                        // Convert double to string and change string length to 25 for all the strings (x position)
                        corr_coeff_to_string[counter] = corr_coeff[i, j].ToString();
                        while (corr_coeff_to_string[counter].Length < 25)
                            corr_coeff_to_string[counter] = " " + corr_coeff_to_string[counter];

                        // Convert double to string and change string length to 25 for all the strings (y position)
                        corr_coeff_to_string[counter + 70] = corr_coeff[i + 14, j].ToString();
                        while (corr_coeff_to_string[counter + 70].Length < 25)
                            corr_coeff_to_string[counter + 70] = " " + corr_coeff_to_string[counter + 70];
                        counter++;
                    }
                }

                // Write to file
                counter = 0;
                txtfile = txtfile + "******************************************************* Correlation Coefficients *****************************************************" + Environment.NewLine;
                txtfile = txtfile + Environment.NewLine;
                txtfile = txtfile + Environment.NewLine;
                txtfile = txtfile + "******************************************************* Correlation with Valence *****************************************************" + Environment.NewLine;
                txtfile = txtfile + "Bands               Delta                    Theta                    Alpha                    Beta                     Gamma" + Environment.NewLine;
                for (int i = 0; i < 14; i++)
                {
                    txtfile = txtfile + channel_real_name[i] + ":";

                    for (int j = 0; j < 5; j++)
                        txtfile = txtfile + corr_coeff_to_string[counter++];

                    txtfile = txtfile + Environment.NewLine;
                }

                txtfile = txtfile + Environment.NewLine;
                txtfile = txtfile + Environment.NewLine;
                txtfile = txtfile + "******************************************************* Correlation with Arousal *****************************************************" + Environment.NewLine;
                txtfile = txtfile + "Bands               Delta                    Theta                    Alpha                    Beta                     Gamma" + Environment.NewLine;
                for (int i = 14; i < 28; i++)
                {
                    txtfile = txtfile + channel_real_name[i - 14] + ":";
                    for (int j = 0; j < 5; j++)
                        txtfile = txtfile + corr_coeff_to_string[counter++];

                    txtfile = txtfile + Environment.NewLine;
                }
                txtfile = txtfile + Environment.NewLine;
                txtfile = txtfile + Environment.NewLine;

                txtfile = txtfile + "*********************************************************** Detailed results *********************************************************" + Environment.NewLine;
                txtfile = txtfile + "How to Interpret a Correlation Coefficient r:" + Environment.NewLine;
                txtfile = txtfile + "If correlation Coefficient is closer to : -1   then: A perfect downhill (negative) linear relationship." + Environment.NewLine;
                txtfile = txtfile + "If correlation Coefficient is closer to : -0.7 then: A strong downhill (negative) linear relationship." + Environment.NewLine;
                txtfile = txtfile + "If correlation Coefficient is closer to : -0.5 then: A moderate downhill (negative) relationship." + Environment.NewLine;
                txtfile = txtfile + "If correlation Coefficient is closer to : -0.3 then: A weak downhill (negative) linear relationship." + Environment.NewLine;
                txtfile = txtfile + "If correlation Coefficient is closer to :  0   then: No linear relationship." + Environment.NewLine;
                txtfile = txtfile + "If correlation Coefficient is closer to :  0.3 then: A weak uphill (positive) linear relationship." + Environment.NewLine;
                txtfile = txtfile + "If correlation Coefficient is closer to :  0.5 then: A moderate uphill (positive) relationship." + Environment.NewLine;
                txtfile = txtfile + "If correlation Coefficient is closer to :  0.7 then: A strong uphill (positive) linear relationship." + Environment.NewLine;
                txtfile = txtfile + "If correlation Coefficient is closer to :  1   then: A perfect uphill (positive) linear relationship." + Environment.NewLine;
                //txtfile = txtfile + Environment.NewLine;

                counter = 0;
                for (int i = 0; i < 14; i++)
                {
                    txtfile = txtfile + Environment.NewLine;
                    txtfile = txtfile + "Correlation " + channel_real_name[i] + " with Valence:" + Environment.NewLine;
                    for (int j = 0; j < 5; j++)
                        txtfile = txtfile + Band_name[j] + ":" + corr_coeff_to_string[counter++] + " is closer to: " + closer_to(corr_coeff[i, j]) + " -> " + linear_relationship(closer_to(corr_coeff[i, j])) + Environment.NewLine;
                    txtfile = txtfile + Environment.NewLine;
                    txtfile = txtfile + "Correlation " + channel_real_name[i] + " with Arousal:" + Environment.NewLine;
                    counter = counter - 5;
                    for (int j = 0; j < 5; j++)
                        txtfile = txtfile + Band_name[j] + ":" + corr_coeff_to_string[counter++ + 70] + " is closer to: " + closer_to(corr_coeff[i + 14, j]) + " -> " + linear_relationship(closer_to(corr_coeff[i + 14, j])) + Environment.NewLine;
                }


                File.WriteAllText(path + "\\Correlation_Coefficients_results_" + date + ".txt", txtfile);
                MessageBox.Show("Correlation Coefficients results saved!");
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)    // exit applications
        {
            exit_button_Click(exit_button, EventArgs.Empty);
        }

        //********************************* Tools Menu *********************************
        private void copyPlotToClipBoardToolStripMenuItem_Click(object sender, EventArgs e)// Copy Current Plot Model to ClipBoard 1920x1080
        {
            copy_to_clipboard(1920,1080);
        }

        private void copyPlotToClipBoardCustomResolutionToolStripMenuItem_Click(object sender, EventArgs e)// Copy Current Plot Model to ClipBoard custom resolution
        {
            Form9 frm9 = new Form9();
            frm9.ShowDialog();
            if (frm9.DialogResult == DialogResult.OK)
            {
                int w = frm9.C_Width;            //values preserved after close
                int h = frm9.C_Height;
                copy_to_clipboard(w, h);
            }
            frm9.Dispose();
        }

        //********************************* Buttons *********************************
        private void exit_button_Click(object sender, EventArgs e)              // exit application
        {
            this.Close();
            Application.Exit();
        }

        private void main_app__button_Click(object sender, EventArgs e)         // return in main form window
        {
            this.Close();               // return to main Form (Form1)
        }

        //********************************* Graphs *********************************
        private void Graph_And_Initialize()                                     // Graph the time series of loaded file and initialize channels matrix
        {
            Disable_Cor_Coeff();
            List<double> cursor_x = new List<double>();     
            List<double> cursor_y = new List<double>();     
            cursor_x.Clear();
            cursor_y.Clear();

            int thickness = 1;                              // thickness of plot line

            List<double> input = new List<double>();        // list with emotiv values. one list for all values
            input.Clear();
            LineSeries[] series = new LineSeries[14];       // series matrix. every series has values from one channel
            for (int index = 0; index < 14; index++)        // initialize series
            {
                series[index] = new LineSeries();
                series[index].StrokeThickness = thickness;
                series[index].Title = channel_name[index];
            }

            if (radioButton1.Checked)
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string line;
                    double x = 0;
                    point_counter = 0;
                    string headerLine = reader.ReadLine();

                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] tokens = line.Split(new string[] { delimiter }, StringSplitOptions.None);// split columns for every row
                        string file_name = tokens[0];                                               // first column of file ST_pause

                        if (file_name != "ST_counter" && file_name != "ST_between" && file_name != "ST_media" && file_name != "ST_evaluation" && file_name != "ST_pause" && file_name != "ST_fixation_cross" && file_name != "ST_ended" && file_name != "Arousal.png" && file_name != "Valence.png" && file_name != "fixation_cross.png")
                        {
                            cursor_x.Add(Double.Parse(tokens[1], CultureInfo.InvariantCulture));    
                            cursor_y.Add(Double.Parse(tokens[2], CultureInfo.InvariantCulture));    
                                
                            for (int index = 0; index < 14; index++)
                            {
                                double y = Double.Parse(tokens[index + column_emotiv_data], CultureInfo.InvariantCulture);
                                input.Add(y);
                                series[index].Points.Add(new DataPoint(x, y));
                            }
                            x += 1 / fs;
                            point_counter++;
                        }
                    }
                }
            }
            else 
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string line;
                    double x = 0;
                    point_counter = 0;
                    string headerLine = reader.ReadLine();

                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] tokens = line.Split(new string[] { delimiter }, StringSplitOptions.None);// split columns for every row
                        string file_name = tokens[0];                                               // first column of file
                        if (file_name == comboBox3.SelectedItem.ToString())
                        {
                            cursor_x.Add(Double.Parse(tokens[1], CultureInfo.InvariantCulture));    
                            cursor_y.Add(Double.Parse(tokens[2], CultureInfo.InvariantCulture));    

                            for (int index = 0; index < 14; index++)
                            {
                                double y = Double.Parse(tokens[index + column_emotiv_data], CultureInfo.InvariantCulture);
                                input.Add(y);
                                series[index].Points.Add(new DataPoint(x, y));
                            }
                            x += 1 / fs;
                            point_counter++;
                        }
                    }
                }
            }

            plot1.Model = new PlotModel();
            plot1.Model.LegendPlacement = LegendPlacement.Outside;
            plot1.Model.PlotType = PlotType.XY;
            plot1.Model.Background = OxyColor.FromRgb(255, 255, 255);
            plot1.Model.TextColor = OxyColor.FromRgb(0, 0, 0);
            
            OxyPlot.Axes.LinearAxis linearAxis1 = new OxyPlot.Axes.LinearAxis();
            linearAxis1.Title = "Time (seconds)";
            linearAxis1.MajorGridlineColor = OxyColor.FromArgb(40, 0, 0, 139);
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineColor = OxyColor.FromArgb(20, 0, 0, 139);
            linearAxis1.MinorGridlineStyle = LineStyle.Solid;
            linearAxis1.IsAxisVisible = true;
            linearAxis1.Position = OxyPlot.Axes.AxisPosition.Bottom;
            plot1.Model.Axes.Add(linearAxis1);

            List<OxyPlot.Axes.LinearAxis> linearAxis = new List<OxyPlot.Axes.LinearAxis>();
            double point = 1;
            double diif = 14;

            for (int i = 0; i < 14; i++)
            {
                OxyPlot.Axes.LinearAxis tempAxis = new OxyPlot.Axes.LinearAxis();
                tempAxis.EndPosition = point;
                tempAxis.StartPosition = point - 1 / diif;
                point = point - 1 / diif;
                tempAxis.Key = channel_name[i];
                //tempAxis.Title = channel_name[i];
                tempAxis.Position = OxyPlot.Axes.AxisPosition.Left;
                linearAxis.Add(tempAxis);
                plot1.Model.Axes.Add(linearAxis[i]);

                series[i].YAxisKey = channel_name[i];
                plot1.Model.Series.Add(series[i]);
            }

            plot1.InvalidatePlot(true);

            for (int i = 0; i < 14; i++) 
            {
                channels[i] = new double[point_counter];
            }
            int column = 0;
            for (int j = 0; j < point_counter * 14 ; j = j + 14)
            {
                for (int i = 0; i < 14; i++) 
                {
                    channels[i][column] = input[i + j];             // seperate input values to each channel
                }
                column++;
            }

            cursor_X_position = new double[point_counter];          
            cursor_Y_position = new double[point_counter];          
            for (int i = 0; i < point_counter; i++)                 
            {
                cursor_X_position[i] = cursor_x[i];
                cursor_Y_position[i] = cursor_y[i];
            }
        }
                                                    
        private void Graph_Time_All_Channels()                                  // Graph the time series all or selected channels
        {
            Disable_Cor_Coeff();
            LineSeries[] series = new LineSeries[14];
            List<int> channel_number = new List<int>();
            int ch, thickness = 1, count = 0;

            if (checkBox0.Checked)
            {
                count = 14;
                for (int index = 0; index < 14; index++)
                {
                    channel_number.Add(index);
                }
            }
            else 
            {
                if (checkBox1.Checked) { count++; channel_number.Add(0); }
                if (checkBox2.Checked) { count++; channel_number.Add(1); }
                if (checkBox3.Checked) { count++; channel_number.Add(2); }
                if (checkBox4.Checked) { count++; channel_number.Add(3); }
                if (checkBox5.Checked) { count++; channel_number.Add(4); }
                if (checkBox6.Checked) { count++; channel_number.Add(5); }
                if (checkBox7.Checked) { count++; channel_number.Add(6); }
                if (checkBox8.Checked) { count++; channel_number.Add(7); }
                if (checkBox9.Checked) { count++; channel_number.Add(8); }
                if (checkBox10.Checked) { count++; channel_number.Add(9); }
                if (checkBox11.Checked) { count++; channel_number.Add(10); }
                if (checkBox12.Checked) { count++; channel_number.Add(11); }
                if (checkBox13.Checked) { count++; channel_number.Add(12); }
                if (checkBox14.Checked) { count++; channel_number.Add(13); }
            }
            

            if (count != 0)
            {
                for (int index = 0; index < count; index++)
                {
                    series[index] = new LineSeries();
                    series[index].Title = channel_name[channel_number[index]];
                    series[index].StrokeThickness = thickness;
                }
            }

            double x = 0;
            for (int i = 0; i < point_counter; i++)
            {
                if (checkBox0.Checked)
                {
                    for (int index = 0; index < 14; index++) 
                    {
                        series[index].Points.Add(new DataPoint(x, channels[index][i]));
                    }
                    
                }
                else
                {
                    ch = 0;
                    if (checkBox1.Checked) { series[ch].Points.Add(new DataPoint(x, channels[ch++][i])); }
                    if (checkBox2.Checked) { series[ch].Points.Add(new DataPoint(x, channels[ch++][i])); }
                    if (checkBox3.Checked) { series[ch].Points.Add(new DataPoint(x, channels[ch++][i])); }
                    if (checkBox4.Checked) { series[ch].Points.Add(new DataPoint(x, channels[ch++][i])); }
                    if (checkBox5.Checked) { series[ch].Points.Add(new DataPoint(x, channels[ch++][i])); }
                    if (checkBox6.Checked) { series[ch].Points.Add(new DataPoint(x, channels[ch++][i])); }
                    if (checkBox7.Checked) { series[ch].Points.Add(new DataPoint(x, channels[ch++][i])); }
                    if (checkBox8.Checked) { series[ch].Points.Add(new DataPoint(x, channels[ch++][i])); }
                    if (checkBox9.Checked) { series[ch].Points.Add(new DataPoint(x, channels[ch++][i])); }
                    if (checkBox10.Checked) { series[ch].Points.Add(new DataPoint(x, channels[ch++][i])); }
                    if (checkBox11.Checked) { series[ch].Points.Add(new DataPoint(x, channels[ch++][i])); }
                    if (checkBox12.Checked) { series[ch].Points.Add(new DataPoint(x, channels[ch++][i])); }
                    if (checkBox13.Checked) { series[ch].Points.Add(new DataPoint(x, channels[ch++][i])); }
                    if (checkBox14.Checked) { series[ch].Points.Add(new DataPoint(x, channels[ch++][i])); }
                }
                x = x + 1 / fs;
            }

            plot1.Model = new PlotModel();
            plot1.Model.LegendPlacement = LegendPlacement.Outside;
            plot1.Model.PlotType = PlotType.XY;
            plot1.Model.Background = OxyColor.FromRgb(255, 255, 255);
            plot1.Model.TextColor = OxyColor.FromRgb(0, 0, 0);

            OxyPlot.Axes.LinearAxis linearAxis1 = new OxyPlot.Axes.LinearAxis();
            linearAxis1.Title = "Time (seconds)";
            linearAxis1.MajorGridlineColor = OxyColor.FromArgb(40, 0, 0, 139);
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineColor = OxyColor.FromArgb(20, 0, 0, 139);
            linearAxis1.MinorGridlineStyle = LineStyle.Solid;
            linearAxis1.IsAxisVisible = true;
            linearAxis1.Position = OxyPlot.Axes.AxisPosition.Bottom;
            plot1.Model.Axes.Add(linearAxis1);

            List<OxyPlot.Axes.LinearAxis> linearAxis = new List<OxyPlot.Axes.LinearAxis>();
            double point = 1;
            double diif = count;

            for (int i = 0; i < count; i++)
            {
                OxyPlot.Axes.LinearAxis tempAxis = new OxyPlot.Axes.LinearAxis();
                tempAxis.EndPosition = point;
                tempAxis.StartPosition = point - 1 / diif;
                point = point - 1 / diif;
                tempAxis.Key = channel_name[channel_number[i]];
                //tempAxis.Title = channel_name[channel_number[i]];
                tempAxis.Position = OxyPlot.Axes.AxisPosition.Left;
                linearAxis.Add(tempAxis);
                plot1.Model.Axes.Add(linearAxis[i]);

                series[i].YAxisKey = channel_name[channel_number[i]];
                plot1.Model.Series.Add(series[i]);
            }

            plot1.InvalidatePlot(true);
        }

        private void Graph_Band_All_Channels()                                  // Graph the band series all or selected channels
        {
            Disable_Cor_Coeff();
            int thickness = 1, Band;

            Band = comboBox1.SelectedIndex - 1;
            List<LineSeries> series = new List<LineSeries>();
            List<int> channel_number = new List<int>();
            LineSeries temp = new LineSeries();

            if (checkBox0.Checked)
            {
                for (int index = 0; index < 14; index++)
                {
                    temp = filter(b[Band], a[Band], channels[index]);
                    temp.Title = channel_name[index];
                    temp.StrokeThickness = thickness;
                    series.Add(temp);
                    channel_number.Add(index);
                }
            }
            else
            {
                if (checkBox1.Checked) { temp = filter(b[Band], a[Band], channels[0]); temp.Title = channel_name[0]; temp.StrokeThickness = thickness; series.Add(temp); channel_number.Add(0); }
                if (checkBox2.Checked) { temp = filter(b[Band], a[Band], channels[1]); temp.Title = channel_name[1]; temp.StrokeThickness = thickness; series.Add(temp); channel_number.Add(1); }
                if (checkBox3.Checked) { temp = filter(b[Band], a[Band], channels[2]); temp.Title = channel_name[2]; temp.StrokeThickness = thickness; series.Add(temp); channel_number.Add(2); }
                if (checkBox4.Checked) { temp = filter(b[Band], a[Band], channels[3]); temp.Title = channel_name[3]; temp.StrokeThickness = thickness; series.Add(temp); channel_number.Add(3); }
                if (checkBox5.Checked) { temp = filter(b[Band], a[Band], channels[4]); temp.Title = channel_name[4]; temp.StrokeThickness = thickness; series.Add(temp); channel_number.Add(4); }
                if (checkBox6.Checked) { temp = filter(b[Band], a[Band], channels[5]); temp.Title = channel_name[5]; temp.StrokeThickness = thickness; series.Add(temp); channel_number.Add(5); }
                if (checkBox7.Checked) { temp = filter(b[Band], a[Band], channels[6]); temp.Title = channel_name[6]; temp.StrokeThickness = thickness; series.Add(temp); channel_number.Add(6); }
                if (checkBox8.Checked) { temp = filter(b[Band], a[Band], channels[7]); temp.Title = channel_name[7]; temp.StrokeThickness = thickness; series.Add(temp); channel_number.Add(7); }
                if (checkBox9.Checked) { temp = filter(b[Band], a[Band], channels[8]); temp.Title = channel_name[8]; temp.StrokeThickness = thickness; series.Add(temp); channel_number.Add(8); }
                if (checkBox10.Checked) { temp = filter(b[Band], a[Band], channels[9]); temp.Title = channel_name[9]; temp.StrokeThickness = thickness; series.Add(temp); channel_number.Add(9); }
                if (checkBox11.Checked) { temp = filter(b[Band], a[Band], channels[10]); temp.Title = channel_name[10]; temp.StrokeThickness = thickness; series.Add(temp); channel_number.Add(10); }
                if (checkBox12.Checked) { temp = filter(b[Band], a[Band], channels[11]); temp.Title = channel_name[11]; temp.StrokeThickness = thickness; series.Add(temp); channel_number.Add(11); }
                if (checkBox13.Checked) { temp = filter(b[Band], a[Band], channels[12]); temp.Title = channel_name[12]; temp.StrokeThickness = thickness; series.Add(temp); channel_number.Add(12); }
                if (checkBox14.Checked) { temp = filter(b[Band], a[Band], channels[13]); temp.Title = channel_name[13]; temp.StrokeThickness = thickness; series.Add(temp); channel_number.Add(13); }
            }

            plot1.Model = new PlotModel();
            plot1.Model.LegendPlacement = LegendPlacement.Outside;
            plot1.Model.PlotType = PlotType.XY;
            plot1.Model.Background = OxyColor.FromRgb(255, 255, 255);
            plot1.Model.TextColor = OxyColor.FromRgb(0, 0, 0);

            OxyPlot.Axes.LinearAxis linearAxis1 = new OxyPlot.Axes.LinearAxis();
            linearAxis1.Title = "Time (seconds)";
            linearAxis1.MajorGridlineColor = OxyColor.FromArgb(40, 0, 0, 139);
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineColor = OxyColor.FromArgb(20, 0, 0, 139);
            linearAxis1.MinorGridlineStyle = LineStyle.Solid;
            linearAxis1.IsAxisVisible = true;
            linearAxis1.Position = OxyPlot.Axes.AxisPosition.Bottom;
            plot1.Model.Axes.Add(linearAxis1);

            List<OxyPlot.Axes.LinearAxis> linearAxis = new List<OxyPlot.Axes.LinearAxis>();
            double point = 1;
            double diif = series.Count;

            for (int i = 0; i < series.Count; i++)
            {
                OxyPlot.Axes.LinearAxis tempAxis = new OxyPlot.Axes.LinearAxis();
                tempAxis.EndPosition = point;
                tempAxis.StartPosition = point - 1 / diif;
                point = point - 1 / diif;
                tempAxis.Key = channel_name[channel_number[i]];
                //tempAxis.Title = channel_name[channel_number[i]];
                tempAxis.Position = OxyPlot.Axes.AxisPosition.Left;
                linearAxis.Add(tempAxis);
                plot1.Model.Axes.Add(linearAxis[i]);

                series[i].YAxisKey = channel_name[channel_number[i]];
                plot1.Model.Series.Add(series[i]);
            }
            plot1.InvalidatePlot(true);
        }

        private void Graph_per_Channel()                                        // Graph the time and bands series for selected channel
        {
            Disable_Cor_Coeff();
            LineSeries series1 = new LineSeries { Title = comboBox2.Text, StrokeThickness = 1 };
            List<LineSeries> series = new List<LineSeries>();
            LineSeries temp = new LineSeries();

            int thickness = 1;
            double x = 0;
            
            for (int i = 0; i < point_counter; i++)
            {
                series1.Points.Add(new DataPoint(x, channels[comboBox2.SelectedIndex][i]));
                x = x + 1 / fs;
            }

            for (int i = 0; i < 5; i++) 
            {
                temp = filter(b[i], a[i], channels[comboBox2.SelectedIndex]); temp.Title = Band_name[i]; temp.StrokeThickness = thickness; series.Add(temp);
            }
            
            plot1.Model = new PlotModel();
            plot1.Model.LegendPlacement = LegendPlacement.Outside;
            plot1.Model.PlotType = PlotType.XY;
            plot1.Model.Background = OxyColor.FromRgb(255, 255, 255);
            plot1.Model.TextColor = OxyColor.FromRgb(0, 0, 0);

            OxyPlot.Axes.LinearAxis linearAxis1 = new OxyPlot.Axes.LinearAxis();
            linearAxis1.Title = "Time (seconds)";
            linearAxis1.MajorGridlineColor = OxyColor.FromArgb(40, 0, 0, 139);
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineColor = OxyColor.FromArgb(20, 0, 0, 139);
            linearAxis1.MinorGridlineStyle = LineStyle.Solid;
            //linearAxis1.IsAxisVisible = true;
            linearAxis1.Position = OxyPlot.Axes.AxisPosition.Bottom;
            plot1.Model.Axes.Add(linearAxis1);

            OxyPlot.Axes.LinearAxis linearAxis2 = new OxyPlot.Axes.LinearAxis();
            linearAxis2.StartPosition = 0.75;
            linearAxis2.EndPosition = 1;
            linearAxis2.Title = "Time";
            linearAxis2.Key = "Time";
            linearAxis2.Position = OxyPlot.Axes.AxisPosition.Left;
            plot1.Model.Axes.Add(linearAxis2);
            series1.YAxisKey = "Time";
            plot1.Model.Series.Add(series1);

            List<OxyPlot.Axes.LinearAxis> linearAxis = new List<OxyPlot.Axes.LinearAxis>();
            double point = 0.75;
            double diif = point / 5;
            for (int i = 0; i < 5; i++)
            {
                OxyPlot.Axes.LinearAxis tempAxis = new OxyPlot.Axes.LinearAxis();
                tempAxis.EndPosition = point;
                tempAxis.StartPosition = point - diif;
                point = point - diif;
                tempAxis.Key = Band_name[i];
                tempAxis.Title = Band_name[i];
                tempAxis.Position = OxyPlot.Axes.AxisPosition.Left;
                linearAxis.Add(tempAxis);
                plot1.Model.Axes.Add(linearAxis[i]);

                series[i].YAxisKey = Band_name[i];
                plot1.Model.Series.Add(series[i]);
            }
            
            plot1.InvalidatePlot(true);
        }

        private void Graph_correlation_coefficients()                           // Graph the coefficients with barseries
        {
            string emotiv_images = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath + "\\Resources\\Images\\Emotiv_Contact_Status\\";
            
            //pictureBox3.Image = Image.FromFile(emotiv_images + "cor_coef_big.jpg");
            pictureBox3.Image = Image.FromFile(emotiv_images + "cor_coef_small.jpg");

            Enable_Cor_Coeff();
            plot1.Model = new PlotModel();
            plot1.InvalidatePlot(true);

            for (int i = 0; i < 14; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    // Valence
                    corr_coeff[i, j] = correlation_coefficients(b[j], a[j], channels[i], cursor_X_position);
                    // Arousal
                    corr_coeff[i + 14, j] = correlation_coefficients(b[j], a[j], channels[i], cursor_Y_position);
                }
            }

            calculate_location();

            comboBox4.SelectedIndex = 0;
            splitContainer2.Panel1.Focus();
        }

        //********************************* RadioButtons *********************************
        private void comboBox3_EnabledChanged(object sender, EventArgs e)       // When comboBox3 enabled changed
        {
            comboBox3.SelectedIndex = -1;
            if (comboBox3.Enabled)
            {
                if (comboBox3.Items.Count > 0)
                    comboBox3.SelectedIndex = 0;
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e) // when comboBox3 selection changed
        {
            if (comboBox3.SelectedIndex > -1)
                initializeRadio();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)    // Draw stimuli full length enable or disable
        {
            if (radioButton1.Checked && initialize)
            {
                comboBox3.Enabled = false;
                initializeRadio();
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)    // Draw media file length enable or disable
        {
            if (radioButton2.Checked && initialize)
            {
                comboBox3.Enabled = true;
            }
        }

        //********************************* TabPage1 *********************************
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)// tabControl properties
        {
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            if (tabControl1.SelectedIndex == 0)
                comboBox1.SelectedIndex = 0;
            else if (tabControl1.SelectedIndex == 1)
                comboBox2.SelectedIndex = 0;
            else if (tabControl1.SelectedIndex == 2)
                Graph_correlation_coefficients();
                
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) // User selection {time,delta,theta,alpha,beta,gamma}
        {
            if (comboBox1.SelectedIndex == 0 && initialize)
                Graph_Time_All_Channels();
            else if (comboBox1.SelectedIndex != -1 && initialize)
                Graph_Band_All_Channels();
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)        // if checkBox 1-14 selection changed
        {
            if(!checkBox1.Checked &&
                !checkBox2.Checked &&
                !checkBox3.Checked &&
                !checkBox4.Checked &&
                !checkBox5.Checked &&
                !checkBox6.Checked &&
                !checkBox7.Checked &&
                !checkBox8.Checked &&
                !checkBox9.Checked &&
                !checkBox10.Checked &&
                !checkBox11.Checked &&
                !checkBox12.Checked &&
                !checkBox13.Checked &&
                !checkBox14.Checked && initialize)
            {
                checkBox0.Checked = true;
            }
            if (checkBox0.Checked && checkBox1.Checked ||
                checkBox2.Checked ||
                checkBox3.Checked ||
                checkBox4.Checked ||
                checkBox5.Checked ||
                checkBox6.Checked ||
                checkBox7.Checked ||
                checkBox8.Checked ||
                checkBox9.Checked ||
                checkBox10.Checked ||
                checkBox11.Checked ||
                checkBox12.Checked ||
                checkBox13.Checked ||
                checkBox14.Checked && initialize)
                checkBox0.Checked = false;
        }

        private void checkBox0_CheckedChanged(object sender, EventArgs e)       // if checkBox0 selection changed
        {
            if (checkBox0.Checked && initialize)
            {
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                checkBox3.Checked = false;
                checkBox4.Checked = false;
                checkBox5.Checked = false;
                checkBox6.Checked = false;
                checkBox7.Checked = false;
                checkBox8.Checked = false;
                checkBox9.Checked = false;
                checkBox10.Checked = false;
                checkBox11.Checked = false;
                checkBox12.Checked = false;
                checkBox13.Checked = false;
                checkBox14.Checked = false;
            }
        }

        private void draw_button_Click(object sender, EventArgs e)              // Draw graphigs selected by user
        {
            if (comboBox1.SelectedIndex == 0)
                Graph_Time_All_Channels();
            else if (comboBox1.SelectedIndex != -1)
                Graph_Band_All_Channels();
        }

        //********************************* TabPage2 *********************************
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) // User selection for channel
        {
            if (comboBox2.SelectedIndex != -1)
                Graph_per_Channel();

            // Display emotiv channel's position in headset
            string emotiv_images = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath + "\\Resources\\Images\\Emotiv_Contact_Status\\"; 
            pictureBox1.Image = null;
            if (comboBox2.SelectedIndex == 0) { pictureBox1.Image = Image.FromFile(emotiv_images + "AF3_on.jpg"); }
            else if (comboBox2.SelectedIndex == 1) { pictureBox1.Image = Image.FromFile(emotiv_images + "F7_on.jpg"); }
            else if (comboBox2.SelectedIndex == 2) { pictureBox1.Image = Image.FromFile(emotiv_images + "F3_on.jpg"); }
            else if (comboBox2.SelectedIndex == 3) { pictureBox1.Image = Image.FromFile(emotiv_images + "FC5_on.jpg"); }
            else if (comboBox2.SelectedIndex == 4) { pictureBox1.Image = Image.FromFile(emotiv_images + "T7_on.jpg"); }
            else if (comboBox2.SelectedIndex == 5) { pictureBox1.Image = Image.FromFile(emotiv_images + "P7_on.jpg"); }
            else if (comboBox2.SelectedIndex == 6) { pictureBox1.Image = Image.FromFile(emotiv_images + "O1_on.jpg"); }
            else if (comboBox2.SelectedIndex == 7) { pictureBox1.Image = Image.FromFile(emotiv_images + "O2_on.jpg"); }
            else if (comboBox2.SelectedIndex == 8) { pictureBox1.Image = Image.FromFile(emotiv_images + "P8_on.jpg"); }
            else if (comboBox2.SelectedIndex == 9) { pictureBox1.Image = Image.FromFile(emotiv_images + "T8_on.jpg"); }
            else if (comboBox2.SelectedIndex == 10) { pictureBox1.Image = Image.FromFile(emotiv_images + "FC6_on.jpg"); }
            else if (comboBox2.SelectedIndex == 11) { pictureBox1.Image = Image.FromFile(emotiv_images + "F4_on.jpg"); }
            else if (comboBox2.SelectedIndex == 12) { pictureBox1.Image = Image.FromFile(emotiv_images + "F8_on.jpg"); }
            else if (comboBox2.SelectedIndex == 13) { pictureBox1.Image = Image.FromFile(emotiv_images + "AF4_on.jpg"); }
        }

        //********************************* TabPage3 *********************************
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e) // User selection for channel
        {
            textBox2.Text = null;
            textBox2.Text = textBox2.Text + "Correlation Coefficients" + Environment.NewLine;
            textBox2.Text = textBox2.Text + Environment.NewLine;
            textBox2.Text = textBox2.Text + "Delta" + Environment.NewLine;
            textBox2.Text = textBox2.Text + "X: " + corr_coeff[comboBox4.SelectedIndex, 0] + Environment.NewLine;
            textBox2.Text = textBox2.Text + "Y: " + corr_coeff[comboBox4.SelectedIndex + 14, 0] + Environment.NewLine;
            //textBox2.Text = textBox2.Text + Environment.NewLine;
            textBox2.Text = textBox2.Text + "Theta" + Environment.NewLine;
            textBox2.Text = textBox2.Text + "X: " + corr_coeff[comboBox4.SelectedIndex, 1] + Environment.NewLine;
            textBox2.Text = textBox2.Text + "Y: " + corr_coeff[comboBox4.SelectedIndex + 14, 1] + Environment.NewLine;
            //textBox2.Text = textBox2.Text + Environment.NewLine;
            textBox2.Text = textBox2.Text + "Alpha" + Environment.NewLine;
            textBox2.Text = textBox2.Text + "X: " + corr_coeff[comboBox4.SelectedIndex, 2] + Environment.NewLine;
            textBox2.Text = textBox2.Text + "Y: " + corr_coeff[comboBox4.SelectedIndex + 14, 2] + Environment.NewLine;
            //textBox2.Text = textBox2.Text + Environment.NewLine;
            textBox2.Text = textBox2.Text + "Beta" + Environment.NewLine;
            textBox2.Text = textBox2.Text + "X: " + corr_coeff[comboBox4.SelectedIndex, 3] + Environment.NewLine;
            textBox2.Text = textBox2.Text + "Y: " + corr_coeff[comboBox4.SelectedIndex + 14, 3] + Environment.NewLine;
            //textBox2.Text = textBox2.Text + Environment.NewLine;
            textBox2.Text = textBox2.Text + "Gamma" + Environment.NewLine;
            textBox2.Text = textBox2.Text + "X: " + corr_coeff[comboBox4.SelectedIndex, 4] + Environment.NewLine;
            textBox2.Text = textBox2.Text + "Y: " + corr_coeff[comboBox4.SelectedIndex + 14, 4];

            // Display emotiv channel's position in headset
            string emotiv_images = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath + "\\Resources\\Images\\Emotiv_Contact_Status\\";
            pictureBox2.Image = null;
            if (comboBox4.SelectedIndex == 0) { pictureBox2.Image = Image.FromFile(emotiv_images + "AF3_on.jpg"); }
            else if (comboBox4.SelectedIndex == 1) { pictureBox2.Image = Image.FromFile(emotiv_images + "F7_on.jpg"); }
            else if (comboBox4.SelectedIndex == 2) { pictureBox2.Image = Image.FromFile(emotiv_images + "F3_on.jpg"); }
            else if (comboBox4.SelectedIndex == 3) { pictureBox2.Image = Image.FromFile(emotiv_images + "FC5_on.jpg"); }
            else if (comboBox4.SelectedIndex == 4) { pictureBox2.Image = Image.FromFile(emotiv_images + "T7_on.jpg"); }
            else if (comboBox4.SelectedIndex == 5) { pictureBox2.Image = Image.FromFile(emotiv_images + "P7_on.jpg"); }
            else if (comboBox4.SelectedIndex == 6) { pictureBox2.Image = Image.FromFile(emotiv_images + "O1_on.jpg"); }
            else if (comboBox4.SelectedIndex == 7) { pictureBox2.Image = Image.FromFile(emotiv_images + "O2_on.jpg"); }
            else if (comboBox4.SelectedIndex == 8) { pictureBox2.Image = Image.FromFile(emotiv_images + "P8_on.jpg"); }
            else if (comboBox4.SelectedIndex == 9) { pictureBox2.Image = Image.FromFile(emotiv_images + "T8_on.jpg"); }
            else if (comboBox4.SelectedIndex == 10) { pictureBox2.Image = Image.FromFile(emotiv_images + "FC6_on.jpg"); }
            else if (comboBox4.SelectedIndex == 11) { pictureBox2.Image = Image.FromFile(emotiv_images + "F4_on.jpg"); }
            else if (comboBox4.SelectedIndex == 12) { pictureBox2.Image = Image.FromFile(emotiv_images + "F8_on.jpg"); }
            else if (comboBox4.SelectedIndex == 13) { pictureBox2.Image = Image.FromFile(emotiv_images + "AF4_on.jpg"); }

            int counter = 0;
            for (int i = 0; i < 140; i++)
            {
                if (i == comboBox4.SelectedIndex + counter)
                {
                    //lb[i].ForeColor = Color.Blue;
                    lb[i].Font = new Font(lb[i].Font, FontStyle.Bold);
                    counter += 14;
                }
                else 
                {
                    lb[i].Font = new Font(lb[i].Font, FontStyle.Regular);
                    //lb[i].ForeColor = Color.Black;
                }
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)              // picturebox event
        {
            splitContainer2.Panel1.Focus();
        }

        private void Enable_Cor_Coeff()                                         // Enabled when tabPage3 selected
        {
            pictureBox3.Visible = true;
            panel3.Visible = true;
            ExportplottopngtoolStripMenuItem1.Enabled = false;
            ExportplottopngcustomresolutiontoolStripMenuItem1.Enabled = false;
            copyPlotToClipBoardToolStripMenuItem.Enabled = false;
            copyPlotToClipBoardCustomResolutionToolStripMenuItem.Enabled = false;
        }

        private void Disable_Cor_Coeff()                                        // Enabled when tabPage1-2 selected
        {
            pictureBox3.Visible = false;
            panel3.Visible = false;
            ExportplottopngtoolStripMenuItem1.Enabled = true;
            ExportplottopngcustomresolutiontoolStripMenuItem1.Enabled = true;
            copyPlotToClipBoardToolStripMenuItem.Enabled = true;
            copyPlotToClipBoardCustomResolutionToolStripMenuItem.Enabled = true;
        }

        private void CreatePicture()                                            // Create PictureBoxes
        {
            for (int i = 0; i < 140; i++)
            {
                PictureBox tp = new PictureBox();
                tp.BackColor = Color.Transparent;
                //tp.SizeMode = PictureBoxSizeMode.Zoom;
                tp.Visible = false;
                pcbx[i] = tp;
                this.pictureBox3.Controls.Add(this.pcbx[i]);

                Label tmp = new Label();
                tmp.BackColor = Color.Transparent;
                tmp.Visible = false;
                lb[i] = tmp;
                this.pictureBox3.Controls.Add(this.lb[i]);
            }
        }

        private void calculate_location()                                       // calculate PictureBoxes location
        {
            // Big image
            /*
            int[] x = new int[] { 176, 135, 187, 166, 121, 149, 181, 250, 281, 306, 261, 242, 292, 250 };
            int[] y = new int[] { 149, 206, 186, 247, 283, 366, 415, 415, 366, 283, 247, 186, 206, 149 };
            int[] px = new int[] { 152, 106, 187, 133, 89, 137, 183, 264, 304, 355, 310, 254, 338, 290 };
            int[] py = new int[] { 161, 211, 201, 242, 283, 384, 430, 430, 384, 283, 242, 201, 211, 161 };
            int xdis = 336, ydis = 468;
            
            System.Drawing.Size lbsize = new System.Drawing.Size(49, 13);
            System.Drawing.Size pcbxsize = new System.Drawing.Size(33, 33);
            */

            // Small image
            //x = new int[] { 102, 78, 106, 95, 71, 87, 104, 139, 160, 172, 147, 137, 165, 145 }; //x = new int[] { 102, 77, 104, 95, 71, 87, 104, 139, 160, 172, 147, 137, 165, 145 };
            //y = new int[] { 81, 114, 102, 140, 161, 207, 233, 233, 207, 161, 140, 102, 114, 81 };

            x = new int[] { 87, 59, 107, 97, 52, 99, 104, 150, 195, 203, 198, 165, 193, 166 }; //x = new int[] { 102, 77, 104, 95, 71, 87, 104, 139, 160, 172, 147, 137, 165, 145 };
            y = new int[] { 79, 108, 102, 142, 181, 218, 267, 267, 218, 181, 142, 117, 108, 79 };

            px = new int[] { 87, 59, 107, 76, 52, 78, 104, 150, 174, 203, 177, 144, 193, 166 };
            py = new int[] { 91, 120, 114, 138, 160, 218, 246, 246, 218, 160, 138, 114, 120, 91 };
            xdis = 192;
            ydis = 268;

            lbsize = new System.Drawing.Size(30, 12);
            pcbxsize = new System.Drawing.Size(21, 21); // 20, 20


            // image and container dimensions
            int w_i = pictureBox3.Image.Width;
            int h_i = pictureBox3.Image.Height;
            int w_c = pictureBox3.Width;
            int h_c = pictureBox3.Height;

            float imageRatio = w_i / (float)h_i; // image W:H ratio
            float containerRatio = w_c / (float)h_c; // container W:H ratio

            if (imageRatio >= containerRatio)
            {
                if (splitContainer2.Panel1.Width <= w_i)
                {
                    pictureBox3.SizeMode = PictureBoxSizeMode.AutoSize;
                    pictureBox3.Dock = DockStyle.None;
                }
                else
                {
                    pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox3.Dock = DockStyle.Fill;
                    // horizontal image
                    float scaleFactor = w_c / (float)w_i;
                    float imageHeigt = w_c / (float)imageRatio;
                    float scaledHeight = imageHeigt / (float)h_i;
                    float filler = Math.Abs(h_c - imageHeigt) / 2;

                    for (int i = 0; i < 14; i++)
                    {
                        x[i] = (int)(x[i] * scaleFactor);
                        px[i] = (int)(px[i] * scaleFactor);

                        y[i] = (int)(y[i] * scaledHeight + filler);
                        py[i] = (int)(py[i] * scaledHeight + filler);
                    }

                    lbsize = new System.Drawing.Size((int)(lbsize.Width * scaleFactor), (int)(lbsize.Height * scaledHeight));
                    pcbxsize = new System.Drawing.Size((int)(pcbxsize.Width * scaledHeight), (int)(pcbxsize.Height * scaledHeight));
                    xdis = (int)(xdis * scaleFactor);
                    ydis = (int)(ydis * scaledHeight);
                }
            }
            else
            {
                if (splitContainer2.Panel1.Height <= h_i)
                {
                    pictureBox3.SizeMode = PictureBoxSizeMode.AutoSize;
                    pictureBox3.Dock = DockStyle.None;
                }
                else
                {
                    pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox3.Dock = DockStyle.Fill;
                    // vertical image
                    float scaleFactor = h_c / (float)h_i;
                    float imageWidth = h_c * imageRatio;
                    float scaledWidth = imageWidth / (float)w_i;
                    float filler = Math.Abs(w_c - imageWidth) / 2;

                    for (int i = 0; i < 14; i++)
                    {
                        x[i] = (int)(x[i] * scaledWidth + filler);
                        px[i] = (int)(px[i] * scaledWidth + filler);

                        y[i] = (int)(y[i] * scaleFactor);
                        py[i] = (int)(py[i] * scaleFactor);
                    }

                    lbsize = new System.Drawing.Size((int)(lbsize.Width * scaledWidth), (int)(lbsize.Height * scaleFactor));
                    pcbxsize = new System.Drawing.Size((int)(pcbxsize.Width * scaledWidth), (int)(pcbxsize.Height * scaledWidth));
                    xdis = (int)(xdis * scaledWidth);
                    ydis = (int)(ydis * scaleFactor);
                }
            }

            int width = pcbxsize.Width - 1;
            int height = pcbxsize.Height - 1;
            Point loc = new Point(0, 0);
            Graphics graphics;
            Bitmap bmp;
            Brush brush;
            int counteri = 0;

            double Min = corr_coeff.Min2D();
            double Max = corr_coeff.Max2D();
            label2.Text = Min.ToString();
            label3.Text = Max.ToString();

            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 14; i++)
                {
                    // Valence
                    pcbx[counteri].Location = new Point((px[i] + j * xdis), py[i]);
                    pcbx[counteri].Size = pcbxsize;

                    // graphics
                    bmp = new Bitmap(width, height);
                    graphics = Graphics.FromImage(bmp);
                    //brush = new SolidBrush(GetColour(corr_coeff[i, j], -1.0, 1.0));
                    
                    //Denug
                    brush = new SolidBrush(GetColour(corr_coeff[i, j], Min, Max));
                    graphics.FillEllipse(brush, new Rectangle(loc.X, loc.Y, width, height));
                    pcbx[counteri].Image = bmp;
                    pcbx[counteri].Visible = true;

                    lb[counteri].Text = corr_coeff[i, j].ToString();
                    lb[counteri].Location = new Point((x[i] + j * xdis), y[i]);
                    lb[counteri].Size = lbsize;
                    //lb[counteri].Text = corr_coeff[i, j].ToString();
                    lb[counteri].Visible = true;

                    // Arousal
                    pcbx[counteri + 70].Location = new Point((px[i] + j * xdis), (py[i] + ydis));
                    pcbx[counteri + 70].Size = pcbxsize;

                    // graphics
                    bmp = new Bitmap(width, height);
                    graphics = Graphics.FromImage(bmp);
                    //brush = new SolidBrush(GetColour(corr_coeff[i + 14, j], -1.0, 1.0));

                    //Denug
                    brush = new SolidBrush(GetColour(corr_coeff[i + 14, j], Min, Max));
                    graphics.FillEllipse(brush, new Rectangle(loc.X, loc.Y, width, height));
                    pcbx[counteri + 70].Image = bmp;
                    pcbx[counteri + 70].Visible = true;

                    lb[counteri + 70].Text = corr_coeff[i + 14, j].ToString();
                    lb[counteri + 70].Location = new Point((x[i] + j * xdis), (y[i] + ydis));
                    lb[counteri + 70].Size = lbsize;
                    //lb[counteri + 70].Text = corr_coeff[i + 14, j].ToString();
                    lb[counteri + 70].Visible = true;
                    counteri++;
                }
            }
        }

        private void pictureBox3_SizeChanged(object sender, EventArgs e)        // PictureBox3 size change
        {
            calculate_location();
        }

        private void splitContainer2_Panel1_SizeChanged(object sender, EventArgs e) // panel 1 size change
        {
            if (tabControl1.SelectedIndex == 2)
            {
                // image and container dimensions
                int w_i = pictureBox3.Image.Width;
                int h_i = pictureBox3.Image.Height;
                int w_c = splitContainer2.Panel1.Width;
                int h_c = splitContainer2.Panel1.Height;

                float imageRatio = w_i / (float)h_i; // image W:H ratio
                float containerRatio = w_c / (float)h_c; // container W:H ratio
                if (imageRatio >= containerRatio)
                {
                    if (splitContainer2.Panel1.Width <= w_i)
                    {
                        pictureBox3.SizeMode = PictureBoxSizeMode.AutoSize;
                        pictureBox3.Dock = DockStyle.None;
                    }
                    else
                    {
                        pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
                        pictureBox3.Dock = DockStyle.Fill;
                    }
                }
                else
                {
                    if (splitContainer2.Panel1.Height <= h_i)
                    {
                        pictureBox3.SizeMode = PictureBoxSizeMode.AutoSize;
                        pictureBox3.Dock = DockStyle.None;
                    }
                    else
                    {
                        pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
                        pictureBox3.Dock = DockStyle.Fill;
                    }
                }
            }
        }

        //********************************* filter and correlation *********************************
        private LineSeries filter(double[] b, double[] a, double[] input)       // filter time series to extract bands
        {
            LineSeries output = new LineSeries();
            int a_order = a.Length;
            int b_order = b.Length;
            int N = input.Length;
            int NZeros = Math.Max(a_order, b_order)-1;

            double[] Y_signal = new double[NZeros+N];
            for (int i = 0; i < NZeros + N; i++) 
                Y_signal[i] = 0;

            double[] temp = new double[NZeros];
            for (int i = 0; i < NZeros; i++)
                temp[i] = 0;

            double[] X_signal = new double[NZeros + N];
            temp.CopyTo(X_signal, 0);
            input.CopyTo(X_signal, NZeros);

            double x = 0;
            for (int i = NZeros; i < NZeros + N; i++)
            {
                double sum = 0;
                for (int j = 0; j < b_order; j++)
                {
                    sum = sum + b[j] * X_signal[i - j];
                }
                for (int j = 1; j < a_order; j++)
                {
                    sum = sum - a[j] * Y_signal[i - j];
                }
                Y_signal[i] = sum;

                //Y_signal[i] = b[0] * X_signal[i] + b[1] * X_signal[i - 1] + b[2] * X_signal[i - 2] + b[3] * X_signal[i - 3] + b[4] * X_signal[i - 4] + b[5] * X_signal[i - 5] + b[6] * X_signal[i - 6] + b[7] * X_signal[i - 7] + b[8] * X_signal[i - 8] + b[9] * X_signal[i - 9] + b[10] * X_signal[i - 10] -
                //                                   a[1] * Y_signal[i - 1] - a[2] * Y_signal[i - 2] - a[3] * Y_signal[i - 3] - a[4] * Y_signal[i - 4] - a[5] * Y_signal[i - 5] - a[6] * Y_signal[i - 6] - a[7] * Y_signal[i - 7] - a[8] * Y_signal[i - 8] - a[9] * Y_signal[i - 9] - a[10] * Y_signal[i - 10];

                output.Points.Add(new DataPoint(x, Y_signal[i]));
                x = x + 1 / fs;
            }

            return output;
        }

        private double[] filter_double(double[] b, double[] a, double[] input)  // filter time series to extract bands
        {
            double[] output = new double[point_counter];
            List<double> list = new List<double>();
            list.Clear();
            int a_order = a.Length;
            int b_order = b.Length;
            int N = input.Length;
            int NZeros = Math.Max(a_order, b_order) - 1;

            double[] Y_signal = new double[NZeros + N];
            for (int i = 0; i < NZeros + N; i++)
                Y_signal[i] = 0;

            double[] temp = new double[NZeros];
            for (int i = 0; i < NZeros; i++)
                temp[i] = 0;

            double[] X_signal = new double[NZeros + N];
            temp.CopyTo(X_signal, 0);
            input.CopyTo(X_signal, NZeros);            

            for (int i = NZeros; i < NZeros + N; i++)
            {
                double sum = 0;
                for (int j = 0; j < b_order; j++)
                {
                    sum = sum + b[j] * X_signal[i - j];
                }
                for (int j = 1; j < a_order; j++)
                {
                    sum = sum - a[j] * Y_signal[i - j];
                }
                Y_signal[i] = sum;

                //Y_signal[i] = b[0] * X_signal[i] + b[1] * X_signal[i - 1] + b[2] * X_signal[i - 2] + b[3] * X_signal[i - 3] + b[4] * X_signal[i - 4] + b[5] * X_signal[i - 5] + b[6] * X_signal[i - 6] + b[7] * X_signal[i - 7] + b[8] * X_signal[i - 8] + b[9] * X_signal[i - 9] + b[10] * X_signal[i - 10] -
                //                                   a[1] * Y_signal[i - 1] - a[2] * Y_signal[i - 2] - a[3] * Y_signal[i - 3] - a[4] * Y_signal[i - 4] - a[5] * Y_signal[i - 5] - a[6] * Y_signal[i - 6] - a[7] * Y_signal[i - 7] - a[8] * Y_signal[i - 8] - a[9] * Y_signal[i - 9] - a[10] * Y_signal[i - 10];

                 double tempasd = Y_signal[i] * Y_signal[i];
                 list.Add(tempasd);
            }

            output = list.ToArray();
            return output;
        }

        private double correlation_coefficients(double[] b, double[] a, double[] channel, double[] position) // calculate the correlation coefficients betwenn channels and mouse position
        {
            double coefficient = 0;
            double[] output = new double[point_counter];
            output = filter_double(b, a, channel);
            coefficient = Correlation.Pearson(output, position);               // calculate the correlation coefficients
            
            return coefficient;
        }

        //********************************* methods *********************************
        private void copy_to_clipboard(int w,int h)                             // copy plot to clipboard
        {
            using (var stream = new MemoryStream())
            {
                var pngExporter = new PngExporter() { Width = w, Height = h };    //{ Width = 1920, Height = 1080 };
                pngExporter.Export(plot1.Model, stream);

                Bitmap bm = new Bitmap(stream);
                Clipboard.SetImage(bm);
            }
        }

        private double closer_to(double input)                                  // return double closer to input
        {
            double output = 0;
            double[] array = new double[9] { -1, -0.7, -0.5, -0.3, 0, 0.3, 0.5, 0.7, 1  };
            double[] temp = new double[9] { 0,0,0,0,0,0,0,0,0 };
            for (int i = 0; i < 9; i++) 
            {
                temp[i] = Math.Abs(array[i] - input);
            }
            int position = -1;
            double min = 10;
            for (int i = 0; i < 9; i++)
            {
                if (temp[i] < min)
                {
                    position = i;
                    min = temp[i];
                }
            }
            output = array[position];
            return output;
        }

        private string linear_relationship(double input)                        // linear relationship status
        {
            string output = null;

            if (input == -1) { output = "A perfect downhill (negative) linear relationship"; }
            else if (input == -0.7) { output = "A strong downhill (negative) linear relationship "; }
            else if (input == -0.5) { output = "A moderate downhill (negative) relationship      "; }
            else if (input == -0.3) { output = "A weak downhill (negative) linear relationship   "; }
            else if (input == 0) { output = "No linear relationship                           "; }
            else if (input == 0.3) { output = "A weak uphill (positive) linear relationship     "; }
            else if (input == 0.5) { output = "A moderate uphill (positive) relationship        "; }
            else if (input == 0.7) { output = "A strong uphill (positive) linear relationship   "; }
            else if (input == 1) { output = "A perfect uphill (positive) linear relationship  "; }

            return output;
        }

        private char Detect(TextReader reader, int rowCount, IList<char> separators) // detect delimiter in file
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

        private Color GetColour(double v, double vmin, double vmax)             // colormap - double to rgb color
        {
            /*
            Return a RGB colour value given a scalar v in the range [vmin,vmax]
            In this case each colour component ranges from 0 (no contribution) to
            1 (fully saturated), modifications for other ranges is trivial.
            The colour is clipped at the end of the scales if v is outside
            the range [vmin,vmax]
            */

            Color col = Color.Transparent;
            double r = 1.0, g = 1.0, b = 1.0;
            double dv;

            if (v < vmin)
                v = vmin;
            if (v > vmax)
                v = vmax;
            dv = vmax - vmin;

            if (v < (vmin + 0.25 * dv))
            {
                r = 0;
                g = 4 * (v - vmin) / dv;
            }
            else if (v < (vmin + 0.5 * dv))
            {
                r = 0;
                b = 1 + 4 * (vmin + 0.25 * dv - v) / dv;
            }
            else if (v < (vmin + 0.75 * dv))
            {
                r = 4 * (v - vmin - 0.5 * dv) / dv;
                b = 0;
            }
            else
            {
                g = 1 + 4 * (vmin + 0.75 * dv - v) / dv;
                b = 0;
            }

            /*
            int red = Math.Min((int)(X * 256), 255);
            int green = Math.Min((int)((X * 256 - red) * 256), 255);
            int blue = Math.Min((int)(((X * 256 - red) * 256 - green) * 256), 255);
            */
            r = r * 255;
            g = g * 255;
            b = b * 255;
            col = Color.FromArgb((int)r, (int)g, (int)b);
            return col;

        }


    }
}
