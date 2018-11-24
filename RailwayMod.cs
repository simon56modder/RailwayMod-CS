using ColossalFramework;
using ICities;
using UnityEngine;

namespace RailwayMod
{
    public class RailwayMod : LoadingExtensionBase, IUserMod
    {
        public RailwayMod()
        {
            try
            {
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = SETTINGS_FILENAME } });
                Debug.Log("railway debug after settings");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[RailwayMod] Failed to add the settings file :");
                Debug.LogException(e);
            }
        }

        public string Name
        {
            get
            {
                return "Railway Mod";
            }
        }
        public string Description
        {
            get { return ""; }
        }

        public static GameObject obj = null;
        public const string SETTINGS_FILENAME = "RailwayModSettings";

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame)
                return;
            obj = new GameObject("RailwayMod");
            obj.AddComponent<StationTrackSelection>();
            if (enableWires.value)
                obj.AddComponent<ThinWires>();
            obj.AddComponent<ReplacementProps>();
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            Object.Destroy(obj);
            if (obj != null)
                obj = null;
        }

        public static readonly SavedBool enableWires = new SavedBool("enableWires", SETTINGS_FILENAME, true, true);

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("Railway Mod");
            group.AddCheckbox("Enable Thin Wires", enableWires.value, (t) => { enableWires.value = t; });
        }
    }
}
