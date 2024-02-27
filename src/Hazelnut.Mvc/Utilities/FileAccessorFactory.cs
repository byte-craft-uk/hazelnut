using System;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using Hazelnut.Entities;

namespace Hazelnut.Mvc
{
    public class FileAccessorFactory : IFileAccessorFactory
    {

        public FileAccessorFactory()
        {
        }

        public async Task<FileAccessor> Create(string path, IPrincipal currentUser)
        {
            var temp = GetTypeAndProperty(path);

            var result = new FileAccessor(temp.Type, temp.PropertyInfo, temp.Id, currentUser);

            await result.LoadBlob();

            return result;
        }

        protected virtual (Type Type, PropertyInfo PropertyInfo, string Id) GetTypeAndProperty(string path)
        {
            var pathParts = path.Split('/');

            if (pathParts.Length < 2)
                throw new Exception($"Invalid path specified: '{path}'");

            var typeName = pathParts[0].Split('.')[0];


            var type = AppDomain.CurrentDomain.FindImplementers(typeof(IEntity), ignoreDrivedClasses: false)
                        .SingleOrDefault(x => x.Name == typeName);

            if (type == null) throw new Exception($"Invalid type name specified: '{typeName}'");

            var property = pathParts[0].Split('.')[1];

            var propertyInfo = type.GetProperty(property);
            if (propertyInfo == null)
                throw new Exception($"Could not find the property '{property}' on the type '{type.FullName}'.");

            return (type, propertyInfo, pathParts[1]);
        }
    }
}