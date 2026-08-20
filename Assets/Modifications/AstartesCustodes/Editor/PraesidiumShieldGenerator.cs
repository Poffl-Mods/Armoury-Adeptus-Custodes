using System;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Kingmaker.View.Equipment;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace AstartesCustodes.Editor
{
    internal static class PraesidiumShieldGenerator
    {
        internal const string ShieldGuid = "e44e715029c9490ab7df80ad4366996b";
        private static readonly string[] ShieldGuids =
        {
            ShieldGuid, "b269c2b1beef4958b6fb80512a9c84d9", "361f5576f230408b86f7232808b6e4b7",
            "945a4bb3c4b34454ba12db26cf025653", "c540d8eb059a42c8aacdd2a3eb540c8e",
            "36b54ba6bf994d2e9863c6a42b53cf1d"
        };
        private static readonly string[] AuricBastionAbilityGuids =
        {
            "30c3bcff9c51470aa2e80ffd9296a394", "57e78bb51bf443ca929ae195a3027c10",
            "106274d10ee74a7d981bab86dc5bcd95", "7e7c7cd17ebd4ca3abc7db9080190846",
            "5716cf3a8453489ca4a85e91adcc34d3", "199618380c81434f9c501f31cb6ffc21"
        };
        private static readonly string[] NullShockwaveAbilityGuids =
        {
            "4c467293b2134fff909bad1107a70164", "73c9f878a07e45299633fdbc9e1f6ba8",
            "61709c7a353c4028a1298ad062c6058e", "05e44a2957d549a59ee78c7ede31b551",
            "c42f55581541496192b345a8aa37ed44", "14559ff88aa344abb87ccec68df1bfb6"
        };
        private static readonly string[] HiddenShieldWeaponGuids =
        {
            "88b3f79fa5cb4f90bd719209f48919c8", "fdac3640dd684eaf9d834f9ecee63580",
            "5b2e1d66ca98498d9b50c4b2c6c50a86", "d5c3a9d3aa04483491bcff4f38dda5d8",
            "bdab5b71f5544e098d1d4dae2c1b8c53", "d085f298636547c7a7a9ea687b3e773d"
        };
        private static readonly string[] ModifierFeatureGuids =
        {
            "11490f81c4a9471aa4d4847945da7ca6", "c56949220c0e4d868e0327a5e143289c",
            "09b0ebdde741499b929044decd69470c", "8fb026650b64454ab5fb9ca79533180d",
            "1aecdd15164544cf88d5c4d6b4e87e45", "f126520f849a4cd4b4986e5bcd6b21f2"
        };
        private static readonly int[] ShieldDamage = { 10, 13, 16, 19, 22, 25 };
        private static readonly int[] CooldownRounds = { 8, 7, 6, 5, 4, 3 };
        private static readonly int[] AttributeBonus = { 0, 2, 4, 6, 8, 10 };
        private static readonly string[] TierNames =
        {
            "Custodian's Aegis", "Sentinel's Aegis", "Praesidium Aegis",
            "Aegis of the Ten Thousand", "Auric Aegis", "Aegis of the Emperor"
        };
        private const string AuricBastionCasterBuffGuid = "8b72831f46d1485ba57183e0fde92943";
        private const string AuricBastionAreaGuid = "c3aeaeb77b78427290d9de0ab4889e6b";
        private const string AuricBastionEffectBuffGuid = "837dea32138544beb11d744c60e98b97";
        private const string NullZoneCasterBuffGuid = "a8ba39cfb0134a32820f8ea80515eba9";
        private const string NullZoneAreaGuid = "847a72d0347c42dda35dafcf3277d175";
        private const string NullZoneMuteBuffGuid = "506bcf8894ba4180b5b0f013688087de";
        private const string AdvancedShieldPrototype = "667427778eb743f2883b0e8915541ab6";
        private const string ForceFieldAbilityPrototype = "2750f3b3b1d54cbb83b0c08acffad6b3";
        private const string ForceFieldBuffPrototype = "7f46b1545f9c4a938cde02e107ce135e";
        private const string BloodBarrierBuffPrototype = "8c7e5e762175450798036b2ffaf09be4";
        private const string AuraBuffPrototype = "ba5aa9fc75de4801860f79dcfdc8c2de";
        private const string AreaEffectPrototype = "c2815ffe22524a79a23b863bf1c7e02c";
        private const string RootedBuffPrototype = "4f13e446713f440bbc30e8d216bc9f1e";
        private const string TelekineticShieldBuffPrototype = "33d15c5d884f40699fda6656d56973c4";
        private const string ShieldAttackAbilityPrototype = "1c5f4edca9f64fe0911bc946e0b4ea4a";
        private const string ShieldHiddenWeaponPrototype = "c26c1a7993ee4461be423cc96f885810";
        private const string StatBonusPrototype = "08e144a9788040ea81a99421b5576bc3";
        private const string Root = "Assets/Modifications/AstartesCustodes";
        private const string Art = Root + "/Art";
        private const string Blueprints = Root + "/Blueprints";
        private const string Localization = Root + "/Localization/enGB.json";
        private const string FbxPath = Art + "/PraesidiumShield.fbx";
        private const string PrefabPath = Art + "/PraesidiumShield.prefab";
        private const string BaseColorPath = Art + "/PraesidiumShield_BaseColor.png";
        private const string MetallicPath = Art + "/PraesidiumShield_Metallic.png";
        private const string NormalPath = Art + "/PraesidiumShield_Normal.png";
        private const string RoughnessPath = Art + "/PraesidiumShield_Roughness.png";
        private const string MaterialPath = Art + "/PraesidiumShield.mat";
        private const string PackedMaskPath = Art + "/PraesidiumShield_MetallicSmoothness.asset";
        private const string PackedNormalPath = Art + "/PraesidiumShield_NormalPacked.asset";
        private const string GoldBaseColorPath = Art + "/PraesidiumShield_GoldBaseColor.asset";
        private const string InventoryIconPath = Art + "/PraesidiumShield_InventoryIcon.png";

        [MenuItem("Astartes Custodes/Generate Praesidium Shield prototype")]
        public static void Generate()
        {
            Directory.CreateDirectory(Art);
            Directory.CreateDirectory(Blueprints);
            GenerateArt();
            GenerateAbilities();
            GenerateBlueprint();
            WriteLocalization();
            AssetDatabase.Refresh();
            Debug.Log("[AstartesCustodes] Praesidium Shield prototype generated.");
        }

        private static void GenerateArt()
        {
            AssetDatabase.ImportAsset(FbxPath, ImportAssetOptions.ForceSynchronousImport);
            ModelImporter importer = AssetImporter.GetAtPath(FbxPath) as ModelImporter;
            if (importer == null) throw new InvalidDataException("PraesidiumShield.fbx could not be imported.");
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.SaveAndReimport();

            GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
            if (fbx == null) throw new InvalidDataException("Praesidium Shield FBX asset is missing after import.");
            Material material = CreateMaterial();

            GameObject root = new GameObject("PraesidiumShield_Root");
            EquipmentOffsets offsets = root.AddComponent<EquipmentOffsets>();
            ConfigureEquipmentOffsets(offsets);
            AddComponentByName(root, "AstartesCustodes.Runtime.GuardianSpearMaterialBinder");
            AddComponentByName(root, "AstartesCustodes.Runtime.PraesidiumShieldBackPositionController");

            GameObject model = UnityEngine.Object.Instantiate(fbx);
            model.name = "PraesidiumShield_FBX_Model";
            model.transform.SetParent(root.transform, false);
            // The source is upright on local Z and faces along local Y, while Owlcat shield prefabs
            // use local Y as their vertical axis and local Z as their surface normal. Convert only
            // that model-space convention; Advanced Shield's off-hand bone still supplies the pose.
            // The generated mesh has a centred pivot, unlike the vanilla shield whose effective
            // grip sits above centre. Lower the visible shield without moving the inherited hand
            // attachment, and soften the initial full quarter-turn seen in the first game test.
            // X is the shield's surface normal after the axis conversion. Move the mesh away
            // from the forearm so the hand rests behind the plate instead of inside it.
            model.transform.localPosition = new Vector3(-0.08f, -0.35f, 0f);
            Quaternion alignToVanillaShieldAxes = Quaternion.Euler(-90f, 0f, 0f);
            Quaternion horizontalGripRotation = Quaternion.AngleAxis(-85f, Vector3.up);
            // Apply the horizontal turn in the prefab parent's already-upright coordinate system.
            // Putting -40 on Euler X instead would tilt the shield's top towards the ground.
            model.transform.localRotation = horizontalGripRotation * alignToVanillaShieldAxes;
            model.transform.localScale = Vector3.one * 75f;
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterial = material;

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Bounds bounds = CalculateBounds(root);
            Debug.Log($"[AstartesCustodes] Praesidium Shield prefab bounds: center={bounds.center}, size={bounds.size}");
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
        }

        private static void AddComponentByName(GameObject host, string fullName)
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => { try { return assembly.GetTypes(); } catch { return Type.EmptyTypes; } })
                .FirstOrDefault(candidate => candidate.FullName == fullName);
            if (type == null) throw new InvalidOperationException(fullName + " was not found.");
            host.AddComponent(type);
        }

        private static void ConfigureEquipmentOffsets(EquipmentOffsets offsets)
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

            // Isolated v1.2.22-based roll test: preserve facing, depth and attachment, and rotate
            // only around the Shield slot's Z axis to straighten the 45-degree diagonal lean.
            ConfigureVisualSlot(slots, 10, new Vector3(0f, 0f, 45f));
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureVisualSlot(SerializedProperty slots, int index, Vector3 rotation)
        {
            SerializedProperty slot = slots.GetArrayElementAtIndex(index);
            slot.FindPropertyRelative("Position").vector3Value = Vector3.zero;
            slot.FindPropertyRelative("Rotation").vector3Value = rotation;
        }

        private static Bounds CalculateBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return new Bounds();
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        private static Material CreateMaterial()
        {
            AssetDatabase.ImportAsset(BaseColorPath, ImportAssetOptions.ForceSynchronousImport);
            Texture2D baseColorSource = LoadPng(BaseColorPath, false);
            Texture2D baseColor = CreateGoldBaseColor(baseColorSource);
            UnityEngine.Object.DestroyImmediate(baseColorSource);
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
                material = new Material(shader) { name = "PraesidiumShield" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else material.shader = shader;
            SetTexture(material, baseColor, "_BaseMap", "_BaseColorMap", "_MainTex");
            SetTexture(material, mask, "_MetallicGlossMap", "_MaskMap", "_MasksMap");
            SetTexture(material, normal, "_BumpMap", "_NormalMap");
            SetFloat(material, 1f, "_Metallic");
            SetFloat(material, 1f, "_Smoothness");
            // Owlcat/Lit exposes roughness rather than Unity Standard's smoothness scalar.
            // Leaving this at its default 1 made the shield matte even with a polished mask.
            SetFloat(material, 0.24f, "_Roughness");
            SetFloat(material, 1f, "_SpecularHighlights", "_EnvironmentReflections");
            // Warm the bronze source towards polished auric gold while retaining the painted
            // albedo and blue gem. Values above one lift the metal highlights in Owlcat/Lit.
            SetColor(material, new Color(1.18f, 1.08f, 0.86f, 1f), "_BaseColor", "_Color", "_AdditionalAlbedoColor");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        private static Texture2D LoadPng(string assetPath, bool linear)
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, true, linear);
            if (!texture.LoadImage(File.ReadAllBytes(assetPath), false)) throw new InvalidDataException("Could not decode " + assetPath);
            return texture;
        }

        private static Texture2D PackMetallicSmoothness(Texture2D metallic, Texture2D roughness)
        {
            if (metallic.width != roughness.width || metallic.height != roughness.height)
                throw new InvalidDataException("Praesidium Shield metallic and roughness textures have different dimensions.");
            Color32[] metal = metallic.GetPixels32();
            Color32[] rough = roughness.GetPixels32();
            for (int i = 0; i < metal.Length; i++)
            {
                int sourceSmoothness = 255 - rough[i].r;
                byte polishedSmoothness = (byte)(sourceSmoothness + (255 - sourceSmoothness) * 0.78f);
                byte polishedMetal = (byte)(metal[i].r + (255 - metal[i].r) * 0.72f);
                metal[i] = new Color32(polishedMetal, 0, 0, polishedSmoothness);
            }
            Texture2D packed = new Texture2D(metallic.width, metallic.height, TextureFormat.RGBA32, true, true)
                { name = "PraesidiumShield_MetallicSmoothness" };
            packed.SetPixels32(metal);
            packed.Apply(true, false);
            return ReplaceTextureAsset(packed, PackedMaskPath);
        }

        private static Texture2D PackUnityNormal(Texture2D source)
        {
            Color32[] pixels = source.GetPixels32();
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, pixels[i].g, 255, pixels[i].r);
            Texture2D packed = new Texture2D(source.width, source.height, TextureFormat.RGBA32, true, true)
                { name = "PraesidiumShield_NormalPacked" };
            packed.SetPixels32(pixels);
            packed.Apply(true, false);
            return ReplaceTextureAsset(packed, PackedNormalPath);
        }

        private static Texture2D CreateGoldBaseColor(Texture2D source)
        {
            Color32[] pixels = source.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 pixel = pixels[i];
                // Preserve the cyan power gem and its surrounding cool highlights.
                bool cyanGem = pixel.b > pixel.r * 1.12f && pixel.g > pixel.r * 1.08f;
                if (cyanGem) continue;

                float luminance = pixel.r * 0.32f + pixel.g * 0.50f + pixel.b * 0.18f;
                pixels[i] = new Color32(
                    (byte)Mathf.Clamp(luminance * 1.58f + 18f, 0f, 255f),
                    (byte)Mathf.Clamp(luminance * 1.20f + 10f, 0f, 255f),
                    (byte)Mathf.Clamp(luminance * 0.58f + 4f, 0f, 255f),
                    pixel.a);
            }
            Texture2D gold = new Texture2D(source.width, source.height, TextureFormat.RGBA32, true, false)
                { name = "PraesidiumShield_GoldBaseColor" };
            gold.SetPixels32(pixels);
            gold.Apply(true, false);
            return ReplaceTextureAsset(gold, GoldBaseColorPath);
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
                throw new InvalidDataException("Praesidium Shield prefab could not be resolved.");
            UnityEngine.Object inventoryIcon = PrepareInventoryIcon();
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(inventoryIcon, out string iconGuid, out long iconFileId))
                throw new InvalidDataException("Praesidium Shield inventory icon could not be resolved.");

            for (int i = 0; i < ShieldGuids.Length; i++)
            {
                int tier = i + 1;
                GenerateHiddenShieldWeapon(i);
                GenerateModifierFeature(i);

                JObject shield = PrepareClone(Load(AdvancedShieldPrototype), ShieldGuids[i], AdvancedShieldPrototype);
                // The vanilla prototype is limited to wielders with specific class/faction facts
                // (for example Tech-Priest or Adeptus Astartes). Praesidium shields are intended
                // to be universally equippable, so retain all item behaviour except that gate.
                shield["Data"]["Components"] = new JArray(
                    shield["Data"]["Components"].Children<JObject>()
                        .Where(component => component["$type"]?.ToString()
                            .Contains("EquipmentRestrictionHasFacts") != true));
                shield["Data"]["m_DisplayName"] = Localized($"praesidium-v{tier}-name");
                shield["Data"]["m_Description"] = Localized($"praesidium-v{tier}-desc");
                shield["Data"]["m_FlavorText"] = Localized("praesidium-shield-flavor");
                AddOverride(shield, "m_DisplayName");
                AddOverride(shield, "m_Description");
                AddOverride(shield, "m_FlavorText");
                shield["Data"]["m_VisualParameters"]["m_WeaponModel"] = UnityReference(prefabGuid, prefabFileId);
                AddOverride(shield, "m_VisualParameters.m_WeaponModel");
                shield["Data"]["m_Icon"] = UnityReference(iconGuid, iconFileId);
                AddOverride(shield, "m_Icon");
                shield["Data"]["m_WeaponComponent"] = "!bp_" + HiddenShieldWeaponGuids[i];
                AddOverride(shield, "m_WeaponComponent");
                shield["Data"]["ItemLevel"] = tier == 6 ? 55 : i * 10 + 9;
                AddOverride(shield, "ItemLevel");
                ((JArray)shield["Data"]["Components"]).Add(CreateAddFact(ModifierFeatureGuids[i], $"praesidium-v{tier}-modifiers"));
                AddOverride(shield, "Components");
                SetShieldAbility(shield, "Ability4", AuricBastionAbilityGuids[i], 1);
                SetShieldAbility(shield, "Ability5", NullShockwaveAbilityGuids[i], 1);
                string fileName = tier == 1 ? "PraesidiumShield_Prototype_Item.jbp" : $"PraesidiumShield_V{tier}_Item.jbp";
                File.WriteAllText(Path.Combine(Blueprints, fileName), shield.ToString(Formatting.Indented));
            }
        }

        private static void GenerateHiddenShieldWeapon(int i)
        {
            int tier = i + 1;
            JObject weapon = PrepareClone(Load(ShieldHiddenWeaponPrototype), HiddenShieldWeaponGuids[i], ShieldHiddenWeaponPrototype);
            weapon["Data"]["Components"] = new JArray();
            weapon["Data"]["m_VisualParameters"]["m_WeaponModel"] = null;
            AddOverride(weapon, "Components");
            AddOverride(weapon, "m_VisualParameters.m_WeaponModel");
            SetOverride(weapon, "WarhammerDamage", ShieldDamage[i]);
            SetOverride(weapon, "WarhammerMaxDamage", ShieldDamage[i]);
            SetOverride(weapon, "CanBeUsedInGame", false);
            SetOverride(weapon, "IsUnlootable", true);
            weapon["Data"]["m_DisplayName"] = Localized($"praesidium-v{tier}-name");
            AddOverride(weapon, "m_DisplayName");
            File.WriteAllText(Path.Combine(Blueprints, $"PraesidiumShield_V{tier}_HiddenWeapon_Item.jbp"), weapon.ToString(Formatting.Indented));
        }

        private static void GenerateModifierFeature(int i)
        {
            int tier = i + 1;
            JObject feature = PrepareClone(Load(StatBonusPrototype), ModifierFeatureGuids[i], StatBonusPrototype);
            JArray components = new JArray();
            if (AttributeBonus[i] > 0)
            {
                components.Add(CreateStatBonus("WarhammerStrength", AttributeBonus[i], $"praesidium-v{tier}-strength"));
                components.Add(CreateStatBonus("WarhammerToughness", AttributeBonus[i], $"praesidium-v{tier}-toughness"));
            }
            feature["Data"]["Components"] = components;
            feature["Data"]["m_DisplayName"] = Localized($"praesidium-v{tier}-modifier-name");
            feature["Data"]["m_Description"] = Localized($"praesidium-v{tier}-modifier-desc");
            feature["Data"]["HideInUI"] = false;
            feature["Data"]["HideInCharacterSheetAndLevelUp"] = true;
            foreach (string property in new[] { "Components", "m_DisplayName", "m_Description", "HideInUI", "HideInCharacterSheetAndLevelUp" })
                AddOverride(feature, property);
            File.WriteAllText(Path.Combine(Blueprints, $"PraesidiumShield_V{tier}_Modifiers_Feature.jbp"), feature.ToString(Formatting.Indented));
        }

        private static JObject CreateStatBonus(string stat, int value, string name)
        {
            JObject component = (JObject)Load(StatBonusPrototype)["Data"]["Components"].Children<JObject>()
                .First(item => item["$type"]?.ToString().Contains("AddStatBonus") == true).DeepClone();
            component["name"] = "$AddStatBonus$" + name;
            component["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            component["m_Overrides"] = new JArray();
            component["Stat"] = stat;
            component["Value"] = value;
            return component;
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

        private static void SetOverride(JObject root, string property, JToken value)
        {
            root["Data"][property] = value;
            AddOverride(root, property);
        }

        private static UnityEngine.Object PrepareInventoryIcon()
        {
            AssetDatabase.ImportAsset(InventoryIconPath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(InventoryIconPath) as TextureImporter;
            if (importer == null) throw new InvalidDataException("Praesidium Shield inventory icon importer was not found.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(InventoryIconPath).FirstOrDefault(asset => asset is Sprite)
                ?? AssetDatabase.LoadMainAssetAtPath(InventoryIconPath);
        }

        private static void SetShieldAbility(JObject shield, string slot, string abilityGuid, int ap)
        {
            JObject ability = (JObject)shield["Data"]["AbilityContainer"][slot];
            ability["Type"] = "Custom";
            ability["Mode"] = "Default";
            ability["m_Ability"] = "!bp_" + abilityGuid;
            ability["m_FXSettings"] = null;
            ability["AP"] = ap;
            foreach (string field in new[] { "Type", "Mode", "m_Ability", "m_FXSettings", "AP" })
                AddOverride(shield, "AbilityContainer." + slot + "." + field);
        }

        private static void GenerateAbilities()
        {
            GenerateAuricBastion();
            GenerateNullShockwave();
        }

        private static void GenerateAuricBastion()
        {
            JObject effect = PrepareClone(Load(ForceFieldBuffPrototype), AuricBastionEffectBuffGuid, ForceFieldBuffPrototype);
            SetText(effect, "praesidium-bastion-effect-name", "praesidium-bastion-effect-desc");
            JObject nullifier = effect["Data"]["Components"].Children<JObject>()
                .First(component => component["$type"]?.ToString().Contains("WarhammerIncomingDamageNullifier") == true);
            nullifier["m_NullifyChances"]["ValueType"] = "Simple";
            nullifier["m_NullifyChances"]["Value"] = 100;
            nullifier["m_Overrides"] = new JArray("m_NullifyChances");
            JObject visual = (JObject)Load(TelekineticShieldBuffPrototype)["Data"]["Components"].Children<JObject>()
                .First(component => component["$type"]?.ToString().Contains("AddVisualForceShield") == true).DeepClone();
            visual["name"] = "$AddVisualForceShield$PraesidiumAuricBastion";
            visual["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            visual["m_Overrides"] = new JArray();
            ((JArray)effect["Data"]["Components"]).Add(visual);
            effect["Data"]["m_Icon"] = UnityReference("01334144738df8b43871501b756957f8", 21300000);
            effect["Data"]["IsImportantBuff"] = true;
            AddOverride(effect, "m_Icon");
            AddOverride(effect, "IsImportantBuff");
            File.WriteAllText(Path.Combine(Blueprints, "PraesidiumShield_AuricBastion_EffectBuff.jbp"), effect.ToString(Formatting.Indented));

            JObject area = PrepareClone(Load(AreaEffectPrototype), AuricBastionAreaGuid, AreaEffectPrototype);
            JObject areaBuff = area["Data"]["Components"].Children<JObject>()
                .First(component => component["$type"]?.ToString().Contains("AbilityAreaEffectBuff") == true);
            areaBuff["m_Buff"] = "!bp_" + AuricBastionEffectBuffGuid;
            areaBuff["m_Overrides"] = new JArray("m_Buff");
            area["Data"]["TargetType"] = "Ally";
            area["Data"]["m_Pattern"]["m_Radius"] = 2;
            area["Data"]["m_Overrides"] = new JArray("TargetType", "m_Pattern");
            File.WriteAllText(Path.Combine(Blueprints, "PraesidiumShield_AuricBastion_Area.jbp"), area.ToString(Formatting.Indented));

            JObject casterBuff = PrepareClone(Load(AuraBuffPrototype), AuricBastionCasterBuffGuid, AuraBuffPrototype);
            SetText(casterBuff, "praesidium-bastion-name", "praesidium-bastion-desc");
            JObject addArea = casterBuff["Data"]["Components"].Children<JObject>()
                .First(component => component["$type"]?.ToString().Contains("AddAreaEffect") == true);
            addArea["m_AreaEffect"] = "!bp_" + AuricBastionAreaGuid;
            addArea["m_Overrides"] = new JArray("m_AreaEffect");
            File.WriteAllText(Path.Combine(Blueprints, "PraesidiumShield_AuricBastion_CasterBuff.jbp"), casterBuff.ToString(Formatting.Indented));

            for (int i = 0; i < AuricBastionAbilityGuids.Length; i++)
            {
                int tier = i + 1;
                JObject ability = PrepareClone(Load(ForceFieldAbilityPrototype), AuricBastionAbilityGuids[i], ForceFieldAbilityPrototype);
                ConfigureAuraAbility(ability, "praesidium-bastion-name", $"praesidium-bastion-v{tier}-desc",
                    AuricBastionCasterBuffGuid, 1, CooldownRounds[i], "2424dc68649196d4b9db0f3bb2a0f4ba");
                string fileName = tier == 1 ? "PraesidiumShield_AuricBastion_Ability.jbp" : $"PraesidiumShield_V{tier}_AuricBastion_Ability.jbp";
                File.WriteAllText(Path.Combine(Blueprints, fileName), ability.ToString(Formatting.Indented));
            }
        }

        private static void GenerateNullShockwave()
        {
            JObject mute = PrepareClone(Load(RootedBuffPrototype), NullZoneMuteBuffGuid, RootedBuffPrototype);
            SetText(mute, "praesidium-null-mute-name", "praesidium-null-mute-desc");
            JObject restriction = new JObject
            {
                ["$type"] = "9ed84940fa824243a3922d86ae07aadc, AbilitySourceLimitation",
                ["name"] = "$AbilitySourceLimitation$PraesidiumNullZone",
                ["m_Flags"] = 0,
                ["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" },
                ["m_Overrides"] = new JArray(),
                ["Sources"] = "PsychicPower"
            };
            // Rooted_CommonBuff is used only as a schema source. Do not inherit its movement,
            // hard-CC or AP-penalty components: this buff must block psychic powers only.
            mute["Data"]["Components"] = new JArray(restriction);
            mute["Data"]["m_Icon"] = UnityReference("a0f204dee7673cc4c891096621c499f2", 21300000);
            mute["Data"]["IsImportantBuff"] = true;
            AddOverride(mute, "Components");
            AddOverride(mute, "m_Icon");
            AddOverride(mute, "IsImportantBuff");
            File.WriteAllText(Path.Combine(Blueprints, "PraesidiumShield_NullZone_MuteBuff.jbp"), mute.ToString(Formatting.Indented));

            JObject area = PrepareClone(Load(AreaEffectPrototype), NullZoneAreaGuid, AreaEffectPrototype);
            JObject areaBuff = area["Data"]["Components"].Children<JObject>()
                .First(component => component["$type"]?.ToString().Contains("AbilityAreaEffectBuff") == true);
            areaBuff["m_Buff"] = "!bp_" + NullZoneMuteBuffGuid;
            areaBuff["m_Overrides"] = new JArray("m_Buff");
            area["Data"]["TargetType"] = "Any";
            area["Data"]["m_Pattern"]["m_Radius"] = 7;
            area["Data"]["m_Overrides"] = new JArray("TargetType", "m_Pattern");
            File.WriteAllText(Path.Combine(Blueprints, "PraesidiumShield_NullZone_Area.jbp"), area.ToString(Formatting.Indented));

            JObject casterBuff = PrepareClone(Load(AuraBuffPrototype), NullZoneCasterBuffGuid, AuraBuffPrototype);
            SetText(casterBuff, "praesidium-null-zone-name", "praesidium-null-zone-desc");
            JObject addArea = casterBuff["Data"]["Components"].Children<JObject>()
                .First(component => component["$type"]?.ToString().Contains("AddAreaEffect") == true);
            addArea["m_AreaEffect"] = "!bp_" + NullZoneAreaGuid;
            addArea["m_Overrides"] = new JArray("m_AreaEffect");
            File.WriteAllText(Path.Combine(Blueprints, "PraesidiumShield_NullZone_CasterBuff.jbp"), casterBuff.ToString(Formatting.Indented));

            for (int i = 0; i < NullShockwaveAbilityGuids.Length; i++)
            {
                int tier = i + 1;
                JObject ability = PrepareClone(Load(ForceFieldAbilityPrototype), NullShockwaveAbilityGuids[i], ForceFieldAbilityPrototype);
                ConfigureAuraAbility(ability, "praesidium-null-zone-name", $"praesidium-null-zone-v{tier}-desc",
                    NullZoneCasterBuffGuid, 1, CooldownRounds[i],
                    // Vanilla Mindbreak: a character clutching their head under psychic pressure.
                    "91a785da5a6da494998b503f0462706e");
                JObject runAction = ability["Data"]["Components"].Children<JObject>()
                    .First(component => component["$type"]?.ToString().Contains("AbilityEffectRunAction") == true);
                runAction["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
                runAction["m_Overrides"] = new JArray();
                JObject spawnFx = (JObject)Load(ForceFieldBuffPrototype)["Data"]["Components"].Children<JObject>()
                    .First(component => component["$type"]?.ToString().Contains("AddIncomingDamageTrigger") == true)
                    ["Actions"]["Actions"].Children<JObject>().First().DeepClone();
                spawnFx["name"] = $"$ContextActionSpawnFx$PraesidiumNullShockwaveV{tier}";
                spawnFx["PrefabLink"]["AssetId"] = "359c978c57ddbba46bcd3187254badf5";
                ((JArray)runAction["Actions"]["Actions"]).Insert(0, spawnFx);
                string fileName = tier == 1 ? "PraesidiumShield_NullShockwave_Ability.jbp" : $"PraesidiumShield_V{tier}_NullShockwave_Ability.jbp";
                File.WriteAllText(Path.Combine(Blueprints, fileName), ability.ToString(Formatting.Indented));
            }
        }

        private static void ConfigureAuraAbility(JObject ability, string nameKey, string descKey, string buffGuid,
            int rounds, int cooldownRounds, string iconGuid)
        {
            SetText(ability, nameKey, descKey);
            JObject runAction = ability["Data"]["Components"].Children<JObject>()
                .First(component => component["$type"]?.ToString().Contains("AbilityEffectRunAction") == true);
            JObject apply = runAction["Actions"]["Actions"].Children<JObject>()
                .First(action => action["$type"]?.ToString().Contains("ContextActionApplyBuff") == true);
            apply["m_Buff"] = "!bp_" + buffGuid;
            apply["ToCaster"] = true;
            apply["BuffEndCondition"] = "CombatEnd";
            JObject duration = ((apply["DurationValue"] ?? apply["Duration"]) as JObject) ?? new JObject();
            JObject bonus = (duration["BonusValue"] as JObject) ?? (duration["m_BonusValue"] as JObject) ?? SimpleContextValue(0);
            JObject roundValue = (duration["RoundsValue"] as JObject) ?? (duration["m_RoundsValue"] as JObject) ?? SimpleContextValue(rounds);
            bonus["Value"] = 0;
            roundValue["Value"] = rounds;
            duration["BonusValue"] = bonus;
            duration["RoundsValue"] = roundValue;
            apply["DurationValue"] = duration;
            JObject shieldAnimation = (JObject)Load(ShieldAttackAbilityPrototype)["Data"]["Components"].Children<JObject>()
                .First(component => component["$type"]?.ToString().Contains("AbilityCustomAnimationByBuff") == true).DeepClone();
            shieldAnimation["name"] = "$AbilityCustomAnimationByBuff$PraesidiumShieldActivation";
            shieldAnimation["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            shieldAnimation["m_Overrides"] = new JArray();
            ability["Data"]["Components"] = new JArray(runAction, shieldAnimation);
            ability["Data"]["Type"] = "Spell";
            ability["Data"]["Range"] = "Personal";
            ability["Data"]["ActionPointCost"] = 1;
            ability["Data"]["AbilityParamsSource"] = "None";
            // BlueprintAbility has no PsychicPower.None enum value in the retail game.
            // Keep the valid value inherited from the vanilla Force Field ability.
            ability["Data"]["PsychicPower"] = "Minor";
            ability["Data"]["VeilThicknessPointsToAdd"] = 0;
            ability["Data"]["CooldownRounds"] = cooldownRounds;
            ability["Data"]["m_Icon"] = UnityReference(iconGuid, 21300000);
            ability["Data"]["Animation"] = "Kick";
            ability["Data"]["CombatStateRestriction"] = "InCombatOnly";
            ability["Data"]["m_Overrides"] = new JArray(
                "Components", "m_DisplayName", "m_Description", "Type", "Range", "ActionPointCost",
                "AbilityParamsSource", "PsychicPower", "VeilThicknessPointsToAdd", "CooldownRounds",
                "m_Icon", "Animation", "CombatStateRestriction");
        }

        private static JObject PsychicPowerGetter(string power, string name) => new JObject
        {
            ["$type"] = "35bdf749faa52ec4cbe9a8e1e733ee7d, CheckAbilityPsychicPowerTypeGetter",
            ["name"] = name,
            ["Settings"] = new JObject
            {
                ["Progression"] = "AsIs", ["m_CustomProgression"] = new JArray(), ["m_StartLevel"] = 0,
                ["m_StepLevel"] = 0, ["Negate"] = false, ["Limit"] = "None", ["Min"] = 0, ["Max"] = 0
            },
            ["m_PowerType"] = power
        };

        private static JObject SimpleContextValue(int value) => new JObject
        {
            ["ValueType"] = "Simple", ["Value"] = value, ["ValueRank"] = "Default",
            ["ValueShared"] = "Damage", ["Property"] = "None", ["m_CustomProperty"] = null,
            ["PropertyName"] = "Value1"
        };

        private static void SetText(JObject root, string nameKey, string descKey)
        {
            root["Data"]["m_DisplayName"] = Localized(nameKey);
            root["Data"]["m_Description"] = Localized(descKey);
            AddOverride(root, "m_DisplayName");
            AddOverride(root, "m_Description");
        }

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
            if (root["Meta"] is JObject meta) meta["ShadowDeleted"] = false;
            foreach (JObject component in root["Data"]["Components"].Children<JObject>())
            {
                component["PrototypeLink"] = new JObject { ["guid"] = prototype, ["name"] = component["name"]?.ToString() ?? "" };
                component["m_Overrides"] = new JArray();
            }
            return root;
        }

        private static JObject Localized(string key) => new JObject
        {
            ["m_Key"] = key, ["m_OwnerString"] = "", ["m_OwnerPropertyPath"] = "",
            ["m_JsonPath"] = "", ["Shared"] = null
        };
        private static JObject UnityReference(string guid, long fileId) => new JObject { ["guid"] = guid, ["fileid"] = fileId };
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
            strings["praesidium-shield-flavor"] = Entry("An auric bulwark bearing the twin eagle, raised between the Emperor's servants and annihilation.");
            strings["praesidium-bastion-name"] = Entry("Auric Bastion");
            strings["praesidium-bastion-effect-name"] = Entry("Auric Bastion");
            strings["praesidium-bastion-effect-desc"] = Entry("All incoming damage is nullified while the unit remains within the Praesidium Shield's protective field.");
            strings["praesidium-null-zone-name"] = Entry("Null Shockwave");
            strings["praesidium-null-mute-name"] = Entry("Null-Muted");
            strings["praesidium-null-mute-desc"] = Entry("Psychic Power abilities cannot be used while this unit remains within the Praesidium Shield's null zone. Weapon, item and Navigator abilities remain available.");
            for (int i = 0; i < ShieldGuids.Length; i++)
            {
                int tier = i + 1;
                string[] levelRanges = { "1-15", "16-25", "26-35", "36-43", "44-49", "50-55" };
                string levels = levelRanges[i];
                string attributes = AttributeBonus[i] == 0
                    ? ""
                    : $"\n• +{AttributeBonus[i]} Strength\n• +{AttributeBonus[i]} Toughness";
                strings[$"praesidium-v{tier}-name"] = Entry(TierNames[i]);
                strings[$"praesidium-v{tier}-desc"] = Entry(
                    $"A Custodes Praesidium Shield.\n\n• Levels: {levels}\n• Shield Bash damage: {ShieldDamage[i]}\n" +
                    $"• Shield block chance: 50%\n• Auric Bastion cooldown: {CooldownRounds[i]} rounds\n" +
                    $"• Null Shockwave cooldown: {CooldownRounds[i]} rounds{attributes}");
                strings[$"praesidium-v{tier}-modifier-name"] = Entry(TierNames[i] + " — Wielder Bonuses");
                strings[$"praesidium-v{tier}-modifier-desc"] = Entry(AttributeBonus[i] == 0
                    ? "This shield grants no additional characteristic bonus at its current tier."
                    : $"While equipped: +{AttributeBonus[i]} Strength and +{AttributeBonus[i]} Toughness.");
                strings[$"praesidium-bastion-v{tier}-desc"] = Entry(
                    $"Raise the Praesidium Shield's auric field. For 1 round, the wielder and allies within 2 cells gain individual force shields that nullify all incoming damage. Cost: 1 AP. Cooldown: {CooldownRounds[i]} rounds.");
                strings[$"praesidium-null-zone-v{tier}-desc"] = Entry(
                    $"Release a nullifying shockwave from the Praesidium Shield. For 1 round, all units within 7 cells—including allies—cannot use Psychic Power abilities. Weapon, item and Navigator abilities are unaffected. Cost: 1 AP. Cooldown: {CooldownRounds[i]} rounds.");
            }
        }

        private static JObject Entry(string text) => new JObject { ["Offset"] = 0, ["Text"] = text };
    }
}
