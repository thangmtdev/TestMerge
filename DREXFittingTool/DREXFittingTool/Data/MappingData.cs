using System.Collections.Generic;

namespace DREXFittingTool.Data
{
    public class MappingData
    {
        public Dictionary<string, string> m_dictParameter;
        public KeyData m_centerWidh;
        public KeyData m_centerHeight;
        public KeyData m_webThickness;
        public KeyData m_FThickness;
        public KeyData m_FMaterial;

        public MappingData(KeyData centerWidth, KeyData centerHeight, KeyData webThickness, KeyData FThickness, KeyData FMaterial)
        {
            m_dictParameter = new Dictionary<string, string>();
            m_centerWidh = centerWidth;
            m_centerHeight = centerHeight;
            m_webThickness = webThickness;
            m_FThickness = FThickness;
            m_FMaterial = FMaterial;
        }
    }

    public class KeyData
    {
        public string m_name;
        public string m_value;

        public KeyData(string name, string value)
        {
            m_name = name;
            m_value = value;
        }
    }
}