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
        internal const string GuardianCleave = "b3d51e80873c49f49422eab82db9f720";
        internal const string BoltBurst = "c4e65f9139c74d54bc08a87caf2bb381";
        internal const string HiddenGreatsword = "d5f76a024ca14d6096e30c499cef65bd";
        internal const string GuardianStrike = "e48c72cc16194cc986f3041cd3bc01aa";

        internal static readonly string[] VisibleWeapons =
        {
            VisibleWeapon,
            "57fe8f5911961a2f7520902d00a08ade", "daa6cb6d22409135822d23800e71f1ea",
            "db7158b0a3ab06fef59f60cb255d3b20", "6e3e198f6446b1a9de3fea683517ff69",
            "bdb166b828685110dff677d0791a0bda"
        };

        private static readonly string[] ModifierFeatures =
        {
            "519fe85173c3595b07859de2cc158124", "07aa1e1bf5ed1a1400b34bcc0d9f47b3",
            "d238ae6890c99240222096be32f7f31d", "e678600653fa97bbfa0b727460ebc52b",
            "881cf5d2d7e98b244f23058eb3702b50", "20a770c998480071f610d85512f34f17"
        };

        private static readonly string[] ShotWeapons =
        {
            HiddenBolter, "0f1b7d2083022685087e217c044f7e3d", "3d237ad1c3263c675fc9fef021246b3d",
            "117f98476e6f9f6b46279a74081c8a29", "0d1d51e21faee51aeed6928f5cd86a3b", "fc2f348879869b5ba90c0770c09b1eff"
        };

        private static readonly string[] BurstWeapons =
        {
            "544db66481a2b732d51b78e729260497", "7909dd4d12cea19143aa387b3c33ffdb",
            "9c8c4d5b1dcb0615e31af9950b33246d", "5938060c531f777c7c486e0c3060f75b",
            "f33889f768f0f3529651c6e2634a94f2", "27ddcc5b2797a73d692c6c0a7520eb1c"
        };

        private static readonly string[] CleaveWeapons =
        {
            HiddenGreatsword, "30728bf7306aa8120d0fd12705dac801", "0ce27ad62fabf27d98ea8e9abb561ebf",
            "b5c1a56d7b092cef49a12ef54609e432", "849473385df21a63a38080412cf67cd8", "7f196f35e9547126748fc8cd8b4705f6"
        };

        private static readonly string[] ShotAbilities =
        {
            BoltShot, "5986262441d6940a7ba74e7199b17222", "728e8542d63b078a690053d7bf9805f7",
            "a6c669d9e59ac497dbdd5d87f4f1c5c1", "f8f4c3ee58372fb4ac8c365577b43af9", "e2d02670d099362458097355bf04adb4"
        };

        private static readonly string[] BurstAbilities =
        {
            BoltBurst, "c7c3a20b3589b3c6fe4829a670e1a917", "e3b3153d60971a366702b2cd235743b1",
            "bd48e166afa8ec7d8747d67ce9f4ba90", "c4636e1c3018bbaf41f8b82d32b37593", "b277a41d4a5beb42441122718a5816bc"
        };

        private static readonly string[] CleaveAbilities =
        {
            GuardianCleave, "176b12672fe98baedf3fcfc2421e651d", "e7a59c6b5ac8130e6dfefd3eaddcde2f",
            "dacd5e8fb259493f316cedf8542e44af", "8b84cfa49fc46904e048762dd69febb5", "95b48767dabd6a13281c7dccd72db4df"
        };

        private static readonly int[] MeleeMin = { 18, 22, 27, 32, 37, 42 };
        private static readonly int[] MeleeMax = { 24, 30, 36, 43, 50, 57 };
        private static readonly int[] MeleePen = { 15, 20, 25, 30, 35, 40 };
        private static readonly int[] CleaveMin = { 14, 18, 22, 27, 31, 35 };
        private static readonly int[] CleaveMax = { 20, 25, 30, 36, 42, 48 };
        private static readonly int[] CleavePen = { 10, 15, 20, 25, 30, 35 };
        private static readonly int[] ShotMin = { 16, 20, 24, 29, 34, 39 };
        private static readonly int[] ShotMax = { 22, 27, 32, 39, 46, 52 };
        private static readonly int[] ShotPen = { 15, 20, 25, 30, 35, 40 };
        private static readonly int[] BurstMin = { 10, 12, 14, 16, 18, 20 };
        private static readonly int[] BurstMax = { 14, 17, 19, 22, 25, 28 };
        private static readonly int[] BurstPen = { 10, 12, 15, 18, 20, 25 };
        private static readonly int[] BurstShots = { 3, 4, 5, 6, 8, 9 };
        private static readonly int[] Ammo = { 9, 12, 15, 18, 24, 27 };
        private static readonly int[] SkillBonus = { 2, 4, 6, 8, 10, 12 };
        private static readonly int[] ParryBonus = { 2, 4, 6, 8, 10, 10 };
        private static readonly int[] CriticalChance = { 2, 4, 6, 9, 12, 15 };
        private static readonly int[] CriticalDamage = { 5, 10, 15, 20, 25, 30 };

        private const string ImperialStaff = "993996a4c0a24463aa400b9441d4caa8";
        private const string AstartesBoltPistol = "5e1bae4c2c7e4bd99411173f8dbe74f0";
        private const string StandardBoltShot = "6a7f0c4523c34de7829c088556b62f11";
        private const string StandardBoltBurst = "347d38e3abad490dad41ee7b77092b24";
        private const string StandardReload = "98f4a31b68e446ad9c63411c7b349146";
        private const string TwoHandedSwordCleave = "163013a18e9c46419b2311454ad2b2c8";
        private const string StaffStrike = "638cd0973175462b9faaeb1242761d32";
        private const string TwoHandedSwordStrike = "9dec1bdade284190b0977f5f70d26d3e";
        private const string SwordStrikeIconGuid = "a6cba97367839af4e8869281de029095";
        private const string GreatSword = "88863b6b0c61404b96b01c2bc648ba5e";
        private const string Vindictor = "0a5e8b407f9940589d44675f42783581";
        private const string VindictorHiddenMelee = "91ab9da13b8848aab46bd885a0199db3";
        private const string VindictorMeleeSingle = "84c32baad3f14585a32f5747d721dfc3";
        private const string VindictorMeleeAoe = "9098215cb3aa482d9c44b9c03a17b8cb";
        private const string EvisceratorCh5 = "4d87435ddfa042269c1fe35df0430f8b";
        private const string WsModifierReference = "08e144a9788040ea81a99421b5576bc3";
        private const string BsModifierReference = "57c442a8026d4216b28a0501cb139d38";
        private const string ParryModifierReference = "53c19a9468d24539863989b3be9ed1f5";
        private const string CriticalChanceReference = "1a1ac3e3f133432a8c5c0d19cce16035";
        private const string CriticalDamageReference = "f70e4a5d21ba4bc9a7fce7e3e84bb59f";
        private const string BolterFx = "afde0e8c0c9848deba8e38a1279ee7df";
        private const string BolterProjectile = "c83759d106dbcb44593c2090aa6d5d95";
        private const string BolterMuzzleLocator = "502467bbbcc0471285a4ab6936a285d8";

        private static readonly string[] References =
        {
            ImperialStaff, AstartesBoltPistol, StandardBoltShot, StandardBoltBurst, StaffStrike, TwoHandedSwordStrike,
            TwoHandedSwordCleave, GreatSword, Vindictor,
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
        private const string GuardianSpearIconPath = Root + "/Art/GuardianSpear_InventoryIcon.png";
        private const string SingleShotIconGuid = "be8ca8564aadea5439b33b0a41a99ef9";
        private const string BurstFireIconGuid = "e30bba760a5331942a4e0829c38e4e4f";
        private const string TwoHandedCleaveIconGuid = "16fd1bbd2c6ab964db8067cbd185a32c";

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
            Type locatorMapperType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                .FirstOrDefault(t => t.FullName == "FxLocatorMapper");
            if (locatorMapperType == null) throw new InvalidOperationException("FxLocatorMapper type was not found.");
            root.AddComponent(locatorMapperType);
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

        [MenuItem("Astartes Custodes/Inspect ammo APIs")]
        public static void InspectAmmoApis()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException exception) { types = exception.Types.Where(type => type != null).ToArray(); }
                foreach (Type type in types.Where(type => type.FullName != null &&
                    (type.FullName.Contains("AbilityAmmoLogic") || type.FullName.Contains("ItemEntityWeapon") ||
                     type.FullName.Contains("WeaponAmmo") || type.FullName == "Kingmaker.UnitLogic.Abilities.AbilityExecutionContext" ||
                     type.FullName == "Kingmaker.UnitLogic.Abilities.AbilityData" ||
                     type.FullName == "Kingmaker.UnitLogic.Abilities.Ability")))
                {
                    UnityEngine.Debug.Log($"[AstartesCustodes][AMMO TYPE] {assembly.GetName().Name} :: {type.FullName}");
                    foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                        UnityEngine.Debug.Log($"[AstartesCustodes][AMMO FIELD] {type.FullName} :: {field.FieldType.FullName} {field.Name}");
                    foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                        UnityEngine.Debug.Log($"[AstartesCustodes][AMMO PROP] {type.FullName} :: {property.PropertyType.FullName} {property.Name} set={property.CanWrite}");
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                        UnityEngine.Debug.Log($"[AstartesCustodes][AMMO METHOD] {type.FullName} :: {method}");
                }
            }
        }

        [MenuItem("Astartes Custodes/Inspect progression APIs")]
        public static void InspectProgressionApis()
        {
            string output = Path.GetFullPath("BlueprintAnalysis/GuardianSpear/progression-api.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            using var writer = new StreamWriter(output, false);
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException exception) { types = exception.Types.Where(type => type != null).ToArray(); }
                foreach (Type type in types.Where(type => type.FullName != null &&
                    (type.FullName.IndexOf("LevelUp", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     type.FullName.IndexOf("LevelChanged", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     type.FullName.IndexOf("ItemsCollection", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     type.FullName.IndexOf("Inventory", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     type.FullName.IndexOf("CombatState", StringComparison.OrdinalIgnoreCase) >= 0)))
                {
                    writer.WriteLine("TYPE " + type.Assembly.GetName().Name + " :: " + type.FullName + " interface=" + type.IsInterface);
                    foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                        writer.WriteLine("  FIELD " + field.FieldType.FullName + " " + field.Name);
                    foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                        writer.WriteLine("  PROP " + property.PropertyType.FullName + " " + property.Name + " set=" + property.CanWrite);
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                        writer.WriteLine("  METHOD " + method);
                }
            }
            UnityEngine.Debug.Log("[AstartesCustodes] Progression API inspection written to " + output);
        }

        [MenuItem("Astartes Custodes/Inspect progression targets")]
        public static void InspectProgressionTargets()
        {
            string output = Path.GetFullPath("BlueprintAnalysis/GuardianSpear/progression-targets.txt");
            using var writer = new StreamWriter(output, false);
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException exception) { types = exception.Types.Where(type => type != null).ToArray(); }
                foreach (Type type in types.Where(type => type.FullName != null &&
                    (type.FullName == "Kingmaker.Game" || type.FullName == "Kingmaker.Player" ||
                     type.FullName == "Kingmaker.EntitySystem.Entities.BaseUnitEntity" ||
                     type.FullName == "Kingmaker.Items.ItemEntity" || type.FullName == "Kingmaker.Items.ItemEntityWeapon" ||
                     type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         .Any(method => method.Name == "HandlePartyCombatStateChanged"))))
                {
                    writer.WriteLine("TYPE " + type.Assembly.GetName().Name + " :: " + type.FullName + " interface=" + type.IsInterface);
                    writer.WriteLine("  INTERFACES " + string.Join(", ", type.GetInterfaces().Select(item => item.FullName)));
                    foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                        if (field.Name.IndexOf("Inventory", StringComparison.OrdinalIgnoreCase) >= 0 || field.Name.IndexOf("Stash", StringComparison.OrdinalIgnoreCase) >= 0 || field.Name.IndexOf("Combat", StringComparison.OrdinalIgnoreCase) >= 0 || field.Name.IndexOf("Level", StringComparison.OrdinalIgnoreCase) >= 0)
                            writer.WriteLine("  FIELD " + field.FieldType.FullName + " " + field.Name);
                    foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                        if (property.Name.IndexOf("Inventory", StringComparison.OrdinalIgnoreCase) >= 0 || property.Name.IndexOf("Stash", StringComparison.OrdinalIgnoreCase) >= 0 || property.Name.IndexOf("Combat", StringComparison.OrdinalIgnoreCase) >= 0 || property.Name.IndexOf("Level", StringComparison.OrdinalIgnoreCase) >= 0 || property.Name == "Player")
                            writer.WriteLine("  PROP " + property.PropertyType.FullName + " " + property.Name + " set=" + property.CanWrite);
                    foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                        if (method.Name.IndexOf("Inventory", StringComparison.OrdinalIgnoreCase) >= 0 || method.Name.IndexOf("Stash", StringComparison.OrdinalIgnoreCase) >= 0 || method.Name.IndexOf("Combat", StringComparison.OrdinalIgnoreCase) >= 0 || method.Name.IndexOf("Level", StringComparison.OrdinalIgnoreCase) >= 0 || method.Name == "HandlePartyCombatStateChanged")
                            writer.WriteLine("  METHOD " + method);
                }
            }
            UnityEngine.Debug.Log("[AstartesCustodes] Progression target inspection written to " + output);
        }

        [MenuItem("Astartes Custodes/Inspect item slot APIs")]
        public static void InspectItemSlotApis()
        {
            string output = Path.GetFullPath("BlueprintAnalysis/GuardianSpear/item-slot-api.txt");
            using var writer = new StreamWriter(output, false);
            string[] names = { "Kingmaker.Items.ItemEntity", "Kingmaker.Items.ItemEntityWeapon", "Kingmaker.Items.Slots.ItemSlot", "Kingmaker.Items.Slots.HandSlot", "Kingmaker.Items.PartUnitBody" };
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (Type type in names.Select(name => assembly.GetType(name, false)).Where(type => type != null))
            {
                writer.WriteLine("TYPE " + type.Assembly.GetName().Name + " :: " + type.FullName);
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    writer.WriteLine("  FIELD " + field.FieldType.FullName + " " + field.Name);
                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    writer.WriteLine("  PROP " + property.PropertyType.FullName + " " + property.Name + " set=" + property.CanWrite);
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    writer.WriteLine("  METHOD " + method);
            }
            UnityEngine.Debug.Log("[AstartesCustodes] Item slot API inspection written to " + output);
        }

        [MenuItem("Astartes Custodes/Generate Guardian Spear prototype")]
        public static void Generate()
        {
            GenerateArt();
            Directory.CreateDirectory(Blueprints);
            Directory.CreateDirectory(Path.Combine(Root, "Localization"));
            UnityEngine.Object icon = PrepareInventoryIcon();
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(icon, out string iconGuid, out long iconFileId))
                throw new InvalidDataException("Guardian Spear inventory icon could not be resolved.");
            UnityEngine.Object prefab = AssetDatabase.LoadMainAssetAtPath(GuardianSpearPrefabPath);
            if (prefab == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out string prefabGuid, out long prefabFileId))
                throw new InvalidDataException("Guardian Spear prefab could not be resolved.");
            JObject meleeReference = Load(VindictorMeleeSingle);
            JObject overrideWeaponTemplate = (JObject)meleeReference["Data"]["Components"]
                .Children<JObject>().First(c => c["$type"]?.ToString().Contains("WarhammerOverrideAbilityWeapon") == true).DeepClone();
            JObject ammoTemplate = (JObject)meleeReference["Data"]["Components"]
                .Children<JObject>().First(c => c["$type"]?.ToString().Contains("AbilityAmmoLogic") == true).DeepClone();

            JObject addFactTemplate = (JObject)Load(EvisceratorCh5)["Data"]["Components"]
                .Children<JObject>().First(c => c["$type"]?.ToString().Contains("AddFactToEquipmentWielder") == true).DeepClone();

            CreateGuardianStrikeAbility();

            for (int i = 0; i < 6; i++)
            {
                int tier = i + 1;
                CreateModifierFeature(i);
                CreateHiddenRangedWeapon(i, false);
                CreateHiddenRangedWeapon(i, true);
                CreateHiddenCleaveWeapon(i);
                CreateAttackAbility(i, false, overrideWeaponTemplate, ammoTemplate);
                CreateAttackAbility(i, true, overrideWeaponTemplate, ammoTemplate);
                CreateCleaveAbility(i, overrideWeaponTemplate);

                JObject visible = PrepareClone(Load(ImperialStaff), VisibleWeapons[i], ImperialStaff);
                visible["Data"]["Components"] = new JArray();
                SetText(visible, $"gs-v{tier}-name", $"gs-v{tier}-desc", "gs-flavor");
                visible["Data"]["m_TypeNameText"] = Localized("gs-type-name");
                AddOverride(visible, "m_TypeNameText");
                SetUnityReference(visible, "m_Icon", iconGuid, iconFileId);
                visible["Data"]["m_VisualParameters"]["m_WeaponModel"] = UnityReference(prefabGuid, prefabFileId);
                AddOverride(visible, "m_VisualParameters.m_WeaponModel");
                Override(visible, "WarhammerDamage", MeleeMin[i]);
                Override(visible, "WarhammerMaxDamage", MeleeMax[i]);
                Override(visible, "WarhammerPenetration", MeleePen[i]);
                Override(visible, "WarhammerMaxAmmo", Ammo[i]);
                Override(visible, "ItemLevel", tier == 6 ? 55 : i * 10 + 9);
                Override(visible, "m_Rarity", tier <= 2 ? "Pattern" : "Unique");
                Override(visible, "Family", "Power");
                Override(visible, "Classification", "Sword");
                Override(visible, "m_IsNotable", true);
                Override(visible, "IsNonRemovable", false);
                Override(visible, "CanBeUsedInGame", true);
                JObject addFact = (JObject)addFactTemplate.DeepClone();
                addFact["name"] = $"$AddFactToEquipmentWielder$guardian-spear-v{tier}";
                addFact["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
                addFact["m_Fact"] = "!bp_" + ModifierFeatures[i];
                ((JArray)visible["Data"]["Components"]).Add(addFact);
                AddOverride(visible, addFact["name"].ToString());
                SetAbilitySlot(visible, "Ability1", "SingleShot", GuardianStrike, "046cf83ca27244998b0603750d4a833e", 1);
                SetAbilitySlot(visible, "Ability2", "SingleShot", ShotAbilities[i], BolterFx, 1);
                SetAbilitySlot(visible, "Ability3", "AOE", CleaveAbilities[i], "046cf83ca27244998b0603750d4a833e", 2);
                SetAbilitySlot(visible, "Ability4", "Burst", BurstAbilities[i], BolterFx, 2);
                SetAbilitySlot(visible, "Ability5", "Reload", StandardReload, BolterFx, 2, "Any");
                visible["Data"]["m_AttackOfOpportunityAbility"] = "!bp_" + GuardianStrike;
                AddOverride(visible, "m_AttackOfOpportunityAbility");
                Save(tier == 1 ? "GuardianSpear_Prototype_Item" : $"GuardianSpear_V{tier}_Item", visible);
            }

            WriteLocalization();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("[AstartesCustodes] Guardian Spear V1-V6 progression generated.");
        }

        private static void CreateGuardianStrikeAbility()
        {
            JObject ability = PrepareClone(Load(TwoHandedSwordStrike), GuardianStrike, TwoHandedSwordStrike);
            SetUnityReference(ability, "m_Icon", SwordStrikeIconGuid, 21300000);
            Save("GuardianSpear_Strike_Ability", ability);
        }

        private static UnityEngine.Object PrepareInventoryIcon()
        {
            AssetDatabase.ImportAsset(GuardianSpearIconPath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(GuardianSpearIconPath) as TextureImporter;
            if (importer == null) throw new InvalidDataException("Guardian Spear inventory icon importer was not found.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(GuardianSpearIconPath).FirstOrDefault(asset => asset is Sprite)
                ?? AssetDatabase.LoadMainAssetAtPath(GuardianSpearIconPath);
        }

        private static void CreateHiddenRangedWeapon(int i, bool burst)
        {
            int tier = i + 1;
            string id = burst ? BurstWeapons[i] : ShotWeapons[i];
            JObject weapon = PrepareClone(Load(AstartesBoltPistol), id, AstartesBoltPistol);
            weapon["Data"]["Components"] = new JArray();
            weapon["Data"]["m_VisualParameters"]["m_WeaponModel"] = null;
            weapon["Data"]["m_VisualParameters"]["m_WeaponAnimationStyle"] = "HeavyOnHip";
            AddOverride(weapon, "m_VisualParameters.m_WeaponModel");
            AddOverride(weapon, "m_VisualParameters.m_WeaponAnimationStyle");
            Override(weapon, "m_HoldingType", "TwoHanded");
            Override(weapon, "IsTwoHanded", true);
            Override(weapon, "CanBeUsedInGame", false);
            Override(weapon, "IsUnlootable", true);
            Override(weapon, "WarhammerMaxAmmo", Ammo[i]);
            Override(weapon, "WarhammerDamage", burst ? BurstMin[i] : ShotMin[i]);
            Override(weapon, "WarhammerMaxDamage", burst ? BurstMax[i] : ShotMax[i]);
            Override(weapon, "WarhammerPenetration", burst ? BurstPen[i] : ShotPen[i]);
            Override(weapon, "RateOfFire", burst ? BurstShots[i] : 1);
            SetText(weapon, "gs-hidden-name", "gs-hidden-desc", "gs-hidden-flavor");
            Save(burst ? $"GuardianSpear_V{tier}_HiddenBurst_Item" :
                (tier == 1 ? "GuardianSpear_HiddenBolter_Item" : $"GuardianSpear_V{tier}_HiddenShot_Item"), weapon);
        }

        private static void CreateHiddenCleaveWeapon(int i)
        {
            int tier = i + 1;
            JObject weapon = PrepareClone(Load(GreatSword), CleaveWeapons[i], GreatSword);
            weapon["Data"]["Components"] = new JArray();
            weapon["Data"]["m_VisualParameters"]["m_WeaponModel"] = null;
            AddOverride(weapon, "m_VisualParameters.m_WeaponModel");
            Override(weapon, "CanBeUsedInGame", false);
            Override(weapon, "IsUnlootable", true);
            Override(weapon, "WarhammerDamage", CleaveMin[i]);
            Override(weapon, "WarhammerMaxDamage", CleaveMax[i]);
            Override(weapon, "WarhammerPenetration", CleavePen[i]);
            SetText(weapon, "gs-hidden-cleave-name", "gs-hidden-cleave-desc", "gs-hidden-cleave-flavor");
            Save(tier == 1 ? "GuardianSpear_HiddenGreatsword_Item" : $"GuardianSpear_V{tier}_HiddenCleave_Item", weapon);
        }

        private static void CreateAttackAbility(int i, bool burst, JObject overrideTemplate, JObject ammoTemplate)
        {
            int tier = i + 1;
            string id = burst ? BurstAbilities[i] : ShotAbilities[i];
            string prototype = burst ? StandardBoltBurst : StandardBoltShot;
            string hiddenWeapon = burst ? BurstWeapons[i] : ShotWeapons[i];
            JObject ability = PrepareClone(Load(prototype), id, prototype);
            JObject overrideWeapon = (JObject)overrideTemplate.DeepClone();
            overrideWeapon["name"] = $"$WarhammerOverrideAbilityWeapon$guardian-spear-v{tier}-{(burst ? "burst" : "shot")}";
            overrideWeapon["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            overrideWeapon["m_Weapon"] = "!bp_" + hiddenWeapon;
            overrideWeapon["m_ForceShowWeaponDamageInUi"] = true;
            ((JArray)ability["Data"]["Components"]).Add(overrideWeapon);
            AddOverride(ability, overrideWeapon["name"].ToString());
            JObject ammo = (JObject)ammoTemplate.DeepClone();
            ammo["name"] = $"$AbilityAmmoLogic$guardian-spear-v{tier}-{(burst ? "burst" : "shot")}";
            ammo["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            ammo["NoAmmoRequired"] = false;
            ammo["AdditionalAmmoCost"] = 0;
            ((JArray)ability["Data"]["Components"]).Add(ammo);
            AddOverride(ability, ammo["name"].ToString());
            ability["Data"]["m_FXSettings"] = "!bp_" + BolterFx;
            ability["Data"]["m_DisplayName"] = Localized(burst ? "gs-burst-name" : "gs-shot-name");
            ability["Data"]["m_Description"] = Localized($"gs-v{tier}-{(burst ? "burst" : "shot")}-desc");
            AddOverride(ability, "m_FXSettings");
            AddOverride(ability, "m_DisplayName");
            AddOverride(ability, "m_Description");
            SetUnityReference(ability, "m_Icon", burst ? BurstFireIconGuid : SingleShotIconGuid, 21300000L);
            Save(burst ? (tier == 1 ? "GuardianSpear_BoltBurst_Ability" : $"GuardianSpear_V{tier}_BoltBurst_Ability") :
                (tier == 1 ? "GuardianSpear_BoltShot_Ability" : $"GuardianSpear_V{tier}_BoltShot_Ability"), ability);
        }

        private static void CreateCleaveAbility(int i, JObject overrideTemplate)
        {
            int tier = i + 1;
            JObject ability = PrepareClone(Load(TwoHandedSwordCleave), CleaveAbilities[i], TwoHandedSwordCleave);
            JObject overrideWeapon = (JObject)overrideTemplate.DeepClone();
            overrideWeapon["name"] = $"$WarhammerOverrideAbilityWeapon$guardian-spear-v{tier}-cleave";
            overrideWeapon["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" };
            overrideWeapon["m_Weapon"] = "!bp_" + CleaveWeapons[i];
            overrideWeapon["m_ForceShowWeaponDamageInUi"] = true;
            ((JArray)ability["Data"]["Components"]).Add(overrideWeapon);
            AddOverride(ability, overrideWeapon["name"].ToString());
            ability["Data"]["m_DisplayName"] = Localized("gs-cleave-name");
            ability["Data"]["m_Description"] = Localized($"gs-v{tier}-cleave-desc");
            AddOverride(ability, "m_DisplayName");
            AddOverride(ability, "m_Description");
            SetUnityReference(ability, "m_Icon", TwoHandedCleaveIconGuid, 21300000L);
            Save(tier == 1 ? "GuardianSpear_Cleave_Ability" : $"GuardianSpear_V{tier}_Cleave_Ability", ability);
        }

        private static void CreateModifierFeature(int i)
        {
            int tier = i + 1;
            JObject feature = PrepareClone(Load(WsModifierReference), ModifierFeatures[i], WsModifierReference);
            feature["Meta"]["ShadowDeleted"] = false;
            JArray components = new JArray();

            JObject ws = CloneComponent(WsModifierReference, "AddStatBonus", $"$AddStatBonus$guardian-spear-v{tier}-ws");
            ws["Stat"] = "WarhammerWeaponSkill";
            ws["Value"] = SkillBonus[i];
            components.Add(ws);

            JObject bs = CloneComponent(BsModifierReference, "AddStatBonus", $"$AddStatBonus$guardian-spear-v{tier}-bs");
            bs["Stat"] = "WarhammerBallisticSkill";
            bs["Value"] = SkillBonus[i];
            components.Add(bs);

            JObject parry = CloneComponent(ParryModifierReference, "WarhammerParryChanceModifierDefender", $"$WarhammerParryChanceModifierDefender$guardian-spear-v{tier}");
            parry["Restrictions"]["Property"]["Getters"] = new JArray();
            parry["ParryChance"]["Value"] = ParryBonus[i];
            components.Add(parry);

            JObject criticalChance = CloneComponent(CriticalChanceReference, "WarhammerRighteousFuryBonus", $"$WarhammerRighteousFuryBonus$guardian-spear-v{tier}");
            criticalChance["Restrictions"]["Property"]["Getters"] = new JArray();
            criticalChance["Value"]["Value"] = CriticalChance[i];
            criticalChance["SpecificRangeType"] = false;
            components.Add(criticalChance);

            JObject criticalDamage = CloneComponent(CriticalDamageReference, "WarhammerCriticalDamageModifierInitiator", $"$WarhammerCriticalDamageModifierInitiator$guardian-spear-v{tier}");
            criticalDamage["Restrictions"]["Property"]["Getters"] = new JArray();
            criticalDamage["PercentCriticalDamageModifier"]["Value"] = CriticalDamage[i];
            components.Add(criticalDamage);

            feature["Data"]["Components"] = components;
            feature["Data"]["m_DisplayName"] = Localized($"gs-v{tier}-modifier-name");
            feature["Data"]["m_Description"] = Localized($"gs-v{tier}-modifier-desc");
            feature["Data"]["HideInUI"] = false;
            feature["Data"]["HideInCharacterSheetAndLevelUp"] = true;
            AddOverride(feature, "m_DisplayName");
            AddOverride(feature, "m_Description");
            AddOverride(feature, "HideInUI");
            AddOverride(feature, "HideInCharacterSheetAndLevelUp");
            Save($"GuardianSpear_V{tier}_Modifiers_Feature", feature);
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

        [MenuItem("Astartes Custodes/Build Guardian Spear prototype")]
        public static void Build()
        {
            Generate();
            SentinelSwordGenerator.Generate();
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

        [MenuItem("Astartes Custodes/Export endgame weapon survey")]
        public static void ExportEndgameWeaponSurvey()
        {
            string output = Path.GetFullPath("BlueprintAnalysis/endgame-weapon-survey.json");
            var results = new JArray();
            foreach (BlueprintItemWeapon weapon in BlueprintsDatabase.LoadAllOfType<BlueprintItemWeapon>())
            {
                string path = BlueprintsDatabase.GetAssetPath(weapon) ?? "";
                if (path.IndexOf("CH5", StringComparison.OrdinalIgnoreCase) < 0 &&
                    path.IndexOf("Chapter5", StringComparison.OrdinalIgnoreCase) < 0 &&
                    path.IndexOf("Unique", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                JObject root = Load(weapon.AssetGuid.ToString());
                JArray components = (JArray)root["Data"]?["Components"];
                JArray facts = new JArray(components?.Children<JObject>()
                    .Where(c => c["$type"]?.ToString().Contains("AddFactToEquipmentWielder") == true)
                    .Select(c => c["m_Fact"]?.ToString()).Where(value => !string.IsNullOrEmpty(value)) ?? Enumerable.Empty<string>());
                if (facts.Count == 0) continue;
                JObject data = (JObject)root["Data"];
                results.Add(new JObject
                {
                    ["id"] = weapon.AssetGuid.ToString(), ["path"] = path, ["facts"] = facts,
                    ["damage"] = data["WarhammerDamage"], ["maxDamage"] = data["WarhammerMaxDamage"],
                    ["penetration"] = data["WarhammerPenetration"], ["recoil"] = data["WarhammerRecoil"],
                    ["range"] = data["WarhammerMaxDistance"], ["ammo"] = data["WarhammerMaxAmmo"],
                    ["rateOfFire"] = data["RateOfFire"], ["family"] = data["Family"],
                    ["classification"] = data["Classification"]
                });
            }
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllText(output, results.ToString(Formatting.Indented));
            UnityEngine.Debug.Log($"[AstartesCustodes] Endgame weapon survey exported: {results.Count} weapons to {output}");
        }

        [MenuItem("Astartes Custodes/Export endgame weapon facts")]
        public static void ExportEndgameWeaponFacts()
        {
            string output = Path.GetFullPath("BlueprintAnalysis/endgame-weapon-facts");
            Directory.CreateDirectory(output);
            string[] ids =
            {
                "31e68b1e7b7342759e011fd556764d01", "c31185d9515d4727aae60696c6fa96b9",
                "53c19a9468d24539863989b3be9ed1f5", "f70e4a5d21ba4bc9a7fce7e3e84bb59f", "903bb235e56d4dbebbfaf9372976b66f",
                "cdd3a569389d4a90b4b4c859652b3e19", "9962dd8fa12e4c8f8b6fe4832ae356c7",
                "572c6fba29f8402fa5d86e157edf8f29", "adf766ad17194a3080f20cd93be25392",
                "bedeb1015ba844e29223c658a5bfdd47", "717d2df9726144debabacc4d027bb5c1"
            };
            foreach (string id in ids)
            {
                JObject root = Load(id);
                string name = root["Data"]?["name"]?.ToString() ?? id;
                foreach (char invalid in Path.GetInvalidFileNameChars()) name = name.Replace(invalid, '_');
                File.WriteAllText(Path.Combine(output, name + "_" + id + ".json"), root.ToString(Formatting.Indented));
            }
            UnityEngine.Debug.Log("[AstartesCustodes] Endgame weapon facts exported to " + output);
        }

        [MenuItem("Astartes Custodes/Export equipment modifier references")]
        public static void ExportEquipmentModifierReferences()
        {
            string output = Path.GetFullPath("BlueprintAnalysis/GuardianSpear/equipment-modifier-references");
            Directory.CreateDirectory(output);
            int count = 0;
            foreach (Kingmaker.UnitLogic.Progression.Features.BlueprintFeature feature in
                BlueprintsDatabase.LoadAllOfType<Kingmaker.UnitLogic.Progression.Features.BlueprintFeature>())
            {
                JObject root = Load(feature.AssetGuid.ToString());
                string json = root.ToString(Formatting.None);
                if (json.IndexOf("WeaponSkill", StringComparison.OrdinalIgnoreCase) < 0 &&
                    json.IndexOf("BallisticSkill", StringComparison.OrdinalIgnoreCase) < 0 &&
                    json.IndexOf("CriticalHit", StringComparison.OrdinalIgnoreCase) < 0 &&
                    json.IndexOf("CriticalDamage", StringComparison.OrdinalIgnoreCase) < 0 &&
                    json.IndexOf("ParryChance", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                string name = root["Data"]?["name"]?.ToString() ?? feature.AssetGuid.ToString();
                foreach (char invalid in Path.GetInvalidFileNameChars()) name = name.Replace(invalid, '_');
                File.WriteAllText(Path.Combine(output, name + "_" + feature.AssetGuid + ".json"), root.ToString(Formatting.Indented));
                if (++count >= 300) break;
            }
            UnityEngine.Debug.Log($"[AstartesCustodes] Exported {count} equipment modifier references to {output}");
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

        private static void SetAbilitySlot(JObject weapon, string slotName, string type, string ability, string fx, int ap, string mode = "Default")
        {
            JObject slot = (JObject)weapon["Data"]["AbilityContainer"][slotName];
            slot["Type"] = type;
            slot["Mode"] = mode;
            slot["m_Ability"] = "!bp_" + ability;
            slot["m_FXSettings"] = "!bp_" + fx;
            slot["AP"] = ap;
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
            // Blueprint Author is Owlcat's internal Authors enum, not a free-form mod credit.
            // Preserve the valid value inherited from the vanilla prototype.
            File.WriteAllText(Path.Combine(Blueprints, name + ".jbp"), root.ToString(Formatting.Indented));
        }

        private static JObject Entry(string text) => new JObject { ["Offset"] = 0, ["Text"] = text };

        private static void WriteLocalization()
        {
            string[] guardianSpearNames =
            {
                "Custodian's Vigil", "Auric Watch", "Praetorian's Oath",
                "Wrath of the Ten Thousand", "Voice of the Golden Throne", "The Emperor's Vengeance"
            };
            JObject strings = new JObject
            {
                ["gs-flavor"] = Entry("The Guardian Spear is both symbol of office and peerless instrument of the Emperor's judgement."),
                ["gs-type-name"] = Entry("Guardian Spear"),
                ["gs-hidden-name"] = Entry("Guardian Spear Hidden Bolter"),
                ["gs-hidden-desc"] = Entry("Internal ranged profile used by the Guardian Spear."),
                ["gs-hidden-flavor"] = Entry("Not intended for inventory or loot."),
                ["gs-hidden-cleave-name"] = Entry("Guardian Spear Hidden Greatsword"),
                ["gs-hidden-cleave-desc"] = Entry("Internal melee profile used to select the two-handed sword cleave animation."),
                ["gs-hidden-cleave-flavor"] = Entry("Not intended for inventory or loot."),
                ["gs-shot-name"] = Entry("Bolt Shot"),
                ["gs-cleave-name"] = Entry("Guardian Cleave"),
                ["gs-burst-name"] = Entry("Bolt Burst"),
                ["sentinel-sword-name"] = Entry("Sentinel Sword"),
                ["sentinel-sword-desc"] = Entry("A master-crafted power sword of the Adeptus Custodes."),
                ["sentinel-sword-flavor"] = Entry("A gleaming blade fashioned for the unwavering guardians of the Golden Throne."),
                ["sentinel-power-field-name"] = Entry("Activate Power Field"),
                ["sentinel-power-field-desc"] = Entry("Activates the Sentinel Sword's power field for 4 rounds. Attacks made with this weapon deal +6 additional damage. Cooldown: 5 rounds."),
                ["sentinel-power-field-buff-desc"] = Entry("The Sentinel Sword is energised. Its attacks deal +6 additional damage."),
                ["sentinel-wave-name"] = Entry("Sentinel Wave"),
                ["sentinel-wave-desc"] = Entry("Swing the Sentinel Sword to project a cutting wave of force at an enemy up to 5 cells away. The attack uses the weapon's normal damage and armour penetration. Cost: 1 AP.")
            };
            for (int i = 0; i < 6; i++)
            {
                int tier = i + 1;
                string levelRange = tier == 1 ? "1-9" : tier == 6 ? "50-55" : $"{i * 10}-{i * 10 + 9}";
                strings[$"gs-v{tier}-name"] = Entry(guardianSpearNames[i]);
                strings[$"gs-v{tier}-desc"] = Entry(
                    $"A master-crafted Custodes hybrid weapon.\n\n" +
                    $"• Levels: {levelRange}\n" +
                    $"• Bolt Burst: {BurstShots[i]} shots\n" +
                    $"• Magazine: {Ammo[i]} rounds\n\n" +
                    $"Modifiers while equipped:\n" +
                    $"• +{SkillBonus[i]} Weapon Skill\n" +
                    $"• +{SkillBonus[i]} Ballistic Skill\n" +
                    $"• +{ParryBonus[i]}% parry chance\n" +
                    $"• +{CriticalChance[i]}% critical hit chance\n" +
                    $"• +{CriticalDamage[i]}% critical damage");
                strings[$"gs-v{tier}-modifier-name"] = Entry($"Guardian Spear V{tier} Mastery");
                strings[$"gs-v{tier}-modifier-desc"] = Entry(
                    $"+{SkillBonus[i]} Weapon Skill, +{SkillBonus[i]} Ballistic Skill, +{ParryBonus[i]}% parry chance, " +
                    $"+{CriticalChance[i]}% critical hit chance and +{CriticalDamage[i]}% critical damage.");
                strings[$"gs-v{tier}-shot-desc"] = Entry(
                    $"Fire one precise bolt for {ShotMin[i]}-{ShotMax[i]} damage and {ShotPen[i]}% armour penetration. Magazine: {Ammo[i]} rounds.");
                strings[$"gs-v{tier}-burst-desc"] = Entry(
                    $"Fire {BurstShots[i]} bolts, each dealing {BurstMin[i]}-{BurstMax[i]} damage with {BurstPen[i]}% armour penetration. Consumes {BurstShots[i]} of {Ammo[i]} rounds.");
                strings[$"gs-v{tier}-cleave-desc"] = Entry(
                    $"Sweep the Guardian Spear through a wide area for {CleaveMin[i]}-{CleaveMax[i]} damage and {CleavePen[i]}% armour penetration.");
            }
            SentinelSwordGenerator.AddLocalizationEntries(strings);
            File.WriteAllText(Path.Combine(Root, "Localization", "enGB.json"),
                new JObject { ["strings"] = strings }.ToString(Formatting.Indented));
        }
    }
}
