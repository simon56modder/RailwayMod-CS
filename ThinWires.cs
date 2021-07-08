using System.Linq;
using UnityEngine;

namespace RailwayMod
{
    public class ThinWires : MonoBehaviour
    {
        void Start()
        {
            //TODO: do this stuff async

            var prefabs = Resources.FindObjectsOfTypeAll<NetInfo>()
                .Where(x => x.m_netAI is TrainTrackBaseAI)
                .ToArray();

            Vector2 sca = new Vector2(3.5f, 1.0f);

            for (int i = 0; i < prefabs.Length; i ++)
            {
                if (prefabs[i] == null)
                    continue;
                foreach (var seg in prefabs[i].m_segments)
                {
                    if (seg == null)
                        continue;
                    if (seg.m_material == null)
                        continue;
                    if (seg.m_material.shader == null)
                        continue;
                    if (seg.m_material.shader.name != "Custom/Net/Electricity")
                        continue;
                    seg.m_material.mainTextureScale = sca;
                    seg.m_segmentMaterial.mainTextureScale = sca;
                    seg.m_lodMaterial.mainTextureScale = sca;
                    seg.m_combinedLod.m_material.mainTextureScale = sca;
                }
                foreach (var node in prefabs[i].m_nodes)
                {
                    if (node == null)
                        continue;
                    if (node.m_material == null)
                        continue;
                    if (node.m_material.shader == null)
                        continue;
                    if (node.m_material.shader.name != "Custom/Net/Electricity")
                        continue;
                    node.m_material.mainTextureScale = sca;
                    node.m_nodeMaterial.mainTextureScale = sca;
                    node.m_lodMaterial.mainTextureScale = sca;
                    node.m_combinedLod.m_material.mainTextureScale = sca;
                }
            }
            Debug.Log("[RailwayMod] [ThinWires] Loading ended.");
        }

        
    }
}
