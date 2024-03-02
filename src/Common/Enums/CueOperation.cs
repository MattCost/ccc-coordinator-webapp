using System.Reflection;
using System.Text.Json.Serialization;

namespace CCC.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CueOperation
{
    [CueOperationPrintAttribute("a")]
    StartRoute = 0,

    [CueOperationPrintAttribute("L")]
    Left = 1,

    [CueOperationPrintAttribute("R")]
    Right = 2,

    [CueOperationPrintAttribute("X")]
    Cross = 3,

    [CueOperationPrintAttribute("S")]
    Straight = 4,

    [CueOperationPrintAttribute("L@")]
    LeftAt = 5,

    [CueOperationPrintAttribute("R@")]
    RightAt = 6,

    [CueOperationPrintAttribute("sL")]
    SlightLeft = 7,

    [CueOperationPrintAttribute("sR")]
    SlightRight = 8,

    [CueOperationPrintAttribute("(L)")]
    LeftCircle = 9,

    [CueOperationPrintAttribute("(R)")]
    RightCircle = 10,

    [CueOperationPrintAttribute("(S)")]
    StraightCircle = 11,
    [CueOperationPrintAttribute("z")]
    EndRoute = 12
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