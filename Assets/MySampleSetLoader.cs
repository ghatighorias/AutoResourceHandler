using UnityEngine;
using System.Collections.Generic;

public static partial class MySampleSetLoader
{
	public static Dictionary<MySampleSetItems,string> MySampleSet_Value_Mapper = new Dictionary<MySampleSetItems,string>()
	{
		{MySampleSetItems.PREFAB_PREFAB2, "Assets/Resources/samplePrefab.prefab"},
		{MySampleSetItems.PREFAB_SAMPLEPREFAB, "Assets/Resources/Prefab2.prefab"},
	};
	
	public static GameObject Load(MySampleSetItems resource)
	{
		return Load<GameObject>(resource);
	}
	public static T Load<T>(MySampleSetItems resource)
	{
		return PrefabAutoResource.AutoLoad<T>(MySampleSet_Value_Mapper[resource]);
	}
	
}

public enum MySampleSetItems
{
	PREFAB_PREFAB2,
	PREFAB_SAMPLEPREFAB,
}

