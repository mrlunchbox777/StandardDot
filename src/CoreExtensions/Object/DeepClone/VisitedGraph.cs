using System.Collections.Generic;

namespace StandardDot.CoreExtensions.Object.DeepClone
{
    /// <summary>
    /// A class that helps <see cref="ObjectDeepCloneExtensions" /> figure out visted properties and fields
    /// </summary>
    internal class VisitedGraph : Dictionary<object, object>
    {
        /// <summary>
        /// Checks if an object has been registered as visted
        /// </summary>
        /// <param name="key">The property in question</param>
        /// <returns>If this property has been visted.</returns>
        public new bool ContainsKey(object key)
        {
            if (key == null)
                return true;
            return base.ContainsKey(key);
        }

        /// <summary>
        /// Get the object from the dictionary with null protection
        /// </summary>
        public new object this[object key]
        {
            get { if (key == null) return null; return base[key]; }
        }
    }
}
