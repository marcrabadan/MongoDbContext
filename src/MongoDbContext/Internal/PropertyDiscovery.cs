using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MongoDbFramework
{
    public sealed class PropertyDiscovery<TFrom> where TFrom : class
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<Type, Action<TFrom>>> _objectInitializers = new ConcurrentDictionary<Type, Dictionary<Type, Action<TFrom>>>();
        private static readonly List<string> _propertyProcessed = new List<string>();

        private readonly TFrom _from;

        public PropertyDiscovery(TFrom from)
        {
            _from = from;
        }

        public void Initialize(Type componentType, Type serviceType, string implementedMethodName)
        {
            var setterMethod = typeof(MongoDbContext).GetMethods().Single(m => m.Name == implementedMethodName && m.IsGenericMethodDefinition);
            InitializeProperty(_from, componentType, serviceType, setterMethod);
            var actions = _objectInitializers[_from.GetType()];
            var action = actions[componentType];
            action(_from);
        }

        #region .: Private methods :.

        private void InitializeProperty(TFrom obj, Type componentType, Type serviceType, MethodInfo setterMethod)
        {
            var documentTypes = new Dictionary<Type, List<string>>();
            var initDelegates = new List<Action<TFrom>>();
            var parameter = Expression.Parameter(typeof(TFrom), "objFrom");
            
            if (!_objectInitializers.ContainsKey(obj.GetType()) || !_objectInitializers[obj.GetType()].ContainsKey(componentType))
            {
                var propertyInfoList = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(c => c.DeclaringType != typeof(TFrom)).ToList();
                foreach (var propertyInfo in propertyInfoList)
                {
                    if(_propertyProcessed.Contains(propertyInfo.Name))
                        continue;
                    
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
                                _propertyProcessed.Add(propertyInfo.Name);
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
                
                if (!_objectInitializers.ContainsKey(obj.GetType()))
                {
                    var dict = new Dictionary<Type, Action<TFrom>>
                    {
                        { componentType, initializer }
                    };
                    _objectInitializers.TryAdd(obj.GetType(), dict);
                }
                else
                {
                    var actions = _objectInitializers[obj.GetType()];
                    if(!actions.ContainsKey(componentType))
                        actions.Add(componentType, initializer);
                    _objectInitializers[obj.GetType()] = actions;
                }
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
