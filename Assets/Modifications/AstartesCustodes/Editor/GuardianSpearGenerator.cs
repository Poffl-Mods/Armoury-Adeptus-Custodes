using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OwlcatModification.Editor;
using OwlcatModification.Editor.Build;
using UnityEditor;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.View.Equipment;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace AstartesCustodes.Editor
{
    internal static class GuardianSpearGenerator
    {
        internal const string VisibleWeapon = "69a10b7bc7a94c5cb59cd91a6d88d160";
        internal const string HiddenBolter = "94fb9c35f58b4442bb7b17f660257f2f";
        internal const string BoltShot = "747e419a3f9c43579f51b27f41e88b35";

        private const string ImperialStaff = "993996a4c0a24463aa400b9441d4caa8";
        private const string AstartesBoltPistol = "5e1bae4c2c7e4bd99411173f8dbe74f0";
        private const string StandardBoltShot = "6a7f0c4523c34de7829c088556b62f11";
        private const string StandardBoltBurst = "347d38e3abad490dad41ee7b77092b24";
        private const string Vindictor = "0a5e8b407f9940589d44675f42783581";
        private const string VindictorHiddenMelee = "91ab9da13b8848aab46bd885a0199db3";
        private const string VindictorMeleeSingle = "84c32baad3f14585a32f5747d721dfc3";
        private const string VindictorMeleeAoe = "9098215cb3aa482d9c44b9c03a17b8cb";
        private const string BolterFx = "afde0e8c0c9848deba8e38a1279ee7df";
        private const string BolterProjectile = "c83759d106dbcb44593c2090aa6d5d95";
        private const string BolterMuzzleLocator = "502467bbbcc0471285a4ab6936a285d8";

        private static readonly string[] References =
        {
            ImperialStaff, AstartesBoltPistol, StandardBoltShot, StandardBoltBurst, Vindictor,
            VindictorHiddenMelee, VindictorMeleeSingle, VindictorMeleeAoe,
            BolterFx, BolterProjectile, BolterMuzzleLocator
        };

        [MenuItem("Astartes Custodes/Inspect weapon prefab APIs")]
        public static void InspectWeaponPrefabApis()
        {
            string output = Path.GetFullPath("BlueprintAnalysis/GuardianSpear/weapon-prefab-api.txt");
            using var writer = new StreamWriter(output, false);
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }
                foreach (Type type in types.Where(t => t.FullName != null &&
                    (t.FullName.IndexOf("EquipmentOffset", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     t.FullName.IndexOf("FxLocator", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     t.FullName.IndexOf("FXLocator", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     t.FullName.IndexOf("WeaponLocator", StringComparison.OrdinalIgnoreCase) >= 0)))
                {
                    writer.WriteLine("TYPE " + type.Assembly.GetName().Name + " :: " + type.FullName);
                    foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                        writer.WriteLine("  FIELD " + field.FieldType.FullName + " " + field.Name);
                    foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                        writer.WriteLine("  PROP " + property.PropertyType.FullName + " " + property.Name);
                }
            }
            writer.WriteLine("BLUEPRINT DATABASE METHODS");
            foreach (MethodInfo method in typeof(BlueprintsDatabase).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                writer.WriteLine("  " + method);
            foreach (string id in new[] { ImperialStaff, AstartesBoltPistol })
            {
                writer.WriteLine("WEAPON " + id);
                BlueprintItemWeapon weapon = BlueprintsDatabase.LoadById<BlueprintItemWeapon>(id);
                DumpObject(writer, weapon, "  ", 0, new System.Collections.Generic.HashSet<object>());
            }
            UnityEngine.Debug.Log("[AstartesCustodes] Weapon prefab API inspection written to " + output);
        }

        private static void DumpObject(StreamWriter writer, object value, string indent, int depth,
            System.Collections.Generic.HashSet<object> visited)
        {
            if (value == null || depth > 4) return;
            Type type = value.GetType();
            if (!type.IsValueType && !(value is string) && !visited.Add(value)) return;
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                object child;
                try { child = field.GetValue(value); } catch { continue; }
                writer.WriteLine(indent + field.Name + " : " + field.FieldType.FullName + " = " + (child ?? "<null>"));
                if (child != null && (field.Name.IndexOf("Visual", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    field.Name.IndexOf("WeaponModel", StringComparison.OrdinalIgnoreCase) >= 0 || child is UnityEngine.GameObject))
                    DumpObject(writer, child, indent + "  ", depth + 1, visited);
            }
        }

        private const string Root = "Assets/Modifications/AstartesCustodes";
        private static string Blueprints => Path.Combine(Root, "Blueprints");
        private static string Art => Path.Combine(Root, "Art");
        private const string GuardianSpearPrefabPath = Root + "/Art/GuardianSpear.prefab";
        private const string GuardianSpearFbxPath = Root + "/Art/GuardianSpear.fbx";

        [MenuItem("Astartes Custodes/Generate Guardian Spear art")]
        public static void GenerateArt()
        {
            Directory.CreateDirectory(Art);
            AssetDatabase.ImportAsset(GuardianSpearFbxPath, ImportAssetOptions.ForceSynchronousImport);
            ModelImporter fbxImporter = AssetImporter.GetAtPath(GuardianSpearFbxPath) as ModelImporter;
            if (fbxImporter == null) throw new InvalidDataException("GuardianSpear.fbx could not be imported by Unity.");
            fbxImporter.meshCompression = ModelImporterMeshCompression.Off;
            fbxImporter.importNormals = ModelImporterNormals.Import;
            fbxImporter.importTangents = ModelImporterTangents.Import;
            fbxImporter.materialImportMode = ModelImporterMaterialImportMode.None;
            fbxImporter.optimizeMeshPolygons = false;
            fbxImporter.optimizeMeshVertices = false;
            fbxImporter.SaveAndReimport();
            GameObject importedFbx = AssetDatabase.LoadAssetAtPath<GameObject>(GuardianSpearFbxPath);
            Material importedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Art + "/GuardianSpear_GLB.mat");
            if (importedFbx == null || importedMaterial == null)
                throw new InvalidDataException("Guardian Spear FBX or preserved GLB material assets are missing.");

            GameObject root = new GameObject("GuardianSpear_Root");
            EquipmentOffsets offsets = root.AddComponent<EquipmentOffsets>();
            offsets.raceScaleList = new System.Collections.Generic.List<EquipmentOffsets.RaceScale>
            {
                // A value just above the owner's base scale prevents Deathwatch's optional 1.5x Staff fallback
                // without materially enlarging this already superhuman-scale custom weapon.
                new EquipmentOffsets.RaceScale { race = Kingmaker.Blueprints.Race.Spacemarine, WeaponScale = 1.01f }
            };
            Type binderType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                .FirstOrDefault(t => t.FullName == "AstartesCustodes.Runtime.GuardianSpearMaterialBinder");
            if (binderType == null) throw new InvalidOperationException("GuardianSpearMaterialBinder runtime type was not found.");
            root.AddComponent(binderType);
            Type shotPoseType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                .FirstOrDefault(t => t.FullName == "AstartesCustodes.Runtime.GuardianSpearShotPoseController");
            if (shotPoseType == null) throw new InvalidOperationException("GuardianSpearShotPoseController runtime type was not found.");
            root.AddComponent(shotPoseType);
            GameObject visual = new GameObject("GuardianSpear_Visual");
            visual.transform.SetParent(root.transform, false);
            // First align the source diagonal to +Y, then turn the already-aligned weapon around its
            // vertical long axis so the blade points away from the wielder.
            visual.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            GameObject model = UnityEngine.Object.Instantiate(importedFbx);
            model.name = "GuardianSpear_FBX_Model";
            model.transform.SetParent(visual.transform, false);
            // Blender's FBX root retained an object-space offset (0.06, 0.22, 0.71). The weapon
            // attachment pivot must remain at the prefab root, so discard that exported object offset.
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterial = importedMaterial;

            GameObject muzzle = new GameObject("GuardianSpear_BolterMuzzle");
            muzzle.transform.SetParent(visual.transform, false);
            // Measured against the integrated barrel after the source model's 45-degree alignment.
            // Equivalent to root local (-0.06, 0.68, 0.08) at the idle +45 degree visual rotation.
            // Parenting it to the pose root makes projectile and muzzle FX follow the shot rotation.
            muzzle.transform.localPosition = new Vector3(0.4384062f, 0.523259f, 0.08f);
            muzzle.transform.localRotation = Quaternion.identity;
            FxLocator locator = muzzle.AddComponent<FxLocator>();
            SerializedObject locatorObject = new SerializedObject(locator);
            locatorObject.FindProperty("m_Group.guid").stringValue = BolterMuzzleLocator;
            locatorObject.ApplyModifiedPropertiesWithoutUndo();

            GameObject gripMarker = new GameObject("GripReference_PivotIsRoot");
            gripMarker.transform.SetParent(root.transform, false);
            gripMarker.transform.localPosition = Vector3.zero;

            PrefabUtility.SaveAsPrefabAsset(root, GuardianSpearPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("[AstartesCustodes] Guardian Spear GLB art generated with shader " + importedMaterial.shader.name);
        }

        [MenuItem("Astartes Custodes/Inspect Guardian Spear FBX")]
        public static void InspectFbx()
        {
            AssetDatabase.ImportAsset(GuardianSpearFbxPath, ImportAssetOptions.ForceSynchronousImport);
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(GuardianSpearFbxPath);
            if (asset == null) throw new InvalidDataException("GuardianSpear.fbx could not be loaded.");
            GameObject instance = UnityEngine.Object.Instantiate(asset);
            try
            {
                foreach (Transform item in instance.GetComponentsInChildren<Transform>(true))
                    UnityEngine.Debug.Log($"[AstartesCustodes][FBX] {item.name}: localPosition={item.localPosition}, localRotation={item.localEulerAngles}, localScale={item.localScale}");
                foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
                    UnityEngine.Debug.Log($"[AstartesCustodes][FBX] Renderer {renderer.name}: bounds center={renderer.bounds.center}, size={renderer.bounds.size}");
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }

        [MenuItem("Astartes Custodes/Inspect animation enums")]
        public static void InspectAnimationEnums()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException exception) { types = exception.Types.Where(type => type != null).ToArray(); }
                foreach (Type type in types.Where(type => type.IsEnum && type.FullName != null))
                {
                    string values = string.Join(", ", Enum.GetNames(type));
                    if ((values.IndexOf("Pistol", StringComparison.OrdinalIgnoreCase) >= 0 &&
                         values.IndexOf("Staff", StringComparison.OrdinalIgnoreCase) >= 0) ||
                        values.IndexOf("HeavyOnHip", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        values.IndexOf("CasterWeapon", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (values.IndexOf("Directional", StringComparison.OrdinalIgnoreCase) >= 0 &&
                         values.IndexOf("None", StringComparison.OrdinalIgnoreCase) >= 0))
                        UnityEngine.Debug.Log($"[AstartesCustodes][ANIMATION ENUM] {type.FullName}: {values}");
                }
            }
        }

        [MenuItem("Astartes Custodes/Inspect ability execution events")]
        public static void InspectAbilityExecutionEvents()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException exception) { types = exception.Types.Where(type => type != null).ToArray(); }
                foreach (Type type in types.Where(type => type.FullName != null &&
                    (type.FullName.IndexOf("AbilityExecution", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     type.FullName.IndexOf("AbilityStart", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     type.FullName.IndexOf("AbilityEnd", StringComparison.OrdinalIgnoreCase) >= 0)))
                {
                    UnityEngine.Debug.Log($"[AstartesCustodes][ABILITY EVENT TYPE] {type.Assembly.GetName().Name} :: {type.FullName} interface={type.IsInterface}");
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                        UnityEngine.Debug.Log($"[AstartesCustodes][ABILITY EVENT METHOD] {type.FullName} :: {method}");
                }
            }
            foreach (MethodInfo method in typeof(Kingmaker.PubSubSystem.Core.EventBus).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.Name.IndexOf("Subscribe", StringComparison.OrdinalIgnoreCase) >= 0))
                UnityEngine.Debug.Log($"[AstartesCustodes][EVENTBUS SUBSCRIBE] {method}");

            Type handler = typeof(Kingmaker.PubSubSystem.IAbilityExecutionProcessHandler);
            foreach (Type inherited in handler.GetInterfaces())
                UnityEngine.Debug.Log($"[AstartesCustodes][ABILITY EVENT INHERITS] {handler.FullName} -> {inherited.FullName}");
            Type genericHandler = typeof(Kingmaker.PubSubSystem.IAbilityExecutionProcessHandler<>);
            foreach (Type inherited in genericHandler.GetInterfaces())
                UnityEngine.Debug.Log($"[AstartesCustodes][ABILITY EVENT INHERITS] {genericHandler.FullName} -> {inherited.FullName}");
        }

        [MenuItem("Astartes Custodes/Generate Guardian Spear prototype")]
        public static void Generate()
        {
            GenerateArt();
            Directory.CreateDirectory(Blueprints);
            Directory.CreateDirectory(Path.Combine(Root, "Localization"));

            JObject visible = PrepareClone(Load(ImperialStaff), VisibleWeapon, ImperialStaff);
            visible["Data"]["Components"] = new JArray();
            SetText(visible, "gs-name", "gs-desc", "gs-flavor");
            SetUnityReference(visible, "m_Icon", "1a969545a91471441b4a02441fae7483", 21300000L);
            UnityEngine.Object prefab = AssetDatabase.LoadMainAssetAtPath(GuardianSpearPrefabPath);
            if (prefab == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string prefabGuid, out long prefabFileId))
                throw new InvalidDataException("Guardian Spear prefab could not be resolved.");
            visible["Data"]["m_VisualParameters"]["m_WeaponModel"] = UnityReference(prefabGuid, prefabFileId);
            AddOverride(visible, "m_VisualParameters.m_WeaponModel");
            Override(visible, "WarhammerDamage", 18);
            Override(visible, "WarhammerMaxDamage", 24);
            Override(visible, "WarhammerPenetration", 15);
            Override(visible, "CanBeUsedInGame", true);
            JObject boltSlot = (JObject)visible["Data"]["AbilityContainer"]["Ability2"];
            // Route animation selection through the existing burst attack path. The custom delivery
            // remains one projectile because the hidden weapon's RateOfFire is explicitly one.
            boltSlot["Type"] = "SingleShot";
            boltSlot["Mode"] = "Default";
            boltSlot["m_Ability"] = "!bp_" + BoltShot;
            boltSlot["m_FXSettings"] = "!bp_" + BolterFx;
            boltSlot["AP"] = 1;
            foreach (string field in new[] { "Type", "Mode", "m_Ability", "m_FXSettings", "AP" })
                AddOverride(visible, "WeaponAbilities.Ability2." + field);
            ClearAbilitySlot(visible, "Ability3");
            ClearAbilitySlot(visible, "Ability4");
            ClearAbilitySlot(visible, "Ability5");
            Save("GuardianSpear_Prototype_Item", visible);

            JObject hidden = PrepareClone(Load(AstartesBoltPistol), HiddenBolter, AstartesBoltPistol);
            hidden["Data"]["Components"] = new JArray();
            hidden["Data"]["m_VisualParameters"]["m_WeaponModel"] = null;
            AddOverride(hidden, "m_VisualParameters.m_WeaponModel");
            // The hidden profile supplies the Bolt Shot damage and its ability-specific animation
            // style. The visible weapon remains Staff, preserving the proven melee swing.
            hidden["Data"]["m_VisualParameters"]["m_WeaponAnimationStyle"] = "Rifle";
            AddOverride(hidden, "m_VisualParameters.m_WeaponAnimationStyle");
            Override(hidden, "m_HoldingType", "TwoHanded");
            Override(hidden, "IsTwoHanded", true);
            Override(hidden, "CanBeUsedInGame", false);
            Override(hidden, "IsUnlootable", true);
            Override(hidden, "WarhammerMaxAmmo", -1);
            Override(hidden, "WarhammerDamage", 17);
            Override(hidden, "WarhammerMaxDamage", 22);
            Override(hidden, "WarhammerPenetration", 20);
            SetText(hidden, "gs-hidden-name", "gs-hidden-desc", "gs-hidden-flavor");
            Save("GuardianSpear_HiddenBolter_Item", hidden);

            JObject shot = PrepareClone(Load(StandardBoltShot), BoltShot, StandardBoltShot);
            JObject meleeReference = Load(VindictorMeleeSingle);
            JObject overrideWeapon = (JObject)meleeReference["Data"]["Components"]
                .Children<JObject>().First(c => c["$type"]?.ToString().Contains("WarhammerOverrideAbilityWeapon") == true).DeepClone();
            overrideWeapon["name"] = "$WarhammerOverrideAbilityWeapon$guardian-spear-hidden-bolter";
            overrideWeapon["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            overrideWeapon["m_Weapon"] = "!bp_" + HiddenBolter;
            overrideWeapon["m_ForceShowWeaponDamageInUi"] = true;
            ((JArray)shot["Data"]["Components"]).Add(overrideWeapon);
            AddOverride(shot, overrideWeapon["name"].ToString());
            JObject ammo = (JObject)meleeReference["Data"]["Components"]
                .Children<JObject>().First(c => c["$type"]?.ToString().Contains("AbilityAmmoLogic") == true).DeepClone();
            ammo["name"] = "$AbilityAmmoLogic$guardian-spear-no-ammo";
            ammo["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            ammo["NoAmmoRequired"] = true;
            ((JArray)shot["Data"]["Components"]).Add(ammo);
            AddOverride(shot, ammo["name"].ToString());
            shot["Data"]["m_FXSettings"] = "!bp_" + BolterFx;
            AddOverride(shot, "m_FXSettings");
            shot["Data"]["m_DisplayName"] = Localized("gs-shot-name");
            shot["Data"]["m_Description"] = Localized("gs-shot-desc");
            AddOverride(shot, "m_DisplayName");
            AddOverride(shot, "m_Description");
            Save("GuardianSpear_BoltShot_Ability", shot);

            WriteLocalization();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("[AstartesCustodes] Guardian Spear prototype generated.");
        }

        [MenuItem("Astartes Custodes/Build Guardian Spear prototype")]
        public static void Build()
        {
            Generate();
            var mod = AssetDatabase.LoadAssetAtPath<Modification>(Root + "/AstartesCustodes.asset");
            if (mod == null) throw new InvalidOperationException("AstartesCustodes Modification asset was not found.");
            var result = Builder.Build(mod);
            if ((int)result != 0) throw new InvalidOperationException("Build failed: " + result);
        }

        [MenuItem("Astartes Custodes/Export Guardian Spear references")]
        public static void ExportReferences()
        {
            string output = Path.GetFullPath("BlueprintAnalysis/GuardianSpear");
            Directory.CreateDirectory(output);
            foreach (string id in References)
            {
                JObject root = Load(id);
                string name = root["Data"]?["name"]?.ToString();
                if (string.IsNullOrWhiteSpace(name)) name = id;
                foreach (char invalid in Path.GetInvalidFileNameChars()) name = name.Replace(invalid, '_');
                File.WriteAllText(Path.Combine(output, name + "_" + id + ".json"), root.ToString(Formatting.Indented));
            }
            UnityEngine.Debug.Log("[AstartesCustodes] Guardian Spear references exported to " + output);
        }

        private static JObject Load(string id)
        {
            BlueprintJsonWrapper wrapper = BlueprintsDatabase.LoadWrapperById(id);
            if (wrapper == null) throw new InvalidDataException("Blueprint not found: " + id);
            using var writer = new StringWriter();
            Json.Serializer.Serialize(writer, wrapper);
            return JObject.Parse(writer.ToString());
        }

        private static Material CreateMaterial(string name, Color color, float metallic, float smoothness)
        {
            string path = Art + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            // The stock mod build strips shader programs. The prefab's runtime binder reconnects these material
            // assets to the already-loaded game shader after instantiation.
            Shader shader = Shader.Find("Owlcat/Lit");
            if (shader == null) throw new InvalidOperationException("No supported lit shader was found.");
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else material.shader = shader;
            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
            return material;
        }

        private static void AddMeshPart(Transform parent, string name, Mesh mesh, Material material)
        {
            string path = Art + "/" + name + ".asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null) AssetDatabase.CreateAsset(mesh, path);
            else
            {
                EditorUtility.CopySerialized(mesh, existing);
                UnityEngine.Object.DestroyImmediate(mesh);
                mesh = existing;
            }
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.AddComponent<MeshFilter>().sharedMesh = mesh;
            child.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private struct Part
        {
            internal PrimitiveType Type;
            internal Vector3 Position;
            internal Vector3 Scale;
            internal Quaternion Rotation;
            internal Part(PrimitiveType type, Vector3 position, Vector3 scale, Quaternion rotation)
            { Type = type; Position = position; Scale = scale; Rotation = rotation; }
        }

        private static Mesh Combine(params Part[] parts)
        {
            var combines = new CombineInstance[parts.Length];
            var temporary = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < parts.Length; i++)
            {
                GameObject primitive = GameObject.CreatePrimitive(parts[i].Type);
                temporary.Add(primitive);
                combines[i] = new CombineInstance
                {
                    mesh = primitive.GetComponent<MeshFilter>().sharedMesh,
                    transform = Matrix4x4.TRS(parts[i].Position, parts[i].Rotation, parts[i].Scale)
                };
            }
            Mesh mesh = new Mesh { name = "GuardianSpearCombinedMesh" };
            mesh.CombineMeshes(combines, true, true, false);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            foreach (GameObject item in temporary) UnityEngine.Object.DestroyImmediate(item);
            return mesh;
        }

        private static Mesh CreateAuricMesh() => Combine(
            new Part(PrimitiveType.Cylinder, new Vector3(0, 0.03f, 0), new Vector3(0.038f, 1.40f, 0.038f), Quaternion.identity),
            new Part(PrimitiveType.Cylinder, new Vector3(0, -1.38f, 0), new Vector3(0.075f, 0.045f, 0.075f), Quaternion.identity),
            new Part(PrimitiveType.Cylinder, new Vector3(0, -0.57f, 0), new Vector3(0.062f, 0.045f, 0.062f), Quaternion.identity),
            new Part(PrimitiveType.Cylinder, new Vector3(0, 0.36f, 0), new Vector3(0.062f, 0.045f, 0.062f), Quaternion.identity),
            new Part(PrimitiveType.Cube, new Vector3(0, 1.04f, 0), new Vector3(0.34f, 0.58f, 0.17f), Quaternion.Euler(0, 0, -5)),
            new Part(PrimitiveType.Cube, new Vector3(0.02f, 1.36f, 0), new Vector3(0.23f, 0.12f, 0.14f), Quaternion.identity),
            new Part(PrimitiveType.Cube, new Vector3(0, 0.68f, 0), new Vector3(0.23f, 0.10f, 0.14f), Quaternion.identity),
            new Part(PrimitiveType.Cube, new Vector3(0, -0.73f, 0), new Vector3(0.13f, 0.35f, 0.10f), Quaternion.identity));

        private static Mesh CreateGunmetalMesh() => Combine(
            new Part(PrimitiveType.Cube, new Vector3(-0.02f, 1.05f, -0.01f), new Vector3(0.25f, 0.43f, 0.20f), Quaternion.Euler(0, 0, -5)),
            new Part(PrimitiveType.Cylinder, new Vector3(-0.13f, 1.37f, -0.105f), new Vector3(0.035f, 0.13f, 0.035f), Quaternion.identity),
            new Part(PrimitiveType.Cylinder, new Vector3(-0.13f, 1.45f, -0.105f), new Vector3(0.052f, 0.035f, 0.052f), Quaternion.identity),
            new Part(PrimitiveType.Cube, new Vector3(0.17f, 0.91f, -0.01f), new Vector3(0.12f, 0.23f, 0.16f), Quaternion.Euler(0, 0, -12)),
            new Part(PrimitiveType.Cylinder, new Vector3(-0.13f, 0.73f, 0), new Vector3(0.055f, 0.045f, 0.055f), Quaternion.Euler(90, 0, 0)),
            new Part(PrimitiveType.Cube, new Vector3(-0.20f, 1.25f, 0), new Vector3(0.08f, 0.11f, 0.13f), Quaternion.Euler(0, 0, 35)));

        private static Mesh CreateGripMesh() => Combine(
            new Part(PrimitiveType.Cylinder, new Vector3(0, -0.12f, 0), new Vector3(0.050f, 0.38f, 0.050f), Quaternion.identity),
            new Part(PrimitiveType.Cylinder, new Vector3(0, -0.95f, 0), new Vector3(0.050f, 0.33f, 0.050f), Quaternion.identity),
            new Part(PrimitiveType.Cylinder, new Vector3(0, 0.46f, 0), new Vector3(0.050f, 0.10f, 0.050f), Quaternion.identity));

        private static Mesh CreateRedMesh() => Combine(
            new Part(PrimitiveType.Cube, new Vector3(-0.02f, 1.00f, -0.115f), new Vector3(0.15f, 0.22f, 0.018f), Quaternion.Euler(0, 0, -5)),
            new Part(PrimitiveType.Cube, new Vector3(0, -0.72f, -0.060f), new Vector3(0.055f, 0.20f, 0.018f), Quaternion.identity));

        private static Mesh CreateBladeMesh()
        {
            Vector2[] mainBlade =
            {
                new Vector2(-0.07f, 1.33f), new Vector2(0.04f, 1.39f), new Vector2(0.18f, 1.55f),
                new Vector2(0.30f, 1.79f), new Vector2(0.34f, 2.04f), new Vector2(0.30f, 2.25f),
                new Vector2(0.21f, 2.42f), new Vector2(0.11f, 2.52f), new Vector2(0.03f, 2.38f),
                new Vector2(-0.04f, 2.10f), new Vector2(-0.08f, 1.72f)
            };
            Vector2[] sideSpike =
            {
                new Vector2(-0.10f, 1.28f), new Vector2(-0.25f, 1.38f), new Vector2(-0.48f, 1.62f),
                new Vector2(-0.19f, 1.49f), new Vector2(-0.07f, 1.42f)
            };
            return CombineRaw(ExtrudePolygon(mainBlade, 0.075f), ExtrudePolygon(sideSpike, 0.060f));
        }

        private static Mesh CombineRaw(params Mesh[] meshes)
        {
            CombineInstance[] combines = meshes.Select(m => new CombineInstance { mesh = m, transform = Matrix4x4.identity }).ToArray();
            Mesh result = new Mesh { name = "GuardianSpearBladeMesh" };
            result.CombineMeshes(combines, true, true, false);
            result.RecalculateNormals();
            result.RecalculateTangents();
            result.RecalculateBounds();
            foreach (Mesh mesh in meshes) UnityEngine.Object.DestroyImmediate(mesh);
            return result;
        }

        private static Mesh ExtrudePolygon(Vector2[] polygon, float thickness)
        {
            int count = polygon.Length;
            Vector3[] vertices = new Vector3[count * 2];
            float half = thickness * 0.5f;
            for (int i = 0; i < count; i++)
            {
                vertices[i] = new Vector3(polygon[i].x, polygon[i].y, -half);
                vertices[i + count] = new Vector3(polygon[i].x, polygon[i].y, half);
            }
            var triangles = new System.Collections.Generic.List<int>();
            for (int i = 1; i < count - 1; i++)
            {
                triangles.Add(0); triangles.Add(i + 1); triangles.Add(i);
                triangles.Add(count); triangles.Add(count + i); triangles.Add(count + i + 1);
            }
            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                triangles.Add(i); triangles.Add(next); triangles.Add(count + next);
                triangles.Add(i); triangles.Add(count + next); triangles.Add(count + i);
            }
            Mesh mesh = new Mesh { name = "ExtrudedGuardianSpearPolygon", vertices = vertices, triangles = triangles.ToArray() };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static JObject PrepareClone(JObject root, string id, string prototype)
        {
            root["AssetId"] = id;
            root["Data"]["PrototypeLink"] = prototype;
            root["Data"]["m_Overrides"] = new JArray();
            foreach (JObject component in root["Data"]["Components"].Children<JObject>())
            {
                component["PrototypeLink"] = new JObject
                {
                    ["guid"] = prototype,
                    ["name"] = component["name"]?.ToString() ?? ""
                };
                component["m_Overrides"] = new JArray();
            }
            return root;
        }

        private static void ClearAbilitySlot(JObject weapon, string slotName)
        {
            JObject slot = (JObject)weapon["Data"]["AbilityContainer"][slotName];
            slot["Type"] = "None";
            slot["Mode"] = "Default";
            slot["m_Ability"] = null;
            slot["m_FXSettings"] = null;
            slot["AP"] = 0;
            foreach (string field in new[] { "Type", "Mode", "m_Ability", "m_FXSettings", "AP" })
                AddOverride(weapon, "WeaponAbilities." + slotName + "." + field);
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

        private static JObject UnityReference(string guid, long fileId) =>
            new JObject { ["guid"] = guid, ["fileid"] = fileId };

        private static void SetUnityReference(JObject root, string property, string guid, long fileId)
        {
            root["Data"][property] = UnityReference(guid, fileId);
            AddOverride(root, property);
        }

        private static void Override(JObject root, string property, JToken value)
        {
            root["Data"][property] = value;
            AddOverride(root, property);
        }

        private static void AddOverride(JObject root, string property)
        {
            JArray overrides = (JArray)root["Data"]["m_Overrides"];
            if (!overrides.Values<string>().Contains(property)) overrides.Add(property);
        }

        private static void Save(string name, JObject root)
        {
            root["Data"]["Author"] = "Poffl";
            AddOverride(root, "Author");
            File.WriteAllText(Path.Combine(Blueprints, name + ".jbp"), root.ToString(Formatting.Indented));
        }

        private static JObject Entry(string text) => new JObject { ["Offset"] = 0, ["Text"] = text };

        private static void WriteLocalization()
        {
            JObject strings = new JObject
            {
                ["gs-name"] = Entry("Guardian Spear Prototype"),
                ["gs-desc"] = Entry("A proof-of-concept polearm with a separate integrated bolt weapon profile. Provides a melee strike and Bolt Shot."),
                ["gs-flavor"] = Entry("Prototype wargear for hybrid-weapon testing."),
                ["gs-hidden-name"] = Entry("Guardian Spear Hidden Bolter"),
                ["gs-hidden-desc"] = Entry("Internal ranged profile used by Guardian Spear Bolt Shot."),
                ["gs-hidden-flavor"] = Entry("Not intended for inventory or loot."),
                ["gs-shot-name"] = Entry("Bolt Shot"),
                ["gs-shot-desc"] = Entry("Fire the Guardian Spear's integrated bolter using its hidden ranged weapon profile.")
            };
            File.WriteAllText(Path.Combine(Root, "Localization", "enGB.json"),
                new JObject { ["strings"] = strings }.ToString(Formatting.Indented));
        }
    }
}
