using AbilityApi;
using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static System.Net.Mime.MediaTypeNames;

namespace StatBooster
{
    [BepInPlugin("org.000diggity000.StatBoostAbility", "StatBoostAbility", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static System.IO.Stream GetResourceStream(string namespaceName, string path) => Assembly.GetExecutingAssembly().GetManifestResourceStream($"{namespaceName}.{path}");
        private void Awake()
        {
            Harmony harmony = new Harmony("org.000diggity000.StatBoostAbility");
            harmony.PatchAll();
            var directoryToModFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var GhostAbilityPrefab = ConstructInstantAbility<GhostAbility>("Stat Booster");
            GhostAbilityPrefab.gameObject.AddComponent<PlayerPhysics>();
            Texture2D GhostAbilityTex = new Texture2D(1, 1);
            GhostAbilityTex.LoadImage(ReadFully(GetResourceStream("StatBooster", "AbilityIcon.png")));
            var iconSprite = Sprite.Create(GhostAbilityTex, new Rect(0f, 0f, GhostAbilityTex.width, GhostAbilityTex.height), new Vector2(0.5f, 0.5f));
            NamedSprite ghostAbility = new NamedSprite("Stat Booster", iconSprite, GhostAbilityPrefab.gameObject, true);
            Api.RegisterNamedSprites(ghostAbility, true);
        }
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        public static T ConstructInstantAbility<T>(string name) where T : MonoUpdatable
        {
            GameObject parent = new GameObject(name);
            GameObject.DontDestroyOnLoad(parent);

            Ability ability = parent.AddComponent<Ability>();

            parent.AddComponent<FixTransform>();
            parent.AddComponent<SpriteRenderer>();
            parent.AddComponent<PlayerBody>();
            parent.AddComponent<DPhysicsBox>();
            parent.AddComponent<PlayerCollision>();
            MonoUpdatable updatable = parent.AddComponent<T>();

            if (updatable == null)
            {
                GameObject.Destroy(parent);
                throw new MissingReferenceException("Invalid type was fed to ConstructInstantAbility");
            }

            return (T)updatable;
        }
    }
    public class GhostAbility : MonoUpdatable, IAbilityComponent
    {
        public Ability ab;
        Player player;
        FixTransform playerTransform;
        PlayerBody body;
        PlayerPhysics playerPhysics;
        SpriteRenderer spriteRenderer;
        public void Awake()
        {
            Updater.RegisterUpdatable(this);
            ab = GetComponent<Ability>();
            ab.Cooldown = (Fix)0;
            //playerTransform = base.GetComponent<FixTransform>();
            //body = base.GetComponent<SpriteRenderer>();
            body = GetComponent<PlayerBody>();
            playerPhysics = GetComponent<PlayerPhysics>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        public void Update()
        {

        }


        public override void Init()
        {

        }
        private void OldUpdate(Fix simDeltaTime)
        {
            
        }
        public override void UpdateSim(Fix SimDeltaTime)
        {
            
            OldUpdate(SimDeltaTime);
        }

        public void OnEnterAbility()
        {
            spriteRenderer.material = ab.GetPlayerInfo().playerMaterial;
            body.position = ab.GetPlayerInfo().position;
            //playerPhysics.UnGround(true, false);
            //playerPhysics.gravity_modifier = (Fix)0;
            playerPhysics.SyncPhysicsTo(ab.GetPlayerInfo());
            AbilityExitInfo info = default(AbilityExitInfo);
            info.position = body.position;
            info.selfImposedVelocity = body.selfImposedVelocity;
            ab.GetPlayerInfo().slimeController.gameObject.GetComponent<PlayerPhysics>().Speed = (Fix)30L;
            ab.GetPlayerInfo().slimeController.gameObject.GetComponent<PlayerPhysics>().jumpStrength = (Fix)60L;
            ab.GetPlayerInfo().slimeController.gameObject.GetComponent<PlayerPhysics>().gravity_modifier = (Fix)0.7f;
            ab.Cooldown = (Fix)999999999;
            ab.ExitAbility(info);
            //ab.GetPlayerInfo().slimeController.enabled = false;
            //ab.GetPlayerInfo().slimeController.gameObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
            
        }

        public void ExitAbility(AbilityExitInfo info)
        {
            this.enabled = false;
            ab.ExitAbility(info);
        }

        public void OnScaleChanged(Fix scaleMultiplier)
        {
            throw new NotImplementedException();
        }
    }
}
