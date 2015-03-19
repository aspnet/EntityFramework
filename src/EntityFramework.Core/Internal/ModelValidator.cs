// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Internal
{
    public abstract class ModelValidator : IModelValidator
    {
        public virtual void Validate(IModel model)
        {
            EnsureNoShadowKeys(model);
            EnsureValidForeignKeyChains(model);
        }

        protected void EnsureNoShadowKeys(IModel model)
        {
            foreach (var entityType in model.EntityTypes)
            {
                foreach (var key in entityType.Keys)
                {
                    if (key.Properties.Any(p => p.IsShadowProperty))
                    {
                        string message;
                        var referencingFk = model.GetReferencingForeignKeys(key).FirstOrDefault();
                        if (referencingFk != null)
                        {
                            message = Strings.ReferencedShadowKey(
                                Property.Format(key.Properties),
                                entityType.FullName,
                                Property.Format(key.Properties.Where(p => p.IsShadowProperty)),
                                Property.Format(referencingFk.Properties),
                                referencingFk.EntityType.FullName);
                        }
                        else
                        {
                            message = Strings.ShadowKey(
                                Property.Format(key.Properties),
                                entityType.FullName,
                                Property.Format(key.Properties.Where(p => p.IsShadowProperty)));
                        }

                        ShowWarning(message);
                    }
                }
            }
        }

        protected void EnsureValidForeignKeyChains(IModel model)
        {
            var verifiedProperties = new Dictionary<IProperty, IProperty>();
            foreach (var entityType in model.EntityTypes)
            {
                foreach (var foreignKey in entityType.ForeignKeys)
                {
                    foreach (var referencedProperty in foreignKey.Properties)
                    {
                        string errorMessage;
                        VerifyRootPrincipal(referencedProperty, verifiedProperties, ImmutableList<IForeignKey>.Empty, out errorMessage);
                        if (errorMessage != null)
                        {
                            ShowError(errorMessage);
                        }
                    }
                }
            }
        }

        private IProperty VerifyRootPrincipal(
            IProperty principalProperty,
            Dictionary<IProperty, IProperty> verifiedProperties,
            ImmutableList<IForeignKey> visitedForeignKeys,
            out string errorMessage)
        {
            errorMessage = null;
            IProperty rootPrincipal;
            if (verifiedProperties.TryGetValue(principalProperty, out rootPrincipal))
            {
                return rootPrincipal;
            }

            var rootPrincipals = new Dictionary<IProperty, IForeignKey>();
            foreach (var foreignKey in principalProperty.EntityType.ForeignKeys)
            {
                for (var index = 0; index < foreignKey.Properties.Count; index++)
                {
                    if (principalProperty == foreignKey.Properties[index])
                    {
                        var nextPrincipalProperty = foreignKey.ReferencedProperties[index];
                        if (visitedForeignKeys.Contains(foreignKey))
                        {
                            var cycleStart = visitedForeignKeys.IndexOf(foreignKey);
                            var cycle = visitedForeignKeys.GetRange(cycleStart, visitedForeignKeys.Count - cycleStart);
                            errorMessage = Strings.CircularDependency(cycle.Select(fk => fk.ToString()).Join());
                            continue;
                        }
                        else
                        {
                            rootPrincipal = VerifyRootPrincipal(nextPrincipalProperty, verifiedProperties, visitedForeignKeys.Add(foreignKey), out errorMessage);
                            if (rootPrincipal == null)
                            {
                                if (principalProperty.GenerateValueOnAdd)
                                {
                                    rootPrincipals[principalProperty] = foreignKey;
                                }
                                continue;
                            }

                            if (principalProperty.GenerateValueOnAdd)
                            {
                                ShowError(Strings.ForeignKeyValueGenerationOnAdd(
                                    principalProperty.Name,
                                    principalProperty.EntityType.Name,
                                    Property.Format(foreignKey.Properties)));
                                return principalProperty;
                            }
                        }

                        rootPrincipals[rootPrincipal] = foreignKey;
                    }
                }
            }

            if (rootPrincipals.Count == 0)
            {
                if (errorMessage != null)
                {
                    return null;
                }

                if (!principalProperty.GenerateValueOnAdd)
                {
                    ShowError(Strings.PrincipalKeyNoValueGenerationOnAdd(principalProperty.Name, principalProperty.EntityType.Name));
                    return null;
                }

                return principalProperty;
            }

            if (rootPrincipals.Count > 1)
            {
                var firstRoot = rootPrincipals.Keys.ElementAt(0);
                var secondRoot = rootPrincipals.Keys.ElementAt(1);
                ShowWarning(Strings.MultipleRootPrincipals(
                    rootPrincipals[firstRoot].EntityType.Name,
                    Property.Format(rootPrincipals[firstRoot].Properties),
                    firstRoot.EntityType.Name,
                    firstRoot.Name,
                    Property.Format(rootPrincipals[secondRoot].Properties),
                    secondRoot.EntityType.Name,
                    secondRoot.Name));

                return firstRoot;
            }

            errorMessage = null;
            rootPrincipal = rootPrincipals.Keys.Single();
            verifiedProperties[principalProperty] = rootPrincipal;
            return rootPrincipal;
        }

        protected virtual void ShowError(string message)
        {
            throw new InvalidOperationException(message);
        }

        protected abstract void ShowWarning(string message);
    }
}
