﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using UMA.CharacterSystem;

namespace UMA
{
	//The DynamicDNAConverterController manages the list Converters (aka DynamicDNAPlugins) the user has decided to use.
	//It is a Scriptable Object, and as Converters are added to it, it creates instances of those and stores them inside itself
	//this is so all the assets this needs are packaged up with it UMA3 style.
	//This asset also calls ApplyDNA on each of the converters when DynamicDNAConverterBehaviour asks it to.
	[System.Serializable]
	public class DynamicDNAConverterController : ScriptableObject
	{
		/// <summary>
		/// The List of all the plugins (converters) assigned to this ConverterController
		/// </summary>
		[SerializeField]
		private List<DynamicDNAPlugin> _plugins = new List<DynamicDNAPlugin>();

		/// <summary>
		/// Contains a list of all the dna names used by all the plugins (converters) assigned to this ConverterController
		/// </summary>
		private List<string> _usedDNANames = new List<string>();

		/// <summary>
		/// The behaviour will assign it self to this converterController, when this converterController is assigned to it, either when ApplyDNAAction is called or the controller is inspected via this Behaviour
		/// </summary>
		private DynamicDNAConverterBehaviour _converterBehaviour;
		[System.NonSerialized]
		private List<DynamicDNAPlugin> _applyDNAPrepassPlugins = new List<DynamicDNAPlugin>();
		[System.NonSerialized]
		private List<DynamicDNAPlugin> _applyDNAPlugins = new List<DynamicDNAPlugin>();
		[System.NonSerialized]
		private bool _prepared = false;

		public DynamicUMADnaAsset DNAAsset
		{
			get
			{
				if (_converterBehaviour != null)
					return _converterBehaviour.dnaAsset;
				else
					return null;
			}
		}

		public DynamicDNAConverterBehaviour converterBehaviour
		{
			get { return _converterBehaviour; }
			set { _converterBehaviour = value; }
		}

		/// <summary>
		/// Returns the number of plugins assigned to this ConverterController Asset
		/// </summary>
		public int PluginCount
		{
			get { return _plugins.Count; }
		}

		#region BACKWARDS COMPATIBILITY

		//Helper methods to make upgrading easier. DynamicDNAConverterBehaviour used to have its own SkeletonModifiers list and StartingPose so these replicate that functionality
		/// <summary>
		/// Gets the first found SkeletonModifiersDNAConverterPlugin in this controllers list and returns its list of SkeletonModifiers. TIP: The controller can have multiple sets of SkeletonModifiers now. Use the GetPlugins methods to get them all.
		/// </summary>
		public List<SkeletonModifier> SkeletonModifiersFirst
		{
			get
			{
				if(GetPlugins(typeof(SkeletonDNAConverterPlugin)).Count > 0)
				{
					return ((GetPlugins(typeof(SkeletonDNAConverterPlugin))[0]) as SkeletonDNAConverterPlugin).skeletonModifiers;
				}
				return new List<SkeletonModifier>();
			}
			set
			{
				if (GetPlugins(typeof(SkeletonDNAConverterPlugin)).Count > 0)
				{
					((GetPlugins(typeof(SkeletonDNAConverterPlugin))[0]) as SkeletonDNAConverterPlugin).skeletonModifiers = value;
				}
			}
		}

		public UMA.PoseTools.UMABonePose StartingPoseFirst
		{
			get
			{
				var bonePosePlugins = GetPlugins(typeof(BonePoseDNAConverterPlugin));
				if (bonePosePlugins.Count > 0)
				{
					for (int i = 0; i < bonePosePlugins.Count; i++)
					{
						if ((bonePosePlugins[i] as BonePoseDNAConverterPlugin).StartingPose != null)
							return (bonePosePlugins[i] as BonePoseDNAConverterPlugin).StartingPose;
					}
				}
				return null;
			}
			set
			{
				var bonePosePlugins = GetPlugins(typeof(BonePoseDNAConverterPlugin));
				if (bonePosePlugins.Count > 0)
				{
					for (int i = 0; i < bonePosePlugins.Count; i++)
					{
						if ((bonePosePlugins[i] as BonePoseDNAConverterPlugin).StartingPose != null)
						{
							(bonePosePlugins[i] as BonePoseDNAConverterPlugin).StartingPose = value;
							return;
						}
					}
				}
			}
		}

		#endregion

		public void Prepare()
		{
			if (!_prepared)
			{
				for (int i = 0; i < _plugins.Count; i++)
				{
					if (_plugins[i].ApplyPass == DynamicDNAPlugin.ApplyPassOpts.Standard)
					{
						if (!_applyDNAPlugins.Contains(_plugins[i]))
							_applyDNAPlugins.Add(_plugins[i]);
					}
					else if (_plugins[i].ApplyPass == DynamicDNAPlugin.ApplyPassOpts.PrePass)
					{
						if (!_applyDNAPrepassPlugins.Contains(_plugins[i]))
						{
							_applyDNAPrepassPlugins.Add(_plugins[i]);
						}
					}
				}
				_prepared = true;
			}
		}

		/// <summary>
		/// Calls ApplyDNA on all this convertersControllers plugins (aka converters) that apply dna during the pre-pass
		/// </summary>
		/// <param name="umaData">The umaData on the avatar</param>
		/// <param name="skeleton">The avatars skeleton</param>
		/// <param name="dnaTypeHash">The dnaTypeHash that this converters behaviour is using</param>
		public void ApplyDNAPrepass(UMAData umaData, UMASkeleton skeleton, int dnaTypeHash)
		{
			if (_applyDNAPrepassPlugins.Count > 0)
			{
				for (int i = 0; i < _applyDNAPrepassPlugins.Count; i++)
				{
					_applyDNAPrepassPlugins[i].ApplyDNA(umaData, skeleton, dnaTypeHash);
				}
			}
		}

		/// <summary>
		/// Calls ApplyDNA on all this convertersControllers plugins (aka converters) that apply dna at the standard time
		/// </summary>
		/// <param name="umaData">The umaData on the avatar</param>
		/// <param name="skeleton">The avatars skeleton</param>
		/// <param name="dnaTypeHash">The dnaTypeHash that this converters behaviour is using</param>
		public void ApplyDNA(UMAData umaData, UMASkeleton skeleton, int dnaTypeHash)
		{
			for (int i = 0; i < _applyDNAPlugins.Count; i++)
			{
				_applyDNAPlugins[i].ApplyDNA(umaData, skeleton, dnaTypeHash);
			}
		}

		/// <summary>
		/// Gets all the used dna names from all the plugins (aka converters). This can be used to speed up searching the dna for names by string
		/// </summary>
		/// <param name="forceRefresh">Set this to true if you know the dna names used by any of the plugins has been changed at runtime</param>
		/// <returns></returns>
		public List<string> GetUsedDNANames(bool forceRefresh = false)
		{
			if (_usedDNANames.Count == 0 || forceRefresh)
				CompileUsedDNANamesList();

			return _usedDNANames;
		}

		/// <summary>
		/// Gets a plugin from the list of plugins assigned to this converterController by index
		/// </summary>
		public DynamicDNAPlugin GetPlugin(int index)
		{
			if (_plugins.Count > index)
			{
				return _plugins[index];
			}
			return null;
		}

		/// <summary>
		/// Gets a plugin from the list of plugins assigned to this converterController by name
		/// </summary>
		public DynamicDNAPlugin GetPlugin(string name)
		{
			for(int i = 0; i < _plugins.Count; i++)
			{
				if (_plugins[i].name == name)
					return _plugins[i];
			}
			return null;
		}

		/// <summary>
		/// Gets all plugins assigned to this converterController that are of the given type
		/// </summary>
		public List<DynamicDNAPlugin> GetPlugins(System.Type pluginType)
		{
			var pluginsOfType = new List<DynamicDNAPlugin>();
			for (int i = 0; i < _plugins.Count; i++)
			{
				if (pluginType.IsAssignableFrom(_plugins[i].GetType()))
					pluginsOfType.Add(_plugins[i]);
			}
			return pluginsOfType;
		}

		/// <summary>
		/// Creates a plugin of the given type (must descend from DynamicDNAPlugin), adds it to this converterControllers plugins list,  and stores its asset in the given DynamicDNAConverterController asset
		/// </summary>
		/// <param name="pluginType">The type of dna plugin to create (must descend from DynamicDNAPlugin)</param>
		/// <returns>Returns the created plugin</returns>
		//This can happen at runtime but no asset is created or stored, it just exists in memory
		public DynamicDNAPlugin AddPlugin(System.Type pluginType)
		{
			DynamicDNAPlugin plugin = null;
			plugin = CreatePlugin(pluginType, this);
			if (plugin != null)
			{
				_prepared = false;
				_plugins.Add(plugin);
#if UNITY_EDITOR
				EditorUtility.SetDirty(this);
				AssetDatabase.SaveAssets();
#endif
				return plugin;
			}
			return null;
		}

		/// <summary>
		/// Removes the given plugin from this converterController, and deletes its asset (in the Editor)
		/// </summary>
		/// <param name="pluginToDelete"></param>
		/// <returns></returns>
		public bool DeletePlugin(DynamicDNAPlugin pluginToDelete)
		{
			//check if the given plugin is indeed inside this asset
			//if it is DestroyImmediate
			if (_plugins.Contains(pluginToDelete))
			{
				_prepared = false;
				_plugins.Remove(pluginToDelete);
				Debug.Log(pluginToDelete.name + " successfully deleted from " + this.name);
#if UNITY_EDITOR
				DestroyImmediate(pluginToDelete, true);
				EditorUtility.SetDirty(this);
				AssetDatabase.SaveAssets();
#endif
			}
			//then Validate the list
			ValidatePlugins();
			return false;
		}

		/// <summary>
		/// At run time this simply clears the plugins list of any empty entries, or null entries, and assigns itself as the converterController for the plugin
		/// At edit time all instantiated plugins inside the given converterController are checked to see if they belong in this list and if they are they get added
		/// This can happen when a plugin script is deleted but then restored again (like when working on different branches in sourceControl)
		/// </summary>
		public void ValidatePlugins()
		{
			bool changed = false;
			var cleanList = new List<DynamicDNAPlugin>();
			for (int i = 0; i < _plugins.Count; i++)
			{
				if (_plugins[i] != null)
				{
					if (DynamicDNAPlugin.IsValidPlugin(_plugins[i]))
					{
						cleanList.Add(_plugins[i]);
						if (_plugins[i].converterController != this)
						{
							_plugins[i].converterController = this;
#if UNITY_EDITOR
							EditorUtility.SetDirty(_plugins[i]);
							changed = true;
#endif
						}
					}
				}
			}
			_plugins = cleanList;
#if UNITY_EDITOR
			//if we are in the editor get all the assets inside the given converterController asset and check if any of those should be in this list
			var thisAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));
			for (int i = 0; i < thisAssets.Length; i++)
			{
				if (thisAssets[i] == this)
					continue;
				if (!DynamicDNAPlugin.IsValidPlugin(thisAssets[i]))
					continue;
				if (!_plugins.Contains(thisAssets[i] as DynamicDNAPlugin))
				{
					_plugins.Add(thisAssets[i] as DynamicDNAPlugin);
					changed = true;
				}
			}
			if (changed)
			{
				EditorUtility.SetDirty(this);
				AssetDatabase.SaveAssets();
			}
#endif
			CompileUsedDNANamesList();
		}

		/// <summary>
		/// Compiles the used names cache. This can be used to speed up searching the dna for names by string
		/// </summary>
		private void CompileUsedDNANamesList()
		{
			_usedDNANames.Clear();
			for (int i = 0; i < _plugins.Count; i++)
			{
				foreach(KeyValuePair<string, List<int>> kp in _plugins[i].IndexesForDnaNames)
				{
					if (!_usedDNANames.Contains(kp.Key) && !string.IsNullOrEmpty(kp.Key))
						_usedDNANames.Add(kp.Key);
				}
			}
		}

		/// <summary>
		/// Creates a new plugin of the given type and stores it inside the given converterController asset
		/// </summary>
		/// <returns>Returns the created asset</returns>
		private static DynamicDNAPlugin CreatePlugin(System.Type pluginType, DynamicDNAConverterController converter)
		{
			//Checks and warnings
			if (pluginType == null)
			{
				Debug.LogWarning("Could not create plugin because the plugin type was null");
				return null;
			}
			if (converter == null)
			{
				Debug.LogWarning("Could not create plugin because no converterController was provided to add it to");
				return null;
			}
			if (!DynamicDNAPlugin.IsValidPluginType(pluginType))
			{
				Debug.LogWarning("Could not create plugin because it did not descend from DynamicDNAPlugin");
				return null;
			}

			DynamicDNAPlugin asset = ScriptableObject.CreateInstance(pluginType) as DynamicDNAPlugin;
			asset.name = converter.GetUniquePluginName(pluginType.Name.Replace("Plugin","") + "s");
#if UNITY_EDITOR
			Debug.Log(pluginType + " created successfully! Its asset '" + asset.name + "' has been stored in " + converter.name);
			AssetDatabase.AddObjectToAsset(asset, converter);
#endif
			return asset;
		}
		
		/// <summary>
		/// Gets a unique name for a plugin relative to this converterController
		/// </summary>
		/// <param name="desiredName">The name you'd like</param>
		public string GetUniquePluginName(string desiredName, DynamicDNAPlugin existingPlugin = null)
		{
			var intSuffix = 0;
			for (int i = 0; i < _plugins.Count; i++)
			{
				if (_plugins[i].name == desiredName && (existingPlugin == null || (existingPlugin != null && existingPlugin != _plugins[i])))
					intSuffix++;
			}
			return desiredName + (intSuffix != 0 ? intSuffix.ToString() : "");
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem("UMA/Create Dynamic DNA Converter Controller")]
		public static DynamicDNAConverterController CreateDynamicDNAConverterControllerAsset()
		{
			return UMA.CustomAssetUtility.CreateAsset<DynamicDNAConverterController>();
		}
		public static DynamicDNAConverterController CreateDynamicDNAConverterControllerAsset(string newAssetPath, bool selectCreatedAsset = true, string baseName = "New")
		{
			return UMA.CustomAssetUtility.CreateAsset<DynamicDNAConverterController>(newAssetPath, selectCreatedAsset, baseName);
		}
#endif
	}
}
