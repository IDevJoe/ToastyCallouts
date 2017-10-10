using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LSPD_First_Response.Mod.API;
using Rage;
using ToastyCallouts.Utilities;

namespace ToastyCallouts.Utilities
{
    public class Conversations
    {
        public struct ConversationLine
        {
            public enum PedName
            {
                SUSPECT,
                LOCALPLAYER,
                OFFICER,
                FIREFIGHTER,
                PARAMEDICS,
                ANIMALCONTROL,
                VICTIM,
                WITNESS
            };
            public PedName _pedName;

            public string[] _lineVariants; //if ShowVariants == true - contains options, otherwise a set of lines to get a random one and display
            public int _subtitleTimeout;
            public Action _afterLine;
        }

        public class Conversation
        {
            public int CurrentLine { get; set; }
            public bool IsFinished { get; set; }

            private ConversationLine[] _lines;
            private GameFiber _fiber;

            public Conversation(ConversationLine[] lines)
            {
                this._lines = lines;
            }

            public void Start()
            {
                Game.DisplayHelp(string.Format("You can press {0} to progress through the conversation", FriendlyKeys.GetFriendlyName(Settings._ConversationProgressionKey)));
                Timing.Tick += Process;
            }

            private String getPedName(ConversationLine.PedName _name)
            {
                String _pedsNameAsString;
                switch (_name)
                {
                    case ConversationLine.PedName.SUSPECT:
                        _pedsNameAsString = "~r~Suspect";
                        break;
                    case ConversationLine.PedName.LOCALPLAYER:
                        _pedsNameAsString = string.Format("~b~Officer {0}", Settings._UnitNum); // TODO: Use unit number from config
                        break;
                    case ConversationLine.PedName.OFFICER:
                        _pedsNameAsString = "~b~Other Officer";
                        break;
                    case ConversationLine.PedName.FIREFIGHTER:
                        _pedsNameAsString = "~g~Firefighter";
                        break;
                    case ConversationLine.PedName.PARAMEDICS:
                        _pedsNameAsString = "~g~Paramedics";
                        break;
                    case ConversationLine.PedName.ANIMALCONTROL:
                        _pedsNameAsString = "~g~Animal Control";
                        break;
                    case ConversationLine.PedName.VICTIM:
                        _pedsNameAsString = "~o~Victim";
                        break;
                    case ConversationLine.PedName.WITNESS:
                        _pedsNameAsString = "~o~Witness";
                        break;
                    default:
                        _pedsNameAsString = "~y~Unkown";
                        break;
                }
                return _pedsNameAsString;
            }

            private void Process()
            {
                if ((CurrentLine == _lines.Length) || (IsFinished) || (!Functions.IsCalloutRunning()))
                {
                    Util.Log("Conversation has finished.", 0);
                    IsFinished = true;
                    Timing.Tick -= Process;
                    return;
                }

                var current = _lines[CurrentLine];

                string lineToDisplay;

                    
                lineToDisplay = MathHelper.Choose<string>(current._lineVariants);

                if (!Game.IsKeyDown(Settings._ConversationProgressionKey)) return;

                if (current._subtitleTimeout == 0)
                {
                    current._subtitleTimeout = int.MaxValue;
                }

                String _pedsNameAsString = getPedName(current._pedName);

                Game.DisplaySubtitle("~h~" + _pedsNameAsString + ": " + lineToDisplay, current._subtitleTimeout);
                current._afterLine?.Invoke();
                CurrentLine++;
            }
        }
    }
}
