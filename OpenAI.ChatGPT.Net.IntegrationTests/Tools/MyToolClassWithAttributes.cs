using OpenAI.ChatGPT.Net.Tools;

namespace OpenAI.ChatGPT.Net.IntegrationTests.Tools
{
    [GPT_Locked]
    public class MyToolClassWithAttributes
    {
        /// <summary>
        /// This Method will not be added by the 
        /// <code>.AddToolClass&lt;<see cref="MyToolClassWithAttributes"/>&gt;();</code>
        /// because this class has the [<see cref="GPT_Locked"/>] Attribute which lockes all methods per default.
        /// </summary>
        public static string MethodWithoutAttribute() => "This shoul not be included in the tools";

        /// <summary>
        /// [<see cref="GPT_Tool"/>] Provides a Description for GPT of this method
        /// <code>
        /// [GPTTool("This is the Tool1")]
        /// public static string Tool1() => "Tool1";
        /// </code>
        /// </summary>
        [GPT_Tool]
        [GPT_Description("This is the Tool1")]
        public static string Tool1() => "Tool1";

        /// <summary>
        /// [<see cref="GPT_Parameters"/>] Provides a Description of the parameters for GPT of this method
        /// Note that this code:
        /// <code>[GPTParameters(parameter1: "This is a number", parameter2: "This is a useless parameter description that will be ignored")]
        /// public static string Tool2(int size) => "Tool2";
        /// </code>
        /// will not produce an error even tough a parameter is decribed, that isn't there.
        /// The description of that parameter will just be ignored.
        /// </summary>
        [GPT_Tool]
        [GPT_Parameters("This is a number", "This is a useless parameter description that will be ignored")]
        public static string Tool2(int size) => "Tool2";

        /// <summary>
        /// Full example with [<see cref="GPT_Tool"/>] and [<see cref="GPT_Parameters"/>]
        /// <code>
        /// [GPTTool("This is the Tool3")]
        /// [GPTParameters(parameter1: "This is a number")]
        /// public static string Tool3(int number) => "Tool3";
        /// </code>
        /// </summary>
        [GPT_Tool]
        [GPT_Description("This is the Tool3")]
        [GPT_Parameters("This is a number")]
        public static string Tool3(int number) => "Tool3";

        /// <summary>
        /// This Method will not be added by the 
        /// <code>.AddToolClass&lt;<see cref="MyToolClassWithAttributes"/>&gt;();</code>
        /// because this class has the [<see cref="GPT_Locked"/>] Attribute which lockes all methods per default.
        /// </summary>
        public void NonStaticMethod() { }

        /// <summary>
        /// Non public Methods can't be used by GPT and can't have GPT Attribute
        /// </summary>
        private static void PrivateMethod1() { }        
        
        /// <summary>
        /// Non public Methods can't be used by GPT and can't have GPT Attribute
        /// </summary>
        private void PrivateMethod2() { }
    }
}
