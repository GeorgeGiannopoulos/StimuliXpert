using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emotiv;
using System.ComponentModel;
using System.Timers;
using System.IO;

namespace StimuliXpert
{
    class mySuperEmotiv
    {
        /*
         ********************************* class Informations *********************************
         * [Variables]
         * bufferSSize = loop size. When loop elapsed, emotiv values saved file 
         * emotivConnected = emotiv status {connected, disconnected}
         * currentUser = id of current user (given in object load)
         * emoLabel = emotion label
         * filenameChn = output name and date of the file
         * basePath = path of folder where the output file saved (given in object load)
         * experimentUserID = same as currentUser but for experiment purposes
         * 
         * engine = communicates with the Emotiv, manages settings and translates the Emotiv detection results into an EmoState
         * es = Containing information about the current state of all activated Emotivs
         * userID = used to uniquely identify a user's headset
         * emotiv_battery_level = present the battery level of emotiv
         * emotiv_status = present the status of emotiv
         * emotiv_wireless_signal_status = present the status of wireless signal
         * 
         * worker = backgroung worker where the values of emotiv saved in file
         * timer = check every 1/(128 KHz) = 7.8125 seconds the status of emotiv read\write
         * 
         * [Mesthods]
         * mySuperEmotiv() = Initialize Emotiv Engine
         * engine_UserAdded() =  Handle new connected user
         * 
         * engine_EmoStateUpdated() = update emotiv state and get channels statuses
         * get_emotiv_battery_level() = get emotiv battery level
         * get_emotiv_status() = get emotiv status
         * get_emotiv_wireless_signal_status() = get emotiv wireless signal status
         * getEmotionlabel() = get Emotion label 
         * 
         * worker_DoWork() = backgroung worker where the values of emotiv saved in file
         * progress() = worker progress level
         * resetall() = reset j and emoLabel
         * SetOutputFilename() = set output file name and date
         * delimiter_return = return delimiter from input = name
         * 
         * Start() = Start the recording
         * Stop() = Stop the recording
         * timer_Elapsed() = timer elapsed event
         * 
         ********************************* Main Form ***************************************
         */

        //********************************* Declare Variables *********************************
        int currentUser;                    // id of current user (given in object load)
        string filenameChn;                 // output name and date of the file
        string basePath, experimentUserID;  // basePath = path of folder where the output file saved | experimentUserID = same as currentUser but for experiment purposes
        string delimiter;
        string file_extension;

        //********************************* Engine Initialization *****************************
        EmoEngine engine;                   // EmoEngine communicates with the Emotiv, manages settings and translates the Emotiv detection results into an EmoState
        EmoState es;                        // Containing information about the current state of all activated Emotivs
        int userID = -1;                    // userID is used to uniquely identify a user's headset

        //********************************* Engine Status Variables *****************************
        private float emotiv_battery_level;         // present the battery level of emotiv
        private bool emotivConnected;               // emotiv status {connected, disconnected}
        private int emotiv_wireless_signal_status;  // present the status of wireless signal
        private string emotiv_contacts_quality;

        //********************************* worker Variables *****************************
        BackgroundWorker worker;            // backgroung worker where the values of emotiv saved in file
        System.Timers.Timer timer;                        // check every 1/(128 KHz) = 7.8125 seconds the status of emotiv read\write

        Object _lockThis2;

        //********************************* Custom Variables *****************************
        public float New_emotiv_battery_level
        {
            get { return emotiv_battery_level; }
            set
            {
                if (emotiv_battery_level != value)
                {
                    emotiv_battery_level = value;
                    OnBatteryChanged(new FloatEventArgs(emotiv_battery_level));
                }
            }
        }

        public int New_emotiv_wireless_signal_status
        {
            get { return emotiv_wireless_signal_status; }
            set
            {
                if (emotiv_wireless_signal_status != value)
                {
                    emotiv_wireless_signal_status = value;
                    OnWireless_Signal_Changed(new IntEventArgs(emotiv_wireless_signal_status));
                }
            }
        }

        public bool New_emotivConnected
        {
            get { return emotivConnected; }
            set
            {
                if (emotivConnected != value)
                {
                    emotivConnected = value;
                    OnConnected_Changed(new BoolEventArgs(emotivConnected));
                }
            }
        }

        public string New_emotiv_contacts_quality
        {
            get { return emotiv_contacts_quality; }
            set
            {
                if (emotiv_contacts_quality != value)
                {
                    emotiv_contacts_quality = value;
                    OnContacts_quality_Changed(new TextEventArgs(emotiv_contacts_quality));
                }
            }
        }

        //********************************* Custom Events *****************************
        public event EventHandler<FloatEventArgs> BatteryChanged;
        public event EventHandler<IntEventArgs> Wireless_Signal_Changed;
        public event EventHandler<BoolEventArgs> Connected_Changed;
        public event EventHandler<TextEventArgs> Contacts_quality_Changed;

        //********************************* Emotiv Methods *********************************
        public mySuperEmotiv(int currentUser)                                   // Initialize Emotiv Engine
        {
            this.currentUser = currentUser;

            // create the engine
            engine = EmoEngine.Instance;
            engine.UserAdded += new EmoEngine.UserAddedEventHandler(engine_UserAdded);
            engine.EmoStateUpdated += new EmoEngine.EmoStateUpdatedEventHandler(engine_EmoStateUpdated);
            
            // connect to Emoengine.  
            engine.Connect();

            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);

            _lockThis2 = new Object();

        }

        public mySuperEmotiv(string experimentUserID)                           // Initialize Emotiv Engine
        {
            this.experimentUserID = experimentUserID;

            // create the engine
            engine = EmoEngine.Instance;
            engine.UserAdded += new EmoEngine.UserAddedEventHandler(engine_UserAdded);
            engine.EmoStateUpdated += new EmoEngine.EmoStateUpdatedEventHandler(engine_EmoStateUpdated);

            // connect to Emoengine.  
            engine.Connect();

            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);

            _lockThis2 = new Object();
        }

        private void engine_UserAdded(object sender, EmoEngineEventArgs e)      // Handle new connected user
        {
            // record the user 
            userID = (int)e.userId;

            // enable data aquisition for this user.
            engine.DataAcquisitionEnable((uint)userID, true);

            // ask for up to 1 second of buffered data
            engine.EE_DataSetBufferSizeInSec(0.050f);

            if (userID == 0)                    // first connected user
                UserInfo.user1Active = true;
            if (userID == 1)                    // second connected user
                UserInfo.user2Active = true;
        }

        private void engine_EmoStateUpdated(object sender, EmoStateUpdatedEventArgs e)  // update emotiv state and get channels statuses
        {
            es = e.emoState;
            if (e.userId == currentUser)
            {
                lock (_lockThis2)
                {
                    var b1 = es.CognitivGetCurrentAction(); 
                    string c1 = b1.ToString();

                    var b2 = es.ExpressivGetLowerFaceAction();
                    string c2 = b2.ToString();

                    var b3 = es.ExpressivGetUpperFaceAction(); 
                    string c3 = b3.ToString();

                    EdkDll.EE_SignalStrength_t b4 = es.GetWirelessSignalStatus();               // emotiv wireless signal Status
                    int getWirelessSignalStatusInt = -1;
                    if (b4 == EdkDll.EE_SignalStrength_t.NO_SIGNAL)                             // emotiv wireless signal Status = NO_SIGNAL
                        getWirelessSignalStatusInt = 0;
                    else if (b4 == EdkDll.EE_SignalStrength_t.BAD_SIGNAL)                       // emotiv wireless signal Status = BAD_SIGNAL
                        getWirelessSignalStatusInt = 1;
                    else if (b4 == EdkDll.EE_SignalStrength_t.GOOD_SIGNAL)                      // emotiv wireless signal Status = GOOD_SIGNAL
                        getWirelessSignalStatusInt = 2;

                    bool Connected = false;
                    if (getWirelessSignalStatusInt != 0)
                    {
                        Connected = true;
                    }
                    //New_emotivConnected = Connected;
                    //New_emotiv_wireless_signal_status = getWirelessSignalStatusInt;

                    int batteryChargeLevel;                                                     // emotiv battery level
                    int maxChargeLevel;                                                         // emotiv max battery level
                    es.GetBatteryChargeLevel(out batteryChargeLevel, out maxChargeLevel);       // get emotiv battery level
                    float bat = ((float)batteryChargeLevel) / ((float)maxChargeLevel);          // bat = % current's user emotiv battery level
                    //New_emotiv_battery_level = bat;                                                 // % current's user emotiv battery level

                    EdkDll.EE_EEG_ContactQuality_t[] CQTable = new EdkDll.EE_EEG_ContactQuality_t[15];  // emotiv Contact Quality for all channels
                    int[] CQTableInt = new int[15];
                    string cq = "";
                    for (int i = 0; i < 14; i++)
                    {
                        CQTable[i] = es.GetContactQuality(i+3);                                   // emotiv Contact Quality for i channels
                        if (CQTable[i] == EdkDll.EE_EEG_ContactQuality_t.EEG_CQ_NO_SIGNAL)      // if emotiv Contact Quality for i channels = NO_SIGNAL
                            CQTableInt[i] = 0;
                        else if (CQTable[i] == EdkDll.EE_EEG_ContactQuality_t.EEG_CQ_VERY_BAD)  // if emotiv Contact Quality for i channels = VERY_BAD
                            CQTableInt[i] = 1;
                        else if (CQTable[i] == EdkDll.EE_EEG_ContactQuality_t.EEG_CQ_POOR)      // if emotiv Contact Quality for i channels = POOR
                            CQTableInt[i] = 2;
                        else if (CQTable[i] == EdkDll.EE_EEG_ContactQuality_t.EEG_CQ_FAIR)      // if emotiv Contact Quality for i channels = FAIR
                            CQTableInt[i] = 3;
                        else if (CQTable[i] == EdkDll.EE_EEG_ContactQuality_t.EEG_CQ_GOOD)      // if emotiv Contact Quality for i channels = GOOD
                            CQTableInt[i] = 4;
                        else
                            CQTableInt[i] = -1;
                        cq = cq + CQTableInt[i].ToString() + ",";
                    }

                    cq = cq.Remove(cq.Length - 1); // remove the invalid character

                    New_emotivConnected = Connected;
                    New_emotiv_wireless_signal_status = getWirelessSignalStatusInt;
                    New_emotiv_battery_level = bat;                                                 // % current's user emotiv battery level
                    New_emotiv_contacts_quality = cq;
                }
            }
        }

        //********************************* BackWorker's Methods *********************************
        private void worker_DoWork(object sender, DoWorkEventArgs e)            // backgroung worker where the values of emotiv saved in file
        {
            // Handle any waiting events
            engine.ProcessEvents();
            
            // If the user has not yet connected, do not proceed
            if (currentUser == 0 && UserInfo.user1Active == false)
            {
                return;
            }
            
            UserInfo.userID = (int)userID;

            // Get Data from current user
            Dictionary<EdkDll.EE_DataChannel_t, double[]> data = engine.GetData((uint)currentUser);

            if (data == null)
            {
                UserInfo.data = false;
                return;
            }

            UserInfo.data = true;

            int _bufferSize = data[EdkDll.EE_DataChannel_t.TIMESTAMP].Length;

            double avAF3 = data[EdkDll.EE_DataChannel_t.AF3].Sum() / _bufferSize;
            double avF7 = data[EdkDll.EE_DataChannel_t.F7].Sum() / _bufferSize;
            double avF3 = data[EdkDll.EE_DataChannel_t.F3].Sum() / _bufferSize;
            double avFC5 = data[EdkDll.EE_DataChannel_t.FC5].Sum() / _bufferSize;
            double avT7 = data[EdkDll.EE_DataChannel_t.T7].Sum() / _bufferSize;
            double avP7 = data[EdkDll.EE_DataChannel_t.P7].Sum() / _bufferSize;
            double avO1 = data[EdkDll.EE_DataChannel_t.O1].Sum() / _bufferSize;
            double avO2 = data[EdkDll.EE_DataChannel_t.O2].Sum() / _bufferSize;
            double avP8 = data[EdkDll.EE_DataChannel_t.P8].Sum() / _bufferSize;
            double avT8 = data[EdkDll.EE_DataChannel_t.T8].Sum() / _bufferSize;
            double avFC6 = data[EdkDll.EE_DataChannel_t.FC6].Sum() / _bufferSize;
            double avF4 = data[EdkDll.EE_DataChannel_t.F4].Sum() / _bufferSize;
            double avF8 = data[EdkDll.EE_DataChannel_t.F8].Sum() / _bufferSize;
            double avAF4 = data[EdkDll.EE_DataChannel_t.AF4].Sum() / _bufferSize;
            double avGYROX = data[EdkDll.EE_DataChannel_t.GYROX].Sum() / _bufferSize;
            double avGYROY = data[EdkDll.EE_DataChannel_t.GYROY].Sum() / _bufferSize;

            TextWriter file = new StreamWriter(basePath + filenameChn, true);
            for (int i = 0; i < _bufferSize; i++)
            {
                file.WriteLine(Form2.name_of_current_media_file + delimiter + Form2.mouse_x + delimiter + Form2.mouse_y + delimiter + data[EdkDll.EE_DataChannel_t.TIMESTAMP][i].ToString() + delimiter + //data[EdkDll.EE_DataChannel_t.COUNTER][i].ToString() + delimiter +  
                    (data[EdkDll.EE_DataChannel_t.AF3][i] - avAF3).ToString() + delimiter + (data[EdkDll.EE_DataChannel_t.F7][i] - avF7).ToString() + delimiter +
                    (data[EdkDll.EE_DataChannel_t.F3][i] - avF3).ToString() + delimiter + (data[EdkDll.EE_DataChannel_t.FC5][i] - avFC5).ToString() + delimiter +
                    (data[EdkDll.EE_DataChannel_t.T7][i] - avT7).ToString() + delimiter + (data[EdkDll.EE_DataChannel_t.P7][i] - avP7).ToString() + delimiter +
                    (data[EdkDll.EE_DataChannel_t.O1][i] - avO1).ToString() + delimiter + (data[EdkDll.EE_DataChannel_t.O2][i] - avO2).ToString() + delimiter +
                    (data[EdkDll.EE_DataChannel_t.P8][i] - avP8).ToString() + delimiter + (data[EdkDll.EE_DataChannel_t.T8][i] - avT8).ToString() + delimiter +
                    (data[EdkDll.EE_DataChannel_t.FC6][i] - avFC6).ToString() + delimiter + (data[EdkDll.EE_DataChannel_t.F4][i] - avF4).ToString() + delimiter +
                    (data[EdkDll.EE_DataChannel_t.F8][i] - avF8).ToString() + delimiter + (data[EdkDll.EE_DataChannel_t.AF4][i] - avAF4).ToString()
                    );
            }
            file.Close();
        }

        public void SetOutputFilename(string basePath, string name, string date)// set output file name and date
        {
            //MessageBox.Show("output");
            this.basePath = basePath;
            delimiter = delimiter_return(Properties.Settings.Default.Delimiter);
            file_extension = Properties.Settings.Default.Extension;
            filenameChn = name + "_" + date + file_extension;

            // Delimiter file's header
            if (Properties.Settings.Default.Header) 
            {
                TextWriter file = new StreamWriter(basePath + filenameChn, true);
                file.WriteLine("File_Name" + delimiter + "Valence" + delimiter + "Arousal" + delimiter + "time_stamp" + delimiter +
                                    "AF3" + delimiter + "F7" + delimiter + "F3" + delimiter + "FC5" + delimiter + "T7" + delimiter + "P7" + delimiter + "O1" + delimiter +
                                    "O2" + delimiter + "P8" + delimiter + "T8" + delimiter + "FC6" + delimiter + "F4" + delimiter + "F8" + delimiter + "AF4");
                file.Close();
            }
        }

        //********************************* timer *********************************
        public void Start()                                                     // read emotiv value every 1/(128 kHz) = 7.8125
        {
            timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Interval = 7.8125; //7.8125
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        public void Stop()                                                      // stop timer and dispose it
        {
            timer.Enabled = false;
            timer.Dispose();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)           // if the backgroung worker==!busy call him
        {
            if (!worker.IsBusy)
                worker.RunWorkerAsync();
        }

        public void check()                                                     // check for events
        {
            New_emotiv_contacts_quality = "0,0,0,0,0,0,0,0,0,0,0,0,0,0";

            // Handle any waiting events
            engine.ProcessEvents();
        }

        //********************************* Methods *********************************
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

        //********************************* Events Handlers *********************************
        protected virtual void OnBatteryChanged(FloatEventArgs e)
        {
            EventHandler<FloatEventArgs> eh = BatteryChanged;
            if (eh != null)
                eh(this, e);
        }
        
        protected virtual void OnWireless_Signal_Changed(IntEventArgs e)
        {
            EventHandler<IntEventArgs> eh = Wireless_Signal_Changed;
            if (eh != null)
                eh(this, e);
        }
        
        protected virtual void OnConnected_Changed(BoolEventArgs e)
        {
            EventHandler<BoolEventArgs> eh = Connected_Changed;
            if (eh != null)
                eh(this, e);
        }

        protected virtual void OnContacts_quality_Changed(TextEventArgs e)
        {
            EventHandler<TextEventArgs> eh = Contacts_quality_Changed;
            if (eh != null)
                eh(this, e);
        }

    }
   

    
}