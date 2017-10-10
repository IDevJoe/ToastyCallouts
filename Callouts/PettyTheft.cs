using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System.Drawing;
using System.Windows.Forms;
using ToastyCallouts.Utilities;

namespace ToastyCallouts.Callouts
{
    [CalloutInfo("TC_PettyTheft", CalloutProbability.Medium)]

    class PettyTheft : Callout
    {
        private Vector3 _sP;
        private Ped _victimPed, _suspectPed;
        private Blip _victimBlip, _suspectBlip, _moneyBlip;
        private LHandle _pursuit;
        private Rage.Object _moneyObject;
        private Checkpoint _moneyCheckpoint;
        private Vector3 _rightHandHoldingCashPileVector3 = new Vector3(0.08f, 0.02f, -0.04f);
        private Rotator _rightHandHoldingCashPileRotator = new Rotator(-86f, -74.66f, -4f);
        private Conversations.Conversation _conversation1, _conversation2;

        private enum Progression
        {
            START_CONVERSATION,
            LINE_3_CHOICE,
            CHOICE_1,
            CHOICE_2,
            LOCATE_SUSPECT,
            FLEE,
            SUSPECT_APPREHENDED,
            SEARCH_SUSPECT,
            LOCATE_MONEY,
            RETURN_MONEY_CONVERSATION_START,
            RETURN_MONEY_ANIM
        };

        private Progression _progressionState;
        private int _cashAmount = MathHelper.GetRandomInteger(20, 101);
        private bool _foundMoneyOnSuspect;
        private bool _convoStarted = false;

        public override bool OnBeforeCalloutDisplayed()
        {
            _sP = Spawnpoints.GetGoodSpawnpoint();

            ShowCalloutAreaBlipBeforeAccepting(_sP, 100f);
            CalloutMessage = "Petty Theft";
            CalloutPosition = _sP;

            Functions.PlayScannerAudioUsingPosition("", _sP); // TODO: Scanner stuff

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutDisplayed()
        {
            _victimPed = new Ped(_sP)
            {
                IsPersistent = true,
                BlockPermanentEvents = true
            };

            _suspectPed = new Ped(_victimPed.Position.Around2D(30f))
            {
                IsPersistent = true,
                BlockPermanentEvents = true
            };

            if (_victimPed && _suspectPed)
            {
                _victimPed.Tasks.StandStill(int.MaxValue);
                _suspectPed.Tasks.ReactAndFlee(_victimPed);
            }
            else
            {
                Cleaning.CatchInvalidObjects(new Entity[] { _victimPed, _suspectPed });
                End();
            }

            base.OnCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            _victimBlip = new Blip(_victimPed)
            {
                Name = "VICTIM",
                IsRouteEnabled = true,
                RouteColor = Color.Yellow
            };
            _victimBlip.SetStandardColor(CalloutStandardization.BlipTypes.CIVILIANS);
            _victimBlip.SetBlipScalePed();
            _victimBlip.Flash(10, 10);

            _progressionState = Progression.START_CONVERSATION;
            // Util.SpectateCameraToggler(_victimPed, _suspectPed); // Is this really needed?
            Game.DisplayHelp(string.Format("Press {0} to end the callout at any time", FriendlyKeys.GetFriendlyName(Settings._EndCalloutKey)));
            return base.OnCalloutAccepted();
        }

        public void StateStartConversation()
        {
            _victimPed.FaceEntity(Main.Player);
            if (_convoStarted) return;
            _convoStarted = true;
            Conversations.ConversationLine[] conversation1Lines =
            {
                new Conversations.ConversationLine()
                { //1
                    _pedName = Conversations.ConversationLine.PedName.VICTIM,
                    _lineVariants = new string[]
                    {
                        "Officer, please help me!",
                        "Please help, my money was stolen!",
                        "Oh my god.. why me?"
                    }
                },
                new Conversations.ConversationLine()
                { //2
                    _pedName = Conversations.ConversationLine.PedName.LOCALPLAYER,
                    _lineVariants = new string[]
                    {
                        "What happened?",
                        "It's going to be alright, just explain to me what happened."
                    }
                },
                new Conversations.ConversationLine()
                { //3
                    _pedName = Conversations.ConversationLine.PedName.VICTIM,
                    _lineVariants = new string[]
                    {
                        "Someone approached me as I was counting my money and grabbed it out of my hands!",
                        "They just came up to me and snatched my cash, they're criminals!",
                        "A thief came up to me and grabbed the money that I had in my hands.",
                        "They grabbed me by the neck and told me to get all the money I had out of my purse, I was so scared!",
                        "It all happened so fast, they just took my money and ran!"
                    }
                },
                new Conversations.ConversationLine()
                { //4
                    _pedName  = Conversations.ConversationLine.PedName.LOCALPLAYER,
                    _lineVariants = new string[]
                    {
                        "Alright, so they took your cash.. do you know how much exactly they took?"
                    }
                },
                new Conversations.ConversationLine() //1
                { //5
                    _pedName = Conversations.ConversationLine.PedName.VICTIM,
                    _lineVariants = new string[]
                    {
                        string.Format("Yeah, I believe it was {0}.", _cashAmount),
                        string.Format("Yeah, it was around {0}.", _cashAmount),
                        string.Format("I think it was about {0}.", _cashAmount),
                        string.Format("Yeah, it should be about {0}.", _cashAmount),
                    }
                },
                new Conversations.ConversationLine()
                {
                    _pedName = Conversations.ConversationLine.PedName.LOCALPLAYER,
                    _lineVariants = new string[]
                    {
                        "Do you remember what the suspect looked like?"
                    }
                },
                new Conversations.ConversationLine() //2
                { //6
                    _pedName  = Conversations.ConversationLine.PedName.VICTIM,
                    _lineVariants = new string[]
                    {
                        "All I can remember is that he was a white male.",
                        "I think he was a white male, but that's all I remember.",
                        "It happened too fast, all I can remember is that he was a white male."
                    }
                },
                new Conversations.ConversationLine() //1 --> 2
                { //7
                    _pedName = Conversations.ConversationLine.PedName.LOCALPLAYER,
                    _lineVariants = new string[]
                    {
                        "Do you remember what the suspect looked like?" //2
                    }
                },
                new Conversations.ConversationLine() //2 --> 1
                { //8
                    _pedName = Conversations.ConversationLine.PedName.LOCALPLAYER,
                    _lineVariants = new string[]
                    {
                        "Alright, do you remember about how much cash they took from you?"
                    }
                },
                new Conversations.ConversationLine()
                { //9
                    _pedName = Conversations.ConversationLine.PedName.LOCALPLAYER,
                    _lineVariants = new string[]
                    {
                        "Do you happen to recall which direction he ran off in?"
                    }
                },
                new Conversations.ConversationLine()
                { //10
                    _pedName = Conversations.ConversationLine.PedName.VICTIM,
                    _lineVariants = new string[]
                    {
                        "Yes I do, he ran that way.",
                        "Yeah I think he ran that way.",
                        "He probably went that way."
                    },
                    _afterLine = delegate
                    {
                        Util.Log("Reached, CurrentLine == 9 --> 10.", 0);
                        _victimPed.FaceEntity(_suspectPed);
                        _victimPed.PointAnimation(5000);
                    }
                },
                new Conversations.ConversationLine()
                { //11
                    _pedName = Conversations.ConversationLine.PedName.LOCALPLAYER,
                    _lineVariants = new string[]
                    {
                        "Alright thank you for your time, we'll let you know if we catch the suspect.",
                        "Thanks for your time, we'll see if we can find the suspect."
                    },
                    _afterLine = delegate
                    {
                        Util.Log("Reached, CurrentLine >= 11.", 0);
                        Util.BlipUpdates(_suspectBlip, CalloutStandardization.BlipTypes.ENEMY, _suspectPed, "SUSPECT");
                        if (_victimBlip) _victimBlip.Delete();

                        _progressionState = Progression.FLEE;
                    }
                }
            };

            _conversation1 = new Conversations.Conversation(conversation1Lines);
            _conversation1.Start();
            _progressionState = Progression.LINE_3_CHOICE;
        }

        public override void Process()
        {
            if (Game.IsKeyDown(Keys.End)) End();

            if (_progressionState == Progression.START_CONVERSATION && Main.Player.TravelDistanceTo(_victimPed) <= 15f)
            {
                StateStartConversation();
            }

            if (_progressionState == Progression.FLEE && Util.IsEntityVisible(_suspectPed))
            {
                _moneyObject = new Object("prop_cash_pile_01", _suspectPed.Position)
                {
                    IsPersistent = true,
                    IsVisible = true
                };

                _moneyObject.PlaceObjectOnGroundProperly();
                if (_suspectBlip) _suspectBlip.Delete();

                _pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(_pursuit, _suspectPed);
                Functions.SetPursuitIsActiveForPlayer(_pursuit, true);

                _progressionState = Progression.SUSPECT_APPREHENDED;
            }

            if (_progressionState == Progression.SUSPECT_APPREHENDED && _suspectPed && !Functions.IsPursuitStillRunning(_pursuit))
            {
                _foundMoneyOnSuspect = MathHelper.GetRandomInteger(2) == 0;
                Game.DisplayHelp(string.Format("Press {0} to search the suspect for the victim's money.", FriendlyKeys.GetFriendlyName(Keys.Y)));
                SearchSuspectForMoney();

                _progressionState = Progression.SEARCH_SUSPECT;
            }

            base.Process();
        }

        public override void End()
        {
            Cleaning.EndAndClean(new Entity[] { _victimPed, _suspectPed, _moneyObject }, new Blip[] { _victimBlip, _suspectBlip, _moneyBlip }, new LHandle[] { _pursuit }, new Checkpoint[] { _moneyCheckpoint });

            base.End();
        }

        private void SearchSuspectForMoney()
        {
            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    GameFiber.Yield();

                    if (Game.IsKeyDown(Keys.Y) && Main.Player.DistanceTo2D(_suspectPed) <= 10f)
                    {
                        _suspectPed.IsPositionFrozen = true;
                        Main.Player.Tasks.GoStraightToPosition(_suspectPed.GetOffsetPositionFront(2f), 2f, _suspectPed.Heading / -1, 1f, 10000).WaitForCompletion(10000);
                        Game.LocalPlayer.Character.Tasks.PlayAnimation("missexile3", "ex03_dingy_search_case_base_michael", 4f, AnimationFlags.Loop).WaitForCompletion(3000);

                        Game.LocalPlayer.Character.Tasks.Clear();
                        _suspectPed.IsPositionFrozen = false;

                        if (!_foundMoneyOnSuspect)
                        {
                            _moneyObject.IsVisible = true;
                            _moneyBlip = new Blip(_moneyObject)
                            {
                                Alpha = 0.40f,
                                Scale = 50f,
                                IsRouteEnabled = true,
                                Name = "VICTIM'S MONEY"
                            };

                            _moneyBlip.SetStandardColor(CalloutStandardization.BlipTypes.OTHER);
                            _moneyBlip.Flash(10, 10);

                            _moneyCheckpoint = new Checkpoint(_moneyObject.Position, Color.Purple, 1f, 1f);
                            _progressionState = Progression.LOCATE_MONEY;
                        }
                        else
                        {
                            Game.DisplayNotification(string.Format("You found ~p~${0}~s~ on the suspect.", _cashAmount));
                        }
                    }

                    if (_progressionState == Progression.LOCATE_MONEY && Main.Player.DistanceTo2D(_moneyObject) <= 10f)
                    {
                        Game.DisplayHelp(string.Format("Press {0} to pickup the victim's money.", FriendlyKeys.GetFriendlyName(Keys.Y)), true);

                        if (Game.IsKeyDown(Keys.Y))
                        {
                            int rightHandBoneIndex = NativeFunction.Natives.GET_PED_BONE_INDEX<int>(Game.LocalPlayer.Character, 28422);
                            Main.Player.Tasks.GoStraightToPosition(_moneyObject.GetOffsetPositionFront(2f), 2f, _moneyObject.Heading, 1f, 10000).WaitForCompletion(10000);

                            Game.LocalPlayer.Character.Tasks.PlayAnimation("random@mugging4", "pickup_low", 4f, AnimationFlags.None);
                            GameFiber.Sleep(500);
                            _moneyObject.AttachTo(Main.Player, rightHandBoneIndex, _rightHandHoldingCashPileVector3, _rightHandHoldingCashPileRotator);
                            GameFiber.Sleep(1000);
                            NativeFunction.Natives.PLAY_SOUND_FRONTEND(-1, "PICK_UP", "HUD_FRONTEND_DEFAULT_SOUNDSET", 1);

                            _moneyObject.IsVisible = false;
                            Game.DisplayNotification(string.Format("You picked up ~p~${0}~s~.", _cashAmount));
                        }

                        _victimBlip = new Blip(_victimPed)
                        {
                            Name = "VICTIM",
                            IsRouteEnabled = true,
                            RouteColor = Color.Yellow
                        };
                        _victimBlip.SetStandardColor(CalloutStandardization.BlipTypes.CIVILIANS);
                        _victimBlip.SetBlipScalePed();
                        _victimBlip.Flash(10, 10);

                        _progressionState = Progression.RETURN_MONEY_CONVERSATION_START;
                    }

                    if (_progressionState == Progression.RETURN_MONEY_CONVERSATION_START && Main.Player.DistanceTo2D(_victimPed) <= 10f)
                    {
                        Conversations.ConversationLine[] conversation2Lines =
                        {
                            new Conversations.ConversationLine()
                            { //1
                                _pedName = Conversations.ConversationLine.PedName.LOCALPLAYER,
                                _lineVariants = new string[]
                                {
                                    "Alright, can you reiterate how much money the suspect took from you?",
                                    "We did get your money back, but can you just repeat how much money they took from you?"
                                }
                            },
                            new Conversations.ConversationLine()
                            { //2
                                _pedName = Conversations.ConversationLine.PedName.VICTIM,
                                _lineVariants = new string[]
                                {
                                    string.Format("Yeah I'm pretty sure they took about ${0}.", _cashAmount),
                                    string.Format("They took around ${0}.", _cashAmount),
                                    string.Format("It was probably around ${0}.", _cashAmount)
                                }
                            },
                            new Conversations.ConversationLine()
                            { //3
                                _pedName = Conversations.ConversationLine.PedName.LOCALPLAYER,
                                _lineVariants = new string[]
                                {
                                    "Alright yeah that is how much we found on him, here you go.",
                                    "Yep that is how much he had on him, here you go."
                                },
                                _afterLine = delegate
                                {
                                    Main.Player.Tasks.GoStraightToPosition(_victimPed.GetOffsetPositionFront(2f), 2f, _victimPed.Heading / -1, 1f, 10000).WaitForCompletion(10000);
                                    _moneyObject.IsVisible = true;
                                    Main.Player.FaceEntity(_victimPed);

                                    Main.Player.Tasks.PlayAnimation("random@mugging4", "return_wallet_positive_b_player", 4f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask).WaitForCompletion(2000);
                                    _moneyObject.IsVisible = false;
                                    Main.Player.Tasks.Clear();
                                    End();
                                }
                            }
                        };

                        _conversation2 = new Conversations.Conversation(conversation2Lines);
                        _conversation2.Start();
                        _progressionState = Progression.RETURN_MONEY_ANIM;
                    }
                }
            });
        }
    }
}
