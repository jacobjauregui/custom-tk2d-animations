using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Tool used to export animation sprites as a PNG.
/// </summary>
public class TK2DAnimationExporter : EditorWindow
{
	private GameObject _targetPrefab;
	private string _animationFolder = string.Empty;


	[MenuItem("Tools/TK2D/Animation Frames Exporter")]
	static void Init()
	{
		GetWindow<TK2DAnimationExporter>("TK2D Frames Exporter");
	}

	/// <summary>
	/// GUI layout for this tool.
	/// </summary>
	private void OnGUI()
	{
		GUILayout.BeginVertical();
		GUILayout.Label("TK2D Sprite Animation", EditorStyles.boldLabel);
		GUILayout.Space(20);

		if (_targetPrefab == null)
		{
			EditorGUILayout.HelpBox("Select the animated character, prefab or GameObject. For example 'Assets/Animations/Hornet Anim' or 'Assets/Animations/Knight'.", MessageType.Info);
		}

		_targetPrefab = (GameObject)EditorGUILayout.ObjectField("GameObject / Prefab", _targetPrefab, typeof(GameObject), true);
		GUILayout.Space(10);

		if (_targetPrefab.TryGetComponent<tk2dSpriteAnimation>(out tk2dSpriteAnimation animation))
		{
			int totalFrames = 0;
			foreach (var clip in animation.clips)
			{
				totalFrames += clip.frames.Length;
			}

			GUILayout.Label($"{animation.clips.Length} clips found ({totalFrames} frames) in {animation.name}.", EditorStyles.boldLabel);
			GUILayout.Space(20);

			GUILayout.BeginHorizontal();
			_animationFolder = EditorGUILayout.TextField(_animationFolder, EditorStyles.boldLabel);
			GUILayout.Space(5);

			if (GUILayout.Button("Browse"))
			{
				_animationFolder = EditorUtility.OpenFolderPanel("Select your animations folder", _animationFolder, "Animations");
			}

			if (string.IsNullOrEmpty(_animationFolder))
			{
				EditorGUILayout.HelpBox("Please, select a folder to save the animations.", MessageType.Error);
				return;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(15);

			if (GUILayout.Button("Export"))
			{
				Debug.Log($"Exporting {totalFrames} frames from {animation.clips.Length} clips...");
				ExportFrames(animation);
			}
			GUILayout.EndVertical();
		}
		else
		{
			EditorGUILayout.HelpBox("Error. No animatios found in the selected Gameobject.", MessageType.Error);
			GUILayout.EndVertical();
			return;
		}
	}

	/// <summary>
	/// <para>
	/// Exports each sprite (frame) from each animation (clip) from the specified prefab as a separate PNG image,<br/>
	/// and creates a JSON file with the clip information serialized.
	/// </para>
	/// The images are saved in a folder named after the clip, inside the main animation folder.
	/// </summary>
	/// <param name="animation"></param>
	private void ExportFrames(tk2dSpriteAnimation animation)
	{
		string animationPath = Path.Combine(_animationFolder, animation.name);
		if (!Directory.Exists(animationPath))
		{
			Directory.CreateDirectory(animationPath);
		}

		for (int i = 0; i < animation.clips.Length; i++)
		{
			tk2dSpriteAnimationClip clip = animation.clips[i];

			if (clip == null)
			{
				continue;
			}

			string clipPath = Path.Combine(animationPath, clip.name);

			if (!Directory.Exists(clipPath))
			{
				Directory.CreateDirectory(clipPath);
			}

			var info = new StringBuilder("{\r\n");
			info.AppendLine($"\t\"Name\": \"{clip.name}\",");
			info.AppendLine($"\t\"WrapMode\": \"{clip.wrapMode}\",");
			info.AppendLine($"\t\"LoopStart\": {clip.loopStart},");
			info.AppendLine($"\t\"Duration\": {clip.Duration},");
			info.AppendLine($"\t\"Fps\": {clip.fps},");
			info.AppendLine($"\t\"Length\": {clip.frames.Length},");
			info.AppendLine("\t\"Frames\": [");

			for (int j = 0; j < clip.frames.Length; j++)
			{
				tk2dSpriteAnimationFrame frame = clip.frames[j];
				tk2dSpriteDefinition def = frame.spriteCollection.spriteDefinitions[frame.spriteId];
				Material material = def.material;
				Texture2D texture = material.mainTexture as Texture2D;
				Texture2D atlas = GetAtlas(texture);

				Vector2 minUV = def.uvs[0];
				Vector2 maxUV = def.uvs[0];
				for (int k = 1; k < def.uvs.Length; k++)
				{
					minUV = Vector2.Min(minUV, def.uvs[k]);
					maxUV = Vector2.Max(maxUV, def.uvs[k]);
				}

				int x = Mathf.FloorToInt(minUV.x * atlas.width);
				int y = Mathf.FloorToInt(minUV.y * atlas.height);
				int w = Mathf.CeilToInt((maxUV.x - minUV.x) * atlas.width);
				int h = Mathf.CeilToInt((maxUV.y - minUV.y) * atlas.height);

				x = Mathf.Clamp(x, 0, atlas.width - 1);
				y = Mathf.Clamp(y, 0, atlas.width - 1);
				w = Mathf.Clamp(w, 1, atlas.width - x);
				h = Mathf.Clamp(h, 1, atlas.width - y);

				string fileName = $"{def.name}_{frame.spriteId}";
				string filePath = $"{clipPath}/{fileName}.png";

				info.AppendLine(j == (clip.frames.Length - 1) ? $"\t\t\"{fileName}\"" : $"\t\t\"{fileName}\",");
				CreateImage(filePath, x, y, w, h, atlas);
			}
			info.AppendLine("\t]");
			info.AppendLine("}");

			string infoFile = $"{clipPath}/{clip.name}.json";
			File.WriteAllText(infoFile, info.ToString());
		}
		AssetDatabase.Refresh();

		Debug.Log("Frames exported succesfully!");
	}

	/// <summary>
	/// It cuts the sprite localized on coordenates 'x' and 'y' with size 'w', 'h' from the atlas. On other words,
	/// creates a PNG image from the specified area of the atlas texture.
	/// </summary>
	/// <param name="imagePath"></param>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="w"></param>
	/// <param name="h"></param>
	/// <param name="atlas"></param>
	private void CreateImage(string imagePath, int x, int y, int w, int h, Texture2D atlas)
	{
		Color[] pixels = atlas.GetPixels(x, y, w, h);

		Texture2D image = new(w, h);
		image.SetPixels(pixels);
		image.Apply();

		File.WriteAllBytes(imagePath, image.EncodeToPNG());
	}

	/// <summary>
	/// Required settings to reimport a readable atlas from the material texture.
	/// </summary>
	/// <param name="texture"></param>
	/// <returns>
	/// The readable atlas as a <see cref="Texture2D"/>.
	/// </returns>
	private Texture2D GetAtlas(Texture2D texture)
	{
		string path = AssetDatabase.GetAssetPath(texture);
		TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);

		if (!importer.isReadable)
		{
			importer.isReadable = true;
			importer.textureType = TextureImporterType.Default;
			importer.textureShape = TextureImporterShape.Texture2D;
			importer.sRGBTexture = true;
			importer.alphaSource = TextureImporterAlphaSource.FromInput;
			importer.alphaIsTransparency = true;
			importer.wrapMode = TextureWrapMode.Clamp;
			importer.filterMode = FilterMode.Bilinear;
			importer.maxTextureSize = texture.width > texture.height ? texture.width : texture.height;
			importer.textureCompression = TextureImporterCompression.CompressedHQ;
			importer.SaveAndReimport();
		}

		return texture;
	}
}
