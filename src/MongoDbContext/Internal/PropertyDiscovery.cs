using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MongoDbFramework
{
    internal class PropertyDiscovery<TFrom> where TFrom : class
    {
        private static readonly ConcurrentDictionary<Type, Action<TFrom>> _objectInitializers = new ConcurrentDictionary<Type, Action<TFrom>>();

        private readonly TFrom _from;

        public PropertyDiscovery(TFrom from)
        {
            _from = from;
        }

        public void Initialize(Type componentType, Type serviceType, string implementedMethodName)
        {
            var setterMethod = typeof(MongoDbContext).GetMethods().Single(m => m.Name == implementedMethodName && m.IsGenericMethodDefinition);
            InitializeProperty(_from, componentType, serviceType, setterMethod);
            var action = _objectInitializers[_from.GetType()];
            action(_from);
        }

        #region .: Private methods :.

        private void InitializeProperty(TFrom obj, Type componentType, Type serviceType, MethodInfo setterMethod)
        {
            var documentTypes = new Dictionary<Type, List<string>>();
            var initDelegates = new List<Action<TFrom>>();
            var parameter = Expression.Parameter(typeof(TFrom), "objFrom");

            Action<TFrom> setAction;
            if (!_objectInitializers.TryGetValue(obj.GetType(), out setAction))
            {
                var propertyInfoList = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(c => c.DeclaringType != typeof(TFrom)).ToList();
                foreach (var propertyInfo in propertyInfoList)
                {
                    var documentType = GetElementType(propertyInfo.PropertyType, componentType, serviceType);
                    if (documentType != null)
                    {
                        List<string> properties;
                        if (!documentTypes.TryGetValue(documentType, out properties))
                        {
                            properties = new List<string>();
                            documentTypes[documentType] = properties;
                        }
                        properties.Add(propertyInfo.Name);

                        if (CollectionPropertyShouldBeInitialized(propertyInfo))
                        {
                            var setter = propertyInfo.GetSetMethod(nonPublic: false);
                            if (setter != null)
                            {
                                var setMethod = setterMethod.MakeGenericMethod(documentType);

                                var newExpression = Expression.Call(parameter, setMethod);
                                var setExpression = Expression.Call(Expression.Convert(parameter, obj.GetType()), setter, newExpression);
                                initDelegates.Add(Expression.Lambda<Action<TFrom>>(setExpression, parameter).Compile());
                            }
                        }
                    }
                }

                Action<TFrom> initializer = dbContext =>
                {
                    foreach (var initer in initDelegates)
                    {
                        initer(dbContext);
                    }
                };

                setAction = initializer;

                _objectInitializers.TryAdd(obj.GetType(), initializer);
            }
        }

        private bool CollectionPropertyShouldBeInitialized(PropertyInfo propertyInfo)
        {
            return !propertyInfo.GetCustomAttributes(typeof(SuppressCollectionInitializationAttribute), inherit: false).Any() &&
                   !propertyInfo.DeclaringType.GetCustomAttributes(typeof(SuppressCollectionInitializationAttribute), inherit: false).Any();
        }

        private Type GetElementType(Type declaredType, Type componentType, Type serviceType)
        {
            if (!declaredType.IsArray)
            {
                var documentType = GetElementTypeFromService(declaredType, serviceType);
                if (documentType != null)
                {
                    var setOfT = componentType.MakeGenericType(documentType);
                    if (declaredType.IsAssignableFrom(setOfT))
                    {
                        return documentType;
                    }
                }
            }

            return null;
        }

        private Type GetElementTypeFromService(Type setType, Type serviceType)
        {
            try
            {
                var setInterface = (setType.IsGenericType && serviceType.IsAssignableFrom(setType.GetGenericTypeDefinition())) ? setType : setType.GetInterface(serviceType.FullName);

                if (setInterface != null && !setInterface.ContainsGenericParameters)
                {
                    return setInterface.GetGenericArguments()[0];
                }
            }
            catch (AmbiguousMatchException)
            {
            }
            return null;
        }

        #endregion
    }
}
