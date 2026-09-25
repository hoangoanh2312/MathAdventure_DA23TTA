#if UNITY_EDITOR
using System.IO;
using MathAdventure.Collectibles;
using MathAdventure.Core;
using MathAdventure.Player;
using MathAdventure.UI;
using MathAdventure.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MathAdventure.Editor
{
    public static class MathAdventureProjectBuilder
    {
        private const string ScenePath = "Assets/Scenes/FoundationTest.unity";

        [MenuItem("MathAdventure/Setup Foundation")]
        public static void SetupFoundation()
        {
            EnsureFolders();
            var scene = File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var manager = EnsureRoot("GameManager");
            EnsureComponent<GameManager>(manager);
            EnsureComponent<SceneLoader>(manager);

            var player = EnsureRoot("Player");
            player.tag = "Player";
            player.transform.position = Vector3.zero;
            var playerBody = EnsureComponent<Rigidbody2D>(player);
            playerBody.gravityScale = 0f;
            playerBody.freezeRotation = true;
            EnsureComponent<CapsuleCollider2D>(player).size = new Vector2(0.75f, 0.9f);
            EnsureComponent<PlayerController>(player);
            EnsureComponent<PlayerAnimator>(player);
            EnsureComponent<PlayerHealth>(player);
            EnsureComponent<PlayerInventory>(player);
            EnsureComponent<PlayerInteractor>(player);
            ConfigureSprite(player, new Color(0.2f, 0.55f, 0.95f), new Vector2(0.8f, 1f), 10);

            var cameraObject = EnsureRoot("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = EnsureComponent<Camera>(cameraObject);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            EnsureComponent<AudioListener>(cameraObject);
            var follow = EnsureComponent<CameraFollow>(cameraObject);
            follow.SetTarget(player.transform);

            CreateWorld();
            CreateCollectibles();
            CreateHud();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("MathAdventure foundation scene created/repaired: " + ScenePath);
        }

        private static void EnsureFolders()
        {
            string[] folders = {
                "Assets/Art", "Assets/Audio", "Assets/Data", "Assets/Editor", "Assets/Prefabs",
                "Assets/Prefabs/Characters", "Assets/Prefabs/Collectibles", "Assets/Prefabs/Environment",
                "Assets/Prefabs/Gameplay", "Assets/Prefabs/UI", "Assets/Resources", "Assets/Resources/Levels",
                "Assets/Scenes", "Assets/Scripts", "Assets/Scripts/Core", "Assets/Scripts/Player",
                "Assets/Scripts/Interaction", "Assets/Scripts/Collectibles", "Assets/Scripts/World",
                "Assets/Scripts/Objectives", "Assets/Scripts/Math", "Assets/Scripts/NPC", "Assets/Scripts/UI",
                "Assets/Scripts/Save"
            };
            foreach (var path in folders)
            {
                var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
                var name = Path.GetFileName(path);
                if (!AssetDatabase.IsValidFolder(path) && !string.IsNullOrEmpty(parent)) AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static void CreateWorld()
        {
            CreateSolid("Ground", new Vector2(0f, 0f), new Vector2(14f, 9f), new Color(0.25f, 0.5f, 0.25f), false);
            CreateSolid("Wall Top", new Vector2(0f, 4.5f), new Vector2(15f, 0.5f), Color.gray, true);
            CreateSolid("Wall Bottom", new Vector2(0f, -4.5f), new Vector2(15f, 0.5f), Color.gray, true);
            CreateSolid("Wall Left", new Vector2(-7.25f, 0f), new Vector2(0.5f, 9f), Color.gray, true);
            CreateSolid("Wall Right", new Vector2(7.25f, 0f), new Vector2(0.5f, 9f), Color.gray, true);
            var zone = CreateSolid("DamageZone", new Vector2(3f, -1f), new Vector2(1.5f, 1.5f), new Color(0.85f, 0.2f, 0.15f), false);
            var collider = EnsureComponent<BoxCollider2D>(zone);
            collider.isTrigger = true;
            EnsureComponent<DamageZone>(zone);
        }

        private static void CreateCollectibles()
        {
            var coin = EnsureRoot("Coin");
            coin.transform.position = new Vector3(-2f, 1f, 0f);
            ConfigureSprite(coin, Color.yellow, new Vector2(0.55f, 0.55f), 5);
            EnsureComponent<CircleCollider2D>(coin).isTrigger = true;
            EnsureComponent<CoinCollectible>(coin);

            var key = EnsureRoot("Key");
            key.transform.position = new Vector3(2f, 1.5f, 0f);
            ConfigureSprite(key, new Color(1f, 0.65f, 0.1f), new Vector2(0.75f, 0.35f), 5);
            EnsureComponent<BoxCollider2D>(key).isTrigger = true;
            EnsureComponent<KeyCollectible>(key);
        }

        private static void CreateHud()
        {
            var canvasObject = EnsureRoot("Canvas");
            var canvas = EnsureComponent<Canvas>(canvasObject);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            EnsureComponent<CanvasScaler>(canvasObject).uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            EnsureComponent<GraphicRaycaster>(canvasObject);

            var hudObject = EnsureChild(canvasObject.transform, "HUD");
            var hud = EnsureComponent<HUDController>(hudObject);
            var hearts = EnsureText(hudObject.transform, "Hearts", new Vector2(20f, -20f), "Heart: 3");
            var score = EnsureText(hudObject.transform, "Score", new Vector2(20f, -55f), "Score: 0");
            var gold = EnsureText(hudObject.transform, "Gold", new Vector2(20f, -90f), "Gold: 0");
            var key = EnsureText(hudObject.transform, "Key", new Vector2(20f, -125f), "Key: 0/1");
            var hudSerialized = new SerializedObject(hud);
            hudSerialized.FindProperty("heartsText").objectReferenceValue = hearts;
            hudSerialized.FindProperty("scoreText").objectReferenceValue = score;
            hudSerialized.FindProperty("goldText").objectReferenceValue = gold;
            hudSerialized.FindProperty("keyText").objectReferenceValue = key;
            hudSerialized.ApplyModifiedPropertiesWithoutUndo();

            var promptObject = EnsureChild(canvasObject.transform, "Interaction Prompt");
            var promptRect = EnsureComponent<RectTransform>(promptObject);
            promptRect.anchorMin = new Vector2(0.5f, 0f);
            promptRect.anchorMax = new Vector2(0.5f, 0f);
            promptRect.pivot = new Vector2(0.5f, 0f);
            promptRect.anchoredPosition = new Vector2(0f, 40f);
            promptRect.sizeDelta = new Vector2(300f, 45f);
            var promptText = EnsureText(promptObject.transform, "Label", Vector2.zero, "[E] TƯƠNG TÁC");
            var textRect = promptText.rectTransform;
            textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = Vector2.zero; textRect.offsetMax = Vector2.zero;
            promptText.alignment = TextAnchor.MiddleCenter;
            var prompt = EnsureComponent<InteractionPromptUI>(promptObject);
            var promptSerialized = new SerializedObject(prompt);
            promptSerialized.FindProperty("label").objectReferenceValue = promptText;
            promptSerialized.ApplyModifiedPropertiesWithoutUndo();

            var interactor = Object.FindAnyObjectByType<PlayerInteractor>();
            if (interactor != null)
            {
                var serialized = new SerializedObject(interactor);
                serialized.FindProperty("promptUI").objectReferenceValue = prompt;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            var eventSystem = EnsureRoot("EventSystem");
            EnsureComponent<EventSystem>(eventSystem);
            EnsureComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>(eventSystem);
        }

        private static GameObject CreateSolid(string name, Vector2 position, Vector2 size, Color color, bool collision)
        {
            var item = EnsureRoot(name);
            item.transform.position = position;
            ConfigureSprite(item, color, size, collision ? 1 : -5);
            var collider = item.GetComponent<BoxCollider2D>();
            if (collision) EnsureComponent<BoxCollider2D>(item).isTrigger = false;
            else if (collider != null && name != "DamageZone") Object.DestroyImmediate(collider);
            return item;
        }

        private static void ConfigureSprite(GameObject item, Color color, Vector2 size, int order)
        {
            var renderer = EnsureComponent<SpriteRenderer>(item);
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            renderer.color = color;
            renderer.sortingOrder = order;
            item.transform.localScale = new Vector3(size.x, size.y, 1f);
        }

        private static Text EnsureText(Transform parent, string name, Vector2 position, string content)
        {
            var item = EnsureChild(parent, name);
            var rect = EnsureComponent<RectTransform>(item);
            rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(0f, 1f); rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position; rect.sizeDelta = new Vector2(260f, 32f);
            var text = EnsureComponent<Text>(item);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 22; text.color = Color.white; text.text = content;
            return text;
        }

        private static GameObject EnsureRoot(string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null) return existing;
            return new GameObject(name);
        }

        private static GameObject EnsureChild(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.gameObject;
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child;
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            return target.TryGetComponent<T>(out var component) ? component : target.AddComponent<T>();
        }

        private static void AddToBuildSettings()
        {
            foreach (var scene in EditorBuildSettings.scenes) if (scene.path == ScenePath) return;
            var current = EditorBuildSettings.scenes;
            var updated = new EditorBuildSettingsScene[current.Length + 1];
            current.CopyTo(updated, 0);
            updated[^1] = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
#endif
