using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Pumkin.MochieTools
{
    internal class MochieHelpers
    {
        static float elementLabelWidth = 70 * EditorGUIUtility.pixelsPerPoint;
        static float valueFieldWidth = 30 * EditorGUIUtility.pixelsPerPoint;
        public static void DrawLine(float height = 1f, bool spaceBefore = true, bool spaceAfter = true)
        {
            if(spaceBefore)
                EditorGUILayout.Space();
            GUILayout.Box(GUIContent.none, MochieData.Styles.EditorLine, GUILayout.ExpandWidth(true), GUILayout.Height(height));
            if(spaceAfter)
                EditorGUILayout.Space();
        }
        
        public static bool DrawLODListWithAddButtonsScrolling(List<GameObject> meshList, List<float> valueList, ref Vector2 scroll, bool drawLabels, float minHeight, float maxHeight)
        {
            bool changed = false;
            EditorGUILayout.BeginHorizontal();
            {
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MinHeight(minHeight), GUILayout.MaxHeight(maxHeight));
            
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                GUIContent gc = GUIContent.none;
                for(int i = 0; i < meshList.Count; i++)
                {
                    if(drawLabels)
                        gc = new GUIContent($"LOD {i}");
                    try
                    {
                        GameObject currentMeshObj = meshList[i] ?? null;
                        GameObject newMeshObj = null;

                        float currentValue = i > 0 ? valueList[i-1] : 100;
                        float newValue = 0;

                        try
                        {
                            float oldWidth = EditorGUIUtility.labelWidth;
                            EditorGUILayout.BeginHorizontal();
                            
                            EditorGUIUtility.labelWidth = elementLabelWidth;
                            
                            EditorGUI.BeginChangeCheck();
                            newMeshObj = EditorGUILayout.ObjectField(gc, currentMeshObj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
                            if(EditorGUI.EndChangeCheck())
                            {
                                if(!newMeshObj || IsValidMeshObject(newMeshObj))
                                {
                                    meshList[i] = newMeshObj;
                                    changed = true;
                                }
                            }

                            EditorGUIUtility.labelWidth = elementLabelWidth;
                            
                            EditorGUI.BeginDisabledGroup(i == 0);
                            EditorGUI.BeginChangeCheck();
                            newValue = EditorGUILayout.FloatField(GUIContent.none, currentValue, GUILayout.MaxWidth(valueFieldWidth));
                            if(EditorGUI.EndChangeCheck())
                            {
                                valueList[i-1] = Mathf.Clamp(newValue, 0, 100);
                            }
                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.EndHorizontal();
                            EditorGUIUtility.labelWidth = oldWidth;
                        }
                        catch(ExitGUIException) { }
                    }
                    catch(Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                EditorGUILayout.EndVertical();
            
                EditorGUILayout.EndScrollView();

                DrawLODListAddButtons(meshList, valueList);
            }
            EditorGUILayout.EndHorizontal();
            return changed;
        }

        static bool IsValidMeshObject(GameObject meshObject)
        {
            return meshObject &&
                   meshObject.GetComponent<MeshFilter>()?.sharedMesh ||
                   meshObject.GetComponent<SkinnedMeshRenderer>()?.sharedMesh;
        }

        static void DrawLODListAddButtons(List<GameObject> objects, List<float> values)
        {
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(MochieData.Styles.IconButton.fixedWidth));
            {
                if(GUILayout.Button(MochieData.Icons.Add, MochieData.Styles.IconButton))
                {
                    objects.ResizeWithDefaults(objects.Count + 1);
                    values.ResizeWithDefaults(values.Count + 1);
                }

                if(GUILayout.Button(MochieData.Icons.Remove, MochieData.Styles.IconButton))
                {
                    objects.ResizeWithDefaults(objects.Count - 1);
                    values.ResizeWithDefaults(values.Count - 1);
                }

                if(GUILayout.Button(MochieData.Icons.RemoveAll, MochieData.Styles.IconButton))
                {
                    objects.Clear();
                    values.Clear();
                }
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Selects and highlights the asset in your unity Project tab
        /// </summary>
        /// <param name="path"></param>
        internal static void PingAssetAtPath(string path)
        {
            var inst = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path).GetInstanceID();
            EditorGUIUtility.PingObject(inst);
        }

        internal static List<GameObject> GetLODsFromSelection(out Material lodGroupMaterial)
        {
            List<GameObject> unsorted = new List<GameObject>();
            lodGroupMaterial = null;
            
            foreach(var obj in Selection.objects)
            {
                if(obj is GameObject go)
                {
                    if(IsValidMeshObject(go))
                        unsorted.Add(go);
                }
                else if(obj is Material mat)
                {
                    lodGroupMaterial = mat;
                }
            }

            return unsorted
                .OrderBy(go => Regex.Match(go.name, @"/(LOD\d)/gi"))
                .ToList();
        }
    }

    internal static class MochieExtensions
    {
        /// <summary>
        /// Resizes the list and fills it with default values when expanding
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="size">New size</param>
        public static void ResizeWithDefaults<T>(this List<T> list, int size)
        {
            if(list is null)
                list = new List<T>();
            else if(size == list.Count)
                return;
            
            if(size <= 0)
            {
                list.Clear();
                return;
            }

            var toAdd = size - list.Count;

            if(toAdd <= 0)
                list.RemoveRange(size, toAdd * -1);
            else
                for(var i = 0; i < toAdd; i++)
                    list.Add(default(T));
        }
    }
}