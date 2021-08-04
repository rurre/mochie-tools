using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pumkin.MochieTools
{
    [Serializable]
    public class LODGroupGenerator : EditorWindow
    {
        const string listLabel = "LODs";
        const float minListHeight = 50;
        const float maxListHeight = 100;

        const int DefaultListSize = 4;

        string SubTitle
        {
            get
            {
                if(string.IsNullOrWhiteSpace(_subTitle))
                    _subTitle = "by Pumkin - v" + version.ToString();
                return _subTitle;
            }
        }
        
        List<GameObject> MeshObjects
        {
            get
            {
                if(_meshObjects == null)
                    _meshObjects.ResizeWithDefaults(DefaultListSize);
                return _meshObjects;
            }
            set => _meshObjects = value;
        }
        
        List<float> MeshValues
        {
            get
            {
                if(_meshValues == null)
                    _meshValues.ResizeWithDefaults(DefaultListSize);
                
                int meshCount = _meshObjects?.Count ?? 0;
                if(_meshValues.Count != meshCount)
                    _meshValues.ResizeWithDefaults(meshCount - 1);
                return _meshValues;
            }
        }
        
        Version version = new Version(1, 0);
        string _subTitle;
        
        List<GameObject> _meshObjects = new List<GameObject>(DefaultListSize);
        List<float> _meshValues = new List<float>(DefaultListSize);

        [SerializeField] string namePrefix;
        [SerializeField] Material lodGroupMaterial;
        [SerializeField] float cullingRange = 5;
        
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
            EditorGUILayout.LabelField("LOD Group Generator", MochieData.Styles.TitleLabel);
            EditorGUILayout.LabelField(SubTitle);

            MochieHelpers.DrawLine();
            
            EditorGUILayout.BeginVertical(MochieData.Styles.RoundedBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(listLabelContent);
                }
                EditorGUILayout.EndHorizontal();

                string lodCount = (MeshObjects?.Count ?? 0).ToString();
                listLabelContent.text = $"{listLabel} ({lodCount})";

                MochieHelpers.DrawLine();
                MochieHelpers.DrawLODListWithAddButtonsScrolling(MeshObjects, MeshValues, ref scroll, true, minListHeight, maxListHeight);
                EditorGUILayout.Space();
                
                EditorGUI.BeginChangeCheck();
                float newCullingRange = EditorGUILayout.FloatField("Culling Range", cullingRange);
                if(EditorGUI.EndChangeCheck())
                    cullingRange = Mathf.Clamp(newCullingRange, 0, newCullingRange);
                
                EditorGUILayout.Space();
                
                if(GUILayout.Button("From Selection", MochieData.Styles.MediumButton))
                {
                    var newObjects = MochieHelpers.GetLODsFromSelection(out Material lodMaterial);
                    if(newObjects != null)
                        MeshObjects = newObjects; 
                    if(lodMaterial)
                        lodGroupMaterial = lodMaterial;
                }
                GUILayout.Space(4);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(MochieData.Styles.RoundedBox);
            {
                namePrefix = EditorGUILayout.TextField("Name Prefix", namePrefix);
                
                EditorGUILayout.Space();
                
                lodGroupMaterial = EditorGUILayout.ObjectField("LOD Group Material", lodGroupMaterial, typeof(Material), true) as Material;
            }
            EditorGUILayout.EndVertical();
            
            
            
            EditorGUILayout.BeginVertical(MochieData.Styles.RoundedBox);
            {
                EditorGUI.BeginDisabledGroup(!CanGenerate);
                if(GUILayout.Button("Generate", MochieData.Styles.BigButton))
                {
                    List<LOD> lods = new List<LOD>();
                    Transform parent = null;
                    LODGroup group = null;

                    for(var i = 0; i < MeshObjects.Count; i++)
                    {
                        GameObject mesh = MeshObjects[i];
                        float value = i < MeshObjects.Count - 1 ? MeshValues[i] : cullingRange;
                        
                        GameObject obj = Instantiate(mesh, parent, true);
                        obj.name = mesh.name;

                        Renderer[] renders = obj.GetComponentsInChildren<Renderer>(true);

                        foreach(var ren in renders)
                            ren.sharedMaterial = lodGroupMaterial;

                        lods.Add(new LOD(value / 100, renders));

                        if(!group)
                            group = obj.AddComponent<LODGroup>();
                        if(!parent)
                            parent = obj.transform;
                    }

                    if(group)
                    {
                        group.SetLODs(lods.ToArray());

                        string path = AssetDatabase.GetAssetPath(MeshObjects[0]);
                        if(!string.IsNullOrWhiteSpace(path))
                        {
                            string prefix = string.IsNullOrWhiteSpace(namePrefix) ? "" : $"{namePrefix}_";
                            string prefabName = $"{prefix}{_meshObjects[0].name}.prefab";
                            path = path.Substring(0, path.LastIndexOf('/') + 1) + prefabName;
                            
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

        bool CanGenerate => MeshObjects != null && MeshObjects.Count > 0 && MeshObjects[0];
    }
}