using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VeryFS.Framework.Editor.Resource;
using VeryFS.Framework.Runtime.Resource;
using Undo = UnityEditor.Undo;

namespace VeryFS.Framework.Editor.Resource
{

public class ResourcePackageEditor : EditorWindow
{
	
	[UnityEditor.Callbacks.OnOpenAsset]
	public static bool OnOpenAsset(int instanceID, int lineNumber)
	{
		var path = AssetDatabase.GetAssetPath(instanceID);
		var guid = AssetDatabase.AssetPathToGUID(path);
		var extension = Path.GetExtension(path).ToLower();
		if (extension ==  ".asset")
		{
			var type= AssetDatabase.GetMainAssetTypeAtPath(path);
			//Debug.LogError(type.Name);
			if (type != typeof(ResourcePackageBuildSettings))
				return false;

			ResourcePackageEditor window = null;
			foreach (var w in Resources.FindObjectsOfTypeAll<ResourcePackageEditor>())
			{
				// if (w.selectedGuid == guid)
				// {
				// 	w.Focus();
				// 	return true;
				// }
				window = w;
				break;
			}
			if (window==null)
				window = EditorWindow.CreateWindow<ResourcePackageEditor>("Resource Package Editor",typeof(ResourcePackageEditor),typeof(SceneView));
			//window.saveChangesMessage = "This window has unsaved changes. Would you like to save?";
			window.Initialize(guid);
			window.Focus();
			return true;
		}

		return false;
	}
	
	
	[SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

	private ResourcePackageBuildSettings mData;

	// private string mWindowTitle;
	
	// [SerializeField]
	// string m_SelectedGUID;
	
	// public string selectedGuid
	// {
	// 	get => m_Selected;
	// 	private set => m_Selected = value;
	// }

	// [SerializeField] public string mAssetPath;

	// public void Initialize(ResourcePackageBuildSettings asset)
	// {
	// 	mData = asset;
	// }
	// private List<string> mAssetPathList = new();


	// private void FindAllSettingAssets<TSetting>() where TSetting : ScriptableObject
	// {
	// 	var settingType = typeof(TSetting);
	// 	var guids = AssetDatabase.FindAssets($"t:{settingType.Name}");
	// 	if (guids.Length == 0)
	// 	{
	// 		Debug.LogWarning($"Create new {settingType.Name}.asset");
	// 		CreateSettingData<TSetting>("PackageBuild_Default");
	//
	// 		guids = AssetDatabase.FindAssets($"t:{settingType.Name}");
	// 	}
	//
	// 	//string oldPath = _assetsDropdown.value;
	// 	_assetsDropdown.choices.Clear();
	// 	foreach (var guid in guids)
	// 	{
	// 		string path = AssetDatabase.GUIDToAssetPath(guid);
	// 		_assetsDropdown.choices.Add(path);
	// 	}
	// }

	public override void SaveChanges()
	{
		// Your custom save procedures here

		Debug.Log($"{this} saved successfully!!!");
		base.SaveChanges();
	}

	public override void DiscardChanges()
	{
		// Your custom procedures to discard changes

		Debug.Log($"{this} discarded changes!!!");
		base.DiscardChanges();
	}
	
	// [MenuItem("VeryFS/设置资源包", false, 2)]
	// private static void OpenResourcePackageEditor()
	// {
	// 	var window = EditorWindow.GetWindow<ResourcePackageEditor>("资源包设置",true);
	// 	window.minSize = new Vector2(800, 600);
	// }
	
	
	
	// private static void CreateSettingData<TSetting>(string fileName) where TSetting : ScriptableObject
	// {
	// 	var setting = ScriptableObject.CreateInstance<TSetting>();
	// 	string filePath = $"Assets/Settings/{fileName}.asset";
	// 	AssetDatabase.CreateAsset(setting, filePath);
	// 	AssetDatabase.SaveAssets();
	// 	AssetDatabase.Refresh();
	// }
	
	private void RefreshWindow()
	{
		FillMain();
		FillGroup();
	}

	private void FillMain()
	{
		
		// string windowTitle = (mData != null ? mData.name : "(nothing loaded)");
		// this.titleContent.text = windowTitle;
		
		//Debug.LogError("FillMain " + windowTitle);
		//Debug.LogError("Fill "+mAssetPath);

		_currentAsset.SetValueWithoutNotify(mData != null ? mData : null);

		_packageName.SetValueWithoutNotify ( mData != null ? mData.PackageName : "");
		_bundleNameStyle.SetValueWithoutNotify( mData != null ? mData.BundleNameStyle : AssetBundleNameStyle.BundleName);

		if (mData != null)
		{
			_groupListView.itemsSource = mData.Groups;
			
			if (mData.Groups.Count > 0)
			{
				_groupListView.RefreshItems();
				_groupListView.SetSelectionWithoutNotify(new int[]{0});
			}
			else
			{
				_groupListView.Clear();
			}
		}
		else
		{
			_groupListView.itemsSource = null;
			_groupListView.Clear();
		}
		
		
	}
	
	private AssetBundleCollectorGroup SelectGroup => mData == null ? null : _groupListView.selectedItem as AssetBundleCollectorGroup;
	

	private void FillGroup()
	{
		

		AssetBundleCollectorGroup group = SelectGroup;

		//Debug.LogError("FillGroup " + (group != null ? group.GroupName : "(NULL)"));
		_collectorScrollView.Clear();
		
		if (group == null)
		{
			_groupName.SetValueWithoutNotify("");
			_groupDesc.SetValueWithoutNotify( "");
			_groupEnabled.SetValueWithoutNotify(true);
		}
		else
		{
			_groupName.SetValueWithoutNotify( group.GroupName);
			_groupDesc.SetValueWithoutNotify( group.GroupDesc);
			_groupEnabled.SetValueWithoutNotify( group.GroupEnabled);

			for (int i = 0; i < group.Collectors.Count; i++)
			{
				_collectorScrollView.Add(MakeCollectorListViewItem(i));
			}
			
		}
	}

	// private void LoadAsset(string path)
	// {
	// 	mData = AssetDatabase.LoadAssetAtPath<ResourcePackageBuildSettings>(path);
	// 	Debug.Log($"Load {mData.PackageName}, {path}");
	// }

	public void Initialize(string assetGuid)
	{
		// if (m_SelectedGUID == assetGuid && mData!=null)
		// 	return;

		var path = AssetDatabase.GUIDToAssetPath(assetGuid);

		var asset = AssetDatabase.LoadAssetAtPath<ResourcePackageBuildSettings>(path);
		if (asset == null || !EditorUtility.IsPersistent(asset))
			return;

		// m_SelectedGUID = assetGuid;
		//m_Selected = assetGuid;
		mData = asset;
	
		RefreshWindow();
	}


	

	public void OnDestroy()
	{
		// 注意：清空所有撤销操作
		//Undo.ClearAll();

		if (mData != null)
		{
			AssetDatabase.SaveAssets();
		}
	}

	// private DropdownField _assetsDropdown;
	private EnumField _bundleNameStyle;
	private TextField _packageName;
	private ListView _groupListView;
	private ObjectField _currentAsset;

	private TextField _groupName;
	private TextField _groupDesc;
	private Toggle _groupEnabled;

	private ScrollView _collectorScrollView;

	
	// private static  void DeleteEmptyDirectory(string outputPath)
	// {
	// 	if (Directory.Exists(outputPath) == false)
	// 		return;
	// 	string[] directories = Directory.GetDirectories(outputPath);
	// 	if (directories.Length > 0)
	// 	{
	// 		foreach (var subDir in directories)
	// 		{
	// 			DeleteEmptyDirectory(subDir);
	// 		}
	// 	}
	// 	else
	// 	{
	// 		string[] files = Directory.GetFiles(outputPath);
	// 		foreach (var file in files)
	// 		{
	// 			if (file.EndsWith(".DS_Store"))
	// 			{
	// 				File.Delete(file);
	// 			}
	// 		}
	// 		files = Directory.GetFiles(outputPath);
	// 		if (files.Length == 0)
	// 		{
	// 			Directory.Delete(outputPath, false);
	// 			Debug.Log("Delete Dir: " + outputPath);
	// 		}
	// 	}
	// }

	

	public void CreateGUI()
	{
		Undo.undoRedoPerformed -= RefreshWindow;
		Undo.undoRedoPerformed += RefreshWindow;
		
		// Each editor window contains a root VisualElement object
		VisualElement root = rootVisualElement;
		// Instantiate UXML
		// VisualElement tree = m_VisualTreeAsset.Instantiate();
		// root.Add(tree);
		m_VisualTreeAsset.CloneTree(root);

		var btnRefresh = root.Q<Button>("RefreshButton");
		btnRefresh.clicked += RefreshWindow;


		// var btnBuild = root.Q<Button>("BuildButton");
		// btnBuild.clicked += () => DoBuild(mData);

		_currentAsset = root.Q<ObjectField>("CurrentAsset");
		_currentAsset.objectType = typeof(ResourcePackageBuildSettings);
		//_currentAsset.SetEnabled(false);
		_currentAsset.RegisterValueChangedCallback(evt =>
		{
			
			string path = AssetDatabase.GetAssetPath(evt.newValue);
			string guid = AssetDatabase.AssetPathToGUID(path);
			if (!string.IsNullOrEmpty(guid))
			{
				if (evt.previousValue != null)
				{
					AssetDatabase.SaveAssets();
					Undo.ClearUndo(evt.previousValue);
				}
				
				Initialize(guid);
			}
		});

		_packageName = root.Q<TextField>("PackageName");
		_packageName.RegisterCallback<FocusOutEvent>(evt =>
		{
			if (mData == null)
				return;
			var textField = evt.target as TextField;

			string text = textField.value.Trim();

			if (string.IsNullOrEmpty(text))
			{
				textField.value = mData.PackageName;
			}
			else if (text != mData.PackageName)
			{
				Undo.RecordObject(mData,"Change PackageName");
				EditorUtility.SetDirty(mData);
				mData.PackageName = text;
			}
		});
		
		
		_bundleNameStyle = root.Q<EnumField>("BundleNameStyle");
		_bundleNameStyle.Init(AssetBundleNameStyle.BundleName_HashName);
		_bundleNameStyle.RegisterValueChangedCallback(evt =>
		{
			var ctrl = evt.target as EnumField;
			if (mData != null && mData.BundleNameStyle != (AssetBundleNameStyle)ctrl.value)
			{
				Undo.RecordObject(mData,"Change BundleNameStyle");
				EditorUtility.SetDirty(mData);
				mData.BundleNameStyle = (AssetBundleNameStyle)ctrl.value;
			}
		});
		
		//bundleNameStyle.value = AssetBundleNameStyle.BundleName;
		
//		root.Add(tree);

		var collectorContainer = root.Q<VisualElement>("CollectorContainer");
		
		_groupName = collectorContainer.Q<TextField>("GroupName");
		_groupName.RegisterCallback<FocusOutEvent>(evt =>
		{
			var group = SelectGroup;
			if (group == null)
				return;
			
			var textField = evt.target as TextField;
			string text = textField.value.Trim();

			if (string.IsNullOrEmpty(text))
			{
				textField.value = group.GroupName;
			}
			else if (text != mData.PackageName)
			{
				Undo.RecordObject(mData,"Change GroupName");
				EditorUtility.SetDirty(mData);
				group.GroupName = text;
				_groupListView.RefreshItem(_groupListView.selectedIndex);
			}
		});
		
		_groupDesc = collectorContainer.Q<TextField>("GroupDesc");
		_groupDesc.RegisterCallback<FocusOutEvent>(evt =>
		{
			var group = SelectGroup;
			if (group == null)
				return;
			
			var textField = evt.target as TextField;
			string text = textField.value.Trim();

			if (text != mData.PackageName)
			{
				Undo.RecordObject(mData,"Change GroupDesc");
				EditorUtility.SetDirty(mData);
				group.GroupDesc = text;
				_groupListView.RefreshItem(_groupListView.selectedIndex);
			}
		});
		
		
		_groupEnabled = collectorContainer.Q<Toggle>("GroupEnabled");
		_groupEnabled.RegisterValueChangedCallback(evt =>
		{
			var group = SelectGroup;
			if (group == null)
				return;
			var ctrl = evt.target as Toggle;
			if (ctrl.value != group.GroupEnabled)
			{
				Undo.RecordObject(mData,"Change GroupEnabled");
				EditorUtility.SetDirty(mData);
				group.GroupEnabled = ctrl.value;
			}
		});
		
		
		var addCollectorBtn = collectorContainer.Q<Button>("AddBtn");
		addCollectorBtn.clicked += () =>
		{
			var group = SelectGroup;
			if (group == null)
				return;
			group.Collectors.Add(new AssetBundleCollector());
			FillGroup();
		};
		// var removeBtn = collectorContainer.Q<Button>("RemoveBtn");

		_collectorScrollView = root.Q<ScrollView>("CollectorScrollView");
		_collectorScrollView.style.height = new Length(100, LengthUnit.Percent);
		_collectorScrollView.viewDataKey = "scrollView";
		
		_groupListView = root.Q<ListView>("GroupListView");
		_groupListView.makeItem = () =>
		{
			VisualElement element = new VisualElement();

			{
				var label = new Label();
				label.name = "Label1";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.flexGrow = 1f;
				label.style.height = 20f;
				label.text = "Group";
				element.Add(label);
			}
			return element;
		};
		
		_groupListView.bindItem = (element, i) =>
		{
			var textField1 = element.Q<Label>("Label1");

			if (_groupListView.itemsSource[i] is AssetBundleCollectorGroup group)
			{
				if (!string.IsNullOrEmpty(group.GroupDesc))
				{
					textField1.text = $"{group.GroupName} ({group.GroupDesc})";
				}
				else
				{
					textField1.text = group.GroupName;
				}
			}
			else
			{
				textField1.text = "INVALID";
			}
		};
		

		// _groupListView.selectionChanged += Debug.Log;
		
		var groupAddContainer = root.Q("GroupAddContainer");
		{
			var addBtn = groupAddContainer.Q<Button>("AddBtn");
			addBtn.clicked += () =>
			{
				Undo.RecordObject(mData,"Add group in "+mData.name);
				EditorUtility.SetDirty(mData);
				mData.NewGroup();
				RefreshWindow();
				// Debug.LogError(string.Join(',',ResourcePackageBuildSettings.FilterRuleHelper.RuleNames));
				// Debug.LogError(string.Join(',',ResourcePackageBuildSettings.AddressRuleHelper.RuleNames));
				// Debug.LogError(string.Join(',',ResourcePackageBuildSettings.PackRuleHelper.RuleNames));

			};
			var removeBtn = groupAddContainer.Q<Button>("RemoveBtn");
			removeBtn.clicked += () =>
			{
				if (_groupListView.selectedItem is AssetBundleCollectorGroup group)
				{
					Undo.RecordObject(mData,"Remove group in "+mData.name);
					EditorUtility.SetDirty(mData);
					_groupListView.itemsSource.RemoveAt(_groupListView.selectedIndex);
					//_groupListView.RemoveAt(_groupListView.selectedIndex);
					RefreshWindow();
				}
			};
		}
		
#if UNITY_2022_1_OR_NEWER
		_groupListView.selectionChanged += objects =>
#else
		_groupListView.onSelectionChange += objects =>
#endif
		{
			//Debug.LogError("Group selection changed");
			FillGroup();
		};
		
		// FindAllSettingAssets<ResourcePackageBuildSettings>();
		// _assetsDropdown.value = _assetsDropdown.choices[0];
		if (mData!=null)
		{
			//Debug.LogError("Create "  + m_SelectedGUID); 
			RefreshWindow();
		}
	}

	private AssetBundleCollector GetConnector(int collectorIdx)
	{
		var group = SelectGroup;
		if (group == null)
			return null;
		var collector = collectorIdx < group.Collectors.Count ? group.Collectors[ collectorIdx] : null;
		return collector;
	}

	private static string FormatPopupItem(RuleDisplayItem item)
	{
		return item!=null ? item.caption : "";
	}
	
	private VisualElement MakeCollectorListViewItem(int collectorIdx)
	{
		
		VisualElement element = new VisualElement();

		VisualElement elementTop = new VisualElement();
		elementTop.style.flexDirection = FlexDirection.Row;
		element.Add(elementTop);

		VisualElement elementBottom = new VisualElement();
		elementBottom.style.flexDirection = FlexDirection.Row;
		element.Add(elementBottom);

		VisualElement elementFoldout = new VisualElement();
		elementFoldout.style.flexDirection = FlexDirection.Row;
		element.Add(elementFoldout);

		VisualElement elementSpace = new VisualElement();
		elementSpace.style.flexDirection = FlexDirection.Column;
		element.Add(elementSpace);

		// Foldout VisualElement
		{
			var label = new Label();
			label.style.width = 90;
			elementFoldout.Add(label);
		}
		
		var foldout = new Foldout();
		{
			foldout.name = "Foldout1";
			foldout.value = false;
			foldout.text = "Assets";
			foldout.RegisterValueChangedCallback(evt =>
			{
				if (evt.newValue)
					FillCollectedAssets(foldout,  collectorIdx);
				else
					foldout.Clear();
			});
			elementFoldout.Add(foldout);
		}
		
		// Top VisualElement
		{
			var button = new Button();
			button.name = "Button1";
			button.text = "-";
			button.style.unityTextAlign = TextAnchor.MiddleCenter;
			button.style.flexGrow = 0f;
			elementTop.Add(button);
			button.clicked += () =>
			{
				var group = SelectGroup;
				if (group != null)
				{
					var collector = GetConnector(collectorIdx);;
					if (collector != null)
					{
						Undo.RecordObject(mData, "Remove Collector");
						EditorUtility.SetDirty(mData);
						group.Collectors.Remove(collector);
						FillGroup();
					}
				}
			};
		}
		var collector = GetConnector( collectorIdx);
		
		//Collect目录
		{
			var objectField = new ObjectField();
			objectField.name = "ObjectField1";
			objectField.label = "Collector";
			// objectField.objectType = typeof(UnityEngine.Object);
			objectField.objectType =typeof(UnityEditor.DefaultAsset);
			objectField.allowSceneObjects = false;
			
			objectField.style.unityTextAlign = TextAnchor.MiddleLeft;
			objectField.style.flexGrow = 1f;
			elementTop.Add(objectField);
			var label = objectField.Q<Label>();
			label.style.minWidth = 63;
			
			var collectObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(collector.Path);
			objectField.SetValueWithoutNotify(collectObject);
			
			objectField.RegisterValueChangedCallback(evt =>
			{
				var collector = GetConnector( collectorIdx);
				if (collector == null)
					return;
				
				string newPath = AssetDatabase.GetAssetPath(evt.newValue);
				if (newPath != collector.Path && Directory.Exists(newPath) )
				{
					Undo.RecordObject(mData,"Change Collector Path");
					EditorUtility.SetDirty(mData);
					collector.Path = newPath;
					//objectField.value.name = collector.Path;
					foldout.value = false;
					foldout.Clear();
				}
				else
				{
					objectField.SetValueWithoutNotify(evt.previousValue);
				}
			});
			
		}

		// Bottom VisualElement
		{
			var label = new Label();
			label.style.width = 90;
			elementBottom.Add(label);
		}
		// {
		// 	var popupField = new PopupField<string>(_collectorTypeList, 0);
		// 	popupField.name = "PopupField0";
		// 	popupField.style.unityTextAlign = TextAnchor.MiddleLeft;
		// 	popupField.style.width = 150;
		// 	elementBottom.Add(popupField);
		// }
		
		//AddressRule
		
		var addressRuleField = new PopupField<RuleDisplayItem>(
			ResourcePackageBuildSettings.AddressRuleHelper.DisplayItems, 0);
		addressRuleField.name = "PopupField1";
		addressRuleField.style.unityTextAlign = TextAnchor.MiddleLeft;
		addressRuleField.style.width = 220;
		addressRuleField.formatListItemCallback = FormatPopupItem;
		addressRuleField.formatSelectedValueCallback = FormatPopupItem;
		addressRuleField.SetValueWithoutNotify(
			ResourcePackageBuildSettings.AddressRuleHelper.GetDisplayItem(collector.AddressRuleName));
		elementBottom.Add(addressRuleField);
		
		addressRuleField.RegisterValueChangedCallback(evt =>
		{
			var collector = GetConnector( collectorIdx);
			if (collector == null)
				return;
			if (collector.AddressRuleName != evt.newValue.name)
			{
				Undo.RecordObject(mData,"Change AddressRuleName");
				EditorUtility.SetDirty(mData);
				collector.AddressRuleName = evt.newValue.name;
				foldout.value = false;
				foldout.Clear();
			}
		});
		
		//FilterRuleName
		var filterRuleField = new PopupField<RuleDisplayItem>(
			ResourcePackageBuildSettings.FilterRuleHelper.DisplayItems, 0);
		filterRuleField.name = "PopupField3";
		filterRuleField.style.unityTextAlign = TextAnchor.MiddleLeft;
		filterRuleField.style.width = 150;
		filterRuleField.formatListItemCallback = FormatPopupItem;
		filterRuleField.formatSelectedValueCallback = FormatPopupItem;
			
			
		filterRuleField.SetValueWithoutNotify(
			ResourcePackageBuildSettings.FilterRuleHelper.GetDisplayItem(collector.FilterRuleName));
			
		filterRuleField.RegisterValueChangedCallback(evt =>
		{
			var collector = GetConnector( collectorIdx);
			if (collector == null)
				return;
			if (collector.FilterRuleName != evt.newValue.name)
			{
				Undo.RecordObject(mData,"Change FilterRuleName");
				EditorUtility.SetDirty(mData);
				collector.FilterRuleName = evt.newValue.name;
				foldout.value = false;
				foldout.Clear();
			}
		});
		
		//PackRule
		{
			var popupField = new PopupField<RuleDisplayItem>(
				ResourcePackageBuildSettings.PackRuleHelper.DisplayItems, 0);
			popupField.name = "PopupField2";
			popupField.style.unityTextAlign = TextAnchor.MiddleLeft;
			popupField.style.width = 220;
			popupField.formatListItemCallback = FormatPopupItem;
			popupField.formatSelectedValueCallback = FormatPopupItem;
			popupField.SetValueWithoutNotify(
				ResourcePackageBuildSettings.PackRuleHelper.GetDisplayItem(collector.PackRuleName));
			elementBottom.Add(popupField);
			popupField.RegisterValueChangedCallback(evt =>
			{
				var collector = GetConnector(collectorIdx);
				if (collector == null)
					return;
				if (collector.PackRuleName != evt.newValue.name)
				{
					Undo.RecordObject(mData, "Change PackRuleName");
					EditorUtility.SetDirty(mData);
					collector.PackRuleName = evt.newValue.name;
					foldout.value = false;
					foldout.Clear();
				}
				var packRule = ResourcePackageBuildSettings.PackRuleHelper.GetRuleInstance(collector.PackRuleName);
				addressRuleField.SetEnabled(!packRule.DisableFileFilterRule);
				
				filterRuleField.SetEnabled(!packRule.DisableFileFilterRule);
				
			});
		}
		
		var packRule = ResourcePackageBuildSettings.PackRuleHelper.GetRuleInstance(collector.PackRuleName);

		addressRuleField.SetEnabled(!packRule.DisableFileFilterRule);
		filterRuleField.SetEnabled(!packRule.DisableFileFilterRule);
		
		// filterRuleField.SetEnabled(!collector.PackRuleName.StartsWith("PackFairyGUI"));
		// addressRuleField.SetEnabled(!collector.PackRuleName.StartsWith("PackFairyGUI"));
		
		elementBottom.Add(filterRuleField);
		// {
		// 	var textField = new TextField();
		// 	textField.name = "TextField0";
		// 	textField.label = "User Data";
		// 	textField.style.width = 200;
		// 	elementBottom.Add(textField);
		// 	var label = textField.Q<Label>();
		// 	label.style.minWidth = 63;
		// }
		// {
		// 	var textField = new TextField();
		// 	textField.name = "TextField1";
		// 	textField.label = "Asset Tags";
		// 	textField.style.width = 100;
		// 	textField.style.marginLeft = 20;
		// 	textField.style.flexGrow = 1;
		// 	elementBottom.Add(textField);
		// 	var label = textField.Q<Label>();
		// 	label.style.minWidth = 40;
		// }

		

		// Space VisualElement
		{
			var label = new Label();
			label.style.height = 10;
			elementSpace.Add(label);
		}

		return element;
	}


	/// <summary>
	/// 填充Assets列表
	/// </summary>
	/// <param name="foldout"></param>
	/// <param name="collectorIdx"></param>
	private void FillCollectedAssets(Foldout foldout, int collectorIdx)
	{
		foldout.Clear();
		if (!foldout.value)
			return;
		
		var group = SelectGroup;
		var collector = GetConnector(collectorIdx);
		if (collector == null)
			return;

		List<CollectAssetInfo> assets = new();
			
		collector.CollectAssets(assets,group.GroupName);

		foreach (var assetInfo in assets)
		{
			VisualElement elementRow = new VisualElement();
			elementRow.style.flexDirection = FlexDirection.Row;
			elementRow.style.flexGrow = 1;
			foldout.Add(elementRow);

			{
				string showInfo = !string.IsNullOrEmpty(assetInfo.Address)
					? $"[{assetInfo.Address}] {assetInfo.AssetPath}"
					: assetInfo.AssetPath;
				var label = new Label();
				label.text = showInfo;
				label.style.minWidth = 350;
				label.style.marginLeft = 0;
				label.style.flexGrow = 0;
				elementRow.Add(label);
			}
			{
				var label = new Label();
				label.text = assetInfo.BundleName;
				label.style.minWidth = 100;
				label.style.marginRight = 0;
				label.style.marginLeft = 20;
				label.style.flexGrow = 1;
				label.style.unityTextAlign = TextAnchor.MiddleRight;
				elementRow.Add(label);
			}
		}
	}
}


// _currentAsset.RegisterValueChangedCallback(evt =>
// {
// 	
// 	var assetPath = AssetDatabase.GUIDToAssetPath(selectedGuid);
// 	if (!File.Exists(assetPath))
// 	{
// 		this.titleContent.text = mData.name + " (deleted)";
// 	}
// 	
// 	//evt.newValue
// 	
// 	// Debug.LogError("Choose " + mData.name);
// 	
// 	// if (_currentAsset.value != null)
// 	// {
// 	// 	//Undo.RecordObject(this, "Choose " + _currentAsset.value.name);
// 	// 	mAssetPath = AssetDatabase.GetAssetPath(_currentAsset.value);
// 	// }
// 	// else
// 	// {
// 	// 	//Undo.RecordObject(this.mAssetPath, "Choose None");
// 	// 	mAssetPath = null;
// 	// }
// 	//
// 	// FillData();
// });
		
// _assetsDropdown = root.Q<DropdownField>("AssetDropdown");
// _assetsDropdown.formatListItemCallback = Path.GetFileName;
// _assetsDropdown.RegisterCallback<ChangeEvent<string>>(evt =>
// {
// 	if (!string.IsNullOrEmpty(evt.newValue))
// 	{
// 		if (mData != null)
// 		{
// 			Debug.LogError(mData.name +","+ evt.newValue);
// 			Undo.SetCurrentGroupName(evt.newValue);
// 			Undo.RecordObject(this.mData, "111Choose " + _assetsDropdown.value);
// 		}
// 			
// 		LoadAsset(_assetsDropdown.value);
// 		FillData();
// 	}
// });
}
