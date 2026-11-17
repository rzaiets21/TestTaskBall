using System;
using TestTaskBall.controllers;
using TestTaskBall.models.level;
using UnityEditor;
using UnityEngine;

namespace TestTaskBall.Editor {
    [CustomEditor(typeof(LevelConfig))]
    public sealed class LevelConfigEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            var config = (LevelConfig)target;

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Level", GUILayout.Height(30))) {
                Generate(config, config.Seed);
            }

            if (GUILayout.Button("Generate Random Level", GUILayout.Height(30))) {
                Generate(config, UnityEngine.Random.Range(0, int.MaxValue));
            }
        }

        private void Generate(LevelConfig config, int seed) {
            try {
                var generator = new LevelGenerator(config.Settings);
                LevelData data = generator.Generate(seed);

                Undo.RecordObject(config, "Generate Level");

                config.SetSeed(seed);
                config.SetData(data);

                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();

                Debug.Log($"Level generated successfully!\nSeed: {seed}\nInitial radius: {data.InitialRadius:F2}\n" +
                          $"Obstacles: {data.Obstacles.Length}\nDecorations: {data.DecorativeObstacles.Length}", config);
            }
            catch (Exception exception) {
                Debug.LogException(exception, config);
            }
        }
    }
}