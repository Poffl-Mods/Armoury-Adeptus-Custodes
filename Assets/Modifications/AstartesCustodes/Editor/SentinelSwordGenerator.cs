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
        internal const string PowerFieldAbilityGuid = "7bd2c146f08a4e5489c991acee473721";
        internal const string PowerFieldBuffGuid = "c89d128481df4fe096639d1d7d8c50af";
        internal const string SentinelWaveAbilityGuid = "641128fea4664f61a734b46f6085ac8e";

        private const string Root = "Assets/Modifications/AstartesCustodes";
        private const string Art = Root + "/Art";
        private const string Blueprints = Root + "/Blueprints";
        private const string Localization = Root + "/Localization/enGB.json";
        private const string FbxPath = Art + "/SentinelSword.fbx";
        private const string PrefabPath = Art + "/SentinelSword.prefab";
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
        private const string SwordAttackFxGuid = "1bc92b9832fe402caa887d8c5d990cb4";
        private const string TelekinesisIconGuid = "d2db9cd1a850eba4790dac666bad955e";
        // Vanilla Aeldari Force Sword activation: an energy-weapon self-buff icon rather than a caster hand.
        private const string PowerFieldIconGuid = "35279bc29c0d21649ad4157b24b22c7a";
        // Built-in two-handed sword attack artwork; used temporarily as an unmistakable sword inventory icon.
        private const string SwordIconGuid = "a6cba97367839af4e8869281de029095";

        [MenuItem("Astartes Custodes/Generate Sentinel Sword")]
        public static void Generate()
        {
            Directory.CreateDirectory(Art);
            Directory.CreateDirectory(Blueprints);
            GenerateArt();
            GenerateBlueprint();
            WriteLocalization();
            AssetDatabase.Refresh();
            Debug.Log("[AstartesCustodes] Sentinel Sword base weapon generated: " + WeaponGuid);
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

            GeneratePowerFieldBlueprints();
            GenerateSentinelWaveBlueprint();

            JObject weapon = PrepareClone(Load(PowerSwordPrototype), WeaponGuid, PowerSwordPrototype);
            weapon["Data"]["Components"] = new JArray
            {
                new JObject
                {
                    ["$type"] = "65221a9a6133bd0408b019b86642d97e, AddFactToEquipmentWielder",
                    ["name"] = "$AddFactToEquipmentWielder$sentinel-power-field",
                    ["m_Flags"] = 0,
                    ["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" },
                    ["m_Overrides"] = new JArray(),
                    ["m_Fact"] = "!bp_" + PowerFieldAbilityGuid
                }
            };
            SetText(weapon, "sentinel-sword-name", "sentinel-sword-desc", "sentinel-sword-flavor");
            weapon["Data"]["m_Icon"] = UnityReference(SwordIconGuid, 21300000L);
            AddOverride(weapon, "m_Icon");
            weapon["Data"]["m_VisualParameters"]["m_WeaponModel"] = UnityReference(prefabGuid, prefabFileId);
            AddOverride(weapon, "m_VisualParameters.m_WeaponModel");
            Override(weapon, "Family", "Power");
            Override(weapon, "Classification", "Sword");
            Override(weapon, "m_HoldingType", "OneHanded");
            Override(weapon, "IsTwoHanded", false);
            Override(weapon, "m_Enchantments", new JArray());
            Override(weapon, "m_Rarity", "Pattern");
            Override(weapon, "CanBeUsedInGame", true);
            Override(weapon, "IsUnlootable", false);
            Override(weapon, "IsNonRemovable", false);
            Override(weapon, "m_IsNotable", true);
            SetWeaponAbility(weapon, "Ability4", "Custom", PowerFieldAbilityGuid, null, 0);
            SetWeaponAbility(weapon, "Ability5", "Custom", SentinelWaveAbilityGuid, SwordAttackFxGuid, 1);
            File.WriteAllText(Path.Combine(Blueprints, "SentinelSword_Item.jbp"), weapon.ToString(Formatting.Indented));
        }

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

        private static void GeneratePowerFieldBlueprints()
        {
            JObject buff = PrepareClone(Load(PowerFieldBuffPrototype), PowerFieldBuffGuid, PowerFieldBuffPrototype);
            JObject damageModifier = buff["Data"]["Components"].Children<JObject>()
                .First(component => component["$type"]?.ToString().Contains("WarhammerDamageModifierInitiator") == true);
            damageModifier["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            damageModifier["m_Overrides"] = new JArray();
            JObject weaponGetter = damageModifier["Restrictions"]?["Property"]?["Getters"]?.Children<JObject>()
                .First(getter => getter["$type"]?.ToString().Contains("CheckAbilityWeaponBlueprintGetter") == true);
            weaponGetter["m_Weapon"] = "!bp_" + WeaponGuid;
            JObject flatDamage = (JObject)damageModifier["UnmodifiableFlatDamageModifier"];
            flatDamage["ValueType"] = "Simple";
            flatDamage["Value"] = 6;
            flatDamage["Property"] = "None";
            flatDamage["m_CustomProperty"] = null;
            flatDamage["PropertyName"] = "Value1";
            flatDamage["Enabled"] = true;
            buff["Data"]["Components"] = new JArray(damageModifier);
            buff["Data"]["m_DisplayName"] = Localized("sentinel-power-field-name");
            buff["Data"]["m_Description"] = Localized("sentinel-power-field-buff-desc");
            buff["Data"]["m_Flags"] = 0;
            buff["Data"]["Stacking"] = "Replace";
            buff["Data"]["FxOnStart"] = new JObject { ["AssetId"] = "cf6b6016a28a1bb42aef4576da77ebb4" };
            AddOverride(buff, "m_DisplayName");
            AddOverride(buff, "m_Description");
            File.WriteAllText(Path.Combine(Blueprints, "SentinelSword_PowerField_Buff.jbp"), buff.ToString(Formatting.Indented));

            JObject ability = PrepareClone(Load(PowerFieldAbilityPrototype), PowerFieldAbilityGuid, PowerFieldAbilityPrototype);
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
                                ["m_Buff"] = "!bp_" + PowerFieldBuffGuid,
                                ["BuffEndCondition"] = "CombatEnd",
                                ["Permanent"] = false,
                                ["DurationValue"] = new JObject
                                {
                                    ["BonusValue"] = SimpleContextValue(0),
                                    ["RoundsValue"] = SimpleContextValue(4)
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
            ability["Data"]["m_Description"] = Localized("sentinel-power-field-desc");
            ability["Data"]["m_Icon"] = UnityReference(PowerFieldIconGuid, 21300000L);
            ability["Data"]["ActionPointCost"] = 0;
            ability["Data"]["CooldownRounds"] = 5;
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
            File.WriteAllText(Path.Combine(Blueprints, "SentinelSword_PowerField_Ability.jbp"), ability.ToString(Formatting.Indented));
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
            ability["Data"]["PsychicPower"] = "None";
            ability["Data"]["VeilThicknessPointsToAdd"] = 0;
            ability["Data"]["CooldownRounds"] = 0;
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
            strings["sentinel-sword-name"] = Entry("Sentinel Sword");
            strings["sentinel-sword-desc"] = Entry("A master-crafted power sword of the Adeptus Custodes.");
            strings["sentinel-sword-flavor"] = Entry("A gleaming blade fashioned for the unwavering guardians of the Golden Throne.");
            strings["sentinel-power-field-name"] = Entry("Activate Power Field");
            strings["sentinel-power-field-desc"] = Entry("Activates the Sentinel Sword's power field for 4 rounds. Attacks made with this weapon deal +6 additional damage. Cooldown: 5 rounds.");
            strings["sentinel-power-field-buff-desc"] = Entry("The Sentinel Sword is energised. Its attacks deal +6 additional damage.");
            strings["sentinel-wave-name"] = Entry("Sentinel Wave");
            strings["sentinel-wave-desc"] = Entry("Swing the Sentinel Sword to project a cutting wave of force at an enemy up to 5 cells away. The attack uses the weapon's normal damage and armour penetration. Cost: 1 AP.");
            File.WriteAllText(Localization, document.ToString(Formatting.Indented));
        }

        private static JObject Entry(string text) => new JObject { ["Offset"] = 0, ["Text"] = text };
    }
}
