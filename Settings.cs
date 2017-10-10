using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToastyCallouts
{
    class Settings
    {
        // TODO: Serialization and all that good stuff

        // Keys
        public static Keys _ConversationProgressionKey { get { return Keys.T; }}
        public static Keys _EndCalloutKey { get { return Keys.End; } }

        public static String _UnitNum { get { return "1K-21"; } }
    }
}
