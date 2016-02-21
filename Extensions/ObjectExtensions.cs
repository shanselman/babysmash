namespace BabySmash.Extensions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    /// <summary>
    /// Contains extension methods for objects
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Uses reflection to find a private field and validate its type.
        /// </summary>
        /// <param name="ownerType">The type that declares the field. FlattenHierarchy doesn't work for private fields, so you have to pass the base type that declares the field.</param>
        /// <param name="valueType">Expected field type.</param>
        /// <param name="fieldName">Name of the field to read.</param>
        /// <returns>Field information.</returns>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="CatalystException">When field is not found or it is of incorrect type.</exception>

        public static FieldInfo GetField(this Type ownerType, Type valueType, string fieldName)
        {
            FieldInfo field = ownerType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                throw new ArgumentException($"Failed to find field '{fieldName}' on type {ownerType.FullName}.");
            }

            if (!valueType.IsAssignableFrom(field.FieldType))
            {
                throw new ArgumentException($"Field '{fieldName}' on type {ownerType.FullName} is {field.FieldType.FullName}, but expected {valueType.FullName}.");
            }

            return field;
        }

        /// <summary>
        /// Uses reflection to read a value of a private field.
        /// </summary>
        /// <typeparam name="TOwner">The type that declares the field. FlattenHierarchy doesn't work for private fields, so you have to pass the base type that declares the field.</typeparam>
        /// <typeparam name="TValue">Expected field type.</typeparam>
        /// <param name="owner">The instance to read the field value from.</param>
        /// <param name="fieldName">Name of the field to read.</param>
        /// <returns>Field value.</returns>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="CatalystException">When field is not found or it is of incorrect type.</exception>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ReadField", Justification = "'Read' is a clear verb and 'field value' is the subject.")]
        public static TValue ReadFieldValue<TOwner, TValue>(this TOwner owner, string fieldName)
        {
            FieldInfo field = GetField(typeof(TOwner), typeof(TValue), fieldName);
            return (TValue)field.GetValue(owner);
        }

        /// <summary>
        /// Uses reflection to set a value of a private field.
        /// </summary>
        /// <typeparam name="TOwner">The type that declares the field. FlattenHierarchy doesn't work for private fields, so you have to pass the base type that declares the field.</typeparam>
        /// <typeparam name="TValue">Expected field type.</typeparam>
        /// <param name="owner">The instance to write the field value to.</param>
        /// <param name="fieldName">Name of the field to write.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="System.ArgumentNullException"/>
        /// <exception cref="CatalystException">When field is not found or it is of incorrect type.</exception>
        public static void WriteFieldValue<TOwner, TValue>(this TOwner owner, string fieldName, TValue value)
        {
            FieldInfo field = GetField(typeof(TOwner), typeof(TValue), fieldName);
            field.SetValue(owner, value);
        }
    }
}