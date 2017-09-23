using LSPD_First_Response.Mod.API;
using Rage;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System.Windows.Forms;
using ToastyCallouts.Callouts;

namespace ToastyCallouts
{
    class RNUIMenu
    {
        /*private static MenuPool _menuPool;

        private static UIMenu _mainMenu;
        private static UIMenu _vehicleSelectorMenu, _forceCalloutMenu;

        private static UIMenuItem _navigateToVehicleSelector, _navigateToForceCallout, _pursuitInProgress, _confirmItem;
        private static UIMenuListItem _modelListItem, _directionItem;
        private static UIMenuCheckboxItem _invincibleCheckboxItem;*/

        private static MenuPool _menuPool;
        private static UIMenu _mainMenu, _forceCalloutMenu;
        private static UIMenuItem _navigateToForceCalloutMenu, _pursuitInProgress, _test;

        public static void Main()
        {
            GameFiber.StartNew(delegate
            {
                _menuPool = new MenuPool();

                _mainMenu = new UIMenu("Toasty Callouts Menu", "Use the options within this menu to help manage calls.");
                _menuPool.Add(_mainMenu);

                _forceCalloutMenu = new UIMenu("Force Callout", "Choose a callout to force from dispatch.");
                _menuPool.Add(_forceCalloutMenu);

                _navigateToForceCalloutMenu = new UIMenuItem("Force Callout", "Choose a callout to force from dispatch.");
                _mainMenu.AddItem(_navigateToForceCalloutMenu);

                _pursuitInProgress = new UIMenuItem("Pursuit in Progress", "Force the callout named Pursuit in Progress.");
                _forceCalloutMenu.AddItem(_pursuitInProgress);

                _mainMenu.BindMenuToItem(_forceCalloutMenu, _navigateToForceCalloutMenu);

                _forceCalloutMenu.ParentMenu = _mainMenu;

                _mainMenu.RefreshIndex();
                _forceCalloutMenu.RefreshIndex();

                _mainMenu.OnItemSelect += OnItemSelect;

                _mainMenu.MouseControlsEnabled = false;
                _mainMenu.AllowCameraMovement = true;

                _forceCalloutMenu.MouseControlsEnabled = false;
                _forceCalloutMenu.AllowCameraMovement = true;

                MainLogic();
                GameFiber.Hibernate();

                /*_menuPool = new MenuPool();

                _mainMenu = new UIMenu("Toasty Callouts Menu", "Use the options within this menu to help manage calls.");
                _menuPool.Add(_mainMenu);

                _vehicleSelectorMenu = new UIMenu("Vehicle Selector Menu", "This is a test; select your vehicle.");
                _menuPool.Add(_vehicleSelectorMenu);
                _vehicleSelectorMenu.SetMenuWidthOffset(35);

                _navigateToVehicleSelector = new UIMenuItem("Vehicle Selector Menuuu");
                _mainMenu.AddItem(_navigateToVehicleSelector);

                _mainMenu.BindMenuToItem(_vehicleSelectorMenu, _navigateToVehicleSelector);
                _vehicleSelectorMenu.ParentMenu = _mainMenu;

                List<dynamic> listWithModels = new List<dynamic>()
                {
                    "POLICE",
                    "POLICE2",
                    "POLICE3",
                    "POLICE4"
                };

                _modelListItem = new UIMenuListItem("Model", "test", listWithModels);
                _vehicleSelectorMenu.AddItem(_modelListItem);

                _invincibleCheckboxItem = new UIMenuCheckboxItem("Invincible", false, "Makes your vehicle invincible.");
                _mainMenu.AddItem(_invincibleCheckboxItem);

                List<dynamic> directions = new List<dynamic>()
                {
                    "Front", "Back"
                };

                _directionItem = new UIMenuListItem("Direction", "Choose a direction to spawn the vehicle in.", directions);
                _mainMenu.AddItem(_directionItem);

                _confirmItem = new UIMenuItem("Confirm", "confirm your changes.");
                _mainMenu.AddItem(_confirmItem);

                _mainMenu.RefreshIndex();
                _vehicleSelectorMenu.RefreshIndex();

                _mainMenu.OnItemSelect += OnItemSelect;

                _mainMenu.MouseControlsEnabled = false;
                _mainMenu.AllowCameraMovement = true;

                _vehicleSelectorMenu.MouseControlsEnabled = false;
                _vehicleSelectorMenu.AllowCameraMovement = true;

                MainLogic();
                GameFiber.Hibernate();*/
            });
        }

        public static void OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (sender == _mainMenu)
            {
                if (selectedItem == _pursuitInProgress)
                {
                    Functions.RegisterCallout(typeof(PursuitinProgress));
                }
            }

            /*if (sender == _mainMenu)
            {
                if (selectedItem == _confirmItem)
                {
                    IDisplayItem modelName = _modelListItem.SelectedItem;
                    Model vehicleModel = new Model(modelName.ToString());

                    bool invincible = _invincibleCheckboxItem.Checked;

                    DisplayItemsCollection directionName = _directionItem.Collection;
                    Vector3 position;

                    if (directionName.ToString() == "Front")
                    {
                        position = Game.LocalPlayer.Character.GetOffsetPositionFront(10f);
                    }
                    else
                    {
                        position = Game.LocalPlayer.Character.GetOffsetPositionFront(-10f);
                    }

                    Vehicle newVehicle = new Vehicle(vehicleModel, position, Game.LocalPlayer.Character.Heading)
                    {
                        IsPersistent = true,
                        IsInvincible = invincible
                    };

                    Game.LocalPlayer.Character.WarpIntoVehicle(newVehicle, -1);
                }
            }*/
        }

        public static void MainLogic()
        {
            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    GameFiber.Yield();

                    if (Game.IsKeyDown(Keys.F6))
                    {
                        _mainMenu.Visible = !_mainMenu.Visible;
                    }

                    _menuPool.ProcessMenus();
                }
            });
        }
    }
}
