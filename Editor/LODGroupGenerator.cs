using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DepthFirstScheduler;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Pumkin.MochieTools
{
    [Serializable]
    public class LODGroupGenerator : EditorWindow
    {
        const string listLabel = "LODs";
        const float minHeight = 50;
        const float maxHeight = 100;
        
        List<MochieData.LODMesh> meshes = new List<MochieData.LODMesh>();
        
        [SerializeField] Material lodGroupMaterial;
        
        [SerializeField] bool expanded = false;
        [SerializeField] Vector2 scroll;
        
        GUIContent listLabelContent = new GUIContent(listLabel);
            
        [MenuItem("Tools/Mochie/LOD Group Generator")]
        public static void ShowWindow()
        {
            var win = EditorWindow.GetWindow<LODGroupGenerator>();
            win.titleContent = new GUIContent("LOD Generator");
            win.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical(MochieData.Styles.RoundedBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(listLabelContent);
                    expanded = GUILayout.Toggle(expanded, MochieData.Icons.Options, MochieData.Styles.Icon);
                }
                EditorGUILayout.EndHorizontal();

                string lodCount = (meshes?.Count ?? 0).ToString();
                listLabelContent.text = $"{listLabel} ({lodCount})";
                
                if(expanded)
                {
                    MochieHelpers.DrawLine();
                    MochieHelpers.DrawLODListWithAddButtonsScrolling(meshes, ref scroll, true, minHeight, maxHeight);
                    EditorGUILayout.Space();
                }
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(MochieData.Styles.RoundedBox);
            {
                lodGroupMaterial = EditorGUILayout.ObjectField("LOD Group Material", lodGroupMaterial, typeof(Material), true) as Material;
            }
            EditorGUILayout.EndVertical();
            
            
            
            EditorGUILayout.BeginVertical(MochieData.Styles.RoundedBox);
            {
                EditorGUI.BeginDisabledGroup(!CanGenerate);
                if(GUILayout.Button("Generate"))
                {
                    List<LOD> lods = new List<LOD>();
                    Transform parent = null;
                    LODGroup group = null;
                    foreach(var lm in meshes)
                    {
                        GameObject obj = Instantiate(lm.MeshObject, parent, true);
                        obj.name = lm.MeshObject.name;
                        
                        Renderer[] renders = obj.GetComponentsInChildren<Renderer>(true);
                        
                        for(int i = 0; i < renders.Length; i++)
                            renders[i].sharedMaterial = lodGroupMaterial;

                        lods.Add(new LOD(lm.Percent/100, renders));

                        if(!group)
                            group = obj.AddComponent<LODGroup>();
                        if(!parent)
                            parent = obj.transform;
                    }

                    if(group)
                    {
                        group.SetLODs(lods.ToArray());

                        string path = AssetDatabase.GetAssetPath(meshes[0].MeshObject);
                        if(!string.IsNullOrWhiteSpace(path))
                        {
                            path = path.Substring(0, path.LastIndexOf('/') + 1) + meshes[0].MeshObject.name + ".prefab";
                            
                            PrefabUtility.SaveAsPrefabAsset(group.gameObject, path);
                            MochieHelpers.PingAssetAtPath(path);
                            
                            Debug.Log("Generated prefab at " + path);
                        }
                    }
                    else
                    {
                        Debug.LogError("LOD group generation failed.");
                    }
                    if(parent)
                        DestroyImmediate(parent.gameObject);
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndVertical();
        }

        bool CanGenerate => meshes != null && meshes.Count > 0 && meshes[0]?.MeshObject;
    }

    // [CustomPropertyDrawer(typeof(MochieData.LODMesh))]
    // public class LODMeshPropertyDrawer : PropertyDrawer
    // {
    //     static string meshName = nameof(MochieData.LODMesh.Mesh);
    //     static string valueName = nameof(MochieData.LODMesh.Percent);
    //     
    //     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //     {
    //         SerializedProperty mesh = property.FindPropertyRelative(meshName);
    //         SerializedProperty value = property.FindPropertyRelative(valueName);
    //         
    //         EditorGUI.PrefixLabel(position, label);
    //         EditorGUI.BeginProperty(position, label, property);
    //         {
    //             EditorGUILayout.BeginHorizontal();
    //             {
    //                 EditorGUI.ObjectField(position, mesh);
    //                 EditorGUI.BeginChangeCheck();
    //                 float newFloat = EditorGUI.FloatField(position, value.floatValue);
    //                 if(EditorGUI.EndChangeCheck())
    //                     value.floatValue = newFloat;
    //             }
    //             EditorGUILayout.EndHorizontal();
    //         }
    //         EditorGUI.EndProperty();
    //         
    //     }
    // }
}