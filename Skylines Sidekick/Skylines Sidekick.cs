using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SkylinesSidekick
{
    /*  Welcome to my first Unity Mod of any kind. Also I've never used C# before. 
     *  Author: JuicyJones
     *  Other credits: Zuppi
     *  Send feedback to: juicyjones@gmail.com

     *  The Skylines City Sidekick includes the following features:
     *  Displays a clock
     *  Pauses the game on load
     *  Unlocks achievements while using mods
     *  Unlocks the Basic Roads milestone
     *  Unlocks the maximum of 25 areas for development
     *  Unlocks the Info Views Window
     *  Increases starting money to $150,000
     *  Decreases the cost of construction, maintenance by 10%
     *  Removes the cost of refunds and relocation
     *  Increases RCI demand by 5%
     *  <disabled> Increases RCI Leveling Rate by 10%
     *  Enables persistent natural resource overlay mode (thanks to Steam user Zuppi for this idea)
    */

    // Declare the name and description for users
    public class SkylinesSidekick : IUserMod
    {

        public string Name
        {
            get { return "Skylines City Sidekick"; }
        }

        public string Description
        {
            get { return "An assistant to help you with many of the difficult tasks of managing your city."; }
        }
    }

    // Turn on achievements with mods
    public class EnableAchievementsLoad : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            Singleton<SimulationManager>.instance.m_metaData.m_disableAchievements = SimulationMetaData.MetaBool.False;
        }
    }

    // Let's pause the game on load. Noooooo-brainer.
    public class PauseGameOnLoad : LoadingExtensionBase
    {
        ILoading loading;
        public override void OnCreated(ILoading loading)
        {
            this.loading = loading;
        }
        public override void OnLevelLoaded(LoadMode mode)
        {
            loading.managers.threading.simulationPaused = true;
        }
    }

    // Increase the total building area from 3x3 to 5x5
    public class MaxAreaForGood : IAreasExtension
    {
        public void OnCreated(IAreas areas)
        {
            areas.maxAreaCount = 25;
        }
        public void OnReleased()
        {
        }
        public bool OnCanUnlockArea(int x, int z, bool originalResult)
        {
            return originalResult;
        }
        public int OnGetAreaPrice(uint ore, uint oil, uint forest, uint fertility, uint water, bool road, bool train, bool ship, bool plane, float landFlatness, int originalPrice)
        {
            return originalPrice;
        }
        public void OnUnlockArea(int x, int z)
        {
        }
    }

    // Unlock the basic roads milestone
    public class UnlockBasicRoadsMilestones : MilestonesExtensionBase
    {
        public override void OnRefreshMilestones()
        {
            milestonesManager.UnlockMilestone("Basic Road Created");
        }
    }

    // Set our starting money
    public class StartMoneyLoadingExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.NewGame)
            {
                try
                {
                    var type = typeof(EconomyManager);
                    var cashAmountField = type.GetField("m_cashAmount", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    cashAmountField.SetValue(EconomyManager.instance, 15000000);
                }
                catch (Exception)
                {
                    //
                }
            }
            base.OnLevelLoaded(mode);
        }
    }

    // A clock. If this is bad, blame my C#, I got it all from Stackoverflow.
    public class LoadingExtension : LoadingExtensionBase
    {
        UITextField time;
        System.Timers.Timer t;
        public override void OnLevelLoaded(LoadMode mode)
        {
            var uiView = GameObject.FindObjectOfType<UIView>();
            if (uiView == null) return;
            var textObject = new GameObject("System Clock", typeof(UITextField));
            textObject.transform.parent = uiView.transform;
            time = textObject.GetComponent<UITextField>();
            time.text = System.DateTime.Now.ToString("hh:mm");

            // Initialize the text fields or die confused
            time.transformPosition = new Vector3(1.50f, -.945f);
            t = new System.Timers.Timer();
            t.AutoReset = false;
            t.Elapsed += new System.Timers.ElapsedEventHandler(setText);
            t.Interval = 1000;
            time.width = 100;
            time.height = 30;
            t.Start();
        }

        private void setText(object sender, System.Timers.ElapsedEventArgs e)
        {
            time.text = System.DateTime.Now.ToString("hh:mm");
            t.Start();
        }
    }

    // Set construction, maintenance, relocation and refund amounts here
    public class EasyModeEconomy : EconomyExtensionBase
    {

        public override int OnGetConstructionCost(int originalConstructionCost, Service service, SubService subService, Level level)
        {
            return originalConstructionCost * 9 / 10;
        }

        public override int OnGetMaintenanceCost(int originalMaintenanceCost, Service service, SubService subService, Level level)
        {
            return originalMaintenanceCost * 9 / 10;
        }

        public override int OnGetRelocationCost(int constructionCost, int relocationCost, Service service, SubService subService, Level level)
        {
            return 0;
        }

        public override int OnGetRefundAmount(int constructionCost, int refundAmount, Service service, SubService subService, Level level)
        {
            return constructionCost;
        }

    }

    // Set demand rates here
    public class EasyModeDemand : DemandExtensionBase
    {

        public override int OnCalculateResidentialDemand(int originalDemand)
        {
            return originalDemand + 5;
        }

        public override int OnCalculateCommercialDemand(int originalDemand)
        {
            return originalDemand + 5;
        }

        public override int OnCalculateWorkplaceDemand(int originalDemand)
        {
            return originalDemand + 5;
        }
        /*  This will artifically max all demand, use sparingly
                public override int OnUpdateDemand(int lastDemand, int nextDemand, int targetDemand){
        
                    return base.OnUpdateDemand(lastDemand,0x7fffffff,0x7fffffff);
                }
        */
    }
    /*
        // Set RCI Zone leveling rates here
        public class EasyModeLevelUp : LevelUpExtensionBase
        {

            public override ResidentialLevelUp OnCalculateResidentialLevelUp(ResidentialLevelUp levelUp, int averageEducation, int landValue, ushort buildingID, Service service, SubService subService, Level currentLevel)
            {
                if (levelUp.landValueProgress != 0)
                {
                    Level targetLevel;

                    if (landValue < 15)
                    {
                        targetLevel = Level.Level1;
                        levelUp.landValueProgress = 1 + (landValue * 15 - 2) / 15;
                    }
                    else if (landValue < 35)
                    {
                        targetLevel = Level.Level2;
                        levelUp.landValueProgress = 1 + ((landValue - 0) * 20 + 10) / 20;
                    }
                    else if (landValue < 60)
                    {
                        targetLevel = Level.Level3;
                        levelUp.landValueProgress = 1 + ((landValue - 0) * 25 + 12) / 25;
                    }
                    else if (landValue < 85)
                    {
                        targetLevel = Level.Level4;
                        levelUp.landValueProgress = 1 + ((landValue - 0) * 25 + 12) / 25;
                    }
                    else
                    {
                        targetLevel = Level.Level5;
                        levelUp.landValueProgress = 1;
                    }

                    if (targetLevel < currentLevel)
                    {
                        levelUp.landValueProgress = 1;
                    }
                    else if (targetLevel > currentLevel)
                    {
                        levelUp.landValueProgress = 15;
                    }

                    if (targetLevel < levelUp.targetLevel)
                    {
                        levelUp.targetLevel = targetLevel;
                    }
                }

                levelUp.landValueTooLow = false;
                if (currentLevel == Level.Level2)
                {
                    if (landValue == 0) levelUp.landValueTooLow = true;
                }
                else if (currentLevel == Level.Level3)
                {
                    if (landValue < 11) levelUp.landValueTooLow = true;
                }
                else if (currentLevel == Level.Level4)
                {
                    if (landValue < 26) levelUp.landValueTooLow = true;
                }
                else if (currentLevel == Level.Level5)
                {
                    if (landValue < 41) levelUp.landValueTooLow = true;
                }

                return levelUp;
            }

            public override CommercialLevelUp OnCalculateCommercialLevelUp(CommercialLevelUp levelUp, int averageWealth, int landValue, ushort buildingID, Service service, SubService subService, Level currentLevel)
            {
                if (levelUp.landValueProgress != 0)
                {
                    Level targetLevel;

                    if (landValue < 30)
                    {
                        targetLevel = Level.Level1;
                        levelUp.landValueProgress = 1 + (landValue * 20 + 15) / 30;
                    }
                    else if (landValue < 60)
                    {
                        targetLevel = Level.Level2;
                        levelUp.landValueProgress = 1 + ((landValue - 0) * 15 + 15) / 30;
                    }
                    else
                    {
                        targetLevel = Level.Level5;
                        levelUp.landValueProgress = 1;
                    }

                    if (targetLevel < currentLevel)
                    {
                        levelUp.landValueProgress = 1;
                    }
                    else if (targetLevel > currentLevel)
                    {
                        levelUp.landValueProgress = 15;
                    }

                    if (targetLevel < levelUp.targetLevel)
                    {
                        levelUp.targetLevel = targetLevel;
                    }
                }

                levelUp.landValueTooLow = false;
                if (currentLevel == Level.Level2)
                {
                    if (landValue < 10) levelUp.landValueTooLow = true;
                }
                else if (currentLevel == Level.Level3)
                {
                    if (landValue < 20) levelUp.landValueTooLow = true;
                }

                return levelUp;
            }

            public override IndustrialLevelUp OnCalculateIndustrialLevelUp(IndustrialLevelUp levelUp, int averageEducation, int serviceScore, ushort buildingID, Service service, SubService subService, Level currentLevel)
            {
                if (levelUp.serviceProgress != 0)
                {
                    Level targetLevel;

                    if (serviceScore < 40)
                    {
                        targetLevel = Level.Level1;
                        levelUp.serviceProgress = 1 + (serviceScore * 20 + 20) / 40;
                    }
                    else if (serviceScore < 80)
                    {
                        targetLevel = Level.Level2;
                        levelUp.serviceProgress = 1 + ((serviceScore - 0) * 15 + 20) / 40;
                    }
                    else
                    {
                        targetLevel = Level.Level5;
                        levelUp.serviceProgress = 1;
                    }

                    if (targetLevel < currentLevel)
                    {
                        levelUp.serviceProgress = 1;
                    }
                    else if (targetLevel > currentLevel)
                    {
                        levelUp.serviceProgress = 15;
                    }

                    if (targetLevel < levelUp.targetLevel)
                    {
                        levelUp.targetLevel = targetLevel;
                    }
                }

                levelUp.tooFewServices = false;
                if (currentLevel == Level.Level2)
                {
                    if (serviceScore < 15) levelUp.tooFewServices = true;
                }
                else if (currentLevel == Level.Level3)
                {
                    if (serviceScore < 30) levelUp.tooFewServices = true;
                }

                return levelUp;
            }

            public override OfficeLevelUp OnCalculateOfficeLevelUp(OfficeLevelUp levelUp, int averageEducation, int serviceScore, ushort buildingID, Service service, SubService subService, Level currentLevel)
            {
                if (levelUp.serviceProgress != 0)
                {
                    Level targetLevel;

                    if (serviceScore < 55)
                    {
                        targetLevel = Level.Level1;
                        levelUp.serviceProgress = 1 + (serviceScore * 25 + 27) / 55;
                    }
                    else if (serviceScore < 110)
                    {
                        targetLevel = Level.Level2;
                        levelUp.serviceProgress = 1 + ((serviceScore - 0) * 15 + 27) / 55;
                    }
                    else
                    {
                        targetLevel = Level.Level5;
                        levelUp.serviceProgress = 1;
                    }

                    if (targetLevel < currentLevel)
                    {
                        levelUp.serviceProgress = 1;
                    }
                    else if (targetLevel > currentLevel)
                    {
                        levelUp.serviceProgress = 15;
                    }

                    if (targetLevel < levelUp.targetLevel)
                    {
                        levelUp.targetLevel = targetLevel;
                    }
                }

                levelUp.tooFewServices = false;
                if (currentLevel == Level.Level2)
                {
                    if (serviceScore < 35) levelUp.tooFewServices = true;
                }
                else if (currentLevel == Level.Level3)
                {
                    if (serviceScore < 80) levelUp.tooFewServices = true;
                }

                return levelUp;
            }

        }
    */

    // Enable the Info View Bar
    public class InfoViewEnablerer : LoadingExtensionBase
    {
        ColossalFramework.UI.UIButton[] infoviewButtons;
        ColossalFramework.UI.UIButton menuButton;

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode.Equals(LoadMode.NewGame) || mode.Equals(LoadMode.LoadGame))
            {
                infoviewButtons = GameObject.Find("InfoViewsPanel").transform.FindChild("Container").gameObject.GetComponentsInChildren<ColossalFramework.UI.UIButton>();

            }
            menuButton = GameObject.Find("InfoMenu").transform.FindChild("Info").GetComponent<ColossalFramework.UI.UIButton>();
            menuButton.eventClick += ButtonClick;
        }

        private void ButtonClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            foreach (ColossalFramework.UI.UIButton button in infoviewButtons)
            {
                button.isEnabled = true;
            }
        }
    }

    // Enable persistent natural resources overlay (thanks Zuppi!)
    public class PersistentResourceView : ThreadingExtensionBase
    {
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (PersistentResourceViewLoader.toggleOn)
            {
                if (!Singleton<InfoManager>.instance.CurrentMode.Equals(InfoManager.InfoMode.NaturalResources))
                {
                    if (Singleton<InfoManager>.instance.CurrentMode.Equals(InfoManager.InfoMode.None))
                    {
                        Singleton<InfoManager>.instance.SetCurrentMode(InfoManager.InfoMode.NaturalResources, InfoManager.SubInfoMode.Default);
                    }
                    else
                    {
                        PersistentResourceViewLoader.toggleOn = false;
                    }
                }
            }
        }
    }

    // And a loader for the natural resources overlay
    public class PersistentResourceViewLoader : LoadingExtensionBase
    {
        public static bool toggleOn;
        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode.Equals(LoadMode.NewGame) || mode.Equals(LoadMode.LoadGame))
            {
                toggleOn = false;
                ColossalFramework.UI.UIButton resourceButton = GameObject.Find("InfoViewsPanel").transform.FindChild("Container").FindChild("Resources").gameObject.GetComponent<ColossalFramework.UI.UIButton>();
                resourceButton.eventClick += ButtonClick;
            }
        }
        private void ButtonClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            toggleMode();
        }
        private void toggleMode()
        {
            toggleOn = !toggleOn;
        }
    }
}
