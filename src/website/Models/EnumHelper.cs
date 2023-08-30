using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CCC.website.Models;
public static class EnumHelper
{
    // // Get the value of the description attribute if the   
    // // enum has one, otherwise use the value.  
    // public static string GetDescription<TEnum>(this TEnum value)
    // {
    //     value = value ?? throw new ArgumentNullException(typeof(TEnum).ToString());
    //     var vs = value.ToString() ?? throw new ArgumentNullException(nameof(value));
    //     var fi = value.GetType().GetField(vs);

    //     if (fi != null)
    //     {
    //         var attributes = (DisplayAttribute[])fi.GetCustomAttributes(typeof(DisplayAttribute), false);
    //         if (attributes.Length > 0)
    //         {
    //             return attributes[0].GetName();
    //         }
    //     }

    //     return vs;
    // }
    public static string GetDisplayName(this Enum enumValue)
    {
        string displayName;
        // var test = enumValue.GetType().GetMember(enumValue.ToString()).First().GetCustomAttribute<DisplayAttribute>()?.GetName();
        
        displayName = enumValue.GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()?
            .GetName() ?? string.Empty;

        return string.IsNullOrEmpty(displayName) ? enumValue.ToString() : displayName;
    }

    public static string GetDisplayDescription(this Enum enumValue)
    {
        string output;
        output = enumValue.GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()?
            .GetDescription() ?? string.Empty;

        return string.IsNullOrEmpty(output) ? enumValue.ToString() : output;
    }

    /// <summary>
    /// Build a select list for an enum
    /// </summary>

    public static SelectList SelectListForEnum<T>() where T : struct
    {
        Type type = typeof(T);
        IEnumerable<SelectListItem> items;
        items = type.IsEnum ? BuildSelectListItems(type) : new List<SelectListItem>();
        return new SelectList(items, "Value", "Text");
    }

    /// <summary>
    /// Build a select list for an enum with a particular value selected 
    /// </summary>
    public static SelectList SelectListForEnum_Selected<T>(T selected) where T : struct
    {
        Type type = typeof(T);
        IEnumerable<SelectListItem> items;
        items = type.IsEnum ? BuildSelectListItems(type) : new List<SelectListItem>();
        return new SelectList(items, "Value", "Text", selected.ToString());
    }

    public static SelectList SelectListForFlagEnumInstance<T>(T value) where T : Enum
    {
        var items = Enum.GetValues(typeof(T))
                .Cast<Enum>()
                .Where(v => value.HasFlag(v))
                .Select(e => new SelectListItem { Value = e.ToString(), Text = e.GetDisplayName() });

        return new SelectList(items, "Value", "Text");
    }

    private static IEnumerable<SelectListItem> BuildSelectListItems(Type t)
    {
        IEnumerable<SelectListItem> x = Enum.GetValues(t)
                   .Cast<Enum>()
                   .Select(e => new SelectListItem { Value = e.ToString(), Text = e.GetDisplayName() });
        return x;
    }
}