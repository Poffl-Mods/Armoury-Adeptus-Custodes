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
            slots.arraySize = 13;
            for (int i = 0; i < slots.arraySize; i++)
            {
                SerializedProperty slot = slots.GetArrayElementAtIndex(i);
                slot.FindPropertyRelative("Position").vector3Value = Vector3.zero;
                slot.FindPropertyRelative("Rotation").vector3Value = Vector3.zero;
            }
            foreach (int index in new[] { 1, 3, 6, 8 })
                slots.GetArrayElementAtIndex(index).FindPropertyRelative("Position").vector3Value =
                    new Vector3(0f, 0f, -0.20f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
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

        private static void GenerateBlueprint()
        {
            UnityEngine.Object prefab = AssetDatabase.LoadMainAssetAtPath(PrefabPath);
            if (prefab == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string prefabGuid, out long prefabFileId))
                throw new InvalidDataException("Sentinel Sword prefab could not be resolved.");

            JObject weapon = PrepareClone(Load(PowerSwordPrototype), WeaponGuid, PowerSwordPrototype);
            weapon["Data"]["Components"] = new JArray();
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
            File.WriteAllText(Path.Combine(Blueprints, "SentinelSword_Item.jbp"), weapon.ToString(Formatting.Indented));
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
            File.WriteAllText(Localization, document.ToString(Formatting.Indented));
        }

        private static JObject Entry(string text) => new JObject { ["Offset"] = 0, ["Text"] = text };
    }
}
