using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StimuliXpert
{
    class UserInfo
    {
        public static int userID;           // userID is used to uniquely identify a user's headset
        public static bool data;
        public static int userID1;          // userID is used to uniquely identify first user headset
        public static int userID2;          // userID is used to uniquely identify second user headset
        public static bool user1Active;     // first user headset is active
        public static bool user2Active;     // second user headset is active

        public static void Init()           // Initialize UserInfo
        {       
            userID = 0;             // 0 = no user 
            userID1 = 0;            // 0 = no user
            userID2 = 0;            // 0 = no user
            data = false;           //

            user1Active = false;    // user1 = no active
        }
    }
}
