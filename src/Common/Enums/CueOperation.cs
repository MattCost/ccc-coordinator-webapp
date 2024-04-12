using System.Reflection;
using System.Text.Json.Serialization;

namespace CCC.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CueOperation
{
    [CueOperationPrintAttribute(" ")]
    Default = 0,
    [CueOperationPrintAttribute("ɑ")]
    StartRoute = 1,

    [CueOperationPrintAttribute("L")]
    Left = 2,

    [CueOperationPrintAttribute("R")]
    Right = 3,

    [CueOperationPrintAttribute("x")]
    Cross = 4,

    [CueOperationPrintAttribute("s")]
    Straight = 5,

    [CueOperationPrintAttribute("L@")]
    LeftAt = 6,

    [CueOperationPrintAttribute("R@")]
    RightAt = 7,

    [CueOperationPrintAttribute("l")]
    SlightLeft = 8,

    [CueOperationPrintAttribute("r")]
    SlightRight = 9,

    [CueOperationPrintAttribute("(L)")]
    LeftCircle = 10,

    [CueOperationPrintAttribute("(R)")]
    RightCircle = 11,

    [CueOperationPrintAttribute("(S)")]
    StraightCircle = 12,
    [CueOperationPrintAttribute("Ω")]

    EndRoute = 13,
    [CueOperationPrint("🍦")]
    IceCream = 14,

    [CueOperationPrint("🫂")]
    MassUp = 15
}

[System.AttributeUsage(System.AttributeTargets.Field)]
public class CueOperationPrintAttribute : System.Attribute
{
    public string Value;

    public CueOperationPrintAttribute(string value)
    {
        Value = value;

    }
}


public static class CueOperationExtensions
{
    public static string GetPrintAttribute(this CueOperation operation)
    {
        Type typeOfEnum = operation.GetType(); //this will be typeof( MyEnum )

        //here is the problem, GetField takes a string
        // the .ToString() on enums is very slow
        FieldInfo fi = typeOfEnum.GetField(operation.ToString()) ?? throw new Exception();

        //get the attribute from the field
        var printAttribute = fi.GetCustomAttributes(typeof(CueOperationPrintAttribute), false).FirstOrDefault()
            as CueOperationPrintAttribute ?? throw new Exception();

        return printAttribute.Value;


    }
}