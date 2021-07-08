using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RailwayMod
{
    public class ReplacementProps : MonoBehaviour
    {
        #region prop definition arrays
        readonly string[] defaultProps = new string[] {
            "1530376523.r69rwp-cat2n_Data",
            "1530376523.r69rwp-cat2e_Data",
            "1530376523.r69rwp-cat2t_Data",
            "1530376523.r69rwp-cat1n_Data",
            "1530376523.r69rwp-cat1e_Data",
            "1530376523.r69rwp-cat1t_Data",
            "1530376523.r69rwp-sigr_Data",
            "1530376523.r69rwp-sigl_Data",
            "Railway Crossing Short",
            "Railway Crossing Medium",
            "Railway Crossing Long",
            "Railway Crossing Very Long",
            "1530376523.r69rwp-buff_Data",
            "1530376523.r69rwp-cat4n_Data",
            "1530376523.r69rwp-cat4e_Data",
            "1530376523.r69rwp-cat4t_Data",
            "1530376523.r69rwp-gant1_Data",
            "1530376523.r69rwp-gant2_Data",
            "1530376523.r69rwp-gant4_Data" };
        PropInfo[] currentProps = new PropInfo[] {
           null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null };
        readonly string[] propIdentifiers = new string[] {
            "r69rwp-cat2n",
            "r69rwp-cat2e",
            "r69rwp-cat2t",
            "r69rwp-cat1n",
            "r69rwp-cat1e",
            "r69rwp-cat1t",
            "r69rwp-sigr",
            "r69rwp-sigl",
            "r69rwp-barr1",
            "r69rwp-barr2",
            "r69rwp-barr3",
            "r69rwp-barr4",
            "r69rwp-buff",
            "r69rwp-cat4n",
            "r69rwp-cat4e",
            "r69rwp-cat4t",
            "r69rwp-gant1",
            "r69rwp-gant2",
            "r69rwp-gant4" };
        readonly string[] propTypes = new string[] {
            "Double normal catenary",
            "Double end-catenary",
            "Double tunnel catenary",
            "Single normal catenary",
            "Single end-catenary",
            "Single tunnel catenary",
            "Right signal",
            "Left signal",
            "Short barrier",
            "Medium barrier",
            "Long barrier",
            "Very long barrier",
            "Buffer",
            "Quad normal catenary",
            "Quad end-catenary",
            "Quad tunnel catenary",
            "Single gantry",
            "Double gantry",
            "Quad gantry" };
        #endregion
        SavedString[] settings;
        public List<Dictionary<PropInfo, string>> replacementProps;

        // UI
        private Rect winrect = new Rect(200, 100, 375, 602);
        private int winheight = 602;
        private short expanded = -1;
        private bool showWindow = false;
        private Vector2 scroll = Vector2.zero;

        void Start()
        {
            settings = new SavedString[19];
            for (short i = 0; i < 19; i++)
            {
                settings[i] = new SavedString(propIdentifiers[i], RailwayMod.SETTINGS_FILENAME, defaultProps[i], true);
            }
            var allProps = Resources.FindObjectsOfTypeAll<PropInfo>();
            LoadReplacementProps(allProps);
            // replace props on load
            for (ushort i = 0; i < 19; i++)
            {
                // check if prop exists
                try
                {
                    allProps.Single(prop => prop.name == settings[i].value);
                    SetProp(allProps.Single(prop => prop.name == settings[i].value), i);
                }
                catch (InvalidOperationException)
                {
                    var p = allProps.Single(prop => prop.name == defaultProps[i]);
                    SetProp(p, i);
                    settings[i].value = p.name;
                    Debug.Log("[RailwayMod] [ReplacementProps] The prop of type " + propTypes[i] + " saved in the settings doesn't exist anymore : setting default.");
                }
            }
            Debug.Log("[RailwayMod] [ReplacementProps] Loading ended.");
        }
        void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift) 
                && Input.GetKey(KeyCode.LeftAlt)
                && Input.GetKeyDown(KeyCode.P)
            )
            {
                showWindow = !showWindow;
            }
        }
        void OnGUI()
        {
            if (showWindow && !ToolsModifierControl.cameraController.m_freeCamera)
                winrect = Utils.ChangeHeight(Utils.ClampRectToScreen(GUI.Window(1524897, winrect, Window, "Railway Props")), winheight);
        }
        void Window(int id)
        {
            GUI.DragWindow(new Rect(0, 0, 350, 24));
            if (GUI.Button(new Rect(351, 3, 20, 21), "x"))
            {
                showWindow = false;
                expanded = -1;
            }


            ushort h = 43;
            for (ushort i = 0; i < 19; i++)
            {
                ushort toIncrease = 28;
                GUI.Label(new Rect(35, 2 + h, 330, 20), "<b>" + propTypes[i] + "</b> : " + replacementProps[i][currentProps[i]]);
                if (GUI.Button(new Rect(5, h, 25, 25), (expanded == i) ? "▼" : "►"))
                {
                    scroll = Vector2.zero;
                    if (expanded == i)
                        expanded = -1;
                    else
                        expanded = (short)i;
                }
                if (expanded == i)
                {
                    if (replacementProps[i].Count > 6)
                    {
                        toIncrease = 196;
                        GUI.Box(new Rect(5, 27 + h, 365, 164), string.Empty);
                        scroll = GUI.BeginScrollView(new Rect(7, 29 + h, 361, 160), scroll, new Rect(0, 0, 342, replacementProps[i].Count * 27 + 4));
                        DrawButtons(i, true);
                        GUI.EndScrollView();
                    }
                    else
                    {
                        toIncrease = (ushort)(34 + 27 * replacementProps[i].Count);
                        GUI.Box(new Rect(5, 27 + h, 365, replacementProps[i].Count * 27 + 4), string.Empty);
                        GUI.BeginGroup(new Rect(new Rect(7, 28 + h, 361, 27 * replacementProps[i].Count)));
                        DrawButtons(i, false);
                        GUI.EndGroup();
                    }
                }
                h += toIncrease;
            }
            winheight = h + 2;
        }

        private void DrawButtons(ushort i, bool scrolling)
        {
            uint j = 0;
            foreach (PropInfo info in replacementProps[i].Keys)
            {
                var s = replacementProps[i][info];
                if (settings[i].value == info.name)
                    s = "<b><i>" + s + "</i></b>";
                if (GUI.Button(new Rect(2, 2 + j * 27, scrolling ? 338 : 357, 25), s))
                {
                    // select info as the prop to use and save 
                    SetProp(info, i);
                    settings[i].value = info.name;
                }
                j += 1;
            }
        }

        // Finds compatible replacement props and sorts them per type
        private void LoadReplacementProps(PropInfo[] allprops)
        {
            replacementProps = new List<Dictionary<PropInfo, string>>();
            for (ushort i = 0; i < 19; i++)
                replacementProps.Add(new Dictionary<PropInfo, string>());
            foreach (var prop in allprops)
            {
                if (prop == null) continue;
                if (prop.m_material == null) continue;
                if (prop.m_material.name == "") continue;

                for (ushort i = 0; i < 19; i ++)
                {
                    if (prop.m_material.name.Contains(propIdentifiers[i]))
                    {
                        if (!replacementProps[i].ContainsKey(prop))
                            replacementProps[i].Add(prop, GetTooltip(prop, i));
                    }
                }
            }
            // add normal catenaries to list when there aren't any end catenary
            foreach (var kvp in replacementProps[0])
            {
                if (!replacementProps[1].Values.Any(st => st == kvp.Value))
                    replacementProps[1].Add(kvp.Key, kvp.Value);
            }
            // same for single
            foreach (var kvp in replacementProps[3])
            {
                if (!replacementProps[4].Values.Any(st => st == kvp.Value))
                    replacementProps[4].Add(kvp.Key, kvp.Value);
            }

            // add vanilla barriers
            for (ushort i = 8; i <= 11; i++)
                replacementProps[i].Add(allprops.FirstOrDefault(prop => prop.name == defaultProps[i]), "Vanilla");
        }
        private void SetProp(PropInfo replacementProp, int type)
        {
            currentProps[type] = replacementProp;
            for (uint i = 0; i < PrefabCollection<NetInfo>.LoadedCount(); i++)
            {
                var prefab = PrefabCollection<NetInfo>.GetLoaded(i);
                if (prefab == null) continue;
                if (prefab.m_lanes != null)
                {
                    foreach (var lane in prefab.m_lanes)
                    {
                        if (lane.m_laneProps != null)
                        {
                            if (lane.m_laneProps.m_props != null) 
                                foreach (var LaneProp in lane.m_laneProps.m_props)
                                {
                                    if (LaneProp.m_prop != null)
                                    {
                                        if (LaneProp.m_prop.name == defaultProps[type])
                                        {
                                            LaneProp.m_finalProp = replacementProp;
                                        }
                                    }
                                }
                        }
                    }
                }
            }
        }
        private string GetTooltip(PropInfo info, int type)
        {
            return info.m_material.name.Trim().GetStringBetween(propIdentifiers[type] + "#", "#");
        }
    }
}
