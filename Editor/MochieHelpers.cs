using System;
using System.Collections.Generic;
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
        
        public static bool DrawLODListWithAddButtonsScrolling(List<MochieData.LODMesh> meshList, ref Vector2 scroll, bool drawLabels, float minHeight, float maxHeight)
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
                        MochieData.LODMesh current = meshList[i];
                        GameObject newMeshObject = null;
                        float newValue = 0;

                        try
                        {
                            float oldWidth = EditorGUIUtility.labelWidth;
                            EditorGUILayout.BeginHorizontal();
                            
                            EditorGUIUtility.labelWidth = elementLabelWidth;
                            
                            EditorGUI.BeginChangeCheck();
                            newMeshObject = EditorGUILayout.ObjectField(gc, current?.MeshObject, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
                            if(EditorGUI.EndChangeCheck())
                            {
                                bool isValidMesh = newMeshObject &&
                                                   newMeshObject.GetComponent<MeshFilter>()?.sharedMesh ||
                                                   newMeshObject.GetComponent<SkinnedMeshRenderer>()?.sharedMesh;

                                if(!newMeshObject || isValidMesh)
                                {
                                    if(meshList[i] == null)
                                        meshList[i] = new MochieData.LODMesh(newMeshObject, 0);
                                    
                                    meshList[i].MeshObject = newMeshObject;
                                    changed = true;
                                }
                            }

                            EditorGUIUtility.labelWidth = elementLabelWidth;
                            
                            EditorGUI.BeginChangeCheck();
                            newValue = EditorGUILayout.FloatField(GUIContent.none, current?.Percent ?? 0, GUILayout.MaxWidth(valueFieldWidth));
                            if(EditorGUI.EndChangeCheck())
                            {
                                if(meshList[i] == null)
                                    meshList[i] = new MochieData.LODMesh(newMeshObject, 0);
                                else
                                    meshList[i].Percent = newValue;
                            }

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

                DrawAddButtons(meshList);
            }
            EditorGUILayout.EndHorizontal();
            return changed;
        }

        static void DrawAddButtons<T>(List<T> list) where T : class
        {
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(MochieData.Styles.IconButton.fixedWidth));
            {
                if(GUILayout.Button(MochieData.Icons.Add, MochieData.Styles.IconButton))
                    list.ResizeWithDefaults(list.Count + 1);
                if(GUILayout.Button(MochieData.Icons.Remove, MochieData.Styles.IconButton))
                    list.ResizeWithDefaults(list.Count - 1);
                if(GUILayout.Button(MochieData.Icons.RemoveAll, MochieData.Styles.IconButton))
                    list.Clear();
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
            if(size == list.Count)
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