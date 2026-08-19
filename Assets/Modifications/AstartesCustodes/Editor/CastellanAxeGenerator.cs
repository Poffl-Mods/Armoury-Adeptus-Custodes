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

        private const string OnslaughtWeaponGuid = "8bdfb393b52546d8ad15fe00e16ae41e";
        private const string SweepWeaponGuid = "1a154907fa884e9d96ee41991500ed61";
        private const string EvisceratorTripleAttack = "a6ec10d23b0f49698e17f30e70423615";
        private const string GreatSword = "88863b6b0c61404b96b01c2bc648ba5e";
        private const string MeleeFx = "046cf83ca27244998b0603750d4a833e";
        private const string BolterFx = "afde0e8c0c9848deba8e38a1279ee7df";
        private const string StandardReload = "98f4a31b68e446ad9c63411c7b349146";

        private const string Root = "Assets/Modifications/AstartesCustodes";
        private const string Art = Root + "/Art";
        private const string Blueprints = Root + "/Blueprints";
        private const string SourceBlueprint = Blueprints + "/GuardianSpear_Prototype_Item.jbp";
        private const string GuardianCleaveBlueprint = Blueprints + "/GuardianSpear_Cleave_Ability.jbp";
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
        private const string BolterMuzzleLocator = "502467bbbcc0471285a4ab6936a285d8";

        [MenuItem("Astartes Custodes/Generate Castellan Axe V1 prototype")]
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
            Debug.Log("[AstartesCustodes] Castellan Axe V1 visual prototype generated: " + WeaponGuid);
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
            // Rotate 105 degrees from the v1.3.10 baseline (200 degrees), i.e. another
            // 15 degrees in the chosen direction beyond the previous 90-degree midpoint.
            Quaternion turnBladeForward = Quaternion.AngleAxis(95f, Vector3.right);
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
            JObject weapon = JObject.Parse(File.ReadAllText(SourceBlueprint));
            weapon["AssetId"] = WeaponGuid;
            weapon["Data"]["m_DisplayName"]["m_Key"] = "castellan-axe-v1-name";
            weapon["Data"]["m_Description"]["m_Key"] = "castellan-axe-v1-desc";
            weapon["Data"]["m_FlavorText"]["m_Key"] = "castellan-axe-flavor";
            weapon["Data"]["m_VisualParameters"]["m_WeaponAnimationStyle"] = "BrutalTwoHanded";
            JArray overrides = (JArray)weapon["Data"]["m_Overrides"];
            if (!overrides.Any(value => value.ToString() == "m_VisualParameters.m_WeaponAnimationStyle"))
                overrides.Add("m_VisualParameters.m_WeaponAnimationStyle");
            weapon["Data"]["m_VisualParameters"]["m_WeaponModel"] = new JObject
            {
                ["guid"] = prefabGuid,
                ["fileid"] = prefabFileId
            };
            GuardianSpearGenerator.SetAbilitySlot(weapon, "Ability1", "SingleShot", OnslaughtGuid, MeleeFx, 2);
            GuardianSpearGenerator.SetAbilitySlot(weapon, "Ability2", "AOE", SweepGuid, MeleeFx, 2);
            GuardianSpearGenerator.SetAbilitySlot(weapon, "Ability3", "SingleShot", GuardianSpearGenerator.BoltShot, BolterFx, 1);
            GuardianSpearGenerator.SetAbilitySlot(weapon, "Ability4", "Burst", GuardianSpearGenerator.BoltBurst, BolterFx, 2);
            GuardianSpearGenerator.SetAbilitySlot(weapon, "Ability5", "Reload", StandardReload, BolterFx, 2, "Any");
            weapon["Data"]["m_AttackOfOpportunityAbility"] = "!bp_" + GuardianSpearGenerator.GuardianStrike;
            GuardianSpearGenerator.AddOverride(weapon, "m_AttackOfOpportunityAbility");
            File.WriteAllText(OutputBlueprint, weapon.ToString(Formatting.Indented));
        }

        private static void GenerateCombatBlueprints()
        {
            CreateHiddenMeleeWeapon(OnslaughtWeaponGuid, 9, 13, 20, "CastellanAxe_V1_HiddenOnslaught_Item");
            CreateHiddenMeleeWeapon(SweepWeaponGuid, 8, 12, 15, "CastellanAxe_V1_HiddenSweep_Item");

            JObject onslaught = GuardianSpearGenerator.PrepareClone(
                GuardianSpearGenerator.Load(EvisceratorTripleAttack), OnslaughtGuid, EvisceratorTripleAttack);
            JObject repeat = (JObject)onslaught["Data"]["Components"].Children<JObject>()
                .SelectMany(component => component.DescendantsAndSelf().OfType<JObject>())
                .First(component => component["$type"]?.ToString().Contains("ContextActionRepeat") == true);
            repeat["RepeatNumber"]["Value"] = 2;
            AddWeaponOverride(onslaught, OnslaughtWeaponGuid, "$WarhammerOverrideAbilityWeapon$castellan-axe-v1-onslaught");
            onslaught["Data"]["m_DisplayName"] = GuardianSpearGenerator.Localized("castellan-onslaught-name");
            onslaught["Data"]["m_Description"] = GuardianSpearGenerator.Localized("castellan-onslaught-v1-desc");
            GuardianSpearGenerator.AddOverride(onslaught, "m_DisplayName");
            GuardianSpearGenerator.AddOverride(onslaught, "m_Description");
            GuardianSpearGenerator.Save("CastellanAxe_V1_Onslaught_Ability", onslaught);

            JObject sweep = GuardianSpearGenerator.PrepareClone(
                JObject.Parse(File.ReadAllText(GuardianCleaveBlueprint)), SweepGuid, GuardianSpearGenerator.GuardianCleave);
            JObject oldOverride = sweep["Data"]["Components"].Children<JObject>()
                .First(component => component["$type"]?.ToString().Contains("WarhammerOverrideAbilityWeapon") == true);
            ((JArray)sweep["Data"]["Components"]).Remove(oldOverride);
            ((JArray)sweep["Data"]["m_Overrides"]).Remove(oldOverride["name"]?.ToString());
            AddWeaponOverride(sweep, SweepWeaponGuid, "$WarhammerOverrideAbilityWeapon$castellan-axe-v1-sweep");
            sweep["Data"]["m_DisplayName"] = GuardianSpearGenerator.Localized("castellan-sweep-name");
            sweep["Data"]["m_Description"] = GuardianSpearGenerator.Localized("castellan-sweep-v1-desc");
            GuardianSpearGenerator.AddOverride(sweep, "m_DisplayName");
            GuardianSpearGenerator.AddOverride(sweep, "m_Description");
            GuardianSpearGenerator.Save("CastellanAxe_V1_Sweep_Ability", sweep);
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
            strings["castellan-axe-v1-name"] = Entry("Castellan Axe — V1");
            strings["castellan-axe-v1-desc"] = Entry("A master-crafted Custodes hybrid weapon built for overwhelming a single foe. Castellan Onslaught strikes twice, while its compact sweep trades reach and damage for crowd control.");
            strings["castellan-axe-flavor"] = Entry("A master-crafted polearm combining a power axe with an integrated bolt weapon.");
            strings["castellan-onslaught-name"] = Entry("Castellan Onslaught");
            strings["castellan-onslaught-v1-desc"] = Entry("Strike the same target twice. Each hit deals 9–13 damage with 20% armour penetration and is resolved separately.");
            strings["castellan-sweep-name"] = Entry("Castellan Sweep");
            strings["castellan-sweep-v1-desc"] = Entry("Make a short sweeping attack, dealing 8–12 damage with 15% armour penetration to enemies in a small area.");
            File.WriteAllText(path, root.ToString(Formatting.Indented));
        }

        private static JObject Entry(string text) => new JObject { ["Offset"] = 0, ["Text"] = text };
    }
}
