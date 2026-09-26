#if UNITY_EDITOR
using System.IO;
using MathAdventure.Collectibles;
using MathAdventure.Core;
using MathAdventure.Player;
using MathAdventure.UI;
using MathAdventure.World;
using MathAdventure.MathSystem;
using MathAdventure.NPC;
using MathAdventure.Objectives;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MathAdventure.Editor
{
    public static class MathAdventureProjectBuilder
    {
        private const string ScenePath = "Assets/Scenes/FoundationTest.unity";
        private const string Island01ScenePath = "Assets/Scenes/Island01.unity";
        private const string GeneratedArtPath = "Assets/Art/Generated";
        private const string PirateCatPath = "Assets/Art/Characters/PirateCat/PirateCat_Animation.png.png";
        private const string GoldCoinPath = "Assets/Art/Collectibles/GoldCoin/GoldCoin.png";
        private const string TinySwordsPath = "Assets/Art/Environment/Tiny Swords/Tiny Swords (Update 010)";

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
            AddToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("MathAdventure foundation scene created/repaired: " + ScenePath);
        }

        [MenuItem("MathAdventure/Build Island 1")]
        public static void BuildIsland01()
        {
            EnsureFolders();
            PrepareRealAssets();
            var scene = File.Exists(Island01ScenePath)
                ? EditorSceneManager.OpenScene(Island01ScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var managerObject = EnsureRoot("GameManager");
            EnsureComponent<GameManager>(managerObject);
            EnsureComponent<SceneLoader>(managerObject);

            var objectivesObject = EnsureRoot("Island 1 Objectives");
            var objectives = EnsureComponent<ObjectiveManager>(objectivesObject);
            var island = EnsureComponent<Island01Manager>(objectivesObject);

            var checkpoint = EnsureRoot("Start Checkpoint");
            checkpoint.transform.position = new Vector3(-13f, -7f, 0f);

            var player = EnsureRoot("Player");
            player.tag = "Player";
            player.transform.position = checkpoint.transform.position;
            var playerBody = EnsureComponent<Rigidbody2D>(player);
            playerBody.gravityScale = 0f;
            playerBody.freezeRotation = true;
            EnsureComponent<CapsuleCollider2D>(player).size = new Vector2(0.75f, 0.9f);
            EnsureComponent<PlayerController>(player);
            EnsureComponent<PlayerAnimator>(player);
            var health = EnsureComponent<PlayerHealth>(player);
            EnsureComponent<PlayerInventory>(player);
            EnsureComponent<PlayerInteractor>(player);
            ConfigureSprite(player, new Color(0.2f, 0.55f, 0.95f), new Vector2(0.8f, 1f), 20);

            var cameraObject = EnsureRoot("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10f);
            var camera = EnsureComponent<Camera>(cameraObject);
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            EnsureComponent<AudioListener>(cameraObject);
            EnsureComponent<CameraFollow>(cameraObject).SetTarget(player.transform);

            CreateIslandMap();
            CreateIslandGameplay(objectives, island);
            CreateHud();
            CreateIslandUI(objectives, island, player, health, checkpoint.transform);
            PolishIslandVisuals(player);
            IntegrateRealAssets(player);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, Island01ScenePath);
            AddToBuildSettings(Island01ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("MathAdventure Island 1 scene created/repaired: " + Island01ScenePath);
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

        private static void CreateIslandMap()
        {
            CreateSolid("Island Ground", Vector2.zero, new Vector2(32f, 20f), new Color(0.72f, 0.62f, 0.35f), false);
            CreateSolid("Boundary North", new Vector2(0f, 10.25f), new Vector2(33f, 0.5f), Color.gray, true);
            CreateSolid("Boundary South", new Vector2(0f, -10.25f), new Vector2(33f, 0.5f), Color.gray, true);
            CreateSolid("Boundary West", new Vector2(-16.25f, 0f), new Vector2(0.5f, 20f), Color.gray, true);
            CreateSolid("Boundary East", new Vector2(16.25f, 0f), new Vector2(0.5f, 20f), Color.gray, true);

            CreateSolid("Water North", new Vector2(-4f, 6f), new Vector2(3f, 8f), new Color(0.15f, 0.48f, 0.8f), true);
            CreateSolid("Water South", new Vector2(-4f, -5f), new Vector2(3f, 6f), new Color(0.15f, 0.48f, 0.8f), true);
            CreateSolid("Bridge", new Vector2(-4f, 1f), new Vector2(3f, 2f), new Color(0.45f, 0.28f, 0.12f), false);
            CreateSolid("Forest", new Vector2(1f, 6f), new Vector2(6f, 5f), new Color(0.16f, 0.42f, 0.18f), false);
            CreateSolid("Village", new Vector2(7f, 5f), new Vector2(5f, 4f), new Color(0.62f, 0.46f, 0.28f), false);
            CreateSolid("Trap Area", new Vector2(6f, 0f), new Vector2(7f, 4f), new Color(0.5f, 0.38f, 0.25f), false);
            CreateSolid("Key Area", new Vector2(11f, -3f), new Vector2(5f, 4f), new Color(0.36f, 0.55f, 0.25f), false);
            CreateSolid("Math Shrine Area", new Vector2(5f, -7f), new Vector2(6f, 4f), new Color(0.38f, 0.32f, 0.58f), false);
            CreateSolid("Treasure Area", new Vector2(13f, 7f), new Vector2(4f, 4f), new Color(0.7f, 0.52f, 0.18f), false);
        }

        private static void CreateIslandGameplay(ObjectiveManager objectives, Island01Manager island)
        {
            Vector2[] coinPositions = {
                new(-11f, -2f), new(-9f, 2f), new(-12f, 6f), new(-6f, 1f), new(0f, 3f),
                new(2f, 7f), new(6f, 6f), new(8f, 1f), new(12f, 1f), new(8f, -5f)
            };
            for (var i = 0; i < coinPositions.Length; i++)
            {
                var coin = EnsureRoot($"Coin {i + 1:00}");
                coin.SetActive(true);
                coin.transform.position = coinPositions[i];
                ConfigureSprite(coin, Color.yellow, new Vector2(0.55f, 0.55f), 12);
                EnsureComponent<CircleCollider2D>(coin).isTrigger = true;
                EnsureComponent<CoinCollectible>(coin);
            }

            Vector2[] trapPositions = { new(4f, 0f), new(7f, -1f), new(9f, 1f) };
            for (var i = 0; i < trapPositions.Length; i++)
            {
                var trap = CreateSolid($"DamageZone {i + 1}", trapPositions[i], new Vector2(1.2f, 1.2f), new Color(0.85f, 0.18f, 0.12f), false);
                EnsureComponent<BoxCollider2D>(trap).isTrigger = true;
                EnsureComponent<DamageZone>(trap);
            }

            var key = EnsureRoot("Island Key");
            key.SetActive(true);
            key.transform.position = new Vector3(11f, -3f, 0f);
            ConfigureSprite(key, new Color(1f, 0.65f, 0.1f), new Vector2(0.8f, 0.4f), 12);
            EnsureComponent<BoxCollider2D>(key).isTrigger = true;
            EnsureComponent<KeyCollectible>(key);

            var npc = EnsureRoot("Island Guide NPC");
            npc.transform.position = new Vector3(-10f, -5f, 0f);
            ConfigureSprite(npc, new Color(0.9f, 0.45f, 0.25f), new Vector2(1f, 1.2f), 11);
            var npcTrigger = EnsureComponent<CircleCollider2D>(npc);
            npcTrigger.radius = 1.6f;
            npcTrigger.isTrigger = true;
            EnsureComponent<NpcController>(npc);

            var shrine = EnsureRoot("Math Shrine");
            shrine.transform.position = new Vector3(5f, -7f, 0f);
            ConfigureSprite(shrine, new Color(0.65f, 0.4f, 0.9f), new Vector2(1.5f, 1.8f), 11);
            var shrineTrigger = EnsureComponent<BoxCollider2D>(shrine);
            shrineTrigger.size = new Vector2(1.8f, 1.8f);
            shrineTrigger.isTrigger = true;
            EnsureComponent<MathShrine>(shrine);

            var chest = EnsureRoot("Final Chest");
            chest.transform.position = new Vector3(13f, 7f, 0f);
            ConfigureSprite(chest, new Color(0.72f, 0.35f, 0.08f), new Vector2(1.6f, 1.1f), 11);
            var chestTrigger = EnsureComponent<BoxCollider2D>(chest);
            chestTrigger.size = new Vector2(1.8f, 1.6f);
            chestTrigger.isTrigger = true;
            EnsureComponent<FinalChest>(chest);
        }

        private static void CreateIslandUI(ObjectiveManager objectives, Island01Manager island, GameObject player, PlayerHealth health, Transform checkpoint)
        {
            var canvas = GameObject.Find("Canvas");
            if (canvas == null) return;

            var hud = canvas.transform.Find("HUD");
            var objectiveText = EnsureText(hud, "Objective", new Vector2(-20f, -20f), "NHIỆM VỤ\nGặp người dân trên đảo");
            objectiveText.alignment = TextAnchor.UpperRight;
            objectiveText.rectTransform.anchorMin = new Vector2(1f, 1f);
            objectiveText.rectTransform.anchorMax = new Vector2(1f, 1f);
            objectiveText.rectTransform.pivot = new Vector2(1f, 1f);
            objectiveText.rectTransform.sizeDelta = new Vector2(380f, 70f);
            var objectiveUI = EnsureComponent<ObjectiveUI>(objectiveText.gameObject);
            SetReference(objectiveUI, "objectives", objectives);
            SetReference(objectiveUI, "objectiveText", objectiveText);

            var dialoguePanel = EnsurePanel(canvas.transform, "Dialogue Panel", new Vector2(640f, 190f), new Vector2(0f, -30f));
            dialoguePanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0f);
            dialoguePanel.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0f);
            dialoguePanel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
            var dialogueText = EnsureCenteredText(dialoguePanel.transform, "Message", new Vector2(0f, 30f), new Vector2(580f, 100f), string.Empty, 22);
            var dialogueClose = EnsureButton(dialoguePanel.transform, "Close", new Vector2(0f, -55f), new Vector2(150f, 42f), "ĐÓNG");
            var dialogue = EnsureComponent<DialogueUI>(dialoguePanel);
            SetReference(dialogue, "messageText", dialogueText);
            SetReference(dialogue, "closeButton", dialogueClose);

            var introPanel = EnsurePanel(canvas.transform, "Island Intro", new Vector2(760f, 560f), Vector2.zero);
            EnsureCenteredText(introPanel.transform, "Title", new Vector2(0f, 210f), new Vector2(700f, 80f), "ISLAND 1\nĐẢO KHỞI ĐẦU", 34);
            EnsureCenteredText(introPanel.transform, "Story", new Vector2(0f, 70f), new Vector2(680f, 170f), "Sau một chuyến hải trình dài, Pirate Cat đặt chân lên\nhòn đảo đầu tiên của hành trình săn kho báu.\nHãy khám phá hòn đảo, thu thập vật phẩm,\ntìm chìa khóa và vượt qua thử thách toán.", 21);
            EnsureCenteredText(introPanel.transform, "Goals", new Vector2(0f, -75f), new Vector2(500f, 120f), "MỤC TIÊU\n• Thu thập xu   • Tìm chìa khóa\n• Giải toán      • Mở kho báu", 20);
            var mapButton = EnsureButton(introPanel.transform, "Map Button", new Vector2(-130f, -220f), new Vector2(220f, 48f), "XEM BẢN ĐỒ");
            var startButton = EnsureButton(introPanel.transform, "Start Button", new Vector2(130f, -220f), new Vector2(220f, 48f), "BẮT ĐẦU");
            var mapPanel = EnsurePanel(introPanel.transform, "Map Preview", new Vector2(650f, 390f), Vector2.zero);
            EnsureCenteredText(mapPanel.transform, "Map Text", Vector2.zero, new Vector2(600f, 340f), "BẢN ĐỒ ĐẢO 1\n\nStart ↗ NPC ↗ Forest\n  ↘ Coin — Bridge — Village\n                 ↘ Trap — Key\n                    ↙ Đền Toán\n                         ↗ Kho báu", 24);
            mapPanel.SetActive(false);
            var intro = EnsureComponent<IslandIntroUI>(introPanel);
            SetReference(intro, "mapButton", mapButton);
            SetReference(intro, "startButton", startButton);
            SetReference(intro, "mapPanel", mapPanel);
            introPanel.SetActive(true);

            var mathPanel = EnsurePanel(canvas.transform, "Math Question", new Vector2(620f, 450f), Vector2.zero);
            var questionText = EnsureCenteredText(mathPanel.transform, "Question", new Vector2(0f, 145f), new Vector2(560f, 80f), "8 + 5 = ?", 42);
            var feedbackText = EnsureCenteredText(mathPanel.transform, "Feedback", new Vector2(0f, -85f), new Vector2(400f, 50f), string.Empty, 26);
            var answers = new Button[4];
            answers[0] = EnsureButton(mathPanel.transform, "Answer 1", new Vector2(-140f, 45f), new Vector2(220f, 60f), "0");
            answers[1] = EnsureButton(mathPanel.transform, "Answer 2", new Vector2(140f, 45f), new Vector2(220f, 60f), "0");
            answers[2] = EnsureButton(mathPanel.transform, "Answer 3", new Vector2(-140f, -30f), new Vector2(220f, 60f), "0");
            answers[3] = EnsureButton(mathPanel.transform, "Answer 4", new Vector2(140f, -30f), new Vector2(220f, 60f), "0");
            var continueMath = EnsureButton(mathPanel.transform, "Continue", new Vector2(0f, -160f), new Vector2(220f, 48f), "TIẾP TỤC");
            var mathUI = EnsureComponent<MathQuestionUI>(mathPanel);
            SetReference(mathUI, "questionText", questionText);
            SetReference(mathUI, "feedbackText", feedbackText);
            SetReferenceArray(mathUI, "answerButtons", answers);
            SetReference(mathUI, "continueButton", continueMath);
            mathPanel.SetActive(false);

            var mathManagerObject = EnsureRoot("Math Question Manager");
            var mathManager = EnsureComponent<MathQuestionManager>(mathManagerObject);
            SetReference(mathManager, "questionUI", mathUI);
            SetReference(mathManager, "objectives", objectives);

            var completionPanel = EnsurePanel(canvas.transform, "Island Completion", new Vector2(620f, 460f), Vector2.zero);
            var summary = EnsureCenteredText(completionPanel.transform, "Summary", new Vector2(0f, 40f), new Vector2(540f, 300f), "HOÀN THÀNH ĐẢO 1!", 28);
            var continueIsland = EnsureButton(completionPanel.transform, "Continue", new Vector2(0f, -175f), new Vector2(220f, 50f), "TIẾP TỤC");
            var completion = EnsureComponent<CompletionUI>(completionPanel);
            SetReference(completion, "summaryText", summary);
            SetReference(completion, "continueButton", continueIsland);
            SetReference(completion, "island", island);
            completionPanel.SetActive(false);

            var deathPanel = EnsurePanel(canvas.transform, "Death Panel", new Vector2(380f, 150f), Vector2.zero);
            EnsureCenteredText(deathPanel.transform, "Death Text", Vector2.zero, new Vector2(340f, 100f), "HẾT TIM", 40);
            deathPanel.SetActive(false);

            var npc = Object.FindAnyObjectByType<NpcController>();
            SetReference(npc, "objectives", objectives);
            SetReference(npc, "dialogueUI", dialogue);
            var shrine = Object.FindAnyObjectByType<MathShrine>();
            SetReference(shrine, "mathManager", mathManager);
            var chest = Object.FindAnyObjectByType<FinalChest>();
            SetReference(chest, "objectives", objectives);
            SetReference(chest, "island", island);
            SetReference(chest, "dialogueUI", dialogue);

            SetReference(island, "playerHealth", health);
            SetReference(island, "player", player.transform);
            SetReference(island, "startCheckpoint", checkpoint);
            SetReference(island, "objectives", objectives);
            SetReference(island, "introUI", intro);
            SetReference(island, "completionUI", completion);
            SetReference(island, "deathPanel", deathPanel);
        }

        private static void PrepareRealAssets()
        {
            if (!AssetDatabase.IsValidFolder(GeneratedArtPath)) AssetDatabase.CreateFolder("Assets/Art", "Generated");

            SlicePirateCat();
            SliceGoldCoin();
            SliceUniformSheet(TinySwordsPath + "/Effects/Fire/Fire.png", "Fire_Frame", 7, 1, 100f);

            string[] directAssets = {
                TinySwordsPath + "/Terrain/Water/Water.png",
                TinySwordsPath + "/Terrain/Ground/Tilemap_Flat.png",
                TinySwordsPath + "/Terrain/Bridge/Bridge_All.png",
                TinySwordsPath + "/Terrain/Water/Foam/Foam.png",
                TinySwordsPath + "/Terrain/Water/Rocks/Rocks_01.png",
                TinySwordsPath + "/Resources/Trees/Tree.png",
                TinySwordsPath + "/Factions/Knights/Buildings/House/House_Blue.png",
                TinySwordsPath + "/Factions/Knights/Buildings/Tower/Tower_Blue.png",
                TinySwordsPath + "/Factions/Knights/Buildings/Castle/Castle_Blue.png",
                TinySwordsPath + "/Factions/Knights/Troops/Warrior/Blue/Warrior_Blue.png",
                TinySwordsPath + "/Factions/Goblins/Troops/Barrel/Blue/Barrel_Blue.png",
                TinySwordsPath + "/UI/Buttons/Button_Blue.png",
                TinySwordsPath + "/UI/Buttons/Button_Hover.png",
                TinySwordsPath + "/UI/Buttons/Button_Blue_Pressed.png"
            };
            foreach (var path in directAssets) ConfigurePixelTexture(path);

            CreatePirateAnimatorAssets();
            AssetDatabase.SaveAssets();
        }

        private static void SlicePirateCat()
        {
            string[] expected = { "Pirate_IdleDown_0", "Pirate_WalkDown_0", "Pirate_WalkUp_0", "Pirate_WalkLeft_0", "Pirate_WalkRight_0" };
            if (HasNamedSprites(PirateCatPath, expected)) { ConfigurePixelTexture(PirateCatPath, 100f); return; }

            int[] xMin = { 127, 424, 737, 1036 };
            int[] xMax = { 370, 660, 976, 1276 };
            int[] yMin = { 893, 668, 450, 233, 17 };
            int[] heights = { 226, 229, 224, 227, 228 };
            string[] rows = { "IdleDown", "WalkDown", "WalkUp", "WalkLeft", "WalkRight" };
            var rects = new System.Collections.Generic.List<SpriteRect>();
            for (var row = 0; row < rows.Length; row++)
            for (var column = 0; column < 4; column++)
                rects.Add(NewSpriteRect($"Pirate_{rows[row]}_{column}", new Rect(xMin[column], yMin[row], xMax[column] - xMin[column], heights[row]), SpriteAlignment.BottomCenter));
            ApplySpriteRects(PirateCatPath, rects, 100f);
        }

        private static void SliceGoldCoin()
        {
            if (HasNamedSprites(GoldCoinPath, new[] { "Coin_Frame_0", "Coin_Frame_7" })) { ConfigurePixelTexture(GoldCoinPath, 100f); return; }
            int[] xMin = { 7, 309, 567, 806, 991, 1184, 1417, 1685 };
            int[] xMax = { 295, 533, 756, 941, 1147, 1381, 1654, 1973 };
            var rects = new System.Collections.Generic.List<SpriteRect>();
            for (var i = 0; i < 8; i++) rects.Add(NewSpriteRect($"Coin_Frame_{i}", new Rect(xMin[i], 233, xMax[i] - xMin[i], 337), SpriteAlignment.Center));
            ApplySpriteRects(GoldCoinPath, rects, 100f);
        }

        private static void SliceUniformSheet(string path, string prefix, int columns, int rows, float pixelsPerUnit)
        {
            if (HasNamedSprites(path, new[] { prefix + "_0", prefix + "_" + (columns * rows - 1) })) { ConfigurePixelTexture(path, pixelsPerUnit); return; }
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null) return;
            var width = texture.width / columns;
            var height = texture.height / rows;
            var rects = new System.Collections.Generic.List<SpriteRect>();
            for (var row = 0; row < rows; row++)
            for (var column = 0; column < columns; column++)
                rects.Add(NewSpriteRect($"{prefix}_{row * columns + column}", new Rect(column * width, texture.height - (row + 1) * height, width, height), SpriteAlignment.Center));
            ApplySpriteRects(path, rects, pixelsPerUnit);
        }

        private static SpriteRect NewSpriteRect(string name, Rect rect, SpriteAlignment alignment)
        {
            return new SpriteRect { name = name, rect = rect, alignment = alignment, pivot = alignment == SpriteAlignment.BottomCenter ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f), spriteID = GUID.Generate() };
        }

        private static void ApplySpriteRects(string path, System.Collections.Generic.List<SpriteRect> rects, float pixelsPerUnit)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var factories = new SpriteDataProviderFactories();
            factories.Init();
            var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            provider.SetSpriteRects(rects.ToArray());
            if (provider.HasDataProvider(typeof(ISpriteNameFileIdDataProvider)))
            {
                var nameProvider = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
                var pairs = new System.Collections.Generic.List<SpriteNameFileIdPair>();
                foreach (var rect in rects) pairs.Add(new SpriteNameFileIdPair(rect.name, rect.spriteID));
                nameProvider.SetNameFileIdPairs(pairs);
            }
            provider.Apply();
            importer.SaveAndReimport();
        }

        private static void ConfigurePixelTexture(string path, float pixelsPerUnit = 100f)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer) return;
            var changed = importer.textureType != TextureImporterType.Sprite || importer.filterMode != FilterMode.Point || importer.mipmapEnabled || importer.textureCompression != TextureImporterCompression.Uncompressed || !Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            if (changed) importer.SaveAndReimport();
        }

        private static bool HasNamedSprites(string path, string[] names)
        {
            var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            foreach (var expected in names)
            {
                var found = false;
                foreach (var asset in assets) if (asset is Sprite && asset.name == expected) { found = true; break; }
                if (!found) return false;
            }
            return true;
        }

        private static void CreatePirateAnimatorAssets()
        {
            var idleDown = CreateSpriteClip(GeneratedArtPath + "/Pirate_IdleDown.anim", LoadSprites(PirateCatPath, "Pirate_IdleDown_"), 5f, true);
            var walkDown = CreateSpriteClip(GeneratedArtPath + "/Pirate_WalkDown.anim", LoadSprites(PirateCatPath, "Pirate_WalkDown_"), 8f, true);
            var walkUpSprites = LoadSprites(PirateCatPath, "Pirate_WalkUp_");
            var walkLeftSprites = LoadSprites(PirateCatPath, "Pirate_WalkLeft_");
            var walkRightSprites = LoadSprites(PirateCatPath, "Pirate_WalkRight_");
            var idleUp = CreateSpriteClip(GeneratedArtPath + "/Pirate_IdleUp.anim", FirstOnly(walkUpSprites), 5f, true);
            var idleLeft = CreateSpriteClip(GeneratedArtPath + "/Pirate_IdleLeft.anim", FirstOnly(walkLeftSprites), 5f, true);
            var idleRight = CreateSpriteClip(GeneratedArtPath + "/Pirate_IdleRight.anim", FirstOnly(walkRightSprites), 5f, true);
            var walkUp = CreateSpriteClip(GeneratedArtPath + "/Pirate_WalkUp.anim", walkUpSprites, 8f, true);
            var walkLeft = CreateSpriteClip(GeneratedArtPath + "/Pirate_WalkLeft.anim", walkLeftSprites, 8f, true);
            var walkRight = CreateSpriteClip(GeneratedArtPath + "/Pirate_WalkRight.anim", walkRightSprites, 8f, true);
            var controllerPath = GeneratedArtPath + "/PirateCat.controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null) return;

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
            controller.AddParameter("FacingX", AnimatorControllerParameterType.Float);
            controller.AddParameter("FacingY", AnimatorControllerParameterType.Float);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            var machine = controller.layers[0].stateMachine;
            foreach (var child in machine.states) machine.RemoveState(child.state);

            var idleTree = new BlendTree { name = "Idle Direction", blendType = BlendTreeType.SimpleDirectional2D, blendParameter = "FacingX", blendParameterY = "FacingY", useAutomaticThresholds = false };
            idleTree.AddChild(idleDown, Vector2.down); idleTree.AddChild(idleUp, Vector2.up); idleTree.AddChild(idleLeft, Vector2.left); idleTree.AddChild(idleRight, Vector2.right);
            AssetDatabase.AddObjectToAsset(idleTree, controller);
            var walkTree = new BlendTree { name = "Walk Direction", blendType = BlendTreeType.SimpleDirectional2D, blendParameter = "MoveX", blendParameterY = "MoveY", useAutomaticThresholds = false };
            walkTree.AddChild(walkDown, Vector2.down); walkTree.AddChild(walkUp, Vector2.up); walkTree.AddChild(walkLeft, Vector2.left); walkTree.AddChild(walkRight, Vector2.right);
            AssetDatabase.AddObjectToAsset(walkTree, controller);
            var idleState = machine.AddState("Idle"); idleState.motion = idleTree;
            var walkState = machine.AddState("Walk"); walkState.motion = walkTree;
            machine.defaultState = idleState;
            var toWalk = idleState.AddTransition(walkState); toWalk.hasExitTime = false; toWalk.duration = 0.05f; toWalk.AddCondition(AnimatorConditionMode.Greater, 0.01f, "Speed");
            var toIdle = walkState.AddTransition(idleState); toIdle.hasExitTime = false; toIdle.duration = 0.05f; toIdle.AddCondition(AnimatorConditionMode.Less, 0.01f, "Speed");
        }

        private static AnimationClip CreateSpriteClip(string path, Sprite[] sprites, float frameRate, bool loop)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) { clip = new AnimationClip { name = Path.GetFileNameWithoutExtension(path) }; AssetDatabase.CreateAsset(clip, path); }
            clip.frameRate = frameRate;
            var frames = new ObjectReferenceKeyframe[sprites.Length];
            for (var i = 0; i < sprites.Length; i++) frames[i] = new ObjectReferenceKeyframe { time = i / frameRate, value = sprites[i] };
            AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding { path = string.Empty, type = typeof(SpriteRenderer), propertyName = "m_Sprite" }, frames);
            var serialized = new SerializedObject(clip);
            var settings = serialized.FindProperty("m_AnimationClipSettings");
            settings.FindPropertyRelative("m_LoopTime").boolValue = loop;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static Sprite[] FirstOnly(Sprite[] sprites) => sprites.Length == 0 ? sprites : new[] { sprites[0] };

        private static Sprite[] LoadSprites(string path, string prefix)
        {
            var list = new System.Collections.Generic.List<Sprite>();
            foreach (var asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(path)) if (asset is Sprite sprite && sprite.name.StartsWith(prefix)) list.Add(sprite);
            list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            return list.ToArray();
        }

        private static Sprite LoadSprite(string path, string name = null)
        {
            Sprite largest = null;
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is not Sprite sprite) continue;
                if (!string.IsNullOrEmpty(name) && sprite.name == name) return sprite;
                if (largest == null || sprite.rect.width * sprite.rect.height > largest.rect.width * largest.rect.height) largest = sprite;
            }
            return largest;
        }

        private static void IntegrateRealAssets(GameObject player)
        {
            IntegratePirateCat(player);
            IntegrateCoinSprites();
            IntegrateTinySwordsTerrain();
            IntegrateTinySwordsProps();
            IntegrateTinySwordsUi();
        }

        private static void IntegratePirateCat(GameObject player)
        {
            var visual = player != null ? player.transform.Find("Visual") : null;
            if (visual == null) return;
            DisableRenderersBelow(visual);
            var renderer = EnsureComponent<SpriteRenderer>(visual.gameObject);
            renderer.enabled = true;
            renderer.color = Color.white;
            renderer.sprite = LoadSprite(PirateCatPath, "Pirate_IdleDown_0");
            renderer.sortingOrder = 1;
            visual.localScale = Vector3.one * 0.72f;
            visual.localPosition = new Vector3(0f, -0.43f, 0f);
            var animator = EnsureComponent<Animator>(visual.gameObject);
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(GeneratedArtPath + "/PirateCat.controller");
            var playerAnimator = player.GetComponent<PlayerAnimator>();
            SetReference(playerAnimator, "animator", animator);
            SetReference(playerAnimator, "visualRoot", visual);
        }

        private static void IntegrateCoinSprites()
        {
            var frames = LoadSprites(GoldCoinPath, "Coin_Frame_");
            for (var i = 1; i <= 10; i++)
            {
                var coin = GameObject.Find($"Coin {i:00}");
                var visual = coin != null ? coin.transform.Find("Visual") : null;
                if (visual == null) continue;
                DisableRenderersBelow(visual);
                var renderer = EnsureComponent<SpriteRenderer>(visual.gameObject);
                renderer.enabled = true; renderer.color = Color.white; renderer.sprite = frames.Length > 0 ? frames[0] : null; renderer.sortingOrder = 2;
                visual.localScale = Vector3.one * 0.34f;
                var animation = EnsureComponent<SpriteFrameAnimation>(visual.gameObject);
                SetReferenceArray(animation, "frames", frames);
                var effect = coin.GetComponent<CollectibleVisual>();
                SetReference(effect, "visualRoot", visual);
            }
        }

        private static void IntegrateTinySwordsTerrain()
        {
            var waterSprite = LoadSprite(TinySwordsPath + "/Terrain/Water/Water.png", "Water_0");
            var water = EnsureRoot("Tiny Swords Water");
            var waterRenderer = EnsureComponent<SpriteRenderer>(water);
            waterRenderer.sprite = waterSprite; waterRenderer.color = Color.white; waterRenderer.sortingOrder = -100; waterRenderer.drawMode = SpriteDrawMode.Tiled; waterRenderer.size = new Vector2(40f, 26f);
            water.transform.position = Vector3.zero;

            var groundSprite = LoadSprite(TinySwordsPath + "/Terrain/Ground/Tilemap_Flat.png", "Tilemap_Flat_0");
            var pathSprite = LoadSprite(TinySwordsPath + "/Terrain/Ground/Tilemap_Flat.png", "Tilemap_Flat_3");
            var groundRoot = EnsureRoot("Tiny Swords Island Ground");
            int[,] mask = {
                {0,0,1,1,1,1,1,0,0,0,0,0},
                {0,1,1,1,1,1,1,1,1,1,1,0},
                {1,1,1,1,1,1,1,1,1,1,1,1},
                {1,1,1,1,1,1,1,1,1,1,1,1},
                {1,1,1,1,1,1,1,1,1,1,1,1},
                {0,1,1,1,1,1,1,1,1,1,1,0},
                {0,0,1,1,1,1,1,1,1,1,0,0}
            };
            for (var y = 0; y < mask.GetLength(0); y++)
            for (var x = 0; x < mask.GetLength(1); x++)
            {
                var tile = EnsureWorldChild(groundRoot.transform, $"Ground {x}-{y}");
                tile.SetActive(mask[y, x] == 1);
                if (mask[y, x] == 0) continue;
                var renderer = EnsureComponent<SpriteRenderer>(tile);
                renderer.sprite = groundSprite; renderer.color = Color.white; renderer.sortingOrder = -80;
                tile.transform.localScale = Vector3.one;
                tile.transform.position = new Vector3(-13.2f + x * 2.4f, -7.2f + y * 2.4f, 0f);
            }
            DisableRenderer("Island Mass West"); DisableRenderer("Island Mass Center"); DisableRenderer("Island Mass East"); DisableRenderer("Island Mass North"); DisableRenderer("Island Mass South");

            foreach (var name in new[] { "Path Start", "Path Bridge", "Path Village", "Path Key", "Path Shrine", "Path Treasure" }) DisableRenderer(name);
            var pathRoot = EnsureRoot("Tiny Swords Island Paths");
            Vector2[] pathTiles = {
                new(-12f,-7f), new(-11f,-5.5f), new(-10f,-4f), new(-9f,-2.5f), new(-8f,-1f),
                new(-6.5f,0f), new(-5f,1f), new(-3.5f,1f), new(-2f,1.5f), new(0f,2.5f),
                new(2f,4f), new(4f,5f), new(6f,5f), new(7f,3f), new(7f,1f),
                new(8f,-1f), new(10f,-3f), new(8f,-4.5f), new(6f,-6f), new(5f,-7f),
                new(9f,1f), new(11f,3f), new(12f,5f), new(13f,7f)
            };
            for (var i = 0; i < pathTiles.Length; i++) CreateLooseSprite(pathRoot.transform, "Path Tile " + i, pathSprite, pathTiles[i], 0.72f, -60);

            ApplyRealSprite("Bridge", TinySwordsPath + "/Terrain/Bridge/Bridge_All.png", "Bridge_All_1", 1.45f, -15, Vector2.zero);
            var foamSprite = LoadSprite(TinySwordsPath + "/Terrain/Water/Foam/Foam.png");
            var foamRoot = EnsureRoot("Shore Foam");
            Vector2[] foamPositions = { new(-12f, -8.5f), new(-6f, 9f), new(2f, -9f), new(10f, 9f), new(14f, 2f) };
            for (var i = 0; i < foamPositions.Length; i++) CreateLooseSprite(foamRoot.transform, "Foam " + i, foamSprite, foamPositions[i], 0.8f, -70);
        }

        private static void IntegrateTinySwordsProps()
        {
            var treePath = TinySwordsPath + "/Resources/Trees/Tree.png";
            foreach (var name in new[] { "Tree A", "Tree B", "Tree C", "Tree D", "Tree E", "Tree F" }) ApplyRealSprite(name, treePath, "Tree_0", 1f, 0, Vector2.zero);
            var forestRoot = EnsureRoot("Tiny Swords Forest Edge");
            var tree = LoadSprite(treePath, "Tree_0");
            Vector2[] forestTrees = { new(-1f, 8.5f), new(1f, 8.8f), new(3f, 8.2f), new(-1.5f, 5.2f), new(3.2f, 5.1f), new(14f, 8.5f), new(15f, 6f) };
            for (var i = 0; i < forestTrees.Length; i++) CreateLooseSprite(forestRoot.transform, "Tree " + i, tree, forestTrees[i], 1f, 20 - Mathf.RoundToInt(forestTrees[i].y * 10f));

            var housePath = TinySwordsPath + "/Factions/Knights/Buildings/House/House_Blue.png";
            ApplyRealSprite("Village House A", housePath, "House_Blue_0", 1.15f, 0, Vector2.zero);
            ApplyRealSprite("Village House B", housePath, "House_Blue_0", 1.05f, 0, Vector2.zero);
            var barrelPath = TinySwordsPath + "/Factions/Goblins/Troops/Barrel/Blue/Barrel_Blue.png";
            ApplyRealSprite("Crate A", barrelPath, null, 0.72f, 0, Vector2.zero);
            ApplyRealSprite("Crate B", barrelPath, null, 0.72f, 0, Vector2.zero);

            ApplyRealSprite("Island Guide NPC", TinySwordsPath + "/Factions/Knights/Troops/Warrior/Blue/Warrior_Blue.png", "Warrior_Blue_0", 0.85f, 1, new Vector2(0f, -0.45f));
            ApplyRealSprite("Math Shrine", TinySwordsPath + "/Factions/Knights/Buildings/Tower/Tower_Blue.png", null, 1.05f, 0, new Vector2(0f, 0.15f));
            var castleRoot = EnsureRoot("Final Area Castle");
            castleRoot.transform.position = new Vector3(13f, 8.2f, 0f);
            var castleVisual = EnsureWorldChild(castleRoot.transform, "Real Visual");
            SetSprite(castleVisual, LoadSprite(TinySwordsPath + "/Factions/Knights/Buildings/Castle/Castle_Blue.png"), 0.9f, 20);
            EnsureComponent<SimpleYSort>(castleRoot);

            var fireFrames = LoadSprites(TinySwordsPath + "/Effects/Fire/Fire.png", "Fire_Frame_");
            for (var i = 1; i <= 3; i++)
            {
                var trap = GameObject.Find($"DamageZone {i}");
                var visual = trap != null ? trap.transform.Find("Visual") : null;
                if (visual == null) continue;
                DisableRenderersBelow(visual);
                var renderer = EnsureComponent<SpriteRenderer>(visual.gameObject);
                renderer.enabled = true; renderer.sprite = fireFrames.Length > 0 ? fireFrames[0] : null; renderer.color = Color.white; renderer.sortingOrder = 5;
                visual.localScale = Vector3.one * 0.72f;
                var animation = EnsureComponent<SpriteFrameAnimation>(visual.gameObject);
                SetReferenceArray(animation, "frames", fireFrames);
            }
        }

        private static void IntegrateTinySwordsUi()
        {
            var canvas = GameObject.Find("Canvas");
            if (canvas == null) return;
            var normal = LoadSprite(TinySwordsPath + "/UI/Buttons/Button_Blue.png");
            var hover = LoadSprite(TinySwordsPath + "/UI/Buttons/Button_Hover.png");
            var pressed = LoadSprite(TinySwordsPath + "/UI/Buttons/Button_Blue_Pressed.png");
            foreach (var button in canvas.GetComponentsInChildren<Button>(true))
            {
                var image = button.GetComponent<Image>();
                if (image != null && normal != null) { image.sprite = normal; image.type = Image.Type.Sliced; image.color = Color.white; }
                button.transition = Selectable.Transition.SpriteSwap;
                button.spriteState = new SpriteState { highlightedSprite = hover, selectedSprite = hover, pressedSprite = pressed, disabledSprite = normal };
            }
        }

        private static void ApplyRealSprite(string objectName, string assetPath, string spriteName, float scale, int order, Vector2 offset)
        {
            var root = GameObject.Find(objectName);
            if (root == null) return;
            DisableRenderersBelow(root.transform);
            var visual = EnsureWorldChild(root.transform, "Real Visual");
            visual.transform.localPosition = offset;
            SetSprite(visual, LoadSprite(assetPath, spriteName), scale, order);
        }

        private static void SetSprite(GameObject target, Sprite sprite, float scale, int order)
        {
            var renderer = EnsureComponent<SpriteRenderer>(target);
            renderer.enabled = true; renderer.sprite = sprite; renderer.color = Color.white; renderer.sortingOrder = order;
            target.transform.localScale = Vector3.one * scale;
        }

        private static void CreateLooseSprite(Transform parent, string name, Sprite sprite, Vector2 position, float scale, int order)
        {
            var item = EnsureWorldChild(parent, name);
            item.transform.position = position;
            SetSprite(item, sprite, scale, order);
        }

        private static void DisableRenderersBelow(Transform root)
        {
            foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(true)) renderer.enabled = false;
        }

        private static void PolishIslandVisuals(GameObject player)
        {
            var camera = Camera.main;
            if (camera != null)
            {
                camera.orthographicSize = 4.8f;
                camera.backgroundColor = new Color(0.08f, 0.34f, 0.56f);
            }

            DisableRenderer("Island Ground");
            DisableRenderer("Forest");
            DisableRenderer("Village");
            DisableRenderer("Trap Area");
            DisableRenderer("Key Area");
            DisableRenderer("Math Shrine Area");
            DisableRenderer("Treasure Area");
            SetRendererColor("Water North", new Color(0.08f, 0.34f, 0.56f));
            SetRendererColor("Water South", new Color(0.08f, 0.34f, 0.56f));

            CreateIslandMass("Island Mass West", new Vector2(-9f, 0f), new Vector2(14f, 18f), -8f);
            CreateIslandMass("Island Mass Center", new Vector2(0f, 1f), new Vector2(13f, 17f), 5f);
            CreateIslandMass("Island Mass East", new Vector2(9f, 1f), new Vector2(13f, 17f), -6f);
            CreateIslandMass("Island Mass North", new Vector2(4f, 7f), new Vector2(19f, 6f), 2f);
            CreateIslandMass("Island Mass South", new Vector2(2f, -7f), new Vector2(18f, 6f), -3f);

            CreatePath("Path Start", new Vector2(-10f, -4f), new Vector2(8f, 1.4f), 38f);
            CreatePath("Path Bridge", new Vector2(-3f, 1f), new Vector2(9f, 1.4f), 4f);
            CreatePath("Path Village", new Vector2(4f, 4f), new Vector2(9f, 1.4f), 28f);
            CreatePath("Path Key", new Vector2(8f, -2f), new Vector2(9f, 1.4f), -24f);
            CreatePath("Path Shrine", new Vector2(5f, -5f), new Vector2(7f, 1.4f), 84f);
            CreatePath("Path Treasure", new Vector2(11f, 4f), new Vector2(7f, 1.4f), 62f);
            BuildBridgeVisual();

            CreateTree("Tree A", new Vector2(-12f, 5f));
            CreateTree("Tree B", new Vector2(-9f, 7f));
            CreateTree("Tree C", new Vector2(0f, 7f));
            CreateTree("Tree D", new Vector2(2f, 5f));
            CreateTree("Tree E", new Vector2(10f, 8f));
            CreateTree("Tree F", new Vector2(14f, 3f));
            CreateBush("Bush A", new Vector2(-7f, -5f));
            CreateBush("Bush B", new Vector2(1f, 2f));
            CreateBush("Bush C", new Vector2(10f, -6f));
            CreateRock("Rock A", new Vector2(-13f, 1f));
            CreateRock("Rock B", new Vector2(3f, -2f));
            CreateRock("Rock C", new Vector2(14f, -5f));
            CreateHouse("Village House A", new Vector2(6f, 6f));
            CreateHouse("Village House B", new Vector2(9f, 5f));
            CreateCrate("Crate A", new Vector2(5f, 3f));
            CreateCrate("Crate B", new Vector2(12f, 6f));

            BuildPlayerVisual(player);
            BuildNpcVisual();
            BuildCollectibleVisuals();
            BuildTrapVisuals();
            BuildShrineVisual();
            BuildChestVisual();
            PolishUi();
        }

        private static void CreateIslandMass(string name, Vector2 position, Vector2 size, float rotation)
        {
            var item = EnsureRoot(name);
            item.transform.position = position;
            item.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
            ConfigureSprite(item, new Color(0.38f, 0.67f, 0.32f), size, -50);
        }

        private static void CreatePath(string name, Vector2 position, Vector2 size, float rotation)
        {
            var item = EnsureRoot(name);
            item.transform.position = position;
            item.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
            ConfigureSprite(item, new Color(0.77f, 0.66f, 0.39f), size, -35);
        }

        private static void BuildBridgeVisual()
        {
            var bridge = GameObject.Find("Bridge");
            if (bridge == null) return;
            DisableRenderer("Bridge");
            for (var i = -2; i <= 2; i++)
                ConfigureWorldSprite(EnsureWorldChild(bridge.transform, $"Plank {i + 3}"), new Color(0.48f, 0.28f, 0.12f), new Vector2(0.5f, 1.7f), -20, new Vector2(i * 0.52f, 0f));
            ConfigureWorldSprite(EnsureWorldChild(bridge.transform, "Rail Top"), new Color(0.25f, 0.14f, 0.07f), new Vector2(3f, 0.12f), -19, new Vector2(0f, 0.88f));
            ConfigureWorldSprite(EnsureWorldChild(bridge.transform, "Rail Bottom"), new Color(0.25f, 0.14f, 0.07f), new Vector2(3f, 0.12f), -19, new Vector2(0f, -0.88f));
        }

        private static void BuildPlayerVisual(GameObject player)
        {
            if (player == null) return;
            var rootRenderer = player.GetComponent<SpriteRenderer>();
            if (rootRenderer != null) rootRenderer.enabled = false;
            var visual = EnsureWorldChild(player.transform, "Visual");
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one * 1.35f;
            ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Shadow"), new Color(0.04f, 0.07f, 0.08f, 0.35f), new Vector2(0.72f, 0.25f), -2, new Vector2(0f, -0.43f));
            ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Body"), new Color(0.18f, 0.38f, 0.62f), new Vector2(0.62f, 0.62f), 1, new Vector2(0f, -0.1f));
            ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Head"), new Color(0.95f, 0.58f, 0.22f), new Vector2(0.68f, 0.58f), 2, new Vector2(0f, 0.35f));
            var earLeft = EnsureWorldChild(visual.transform, "Ear Left");
            earLeft.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            ConfigureWorldSprite(earLeft, new Color(0.95f, 0.58f, 0.22f), new Vector2(0.25f, 0.25f), 1, new Vector2(-0.22f, 0.67f));
            var earRight = EnsureWorldChild(visual.transform, "Ear Right");
            earRight.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            ConfigureWorldSprite(earRight, new Color(0.95f, 0.58f, 0.22f), new Vector2(0.25f, 0.25f), 1, new Vector2(0.22f, 0.67f));
            ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Eye Left"), Color.black, new Vector2(0.07f, 0.1f), 3, new Vector2(-0.14f, 0.38f));
            ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Eye Right"), Color.black, new Vector2(0.07f, 0.1f), 3, new Vector2(0.14f, 0.38f));
            ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Belt"), new Color(0.28f, 0.14f, 0.05f), new Vector2(0.65f, 0.1f), 3, new Vector2(0f, -0.08f));
            var animator = EnsureComponent<PlayerAnimator>(player);
            SetReference(animator, "visualRoot", visual.transform);
            EnsureComponent<DamageFlash>(player);
            EnsureComponent<SimpleYSort>(player);
        }

        private static void BuildNpcVisual()
        {
            var npc = GameObject.Find("Island Guide NPC");
            if (npc == null) return;
            var renderer = npc.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.enabled = false;
            var visual = EnsureWorldChild(npc.transform, "Visual");
            ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Body"), new Color(0.62f, 0.2f, 0.16f), new Vector2(0.75f, 0.85f), 0, new Vector2(0f, -0.15f));
            ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Head"), new Color(0.82f, 0.62f, 0.42f), new Vector2(0.64f, 0.6f), 1, new Vector2(0f, 0.55f));
            ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Quest Marker"), new Color(1f, 0.82f, 0.1f), new Vector2(0.22f, 0.55f), 2, new Vector2(0f, 1.25f));
            EnsureComponent<SimpleYSort>(npc);
        }

        private static void BuildCollectibleVisuals()
        {
            for (var i = 1; i <= 10; i++)
            {
                var coin = GameObject.Find($"Coin {i:00}");
                if (coin == null) continue;
                var renderer = coin.GetComponent<SpriteRenderer>();
                if (renderer != null) renderer.enabled = false;
                var visual = EnsureWorldChild(coin.transform, "Visual");
                ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Outer"), new Color(1f, 0.7f, 0.05f), new Vector2(0.7f, 0.7f), 0, Vector2.zero);
                ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Inner"), new Color(1f, 0.9f, 0.3f), new Vector2(0.42f, 0.42f), 1, Vector2.zero);
                var effect = EnsureComponent<CollectibleVisual>(coin);
                SetReference(effect, "visualRoot", visual.transform);
            }

            var key = GameObject.Find("Island Key");
            if (key == null) return;
            var keyRenderer = key.GetComponent<SpriteRenderer>();
            if (keyRenderer != null) keyRenderer.enabled = false;
            var keyVisual = EnsureWorldChild(key.transform, "Visual");
            ConfigureWorldSprite(EnsureWorldChild(keyVisual.transform, "Ring"), new Color(1f, 0.78f, 0.08f), new Vector2(0.52f, 0.52f), 1, new Vector2(-0.3f, 0.1f));
            ConfigureWorldSprite(EnsureWorldChild(keyVisual.transform, "Shaft"), new Color(1f, 0.72f, 0.05f), new Vector2(0.75f, 0.18f), 2, new Vector2(0.25f, 0.1f));
            ConfigureWorldSprite(EnsureWorldChild(keyVisual.transform, "Tooth"), new Color(1f, 0.72f, 0.05f), new Vector2(0.18f, 0.38f), 2, new Vector2(0.5f, -0.05f));
            var keyEffect = EnsureComponent<CollectibleVisual>(key);
            SetReference(keyEffect, "visualRoot", keyVisual.transform);
            SetBool(keyEffect, "pulse", true);
        }

        private static void BuildTrapVisuals()
        {
            for (var i = 1; i <= 3; i++)
            {
                var trap = GameObject.Find($"DamageZone {i}");
                if (trap == null) continue;
                var renderer = trap.GetComponent<SpriteRenderer>();
                if (renderer != null) renderer.enabled = false;
                var visual = EnsureWorldChild(trap.transform, "Visual");
                for (var spike = -1; spike <= 1; spike++)
                {
                    var piece = EnsureWorldChild(visual.transform, $"Spike {spike + 2}");
                    piece.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                    ConfigureWorldSprite(piece, new Color(0.78f, 0.82f, 0.85f), new Vector2(0.36f, 0.36f), 1, new Vector2(spike * 0.36f, 0f));
                }
                ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Warning"), new Color(0.8f, 0.16f, 0.1f), new Vector2(1.25f, 0.12f), 0, new Vector2(0f, -0.35f));
            }
        }

        private static void BuildShrineVisual()
        {
            var shrine = GameObject.Find("Math Shrine");
            if (shrine == null) return;
            var renderer = shrine.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.enabled = false;
            var visual = EnsureWorldChild(shrine.transform, "Visual");
            ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Base"), new Color(0.32f, 0.25f, 0.48f), new Vector2(1.7f, 0.45f), 0, new Vector2(0f, -0.55f));
            ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Pillar Left"), new Color(0.62f, 0.56f, 0.76f), new Vector2(0.35f, 1.5f), 1, new Vector2(-0.55f, 0.1f));
            ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Pillar Right"), new Color(0.62f, 0.56f, 0.76f), new Vector2(0.35f, 1.5f), 1, new Vector2(0.55f, 0.1f));
            ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Rune"), new Color(0.35f, 0.9f, 1f), new Vector2(0.5f, 0.5f), 2, new Vector2(0f, 0.35f));
        }

        private static void BuildChestVisual()
        {
            var chest = GameObject.Find("Final Chest");
            if (chest == null) return;
            var renderer = chest.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.enabled = false;
            var visual = EnsureWorldChild(chest.transform, "Visual");
            var glowObject = EnsureWorldChild(visual.transform, "Glow");
            var glow = ConfigureWorldSprite(glowObject, new Color(1f, 0.9f, 0.2f, 0f), new Vector2(2.1f, 1.7f), -1, Vector2.zero);
            ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Body"), new Color(0.48f, 0.22f, 0.06f), new Vector2(1.5f, 0.72f), 0, new Vector2(0f, -0.22f));
            var lid = EnsureWorldChild(visual.transform, "Lid");
            ConfigureWorldSprite(lid, new Color(0.7f, 0.36f, 0.08f), new Vector2(1.5f, 0.45f), 1, new Vector2(0f, 0.35f));
            ConfigureWorldSprite(EnsureWorldChild(visual.transform, "Lock"), new Color(1f, 0.75f, 0.12f), new Vector2(0.25f, 0.35f), 2, new Vector2(0f, -0.05f));
            var chestVisual = EnsureComponent<ChestVisual>(chest);
            SetReference(chestVisual, "lid", lid.transform);
            SetReference(chestVisual, "glow", glow);
            var chestLogic = EnsureComponent<FinalChest>(chest);
            SetReference(chestLogic, "chestVisual", chestVisual);
            EnsureComponent<SimpleYSort>(chest);
        }

        private static void PolishUi()
        {
            var canvas = GameObject.Find("Canvas");
            if (canvas == null) return;
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null) { scaler.referenceResolution = new Vector2(1280f, 720f); scaler.matchWidthOrHeight = 0.5f; }

            var hud = canvas.transform.Find("HUD");
            if (hud != null)
            {
                var statsPanel = EnsurePanel(hud, "Stats Backdrop", new Vector2(245f, 168f), new Vector2(16f, -16f));
                var statsRect = statsPanel.GetComponent<RectTransform>();
                statsRect.anchorMin = Vector2.up; statsRect.anchorMax = Vector2.up; statsRect.pivot = Vector2.up;
                statsRect.anchoredPosition = new Vector2(12f, -12f);
                statsPanel.GetComponent<Image>().color = new Color(0.03f, 0.08f, 0.12f, 0.82f);
                statsPanel.transform.SetAsFirstSibling();
                var objectivePanel = EnsurePanel(hud, "Objective Backdrop", new Vector2(400f, 98f), new Vector2(-12f, -12f));
                var objectiveRect = objectivePanel.GetComponent<RectTransform>();
                objectiveRect.anchorMin = Vector2.one; objectiveRect.anchorMax = Vector2.one; objectiveRect.pivot = Vector2.one;
                objectiveRect.anchoredPosition = new Vector2(-12f, -12f);
                objectivePanel.GetComponent<Image>().color = new Color(0.03f, 0.08f, 0.12f, 0.82f);
                objectivePanel.transform.SetSiblingIndex(1);
            }

            var prompt = canvas.transform.Find("Interaction Prompt");
            if (prompt != null)
            {
                var image = EnsureComponent<Image>(prompt.gameObject);
                image.color = new Color(0.02f, 0.05f, 0.08f, 0.9f);
                prompt.GetComponent<RectTransform>().sizeDelta = new Vector2(360f, 54f);
                var label = prompt.GetComponentInChildren<Text>(true);
                if (label != null) { label.fontSize = 24; label.fontStyle = FontStyle.Bold; }
            }

            var dialogue = canvas.transform.Find("Dialogue Panel");
            if (dialogue != null)
            {
                var rect = dialogue.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(820f, 220f);
                rect.anchoredPosition = new Vector2(0f, 22f);
                EnsureCenteredText(dialogue, "Npc Name", new Vector2(-310f, 78f), new Vector2(180f, 40f), "NGƯỜI DÂN ĐẢO", 22).color = new Color(1f, 0.78f, 0.25f);
                EnsureComponent<CanvasGroup>(dialogue.gameObject);
                EnsureComponent<PanelFade>(dialogue.gameObject);
            }

            var math = canvas.transform.Find("Math Question");
            if (math != null)
            {
                math.GetComponent<RectTransform>().sizeDelta = new Vector2(760f, 500f);
                EnsureCenteredText(math, "Header", new Vector2(0f, 205f), new Vector2(620f, 45f), "THỬ THÁCH TOÁN", 26).color = new Color(1f, 0.78f, 0.22f);
                EnsureComponent<CanvasGroup>(math.gameObject);
                EnsureComponent<PanelFade>(math.gameObject);
            }

            var intro = canvas.transform.Find("Island Intro");
            if (intro != null)
            {
                intro.GetComponent<Image>().color = new Color(0.04f, 0.12f, 0.16f, 0.97f);
                EnsureComponent<CanvasGroup>(intro.gameObject);
                EnsureComponent<PanelFade>(intro.gameObject);
            }

            var map = intro != null ? intro.Find("Map Preview") : null;
            if (map != null) BuildMapPreview(map);

            foreach (var button in canvas.GetComponentsInChildren<Button>(true))
            {
                var colors = button.colors;
                colors.normalColor = new Color(0.18f, 0.48f, 0.68f);
                colors.highlightedColor = new Color(0.28f, 0.68f, 0.86f);
                colors.pressedColor = new Color(0.1f, 0.32f, 0.5f);
                colors.selectedColor = colors.highlightedColor;
                colors.fadeDuration = 0.08f;
                button.colors = colors;
            }
        }

        private static void BuildMapPreview(Transform map)
        {
            var oldText = map.Find("Map Text");
            if (oldText != null) oldText.gameObject.SetActive(false);
            var island = EnsurePanel(map, "Island Shape", new Vector2(500f, 245f), Vector2.zero);
            island.GetComponent<Image>().color = new Color(0.3f, 0.62f, 0.3f, 1f);
            CreateMapMarker(island.transform, "START", new Vector2(-190f, -75f), new Color(0.2f, 0.8f, 1f));
            CreateMapMarker(island.transform, "KEY", new Vector2(115f, -45f), new Color(1f, 0.78f, 0.1f));
            CreateMapMarker(island.transform, "MATH", new Vector2(20f, -92f), new Color(0.65f, 0.4f, 1f));
            CreateMapMarker(island.transform, "FINAL", new Vector2(190f, 80f), new Color(1f, 0.45f, 0.12f));
            EnsureCenteredText(map, "Map Header", new Vector2(0f, 165f), new Vector2(500f, 42f), "BẢN ĐỒ ĐẢO 1", 26);
        }

        private static void CreateMapMarker(Transform parent, string label, Vector2 position, Color color)
        {
            var marker = EnsurePanel(parent, label + " Marker", new Vector2(70f, 34f), position);
            marker.GetComponent<Image>().color = color;
            EnsureCenteredText(marker.transform, "Label", Vector2.zero, new Vector2(70f, 34f), label, 14).color = Color.black;
        }

        private static void CreateTree(string name, Vector2 position)
        {
            var root = EnsureRoot(name);
            root.transform.position = position;
            ConfigureWorldSprite(EnsureWorldChild(root.transform, "Trunk"), new Color(0.34f, 0.17f, 0.07f), new Vector2(0.45f, 1.2f), 0, new Vector2(0f, 0.15f));
            ConfigureWorldSprite(EnsureWorldChild(root.transform, "Canopy Left"), new Color(0.08f, 0.38f, 0.15f), new Vector2(1.2f, 1.2f), 1, new Vector2(-0.38f, 1f));
            ConfigureWorldSprite(EnsureWorldChild(root.transform, "Canopy Right"), new Color(0.1f, 0.48f, 0.18f), new Vector2(1.2f, 1.2f), 2, new Vector2(0.4f, 1.05f));
            ConfigureWorldSprite(EnsureWorldChild(root.transform, "Canopy Top"), new Color(0.14f, 0.55f, 0.2f), new Vector2(1.1f, 1.1f), 3, new Vector2(0f, 1.55f));
            EnsureComponent<SimpleYSort>(root);
        }

        private static void CreateBush(string name, Vector2 position)
        {
            var root = EnsureRoot(name);
            root.transform.position = position;
            ConfigureWorldSprite(EnsureWorldChild(root.transform, "Left"), new Color(0.12f, 0.48f, 0.18f), new Vector2(0.8f, 0.65f), 0, new Vector2(-0.3f, 0f));
            ConfigureWorldSprite(EnsureWorldChild(root.transform, "Right"), new Color(0.16f, 0.58f, 0.22f), new Vector2(0.8f, 0.65f), 1, new Vector2(0.3f, 0f));
            EnsureComponent<SimpleYSort>(root);
        }

        private static void CreateRock(string name, Vector2 position)
        {
            var root = EnsureRoot(name);
            root.transform.position = position;
            root.transform.rotation = Quaternion.Euler(0f, 0f, 12f);
            ConfigureWorldSprite(EnsureWorldChild(root.transform, "Rock"), new Color(0.38f, 0.43f, 0.45f), new Vector2(1f, 0.7f), 0, Vector2.zero);
            EnsureComponent<SimpleYSort>(root);
        }

        private static void CreateHouse(string name, Vector2 position)
        {
            var root = EnsureRoot(name);
            root.transform.position = position;
            ConfigureWorldSprite(EnsureWorldChild(root.transform, "Wall"), new Color(0.78f, 0.58f, 0.32f), new Vector2(2.1f, 1.6f), 0, new Vector2(0f, 0.3f));
            var roof = EnsureWorldChild(root.transform, "Roof");
            roof.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            ConfigureWorldSprite(roof, new Color(0.48f, 0.16f, 0.1f), new Vector2(1.65f, 1.65f), 1, new Vector2(0f, 1.25f));
            ConfigureWorldSprite(EnsureWorldChild(root.transform, "Door"), new Color(0.28f, 0.13f, 0.06f), new Vector2(0.55f, 0.9f), 2, new Vector2(0f, 0.05f));
            EnsureComponent<SimpleYSort>(root);
        }

        private static void CreateCrate(string name, Vector2 position)
        {
            var root = EnsureRoot(name);
            root.transform.position = position;
            ConfigureWorldSprite(EnsureWorldChild(root.transform, "Crate"), new Color(0.5f, 0.28f, 0.1f), new Vector2(0.8f, 0.8f), 0, Vector2.zero);
            var brace = EnsureWorldChild(root.transform, "Brace");
            brace.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            ConfigureWorldSprite(brace, new Color(0.72f, 0.45f, 0.2f), new Vector2(0.12f, 0.9f), 1, Vector2.zero);
            EnsureComponent<SimpleYSort>(root);
        }

        private static GameObject EnsureWorldChild(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing.gameObject;
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static SpriteRenderer ConfigureWorldSprite(GameObject item, Color color, Vector2 size, int order, Vector2 localPosition)
        {
            var renderer = EnsureComponent<SpriteRenderer>(item);
            renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            renderer.color = color;
            renderer.sortingOrder = order;
            item.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
            item.transform.localScale = new Vector3(size.x, size.y, 1f);
            return renderer;
        }

        private static void DisableRenderer(string objectName)
        {
            var item = GameObject.Find(objectName);
            if (item != null && item.TryGetComponent<SpriteRenderer>(out var renderer)) renderer.enabled = false;
        }

        private static void SetRendererColor(string objectName, Color color)
        {
            var item = GameObject.Find(objectName);
            if (item != null && item.TryGetComponent<SpriteRenderer>(out var renderer)) renderer.color = color;
        }

        private static void SetBool(Object target, string propertyName, bool value)
        {
            if (target == null) return;
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            if (property == null) return;
            property.boolValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
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

        private static GameObject EnsurePanel(Transform parent, string name, Vector2 size, Vector2 position)
        {
            var panel = EnsureChild(parent, name);
            var rect = EnsureComponent<RectTransform>(panel);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = EnsureComponent<Image>(panel);
            image.color = new Color(0.05f, 0.08f, 0.12f, 0.94f);
            return panel;
        }

        private static Text EnsureCenteredText(Transform parent, string name, Vector2 position, Vector2 size, string content, int fontSize)
        {
            var text = EnsureText(parent, name, position, content);
            text.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            text.rectTransform.anchoredPosition = position;
            text.rectTransform.sizeDelta = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = fontSize;
            return text;
        }

        private static Button EnsureButton(Transform parent, string name, Vector2 position, Vector2 size, string label)
        {
            var buttonObject = EnsureChild(parent, name);
            var rect = EnsureComponent<RectTransform>(buttonObject);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = EnsureComponent<Image>(buttonObject);
            image.color = new Color(0.18f, 0.5f, 0.72f, 1f);
            var button = EnsureComponent<Button>(buttonObject);
            button.targetGraphic = image;
            var text = EnsureCenteredText(buttonObject.transform, "Label", Vector2.zero, size, label, 20);
            text.raycastTarget = false;
            return button;
        }

        private static void SetReference(Object target, string propertyName, Object value)
        {
            if (target == null) return;
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            if (property == null) return;
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetReferenceArray(Object target, string propertyName, Object[] values)
        {
            if (target == null) return;
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            if (property == null) return;
            property.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
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

        private static void AddToBuildSettings(string scenePath)
        {
            foreach (var scene in EditorBuildSettings.scenes) if (scene.path == scenePath) return;
            var current = EditorBuildSettings.scenes;
            var updated = new EditorBuildSettingsScene[current.Length + 1];
            current.CopyTo(updated, 0);
            updated[^1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
#endif
