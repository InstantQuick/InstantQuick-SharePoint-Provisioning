using System.Collections.Generic;

namespace IQAppRuntimeResources
{
    public class ScriptBuilder
    {
        public static string Build(DataPageResource resources, string resourceServerUrl, string resourceFiletext,
            string clientId, string version)
        {
            //A Page Resource can contain a simple script
            if (resources.Resources.Count == 0)
                return resourceFiletext;

            var output = string.Empty;

            var scripts = new List<PageResource>();
            var styleSheets = new List<PageResource>();

            foreach (var resource in resources.Resources)
            {
                if (resource.ResourceType == ResourceTypes.Script) scripts.Add(resource);
                else styleSheets.Add(resource);
            }

            if (styleSheets.Count > 0)
            {
                foreach (var styleSheet in styleSheets)
                {
                    //The next line is a bug waiting to happen: TODO
                    styleSheet.FullUrl = !styleSheet.ExternalResource
                        ? $@"//{resourceServerUrl}?key={styleSheet.Url}&c={clientId}&v={version}"
                        : styleSheet.Url;
                    output = output + $"dLink('{styleSheet.FullUrl}');";
                }
            }

            if (scripts.Count > 0)
            {
                output = output + "$LAB";
                for (var i = 0; i < scripts.Count; i++)
                {
                    string append;
                    var script = scripts[i];
                    //The next line is a bug waiting to happen: TODO
                    script.FullUrl = !script.ExternalResource
                        ? $@"//{resourceServerUrl}?key={script.Url}&c={clientId}&v={version}"
                        : script.Url;

                    if (i != scripts.Count - 1)
                    {
                        append = script.Wait ? $".script('{script.FullUrl}').wait()" : $".script('{script.FullUrl}')";
                    }
                    else
                    {
                        append = resources.PageInitializationScript
                            ? $".script('{script.FullUrl}').wait(function(){{{resourceFiletext}}});"
                            : $".script('{script.FullUrl}');";
                    }
                    output = output + append;
                }
            }
            else if (resources.PageInitializationScript)
            {
                output = output + resourceFiletext;
            }

            if (resources.PageScript) output = output + resourceFiletext;

            return output;
        }
    }
}