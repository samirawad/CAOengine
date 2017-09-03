using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConditionFramework
{
    public static class JsonExtensions
    {
        public static JToken ReplacePath<T>(this JToken root, string path, T newValue)
        {
            if (root == null || path == null)
                throw new ArgumentNullException();

            foreach (var value in root.SelectTokens(path).ToList())
            {
                if (value == root)
                    root = JToken.FromObject(newValue);
                else
                    value.Replace(JToken.FromObject(newValue));
            }
            return root;
        }

        public static JToken RemoveFields(this JToken token, string[] fields)
        {
            JContainer container = token as JContainer;
            if (container == null) return token;

            List<JToken> removeList = new List<JToken>();
            foreach (JToken el in container.Children())
            {
                JProperty p = el as JProperty;
                if (p != null && fields.Contains(p.Name))
                {
                    removeList.Add(el);
                }
                el.RemoveFields(fields);
            }

            foreach (JToken el in removeList)
            {
                el.Remove();
            }

            return token;
        }
    }
}
