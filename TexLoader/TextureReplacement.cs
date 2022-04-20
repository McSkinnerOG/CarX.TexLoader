using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection; 
using UnityEngine;
using UnityEngine.SceneManagement; 
namespace TexLoader
{
	public static class TextureReplacement
	{
		public static Dictionary<string, Texture> texSet = new Dictionary<string, Texture>();
		public static Dictionary<string, TextureInfo> loadedTextures = new Dictionary<string, TextureInfo>(); 
		public static string[] normalMap = { "_AYELMAO" };
		public static int SubMapInt = 0;
		public static string SubMapExposedString = " ";
		public static string SUBmapPath = " ";
		public static void HandleTextures(bool allowDump)
		{
			string basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Textures"); 
			string dumpPath = Path.Combine(basePath, "Dump");  
			string dumpMapPath = Path.Combine(dumpPath, SceneManager.GetActiveScene().name.ToLowerInvariant());  
			string loadPath = Path.Combine(basePath, "Load");  
			string mapPath = Path.Combine(loadPath, SceneManager.GetActiveScene().name.ToLowerInvariant());
			Directory.CreateDirectory(basePath);
			Directory.CreateDirectory(dumpPath);
			Directory.CreateDirectory(dumpMapPath);
			Directory.CreateDirectory(loadPath);
			Directory.CreateDirectory(mapPath); 
			Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
			TexLoader.Logger.LogInfo("Found " + materials.Length + " materials");
			foreach (Material material in materials)
			{
				string[] propertyNames = material.GetTexturePropertyNames();
				foreach (string propertyName in propertyNames)
				{
					string matTexName = material.name + (propertyName.StartsWith("_") ? "" : "-") + propertyName;
					string texName = null;
					Texture texture = material.GetTexture(propertyName);
					if (texture != null)
					{
						bool dumpTexture = allowDump;
						if (TexLoader.useTextureName.Value && !TexLoader.ignoreName.Contains(texture.name)) { texName = texture.name; } 
						if (String.IsNullOrWhiteSpace(texName)) { texName = matTexName; }
						if (TexLoader.blackList.Contains(texName) || loadedTextures.Any(pair => pair.Value.tex == texture)) { dumpTexture = false; }
						else if (!texSet.ContainsKey(texName)) { if (TexLoader.detectCollision.Value) { texSet.Add(texName, texture); } }
						else if (texSet[texName] != texture)
						{ TexLoader.Logger.LogWarning("Duplicate texture name: " + texName); }
						if (TexLoader.textureDump.Value && dumpTexture)
						{
							if (texture is Texture2D tex2d)
							{
								Texture2D newTexture = DuplicateTexture(tex2d);
								if (TexLoader.normalFix.Contains(propertyName))
								{
									Color[] pixels = newTexture.GetPixels();
									for (int i = 0; i < pixels.Length; i++)
									{
										pixels[i].r *= pixels[i].a;
										float x = pixels[i].r * 2f - 1f;
										float y = pixels[i].g * 2f - 1f;
										pixels[i].b = (Mathf.Sqrt(1f - x * x - y * y) + 1f) / 2f;
										pixels[i].a = 1f;
									}
									newTexture.SetPixels(pixels);
								}
								File.WriteAllBytes(Path.Combine(dumpMapPath, texName + ".png"), newTexture.EncodeToPNG());
							}
							else { TexLoader.Logger.LogWarning("Don't know how to handle texture of type " + texture.GetType().Name + " (" + texName + ")"); }
						}else if (TexLoader.textureLoadPack.Value)
						{
							SUBmapPath = Directory.GetDirectories(mapPath).ElementAt(TexLoader.textureLoadPackInt.Value);
							SubMapExposedString = SUBmapPath.Replace(mapPath, "");
							SubMapInt = Directory.GetDirectories(mapPath).Count();
							Directory.CreateDirectory(SUBmapPath); 
							string texPath = Path.Combine(SUBmapPath, matTexName + ".png");
							if (!File.Exists(texPath) && !String.IsNullOrWhiteSpace(texName) && texName != matTexName) { texPath = Path.Combine(SUBmapPath, texName + ".png"); }
							if (File.Exists(texPath))
							{
								Texture2D tex;
								bool needsLoad = false;
								DateTime time = File.GetLastWriteTime(texPath);
								if (loadedTextures.ContainsKey(texPath))
								{
									tex = loadedTextures[texPath].tex; // Reuse existing texture
									if (!loadedTextures[texPath].time.Equals(time)) { needsLoad = true; }
								}
								else
								{
									tex = new Texture2D(2, 2, TextureFormat.RGBA32, true, normalMap.Contains(propertyName));
									loadedTextures.Add(texPath, new TextureInfo(tex, new DateTime()));
									needsLoad = true;
								}
								if (texture != null) { tex.name = texture.name; }
								tex.filterMode = TexLoader.textureFilter.Value;
								if (needsLoad)
								{
									if (tex.LoadImage(File.ReadAllBytes(texPath), !TexLoader.normalFix.Contains(propertyName)))
									{
										if (TexLoader.normalFix.Contains(propertyName) && tex.isReadable)
										{
											Color[] pixels = tex.GetPixels();
											for (int i = 0; i < pixels.Length; i++)
											{
												pixels[i].a = pixels[i].r;
												pixels[i].r = 1f;
												pixels[i].b = pixels[i].g;
											}
											tex.SetPixels(pixels);
										}
										TexLoader.Logger.LogInfo("Loaded " + Path.GetFileName(texPath) + " (" + tex.width + "x" + tex.height + ") for " + material.name + "." + propertyName);
										material.SetTexture(propertyName, tex);
										loadedTextures[texPath].time = time;
									}
									else { TexLoader.Logger.LogError("Failed to load " + Path.GetFileName(texPath)); }
								}
								else
								{
									TexLoader.Logger.LogInfo("Reusing loaded texture " + Path.GetFileName(texPath) + " for " + material.name + "." + propertyName);
									material.SetTexture(propertyName, tex);
								}
							}
						}else if (TexLoader.textureLoadOG.Value)
						{
							string texPath = Path.Combine(dumpMapPath, matTexName + ".png");
							if (!File.Exists(texPath) && !String.IsNullOrWhiteSpace(texName) && texName != matTexName) { texPath = Path.Combine(dumpMapPath, texName + ".png"); }
							if (File.Exists(texPath))
							{
								Texture2D tex;
								bool needsLoad = false;
								DateTime time = File.GetLastWriteTime(texPath);
								if (loadedTextures.ContainsKey(texPath))
								{
									tex = loadedTextures[texPath].tex; // Reuse existing texture
									if (!loadedTextures[texPath].time.Equals(time)) { needsLoad = true; }
								}
								else
								{
									tex = new Texture2D(2, 2, TextureFormat.RGBA32, true, normalMap.Contains(propertyName));
									loadedTextures.Add(texPath, new TextureInfo(tex, new DateTime()));
									needsLoad = true;
								}
								if (texture != null) { tex.name = texture.name; }
								tex.filterMode = TexLoader.textureFilter.Value;
								if (needsLoad)
								{
									if (tex.LoadImage(File.ReadAllBytes(texPath), !TexLoader.normalFix.Contains(propertyName)))
									{
										if (TexLoader.normalFix.Contains(propertyName) && tex.isReadable)
										{
											Color[] pixels = tex.GetPixels();
											for (int i = 0; i < pixels.Length; i++)
											{
												pixels[i].a = pixels[i].r;
												pixels[i].r = 1f;
												pixels[i].b = pixels[i].g;
											}
											tex.SetPixels(pixels);
										}
										TexLoader.Logger.LogInfo("Loaded " + Path.GetFileName(texPath) + " (" + tex.width + "x" + tex.height + ") for " + material.name + "." + propertyName);
										material.SetTexture(propertyName, tex);
										loadedTextures[texPath].time = time;
									}
									else { TexLoader.Logger.LogError("Failed to load " + Path.GetFileName(texPath)); }
								}
								else
								{
									TexLoader.Logger.LogInfo("Reusing loaded texture " + Path.GetFileName(texPath) + " for " + material.name + "." + propertyName);
									material.SetTexture(propertyName, tex);
								}
							}
						}
					}
					 
				}
			} if (TexLoader.detectCollision.Value) { TexLoader.Logger.LogInfo("Found " + texSet.Count + " textures"); }
		} 
		public static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode) { HandleTextures(TexLoader.textureDump.Value); } 
		public static Texture2D DuplicateTexture(Texture2D source)
		{ 
			RenderTexture renderTex = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, source.graphicsFormat.ToString().EndsWith("_SRGB") ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear);
			Graphics.Blit(source, renderTex);
			Texture2D texture = RenderToTexture(renderTex);
			RenderTexture.ReleaseTemporary(renderTex);
			return texture;
		} 
		public static Texture2D RenderToTexture(RenderTexture renderTex)
		{ 
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = renderTex;
			Texture2D texture = new Texture2D(renderTex.width, renderTex.height, TextureFormat.ARGB32, false, false);
			texture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
			texture.Apply();
			RenderTexture.active = previous;
			return texture;
		}
	}
}
