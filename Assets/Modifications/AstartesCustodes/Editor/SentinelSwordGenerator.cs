using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Kingmaker.View.Equipment;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace AstartesCustodes.Editor
{
    internal static class SentinelSwordGenerator
    {
        internal const string WeaponGuid = "94d7497e0b1941c1910a6b29ed8911c2";
        internal static readonly string[] WeaponGuids =
        {
            WeaponGuid, "2a4f993df4414fc58b0a98dca5867182", "716bd961aa6e4b69bf29305ed5898c32",
            "82ea713354744f8f879d55f53d21e78d", "ca4598d179194496ab44584b22c98761", "ed577626cb164bdebb29849e01bd2ec7"
        };
        private static readonly string[] PowerFieldAbilityGuids =
        {
            "7bd2c146f08a4e5489c991acee473721", "51bf0ad45c474982acf010915d102be1", "982d06e208d24556ba20d0a419075647",
            "25a4857bb6604f298311a1d8dc8c92bd", "fac22e7feb624bf9bb01e7ff82e8af1d", "30fa7b9e996d4023adf40267f6a1a7c4"
        };
        private static readonly string[] PowerFieldBuffGuids =
        {
            "c89d128481df4fe096639d1d7d8c50af", "28eb64168c8c4671955baa5a44705e16", "34847618e4334e45834f6920d35ab8d8",
            "070cc0ff43ef4e878306311a74eb671c", "8ce9c82a59be41b5bb7a35dded4e4bc8", "98621a4eeef94232a0ba13913de2dde1"
        };
        private static readonly string[] ModifierFeatureGuids =
        {
            "a9809b2974f04717b44d20a5e9ec99ed", "d72a76bfd6b14741af45b465786ad85c", "70614d41990d46a7acda8035f570d9cc",
            "d775f0b20bc646428ac1fb44c075b894", "ef38a780b02e4915977a53c9578272af", "32d05afe887748048598d8c7c7868374"
        };
        internal const string PowerFieldAbilityGuid = "7bd2c146f08a4e5489c991acee473721";
        internal const string PowerFieldBuffGuid = "c89d128481df4fe096639d1d7d8c50af";
        internal const string SentinelWaveAbilityGuid = "641128fea4664f61a734b46f6085ac8e";
        private static readonly string[] SentinelCleaveAbilityGuids =
        {
            "eb82eb0ba0de4554a97438190498a90b", "d38d7b04f24641d59d048afda2dff6c8", "b712356d2ca34478891ff8e55638a2d7",
            "7283e4ec972d427fac60407aba50eaf2", "d41180db811e44d8beec4eb241db95f1", "1814d159a21f4703b9ce84bbff33bfd2"
        };
        private static readonly string[] SentinelOnslaughtAbilityGuids =
        {
            "887833acc8b647caac76c48714fc78b0", "0b60557b7eb849ee933825c089745ff1", "0ad616083be647f9a7c304f1cda2d51a",
            "b69755388d744c238c266b044b1ec495", "bd18e205e85e463c95f8a2ecfd9a897a", "7a4fdb4f9fad4b01ae2f1e0ff79c99eb"
        };
        private static readonly string[] CleaveWeaponGuids =
        {
            "7221ce8c21604cc997d93a4e200b4f81", "cc11a17b6cda4cf783703fae1db2cba4", "228532198f3541c9bd3a971e6210a02b",
            "643838107ded47f2a620dac2fc47df89", "e9759ff697e74d7189acc4e7617fd75e", "67fba488997f4f47a87be85fb6307367"
        };
        private static readonly string[] OnslaughtWeaponGuids =
        {
            "1952402769154ebe9036762b4b03c457", "47a99eb2cc1348a59c4730dc900237f5", "91b3679f8d9e4a1b9a06a44c8d5921bd",
            "828fb0f2ee0644098cfe04bfea7bd089", "d5fcadd003eb412f859a1880480216bb", "7f7c6e43c9004a05b353b90475bad1e6"
        };

        private const string Root = "Assets/Modifications/AstartesCustodes";
        private const string Art = Root + "/Art";
        private const string Blueprints = Root + "/Blueprints";
        private const string Localization = Root + "/Localization/enGB.json";
        private const string FbxPath = Art + "/SentinelSword.fbx";
        private const string PrefabPath = Art + "/SentinelSword.prefab";
        private const string InventoryIconPath = Art + "/SentinelSword_InventoryIcon.png";
        private const string BaseColorPath = Art + "/SentinelSword_BaseColor.png";
        private const string MetallicPath = Art + "/SentinelSword_Metallic.png";
        private const string NormalPath = Art + "/SentinelSword_Normal_Source.png";
        private const string RoughnessPath = Art + "/SentinelSword_Roughness.png";
        private const string MaterialPath = Art + "/SentinelSword.mat";
        private const string PackedMaskPath = Art + "/SentinelSword_MetallicSmoothness.asset";
        private const string PackedNormalPath = Art + "/SentinelSword_Normal.asset";

        // Early-game one-handed power sword; special components are removed for the initial plain sword version.
        private const string PowerSwordPrototype = "c431fcd14b45453e8fea6b2b4186778d";
        private const string PowerFieldAbilityPrototype = "afdae4482b3d4161a75224e8e52e8baf";
        private const string PowerFieldBuffPrototype = "22144723ab574b998e90580b8385a26e";
        private const string SentinelWavePrototype = "9dec1bdade284190b0977f5f70d26d3e";
        private const string CleavePrototype = "bac8a9c632934bec87c72fece5831673";
        private const string OnslaughtPrototype = "bb3fd8ea6c9e425780be92e054ace715";
        private const string DamageModifierReference = "8ee5002b220a42249cd6e0ecd416d451";
        private const string OverrideWeaponReference = "84c32baad3f14585a32f5747d721dfc3";
        private const string SwordAttackFxGuid = "1bc92b9832fe402caa887d8c5d990cb4";
        private const string TelekinesisIconGuid = "d2db9cd1a850eba4790dac666bad955e";
        // Vanilla Aeldari Force Sword activation: an energy-weapon self-buff icon rather than a caster hand.
        private const string PowerFieldIconGuid = "35279bc29c0d21649ad4157b24b22c7a";
        private const string WsModifierReference = "08e144a9788040ea81a99421b5576bc3";
        private const string ParryModifierReference = "53c19a9468d24539863989b3be9ed1f5";
        private const string DodgeModifierReference = "96e84143a45d4150b0233b5b58087fd1";
        private static readonly int[] MinDamage = { 9, 12, 15, 18, 25, 32 };
        private static readonly int[] MaxDamage = { 13, 17, 21, 26, 36, 45 };
        private static readonly int[] Penetration = { 20, 25, 30, 35, 40, 45 };
        private static readonly int[] WeaponSkill = { 2, 4, 6, 8, 10, 12 };
        private static readonly int[] Parry = { 3, 5, 7, 9, 12, 15 };
        private static readonly int[] Dodge = { 0, 2, 3, 5, 7, 10 };
        private static readonly int[] PowerFieldDuration = { 2, 3, 4, 5, 6, 0 };
        private static readonly int[] PowerFieldDamage = { 5, 7, 9, 11, 15, 20 };

        [MenuItem("Astartes Custodes/Generate Sentinel Sword")]
        public static void Generate()
        {
            Directory.CreateDirectory(Art);
            Directory.CreateDirectory(Blueprints);
            GenerateArt();
            GenerateBlueprint();
            WriteLocalization();
            AssetDatabase.Refresh();
            Debug.Log("[AstartesCustodes] Sentinel Sword V1-V6 generated.");
        }

        private static void GenerateArt()
        {
            AssetDatabase.ImportAsset(FbxPath, ImportAssetOptions.ForceSynchronousImport);
            ModelImporter importer = AssetImporter.GetAtPath(FbxPath) as ModelImporter;
            if (importer == null) throw new InvalidDataException("SentinelSword.fbx could not be imported.");
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.SaveAndReimport();

            GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
            if (fbx == null) throw new InvalidDataException("Sentinel Sword FBX asset is missing after import.");
            Material material = CreateMaterial();

            GameObject root = new GameObject("SentinelSword_Root");
            EquipmentOffsets offsets = root.AddComponent<EquipmentOffsets>();
            ConfigureHolsterOffsets(offsets);
            AddComponentByName(root, "FxLocatorMapper", false);
            AddComponentByName(root, "AstartesCustodes.Runtime.GuardianSpearMaterialBinder", true);

            // Keep the imported mesh directly inside the generated weapon prefab. Nested FBX prefab
            // instances are not resolved reliably by Owlcat's stripped modification asset bundles.
            GameObject model = UnityEngine.Object.Instantiate(fbx);
            model.name = "SentinelSword_FBX_Model";
            model.transform.SetParent(root.transform, false);
            // The source is centred and its long axis is local Z (handle at +Z, blade at -Z),
            // while Owlcat one-handed weapon prefabs expect the blade on local +Y. Rotate the
            // source axis into that convention and place the middle of the red grip at root.
            // This lets the animation put the blade over the shoulder instead of above the head.
            model.transform.localPosition = new Vector3(0f, 0.49f, 0f);
            Quaternion alignBladeToWeaponAxis = Quaternion.Euler(90f, 0f, 0f);
            // Keep the blade plane established at 90 degrees, but turn the asymmetrical
            // auxiliary handle to the underside of the weapon.
            Quaternion rollBladeFace = Quaternion.AngleAxis(270f, Vector3.up);
            model.transform.localRotation = rollBladeFace * alignBladeToWeaponAxis;
            // Meshy/Blender exported this FBX in centimetres. Unity retains the 0.01 file-unit
            // conversion inside the imported mesh, so compensate here to obtain a roughly
            // 1.30-metre in-game sword (the former 0.76 scale produced 1-centimetre bounds).
            model.transform.localScale = Vector3.one * 68f;
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterial = material;

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
        }

        private static void ConfigureHolsterOffsets(EquipmentOffsets offsets)
        {
            // Drawn weapons use m_MainHand/m_OffHand. Holstered weapons instead use indexed
            // UnitEquipmentVisualSlotType entries: RightFront01=1, LeftFront01=3,
            // LeftBack01=6 and RightBack01=8. Move the oversized sword away from the body
            // without disturbing the already-correct hand and shoulder animation.
            SerializedObject serialized = new SerializedObject(offsets);
            SerializedProperty slots = serialized.FindProperty("m_SlotOffsets");
            slots.arraySize = 12;
            for (int i = 0; i < slots.arraySize; i++)
            {
                SerializedProperty slot = slots.GetArrayElementAtIndex(i);
                slot.FindPropertyRelative("Position").vector3Value = Vector3.zero;
                slot.FindPropertyRelative("Rotation").vector3Value = Vector3.zero;
            }
            // Exact offsets used by Owlcat's MSW_PowerSword1 and MSW_PowerSword2 prefabs.
            ConfigureHolsterSlot(slots, 1, new Vector3(0.04f, 0.03f, -0.15f), new Vector3(1.75f, 255.27f, 266.19f));
            ConfigureHolsterSlot(slots, 3, new Vector3(-0.01f, -0.04f, -0.11f), new Vector3(11.24f, 90.97f, 74.20f));
            ConfigureHolsterSlot(slots, 6, new Vector3(0.01f, -0.03f, -0.12f), new Vector3(358.31f, 95.50f, 90.41f));
            ConfigureHolsterSlot(slots, 8, new Vector3(-0.06f, -0.04f, -0.09f), new Vector3(0.91f, 281.02f, 276.02f));
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureHolsterSlot(SerializedProperty slots, int index,
            Vector3 position, Vector3 rotation)
        {
            SerializedProperty slot = slots.GetArrayElementAtIndex(index);
            slot.FindPropertyRelative("Position").vector3Value = position;
            slot.FindPropertyRelative("Rotation").vector3Value = rotation;
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
            if (baseColor == null) throw new InvalidDataException("Sentinel Sword base-colour texture is missing.");

            Texture2D metallic = LoadPng(MetallicPath, true);
            Texture2D roughness = LoadPng(RoughnessPath, true);
            Texture2D normalSource = LoadPng(NormalPath, true);
            Texture2D mask = PackMetallicSmoothness(metallic, roughness);
            Texture2D normal = PackUnityNormal(normalSource);

            Shader shader = Shader.Find("Owlcat/Lit");
            if (shader == null) throw new InvalidOperationException("Owlcat/Lit is unavailable.");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "SentinelSword" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else material.shader = shader;
            SetTexture(material, baseColor, "_BaseMap", "_BaseColorMap", "_MainTex");
            SetTexture(material, mask, "_MetallicGlossMap", "_MaskMap", "_MasksMap");
            SetTexture(material, normal, "_BumpMap", "_NormalMap");
            SetFloat(material, 1f, "_Metallic");
            SetFloat(material, 1f, "_Smoothness");
            SetFloat(material, 0.24f, "_Roughness");
            Color brighterAlbedo = new Color(1.15f, 1.15f, 1.15f, 1f);
            SetColor(material, brighterAlbedo, "_BaseColor", "_Color", "_AdditionalAlbedoColor");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        private static Texture2D LoadPng(string assetPath, bool linear)
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, true, linear);
            if (!texture.LoadImage(File.ReadAllBytes(assetPath), false))
                throw new InvalidDataException("Could not decode " + assetPath);
            return texture;
        }

        private static Texture2D PackMetallicSmoothness(Texture2D metallic, Texture2D roughness)
        {
            if (metallic.width != roughness.width || metallic.height != roughness.height)
                throw new InvalidDataException("Sentinel Sword metallic and roughness textures have different dimensions.");
            Color32[] metal = metallic.GetPixels32();
            Color32[] rough = roughness.GetPixels32();
            for (int i = 0; i < metal.Length; i++) metal[i] = new Color32(metal[i].r, 0, 0, (byte)(255 - rough[i].r));
            Texture2D packed = new Texture2D(metallic.width, metallic.height, TextureFormat.RGBA32, true, true)
                { name = "SentinelSword_MetallicSmoothness" };
            packed.SetPixels32(metal);
            packed.Apply(true, false);
            return ReplaceTextureAsset(packed, PackedMaskPath);
        }

        private static Texture2D PackUnityNormal(Texture2D source)
        {
            Color32[] pixels = source.GetPixels32();
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, pixels[i].g, 255, pixels[i].r);
            Texture2D packed = new Texture2D(source.width, source.height, TextureFormat.RGBA32, true, true)
                { name = "SentinelSword_Normal" };
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
                throw new InvalidDataException("Sentinel Sword prefab could not be resolved.");
            UnityEngine.Object inventoryIcon = PrepareInventoryIcon();
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(inventoryIcon, out string iconGuid, out long iconFileId))
                throw new InvalidDataException("Sentinel Sword inventory icon could not be resolved.");

            JObject overrideWeaponTemplate = (JObject)Load(OverrideWeaponReference)["Data"]["Components"]
                .Children<JObject>().First(component => component["$type"]?.ToString().Contains("WarhammerOverrideAbilityWeapon") == true).DeepClone();
            for (int i = 0; i < WeaponGuids.Length; i++)
            {
                GenerateHiddenAttackWeapon(i, true);
                GenerateHiddenAttackWeapon(i, false);
                GenerateProfiledMeleeAbility(i, true, overrideWeaponTemplate);
                GenerateProfiledMeleeAbility(i, false, overrideWeaponTemplate);
                GenerateModifierFeature(i);
                GeneratePowerFieldBlueprints(i);
            }
            GenerateSentinelWaveBlueprint();
            for (int i = 0; i < WeaponGuids.Length; i++)
            {
                int tier = i + 1;
                JObject weapon = PrepareClone(Load(PowerSwordPrototype), WeaponGuids[i], PowerSwordPrototype);
                weapon["Data"]["Components"] = new JArray(CreateAddFact(ModifierFeatureGuids[i], $"sentinel-v{tier}-modifiers"));
                SetText(weapon, $"sentinel-v{tier}-name", $"sentinel-v{tier}-desc", "sentinel-sword-flavor");
                weapon["Data"]["m_Icon"] = UnityReference(iconGuid, iconFileId);
                AddOverride(weapon, "m_Icon");
                weapon["Data"]["m_VisualParameters"]["m_WeaponModel"] = UnityReference(prefabGuid, prefabFileId);
                AddOverride(weapon, "m_VisualParameters.m_WeaponModel");
                Override(weapon, "Family", "Power");
                Override(weapon, "Classification", "Sword");
                Override(weapon, "m_HoldingType", "OneHanded");
                Override(weapon, "IsTwoHanded", false);
                Override(weapon, "WarhammerDamage", MinDamage[i]);
                Override(weapon, "WarhammerMaxDamage", MaxDamage[i]);
                Override(weapon, "WarhammerPenetration", Penetration[i]);
                Override(weapon, "m_Enchantments", new JArray());
                Override(weapon, "m_Rarity", "Pattern");
                Override(weapon, "CanBeUsedInGame", true);
                Override(weapon, "IsUnlootable", false);
                Override(weapon, "IsNonRemovable", false);
                Override(weapon, "m_IsNotable", true);
                SetWeaponAbility(weapon, "Ability2", "AOE", SentinelCleaveAbilityGuids[i], SwordAttackFxGuid, 2);
                SetWeaponAbility(weapon, "Ability3", "AOE", SentinelOnslaughtAbilityGuids[i], SwordAttackFxGuid, 2);
                SetWeaponAbility(weapon, "Ability4", "Custom", PowerFieldAbilityGuids[i], null, 0);
                SetWeaponAbility(weapon, "Ability5", "Custom", SentinelWaveAbilityGuid, SwordAttackFxGuid, 1);
                string fileName = tier == 1 ? "SentinelSword_Item.jbp" : $"SentinelSword_V{tier}_Item.jbp";
                File.WriteAllText(Path.Combine(Blueprints, fileName), weapon.ToString(Formatting.Indented));
            }
        }

        private static UnityEngine.Object PrepareInventoryIcon()
        {
            AssetDatabase.ImportAsset(InventoryIconPath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(InventoryIconPath) as TextureImporter;
            if (importer == null) throw new InvalidDataException("Sentinel Sword inventory icon importer was not found.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(InventoryIconPath).FirstOrDefault(asset => asset is Sprite)
                ?? AssetDatabase.LoadMainAssetAtPath(InventoryIconPath);
        }

        private static void GenerateHiddenAttackWeapon(int i, bool cleave)
        {
            int tier = i + 1;
            int percent = cleave ? 90 : 80;
            string guid = cleave ? CleaveWeaponGuids[i] : OnslaughtWeaponGuids[i];
            JObject weapon = PrepareClone(Load(PowerSwordPrototype), guid, PowerSwordPrototype);
            weapon["Data"]["Components"] = new JArray();
            weapon["Data"]["m_VisualParameters"]["m_WeaponModel"] = null;
            AddOverride(weapon, "m_VisualParameters.m_WeaponModel");
            Override(weapon, "CanBeUsedInGame", false);
            Override(weapon, "IsUnlootable", true);
            Override(weapon, "WarhammerDamage", ScaleDamage(MinDamage[i], percent));
            Override(weapon, "WarhammerMaxDamage", ScaleDamage(MaxDamage[i], percent));
            Override(weapon, "WarhammerPenetration", Penetration[i]);
            SetText(weapon, $"sentinel-v{tier}-name", $"sentinel-v{tier}-desc", "sentinel-sword-flavor");
            string fileName = tier == 1
                ? $"SentinelSword_Hidden{(cleave ? "Cleave" : "Onslaught")}_Item.jbp"
                : $"SentinelSword_V{tier}_Hidden{(cleave ? "Cleave" : "Onslaught")}_Item.jbp";
            File.WriteAllText(Path.Combine(Blueprints, fileName), weapon.ToString(Formatting.Indented));
        }

        private static int ScaleDamage(int value, int percent) => (value * percent + 50) / 100;

        private static void GenerateProfiledMeleeAbility(int i, bool cleave, JObject overrideTemplate)
        {
            int tier = i + 1;
            string prototype = cleave ? CleavePrototype : OnslaughtPrototype;
            string guid = cleave ? SentinelCleaveAbilityGuids[i] : SentinelOnslaughtAbilityGuids[i];
            string hiddenWeapon = cleave ? CleaveWeaponGuids[i] : OnslaughtWeaponGuids[i];
            JObject ability = PrepareClone(Load(prototype), guid, prototype);
            JObject overrideWeapon = (JObject)overrideTemplate.DeepClone();
            overrideWeapon["name"] = $"$WarhammerOverrideAbilityWeapon$sentinel-v{tier}-{(cleave ? "cleave" : "onslaught")}";
            overrideWeapon["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            overrideWeapon["m_Weapon"] = "!bp_" + hiddenWeapon;
            overrideWeapon["m_ForceShowWeaponDamageInUi"] = true;
            ((JArray)ability["Data"]["Components"]).Add(overrideWeapon);
            AddOverride(ability, overrideWeapon["name"].ToString());
            ability["Data"]["m_DisplayName"] = Localized(cleave ? "sentinel-cleave-name" : "sentinel-onslaught-name");
            ability["Data"]["m_Description"] = Localized(cleave ? "sentinel-cleave-desc" : "sentinel-onslaught-desc");
            ability["Data"]["m_Icon"] = UnityReference(cleave ? "16fd1bbd2c6ab964db8067cbd185a32c" : "8836c5b61879f50448931d4962b0aa88", 21300000L);
            AddOverride(ability, "m_DisplayName");
            AddOverride(ability, "m_Description");
            AddOverride(ability, "m_Icon");
            string fileName = tier == 1
                ? $"SentinelSword_{(cleave ? "Cleave" : "Onslaught")}_Ability.jbp"
                : $"SentinelSword_V{tier}_{(cleave ? "Cleave" : "Onslaught")}_Ability.jbp";
            File.WriteAllText(Path.Combine(Blueprints, fileName), ability.ToString(Formatting.Indented));
        }

        private static JObject CreateAddFact(string guid, string name) => new JObject
        {
            ["$type"] = "65221a9a6133bd0408b019b86642d97e, AddFactToEquipmentWielder",
            ["name"] = "$AddFactToEquipmentWielder$" + name,
            ["m_Flags"] = 0,
            ["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" },
            ["m_Overrides"] = new JArray(),
            ["m_Fact"] = "!bp_" + guid
        };

        private static void SetWeaponAbility(JObject weapon, string slotName, string type,
            string abilityGuid, string fxGuid, int ap)
        {
            JObject slot = (JObject)weapon["Data"]["AbilityContainer"][slotName];
            slot["Type"] = type;
            slot["Mode"] = "Default";
            slot["m_Ability"] = "!bp_" + abilityGuid;
            slot["m_FXSettings"] = fxGuid == null ? null : "!bp_" + fxGuid;
            slot["OnHitOverrideType"] = "None";
            slot["m_OnHitActions"] = null;
            slot["AP"] = ap;
            AddOverride(weapon, "WeaponAbilities." + slotName + ".Type");
            AddOverride(weapon, "WeaponAbilities." + slotName + ".Mode");
            AddOverride(weapon, "WeaponAbilities." + slotName + ".m_Ability");
            AddOverride(weapon, "WeaponAbilities." + slotName + ".m_FXSettings");
            AddOverride(weapon, "WeaponAbilities." + slotName + ".AP");
        }

        private static void GeneratePowerFieldBlueprints(int i)
        {
            int tier = i + 1;
            JObject buff = PrepareClone(Load(PowerFieldBuffPrototype), PowerFieldBuffGuids[i], PowerFieldBuffPrototype);
            JObject damageModifier = buff["Data"]["Components"].Children<JObject>()
                .First(component => component["$type"]?.ToString().Contains("WarhammerDamageModifierInitiator") == true);
            damageModifier["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            damageModifier["m_Overrides"] = new JArray();
            JObject flatDamage = (JObject)damageModifier["UnmodifiableFlatDamageModifier"];
            flatDamage["ValueType"] = "Simple";
            flatDamage["Value"] = PowerFieldDamage[i];
            flatDamage["Property"] = "None";
            flatDamage["m_CustomProperty"] = null;
            flatDamage["PropertyName"] = "Value1";
            flatDamage["Enabled"] = true;
            JArray damageModifiers = new JArray();
            string[] affectedWeapons = { WeaponGuids[i], CleaveWeaponGuids[i], OnslaughtWeaponGuids[i] };
            for (int weaponIndex = 0; weaponIndex < affectedWeapons.Length; weaponIndex++)
            {
                JObject scopedModifier = (JObject)damageModifier.DeepClone();
                scopedModifier["name"] = $"$WarhammerDamageModifierInitiator$sentinel-v{tier}-power-field-{weaponIndex}";
                JObject weaponGetter = scopedModifier["Restrictions"]?["Property"]?["Getters"]?.Children<JObject>()
                    .First(getter => getter["$type"]?.ToString().Contains("CheckAbilityWeaponBlueprintGetter") == true);
                weaponGetter["m_Weapon"] = "!bp_" + affectedWeapons[weaponIndex];
                damageModifiers.Add(scopedModifier);
            }
            buff["Data"]["Components"] = damageModifiers;
            buff["Data"]["m_DisplayName"] = Localized("sentinel-power-field-name");
            buff["Data"]["m_Description"] = Localized($"sentinel-v{tier}-power-field-buff-desc");
            buff["Data"]["m_Flags"] = 0;
            buff["Data"]["Stacking"] = "Replace";
            buff["Data"]["FxOnStart"] = new JObject { ["AssetId"] = "cf6b6016a28a1bb42aef4576da77ebb4" };
            AddOverride(buff, "m_DisplayName");
            AddOverride(buff, "m_Description");
            File.WriteAllText(Path.Combine(Blueprints, tier == 1 ? "SentinelSword_PowerField_Buff.jbp" : $"SentinelSword_V{tier}_PowerField_Buff.jbp"), buff.ToString(Formatting.Indented));

            JObject ability = PrepareClone(Load(PowerFieldAbilityPrototype), PowerFieldAbilityGuids[i], PowerFieldAbilityPrototype);
            ability["Data"]["Components"] = new JArray
            {
                new JObject
                {
                    ["$type"] = "66e032e5cf38801428940a1a0d14b946, AbilityEffectRunAction",
                    ["name"] = "$AbilityEffectRunAction$sentinel-power-field",
                    ["m_Flags"] = 0,
                    ["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" },
                    ["m_Overrides"] = new JArray(),
                    ["SavingThrowType"] = "Unknown",
                    ["Actions"] = new JObject
                    {
                        ["Actions"] = new JArray
                        {
                            new JObject
                            {
                                ["$type"] = "5d13a597de91e4746b804f8233518523, ContextActionApplyBuff",
                                ["name"] = "$ContextActionApplyBuff$sentinel-power-field",
                                ["m_Buff"] = "!bp_" + PowerFieldBuffGuids[i],
                                ["BuffEndCondition"] = "CombatEnd",
                                ["Permanent"] = i == 5,
                                ["DurationValue"] = new JObject
                                {
                                    ["BonusValue"] = SimpleContextValue(0),
                                    ["RoundsValue"] = SimpleContextValue(PowerFieldDuration[i])
                                },
                                ["ToCaster"] = true,
                                ["AsChild"] = false,
                                ["SameDuration"] = false,
                                ["Ranks"] = SimpleContextValue(1),
                                ["ActionsOnApply"] = new JObject { ["Actions"] = new JArray() },
                                ["ActionsOnImmune"] = new JObject { ["Actions"] = new JArray() },
                                ["AddFactSource"] = true
                            }
                        }
                    }
                }
            };
            ability["Data"]["m_DisplayName"] = Localized("sentinel-power-field-name");
            ability["Data"]["m_Description"] = Localized($"sentinel-v{tier}-power-field-desc");
            ability["Data"]["m_Icon"] = UnityReference(PowerFieldIconGuid, 21300000L);
            ability["Data"]["ActionPointCost"] = 0;
            ability["Data"]["CooldownRounds"] = 7;
            ability["Data"]["Range"] = "Personal";
            ability["Data"]["CanTargetPoint"] = false;
            ability["Data"]["CanTargetEnemies"] = false;
            ability["Data"]["CanTargetFriends"] = false;
            ability["Data"]["CanTargetSelf"] = true;
            ability["Data"]["NotOffensive"] = true;
            ability["Data"]["Animation"] = "None";
            ability["Data"]["IsFreeAction"] = true;
            ability["Data"]["CombatStateRestriction"] = "InCombatOnly";
            AddOverride(ability, "m_DisplayName");
            AddOverride(ability, "m_Description");
            AddOverride(ability, "m_Icon");
            AddOverride(ability, "CooldownRounds");
            File.WriteAllText(Path.Combine(Blueprints, tier == 1 ? "SentinelSword_PowerField_Ability.jbp" : $"SentinelSword_V{tier}_PowerField_Ability.jbp"), ability.ToString(Formatting.Indented));
        }

        private static void GenerateModifierFeature(int i)
        {
            int tier = i + 1;
            JObject feature = PrepareClone(Load(WsModifierReference), ModifierFeatureGuids[i], WsModifierReference);
            feature["Meta"]["ShadowDeleted"] = false;
            JArray components = new JArray();
            JObject ws = CloneComponent(WsModifierReference, "AddStatBonus", $"$AddStatBonus$sentinel-v{tier}-ws");
            ws["Stat"] = "WarhammerWeaponSkill";
            ws["Value"] = WeaponSkill[i];
            components.Add(ws);
            JObject parry = CloneComponent(ParryModifierReference, "WarhammerParryChanceModifierDefender", $"$WarhammerParryChanceModifierDefender$sentinel-v{tier}");
            parry["Restrictions"]["Property"]["Getters"] = new JArray();
            parry["ParryChance"]["Value"] = Parry[i];
            components.Add(parry);
            if (Dodge[i] > 0)
            {
                JObject dodge = CloneComponent(DodgeModifierReference, "WarhammerDodgeChanceModifierDefender", $"$WarhammerDodgeChanceModifierDefender$sentinel-v{tier}");
                dodge["Restrictions"]["Property"]["Getters"] = new JArray();
                dodge["DodgeChance"]["Value"] = Dodge[i];
                dodge["PercentDodgeModifier"] = false;
                components.Add(dodge);
            }
            feature["Data"]["Components"] = components;
            feature["Data"]["m_DisplayName"] = Localized($"sentinel-v{tier}-modifier-name");
            feature["Data"]["m_Description"] = Localized($"sentinel-v{tier}-modifier-desc");
            feature["Data"]["HideInUI"] = false;
            feature["Data"]["HideInCharacterSheetAndLevelUp"] = true;
            foreach (string property in new[] { "m_DisplayName", "m_Description", "HideInUI", "HideInCharacterSheetAndLevelUp" }) AddOverride(feature, property);
            File.WriteAllText(Path.Combine(Blueprints, $"SentinelSword_V{tier}_Modifiers_Feature.jbp"), feature.ToString(Formatting.Indented));
        }

        private static JObject CloneComponent(string blueprintId, string typeName, string name)
        {
            JObject component = (JObject)Load(blueprintId)["Data"]["Components"].Children<JObject>()
                .First(item => item["$type"]?.ToString().Contains(typeName) == true).DeepClone();
            component["name"] = name;
            component["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            component["m_Overrides"] = new JArray();
            return component;
        }

        private static void GenerateSentinelWaveBlueprint()
        {
            JObject ability = PrepareClone(Load(SentinelWavePrototype), SentinelWaveAbilityGuid, SentinelWavePrototype);
            ability["Data"]["m_DisplayName"] = Localized("sentinel-wave-name");
            ability["Data"]["m_Description"] = Localized("sentinel-wave-desc");
            ability["Data"]["m_Icon"] = UnityReference(TelekinesisIconGuid, 21300000L);
            ability["Data"]["Type"] = "Weapon";
            ability["Data"]["Range"] = "Custom";
            ability["Data"]["CustomRange"] = 5;
            ability["Data"]["MinRange"] = 1;
            ability["Data"]["ActionPointCost"] = 1;
            ability["Data"]["AbilityParamsSource"] = "Weapon";
            // PsychicPower has no None enum value. Minor is the engine default; the ability
            // remains non-psychic because its AbilityParamsSource is Weapon.
            ability["Data"]["PsychicPower"] = "Minor";
            ability["Data"]["VeilThicknessPointsToAdd"] = 0;
            ability["Data"]["CooldownRounds"] = 2;
            ability["Data"]["CanTargetEnemies"] = true;
            ability["Data"]["CanTargetFriends"] = false;
            ability["Data"]["CanTargetSelf"] = false;
            ability["Data"]["NotOffensive"] = false;
            ability["Data"]["Animation"] = "Directional";
            ability["Data"]["ShouldTurnToTarget"] = true;
            ability["Data"]["m_AbilityGroups"] = new JArray();
            // The normal sword attack delivery is deliberately retained. It drives the equipped
            // weapon's actual melee swing even though this custom attack may target up to 5 cells.
            ability["Data"]["m_FXSettings"] = "!bp_" + SwordAttackFxGuid;
            foreach (string property in new[] { "m_DisplayName", "m_Description", "m_Icon", "Type", "Range",
                "CustomRange", "MinRange", "ActionPointCost", "AbilityParamsSource", "PsychicPower",
                "VeilThicknessPointsToAdd", "CooldownRounds", "CanTargetEnemies", "CanTargetFriends",
                "CanTargetSelf", "NotOffensive", "Animation", "ShouldTurnToTarget", "m_AbilityGroups", "m_FXSettings" })
                AddOverride(ability, property);
            File.WriteAllText(Path.Combine(Blueprints, "SentinelSword_Wave_Ability.jbp"), ability.ToString(Formatting.Indented));
        }

        private static JObject SimpleContextValue(int value) => new JObject
        {
            ["ValueType"] = "Simple", ["Value"] = value, ["ValueRank"] = "Default",
            ["ValueShared"] = "Damage", ["Property"] = "None", ["m_CustomProperty"] = null,
            ["PropertyName"] = "Value1"
        };

        private static JObject Load(string id)
        {
            BlueprintJsonWrapper wrapper = BlueprintsDatabase.LoadWrapperById(id);
            if (wrapper == null) throw new InvalidDataException("Blueprint not found: " + id);
            using var writer = new StringWriter();
            Json.Serializer.Serialize(writer, wrapper);
            return JObject.Parse(writer.ToString());
        }

        private static JObject PrepareClone(JObject root, string id, string prototype)
        {
            root["AssetId"] = id;
            root["Data"]["PrototypeLink"] = prototype;
            root["Data"]["m_Overrides"] = new JArray();
            foreach (JObject component in root["Data"]["Components"].Children<JObject>())
            {
                component["PrototypeLink"] = new JObject { ["guid"] = prototype, ["name"] = component["name"]?.ToString() ?? "" };
                component["m_Overrides"] = new JArray();
            }
            return root;
        }

        private static void SetText(JObject root, string name, string description, string flavor)
        {
            root["Data"]["m_DisplayName"] = Localized(name);
            root["Data"]["m_Description"] = Localized(description);
            root["Data"]["m_FlavorText"] = Localized(flavor);
            AddOverride(root, "m_DisplayName");
            AddOverride(root, "m_Description");
            AddOverride(root, "m_FlavorText");
        }

        private static JObject Localized(string key) => new JObject
        {
            ["m_Key"] = key, ["m_OwnerString"] = "", ["m_OwnerPropertyPath"] = "",
            ["m_JsonPath"] = "", ["Shared"] = null
        };

        private static JObject UnityReference(string guid, long fileId) => new JObject { ["guid"] = guid, ["fileid"] = fileId };

        private static void Override(JObject root, string property, JToken value)
        { root["Data"][property] = value; AddOverride(root, property); }

        private static void AddOverride(JObject root, string property)
        {
            JArray overrides = (JArray)root["Data"]["m_Overrides"];
            if (!overrides.Values<string>().Contains(property)) overrides.Add(property);
        }

        private static void WriteLocalization()
        {
            JObject document = File.Exists(Localization) ? JObject.Parse(File.ReadAllText(Localization)) : new JObject();
            JObject strings = document["strings"] as JObject ?? new JObject();
            document["strings"] = strings;
            AddLocalizationEntries(strings);
            File.WriteAllText(Localization, document.ToString(Formatting.Indented));
        }

        internal static void AddLocalizationEntries(JObject strings)
        {
            string[] sentinelSwordNames =
            {
                "Custodian's Edge", "Auric Talon", "Praetorian's Answer",
                "Blade of the Ten Thousand", "Judgement of Terra", "The Emperor's Final Decree"
            };
            strings["sentinel-sword-flavor"] = Entry("A gleaming blade fashioned for the unwavering guardians of the Golden Throne.");
            strings["sentinel-power-field-name"] = Entry("Activate Power Field");
            strings["sentinel-wave-name"] = Entry("Sentinel Wave");
            strings["sentinel-wave-desc"] = Entry("Swing the Sentinel Sword to project a cutting wave of force at an enemy up to 5 cells away. The attack uses the weapon's normal damage and armour penetration. Cost: 1 AP. Cooldown: 2 rounds.");
            strings["sentinel-cleave-name"] = Entry("Cleave");
            strings["sentinel-cleave-desc"] = Entry("Sweep the Sentinel Sword through several enemies. Deals 90% of the weapon's Strike damage.");
            strings["sentinel-onslaught-name"] = Entry("Onslaught");
            strings["sentinel-onslaught-desc"] = Entry("Unleash a wide, powerful sweep. Deals 80% of the weapon's Strike damage.");
            for (int i = 0; i < 6; i++)
            {
                int tier = i + 1;
                string[] levelRanges = { "1-15", "16-25", "26-35", "36-43", "44-49", "50-55" };
                string levels = levelRanges[i];
                string duration = tier == 6 ? "until combat ends" : $"for {PowerFieldDuration[i]} rounds";
                string dodge = Dodge[i] > 0 ? $"\n• +{Dodge[i]}% dodge chance" : "";
                strings[$"sentinel-v{tier}-name"] = Entry(sentinelSwordNames[i]);
                strings[$"sentinel-v{tier}-desc"] = Entry($"A master-crafted Custodes power sword.\n\n• Levels: {levels}\n• Power Field: +{PowerFieldDamage[i]} damage {duration}\n• Power Field cooldown: 7 rounds\n• Sentinel Wave: 5-cell range, 1 AP, 2-round cooldown\n\nModifiers while equipped:\n• +{WeaponSkill[i]} Weapon Skill\n• +{Parry[i]}% parry chance{dodge}");
                strings[$"sentinel-v{tier}-modifier-name"] = Entry($"{sentinelSwordNames[i]} Mastery");
                strings[$"sentinel-v{tier}-modifier-desc"] = Entry($"While equipped:\n• +{WeaponSkill[i]} Weapon Skill\n• +{Parry[i]}% parry chance{dodge}");
                strings[$"sentinel-v{tier}-power-field-desc"] = Entry($"Activates the Sentinel Sword's power field {duration}. Attacks made with this weapon deal +{PowerFieldDamage[i]} additional damage. Cooldown: 7 rounds.");
                strings[$"sentinel-v{tier}-power-field-buff-desc"] = Entry($"The Sentinel Sword is energised. Its attacks deal +{PowerFieldDamage[i]} additional damage.");
            }
        }

        private static JObject Entry(string text) => new JObject { ["Offset"] = 0, ["Text"] = text };
    }
}
