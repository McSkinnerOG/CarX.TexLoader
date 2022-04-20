using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TexLoader
{
	[BepInPlugin(GUID, PluginName, Version)]
	public class TexLoader : BaseUnityPlugin
	{
		public const string GUID = "valid.tex.loader"; // ALL CREDIT GOES TO BLUE-AMULET THIS IS JUST A PORT OF HIS VSIDELOADER!!!
		public const string PluginName = "TexLoader";
		public const string Version = "2.0.0";
		public static new ManualLogSource Logger;
		public static readonly Harmony harmony = new Harmony(GUID);
		public void Update()
		{
			if (Input.GetKeyDown(keyCodeReload.Value))
			{ 
				if (textureLoadPack.Value) { TextureReplacement.HandleTextures(false); GUICommonGodVoice.ShowText("-=|| RELOADED TEXTURES ||=-", 6f, null, false); Debug.Log("Reloaded textures"); }
				else { Debug.Log("Texture loading not enabled in config"); }
			}
			if (Input.GetKeyDown(keyCodeLastPack.Value))
			{
				if (textureLoadPack.Value)
				{
					textureLoadPackInt.Value--;
					if (textureLoadPackInt.Value < 0)
					{
						textureLoadPackInt.Value = 0;
						Config.Save();
					}
					Config.Save(); 
					TextureReplacement.HandleTextures(false);
					string labeltext = " -=|| TEXTUREPACK: " + TextureReplacement.SubMapExposedString.ToUpper() + "  LOADED! ||=- ";
					GUICommonGodVoice.ShowText(labeltext, 6f, null, false);
					Debug.Log("Reloaded textures");
				} else { Debug.Log("Texture loading not enabled in config"); }
			}
			if (Input.GetKeyDown(keyCodeNextPack.Value))
			{
				if (textureLoadPack.Value)
				{
					textureLoadPackInt.Value++;
					if (textureLoadPackInt.Value > TextureReplacement.SubMapInt)
					{
						textureLoadPackInt.Value = TextureReplacement.SubMapInt;
						Config.Save();
					}
					Config.Save();
                    TextureReplacement.HandleTextures(false);
					string labeltext = " -=|| TEXTUREPACK: " + TextureReplacement.SubMapExposedString.ToUpper() + "  LOADED! ||=- ";
					GUICommonGodVoice.ShowText(labeltext, 6f, null, false);
					Debug.Log("Reloaded textures");
				} else { Debug.Log("Texture loading not enabled in config"); }
			} 
		} 
		public void Awake()
		{
			Logger = base.Logger;
			InitConfig();
			harmony.PatchAll(Assembly.GetExecutingAssembly()); 
			if (textureDump.Value || textureLoadPack.Value)
			{
				SceneManager.sceneLoaded += TextureReplacement.OnSceneLoaded;
				//TextureReplacement.OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single); // Trigger for current scene as well
			}
		}
		public static Dictionary<Type, string> typeStr = new Dictionary<Type, string>();
		public static ConfigEntry<bool> betterLighting;
		public static ConfigEntry<bool> textureDump;
		public static ConfigEntry<bool> textureLoadOG;
		public static ConfigEntry<bool> textureLoadPack;
		public static ConfigEntry<int> textureLoadPackInt;
		public static ConfigEntry<bool> detectCollision;
		public static ConfigEntry<FilterMode> textureFilter;
		public static ConfigEntry<KeyCode> keyCodeReload;
		public static ConfigEntry<KeyCode> keyCodeNextPack;
		public static ConfigEntry<KeyCode> keyCodeLastPack;
		public static ConfigEntry<bool> useTextureName;
		public static string[] blackList = { "Font Texture", "Heightmap_basematerial_ClearedMaskTex", "Color Grading Log LUT", "Hidden/BlitCopy_MainTex", "UnityWhite" };
		public static string[] ignoreName = { "diffuse" };
		public static string[] normalFix;
		public void InitConfig()
		{
			betterLighting = Config.Bind("Shader", "BetterLighting", true, "Removes banding in scene lighting");
			textureDump = Config.Bind("Texture", "Dump", false, "Dump textures to disk");
			textureLoadOG = Config.Bind("Texture", "LoadOG", false, "Load ORIGINAL textures from disk");
			textureLoadPack = Config.Bind("Texture", "LoadPack", false, "Load Pack textures from disk based on ORDER IN FOLDER");
			textureLoadPackInt = Config.Bind("Texture", "LoadPackInt", 0, "Int For Pack you want to load");
			detectCollision = Config.Bind("Texture", "DetectCollision", false, "Detect textures with same name but different contents");
			textureFilter = Config.Bind("Texture", "TextureFilter", FilterMode.Trilinear, "Texture filtering mode");
			keyCodeReload = Config.Bind("Texture", "Reload Pack Keybind", KeyCode.UpArrow, "Reload Key For Textures");
			keyCodeNextPack = Config.Bind("Texture", "Next Pack Keybind", KeyCode.RightArrow, "Next Pack Key.");
			keyCodeLastPack = Config.Bind("Texture", "Last Pack Keybind", KeyCode.LeftArrow, "Last Pack Key.");
			useTextureName = Config.Bind("Texture", "UseTextureName", true, "Use texture names instead of material name");
			blackList = Config.Bind("Texture", "BlackList", String.Join(",", blackList), "Textures not to dump").Value.Split(',');
			ignoreName = Config.Bind("Texture", "IgnoreName", String.Join(",", ignoreName), "Texture names to ignore").Value.Split(',');
			normalFix = Config.Bind("Texture", "NormalFix", String.Join(",", TextureReplacement.normalMap), "Property names containing DXT5nm normal maps").Value.Split(',');
		}
	}
}
