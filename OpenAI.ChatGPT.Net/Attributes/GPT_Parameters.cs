﻿namespace OpenAI.ChatGPT.Net.Tools
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class GPT_Parameters(params string[] descriptionsForAPI) : Attribute
    {
        public string[] DescriptionsForAPI { get; } = descriptionsForAPI;
    }
}
