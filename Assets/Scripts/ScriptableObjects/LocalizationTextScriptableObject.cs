using UnityEngine;
using System.Collections.Generic;
using curry.Text;

namespace curry.Scriptable
{
    [CreateAssetMenu(fileName = "LocalizationTextData", menuName = "Scriptable Objects/LocalizationTextData")]
    public class LocalizationTextDataScriptableObject : ScriptableObject
    {
        [SerializeField]
        private List<LocalizationTextData> m_DataList;
        public List<LocalizationTextData> DataList { get { return m_DataList; } set { m_DataList = value; } }
    }
}