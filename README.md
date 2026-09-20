# OBJ 模型浏览工具（Unity）

将一个 `.obj` 模型导入 Unity 场景并浏览（旋转 / 平移 / 缩放），核心设计目标是**能够直接访问模型顶点**，便于未来扩展（顶点编辑、测量、拾取、碰撞检测等）。

## 为什么选择"自写 OBJ 解析器"

Unity 默认把模型作为只读资源导入（Mesh 顶点在运行时是 `isReadable=false` 或需要 `Read/Write Enabled`），而且官方没有运行时直接读 `.obj` 文本的 API。这里采用**自定义解析器**：

- 解析结果保存为纯数据 `ObjData`（顶点、UV、法线、面、材质组），与 Unity Mesh 解耦；
- 顶点位置保存在 `List<Vector3>` 中，随时可读可改；
- 构建出的 `Mesh` 也可再转回数据，未来做编辑功能无需额外转换。

## 文件结构

```
Assets/
  Scripts/
    ObjLoader/
      ObjData.cs         # 数据容器（顶点/面/材质组）
      ObjParser.cs       # .obj 文本 -> ObjData
      MeshBuilder.cs     # ObjData -> Unity Mesh（三角化、去重、负索引、法线补算）
    Viewer/
      ModelViewer.cs     # 加载入口 + 顶点访问 API
      CameraOrbit.cs     # 相机浏览控制（旋转/平移/缩放）
    Editor/
      ObjViewerMenu.cs   # 编辑器菜单：搭建场景 / 选择文件导入
```

## 快速开始

1. 新建 Unity 项目（任意版本，推荐 2019.4+），把 `Assets/` 拷贝进项目。
2. 菜单栏点 **Tools → OBJ Viewer → Setup Scene**，自动创建相机、灯光、`ModelViewer` 物体。
3. 两种方式加载模型：
   - **编辑器导入**：**Tools → OBJ Viewer → Import OBJ File**，选择 `.obj` 文件即可立即看到模型并自动对焦。
   - **运行时加载**：把 `.obj` 放到 `Assets/StreamingAssets/`，在 `ModelViewer` 组件里填 `objPath`（如 `model.obj`），勾选 `Load On Start` 后运行。
4. 浏览操作：
   - 鼠标**左键拖动**：旋转视角
   - 鼠标**右键/中键拖动**：平移
   - 鼠标**滚轮**：缩放

## 顶点访问 API（未来扩展的关键）

`ModelViewer` 组件上直接可用：

| 成员 | 说明 |
| --- | --- |
| `objData` | 原始解析数据，含 `Positions` / `UVs` / `Normals` / `Faces` |
| `VertexCount` | 顶点总数 |
| `VertexPositions` | 顶点位置列表 `List<Vector3>` |
| `GetVertex(int index)` | 按 0 基下标取顶点 |
| `VertexToWorld(int index)` | 顶点转世界坐标 |
| `loadedMesh` | 构建出的 Unity `Mesh`（`mesh.vertices` 也可直接访问） |
| `SpawnVertexMarkers()` | 在每个顶点生成小球，可视化验证顶点访问（演示） |

示例（在任意脚本里）：

```csharp
ModelViewer viewer = FindObjectOfType<ModelViewer>();
// 遍历所有顶点
for (int i = 0; i < viewer.VertexCount; i++)
{
    Vector3 p = viewer.GetVertex(i);
    // ... 例如做拾取、测量、变形等
}
```

## 解析器支持的 OBJ 特性

- `v`（顶点）、`vt`（纹理坐标）、`vn`（法线）
- `f`（面）：支持 `v`、`v/vt`、`v//vn`、`v/vt/vn`，支持**负索引**（`-1` 表示最后一个顶点）
- 四边形及以上多边形自动**扇形三角化**
- `g` / `o` / `usemtl`：按材质拆分为子网格（subMesh）
- 注释行 `#`、空行

## 已知限制

- 材质贴图（`.mtl` / 纹理）未自动加载，仅解析材质名；如需贴图可扩展 `ModelViewer.AssignMeshAndMaterial`。
- 单文件 OBJ（不含 `mtllib` 引用）开箱即用。

## 未来可扩展方向

- 顶点拾取 / 高亮 / 拖拽编辑（直接改 `objData.Positions` 后 `MeshBuilder.Build` 重建即可）
- 顶点法线可视化、曲率分析
- 尺寸测量、包围盒、最近点查询
- 导出回 `.obj`
