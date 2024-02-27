using Hazelnut.Entities;
using System.Reflection;
using System.Security.Principal;

namespace Hazelnut.Mvc
{
    public class FileAccessor
    {
        protected readonly Type Type;
        protected readonly PropertyInfo PropertyInfo;
        protected string Id;
        protected readonly IPrincipal CurrentUser;

        public IEntity Instance { get; private set; }

        public Blob Blob { get; private set; }

        public string SecurityErrors { get; private set; }

        /// <summary>
        /// Use create method to instantiate the class.
        /// </summary>
        public FileAccessor(Type type, PropertyInfo propertyInfo, string id, IPrincipal user)
        {
            Type = type;
            PropertyInfo = propertyInfo;
            Id = id;
            CurrentUser = user;
        }

        public async Task LoadBlob()
        {
            await FindRequestedObject();
        }

        async Task FindRequestedObject()
        {
            foreach (var key in new[] { ".", "/" })
                if (Id.Contains(key)) Id = Id.Substring(0, Id.IndexOf(key));

            Instance = Type as IEntity;

            if (Instance == null) throw new Exception($"Invalid {Type.FullName} ID specified: '{Id}'");

            Blob = PropertyInfo?.GetValue(Instance) as Blob;
            if (Blob == null)
                throw new Exception("Failed to find a Blob property named '" + PropertyInfo.Name
                + "' on " + Instance.GetType().FullName + ".");
        }

    }
}