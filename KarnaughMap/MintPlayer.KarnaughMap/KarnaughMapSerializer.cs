using System.CodeDom;
using System.ComponentModel.Design.Serialization;

namespace MintPlayer.KarnaughMap;

internal class KarnaughMapSerializer : global::Microsoft.DotNet.DesignTools.Serialization.CodeDomSerializer
{
    public override object Serialize(IDesignerSerializationManager manager, object value)
    {
        var userControlSerializer = manager.GetSerializer(typeof(KarnaughMap).BaseType, typeof(global::Microsoft.DotNet.DesignTools.Serialization.CodeDomSerializer)) as global::Microsoft.DotNet.DesignTools.Serialization.CodeDomSerializer;
        if (userControlSerializer == null)
            MessageBox.Show("userControlSerializer == null");

        var statements = userControlSerializer.Serialize(manager, value) as CodeStatementCollection;
        if (statements == null)
            MessageBox.Show("statements == null");

        var karnaughMap = value as KarnaughMap;
        if (karnaughMap == null)
        {
            MessageBox.Show("karnaughMap == null");
            return null;
        }

        if (statements != null)
        {
            var targetObject = base.GetExpression(manager, value);

            foreach (var input in karnaughMap.InputVariables)
            {
                statements.Add(
                    new CodeMethodInvokeExpression(
                        new CodePropertyReferenceExpression(targetObject, "InputVariables"),
                        "AddRange",
                        new CodeArrayCreateExpression(
                            new CodeTypeReference(typeof(string)),
                            karnaughMap.InputVariables.Select(i => new CodePrimitiveExpression(i)).ToArray()
                        )
                    )
                );
            }

        }
        return statements;
    }
}
