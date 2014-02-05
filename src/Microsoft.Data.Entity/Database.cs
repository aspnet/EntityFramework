// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.Data.Entity
{
    public class Database
    {
        public virtual void Create()
        {
            // TODO
        }

        public virtual bool Delete()
        {
            // TODO
            return false;
        }

        public virtual Task CreateAsync()
        {
            // TODO
            return Task.FromResult(false);
        }

        public virtual Task<bool> DeleteAsync()
        {
            // TODO
            return Task.FromResult(false);
        }
    }
}
