﻿using OpenAI.ChatGPT.Net.Tools;

namespace OpenAI.ChatGPT.Net.IntegrationTests.Tools
{
    /// <summary>
    /// Class containing tools must be public
    /// </summary>
    public class MyToolClass
    {
        public static int MyField;
        [GPT_Data]
        public static int MyProperty { get; set; }
        
        [GPT_Data]
        public static int MyProperty2 { get; private set; }

        [GPT_Data(PropertyAccess.Getter)]
        public static int MyProperty3 { get; set; }


        public static string Tool1() => "Tool1";
       
        public static string Tool2() => "Tool2";

        /// <summary>
        /// This tool is first added in the example by adding all tools of this class
        /// and then it is removed specifically again.
        /// <example>
        /// <para/>For example:
        /// <code>
        /// Model model = new Model("gpt-4o", "key");
        /// model.AddToolClass&lt;MyToolClass&gt;();
        /// model.RemoveTool(MyToolClass.RemovedTool);
        /// </code>
        /// </example>
        /// See also <see cref="AddTools"/>
        /// </summary>
        public static string RemovedTool()
        {
            return "RemovedTool executed";
        }

        /// <summary>
        /// Methods locked with [<see cref="GPT_Locked"/>] can't be used by GPT even if you attempt to add them.
        /// </summary>
        /// <returns></returns>
        [GPT_Locked]
        public static string LockedMethod() => "LockedMethod executed";


        /// <summary>
        /// Non static Methods can't be used by GPT
        /// </summary>
        public void NonStaticMethod() { }

        /// <summary>
        /// Non public Methods can't be used by GPT
        /// </summary>
        private static void PrivateMethod() { }
    }
}
