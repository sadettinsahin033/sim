using UnityEditor;
using UnityEngine;

#if __BETTERSHADERS__
using JBooth.BetterShaders;

namespace JBooth.Road
{
    public class StackableRoadEditor : SubShaderMaterialEditor
    {
        LitBaseStub stub = null;

        public override void OnGUI(MaterialEditor materialEditor, ShaderGUI shaderGUI, MaterialProperty[] props, Material mat)
        {
            if (stub == null)
            {
                stub = new LitBaseStub(shaderGUI, mat);
            }

            stub.DoRoadGUI(materialEditor, props);
        }
    }
}
#endif
