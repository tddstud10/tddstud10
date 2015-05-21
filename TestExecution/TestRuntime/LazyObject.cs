using System;

// From: http://stackoverflow.com/questions/3207580/implementation-of-lazyt-for-net-3-5/3207743#3207743
namespace R4nd0mApps.TddStud10.TestRuntime
{
    /// <summary>
    /// Provides support for lazy initialization.
    /// </summary>
    /// <typeparam name="T">Specifies the type of object that is being lazily initialized.</typeparam>
    public sealed class LazyObject<T>
    {
        private readonly object padlock = new object();
        private readonly Func<T> createValue;
        private bool isValueCreated;
        private T value;

        /// <summary>
        /// Gets the lazily initialized value of the current Lazy{T} instance.
        /// </summary>
        public T Value
        {
            get
            {
                if (!isValueCreated)
                {
                    lock (padlock)
                    {
                        if (!isValueCreated)
                        {
                            value = createValue();
                            isValueCreated = true;
                        }
                    }
                }
                return value;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether a value has been created for this Lazy{T} instance.
        /// </summary>
        public bool IsValueCreated
        {
            get
            {
                lock (padlock)
                {
                    return isValueCreated;
                }
            }
        }


        /// <summary>
        /// Initializes a new instance of the Lazy{T} class.
        /// </summary>
        /// <param name="createValue">The delegate that produces the value when it is needed.</param>
        public LazyObject(Func<T> createValue)
        {
            if (createValue == null) throw new ArgumentNullException("createValue");

            this.createValue = createValue;
        }


        /// <summary>
        /// Creates and returns a string representation of the Lazy{T}.Value.
        /// </summary>
        /// <returns>The string representation of the Lazy{T}.Value property.</returns>
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}