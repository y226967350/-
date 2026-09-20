#if UNITY_EDITOR
using System.IO;
using ObjViewer.Viewer;
using UnityEditor;
using UnityEngine;

namespace ObjViewer.EditorTools
{
    /// <summary>
    /// 编辑器菜单：一键搭建浏览场景，或从文件选择器导入 obj。
    /// </summary>
    public static class ObjViewerMenu
    {
        [MenuItem("Tools/OBJ Viewer/Setup Scene")]
        public static void SetupScene()
        {
            // 相机
            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = new GameObject("Main Camera").AddComponent<Camera>();
                cam.tag = "MainCamera";
                cam.transform.position = new Vector3(0, 1, -5);
                cam.transform.LookAt(Vector3.zero);
            }
            if (cam.GetComponent<CameraOrbit>() == null)
            {
                cam.gameObject.AddComponent<CameraOrbit>();
            }

            // 灯光
            if (Object.FindObjectOfType<Light>() == null)
            {
                GameObject lightObj = new GameObject("Directional Light");
                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            }

            // 模型加载器
            ModelViewer viewer = Object.FindObjectOfType<ModelViewer>();
            if (viewer == null)
            {
                viewer = new GameObject("ModelViewer").AddComponent<ModelViewer>();
            }

            // 让相机跟随 viewer 加载出的模型
            CameraOrbit orbit = cam.GetComponent<CameraOrbit>();
            orbit.target = viewer.transform;

            Selection.activeGameObject = viewer.gameObject;
            Debug.Log("[OBJ Viewer] 场景已就绪。请在 ModelViewer 组件里填写 objPath，或使用 Import OBJ File 选择文件。");
        }

        [MenuItem("Tools/OBJ Viewer/Import OBJ File")]
        public static void ImportObjFile()
        {
            string path = EditorUtility.OpenFilePanel("选择 OBJ 模型", "", "obj");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            ModelViewer viewer = Object.FindObjectOfType<ModelViewer>();
            if (viewer == null)
            {
                SetupScene();
                viewer = Object.FindObjectOfType<ModelViewer>();
            }

            viewer.objPath = path;
            viewer.loadOnStart = false;
            viewer.LoadObjFromText(File.ReadAllText(path), Path.GetFileName(path));

            // 让相机对准模型
            CameraOrbit orbit = Camera.main != null ? Camera.main.GetComponent<CameraOrbit>() : null;
            if (orbit != null && viewer.loadedObject != null)
            {
                orbit.FrameOn(viewer.loadedObject.transform);
            }

            EditorGUIUtility.PingObject(viewer.loadedObject);
            Debug.Log($"[OBJ Viewer] 已导入：{path}");
        }
    }
}
#endif
