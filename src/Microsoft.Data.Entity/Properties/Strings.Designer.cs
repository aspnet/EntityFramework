// <auto-generated />
namespace Microsoft.Data.Entity
{
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal static class Strings
    {
        private static readonly ResourceManager _resourceManager
            = new ResourceManager("Microsoft.Data.Entity.Strings", typeof(Strings).GetTypeInfo().Assembly);

        /// <summary>
        /// The argument '{argumentName}' cannot be null, empty or contain only white space.
        /// </summary>
        internal static string ArgumentIsNullOrWhitespace(object argumentName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("ArgumentIsNullOrWhitespace", "argumentName"), argumentName);
        }

        /// <summary>
        /// The value provided for argument '{argumentName}' must be a valid value of enum type '{enumType}'.
        /// </summary>
        internal static string InvalidEnumValue(object argumentName, object enumType)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("InvalidEnumValue", "argumentName", "enumType"), argumentName, enumType);
        }

        /// <summary>
        /// The properties expression '{expression}' is not valid. The expression should represent a property access: 't =&gt; t.MyProperty'. When specifying multiple properties use an anonymous type: 't =&gt; new {{ t.MyProperty1, t.MyProperty2 }}'.
        /// </summary>
        internal static string InvalidPropertiesExpression(object expression)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("InvalidPropertiesExpression", "expression"), expression);
        }

        /// <summary>
        /// The expression '{expression}' is not a valid property expression. The expression should represent a property access: 't =&gt; t.MyProperty'.
        /// </summary>
        internal static string InvalidPropertyExpression(object expression)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("InvalidPropertyExpression", "expression"), expression);
        }

        /// <summary>
        /// The EntityConfiguration property '{propertyName}' has not been set.
        /// </summary>
        internal static string MissingConfigurationItem(object propertyName)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString("MissingConfigurationItem", "propertyName"), propertyName);
        }

        private static string GetString(string name, params string[] argumentNames)
        {
            var value = _resourceManager.GetString(name);

            System.Diagnostics.Debug.Assert(value != null);

            for (var i = 0; i < argumentNames.Length; i++)
            {
                value = value.Replace("{" + argumentNames[i] + "}", "{" + i + "}");
            }

            return value;
        }
    }
}
