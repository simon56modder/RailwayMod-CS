using System.Collections.Generic;
using UnityEngine;

namespace RailwayMod
{
    public class StationTrackSelection : MonoBehaviour
    {
        // buffers
        private List<BuildingInfo> stations;
        private Dictionary<STType, List<NetInfo>> tracks;
        private List<NetInfo> alltracks;

        // prefab
        private BuildingInfo selectedPrefab;
        private List<STType> prefabNetTypes;

        // UI
        private Rect winrect = new Rect(300, 300, 375, 200);
        private float winheight = 47;
        private Dictionary<STType, Vector2> expandedTypes;

        void Start()
        {
            winrect.x = Screen.width - winrect.width;
            selectedPrefab = null;
            prefabNetTypes = new List<STType>();
            expandedTypes = new Dictionary<STType, Vector2>();
            // create buffer for tracks ranged by type
            tracks = new Dictionary<STType, List<NetInfo>>();
            alltracks = new List<NetInfo>();
            PopulateTracks(Resources.FindObjectsOfTypeAll<NetInfo>());
            // create buffer for stations
            stations = new List<BuildingInfo>();
            PopulateStations(Resources.FindObjectsOfTypeAll<BuildingInfo>());
            Debug.Log("[RailwayMod] [StationTrackSelection] Loading ended.");
        }
        void Update()
        {
            if (ToolsModifierControl.toolController.CurrentTool.GetType() == typeof(BuildingTool))
            {
                var prefab = ((BuildingTool)ToolsModifierControl.toolController.CurrentTool).m_prefab;
                if (selectedPrefab == null || prefab != selectedPrefab)
                {
                    if (stations.Contains(prefab))
                    {
                        // execute here when a station gets selected
                        selectedPrefab = prefab;
                        prefabNetTypes.Clear();
                        FindTrackTypes(prefab);
                        expandedTypes.Clear();
                        winheight = 48 + (28 * prefabNetTypes.Count) + (expandedTypes.Count * 100);
                    }
                    else
                        ResetSelection();
                }
            }
            else
                ResetSelection();
        }
        void OnGUI()
        {
            if (selectedPrefab != null && !ToolsModifierControl.cameraController.m_freeCamera)
                winrect = Utils.ChangeHeight(Utils.ClampRectToScreen(GUI.Window(1524896, winrect, Window, "Railway Station Tracks")), winheight);
        }
        private void Window(int id)
        {
            GUI.DragWindow(new Rect(0, 0, 375, 24));
            GUI.Label(new Rect(5, 18, 365, 23), "Choose station tracks to use on \"" + selectedPrefab.GetUncheckedLocalizedTitle() + "\"");
            ushort h = 43;
            foreach (STType type in prefabNetTypes)
            {
                GUI.Label(new Rect(35, 2 + h, 330, 20), type.ToString());
                var expanded = expandedTypes.ContainsKey(type);
                ushort toIncrease = 28;
                if (GUI.Button(new Rect(5, h, 25, 25), expanded ? "▼" : "►"))
                {
                    if (expanded)
                    {
                        expandedTypes.Remove(type);
                        expanded = false;
                    }
                    else
                    {
                        expandedTypes.Add(type, Vector2.zero);
                        expanded = true;
                    }
                    winheight = 48 + (28 * prefabNetTypes.Count) + (expandedTypes.Count * 100);
                }
                if (expanded)
                {
                    var t = tracks[type];
                    if (t.Count > 10)
                    {
                        toIncrease = 304;
                        GUI.Box(new Rect(5, 27 + h, 365, 272), string.Empty);
                        expandedTypes[type] = GUI.BeginScrollView(new Rect(7, 29 + h, 361, 268), expandedTypes[type], new Rect(0, 0, 342, t.Count * 27 + 4));
                        DrawButtons(t, type, true);
                        GUI.EndScrollView();
                    }
                    else
                    {
                        toIncrease = (ushort)(34 + 27 * t.Count);
                        GUI.Box(new Rect(5, 27 + h, 365, 27 * t.Count + 4), string.Empty);
                        GUI.BeginGroup(new Rect(new Rect(7, 29 + h, 361, 27 * t.Count)));
                        DrawButtons(t, type, false);
                        GUI.EndGroup();
                    }
                }
                h += toIncrease;
            }
            winheight = h + 2;
        }

        private void DrawButtons(List<NetInfo> t, STType type, bool scrollNeeded)
        {
            for (int i = 0; i < t.Count; i++)
            {
                if (GUI.Button(new Rect(2, 2 + i * 27, scrollNeeded ? 338 : 357, 25), t[i].GetUncheckedLocalizedTitle()))
                {
                    // select 't[i]' as the prefab to use on all paths of STType 'type'. 
                    SetStationTrackForType(selectedPrefab, type, t[i]);
                }
            }
        }

        // reset all selection params.
        private void ResetSelection()
        {
            selectedPrefab = null;
            prefabNetTypes.Clear();
            expandedTypes.Clear();
        }

        // self-explanatory
        private void SetStationTrackForType(BuildingInfo info, STType type, NetInfo net)
        {
            if (info.m_paths != null)
            {
                var paths = info.m_paths;
                if (paths.Length != 0)
                {
                    for (uint i = 0; i < paths.Length; i++)
                    {
                        if (paths[i].m_netInfo != null)
                        {
                            if (GetTrackType(paths[i].m_netInfo) == type)
                                paths[i].m_finalNetInfo = net;
                        }
                    }
                }
            }

            //SubBuildings
            if (info.m_subBuildings == null)
                return;

            var subBuildingsCount = info.m_subBuildings.Length;
            for (int i = 0; i < subBuildingsCount; i++)
            {
                var subBuildingInfo = info.m_subBuildings[i];
                if (subBuildingInfo == null)
                    continue;

                this.SetStationTrackForType(subBuildingInfo.m_buildingInfo, type, net);
            }
        }

        // Populates prefabTrackTypes according to the types contained by the given prefab.
        private void FindTrackTypes(BuildingInfo info)
        {
            if (info.m_paths != null)
            {
                var paths = info.m_paths;
                if (paths.Length != 0)
                {
                    for (uint i = 0; i < paths.Length; i++)
                    {
                        if (paths[i].m_netInfo != null)
                        {
                            var type = GetTrackType(paths[i].m_netInfo);
                            if (!prefabNetTypes.Contains(type) && type != STType.none)
                                prefabNetTypes.Add(type);
                        }
                    }
                }
            }

            //SubBuildings
            if (info.m_subBuildings == null)
                return;

            var subBuildingsCount = info.m_subBuildings.Length;
            for (int i = 0; i < subBuildingsCount; i++)
            {
                var subBuildingInfo = info.m_subBuildings[i];
                if (subBuildingInfo == null)
                    continue;
                this.FindTrackTypes(subBuildingInfo.m_buildingInfo);
            }
        }
        // Returns the STType of a given compatible Train track.
        private STType GetTrackType(NetInfo i)
        {
            foreach (STType type in tracks.Keys)
            {
                if (tracks[type].Contains(i))
                    return type;
            }

            return STType.none;
        }

        // Creates the list of compatible NetInfos
        private void PopulateTracks(NetInfo[] networks)
        {
            // initiate list for each type
            tracks[STType.NormalDouble] = new List<NetInfo>();
            tracks[STType.ElevatedDouble] = new List<NetInfo>();
            tracks[STType.SingleOneSided] = new List<NetInfo>();
            tracks[STType.SingleTwoSided] = new List<NetInfo>();
            tracks[STType.Wide] = new List<NetInfo>();

            // read through all networks
            for (uint i = 0; i < networks.Length; i++)
            {
                var net = networks[i];
                if (net.m_segments.Length == 0)
                    continue;
                // add netinfos with tag
                if (net.m_segments[0].m_lodMaterial.name.Contains("r69rwst"))
                {
                    // read tag and assign netinfo to dictionary
                    switch (net.m_segments[0].m_lodMaterial.name.Trim().ToLower())
                    {
                        case "r69rwst-double":
                            addTrackToList(STType.NormalDouble, net);
                            continue;
                        case "r69rwst-edouble":
                            addTrackToList(STType.ElevatedDouble, net);
                            continue;
                        case "r69rwst-single1":
                            addTrackToList(STType.SingleOneSided, net);
                            continue;
                        case "r69rwst-single2":
                            addTrackToList(STType.SingleTwoSided, net);
                            continue;
                        case "r69rwst-wide":
                            addTrackToList(STType.Wide, net);
                            continue;
                    }
                }
            }
            // normal double
            addTrackToList(STType.NormalDouble, "Train Station Track");
            addTrackToList(STType.NormalDouble, "Train Cargo Track");
            addTrackToList(STType.NormalDouble, "Train Station Track (C)");
            addTrackToList(STType.NormalDouble, "Train Station Track (NP)");
            addTrackToList(STType.NormalDouble, "Train Station Track (CNP)");
            // elevated double
            addTrackToList(STType.ElevatedDouble, "Train Cargo Track Elevated");
            addTrackToList(STType.ElevatedDouble, "Station Track Elevated (C)");
            addTrackToList(STType.ElevatedDouble, "Station Track Elevated (NP)");
            addTrackToList(STType.ElevatedDouble, "Station Track Elevated (CNP)");
            addTrackToList(STType.ElevatedDouble, "Station Track Elevated Narrow");
            addTrackToList(STType.ElevatedDouble, "Station Track Elevated Narrow (C)");
            addTrackToList(STType.ElevatedDouble, "Station Track Elevated Narrow (NP)");
            addTrackToList(STType.ElevatedDouble, "Station Track Elevated Narrow (CNP)");
            addTrackToList(STType.ElevatedDouble, "Station Track Eleva");
            // single 1
            addTrackToList(STType.SingleOneSided, "Rail1LStation");
            // single 2
            addTrackToList(STType.SingleTwoSided, "Rail1L2SidedStation");
            // wide
            addTrackToList(STType.Wide, "1194290640.Wide Train Station Track_Data");
        }
        private void addTrackToList(STType type, string name)
        {
            var p = PrefabCollection<NetInfo>.FindLoaded(name);
            if (p != null)
            {
                if (tracks[type].Contains(p))
                    return;
                tracks[type].Add(p);
                alltracks.Add(p);
            }
        }
        private void addTrackToList(STType type, NetInfo info)
        {
            if (info != null)
            {
                if (tracks[type].Contains(info))
                    return;
                tracks[type].Add(info);
                alltracks.Add(info);
            }
        }

        // Creates the list of prefabs that have compatible Train Station tracks
        private void PopulateStations(BuildingInfo[] buildings)
        {
            // read through all buildings
            for (uint i = 0; i < buildings.Length; i++)
            {
                // skip buildings with errors or no paths
                if (buildings[i] == null)
                    continue;
                if (buildings[i].m_paths == null)
                    continue;
                if (buildings[i].m_paths.Length == 0)
                    continue;

                // read through all paths
                foreach (var p in buildings[i].m_paths)
                {
                    // skip paths with null netInfo
                    if (p.m_netInfo == null)
                        continue;
                    if (alltracks.Contains(p.m_netInfo) && !stations.Contains(buildings[i]))
                    {
                        stations.Add(buildings[i]);
                    }
                }
            }
        }
    }
        
    public enum STType
    {
        NormalDouble,
        ElevatedDouble,
        SingleOneSided,
        SingleTwoSided,
        Wide,
        none
    }
}
