using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Kingmaker.View.Equipment;
using Kingmaker.Visual.Particles;

namespace AstartesCustodes.Editor
{
    internal static class CastellanAxeGenerator
    {
        internal const string WeaponGuid = "107fbf6e543d41c9ab5783dd7f761be6";
        internal const string OnslaughtGuid = "9c5b8ea2bcef4dc0ad202ec700dd3efd";
        internal const string SweepGuid = "34c7aa1147264785b5aedcbf982d06d8";

        internal static readonly string[] WeaponGuids =
        {
            WeaponGuid, "0ea483280c3c4f68b2894debf9f5930c", "53fd9030471a47ec832b7f3a9bf5d889",
            "b81b2c7c002e44dc8a6a711c82fdb2d7", "3b5e0fc217f441ed815b70dcdffdc72a", "c2e60e63e79749a9a71fcbff8db63d59"
        };
        private static readonly string[] OnslaughtAbilityGuids =
        {
            OnslaughtGuid, "03d73fd86e524494a9e4a1f20e567569", "54842b70fdb64db1b120d572c5149272",
            "8a26289d72ee49adb389de5d0d224232", "67c809f5dd1043ccbf58cc33c4e01f47", "ed307b84a1484fc09019bc13dd2a1e33"
        };
        private static readonly string[] SweepAbilityGuids =
        {
            SweepGuid, "283ef8148c7847579fcc787c094f1599", "513e89c1e3e14eaa914fe3eeef6dabba",
            "e825e5286147400981bdc8476c88fd75", "8fdcda7691db4e67a7ae3521124ca6fe", "6545ed149a83456f900003dc510661d5"
        };
        private static readonly string[] ExecutionerBoltAbilityGuids =
        {
            "67b07b4445b348168d63903592a5ee50", "b78d52e76e84478f8868bef1e882cf6f", "3ae119d574d54650a27e75904c81e1de",
            "841570aa8b6249bba3dbd46175d4946b", "1462ce0843384098a4fb2c3f5a4246c9", "5635cd61d63541258a977ffeb7134026"
        };
        private static readonly string[] HaftStrikeAbilityGuids =
        {
            "1b35d60012964a81a6a425f1b444f2d7", "48b061760b4f46a188958c6e4e633d49", "4f669f32e5654868a844afb076f1d489",
            "17c894e7b08942acaf1a4c9a1186fd97", "484a1624ace4416fb2e604cd53c26786", "f4cd6ca5c1c74efa852db7b2cda0ecff"
        };

        private static readonly string[] OnslaughtWeaponGuids =
        {
            "8bdfb393b52546d8ad15fe00e16ae41e", "787b6361b4174161b7ea493c01473119", "ad55f0fd65ba4a45ae7f7d9ce4d572e6",
            "595ee70e3e6e4495ada28307f5694fcc", "f8666bbfe66149ddb19a249b7b8475ae", "e92057e911104048b352c04b6fd43481"
        };
        private static readonly string[] SweepWeaponGuids =
        {
            "1a154907fa884e9d96ee41991500ed61", "dacd9a5fd053427885120cf0a41c6e1f", "a9d4685015a14989be6cc01c88236521",
            "fb993ac6a51f41469214c9e5436712fc", "bbd546647bad44e4b5a896b20bebf1db", "803ed9c24f13455f933276103a0e814a"
        };
        private static readonly string[] ExecutionerBoltWeaponGuids =
        {
            "454b060041f44c21887f2ea1b69534ca", "289ba63662fc4cb0a62777bd891795de", "4cf95953444c41f88b38907c776c5f80",
            "37eea0b441e345ea94a7f74388ea25d7", "ff6c4ad4134d4fe3852c7e7c8dcd77e4", "1eed7697f46342038869764652480a08"
        };
        private static readonly string[] HaftStrikeWeaponGuids =
        {
            "fae72fb29fa34f6ebb750bf4ec38717c", "33d6548b8e5c4d839c4bdf10dd89d64a", "54fd783bf420415b911fb6f2e84b2954",
            "a328a2018afa4002acfdbdb05c162d0b", "cc4f3316203c4b759bfaff482fc75ab0", "a80ed97220284d1bbec66fecc7aa9099"
        };
        private static readonly int[] StrikeMin = { 14, 19, 24, 29, 39, 51 };
        private static readonly int[] StrikeMax = { 20, 27, 34, 40, 53, 69 };
        private static readonly int[] StrikePen = { 25, 30, 35, 40, 45, 50 };
        private static readonly int[] OnslaughtMin = { 9, 13, 16, 14, 18, 24 };
        private static readonly int[] OnslaughtMax = { 13, 18, 22, 20, 25, 32 };
        private static readonly int[] OnslaughtPen = { 20, 25, 30, 35, 40, 45 };
        private static readonly int[] SweepMin = { 11, 14, 18, 22, 29, 38 };
        private static readonly int[] SweepMax = { 15, 20, 26, 30, 40, 52 };
        private static readonly int[] SweepPen = { 15, 20, 25, 30, 35, 40 };
        private static readonly int[] HaftMin = { 7, 9, 12, 14, 19, 25 };
        private static readonly int[] HaftMax = { 10, 13, 17, 20, 26, 34 };
        private const string EvisceratorTripleAttack = "a6ec10d23b0f49698e17f30e70423615";
        private const string GreatSword = "88863b6b0c61404b96b01c2bc648ba5e";
        private const string SmallCleave = "bac8a9c632934bec87c72fece5831673";
        private const string PreciseShot = "8fe7633db25d46a8bebc2868b8acff12";
        private const string HammerPush = "bd5f109c46684a09b07fb13743d2a6d7";
        private const string PushAdditionalEffect = "1b19d696bfb0417a9ba9b93b85474b76";
        private const string MacesMeleeAttack = "638cd0973175462b9faaeb1242761d32";
        private const string HaftStrikePushEffectGuid = "8072aab2b2f04707ae2c4b782148016a";
        private const string PowerAxeFx = "5a72a93ee3bd4a049aa4ddef1e4c7f84";
        private const string BluntFx = "b42607999b9740c1ac74fc7d63ae0451";
        private const string BolterFx = "afde0e8c0c9848deba8e38a1279ee7df";
        private const string StandardReload = "98f4a31b68e446ad9c63411c7b349146";

        private const string Root = "Assets/Modifications/AstartesCustodes";
        private const string Art = Root + "/Art";
        private const string Blueprints = Root + "/Blueprints";
        private const string SourceBlueprint = Blueprints + "/GuardianSpear_Prototype_Item.jbp";
        private const string GuardianCleaveBlueprint = Blueprints + "/GuardianSpear_Cleave_Ability.jbp";
        private const string SentinelCleaveBlueprint = Blueprints + "/SentinelSword_Cleave_Ability.jbp";
        private const string OutputBlueprint = Blueprints + "/CastellanAxe_Prototype_Item.jbp";
        private const string FbxPath = Art + "/CastellanAxe.fbx";
        private const string PrefabPath = Art + "/CastellanAxe.prefab";
        private const string BaseColorPath = Art + "/CastellanAxe_BaseColor.png";
        private const string MetallicPath = Art + "/CastellanAxe_Metallic.png";
        private const string RoughnessPath = Art + "/CastellanAxe_Roughness.png";
        private const string NormalSourcePath = Art + "/CastellanAxe_Normal_Source.png";
        private const string MaterialPath = Art + "/CastellanAxe.mat";
        private const string PackedMaskPath = Art + "/CastellanAxe_MetallicSmoothness.asset";
        private const string PackedNormalPath = Art + "/CastellanAxe_Normal.asset";
        private const string InventoryIconPath = Art + "/CastellanAxe_InventoryIcon.png";
        private const string BolterMuzzleLocator = "502467bbbcc0471285a4ab6936a285d8";

        [MenuItem("Astartes Custodes/Generate Castellan Axe")]
        public static void Generate()
        {
            Directory.CreateDirectory(Art);
            Directory.CreateDirectory(Blueprints);
            GenerateArt();
            GenerateCombatBlueprints();
            GenerateBlueprint();
            AddLocalization();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AstartesCustodes] Castellan Axe V1-V6 generated: " + WeaponGuid);
        }

        private static void GenerateArt()
        {
            AssetDatabase.ImportAsset(FbxPath, ImportAssetOptions.ForceSynchronousImport);
            ModelImporter importer = AssetImporter.GetAtPath(FbxPath) as ModelImporter;
            if (importer == null) throw new InvalidDataException("CastellanAxe.fbx could not be imported.");
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.SaveAndReimport();

            GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
            if (fbx == null) throw new InvalidDataException("Castellan Axe FBX is missing after import.");
            Material material = CreateMaterial();

            GameObject root = new GameObject("CastellanAxe_Root");
            EquipmentOffsets offsets = root.AddComponent<EquipmentOffsets>();
            ConfigureBackSheathOffsets(offsets);
            AddComponentByName(root, "FxLocatorMapper", false);
            AddComponentByName(root, "AstartesCustodes.Runtime.GuardianSpearMaterialBinder", true);
            AddComponentByName(root, "AstartesCustodes.Runtime.GuardianSpearShotPoseController", true);

            // Retain the Guardian Spear pose hierarchy so its hybrid shot handling can be reused.
            GameObject visual = new GameObject("GuardianSpear_Visual");
            visual.transform.SetParent(root.transform, false);
            GameObject model = UnityEngine.Object.Instantiate(fbx);
            model.name = "CastellanAxe_FBX_Model";
            model.transform.SetParent(visual.transform, false);
            // BrutalTwoHanded inverts the apparent weapon axis relative to the prefab convention.
            // Move 0.84 m from the previous test in the opposite direction, leaving the mesh
            // roughly 0.42 m beyond its original centre on the desired side of the fixed grip.
            // Restore the v1.3.7 longitudinal/depth position. In BrutalTwoHanded the remaining
            // local Z axis is the true lateral grip axis; use it to shift the shaft left without
            // changing its height or front/back placement.
            // Final grip alignment: 5 cm forward along the compensated depth axis and 3 cm
            // right along the true lateral Z axis, relative to the approved v1.3.13 pose.
            model.transform.localPosition = new Vector3(0.335f, 0.265f, 0.02f);
            // Meshy's long axis is +X. The shared pose root adds the remaining 45 degrees at
            // runtime, so this 45-degree model rotation produces the Spear's final +Y axis.
            // Flip the complete weapon end-for-end: the axe head belongs above the upper hand,
            // with the pommel extending downwards like the vanilla Omnissian Axe stance.
            Quaternion alignAxe = Quaternion.Euler(0f, 0f, 225f);
            // Turn the axe head 55 degrees around the shaft relative to the original 95-degree
            // grip alignment, presenting the blade cleanly through the attack swing.
            Quaternion turnBladeForward = Quaternion.AngleAxis(150f, Vector3.right);
            model.transform.localRotation = alignAxe * turnBladeForward;
            // Blender exports this Meshy FBX in centimetres. Restore a roughly 1.9 m weapon.
            model.transform.localScale = Vector3.one * 100f;
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterial = material;

            GameObject muzzle = new GameObject("GuardianSpear_BolterMuzzle");
            muzzle.transform.SetParent(visual.transform, false);
            // Initial estimate at the integrated bolter's forward end; tune after the visual test.
            muzzle.transform.localPosition = new Vector3(-0.55f, -0.55f, 0.12f);
            FxLocator locator = muzzle.AddComponent<FxLocator>();
            SerializedObject locatorObject = new SerializedObject(locator);
            locatorObject.FindProperty("m_Group.guid").stringValue = BolterMuzzleLocator;
            locatorObject.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void ConfigureBackSheathOffsets(EquipmentOffsets offsets)
        {
            SerializedObject serialized = new SerializedObject(offsets);
            SerializedProperty slots = serialized.FindProperty("m_SlotOffsets");
            slots.arraySize = 12;
            for (int i = 0; i < slots.arraySize; i++)
            {
                SerializedProperty slot = slots.GetArrayElementAtIndex(i);
                slot.FindPropertyRelative("Position").vector3Value = Vector3.zero;
                slot.FindPropertyRelative("Rotation").vector3Value = Vector3.zero;
            }
            SetBackSlot(slots, 6, new Vector3(0.18f, -0.18f, -0.12f));
            SetBackSlot(slots, 8, new Vector3(-0.18f, -0.18f, -0.12f));
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetBackSlot(SerializedProperty slots, int index, Vector3 position)
        {
            SerializedProperty slot = slots.GetArrayElementAtIndex(index);
            slot.FindPropertyRelative("Position").vector3Value = position;
            // Holster-only half-turn from the Spear baseline: reverse the shaft so the heavy
            // axe head points down while leaving the drawn BrutalTwoHanded pose untouched.
            slot.FindPropertyRelative("Rotation").vector3Value = new Vector3(90f, 0f, 0f);
        }

        private static void AddComponentByName(GameObject host, string fullName, bool required)
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => { try { return assembly.GetTypes(); } catch { return Type.EmptyTypes; } })
                .FirstOrDefault(candidate => candidate.FullName == fullName || candidate.Name == fullName);
            if (type == null)
            {
                if (required) throw new InvalidOperationException(fullName + " was not found.");
                return;
            }
            host.AddComponent(type);
        }

        private static Material CreateMaterial()
        {
            AssetDatabase.ImportAsset(BaseColorPath, ImportAssetOptions.ForceSynchronousImport);
            Texture2D baseColor = AssetDatabase.LoadAssetAtPath<Texture2D>(BaseColorPath);
            if (baseColor == null) throw new InvalidDataException("Castellan Axe base colour is missing.");
            Texture2D metallic = LoadPng(MetallicPath, true);
            Texture2D roughness = LoadPng(RoughnessPath, true);
            Texture2D normalSource = LoadPng(NormalSourcePath, true);
            Texture2D mask = PackMetallicSmoothness(metallic, roughness);
            Texture2D normal = PackUnityNormal(normalSource);

            Shader shader = Shader.Find("Owlcat/Lit");
            if (shader == null) throw new InvalidOperationException("Owlcat/Lit is unavailable.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "CastellanAxe" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else material.shader = shader;
            SetTexture(material, baseColor, "_BaseMap", "_BaseColorMap", "_MainTex");
            SetTexture(material, mask, "_MetallicGlossMap", "_MaskMap", "_MasksMap");
            SetTexture(material, normal, "_BumpMap", "_NormalMap");
            SetFloat(material, 1f, "_Metallic");
            SetFloat(material, 1f, "_Smoothness");
            SetFloat(material, 0.24f, "_Roughness");
            SetColor(material, new Color(1.15f, 1.15f, 1.15f, 1f), "_BaseColor", "_Color", "_AdditionalAlbedoColor");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Texture2D LoadPng(string path, bool linear)
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, true, linear);
            if (!texture.LoadImage(File.ReadAllBytes(path), false)) throw new InvalidDataException("Could not decode " + path);
            return texture;
        }

        private static Texture2D PackMetallicSmoothness(Texture2D metallic, Texture2D roughness)
        {
            if (metallic.width != roughness.width || metallic.height != roughness.height)
                throw new InvalidDataException("Castellan Axe metallic and roughness sizes differ.");
            Color32[] metal = metallic.GetPixels32();
            Color32[] rough = roughness.GetPixels32();
            for (int i = 0; i < metal.Length; i++) metal[i] = new Color32(metal[i].r, 0, 0, (byte)(255 - rough[i].r));
            Texture2D packed = new Texture2D(metallic.width, metallic.height, TextureFormat.RGBA32, true, true)
                { name = "CastellanAxe_MetallicSmoothness" };
            packed.SetPixels32(metal);
            packed.Apply(true, false);
            return ReplaceTextureAsset(packed, PackedMaskPath);
        }

        private static Texture2D PackUnityNormal(Texture2D source)
        {
            Color32[] pixels = source.GetPixels32();
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, pixels[i].g, 255, pixels[i].r);
            Texture2D packed = new Texture2D(source.width, source.height, TextureFormat.RGBA32, true, true)
                { name = "CastellanAxe_Normal" };
            packed.SetPixels32(pixels);
            packed.Apply(true, false);
            return ReplaceTextureAsset(packed, PackedNormalPath);
        }

        private static Texture2D ReplaceTextureAsset(Texture2D source, string path)
        {
            Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing == null) { AssetDatabase.CreateAsset(source, path); return source; }
            EditorUtility.CopySerialized(source, existing);
            UnityEngine.Object.DestroyImmediate(source);
            return existing;
        }

        private static void SetTexture(Material material, Texture texture, params string[] names)
        { foreach (string name in names) if (material.HasProperty(name)) material.SetTexture(name, texture); }

        private static void SetFloat(Material material, float value, params string[] names)
        { foreach (string name in names) if (material.HasProperty(name)) material.SetFloat(name, value); }

        private static void SetColor(Material material, Color value, params string[] names)
        { foreach (string name in names) if (material.HasProperty(name)) material.SetColor(name, value); }

        private static void GenerateBlueprint()
        {
            UnityEngine.Object prefab = AssetDatabase.LoadMainAssetAtPath(PrefabPath);
            if (prefab == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string prefabGuid, out long prefabFileId))
                throw new InvalidDataException("Castellan Axe prefab could not be resolved.");
            UnityEngine.Object inventoryIcon = PrepareInventoryIcon();
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(inventoryIcon, out string iconGuid, out long iconFileId))
                throw new InvalidDataException("Castellan Axe inventory icon could not be resolved.");
            for (int i = 0; i < WeaponGuids.Length; i++)
            {
                int tier = i + 1;
                string source = tier == 1 ? SourceBlueprint : Blueprints + $"/GuardianSpear_V{tier}_Item.jbp";
                JObject weapon = JObject.Parse(File.ReadAllText(source));
                weapon["AssetId"] = WeaponGuids[i];
                weapon["Data"]["m_DisplayName"] = GuardianSpearGenerator.Localized($"castellan-axe-v{tier}-name");
                weapon["Data"]["m_Description"] = GuardianSpearGenerator.Localized($"castellan-axe-v{tier}-desc");
                weapon["Data"]["m_FlavorText"] = GuardianSpearGenerator.Localized("castellan-axe-flavor");
                weapon["Data"]["m_VisualParameters"]["m_WeaponAnimationStyle"] = "BrutalTwoHanded";
                GuardianSpearGenerator.AddOverride(weapon, "m_DisplayName");
                GuardianSpearGenerator.AddOverride(weapon, "m_Description");
                GuardianSpearGenerator.AddOverride(weapon, "m_FlavorText");
                GuardianSpearGenerator.AddOverride(weapon, "m_VisualParameters.m_WeaponAnimationStyle");
                weapon["Data"]["m_VisualParameters"]["m_WeaponModel"] = new JObject { ["guid"] = prefabGuid, ["fileid"] = prefabFileId };
                GuardianSpearGenerator.AddOverride(weapon, "m_VisualParameters.m_WeaponModel");
                weapon["Data"]["m_Icon"] = new JObject { ["guid"] = iconGuid, ["fileid"] = iconFileId };
                GuardianSpearGenerator.AddOverride(weapon, "m_Icon");
                GuardianSpearGenerator.Override(weapon, "WarhammerDamage", StrikeMin[i]);
                GuardianSpearGenerator.Override(weapon, "WarhammerMaxDamage", StrikeMax[i]);
                GuardianSpearGenerator.Override(weapon, "WarhammerPenetration", StrikePen[i]);
                GuardianSpearGenerator.Override(weapon, "WarhammerMaxAmmo", 0);
                GuardianSpearGenerator.SetAbilitySlot(weapon, "Ability1", "SingleShot", GuardianSpearGenerator.GuardianStrike, PowerAxeFx, 1);
                GuardianSpearGenerator.SetAbilitySlot(weapon, "Ability2", "SingleShot", OnslaughtAbilityGuids[i], PowerAxeFx, 2);
                GuardianSpearGenerator.SetAbilitySlot(weapon, "Ability3", "AOE", SweepAbilityGuids[i], PowerAxeFx, 2);
                GuardianSpearGenerator.SetAbilitySlot(weapon, "Ability4", "SingleShot", ExecutionerBoltAbilityGuids[i], BolterFx, 2);
                GuardianSpearGenerator.SetAbilitySlot(weapon, "Ability5", "SingleShot", HaftStrikeAbilityGuids[i], BluntFx, 1);
                JObject haftSlot = (JObject)weapon["Data"]["AbilityContainer"]["Ability5"];
                haftSlot["OnHitOverrideType"] = "Add";
                haftSlot["m_OnHitActions"] = "!bp_" + HaftStrikePushEffectGuid;
                GuardianSpearGenerator.AddOverride(weapon, "WeaponAbilities.Ability5.OnHitOverrideType");
                GuardianSpearGenerator.AddOverride(weapon, "WeaponAbilities.Ability5.m_OnHitActions");
                weapon["Data"]["m_AttackOfOpportunityAbility"] = "!bp_" + GuardianSpearGenerator.GuardianStrike;
                GuardianSpearGenerator.AddOverride(weapon, "m_AttackOfOpportunityAbility");
                string output = tier == 1 ? OutputBlueprint : Blueprints + $"/CastellanAxe_V{tier}_Item.jbp";
                File.WriteAllText(output, weapon.ToString(Formatting.Indented));
            }
        }

        private static UnityEngine.Object PrepareInventoryIcon()
        {
            AssetDatabase.ImportAsset(InventoryIconPath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(InventoryIconPath) as TextureImporter;
            if (importer == null) throw new InvalidDataException("Castellan Axe inventory icon importer was not found.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(InventoryIconPath).FirstOrDefault(asset => asset is Sprite)
                ?? AssetDatabase.LoadMainAssetAtPath(InventoryIconPath);
        }

        private static void GenerateCombatBlueprints()
        {
            GenerateHaftStrikePushEffect();
            for (int i = 0; i < WeaponGuids.Length; i++)
            {
                int tier = i + 1;
                CreateHiddenMeleeWeapon(OnslaughtWeaponGuids[i], OnslaughtMin[i], OnslaughtMax[i], OnslaughtPen[i], $"CastellanAxe_V{tier}_HiddenOnslaught_Item");
                CreateHiddenMeleeWeapon(SweepWeaponGuids[i], SweepMin[i], SweepMax[i], SweepPen[i], $"CastellanAxe_V{tier}_HiddenSweep_Item");
                CreateHiddenMeleeWeapon(HaftStrikeWeaponGuids[i], HaftMin[i], HaftMax[i], StrikePen[i], $"CastellanAxe_V{tier}_HiddenHaftStrike_Item");
                CreateExecutionerBoltWeapon(i);
                JObject onslaught = GuardianSpearGenerator.PrepareClone(
                    GuardianSpearGenerator.Load(EvisceratorTripleAttack), OnslaughtAbilityGuids[i], EvisceratorTripleAttack);
                JObject repeat = (JObject)onslaught["Data"]["Components"].Children<JObject>()
                    .SelectMany(component => component.DescendantsAndSelf().OfType<JObject>())
                    .First(component => component["$type"]?.ToString().Contains("ContextActionRepeat") == true);
                repeat["RepeatNumber"]["Value"] = tier >= 4 ? 3 : 2;
                AddWeaponOverride(onslaught, OnslaughtWeaponGuids[i], $"$WarhammerOverrideAbilityWeapon$castellan-axe-v{tier}-onslaught");
                onslaught["Data"]["m_DisplayName"] = GuardianSpearGenerator.Localized("castellan-onslaught-name");
                onslaught["Data"]["m_Description"] = GuardianSpearGenerator.Localized($"castellan-onslaught-v{tier}-desc");
                onslaught["Data"]["m_Icon"] = new JObject { ["guid"] = "a6cba97367839af4e8869281de029095", ["fileid"] = 21300000L };
                GuardianSpearGenerator.AddOverride(onslaught, "m_DisplayName");
                GuardianSpearGenerator.AddOverride(onslaught, "m_Description");
                GuardianSpearGenerator.AddOverride(onslaught, "m_Icon");
                GuardianSpearGenerator.Save($"CastellanAxe_V{tier}_Onslaught_Ability", onslaught);

                JObject sweep = GuardianSpearGenerator.PrepareClone(
                    JObject.Parse(File.ReadAllText(SentinelCleaveBlueprint)), SweepAbilityGuids[i], SmallCleave);
                JObject oldOverride = sweep["Data"]["Components"].Children<JObject>()
                    .First(component => component["$type"]?.ToString().Contains("WarhammerOverrideAbilityWeapon") == true);
                ((JArray)sweep["Data"]["Components"]).Remove(oldOverride);
                ((JArray)sweep["Data"]["m_Overrides"]).Remove(oldOverride["name"]?.ToString());
                AddWeaponOverride(sweep, SweepWeaponGuids[i], $"$WarhammerOverrideAbilityWeapon$castellan-axe-v{tier}-sweep");
                sweep["Data"]["m_DisplayName"] = GuardianSpearGenerator.Localized("castellan-sweep-name");
                sweep["Data"]["m_Description"] = GuardianSpearGenerator.Localized($"castellan-sweep-v{tier}-desc");
                GuardianSpearGenerator.AddOverride(sweep, "m_DisplayName");
                GuardianSpearGenerator.AddOverride(sweep, "m_Description");
                GuardianSpearGenerator.Save($"CastellanAxe_V{tier}_Sweep_Ability", sweep);

                GenerateExecutionerBoltAbility(i);
                GenerateHaftStrikeAbility(i);
            }
        }

        private static void GenerateHaftStrikePushEffect()
        {
            JObject effect = GuardianSpearGenerator.PrepareClone(
                GuardianSpearGenerator.Load(PushAdditionalEffect), HaftStrikePushEffectGuid, PushAdditionalEffect);
            JObject push = effect["Data"]["OnHitActions"]["Actions"].Children<JObject>()
                .First(action => action["$type"]?.ToString().Contains("ContextActionPush") == true);
            push["Cells"]["ValueType"] = "Simple";
            push["Cells"]["Value"] = 1;
            push["Cells"]["Property"] = "None";
            push["Cells"]["m_CustomProperty"] = null;
            GuardianSpearGenerator.Save("CastellanAxe_HaftStrike_PushEffect", effect);
        }

        private static void CreateExecutionerBoltWeapon(int i)
        {
            int tier = i + 1;
            string source = tier == 1
                ? Blueprints + "/GuardianSpear_HiddenBolter_Item.jbp"
                : Blueprints + $"/GuardianSpear_V{tier}_HiddenShot_Item.jbp";
            JObject sourceWeapon = JObject.Parse(File.ReadAllText(source));
            string prototype = sourceWeapon["AssetId"].ToString();
            JObject weapon = GuardianSpearGenerator.PrepareClone(sourceWeapon, ExecutionerBoltWeaponGuids[i], prototype);
            GuardianSpearGenerator.Override(weapon, "WarhammerMaxAmmo", 0);
            GuardianSpearGenerator.Save($"CastellanAxe_V{tier}_HiddenExecutionerBolt_Item", weapon);
        }

        private static void GenerateExecutionerBoltAbility(int i)
        {
            int tier = i + 1;
            JObject ability = GuardianSpearGenerator.PrepareClone(
                GuardianSpearGenerator.Load(PreciseShot), ExecutionerBoltAbilityGuids[i], PreciseShot);
            AddWeaponOverride(ability, ExecutionerBoltWeaponGuids[i], $"$WarhammerOverrideAbilityWeapon$castellan-axe-v{tier}-executioner-bolt");
            ability["Data"]["m_DisplayName"] = GuardianSpearGenerator.Localized("castellan-executioner-bolt-name");
            ability["Data"]["m_Description"] = GuardianSpearGenerator.Localized($"castellan-executioner-bolt-v{tier}-desc");
            ability["Data"]["CooldownRounds"] = 2;
            ability["Data"]["m_Icon"] = new JObject { ["guid"] = "3282527c29982764b8f36f5cf4c60a49", ["fileid"] = 21300000L };
            foreach (string property in new[] { "m_DisplayName", "m_Description", "CooldownRounds", "m_Icon" })
                GuardianSpearGenerator.AddOverride(ability, property);
            GuardianSpearGenerator.Save($"CastellanAxe_V{tier}_ExecutionerBolt_Ability", ability);
        }

        private static void GenerateHaftStrikeAbility(int i)
        {
            int tier = i + 1;
            JObject ability = GuardianSpearGenerator.PrepareClone(
                GuardianSpearGenerator.Load(HammerPush), HaftStrikeAbilityGuids[i], HammerPush);
            AddWeaponOverride(ability, HaftStrikeWeaponGuids[i], $"$WarhammerOverrideAbilityWeapon$castellan-axe-v{tier}-haft-strike");
            AddHaftStrikeAnimationAndStun(ability, tier);
            ability["Data"]["m_DisplayName"] = GuardianSpearGenerator.Localized("castellan-haft-strike-name");
            ability["Data"]["m_Description"] = GuardianSpearGenerator.Localized($"castellan-haft-strike-v{tier}-desc");
            ability["Data"]["ActionPointCost"] = 1;
            ability["Data"]["m_Icon"] = new JObject { ["guid"] = "dc6c9d334c722964d9c668867857a87a", ["fileid"] = 21300000L };
            foreach (string property in new[] { "m_DisplayName", "m_Description", "ActionPointCost", "m_Icon" })
                GuardianSpearGenerator.AddOverride(ability, property);
            GuardianSpearGenerator.Save($"CastellanAxe_V{tier}_HaftStrike_Ability", ability);
        }

        private static void AddHaftStrikeAnimationAndStun(JObject ability, int tier)
        {
            JObject alternativeAnimation = new JObject
            {
                ["$type"] = "dbc2c558fd814daaa755adc5dc92a1f8, WarhammerAttackAlternativeAnimationStyle",
                ["name"] = $"$WarhammerAttackAlternativeAnimationStyle$castellan-axe-v{tier}-haft-strike",
                ["m_Flags"] = 0,
                ["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" },
                ["m_Overrides"] = new JArray(),
                ["m_WeaponAnimationStyle"] = "Staff"
            };
            ((JArray)ability["Data"]["Components"]).Add(alternativeAnimation);
            GuardianSpearGenerator.AddOverride(ability, alternativeAnimation["name"].ToString());

            JObject maceAttack = GuardianSpearGenerator.Load(MacesMeleeAttack);
            JObject sourceRunAction = maceAttack["Data"]["Components"].Children<JObject>()
                .First(component => component["$type"]?.ToString().Contains("AbilityEffectRunAction") == true);
            JObject runAction = (JObject)sourceRunAction.DeepClone();
            string runActionName = $"$AbilityEffectRunAction$castellan-axe-v{tier}-haft-stun";
            runAction["name"] = runActionName;
            runAction["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            runAction["m_Overrides"] = new JArray();
            JObject dodgeActions = runAction.DescendantsAndSelf().OfType<JObject>()
                .First(action => action["$type"]?.ToString().Contains("DodgeActions") == true);
            JObject stunSave = (JObject)dodgeActions.DescendantsAndSelf().OfType<JObject>()
                .First(action => action["$type"]?.ToString().Contains("ContextActionSavingThrow") == true)
                .DeepClone();
            stunSave["Type"] = "Fortitude";
            dodgeActions["ActionsOnHit"]["Actions"] = new JArray(stunSave);
            dodgeActions["ActionsOnDodge"]["Actions"] = new JArray();
            ((JArray)ability["Data"]["Components"]).Add(runAction);
            GuardianSpearGenerator.AddOverride(ability, runActionName);
        }

        private static void CreateHiddenMeleeWeapon(string guid, int min, int max, int penetration, string fileName)
        {
            JObject weapon = GuardianSpearGenerator.PrepareClone(GuardianSpearGenerator.Load(GreatSword), guid, GreatSword);
            weapon["Data"]["Components"] = new JArray();
            weapon["Data"]["m_VisualParameters"]["m_WeaponModel"] = null;
            GuardianSpearGenerator.AddOverride(weapon, "m_VisualParameters.m_WeaponModel");
            GuardianSpearGenerator.Override(weapon, "CanBeUsedInGame", false);
            GuardianSpearGenerator.Override(weapon, "IsUnlootable", true);
            GuardianSpearGenerator.Override(weapon, "WarhammerDamage", min);
            GuardianSpearGenerator.Override(weapon, "WarhammerMaxDamage", max);
            GuardianSpearGenerator.Override(weapon, "WarhammerPenetration", penetration);
            GuardianSpearGenerator.Save(fileName, weapon);
        }

        private static void AddWeaponOverride(JObject ability, string weaponGuid, string name)
        {
            JObject template = (JObject)JObject.Parse(File.ReadAllText(GuardianCleaveBlueprint))["Data"]["Components"]
                .Children<JObject>().First(component => component["$type"]?.ToString().Contains("WarhammerOverrideAbilityWeapon") == true).DeepClone();
            template["name"] = name;
            template["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            template["m_Weapon"] = "!bp_" + weaponGuid;
            template["m_ForceShowWeaponDamageInUi"] = true;
            ((JArray)ability["Data"]["Components"]).Add(template);
            GuardianSpearGenerator.AddOverride(ability, name);
        }

        private static void AddLocalization()
        {
            string path = Root + "/Localization/enGB.json";
            JObject root = JObject.Parse(File.ReadAllText(path));
            JObject strings = (JObject)root["strings"];
            string[] names =
            {
                "Castellan's Vigil", "Auric Executioner", "Praetorian's Wrath",
                "Axe of the Ten Thousand", "Sentence of the Golden Throne", "The Emperor's Headsman"
            };
            string[] levels = { "1-15", "16-25", "26-35", "36-43", "44-49", "50-55" };
            strings["castellan-axe-flavor"] = Entry("A master-crafted polearm combining a power axe with an integrated bolt weapon.");
            strings["castellan-onslaught-name"] = Entry("Castellan Onslaught");
            strings["castellan-sweep-name"] = Entry("Castellan Sweep");
            strings["castellan-executioner-bolt-name"] = Entry("Executioner Bolt");
            strings["castellan-haft-strike-name"] = Entry("Haft Strike");
            for (int i = 0; i < WeaponGuids.Length; i++)
            {
                int tier = i + 1;
                int hits = tier >= 4 ? 3 : 2;
                strings[$"castellan-axe-v{tier}-name"] = Entry(names[i]);
                strings[$"castellan-axe-v{tier}-desc"] = Entry(
                    $"A master-crafted Custodes hybrid weapon.\n\n• Levels: {levels[i]}\n" +
                    $"• Strike: {StrikeMin[i]}–{StrikeMax[i]} damage, {StrikePen[i]}% armour penetration\n" +
                    $"• Castellan Onslaught: {hits} separately resolved {OnslaughtMin[i]}–{OnslaughtMax[i]} hits\n" +
                    $"• Castellan Sweep: {SweepMin[i]}–{SweepMax[i]} damage in a compact 3-cell arc\n" +
                    "• Executioner Bolt: precise long-range shot, no ammunition, 2-round cooldown\n" +
                    "• Haft Strike: low-damage 1 AP attack that pushes the target 1 cell and can stun for 1 round");
                strings[$"castellan-onslaught-v{tier}-desc"] = Entry(
                    $"Strike the same target {hits} times. Each hit deals {OnslaughtMin[i]}–{OnslaughtMax[i]} damage with {OnslaughtPen[i]}% armour penetration and is resolved separately.");
                strings[$"castellan-sweep-v{tier}-desc"] = Entry(
                    $"Make a compact 3-cell sweeping attack, dealing {SweepMin[i]}–{SweepMax[i]} damage with {SweepPen[i]}% armour penetration.");
                strings[$"castellan-executioner-bolt-v{tier}-desc"] = Entry(
                    "Fire one carefully aimed bolt at 150% of the weapon's normal effective range with +10% hit chance. This attack uses no ammunition. Cooldown: 2 rounds.");
                strings[$"castellan-haft-strike-v{tier}-desc"] = Entry(
                    $"Strike with the weapon's haft for {HaftMin[i]}–{HaftMax[i]} damage and push the target 1 cell. On a failed Toughness resistance test, the target is stunned for 1 round. Cost: 1 AP.");
            }
            File.WriteAllText(path, root.ToString(Formatting.Indented));
        }

        private static JObject Entry(string text) => new JObject { ["Offset"] = 0, ["Text"] = text };
    }
}
