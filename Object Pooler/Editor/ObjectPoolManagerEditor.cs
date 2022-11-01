using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ObjectPoolManager))]
public class ObjectPoolManagerEditor : Editor
{
    private SerializedProperty objectPoolList;
    private SerializedProperty poolInitializationMode;
    private SerializedProperty allowLogs;
    private SerializedProperty showSummary;

    private ObjectPoolManager manager;

    void OnEnable()
    {
        manager = target as ObjectPoolManager;

        objectPoolList = serializedObject.FindProperty("ObjectPoolList");
        poolInitializationMode = serializedObject.FindProperty("InitMode");
        allowLogs = serializedObject.FindProperty("AllowLogs");
        showSummary = serializedObject.FindProperty("ShowSummaryOnInspectorGUI");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawManagerSettings();
        DrawPoolObjects();
        DrawPoolSummary();

        serializedObject.ApplyModifiedProperties();
    }

    void DrawManagerSettings()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Manager Settings", EditorStyles.largeLabel);
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(poolInitializationMode);
        EditorGUILayout.PropertyField(allowLogs);
    }

    void DrawPoolObjects()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Pools", EditorStyles.largeLabel);
        EditorGUILayout.Separator();

        if (GUILayout.Button("Add New Object Pool"))
        {
            manager.ObjectPoolList.Add(new ObjectPool());
        }

        int listCount = objectPoolList.arraySize;

        if(listCount>0)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Expand All", EditorStyles.miniButton))
            {
                for (int i = 0; i < listCount; i++)
                {
                    SerializedProperty objectPool = objectPoolList.GetArrayElementAtIndex(i);
                    if (objectPool == null) break;

                    objectPool.FindPropertyRelative("IsExpandedOnInspectorGUI").boolValue = true;
                }
            }

            if (GUILayout.Button("Collapse All", EditorStyles.miniButton))
            {
                for (int i = 0; i < listCount; i++)
                {
                    SerializedProperty objectPool = objectPoolList.GetArrayElementAtIndex(i);
                    if (objectPool == null) break;

                    objectPool.FindPropertyRelative("IsExpandedOnInspectorGUI").boolValue = false;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Separator();

        for (int i = 0; i < listCount; i++)
        {
            SerializedProperty objectPool = objectPoolList.GetArrayElementAtIndex(i);
            if (objectPool == null) break;

            SerializedProperty IsExpanded = objectPool.FindPropertyRelative("IsExpandedOnInspectorGUI");
            SerializedProperty Prefab = objectPool.FindPropertyRelative("Prefab");
            SerializedProperty Count = objectPool.FindPropertyRelative("Count");
            SerializedProperty IsParticle = objectPool.FindPropertyRelative("IsParticle");
            SerializedProperty AllowAutoIncrease = objectPool.FindPropertyRelative("AllowAutoIncrease");
            SerializedProperty AutoIncreaseThresholdPercentage = objectPool.FindPropertyRelative("AutoIncreaseThresholdPercentage");
            SerializedProperty AutoIncreaseCoefficient = objectPool.FindPropertyRelative("AutoIncreaseCoefficient");
            SerializedProperty MaxInstances = objectPool.FindPropertyRelative("MaxInstances");

            string groupName = Prefab.objectReferenceValue != null ? Prefab.objectReferenceValue.name : "Element " + i;
            IsExpanded.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(IsExpanded.boolValue, groupName);

            if (IsExpanded.boolValue)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.PropertyField(Prefab);
                EditorGUILayout.PropertyField(Count);
                EditorGUILayout.PropertyField(IsParticle);
                EditorGUILayout.PropertyField(AllowAutoIncrease);

                if (AllowAutoIncrease.boolValue == true)
                {
                    EditorGUILayout.Separator();
                    EditorGUILayout.PropertyField(AutoIncreaseThresholdPercentage);
                    EditorGUILayout.PropertyField(AutoIncreaseCoefficient);
                    EditorGUILayout.PropertyField(MaxInstances);
                    EditorGUILayout.Separator();
                    EditorGUILayout.HelpBox(
                        "If there are less than " + (int)(Count.intValue * (AutoIncreaseThresholdPercentage.intValue / 100f)) +
                        " instances left in reserve, " + (int)(Count.intValue * AutoIncreaseCoefficient.floatValue) +
                        " more instances will be populated.\nMax Instance Count: " + MaxInstances.intValue, MessageType.Info);
                }

                EditorGUILayout.Separator();

                if (GUILayout.Button("Remove " + groupName, EditorStyles.toolbarButton))
                {
                    objectPoolList.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            EditorGUILayout.Separator();
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }

    void DrawPoolSummary()
    {
        showSummary.boolValue = EditorGUILayout.BeginFoldoutHeaderGroup(showSummary.boolValue, "Pool Summary");

        if (showSummary.boolValue)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.HelpBox("Pool is initialized: " + manager.IsInitialized, MessageType.Info);

            if (Application.isPlaying == false) return;

            int listCount = objectPoolList.arraySize;

            for (int i = 0; i < listCount; i++)
            {
                SerializedProperty objectPool = objectPoolList.GetArrayElementAtIndex(i);
                if (objectPool == null) break;

                var prefab = (GameObject)objectPool.FindPropertyRelative("Prefab").objectReferenceValue;
                if (prefab == null) break;

                EditorGUILayout.HelpBox(
                    prefab.name.ToUpper() + 
                   "\n      In Use: " + manager.GetCountInUseByPrefab(prefab) +
                   "\n      In Reserve: " + manager.GetCountInReserveByPrefab(prefab) + 
                   "\n      Total: " + manager.GetCountTotalByPrefab(prefab),
                   MessageType.Info);             
            }
        }
    }
}
