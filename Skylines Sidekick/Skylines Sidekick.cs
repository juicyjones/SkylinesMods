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
     *  Other credits: Pedro Takeshi, Sadler, Zuppi
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
     *  Adjustable Demand sliders (thanks to Steam user Pedro Takeshi)
     *  Automatically Bulldozes abandoned or burned buildings, with an in-game toggle for each (thanks to Steam user Sadler)
     *  Enables persistent natural resource overlay mode (thanks to Steam user Zuppi)
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
            time.text = System.DateTime.Now.ToString("hh:mm") ;

            // Initialize the text fields or die confused
            time.transformPosition = new Vector3(1.50f,-.945f);
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
    // Setup and deploy automatic bulldozers (thanks Sadler!)
    public class AutoBulldozeThread : ThreadingExtensionBase
    {
        public static AudioGroup nullAudioGroup;
        bool m_initialized = false;

        public bool init()
        {
            if (m_initialized == true) return true;
            UIComponent bullBar = UIView.Find("BulldozerBar");
            if (bullBar == null) return false;
            GameObject obDemolishAbandoned = new GameObject();
            UIButton checkDemolishAbandoned = obDemolishAbandoned.AddComponent<UIButton>();
            checkDemolishAbandoned.transform.parent = bullBar.transform;
            checkDemolishAbandoned.transformPosition = new Vector3(-1.0f, 0.0f);
            checkDemolishAbandoned.text = "Abandoned";
            nullAudioGroup = new AudioGroup(0, new SavedFloat("NOTEXISTINGELEMENT", Settings.gameSettingsFile, 0, false));
            UnityEngine.Debug.LogWarning("Autobulldoze initialized.");
            m_initialized = true;
            return true;
        }

        public override void OnCreated(IThreading threading)
        {
            m_initialized = false;
            BulldozerPanelInterface.initialized = false;
        }

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            init();
            BulldozerPanelInterface.init();
        }
        /*
        private void demolishNowButtonClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            BuildingManager buildManager = Singleton<BuildingManager>.instance;
            for (ushort i = 0; i < buildManager.m_buildings.m_buffer.Length; i++)
            {
                demolishBuilding(i, false);
            }
        }
        */
        private int GetBuildingRefundAmount(ushort building)
        {
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if (Singleton<SimulationManager>.instance.IsRecentBuildIndex(instance.m_buildings.m_buffer[(int)building].m_buildIndex))
                return instance.m_buildings.m_buffer[(int)building].Info.m_buildingAI.GetRefundAmount(building, ref instance.m_buildings.m_buffer[(int)building]);
            else
                return 0;
        }

        public static void DispatchAutobulldozeEffect(BuildingInfo info, Vector3 pos, float angle, int length)
        {
            EffectInfo effect = Singleton<BuildingManager>.instance.m_properties.m_bulldozeEffect;
            if (effect == null) return;
            InstanceID instance = new InstanceID();
            EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(Matrix4x4.TRS(Building.CalculateMeshPosition(info, pos, angle, length), Building.CalculateMeshRotation(angle), Vector3.one), info.m_lodMeshData);
            Singleton<EffectManager>.instance.DispatchEffect(effect, instance, spawnArea, Vector3.zero, 0.0f, 1f, nullAudioGroup);
        }

        private void DeleteBuildingImpl(ushort building, bool showEffect)
        {
            if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)building].m_flags != Building.Flags.None)
            {
                BuildingManager instance = Singleton<BuildingManager>.instance;
                BuildingInfo info = instance.m_buildings.m_buffer[(int)building].Info;
                if (info.m_buildingAI.CheckBulldozing(building, ref instance.m_buildings.m_buffer[(int)building]) == ToolBase.ToolErrors.None)
                {
                    if (info.m_placementStyle == ItemClass.Placement.Automatic)
                    {
                        //this.m_bulldozingMode = BulldozeTool.Mode.Building;
                        //this.m_bulldozingService = info.m_class.m_service;
                        //this.m_deleteTimer = 0.1f;
                    }
                    int buildingRefundAmount = this.GetBuildingRefundAmount(building);
                    if (buildingRefundAmount != 0)
                        Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.RefundAmount, buildingRefundAmount, info.m_class);
                    Vector3 pos = instance.m_buildings.m_buffer[(int)building].m_position;
                    float angle = instance.m_buildings.m_buffer[(int)building].m_angle;
                    int length = instance.m_buildings.m_buffer[(int)building].Length;
                    instance.ReleaseBuilding(building);
                    if (info.m_class.m_service > ItemClass.Service.Office)
                        Singleton<CoverageManager>.instance.CoverageUpdated(info.m_class.m_service, info.m_class.m_subService, info.m_class.m_level);
                    if (showEffect) DispatchAutobulldozeEffect(info, pos, angle, length);
                }
            }
            //if ((int) building == (int) this.m_hoverInstance.Building)
            //      this.m_hoverInstance.Building = (ushort) 0;
            //if ((int) building != (int) this.m_lastInstance.Building)
            //      return;
            //this.m_lastInstance.Building = (ushort) 0;
        }

        public void demolishBuilding(ushort index, bool isAuto)
        {
            SimulationManager simManager = Singleton<SimulationManager>.instance;
            BuildingManager buildManager = Singleton<BuildingManager>.instance;

            //if (!BulldozerPanelInterface.b_demolishAutomatically && isAuto) return;

            if (index >= buildManager.m_buildings.m_buffer.Length)
            {
                UnityEngine.Debug.LogWarning("Autodemolish: building " + index + " not exists.");
                return;
            }
            Building build = buildManager.m_buildings.m_buffer[index];
            bool needToDemolish = false;
            if (BulldozerPanelInterface.b_demolishAbandoned && ((build.m_flags & Building.Flags.Abandoned) != Building.Flags.None)) needToDemolish = true;
            if (BulldozerPanelInterface.b_demolishBurned && ((build.m_flags & Building.Flags.BurnedDown) != Building.Flags.None)) needToDemolish = true;
            if (needToDemolish)
            {
                // UnityEngine.Debug.LogWarning("Autobulldozed #" + index);
                DeleteBuildingImpl(index, true);
                return;
            }
        }

        public override void OnAfterSimulationTick()
        {
            //NetManager netManager = Singleton<NetManager>.instance;
            //PathManager pathManager = Singleton<PathManager>.instance;
            SimulationManager simManager = Singleton<SimulationManager>.instance;
            //VehicleManager vehManager = Singleton<VehicleManager>.instance;            
            BuildingManager buildManager = Singleton<BuildingManager>.instance;
            //   if ((simManager.m_currentTickIndex % 100) != 0) return;
            //uint frame = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            for (ushort i = (ushort)(simManager.m_currentTickIndex % 1000); i < buildManager.m_buildings.m_buffer.Length; i += 1000)
            {
                demolishBuilding(i, true);
                Building build = buildManager.m_buildings.m_buffer[i];
            }
        }
    }

    // And provide some interface for it
    class BulldozerPanelInterface
    {
        public static bool initialized = false;
        public static UIView uiView;

        public static UIButton demolishAbandonedButton;
        public static UIButton demolishBurnedButton;
        public static UIButton demolishAutoButton;
        public static UIButton demolishNowButton;

        public static SavedBool b_demolishAbandoned = new SavedBool("ModDemolishAbandoned", Settings.gameSettingsFile, true, true);
        public static SavedBool b_demolishBurned = new SavedBool("ModDemolishBurned", Settings.gameSettingsFile, true, true);
        public static SavedBool b_demolishAutomatically = new SavedBool("ModDemolishAutomatically", Settings.gameSettingsFile, true, true);


        public static void initButton(UIButton button, bool isCheck)
        {
            string sprite = "SubBarButtonBase";//"ButtonMenu";
            string spriteHov = sprite + "Hovered";
            //demolishAbandonedButton.colorizeSprites = true;
            button.normalBgSprite = spriteHov;
            //demolishAbandonedButton.spritePadding = new RectOffset(5, 5, 2, 2);
            button.disabledBgSprite = spriteHov;// + "Disabled";
            button.hoveredBgSprite = spriteHov;// + "Hovered";
            button.focusedBgSprite = spriteHov;// + "Focused";
            button.pressedBgSprite = sprite + "Pressed";
            button.textColor = new Color32(255, 255, 255, 255);

        }
        public static void updateCheckButton(UIButton button, bool isActive)
        {
            Color32 inactiveColor = new Color32(64, 64, 64, 255);
            Color32 activeColor = new Color32(255, 64, 64, 255);
            Color32 whiteColor = new Color32(255, 255, 255, 255);
            Color32 textColor = new Color32(255, 255, 255, 255);
            Color32 textColorDis = new Color32(128, 128, 128, 255);

            if (isActive == true)
            {
                button.color = activeColor;
                button.focusedColor = activeColor;
                button.hoveredColor = activeColor;
                button.pressedColor = activeColor;
                button.textColor = textColor;
            }
            else
            {
                button.color = inactiveColor;
                button.focusedColor = inactiveColor;
                button.hoveredColor = inactiveColor;
                button.pressedColor = inactiveColor;
                button.textColor = textColorDis;
            }
            button.Unfocus();
        }

        public static void init()
        {
            if (initialized) return;
            uiView = GameObject.FindObjectOfType<UIView>();
            if (uiView == null) return;
            if (UIView.Find("BulldozerBar") == null) return;

            /////////////////////////////////////////////////////

            GameObject demButton = new GameObject();
            GameObject demButton2 = new GameObject();
            GameObject demButton3 = new GameObject();
            GameObject demButton4 = new GameObject();

            demButton.transform.parent = UIView.Find("BulldozerBar").transform;
            demButton2.transform.parent = UIView.Find("BulldozerBar").transform;
            demButton3.transform.parent = UIView.Find("BulldozerBar").transform;
            demButton4.transform.parent = UIView.Find("BulldozerBar").transform;

            demolishAbandonedButton = demButton.AddComponent<UIButton>();
            demolishAbandonedButton.relativePosition = new Vector3(1750.0f, -20.0f);
            demolishAbandonedButton.text = "Abandoned";
            demolishAbandonedButton.width = 130;
            demolishAbandonedButton.height = 50;
            initButton(demolishAbandonedButton, true);

            demolishBurnedButton = demButton2.AddComponent<UIButton>();
            demolishBurnedButton.relativePosition = new Vector3(1595.0f, -20.0f);
            demolishBurnedButton.text = "Burned";
            demolishBurnedButton.width = 130;
            demolishBurnedButton.height = 50;
            initButton(demolishBurnedButton, true);
            /*
            demolishAutoButton = demButton3.AddComponent<UIButton>();
            demolishAutoButton.relativePosition = new Vector3(550.0f, -20.0f);
            demolishAutoButton.text = "Autodemolish";
            demolishAutoButton.width = 120;
            demolishAutoButton.height = 50;                
            initButton(demolishAutoButton, true);

            demolishNowButton = demButton4.AddComponent<UIButton>();
            demolishNowButton.relativePosition = new Vector3(680.0f, -20.0f);
            demolishNowButton.text = "Demolish NOW";
            demolishNowButton.width = 120;
            demolishNowButton.height = 50;
            initButton(demolishNowButton, false);
            */
            demolishAbandonedButton.eventClick += demolishAbandonedClick;
            demolishBurnedButton.eventClick += demolishBurnedClick;
            //demolishAutoButton.eventClick += demolishAutoClick;
            updateCheckButton(demolishAbandonedButton, b_demolishAbandoned.value);
            updateCheckButton(demolishBurnedButton, b_demolishBurned.value);
            //updateCheckButton(demolishAutoButton, b_demolishAutomatically.value);
            initialized = true;
        }
        private static void demolishAutoClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            b_demolishAutomatically.value = !(b_demolishAutomatically.value);
            updateCheckButton(demolishAutoButton, b_demolishAutomatically.value);
        }
        private static void demolishBurnedClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            b_demolishBurned.value = !(b_demolishBurned.value);
            updateCheckButton(demolishBurnedButton, b_demolishBurned.value);
        }
        private static void demolishAbandonedClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            b_demolishAbandoned.value = !(b_demolishAbandoned.value);
            updateCheckButton(demolishAbandonedButton, b_demolishAbandoned.value);
        }
    }

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

    /* Declare the AdjustableDemand variables (thanks Pedro Takeshi!
       * I also renamed everything here to avoid conflicts.
       * Names before were Class1, Class2, var, etc.
    */
    public static class AdjustableDemandVars
    {
        public static string state = "OFF";
        public static string speed = "Fast";
        public static string Rmode = "Fixed";
        public static string Cmode = "Fixed";
        public static string Imode = "Fixed";
        public static int Rcurrent = 100;
        public static int Ccurrent = 100;
        public static int Icurrent = 100;
        public static int Rmin = 0;
        public static int Cmin = 0;
        public static int Imin = 0;
    }
    
    // Start the updater for the AdjustableDemand feature
    public class AdjustableDemandUpdate : DemandExtensionBase
    {
        public override int OnCalculateResidentialDemand(int originalDemand)
        {
            if (AdjustableDemandVars.state == "ON")
            {
                if (AdjustableDemandVars.Rmode == "Fixed")
                {
                    return AdjustableDemandVars.Rcurrent;
                }
                else
                {
                    return originalDemand + AdjustableDemandVars.Rcurrent;
                }
            }
            else
            {
                return originalDemand;
            }
        }

        public override int OnCalculateCommercialDemand(int originalDemand)
        {
            if (AdjustableDemandVars.state == "ON")
            {
                if (AdjustableDemandVars.Cmode == "Fixed")
                {
                    return AdjustableDemandVars.Ccurrent;
                }
                else
                {
                    return originalDemand + AdjustableDemandVars.Ccurrent;
                }
            }
            else
            {
                return originalDemand;
            }
        }

        public override int OnCalculateWorkplaceDemand(int originalDemand)
        {
            if (AdjustableDemandVars.state == "ON")
            {
                if (AdjustableDemandVars.Imode == "Fixed")
                {
                    return AdjustableDemandVars.Icurrent;
                }
                else
                {
                    return originalDemand + AdjustableDemandVars.Icurrent;
                }
            }
            else
            {
                return originalDemand;
            }
        }

        public override int OnUpdateDemand(int lastDemand, int nextDemand, int targetDemand)
        {
            if (AdjustableDemandVars.speed == "Slow" || AdjustableDemandVars.state == "OFF")
            {
                return nextDemand;
            }
            else
            {
                return targetDemand;
            }
        }
    }

    // And a loader for the AdjustableDemand feature
    public class AdjustableDemandLoader : LoadingExtensionBase
    {
        private GameObject obj;
        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.NewGame || mode == LoadMode.LoadGame)
            {
                obj = new GameObject();
                obj.AddComponent<AdjustableDemandUI>();
            }
        }

        public override void OnLevelUnloading()
        {
            GameObject.Destroy(obj);
            obj = null;
        }
    }

    // And a UI for the Adjustable Demand feature
    public class AdjustableDemandUI: MonoBehaviour
    {
        private Rect windowRect = new Rect(0, 0, 0, 0);
        private bool doWindow0;
        void OnGUI()
        {
            doWindow0 = GUI.Toggle(new Rect(Screen.width / 2 - 170, Screen.height - 25, 100, 20), doWindow0, "Edit");
            if (doWindow0)
                windowRect = GUILayout.Window(0, windowRect, DoMyWindow, "Adjust Demand");
        }

        void DoMyWindow(int windowID)
        {
            //Residential
            GUILayout.BeginHorizontal();
            GUILayout.Label("R:");
            AdjustableDemandVars.Rcurrent = Mathf.RoundToInt(GUILayout.HorizontalSlider(AdjustableDemandVars.Rcurrent, AdjustableDemandVars.Rmin, 100, GUILayout.Width(256)));
            GUILayout.Label(AdjustableDemandVars.Rcurrent.ToString());
            if (GUILayout.Button(AdjustableDemandVars.Rmode))
            {
                if (AdjustableDemandVars.Rmode == "Fixed")
                {
                    AdjustableDemandVars.Rmode = "Offset";
                    AdjustableDemandVars.Rmin = -100;
                }
                else
                {
                    AdjustableDemandVars.Rmode = "Fixed";
                    if (AdjustableDemandVars.Rcurrent < 0)
                    {
                        AdjustableDemandVars.Rcurrent = 0;
                    }
                    AdjustableDemandVars.Rmin = 0;
                }
            }
            GUILayout.EndHorizontal();

            //Commercial
            GUILayout.BeginHorizontal();
            GUILayout.Label("C:");
            AdjustableDemandVars.Ccurrent = Mathf.RoundToInt(GUILayout.HorizontalSlider(AdjustableDemandVars.Ccurrent, AdjustableDemandVars.Cmin, 100, GUILayout.Width(256)));
            GUILayout.Label(AdjustableDemandVars.Ccurrent.ToString());
            if (GUILayout.Button(AdjustableDemandVars.Cmode))
            {
                if (AdjustableDemandVars.Cmode == "Fixed")
                {
                    AdjustableDemandVars.Cmode = "Offset";
                    AdjustableDemandVars.Cmin = -100;
                }
                else
                {
                    AdjustableDemandVars.Cmode = "Fixed";
                    if (AdjustableDemandVars.Ccurrent < 0)
                    {
                        AdjustableDemandVars.Ccurrent = 0;
                    }
                    AdjustableDemandVars.Cmin = 0;
                }
            }
            GUILayout.EndHorizontal();

            //Industrial
            GUILayout.BeginHorizontal();
            GUILayout.Label("I:");
            AdjustableDemandVars.Icurrent = Mathf.RoundToInt(GUILayout.HorizontalSlider(AdjustableDemandVars.Icurrent, AdjustableDemandVars.Imin, 100, GUILayout.Width(256)));
            GUILayout.Label(AdjustableDemandVars.Icurrent.ToString());
            if (GUILayout.Button(AdjustableDemandVars.Imode))
            {
                if (AdjustableDemandVars.Imode == "Fixed")
                {
                    AdjustableDemandVars.Imode = "Offset";
                    AdjustableDemandVars.Imin = -100;
                }
                else
                {
                    AdjustableDemandVars.Imode = "Fixed";
                    if (AdjustableDemandVars.Icurrent < 0)
                    {
                        AdjustableDemandVars.Icurrent = 0;
                    }
                    AdjustableDemandVars.Imin = 0;
                }
            }
            GUILayout.EndHorizontal();

            //General
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(AdjustableDemandVars.state))
            {
                if (AdjustableDemandVars.state == "ON")
                {
                    AdjustableDemandVars.state = "OFF";
                }
                else
                {
                    AdjustableDemandVars.state = "ON";
                }
            }
            if (GUILayout.Button(AdjustableDemandVars.speed))
            {
                if (AdjustableDemandVars.speed == "Fast")
                {
                    AdjustableDemandVars.speed = "Slow";
                }
                else
                {
                    AdjustableDemandVars.speed = "Fast";
                }
            }
            if (GUILayout.Button("Reset Sliders"))
            {
                AdjustableDemandVars.Rcurrent = 0;
                AdjustableDemandVars.Ccurrent = 0;
                AdjustableDemandVars.Icurrent = 0;
            }
            GUILayout.EndHorizontal();
            if (windowRect.x == 0 && windowRect.y == 0)
            {
                windowRect.x = (int)(Screen.width * 0.5f - windowRect.width * 0.5f);
                windowRect.y = (int)(Screen.height * 0.5f - windowRect.height * 0.5f);
            }
            GUI.DragWindow();            
        }
    }
}