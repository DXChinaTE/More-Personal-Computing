using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;

namespace SimpleHello
{
    /// <summary>
    /// this class is read resource helper
    /// </summary>
    class ResourceManagerHelper
    {
        public static string ReadValue(string key)
        {
            ResourceContext defaultContextForCurrentView = ResourceContext.GetForCurrentView();
            ResourceMap stringResourcesResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");
            return stringResourcesResourceMap.GetValue(key, defaultContextForCurrentView).ValueAsString;
        }
    }
}
